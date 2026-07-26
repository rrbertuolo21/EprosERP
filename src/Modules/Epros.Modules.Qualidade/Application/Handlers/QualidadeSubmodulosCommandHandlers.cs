using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    // ============ QLD-INS ============
    public class CriarPlanoInspecaoCommandHandler : ICommandHandler<CriarPlanoInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoInspecaoCommandHandler(ContextQualidade context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoInspecaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (await _context.PlanosInspecao.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken))
                return CommandResult.Falha($"Ja existe um plano com o codigo '{request.Codigo}' neste tenant.", block: true);

            var plano = new PlanoInspecao(request.Codigo, request.Descricao, request.Contexto, request.ResponsavelId,
                request.ProdutoId, request.ProcessoId, request.EtapaId, request.DataInicioVigencia, tenantId, usuario);

            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            _context.PlanosInspecao.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Plano de inspecao criado com sucesso!", new { plano.Id, plano.Codigo, Status = plano.Status.ToString() });
        }
    }

    // ============ QLD-ACR ============
    public class CriarAnaliseAcrCommandHandler : ICommandHandler<CriarAnaliseAcrCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAnaliseAcrCommandHandler(ContextQualidade context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAnaliseAcrCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (await _context.AcrAnalises.AnyAsync(a => a.Codigo == request.Codigo, cancellationToken))
                return CommandResult.Falha($"Ja existe uma analise com o codigo '{request.Codigo}' neste tenant.", block: true);

            var analise = new AcrAnalise(request.Codigo, request.Descricao, request.TipoAnalise, request.ResponsavelId,
                request.LocalId, request.DocumentoFiscalId, tenantId, usuario);

            if (!analise.IsValid)
                return CommandResult.Falha(analise.Notifications.Select(n => n.Message));

            _context.AcrAnalises.Add(analise);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Analise de aceite/rejeicao criada com sucesso!", new { analise.Id, analise.Codigo, Status = analise.Status.ToString() });
        }
    }

    // ============ QLD-ADM ============
    public class CriarRegistroAdmCommandHandler : ICommandHandler<CriarRegistroAdmCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRegistroAdmCommandHandler(ContextQualidade context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRegistroAdmCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (await _context.AdmQualidades.AnyAsync(a => a.Codigo == request.Codigo, cancellationToken))
                return CommandResult.Falha($"Ja existe um registro com o codigo '{request.Codigo}' neste tenant.", block: true);

            var registro = new AdmQualidade(request.Codigo, request.Descricao, request.ResponsavelId, tenantId, usuario);

            if (!registro.IsValid)
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));

            _context.AdmQualidades.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Registro de administracao da qualidade criado com sucesso!", new { registro.Id, registro.Codigo, Status = registro.Status.ToString() });
        }
    }

    // ============ QLD-ATR ============
    public class CriarAtributoCommandHandler : ICommandHandler<CriarAtributoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAtributoCommandHandler(ContextQualidade context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAtributoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-002/RN-005: codigo unico e nome interno unico por escopo.
            if (await _context.AtrAtributos.AnyAsync(a => a.Codigo == request.Codigo, cancellationToken))
                return CommandResult.Falha($"Ja existe um atributo com o codigo '{request.Codigo}' neste tenant.", block: true);
            if (await _context.AtrAtributos.AnyAsync(a => a.NomeInterno == request.NomeInterno && a.Escopo == request.Escopo, cancellationToken))
                return CommandResult.Falha($"Ja existe um atributo com o nome interno '{request.NomeInterno}' neste escopo.", block: true);

            var atributo = new AtrAtributo(request.Codigo, request.NomeInterno, request.Rotulo, request.TipoAtributo,
                request.TipoDado, request.Escopo, request.ExibirFormularioPadrao, request.Obrigatorio,
                request.TipoCaracteristica, request.SensivelLgpd, request.Posicao, request.ResponsavelId, tenantId, usuario);

            if (!atributo.IsValid)
                return CommandResult.Falha(atributo.Notifications.Select(n => n.Message));

            _context.AtrAtributos.Add(atributo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Atributo criado com sucesso!", new { atributo.Id, atributo.Codigo, Status = atributo.Status.ToString() });
        }
    }
}
