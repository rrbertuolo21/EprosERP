using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Security;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Persiste o agregado Compra completo: cabeçalho + itens (com entrada de estoque e movimentação),
    /// emitente, destinatário, imposto (nível compra), total, transporte e pagamentos. Tudo em uma única
    /// transação (SaveChanges atômico) com evento no Outbox.
    /// </summary>
    public class LancarCompraFiscalCommandHandler : ICommandHandler<LancarCompraFiscalCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public LancarCompraFiscalCommandHandler(
            ContextEstoque context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(LancarCompraFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // CD3/SRC-008: se a origem está sob alçada, só efetiva com o pedido de aprovação APROVADO.
            var erroAlcada = await AlcadaCompraGate.GarantirAprovadaAsync(_context, request.AprovacaoOrigemId, cancellationToken);
            if (erroAlcada != null) return erroAlcada;

            // 1. Evitar duplicidade por chave de acesso.
            var notaExistente = await _context.Compras.AnyAsync(c => c.ChaveAcesso == request.ChaveAcesso, cancellationToken);
            if (notaExistente)
                return CommandResult.Falha("Esta nota fiscal já foi lançada para este inquilino.");

            // 2. Agregado raiz.
            var compra = new Compra(
                request.FornecedorCnpj,
                request.FornecedorNome,
                request.NumeroNota,
                request.ChaveAcesso,
                request.ValorTotal,
                request.DataEmissao,
                tenantId,
                usuario);

            if (!compra.IsValid)
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Dados de cabeçalho da nota fiscal são inválidos.");

            // 3. Itens (+ produto/estoque/movimentação). Entrada de estoque via MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            var fato = new FatoGeradorEstoque(null, compra.Id, null, EOrigemFatoGeradorEstoque.EntradaFiscal, tenantId, usuario, referenciaExterna: $"Compra fiscal NF {request.NumeroNota}");
            _context.FatosGeradoresEstoque.Add(fato);

            foreach (var itemInput in request.Itens)
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Sku == itemInput.Sku, cancellationToken);
                if (produto == null)
                {
                    produto = new Produto(itemInput.Sku, itemInput.NomeProduto, 0m, tenantId, usuario);
                    if (!produto.IsValid)
                        return CommandResult.Falha(produto.Notifications.Select(n => n.Message), $"Erro ao criar produto com SKU {itemInput.Sku}.");
                    _context.Produtos.Add(produto);
                }

                var custeioPadrao = await _context.EstoqueProdutos
                    .Where(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == produto.Id)
                    .Select(e => (ETipoCusteioEstoque?)e.TipoCusteioEstoque)
                    .FirstOrDefaultAsync(cancellationToken) ?? ETipoCusteioEstoque.CustoMedio;

                var resEntrada = await motor.AplicarEntradaAsync(
                    MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, ETipoEstoque.Geral, itemInput.Quantidade, itemInput.PrecoUnitario,
                    fato.Id, null, null, null, custeioPadrao, cancellationToken);
                if (!resEntrada.Sucesso)
                    return CommandResult.Falha(resEntrada.Erro ?? $"Erro ao lançar entrada de estoque para o SKU {itemInput.Sku}.");

                var movimento = new MovimentoEstoque(
                    produto.Id,
                    itemInput.Quantidade,
                    "Entrada",
                    $"Lançamento fiscal de compra - NF-e nº {request.NumeroNota}",
                    tenantId,
                    usuario);
                if (!movimento.IsValid)
                    return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), $"Erro ao criar movimentação de estoque para o SKU {itemInput.Sku}.");
                _context.MovimentosEstoque.Add(movimento);

                compra.AdicionarItem(
                    produto.Id,
                    itemInput.Quantidade,
                    itemInput.PrecoUnitario,
                    itemInput.ValorIms,
                    itemInput.ValorIpi,
                    usuario,
                    codigoProduto: itemInput.Sku,
                    descricaoProduto: itemInput.NomeProduto,
                    ncm: itemInput.Ncm,
                    cfop: itemInput.Cfop,
                    unidadeComercial: itemInput.UnidadeComercial);
            }

            // 4. Emitente.
            if (request.Emitente is { } emit)
            {
                var emitente = new CompraEmitente(
                    compra.Id, null, null, emit.Cnpj, emit.Cpf, emit.RazaoSocial, emit.NomeFantasia,
                    emit.Telefone, emit.InscricaoEstadual, null, null, emit.Cnae, emit.RegimeTributario, null, tenantId, usuario);
                compra.DefinirEmitente(emitente);
            }

            // 5. Destinatário.
            if (request.Destinatario is { } dest)
            {
                var destinatario = new CompraDestinatario(
                    compra.Id, dest.PessoaId, dest.Cnpj, dest.Cpf, dest.RazaoSocial, dest.Telefone,
                    dest.InscricaoEstadual, null, dest.IndicadorIE, dest.Email, dest.EhConsumidorFinal, tenantId, usuario);
                compra.DefinirDestinatario(destinatario);
            }

            // 6. Imposto (nível compra).
            if (request.Imposto is { } imp)
            {
                var imposto = new CompraImposto(compra.Id, imp.ValorAliquotaCreditoIcms, tenantId, usuario);
                compra.DefinirImposto(imposto);
            }

            // 7. Total.
            if (request.Total is { } tot)
            {
                var total = new CompraTotal(
                    compra.Id, 0m, tot.ValorIcms, 0m, 0m, 0m, 0m, 0m, 0m, tot.ValorProduto, tot.ValorFrete,
                    tot.ValorSeguro, tot.ValorDesconto, 0m, tot.ValorIpi, 0m, 0m, 0m, 0m, tot.ValorNotaFiscal, tenantId, usuario);
                compra.DefinirTotal(total);
            }

            // 8. Transporte (+ transportadora opcional).
            if (request.Transporte is { } tr)
            {
                var transporte = new CompraTransporte(compra.Id, tenantId, usuario);
                if (!string.IsNullOrWhiteSpace(tr.TransportadoraRazaoSocialOuNome))
                {
                    transporte.IncluirTransportadora(
                        tr.TransportadoraPessoaId, tr.TransportadoraCnpj, tr.TransportadoraCpf,
                        tr.TransportadoraRazaoSocialOuNome, tr.TransportadoraInscricaoEstadual, null, null, tr.TransportadoraUf, usuario);
                }
                compra.DefinirTransporte(transporte);
            }

            // 9. Pagamentos.
            if (request.Pagamentos is { Count: > 0 } pagamentos)
            {
                foreach (var pag in pagamentos)
                {
                    var pagamento = new CompraPagamento(
                        compra.Id, pag.ValorTroco, pag.TipoPagamento, pag.ValorPagamento,
                        ETipoIntegracaoPagamentoCartao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, tenantId, usuario);
                    compra.AdicionarPagamento(pagamento);
                }
            }

            // 10. Validação do agregado completo.
            if (!compra.AgregadoValido())
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Invariantes de domínio do agregado de compra não foram atendidas.");

            _context.Compras.Add(compra);

            // 11. Outbox (transacional com o SaveChanges).
            var evento = new
            {
                CompraId = compra.Id,
                TenantId = tenantId,
                compra.FornecedorCnpj,
                compra.FornecedorNome,
                compra.NumeroNota,
                compra.ValorTotal,
                TemEmitente = compra.Emitente != null,
                TemDestinatario = compra.Destinatario != null,
                QtdPagamentos = compra.Pagamentos.Count,
                QtdItens = compra.Itens.Count
            };
            var payloadJson = JsonSerializer.Serialize(evento);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "CompraFiscalLancada", payloadJson));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Compra fiscal lançada com sucesso!", new { CompraId = compra.Id });
        }
    }
}
