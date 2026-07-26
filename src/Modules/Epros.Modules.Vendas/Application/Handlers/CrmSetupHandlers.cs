using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Vendas.Application.Handlers
{
    // CRM-002: pipelines, etapas, origens e etiquetas registram autor e tenant.

    public class CriarCrmPipelineCommandHandler : ICommandHandler<CriarCrmPipelineCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmPipelineCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmPipelineCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var pipeline = new CrmPipeline(request.Nome, request.CriadorUsuarioId, tenantId, usuario);
            if (!pipeline.IsValid) return CommandResult.Falha(pipeline.Notifications.Select(n => n.Message), "Dados do pipeline inválidos.");
            _context.CrmPipelines.Add(pipeline);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pipeline criado com sucesso!", new { pipeline.Id });
        }
    }

    public class CriarCrmEtapaCommandHandler : ICommandHandler<CriarCrmEtapaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmEtapaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmEtapaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var etapa = new CrmEtapa(request.PipelineId, request.TipoEtapa, request.Nome, request.Ordem, tenantId, usuario);
            if (!etapa.IsValid) return CommandResult.Falha(etapa.Notifications.Select(n => n.Message), "Dados da etapa inválidos.");
            _context.CrmEtapas.Add(etapa);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Etapa criada com sucesso!", new { etapa.Id });
        }
    }

    public class CriarCrmEtiquetaCommandHandler : ICommandHandler<CriarCrmEtiquetaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmEtiquetaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmEtiquetaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var etiqueta = new CrmEtiqueta(request.PipelineId, request.Nome, request.Cor, tenantId, usuario);
            if (!etiqueta.IsValid) return CommandResult.Falha(etiqueta.Notifications.Select(n => n.Message), "Dados da etiqueta inválidos.");
            _context.CrmEtiquetas.Add(etiqueta);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Etiqueta criada com sucesso!", new { etiqueta.Id });
        }
    }

    public class CriarCrmOrigemCommandHandler : ICommandHandler<CriarCrmOrigemCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmOrigemCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmOrigemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var origem = new CrmOrigem(request.Nome, tenantId, usuario);
            if (!origem.IsValid) return CommandResult.Falha(origem.Notifications.Select(n => n.Message), "Dados da origem inválidos.");
            _context.CrmOrigens.Add(origem);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Origem criada com sucesso!", new { origem.Id });
        }
    }

    public class CriarCrmCampanhaCommandHandler : ICommandHandler<CriarCrmCampanhaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCrmCampanhaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCrmCampanhaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var campanha = new CrmCampanha(
                request.Nome, request.Tipo, request.Status, request.DataInicio, request.DataFim, request.Frequencia,
                request.MoedaId, request.Orcamento, request.CustoEsperado, request.CustoReal, request.ReceitaEsperada,
                request.Objetivo, request.Conteudo, request.ResponsavelUsuarioId, tenantId, usuario);
            if (!campanha.IsValid) return CommandResult.Falha(campanha.Notifications.Select(n => n.Message), "Dados da campanha inválidos.");
            _context.CrmCampanhas.Add(campanha);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Campanha criada com sucesso!", new { campanha.Id });
        }
    }
}
