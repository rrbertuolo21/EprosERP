using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===== Criar revisao de confiabilidade =====
    public record CriarRevisaoConfiabilidadeCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        Guid? AtivoId,
        string? FuncaoOperacional,
        string? EstadoConservacao,
        string? CriticidadeOperacional) : ICommand;

    public class CriarRevisaoConfiabilidadeCommandValidator : AbstractValidator<CriarRevisaoConfiabilidadeCommand>
    {
        public CriarRevisaoConfiabilidadeCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500);
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    public class CriarRevisaoConfiabilidadeCommandHandler : ICommandHandler<CriarRevisaoConfiabilidadeCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRevisaoConfiabilidadeCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-CRV-001: codigo unico por tenant.
            var existe = await _context.RevisoesConfiabilidade.AnyAsync(r => r.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe revisao de confiabilidade com este codigo para a empresa/tenant.");

            var revisao = new RevisaoConfiabilidade(
                request.Codigo, request.Descricao, request.ResponsavelId, request.AtivoId,
                request.FuncaoOperacional, request.EstadoConservacao, request.CriticidadeOperacional, tenantId, usuario);
            if (!revisao.IsValid)
                return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));

            revisao.RegistrarHistorico(new HistoricoConfiabilidade(revisao.Id, EAcaoHistoricoConfiabilidade.Criado, request.ResponsavelId, null, null, "{}", tenantId, usuario));
            _context.RevisoesConfiabilidade.Add(revisao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao de confiabilidade criada com sucesso.", new { revisao.Id });
        }
    }

    // ===== Adicionar modo de falha / FMEA =====
    public record AdicionarModoFalhaCommand(
        Guid RevisaoId,
        int Sequencia,
        string? Componente,
        string ModoFalha,
        string? EfeitoFalha,
        string? CausaFalha,
        string? ControleAtual,
        int? Severidade,
        int? Ocorrencia,
        int? Deteccao,
        decimal? Quantidade,
        string? Observacao) : ICommand;

    public class AdicionarModoFalhaCommandHandler : ICommandHandler<AdicionarModoFalhaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarModoFalhaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarModoFalhaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var revisao = await _context.RevisoesConfiabilidade
                .Include(r => r.ModosFalha)
                .FirstOrDefaultAsync(r => r.Id == request.RevisaoId, cancellationToken);
            if (revisao == null)
                return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");

            var modo = new ModoFalhaConfiabilidade(
                revisao.Id, request.Sequencia, request.Componente, request.ModoFalha, request.EfeitoFalha,
                request.CausaFalha, request.ControleAtual, request.Severidade, request.Ocorrencia, request.Deteccao,
                request.Quantidade, request.Observacao, tenantId, usuario);

            revisao.AdicionarModoFalha(modo, usuario);
            if (!revisao.IsValid)
                return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Modo de falha adicionado.", new { modo.Id, modo.Rpn });
        }
    }

    // ===== Registrar indicador =====
    public record RegistrarIndicadorConfiabilidadeCommand(
        Guid RevisaoId,
        ETipoIndicadorConfiabilidade TipoIndicador,
        DateTime? PeriodoInicio,
        DateTime? PeriodoFim,
        decimal Valor,
        string? Unidade,
        string FormulaAplicada,
        string? OrigemDados,
        ECalculadoPorConfiabilidade CalculadoPor) : ICommand;

    public class RegistrarIndicadorConfiabilidadeCommandHandler : ICommandHandler<RegistrarIndicadorConfiabilidadeCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarIndicadorConfiabilidadeCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarIndicadorConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var revisao = await _context.RevisoesConfiabilidade
                .Include(r => r.Indicadores)
                .FirstOrDefaultAsync(r => r.Id == request.RevisaoId, cancellationToken);
            if (revisao == null)
                return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");

            var indicador = new IndicadorConfiabilidade(
                revisao.Id, request.TipoIndicador, request.PeriodoInicio, request.PeriodoFim, request.Valor,
                request.Unidade, request.FormulaAplicada, request.OrigemDados, request.CalculadoPor, tenantId, usuario);

            revisao.AdicionarIndicador(indicador, usuario);
            if (!revisao.IsValid)
                return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Indicador registrado.", new { indicador.Id });
        }
    }

    // ===== Recomendar estrategia =====
    public record RecomendarEstrategiaCommand(
        Guid RevisaoId,
        EEstrategiaManutencao Estrategia,
        string Justificativa,
        int? RpnReferencia,
        decimal? MtbfReferencia,
        decimal? MttrReferencia,
        decimal? DisponibilidadeReferencia,
        Guid ResponsavelId) : ICommand;

    public class RecomendarEstrategiaCommandHandler : ICommandHandler<RecomendarEstrategiaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RecomendarEstrategiaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RecomendarEstrategiaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var revisao = await _context.RevisoesConfiabilidade
                .Include(r => r.Recomendacoes)
                .FirstOrDefaultAsync(r => r.Id == request.RevisaoId, cancellationToken);
            if (revisao == null)
                return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");

            var recomendacao = new RecomendacaoEstrategia(
                revisao.Id, request.Estrategia, request.Justificativa, request.RpnReferencia, request.MtbfReferencia,
                request.MttrReferencia, request.DisponibilidadeReferencia, request.ResponsavelId, tenantId, usuario);

            revisao.AdicionarRecomendacao(recomendacao, usuario);
            if (!revisao.IsValid)
                return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Recomendacao de estrategia registrada.", new { recomendacao.Id });
        }
    }

    // ===== Workflow: submeter / aprovar / rejeitar / suspender / retomar / encerrar =====
    public record SubmeterRevisaoConfiabilidadeCommand(Guid RevisaoId) : ICommand;
    public record AprovarRevisaoConfiabilidadeCommand(Guid RevisaoId, Guid AprovadorId) : ICommand;
    public record RejeitarRevisaoConfiabilidadeCommand(Guid RevisaoId, string Motivo) : ICommand;
    public record SuspenderRevisaoConfiabilidadeCommand(Guid RevisaoId, string Motivo) : ICommand;
    public record RetomarRevisaoConfiabilidadeCommand(Guid RevisaoId) : ICommand;
    public record EncerrarRevisaoConfiabilidadeCommand(Guid RevisaoId, string Motivo) : ICommand;

    public class RevisaoConfiabilidadeWorkflowHandler :
        ICommandHandler<SubmeterRevisaoConfiabilidadeCommand>,
        ICommandHandler<AprovarRevisaoConfiabilidadeCommand>,
        ICommandHandler<RejeitarRevisaoConfiabilidadeCommand>,
        ICommandHandler<SuspenderRevisaoConfiabilidadeCommand>,
        ICommandHandler<RetomarRevisaoConfiabilidadeCommand>,
        ICommandHandler<EncerrarRevisaoConfiabilidadeCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RevisaoConfiabilidadeWorkflowHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        private async Task<RevisaoConfiabilidade?> Carregar(Guid id, CancellationToken ct) =>
            await _context.RevisoesConfiabilidade.FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<CommandResult> Handle(SubmeterRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await Carregar(request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Submeter(usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao submetida para analise.", new { revisao.Id, Status = revisao.Status.ToString() });
        }

        public async Task<CommandResult> Handle(AprovarRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await _context.RevisoesConfiabilidade.Include(r => r.Historicos).FirstOrDefaultAsync(r => r.Id == request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Aprovar(request.AprovadorId, usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            revisao.RegistrarHistorico(new HistoricoConfiabilidade(revisao.Id, EAcaoHistoricoConfiabilidade.Aprovado, request.AprovadorId, null, null, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao aprovada e ativada.", new { revisao.Id, Status = revisao.Status.ToString() });
        }

        public async Task<CommandResult> Handle(RejeitarRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await Carregar(request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Rejeitar(request.Motivo, usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao rejeitada.", new { revisao.Id, Status = revisao.Status.ToString() });
        }

        public async Task<CommandResult> Handle(SuspenderRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await Carregar(request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Suspender(request.Motivo, usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao suspensa.", new { revisao.Id, Status = revisao.Status.ToString() });
        }

        public async Task<CommandResult> Handle(RetomarRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await Carregar(request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Retomar(usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao retomada.", new { revisao.Id, Status = revisao.Status.ToString() });
        }

        public async Task<CommandResult> Handle(EncerrarRevisaoConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var revisao = await Carregar(request.RevisaoId, cancellationToken);
            if (revisao == null) return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");
            revisao.Encerrar(request.Motivo, usuario);
            if (!revisao.IsValid) return CommandResult.Falha(revisao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Revisao encerrada.", new { revisao.Id, Status = revisao.Status.ToString() });
        }
    }
}
