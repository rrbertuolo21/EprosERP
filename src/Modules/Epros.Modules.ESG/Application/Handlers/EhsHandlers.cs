using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class CriarRegistroEhsCommandHandler : ICommandHandler<CriarRegistroEhsCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRegistroEhsCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRegistroEhsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.RegistrosEhs.AnyAsync(r => r.Codigo == request.Codigo, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe registro EHS com codigo {request.Codigo}.");

            var registro = new RegistroEhs(request.Codigo, request.Descricao, request.Tipo, request.ResponsavelId, request.LocalId, tenantId, usuario);
            if (!registro.IsValid)
                return CommandResult.Falha(registro.Notifications.Select(n => n.Message));

            _context.RegistrosEhs.Add(registro);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Registro EHS criado!", new { registro.Id });
        }
    }

    public class RegistrarAtividadeOcupacionalCommandHandler : ICommandHandler<RegistrarAtividadeOcupacionalCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarAtividadeOcupacionalCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarAtividadeOcupacionalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var atividade = new AtividadeOcupacional(request.RegistroEhsId, request.IdFolhaPpp, request.DataInicio, request.DataFim, request.Descricao, tenantId, usuario);
            if (!atividade.IsValid)
                return CommandResult.Falha(atividade.Notifications.Select(n => n.Message));

            _context.AtividadesOcupacionais.Add(atividade);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Atividade ocupacional registrada!", new { atividade.Id });
        }
    }

    public class RegistrarFatorRiscoCommandHandler : ICommandHandler<RegistrarFatorRiscoCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarFatorRiscoCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarFatorRiscoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fator = new FatorRisco(
                request.AtividadeId, request.IdFolhaPpp, request.DataInicio, request.DataFim, request.Tipo,
                request.FatorRiscoDescricao, request.Intensidade, request.TecnicaUtilizada, request.EpcEficaz, request.EpiEficaz,
                request.CaEpi, request.AtendimentoNr061, request.AtendimentoNr062, request.AtendimentoNr063,
                request.AtendimentoNr064, request.AtendimentoNr065, tenantId, usuario);

            if (!fator.IsValid)
                return CommandResult.Falha(fator.Notifications.Select(n => n.Message));

            _context.FatoresRisco.Add(fator);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fator de risco registrado!", new { fator.Id });
        }
    }

    public class RegistrarLicencaAmbientalCommandHandler : ICommandHandler<RegistrarLicencaAmbientalCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarLicencaAmbientalCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarLicencaAmbientalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var registro = await _context.RegistrosEhs.AnyAsync(r => r.Id == request.RegistroEhsId, cancellationToken);
            if (!registro)
                return CommandResult.Falha("Registro EHS nao encontrado.");

            var licenca = new LicencaAmbiental(
                request.RegistroEhsId, request.Tipo, request.Numero, request.Autoridade,
                request.DataEmissao, request.DataValidade, request.ResponsavelId, request.ArquivoId, tenantId, usuario);

            if (!licenca.IsValid)
                return CommandResult.Falha(licenca.Notifications.Select(n => n.Message));

            _context.LicencasAmbientais.Add(licenca);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Licenca registrada!", new { licenca.Id });
        }
    }

    public class RegistrarResiduoMovimentoCommandHandler : ICommandHandler<RegistrarResiduoMovimentoCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarResiduoMovimentoCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarResiduoMovimentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var residuo = new ResiduoMovimento(
                request.RegistroEhsId, request.TipoResiduo, request.Classificacao, request.Origem, request.Quantidade,
                request.Unidade, request.Data, request.LocalId, request.TipoMovimento, request.DestinoId, request.EvidenciaArquivoId,
                tenantId, usuario);

            if (!residuo.IsValid)
                return CommandResult.Falha(residuo.Notifications.Select(n => n.Message));

            _context.ResiduosMovimento.Add(residuo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Movimento de residuo registrado!", new { residuo.Id });
        }
    }

    public class RegistrarIncidenteCommandHandler : ICommandHandler<RegistrarIncidenteCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarIncidenteCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarIncidenteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var incidente = new Incidente(
                request.RegistroEhsId, request.Tipo, request.DataHora, request.LocalId, request.Descricao,
                request.Gravidade, request.Impacto, request.PessoaId, request.NumeroCat, tenantId, usuario);

            if (!incidente.IsValid)
                return CommandResult.Falha(incidente.Notifications.Select(n => n.Message));

            _context.Incidentes.Add(incidente);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Incidente registrado!", new { incidente.Id });
        }
    }

    public class RegistrarAcaoCorretivaCommandHandler : ICommandHandler<RegistrarAcaoCorretivaCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarAcaoCorretivaCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarAcaoCorretivaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var acao = new AcaoCorretiva(
                request.IncidenteId, request.FatorRiscoId, request.Descricao, request.Causa,
                request.ResponsavelId, request.Prazo, request.EvidenciaArquivoId, tenantId, usuario);

            if (!acao.IsValid)
                return CommandResult.Falha(acao.Notifications.Select(n => n.Message));

            _context.AcoesCorretivas.Add(acao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Acao corretiva registrada!", new { acao.Id });
        }
    }
}
