using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Registra e aplica avaria/perda como saída controlada do estoque (MVM-026).</summary>
    public class CriarAvariaEstoqueCommandHandler : ICommandHandler<CriarAvariaEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAvariaEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAvariaEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var avaria = new AvariaEstoque(request.ProdutoId, request.Codigo, request.Nome, request.CategoriaId, request.PrecoCompra, request.Quantidade, request.DataAvaria, request.Nota, request.Referencia, tenantId, usuario);
            if (!avaria.IsValid)
                return CommandResult.Falha(avaria.Notifications.Select(n => n.Message), "Dados da avaria são inválidos.");

            _context.AvariasEstoque.Add(avaria);

            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Avaria, tenantId, usuario, referenciaExterna: $"Avaria {avaria.Id}");
            _context.FatosGeradoresEstoque.Add(fato);

            // MVM-026: saída controlada do estoque.
            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            var resultado = await motor.AplicarSaidaAsync(request.EmpresaId, request.ProdutoId, request.Quantidade, fato.Id, null, cancellationToken);
            if (!resultado.Sucesso)
                return CommandResult.Falha(resultado.Erro ?? "Falha ao baixar estoque da avaria.");

            avaria.Aplicar(usuario);

            CriarEstoqueMovimentoManualCommandHandler.RegistrarHistorico(_context, "AvariaEstoque", avaria.Id, "avaria_registrada", "Rascunho", "Aplicado", request.Nota, usuario, tenantId);
            CriarEstoqueMovimentoManualCommandHandler.PublicarEvento(_context, tenantId, "est.mvm.avaria_registrada", new { avaria.Id, avaria.ProdutoId, avaria.Quantidade, avaria.PrecoCompra });

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Avaria registrada e estoque baixado com sucesso!", new { avaria.Id });
        }
    }

    /// <summary>
    /// Cria transferência entre locais (MVM-020/021). Valida origem≠destino e saldo disponível e MOVE O
    /// KARDEX pelo motor único: SAÍDA na origem + ENTRADA no destino, dentro de UMA transação (MVM-029).
    /// D1: a chave de saldo é (EmpresaId+ProdutoId); a transferência é intra-empresa, então o saldo AGREGADO
    /// e o custo médio ficam invariantes (entra o mesmo custo médio que saiu — sem drift de valorização), e
    /// as fichas de saída/entrada carregam o LocalOrigemId/LocalDestinoId para rastreio. A granularidade real
    /// de SALDO por local+lote/série é a fatia D2 (fora deste gap). Idempotente por FatoGerador.ReferenciaExterna.
    /// </summary>
    public class CriarTransferenciaEstoqueCommandHandler : ICommandHandler<CriarTransferenciaEstoqueCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTransferenciaEstoqueCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarTransferenciaEstoqueCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("A transferência deve possuir ao menos um item.");

            // MVM-020: origem diferente de destino (validado no domínio).
            var transferencia = new TransferenciaEstoque(request.LocalOrigemId, request.LocalDestinoId, request.DataTransferencia, request.ValorFrete, request.Observacao, tenantId, usuario);
            if (!transferencia.IsValid)
                return CommandResult.Falha(transferencia.Notifications.Select(n => n.Message), "Dados da transferência são inválidos.");

            // MVM-021: valida saldo disponível na origem (nível agregado empresa/produto) e captura o custo
            // médio vigente de cada item ANTES de mover, para reentrar no destino pelo mesmo custo (invariância).
            var custoMedioPorProduto = new System.Collections.Generic.Dictionary<Guid, decimal>();
            foreach (var input in request.Itens)
            {
                var saldo = await _context.EstoqueProdutos
                    .FirstOrDefaultAsync(e => e.EmpresaId == request.EmpresaId && e.ProdutoId == input.ProdutoId, cancellationToken);
                if (saldo == null || saldo.QuantidadeSaldoEstoque < input.Quantidade)
                    return CommandResult.Falha("Saldo insuficiente na origem para a transferência (MVM-021).");
                custoMedioPorProduto[input.ProdutoId] = saldo.ValorCustoMedio;
            }

            _context.TransferenciasEstoque.Add(transferencia);
            foreach (var input in request.Itens)
            {
                _context.TransferenciasEstoqueItens.Add(new TransferenciaEstoqueItem(transferencia.Id, input.ProdutoId, input.Quantidade, input.ValorUnitario, input.Lote, input.DataValidade, tenantId, usuario));
            }

            // MVM-020/021 + MVM-029: move o kardex pelo MOTOR ÚNICO (saída origem + entrada destino) numa única
            // transação. Idempotente: se já existe FatoGerador desta transferência, não move de novo (anti-dupla).
            var referenciaFato = $"Transferencia {transferencia.Id}";
            var jaMovido = await _context.FatosGeradoresEstoque
                .IgnoreQueryFilters()
                .AnyAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == referenciaFato, cancellationToken);
            if (!jaMovido)
            {
                var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
                var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Transferencia, tenantId, usuario, referenciaExterna: referenciaFato);
                _context.FatosGeradoresEstoque.Add(fato);

                foreach (var input in request.Itens)
                {
                    var custoMedio = custoMedioPorProduto[input.ProdutoId];

                    // SAÍDA na origem (tag LocalOrigemId na ficha de saída).
                    var saida = await motor.AplicarSaidaAsync(request.EmpresaId, input.ProdutoId, input.Quantidade, fato.Id, request.LocalOrigemId, cancellationToken);
                    if (!saida.Sucesso)
                        return CommandResult.Falha(saida.Erro ?? "Falha ao baixar o estoque na origem da transferência.");

                    // ENTRADA no destino pelo MESMO custo médio (tag LocalDestinoId + lote/validade na ficha de entrada).
                    var entrada = await motor.AplicarEntradaAsync(request.EmpresaId, input.ProdutoId, ETipoEstoque.Geral, input.Quantidade, custoMedio, fato.Id, request.LocalDestinoId, input.Lote, input.DataValidade, ETipoCusteioEstoque.CustoMedio, cancellationToken);
                    if (!entrada.Sucesso)
                        return CommandResult.Falha(entrada.Erro ?? "Falha ao dar entrada no destino da transferência.");
                }
            }

            transferencia.Confirmar(usuario);
            CriarEstoqueMovimentoManualCommandHandler.RegistrarHistorico(_context, "TransferenciaEstoque", transferencia.Id, "transferencia_aplicada", "Rascunho", "Confirmada", request.Observacao, usuario, tenantId);
            CriarEstoqueMovimentoManualCommandHandler.PublicarEvento(_context, tenantId, "est.mvm.transferencia_aplicada", new { transferencia.Id, request.LocalOrigemId, request.LocalDestinoId });

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Transferência registrada com sucesso!", new { transferencia.Id });
        }
    }

    /// <summary>Cria requisição interna (cabeçalho + itens). Saída efetiva no atendimento (MVM-027).</summary>
    public class CriarRequisicaoInternaCommandHandler : ICommandHandler<CriarRequisicaoInternaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRequisicaoInternaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRequisicaoInternaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // MVM-027: exige colaborador, data, situação e itens.
            if (request.ColaboradorId == Guid.Empty)
                return CommandResult.Falha("O colaborador é obrigatório (MVM-027).");
            if (request.Itens == null || request.Itens.Count == 0)
                return CommandResult.Falha("A requisição deve possuir ao menos um item (MVM-027).");

            var requisicao = new RequisicaoInterna(request.ColaboradorId, request.DataRequisicao, request.Situacao, tenantId, usuario);
            _context.RequisicoesInternas.Add(requisicao);

            foreach (var input in request.Itens)
            {
                if (input.ProdutoId == Guid.Empty || input.Quantidade <= 0m)
                    return CommandResult.Falha("Item de requisição inválido (produto e quantidade obrigatórios).");
                _context.RequisicoesInternasItens.Add(new RequisicaoInternaItem(requisicao.Id, input.ProdutoId, input.Quantidade, tenantId, usuario));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Requisição interna registrada com sucesso!", new { requisicao.Id });
        }
    }

    /// <summary>
    /// Importa/registra saldo inicial (MVM-023/024). Valida cada linha e aplica entrada para as válidas,
    /// gerando resumo de aceitas/rejeitadas (CA-MVM-011) e evento de importação (§13).
    /// </summary>
    public class ImportarSaldoInicialCommandHandler : ICommandHandler<ImportarSaldoInicialCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ImportarSaldoInicialCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ImportarSaldoInicialCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var importacao = new SaldoInicialImportacao(request.ArquivoNome, tenantId, usuario);
            _context.SaldosIniciaisImportacoes.Add(importacao);

            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.SaldoInicial, tenantId, usuario, referenciaExterna: $"SaldoInicial {importacao.Id}");
            _context.FatosGeradoresEstoque.Add(fato);

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            int total = request.Itens?.Count ?? 0, ok = 0, erro = 0;

            foreach (var input in request.Itens ?? new System.Collections.Generic.List<SaldoInicialItemInput>())
            {
                var produtoId = input.ProdutoId;
                // MVM-024: localizar produto por código quando o Id não veio.
                if (produtoId == null || produtoId == Guid.Empty)
                {
                    produtoId = await _context.Produtos
                        .Where(p => p.Sku == input.ProdutoCodigo)
                        .Select(p => (Guid?)p.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                var item = new SaldoInicialItem(importacao.Id, input.ProdutoCodigo, produtoId, input.LocalNome, input.LocalId, input.Quantidade, input.CustoUnitario, input.Lote, input.DataValidade, tenantId, usuario);

                if (produtoId == null || produtoId == Guid.Empty)
                {
                    item.RegistrarErro("Produto não localizado.");
                    erro++;
                }
                else if (input.Quantidade <= 0m || input.CustoUnitario < 0m)
                {
                    item.RegistrarErro("Quantidade/custo inválidos.");
                    erro++;
                }
                else
                {
                    var res = await motor.AplicarEntradaAsync(request.EmpresaId, produtoId.Value, ETipoEstoque.Geral, input.Quantidade, input.CustoUnitario, fato.Id, input.LocalId, input.Lote, input.DataValidade, ETipoCusteioEstoque.CustoMedio, cancellationToken);
                    if (res.Sucesso) ok++;
                    else { item.RegistrarErro(res.Erro ?? "Falha ao aplicar."); erro++; }
                }

                _context.SaldosIniciaisItens.Add(item);
            }

            importacao.ConcluirProcessamento(total, ok, erro, usuario);
            CriarEstoqueMovimentoManualCommandHandler.PublicarEvento(_context, tenantId, "est.mvm.saldo_inicial_importado", new { importacao.Id, total, ok, erro });

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Importação de saldo inicial processada.", new { importacao.Id, LinhasTotal = total, LinhasProcessadas = ok, LinhasErro = erro });
        }
    }
}
