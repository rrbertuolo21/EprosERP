using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers do submódulo Subcontratação (EST-SUB). Gravam em transação única e respeitam tenant (filtro
    /// global do ContextBase). SUB-005: envio/retorno recalculam saldo em poder de terceiros. CA-004: impede
    /// retorno maior que o saldo enviado. Documento fiscal/CFOP (SUB-006/007/008) e integração com estoque/
    /// contas a pagar (SUB-009/010) permanecem como referências externas/pendências — nada de fiscal de memória.
    /// </summary>
    public class CriarSubOrdemCommandHandler : ICommandHandler<CriarSubOrdemCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarSubOrdemCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarSubOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = new SubOrdem(request.EmpresaId, request.NumeroOrdem, request.OrdemProducaoId, request.FornecedorId, request.DataEmissao, request.DataPrevistaRetorno, request.Observacao, tenantId, usuario);
            if (!ordem.IsValid)
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message), "Dados da ordem de subcontratação são inválidos.");

            _context.SubOrdens.Add(ordem);

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubOrdemItem(ordem.Id, input.ProdutoId, input.QuantidadePlanejada, input.Unidade, input.OperacaoTerceirizada, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item da ordem de subcontratação inválido.");
                    _context.SubOrdemItens.Add(item);
                }
            }

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "ordem_criada", null, null, null, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ordem de subcontratação criada com sucesso!", new { ordem.Id });
        }
    }

    public class RegistrarSubEnvioCommandHandler : ICommandHandler<RegistrarSubEnvioCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubEnvioCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubEnvioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var envio = new SubEnvio(request.OrdemId, request.DataEnvio, request.DocumentoFiscalId, tenantId, usuario);
            if (!envio.IsValid)
                return CommandResult.Falha(envio.Notifications.Select(n => n.Message), "Dados da remessa são inválidos.");

            _context.SubEnvios.Add(envio);

            // KARDEX (Gap SUB): a remessa é uma SAÍDA física do estoque próprio (a mercadoria vai para o
            // terceiro). Passa pelo MOTOR ÚNICO (D1, bucket EmpresaPadrao — mesmo dos demais fluxos), numa
            // única transação, idempotente por FatoGerador.ReferenciaExterna. CFOP/fiscal = valida-contador.
            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            var refFato = $"SubEnvio {envio.Id}";
            var jaMovido = await _context.FatosGeradoresEstoque.IgnoreQueryFilters()
                .AnyAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == refFato, cancellationToken);
            FatoGeradorEstoque? fato = null;
            if (!jaMovido)
            {
                fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Transferencia, tenantId, usuario, referenciaExterna: refFato);
                _context.FatosGeradoresEstoque.Add(fato);
            }

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubEnvioItem(envio.Id, input.ProdutoId, input.QuantidadeEnviada, input.LoteId, input.LocalOrigemId, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de remessa inválido.");
                    _context.SubEnvioItens.Add(item);

                    // SUB-005/010: atualiza saldo em poder de terceiros (controle de posse no terceiro).
                    await AtualizarSaldoEnvioAsync(ordem.FornecedorId, input.ProdutoId, ordem.Id, input.QuantidadeEnviada, tenantId, usuario, cancellationToken);

                    // SAÍDA do kardex próprio pelo motor (respeita saldo/estoque negativo — D8).
                    if (fato != null)
                    {
                        var saida = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, input.ProdutoId, input.QuantidadeEnviada, fato.Id, input.LocalOrigemId, cancellationToken);
                        if (!saida.Sucesso)
                            return CommandResult.Falha(saida.Erro ?? "Falha ao baixar o estoque na remessa de subcontratação.");
                    }
                }
            }

            envio.Confirmar(usuario);
            ordem.MarcarEmProcesso(usuario);

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "envio_registrado", null, null, null, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.sub.envio_registrado",
                JsonSerializer.Serialize(new { ordemId = ordem.Id, envioId = envio.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Remessa de subcontratação registrada com sucesso!", new { envio.Id });
        }

        private async Task AtualizarSaldoEnvioAsync(Guid fornecedorId, Guid produtoId, Guid ordemId, decimal quantidade, string tenantId, string usuario, CancellationToken ct)
        {
            var saldo = await _context.SubSaldosTerceiro.FirstOrDefaultAsync(s => s.FornecedorId == fornecedorId && s.ProdutoId == produtoId && s.OrdemId == ordemId && s.DeletadoEm == null, ct);
            if (saldo == null)
            {
                saldo = new SubSaldoTerceiro(fornecedorId, produtoId, ordemId, tenantId, usuario);
                _context.SubSaldosTerceiro.Add(saldo);
            }
            saldo.RegistrarEnvio(quantidade, usuario);
        }
    }

    public class RegistrarSubRetornoCommandHandler : ICommandHandler<RegistrarSubRetornoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubRetornoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubRetornoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var retorno = new SubRetorno(request.OrdemId, request.DataRetorno, request.DocumentoFiscalId, tenantId, usuario);
            if (!retorno.IsValid)
                return CommandResult.Falha(retorno.Notifications.Select(n => n.Message), "Dados do retorno são inválidos.");

            _context.SubRetornos.Add(retorno);

            // KARDEX (Gap SUB): o retorno é uma ENTRADA física no estoque próprio (a mercadoria volta do
            // terceiro). Passa pelo MOTOR ÚNICO (D1, bucket EmpresaPadrao), numa única transação, idempotente
            // por FatoGerador.ReferenciaExterna. Reentra pelo custo médio vigente do produto (a valorização do
            // beneficiamento/serviço é apropriada à parte — SUB-009). CFOP/fiscal = valida-contador.
            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            var refFato = $"SubRetorno {retorno.Id}";
            var jaMovido = await _context.FatosGeradoresEstoque.IgnoreQueryFilters()
                .AnyAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == refFato, cancellationToken);
            FatoGeradorEstoque? fato = null;
            if (!jaMovido)
            {
                fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Transferencia, tenantId, usuario, referenciaExterna: refFato);
                _context.FatosGeradoresEstoque.Add(fato);
            }

            if (request.Itens != null)
            {
                foreach (var input in request.Itens)
                {
                    var item = new SubRetornoItem(retorno.Id, input.ProdutoId, input.QuantidadeRetorno, input.QuantidadeAprovada, input.QuantidadePerda, input.QuantidadeSucata, input.Rendimento, tenantId, usuario);
                    if (!item.IsValid)
                        return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Item de retorno inválido.");

                    // CA-004: impedir retorno maior que saldo em poder do terceiro.
                    var saldo = await _context.SubSaldosTerceiro.FirstOrDefaultAsync(s => s.FornecedorId == ordem.FornecedorId && s.ProdutoId == input.ProdutoId && s.OrdemId == ordem.Id && s.DeletadoEm == null, cancellationToken);
                    var perda = input.QuantidadePerda ?? 0m;
                    if (saldo == null || (input.QuantidadeRetorno + perda) > saldo.QuantidadeEmPoderTerceiro)
                        return CommandResult.Falha("Retorno maior que o saldo em poder do terceiro [CA-004].");

                    _context.SubRetornoItens.Add(item);
                    saldo.RegistrarRetorno(input.QuantidadeRetorno, perda, usuario);

                    // ENTRADA no kardex próprio, pela quantidade que RETORNA (perda/sucata não reentram),
                    // valorizada pelo custo médio vigente do produto.
                    if (fato != null && input.QuantidadeRetorno > 0m)
                    {
                        var custoMedio = await _context.EstoqueProdutos
                            .Where(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == input.ProdutoId)
                            .Select(e => (decimal?)e.ValorCustoMedio)
                            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

                        var entrada = await motor.AplicarEntradaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, input.ProdutoId, ETipoEstoque.Geral, input.QuantidadeRetorno, custoMedio, fato.Id, null, null, null, ETipoCusteioEstoque.CustoMedio, cancellationToken);
                        if (!entrada.Sucesso)
                            return CommandResult.Falha(entrada.Erro ?? "Falha ao dar entrada do retorno de subcontratação no estoque.");
                    }
                }
            }

            retorno.Confirmar(usuario);
            ordem.MarcarRetornada(usuario);

            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "retorno_registrado", null, null, null, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "estoque.sub.retorno_registrado",
                JsonSerializer.Serialize(new { ordemId = ordem.Id, retornoId = retorno.Id, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Retorno de subcontratação registrado com sucesso!", new { retorno.Id });
        }
    }

    /// <summary>
    /// SUB-009 — registra a cobrança do serviço de beneficiamento e publica evento para gerar a compra do
    /// serviço + contas a pagar (fato gerador único). ValorServico deve ser positivo.
    /// </summary>
    public class RegistrarSubServicoCobrancaCommandHandler : ICommandHandler<RegistrarSubServicoCobrancaCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubServicoCobrancaCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubServicoCobrancaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.ValorServico <= 0m)
                return CommandResult.Falha("O valor do serviço deve ser maior que zero [SUB-030].");

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var cobranca = new SubServicoCobranca(request.OrdemId, request.CompraId, request.ValorServico, tenantId, usuario);
            _context.SubServicosCobranca.Add(cobranca);
            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "servico_cobrado", null, null, null, tenantId, usuario));

            // SUB-009: gera a compra do serviço + contas a pagar via evento (fato gerador único, idempotente).
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.SubServicoCobrado,
                JsonSerializer.Serialize(new { ordemId = ordem.Id, cobrancaId = cobranca.Id, request.ValorServico, request.CompraId, idempotencia = $"sub-servico:{cobranca.Id}", usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cobrança de serviço registrada — compra do serviço solicitada.", new { cobranca.Id });
        }
    }

    /// <summary>
    /// SUB-008 — registra o documento fiscal de remessa/retorno da subcontratação com CFOP PARAMETRIZADO
    /// (valida-contador). O código não calcula o CFOP: apenas persiste o informado e publica o evento.
    /// </summary>
    public class RegistrarSubDocumentoFiscalCommandHandler : ICommandHandler<RegistrarSubDocumentoFiscalCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarSubDocumentoFiscalCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarSubDocumentoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.SubOrdens.FirstOrDefaultAsync(o => o.Id == request.OrdemId && o.DeletadoEm == null, cancellationToken);
            if (ordem == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            var doc = new SubDocumentoFiscal(request.OrdemId, request.EnvioId, request.RetornoId, request.DocumentoFiscalId,
                request.CfopRemessa, request.CfopRetorno, tenantId, usuario);
            _context.SubDocumentosFiscais.Add(doc);
            _context.SubHistoricos.Add(new SubHistorico(ordem.Id, "documento_fiscal_registrado", null, null, null, tenantId, usuario));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.SubDocumentoFiscalRegistrado,
                JsonSerializer.Serialize(new { ordemId = ordem.Id, documentoId = doc.Id, request.CfopRemessa, request.CfopRetorno, usuario })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento fiscal de subcontratação registrado (CFOP parametrizado — valida-contador).", new { doc.Id });
        }
    }
}
