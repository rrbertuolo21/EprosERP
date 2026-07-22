using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;

namespace Epros.Modules.Vendas.Application.Handlers
{
    /// <summary>
    /// Registra uma venda de PDV: valida o caixa de destino (deve existir e estar aberto),
    /// materializa a entidade <see cref="Domain.Entities.Venda"/> com seus itens e, quando a venda
    /// é emitida/contingência, publica o evento de faturamento (<c>VendaFaturadaEventNotification</c>)
    /// para integração com Estoque/Financeiro.
    /// </summary>
    public class RegistrarVendaCommandHandler : ICommandHandler<RegistrarVendaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IMediator _mediator;

        public RegistrarVendaCommandHandler(
            ContextVendas context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IMediator mediator)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _mediator = mediator;
        }

        /// <summary>
        /// Processa o comando de registro de venda. Retorna falha se o caixa não for encontrado,
        /// estiver fechado, ou se a venda/itens forem inválidos; caso contrário grava a venda e,
        /// se emitida, dispara o evento de faturamento.
        /// </summary>
        public async Task<CommandResult> Handle(RegistrarVendaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";
            var dataCriacao = DateTime.UtcNow;

            // 1. Validar se o caixa correspondente está aberto
            Guid.TryParse(request.CaixaId, out Guid caixaGuid);
            var caixa = await _context.Caixas
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == caixaGuid, cancellationToken);

            if (caixa == null)
            {
                return CommandResult.Falha("Caixa não encontrado.");
            }

            if (caixa.Status != ECaixaStatus.Aberto)
            {
                return CommandResult.Falha("Não é possível registrar vendas em um caixa fechado.");
            }

            // 2. Instanciar a entidade Venda
            var vendaId = Guid.NewGuid();
            var syncId = Guid.NewGuid();

            // O contrato do PDV recebe o status como texto ('Emitida'/'Contingencia'/'Salvar'...).
            // A entidade guarda o enum tipado do legado (EVendaStatus). Uma venda emitida/contingência
            // significa que o documento fiscal foi produzido => Transmitido; caso contrário, Salvar.
            var status = MapearStatus(request.Status, out var emitida);

            var venda = new Venda(
                id: vendaId,
                syncId: syncId,
                caixaId: request.CaixaId,
                total: request.Total,
                status: status,
                tenantId: tenantId,
                criadoPor: criadoPor,
                criadoEm: dataCriacao,
                modeloFiscal: request.ModeloFiscal,
                naturezaOperacao: request.NaturezaOperacao,
                informacoesComplementares: request.InformacoesComplementares,
                informacoesAdicionaisFisco: request.InformacoesAdicionaisFisco,
                modalidadeFrete: request.ModalidadeFrete,
                vendaOrigem: request.VendaOrigem,
                incluirFreteNoTotal: request.IncluirFreteNoTotal,
                clienteId: request.ClienteId,
                valorDesconto: request.ValorDesconto,
                valorFrete: request.ValorFrete,
                formaPagamento: request.FormaPagamento
            );

            // 3. Adicionar itens
            foreach (var itemInput in request.Itens)
            {
                var item = new VendaItem(
                    vendaId: vendaId,
                    produtoId: itemInput.ProdutoId,
                    quantidade: itemInput.Quantidade,
                    precoUnitario: itemInput.PrecoUnitario,
                    tenantId: tenantId,
                    criadoPor: criadoPor,
                    codigoProduto: itemInput.CodigoProduto,
                    codigoEan: itemInput.CodigoEan,
                    descricaoProduto: itemInput.DescricaoProduto,
                    ncm: itemInput.Ncm,
                    cestId: itemInput.CestId,
                    cest: itemInput.Cest,
                    codigoAnpId: itemInput.CodigoAnpId,
                    codigoAnp: itemInput.CodigoAnp,
                    cfop: itemInput.Cfop,
                    unidadeComercial: itemInput.UnidadeComercial,
                    valorDesconto: itemInput.ValorDesconto,
                    valorFreteRateado: itemInput.ValorFreteRateado,
                    valorCusto: itemInput.ValorCusto
                );

                venda.AdicionarItem(item);
            }

            if (!venda.IsValid)
            {
                return CommandResult.Falha(venda.Notifications.Select(n => n.Message), "Falha na validação dos dados da venda.");
            }

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Se a venda for faturada com sucesso (ou emitida/contingência), disparar a integração
            if (emitida)
            {
                var eventNotification = new VendaFaturadaEventNotification(
                    VendaId: venda.Id,
                    TenantId: tenantId,
                    Total: venda.Total,
                    CriadoEm: venda.CriadoEm,
                    Itens: venda.Itens.Select(i => new VendaFaturadaItemNotification(i.ProdutoId, i.Quantidade, i.PrecoUnitario)).ToList(),
                    UserId: criadoPor
                );

                await _mediator.Publish(eventNotification, cancellationToken);
            }

            return CommandResult.Ok("Venda registrada com sucesso!", new { VendaId = venda.Id });
        }

        /// <summary>
        /// Traduz o status textual do contrato do PDV para o enum tipado do legado (EVendaStatus).
        /// 'Emitida'/'Contingencia' => documento fiscal produzido (Transmitido) e sinaliza faturamento;
        /// 'Errado'/'Rejeitada' => Errado; demais/vazio => Salvar. Aceita também o nome do próprio enum.
        /// </summary>
        private static EVendaStatus MapearStatus(string? status, out bool emitida)
        {
            emitida = false;
            var s = (status ?? string.Empty).Trim();

            if (Enum.TryParse<EVendaStatus>(s, ignoreCase: true, out var parsed))
            {
                emitida = parsed == EVendaStatus.Transmitido;
                return parsed;
            }

            switch (s.ToLowerInvariant())
            {
                case "emitida":
                case "contingencia":
                case "contingência":
                case "transmitido":
                case "transmitida":
                    emitida = true;
                    return EVendaStatus.Transmitido;
                case "errado":
                case "rejeitada":
                case "rejeitado":
                    return EVendaStatus.Errado;
                default:
                    return EVendaStatus.Salvar;
            }
        }
    }
}
