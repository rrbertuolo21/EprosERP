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
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Services;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handlers do submódulo Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR). Definem dados de
    /// comércio exterior na compra, mantêm o parâmetro de rateio landed (desligado por padrão) e nacionalizam
    /// (entrada Estoque D1 + títulos financeiros via Outbox). Tributos = valida-contador (factuais).
    /// </summary>
    public class DefinirComercioExteriorCompraCommandHandler : ICommandHandler<DefinirComercioExteriorCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ICurrentUser _currentUser;

        public DefinirComercioExteriorCompraCommandHandler(ContextEstoque context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirComercioExteriorCompraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var compra = await _context.Compras.FirstOrDefaultAsync(c => c.Id == request.CompraId, cancellationToken);
            if (compra == null)
                return CommandResult.Falha("Compra não encontrada.");

            if (!compra.DefinirComercioExterior(request.Incoterm, request.Moeda, request.TaxaCambio, usuario))
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Dados de comércio exterior inválidos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Dados de comércio exterior atualizados.", new { compra.Id, compra.Incoterm, compra.Moeda, compra.TaxaCambio });
        }
    }

    public class SalvarRateioLandedConfigCommandHandler : ICommandHandler<SalvarRateioLandedConfigCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SalvarRateioLandedConfigCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SalvarRateioLandedConfigCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var config = await _context.ComprasImportacaoRateioConfigs
                .FirstOrDefaultAsync(c => c.EmpresaId == request.EmpresaId, cancellationToken);
            if (config == null)
            {
                config = new ComprasImportacaoRateioConfig(request.EmpresaId, request.Habilitado, request.IncluirTributos,
                    request.IncluirFrete, request.IncluirDespesas, request.Metodo, tenantId, usuario);
                _context.ComprasImportacaoRateioConfigs.Add(config);
            }
            else
            {
                config.Alterar(request.Habilitado, request.IncluirTributos, request.IncluirFrete, request.IncluirDespesas, request.Metodo, usuario);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parâmetro de rateio landed salvo.", new { config.Id, config.Habilitado, config.Metodo });
        }
    }

    public class NacionalizarImportacaoCommandHandler : ICommandHandler<NacionalizarImportacaoCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public NacionalizarImportacaoCommandHandler(ContextEstoque context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(NacionalizarImportacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var compra = await _context.Compras.Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == request.CompraId, cancellationToken);
            if (compra == null)
                return CommandResult.Falha("Compra não encontrada.");
            if (!compra.Itens.Any())
                return CommandResult.Falha("Compra sem itens para nacionalizar.");

            // CEX-006: idempotência por compra — não republica se já nacionalizada.
            var jaNacionalizada = await _context.OutboxMessages.AnyAsync(
                o => o.EventType == CatalogoEventosIntegracao.Compras.ImportacaoNacionalizada
                  && o.Payload.Contains(compra.Id.ToString()), cancellationToken);
            if (jaNacionalizada)
                return CommandResult.Falha("Importação desta compra já foi nacionalizada (idempotência CEX-006).");

            var config = await _context.ComprasImportacaoRateioConfigs
                .FirstOrDefaultAsync(c => c.EmpresaId == request.EmpresaId, cancellationToken);

            // CEX-003/NF-02: landed DESLIGADO por padrão. Só compõe quando há config habilitada.
            decimal montanteLanded = 0m;
            bool landedAplicado = false;
            if (config is { Habilitado: true })
            {
                montanteLanded += config.IncluirTributos ? request.MontanteTributos : 0m;
                montanteLanded += config.IncluirFrete ? request.MontanteFrete : 0m;    // NF-04: frete compõe custo
                montanteLanded += config.IncluirDespesas ? request.MontanteDespesas : 0m;
                landedAplicado = montanteLanded > 0m;
            }

            var baseItens = compra.Itens
                .Select(i => new RateioLandedItem(i.Id, i.ValorTotal, i.Quantidade, i.Quantidade))
                .ToList();

            var metodo = config?.Metodo ?? Domain.Enums.ERateioLandedMetodo.PorValor;
            var rateio = landedAplicado
                ? RateioLandedCalculadora.Ratear(baseItens, montanteLanded, metodo)
                : baseItens.Select(b => new RateioLandedResultado(b.ItemId, b.Valor, 0m, b.Valor)).ToList();

            var itensPayload = rateio.Select(r => new { itemId = r.ItemId, r.ValorBase, r.ParcelaLanded, custo = r.CustoFinal }).ToList();

            // CEX-006: entrada nacionalizada consumida pelo motor único D1, com custo (landed quando ligado).
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.ImportacaoNacionalizada,
                JsonSerializer.Serialize(new
                {
                    compraId = compra.Id,
                    compra.Incoterm,
                    compra.Moeda,
                    compra.TaxaCambio,
                    landedAplicado,
                    montanteLanded,
                    metodo = metodo.ToString(),
                    idempotencia = $"import-nac:{compra.Id}",
                    itens = itensPayload,
                    usuario
                })));

            // Títulos financeiros de tributos/frete (fato gerador único, idempotente por compra).
            var totalTitulos = request.MontanteTributos + request.MontanteFrete + request.MontanteDespesas;
            if (totalTitulos > 0m)
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Compras.ImportacaoTitulosFinanceiros,
                    JsonSerializer.Serialize(new
                    {
                        compraId = compra.Id,
                        request.MontanteTributos,
                        request.MontanteFrete,
                        request.MontanteDespesas,
                        total = totalTitulos,
                        idempotencia = $"import-titulos:{compra.Id}"
                    })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Importação nacionalizada — entrada e títulos publicados.",
                new { compra.Id, landedAplicado, montanteLanded, itens = itensPayload.Count });
        }
    }
}
