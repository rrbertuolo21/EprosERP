using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    /// <summary>D-TEC-05 — cria nó no catálogo único.</summary>
    public class CriarNoTaxonomiaCommandHandler : ICommandHandler<CriarNoTaxonomiaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarNoTaxonomiaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarNoTaxonomiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.TaxonomiasNormativas.AnyAsync(t => t.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha("Ja existe no de taxonomia com este codigo para o tenant.");

            if (request.CatalogoPaiId != null)
            {
                var paiExiste = await _context.TaxonomiasNormativas.AnyAsync(t => t.Id == request.CatalogoPaiId, cancellationToken);
                if (!paiExiste)
                    return CommandResult.Falha("O no pai informado nao existe.");
            }

            var no = new TaxonomiaNormativa(request.Codigo, request.Tipo, request.Nome, request.CatalogoPaiId, tenantId, usuario);
            if (!no.IsValid)
                return CommandResult.Falha(no.Notifications.Select(n => n.Message));

            _context.TaxonomiasNormativas.Add(no);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("No de taxonomia criado.", new { no.Id, no.Tipo });
        }
    }

    /// <summary>D-TEC-05 — cria aresta de rastreabilidade.</summary>
    public class VincularTaxonomiaCommandHandler : ICommandHandler<VincularTaxonomiaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularTaxonomiaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularTaxonomiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.TaxonomiaVinculos.AnyAsync(v =>
                v.OrigemTipo == request.OrigemTipo && v.OrigemId == request.OrigemId &&
                v.DestinoTipo == request.DestinoTipo && v.DestinoId == request.DestinoId &&
                v.Natureza == request.Natureza, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha("Este vinculo de rastreabilidade ja existe.");

            var vinculo = new TaxonomiaVinculo(
                request.OrigemTipo, request.OrigemId, request.DestinoTipo, request.DestinoId, request.Natureza, tenantId, usuario);
            if (!vinculo.IsValid)
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.TaxonomiaVinculos.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Vinculo de rastreabilidade criado.", new { vinculo.Id, vinculo.Natureza });
        }
    }

    /// <summary>D-TEC-05 — classifica um agregado num nó do catálogo (FK opcional).</summary>
    public class ClassificarAgregadoTaxonomiaCommandHandler : ICommandHandler<ClassificarAgregadoTaxonomiaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public ClassificarAgregadoTaxonomiaCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ClassificarAgregadoTaxonomiaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var noExiste = await _context.TaxonomiasNormativas.AnyAsync(t => t.Id == request.TaxonomiaNormativaId, cancellationToken);
            if (!noExiste)
                return CommandResult.Falha("O no de taxonomia informado nao existe.");

            switch (request.AgregadoTipo)
            {
                case "Politica":
                    var pol = await _context.Politicas.FirstOrDefaultAsync(p => p.Id == request.AgregadoId, cancellationToken);
                    if (pol == null) return CommandResult.Falha("Politica nao encontrada.");
                    pol.Classificar(request.TaxonomiaNormativaId, usuario);
                    break;
                case "Obrigacao":
                    var reg = await _context.RegistrosRegulatorios.FirstOrDefaultAsync(r => r.Id == request.AgregadoId, cancellationToken);
                    if (reg == null) return CommandResult.Falha("Obrigacao (registro) nao encontrada.");
                    reg.Classificar(request.TaxonomiaNormativaId, usuario);
                    break;
                case "Controle":
                    var ctrl = await _context.ControlesInternos.FirstOrDefaultAsync(c => c.Id == request.AgregadoId, cancellationToken);
                    if (ctrl == null) return CommandResult.Falha("Controle nao encontrado.");
                    ctrl.Classificar(request.TaxonomiaNormativaId, usuario);
                    break;
                case "Risco":
                    var risco = await _context.RiscosCorporativos.FirstOrDefaultAsync(r => r.Id == request.AgregadoId, cancellationToken);
                    if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
                    risco.Classificar(request.TaxonomiaNormativaId, usuario);
                    break;
                default:
                    return CommandResult.Falha("Tipo de agregado invalido.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Agregado classificado na taxonomia normativa unica.",
                new { request.AgregadoTipo, request.AgregadoId, request.TaxonomiaNormativaId });
        }
    }
}
