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
    public class CriarFrameworkRelCommandHandler : ICommandHandler<CriarFrameworkRelCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFrameworkRelCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFrameworkRelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.FrameworksRel.AnyAsync(f => f.Codigo == request.Codigo && f.Versao == request.Versao, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe framework {request.Codigo} versao {request.Versao}.");

            var framework = new FrameworkRel(request.Codigo, request.Versao, request.Descricao, request.InicioVigencia, request.FimVigencia, tenantId, usuario);
            if (!framework.IsValid)
                return CommandResult.Falha(framework.Notifications.Select(n => n.Message));

            _context.FrameworksRel.Add(framework);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Framework criado!", new { framework.Id });
        }
    }

    public class AdicionarRequisitoRelCommandHandler : ICommandHandler<AdicionarRequisitoRelCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarRequisitoRelCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarRequisitoRelCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var framework = await _context.FrameworksRel.AnyAsync(f => f.Id == request.FrameworkId, cancellationToken);
            if (!framework)
                return CommandResult.Falha("Framework nao encontrado.");

            var requisito = new RequisitoRel(request.FrameworkId, request.Codigo, request.Titulo, request.TipoResposta, request.Obrigatorio, request.Ordem, tenantId, usuario);
            if (!requisito.IsValid)
                return CommandResult.Falha(requisito.Notifications.Select(n => n.Message));

            _context.RequisitosRel.Add(requisito);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Requisito adicionado!", new { requisito.Id });
        }
    }

    public class AdicionarItemRelatorioCommandHandler : ICommandHandler<AdicionarItemRelatorioCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemRelatorioCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemRelatorioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var relatorio = await _context.RelatoriosESG.AnyAsync(r => r.Id == request.RelatorioId, cancellationToken);
            if (!relatorio)
                return CommandResult.Falha("Relatorio ESG nao encontrado.");

            var duplicado = await _context.ItensRelatorioEsg.AnyAsync(i => i.RelatorioId == request.RelatorioId && i.Sequencia == request.Sequencia, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Ja existe item com sequencia {request.Sequencia} neste relatorio.");

            var item = new ItemRelatorioEsg(request.RelatorioId, request.Sequencia, request.Quantidade, request.Observacao, request.RequisitoId, request.TipoConteudo, request.Justificativa, tenantId, usuario);
            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            _context.ItensRelatorioEsg.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item adicionado ao relatorio!", new { item.Id });
        }
    }

    public class VincularIndicadorReferenciaCommandHandler : ICommandHandler<VincularIndicadorReferenciaCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularIndicadorReferenciaCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularIndicadorReferenciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var item = await _context.ItensRelatorioEsg.AnyAsync(i => i.Id == request.ItemId, cancellationToken);
            if (!item)
                return CommandResult.Falha("Item do relatorio nao encontrado.");

            var indicador = new IndicadorReferencia(request.ItemId, request.OrigemDominio, request.OrigemEntidade, request.OrigemId, request.CodigoIndicador, request.RegraComposicao, tenantId, usuario);
            if (!indicador.IsValid)
                return CommandResult.Falha(indicador.Notifications.Select(n => n.Message));

            _context.IndicadoresReferencia.Add(indicador);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Indicador vinculado!", new { indicador.Id });
        }
    }

    public class CapturarSnapshotIndicadorCommandHandler : ICommandHandler<CapturarSnapshotIndicadorCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CapturarSnapshotIndicadorCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CapturarSnapshotIndicadorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var indicador = await _context.IndicadoresReferencia.AnyAsync(i => i.Id == request.IndicadorReferenciaId, cancellationToken);
            if (!indicador)
                return CommandResult.Falha("Indicador referenciado nao encontrado.");

            var snapshot = new SnapshotIndicador(request.IndicadorReferenciaId, request.OrigemVersao, request.DataCorte, request.ValorNumerico, request.ValorTexto, request.Unidade, request.Dimensoes, request.StatusOrigem, tenantId, usuario);
            if (!snapshot.IsValid)
                return CommandResult.Falha(snapshot.Notifications.Select(n => n.Message));

            _context.SnapshotsIndicador.Add(snapshot);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Snapshot capturado!", new { snapshot.Id, snapshot.HashConteudo });
        }
    }

    public class AbrirPendenciaRelatorioCommandHandler : ICommandHandler<AbrirPendenciaRelatorioCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirPendenciaRelatorioCommandHandler(ContextESG context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirPendenciaRelatorioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var relatorio = await _context.RelatoriosESG.AnyAsync(r => r.Id == request.RelatorioId, cancellationToken);
            if (!relatorio)
                return CommandResult.Falha("Relatorio ESG nao encontrado.");

            var pendencia = new PendenciaRelatorio(request.RelatorioId, request.ItemId, request.Criticidade, request.ResponsavelId, request.Prazo, tenantId, usuario);
            if (!pendencia.IsValid)
                return CommandResult.Falha(pendencia.Notifications.Select(n => n.Message));

            _context.PendenciasRelatorio.Add(pendencia);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Pendencia aberta!", new { pendencia.Id });
        }
    }
}
