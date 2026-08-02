using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Domain.Services;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===== Criar monitoramento preditivo =====
    public record CriarMonitoramentoPreditivoCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        Guid? EquipamentoId,
        string? Observacao) : ICommand;

    public class CriarMonitoramentoPreditivoCommandValidator : AbstractValidator<CriarMonitoramentoPreditivoCommand>
    {
        public CriarMonitoramentoPreditivoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500);
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    public class CriarMonitoramentoPreditivoCommandHandler : ICommandHandler<CriarMonitoramentoPreditivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMonitoramentoPreditivoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMonitoramentoPreditivoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN: codigo unico por tenant.
            var existe = await _context.MonitoramentosPreditivos.AnyAsync(m => m.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe monitoramento preditivo com este codigo para a empresa/tenant.");

            var monitoramento = new MonitoramentoPreditivo(request.Codigo, request.Descricao, request.ResponsavelId, request.EquipamentoId, request.Observacao, tenantId, usuario);
            if (!monitoramento.IsValid)
                return CommandResult.Falha(monitoramento.Notifications.Select(n => n.Message));

            monitoramento.RegistrarHistorico(new HistoricoPreditivo(monitoramento.Id, null, EAcaoHistoricoPreditivo.Criado, request.ResponsavelId, null, "{}", tenantId, usuario));
            _context.MonitoramentosPreditivos.Add(monitoramento);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Monitoramento preditivo criado com sucesso.", new { monitoramento.Id });
        }
    }

    // ===== Adicionar ponto de medicao =====
    public record AdicionarPontoMedicaoCommand(
        Guid MonitoramentoId,
        Guid EquipamentoId,
        string CodigoPonto,
        string Variavel,
        string Unidade,
        string? LocalTecnico,
        string? Periodicidade) : ICommand;

    public class AdicionarPontoMedicaoCommandHandler : ICommandHandler<AdicionarPontoMedicaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarPontoMedicaoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarPontoMedicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var monitoramento = await _context.MonitoramentosPreditivos
                .Include(m => m.PontosMedicao)
                .FirstOrDefaultAsync(m => m.Id == request.MonitoramentoId, cancellationToken);
            if (monitoramento == null)
                return CommandResult.Falha("Monitoramento preditivo nao encontrado.");

            var ponto = new PontoMedicao(monitoramento.Id, request.EquipamentoId, request.CodigoPonto, request.Variavel,
                request.Unidade, request.LocalTecnico, request.Periodicidade, tenantId, usuario);

            monitoramento.AdicionarPontoMedicao(ponto, usuario);
            if (!monitoramento.IsValid)
                return CommandResult.Falha(monitoramento.Notifications.Select(n => n.Message));

            // Filho novo em agregado já existente: Add explícito garante estado Added (evita UPDATE de linha inexistente).
            _context.PontosMedicao.Add(ponto);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Ponto de medicao adicionado.", new { ponto.Id });
        }
    }

    // ===== Adicionar regra de monitoramento =====
    public record AdicionarRegraMonitoramentoCommand(
        Guid PontoMedicaoId,
        ETipoRegraMonitoramento TipoRegra,
        string? Operador,
        decimal? LimiteMinimo,
        decimal? LimiteMaximo,
        string? JanelaAvaliacao,
        string Severidade,
        string AcaoEsperada,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim) : ICommand;

    public class AdicionarRegraMonitoramentoCommandHandler : ICommandHandler<AdicionarRegraMonitoramentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarRegraMonitoramentoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarRegraMonitoramentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ponto = await _context.PontosMedicao
                .Include(p => p.Regras)
                .FirstOrDefaultAsync(p => p.Id == request.PontoMedicaoId, cancellationToken);
            if (ponto == null)
                return CommandResult.Falha("Ponto de medicao nao encontrado.");

            var regra = new RegraMonitoramento(ponto.Id, request.TipoRegra, request.Operador, request.LimiteMinimo,
                request.LimiteMaximo, request.JanelaAvaliacao, request.Severidade, request.AcaoEsperada,
                request.VigenciaInicio, request.VigenciaFim, tenantId, usuario);

            ponto.AdicionarRegra(regra, usuario);
            if (!ponto.IsValid)
                return CommandResult.Falha(ponto.Notifications.Select(n => n.Message));

            // Filho novo em agregado já existente: Add explícito garante estado Added (evita UPDATE de linha inexistente).
            _context.RegrasMonitoramento.Add(regra);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Regra de monitoramento adicionada.", new { regra.Id });
        }
    }

    // ===== Registrar leitura de condicao (somente ponto ativo) =====
    public record RegistrarLeituraCondicaoCommand(
        Guid PontoMedicaoId,
        DateTime DataHoraMedicao,
        decimal Valor,
        string Unidade,
        decimal? QualidadeDado,
        string Origem,
        string? PayloadJson) : ICommand;

    public class RegistrarLeituraCondicaoCommandHandler : ICommandHandler<RegistrarLeituraCondicaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarLeituraCondicaoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarLeituraCondicaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ponto = await _context.PontosMedicao
                .Include(p => p.Leituras)
                .Include(p => p.Regras)
                .FirstOrDefaultAsync(p => p.Id == request.PontoMedicaoId, cancellationToken);
            if (ponto == null)
                return CommandResult.Falha("Ponto de medicao nao encontrado.");

            // RN: monitoramento deve estar ativo para receber leitura.
            var monitoramento = await _context.MonitoramentosPreditivos
                .FirstOrDefaultAsync(m => m.Id == ponto.MonitoramentoId, cancellationToken);
            if (monitoramento == null || !monitoramento.PermiteLeituraOuAlarme())
                return CommandResult.Falha("Somente monitoramento ativo pode receber leitura.");

            // D12: validacao da leitura (unidade/sequencia/duplicidade/qualidade) -> LEITURA_INVALIDA.
            var duplicada = ponto.Leituras.Any(l => l.DataHoraMedicao == request.DataHoraMedicao);
            var ultimaData = ponto.Leituras.Any() ? ponto.Leituras.Max(l => l.DataHoraMedicao) : (DateTime?)null;
            var validacao = MotorAvaliacaoPreditiva.ValidarLeitura(
                request.Unidade, ponto.Unidade, request.QualidadeDado, request.DataHoraMedicao, ultimaData, duplicada,
                MotorAvaliacaoPreditiva.QualidadeMinimaDefault);
            if (!validacao.Valida)
                return CommandResult.Falha(validacao.Motivo!);

            var leitura = new LeituraCondicao(ponto.Id, request.DataHoraMedicao, request.Valor, request.Unidade,
                request.QualidadeDado, request.Origem, request.PayloadJson, tenantId, usuario);

            ponto.RegistrarLeitura(leitura, usuario);
            if (!ponto.IsValid)
                return CommandResult.Falha(ponto.Notifications.Select(n => n.Message));

            // Filho novo em agregado já existente: Add explícito garante estado Added (evita UPDATE de linha inexistente).
            _context.LeiturasCondicao.Add(leitura);

            // D11: avaliacao AUTOMATICA das regras vigentes na leitura (nao comando manual).
            var alarmesGerados = new System.Collections.Generic.List<Guid>();
            var alarmesAbertos = await _context.AlarmesPreditivos
                .Where(a => a.MonitoramentoId == monitoramento.Id
                            && a.PontoMedicaoId == ponto.Id
                            && (a.Status == EStatusAlarmePreditivo.Aberto || a.Status == EStatusAlarmePreditivo.EmAnalise))
                .Select(a => a.RegraId)
                .ToListAsync(cancellationToken);

            foreach (var regra in ponto.Regras)
            {
                if (!regra.GeraAlarme()) continue;
                if (regra.VigenciaInicio.HasValue && request.DataHoraMedicao < regra.VigenciaInicio.Value) continue;
                if (regra.VigenciaFim.HasValue && request.DataHoraMedicao > regra.VigenciaFim.Value) continue;
                if (!MotorAvaliacaoPreditiva.RegraDispara(request.Valor, regra.TipoRegra, regra.Operador, regra.LimiteMinimo, regra.LimiteMaximo)) continue;

                // D13 (correlacao/deduplicacao): nao repete alarme aberto para a mesma regra+ponto.
                if (alarmesAbertos.Contains(regra.Id)) continue;

                var descricao = $"Alarme automatico: {regra.TipoRegra} {regra.Operador} (min={regra.LimiteMinimo}, max={regra.LimiteMaximo}); leitura={request.Valor} {request.Unidade}.";
                var alarme = new AlarmePreditivo(monitoramento.Id, ponto.Id, regra.Id, leitura.Id, regra.Severidade, descricao, tenantId, usuario);
                if (!alarme.IsValid) continue;
                var historico = new HistoricoPreditivo(monitoramento.Id, alarme.Id, EAcaoHistoricoPreditivo.Disparado, monitoramento.ResponsavelId, null, "{}", tenantId, usuario);
                _context.AlarmesPreditivos.Add(alarme);
                _context.HistoricosPreditivos.Add(historico);
                alarmesGerados.Add(alarme.Id);
                alarmesAbertos.Add(regra.Id); // evita duplicar dentro do mesmo lote
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok(
                alarmesGerados.Count > 0
                    ? $"Leitura registrada; {alarmesGerados.Count} alarme(s) disparado(s) automaticamente."
                    : "Leitura registrada.",
                new { leitura.Id, AlarmesGerados = alarmesGerados });
        }
    }

    // ===== Disparar alarme (somente monitoramento ativo + regra ativa) =====
    public record DispararAlarmePreditivoCommand(
        Guid MonitoramentoId,
        Guid PontoMedicaoId,
        Guid RegraId,
        Guid? LeituraId,
        string Severidade,
        string Descricao) : ICommand;

    public class DispararAlarmePreditivoCommandHandler : ICommandHandler<DispararAlarmePreditivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DispararAlarmePreditivoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DispararAlarmePreditivoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var monitoramento = await _context.MonitoramentosPreditivos
                .Include(m => m.Alarmes)
                .FirstOrDefaultAsync(m => m.Id == request.MonitoramentoId, cancellationToken);
            if (monitoramento == null)
                return CommandResult.Falha("Monitoramento preditivo nao encontrado.");

            // RN: apenas monitoramento ativo gera alarme.
            if (!monitoramento.PermiteLeituraOuAlarme())
                return CommandResult.Falha("Somente monitoramento ativo pode gerar alarme.");

            // RN: apenas regra ativa gera alarme.
            var regra = await _context.RegrasMonitoramento.FirstOrDefaultAsync(r => r.Id == request.RegraId, cancellationToken);
            if (regra == null)
                return CommandResult.Falha("Regra de monitoramento nao encontrada.");
            if (!regra.GeraAlarme())
                return CommandResult.Falha("Somente regra ativa pode gerar alarme.");

            var alarme = new AlarmePreditivo(monitoramento.Id, request.PontoMedicaoId, request.RegraId, request.LeituraId,
                request.Severidade, request.Descricao, tenantId, usuario);

            monitoramento.RegistrarAlarme(alarme, usuario);
            if (!monitoramento.IsValid)
                return CommandResult.Falha(monitoramento.Notifications.Select(n => n.Message));

            var historico = new HistoricoPreditivo(monitoramento.Id, alarme.Id, EAcaoHistoricoPreditivo.Disparado, monitoramento.ResponsavelId, null, "{}", tenantId, usuario);
            monitoramento.RegistrarHistorico(historico);
            // Chave Guid é gerada pela convenção (ValueGeneratedOnAdd) e já vem preenchida do construtor; ao
            // pendurar filhos novos num agregado JÁ existente, o EF os classificaria como Modified. Add explícito
            // no DbSet garante estado Added (senão o SaveChanges tenta UPDATE de linha inexistente).
            _context.AlarmesPreditivos.Add(alarme);
            _context.HistoricosPreditivos.Add(historico);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alarme preditivo disparado.", new { alarme.Id });
        }
    }

    // ===== Converter alarme em ordem de trabalho =====
    public record ConverterAlarmeEmOrdemCommand(Guid AlarmeId, Guid OrdemTrabalhoId, string? StatusRetorno, string? PayloadRetorno) : ICommand;

    public class ConverterAlarmeEmOrdemCommandHandler : ICommandHandler<ConverterAlarmeEmOrdemCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConverterAlarmeEmOrdemCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConverterAlarmeEmOrdemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var alarme = await _context.AlarmesPreditivos
                .Include(a => a.Vinculos)
                .FirstOrDefaultAsync(a => a.Id == request.AlarmeId, cancellationToken);
            if (alarme == null)
                return CommandResult.Falha("Alarme preditivo nao encontrado.");

            var vinculo = new VinculoOrdemTrabalhoPreditivo(alarme.Id, request.OrdemTrabalhoId, request.StatusRetorno, request.PayloadRetorno, tenantId, usuario);
            alarme.ConverterEmOrdem(vinculo, usuario);
            if (!alarme.IsValid)
                return CommandResult.Falha(alarme.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alarme convertido em ordem de trabalho.", new { AlarmeId = alarme.Id, VinculoId = vinculo.Id, Status = alarme.Status.ToString() });
        }
    }

    // ===== Descartar alarme (motivo obrigatorio) =====
    public record DescartarAlarmePreditivoCommand(Guid AlarmeId, string Motivo) : ICommand;

    public class DescartarAlarmePreditivoCommandHandler : ICommandHandler<DescartarAlarmePreditivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public DescartarAlarmePreditivoCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DescartarAlarmePreditivoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var alarme = await _context.AlarmesPreditivos.FirstOrDefaultAsync(a => a.Id == request.AlarmeId, cancellationToken);
            if (alarme == null)
                return CommandResult.Falha("Alarme preditivo nao encontrado.");
            alarme.Descartar(request.Motivo, usuario);
            if (!alarme.IsValid)
                return CommandResult.Falha(alarme.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alarme descartado.", new { alarme.Id, Status = alarme.Status.ToString() });
        }
    }

    // ===== Workflow do monitoramento =====
    public record SubmeterMonitoramentoPreditivoCommand(Guid MonitoramentoId) : ICommand;
    public record AprovarMonitoramentoPreditivoCommand(Guid MonitoramentoId) : ICommand;
    public record SuspenderMonitoramentoPreditivoCommand(Guid MonitoramentoId) : ICommand;
    public record EncerrarMonitoramentoPreditivoCommand(Guid MonitoramentoId, string Motivo) : ICommand;

    public class MonitoramentoPreditivoWorkflowHandler :
        ICommandHandler<SubmeterMonitoramentoPreditivoCommand>,
        ICommandHandler<AprovarMonitoramentoPreditivoCommand>,
        ICommandHandler<SuspenderMonitoramentoPreditivoCommand>,
        ICommandHandler<EncerrarMonitoramentoPreditivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public MonitoramentoPreditivoWorkflowHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        private async Task<MonitoramentoPreditivo?> Carregar(Guid id, CancellationToken ct) =>
            await _context.MonitoramentosPreditivos.FirstOrDefaultAsync(m => m.Id == id, ct);

        public async Task<CommandResult> Handle(SubmeterMonitoramentoPreditivoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var m = await Carregar(request.MonitoramentoId, cancellationToken);
            if (m == null) return CommandResult.Falha("Monitoramento preditivo nao encontrado.");
            m.Submeter(usuario);
            if (!m.IsValid) return CommandResult.Falha(m.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Monitoramento submetido para analise.", new { m.Id, Status = m.Status.ToString() });
        }

        public async Task<CommandResult> Handle(AprovarMonitoramentoPreditivoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var m = await _context.MonitoramentosPreditivos.Include(x => x.Historicos).FirstOrDefaultAsync(x => x.Id == request.MonitoramentoId, cancellationToken);
            if (m == null) return CommandResult.Falha("Monitoramento preditivo nao encontrado.");
            m.Aprovar(usuario);
            if (!m.IsValid) return CommandResult.Falha(m.Notifications.Select(n => n.Message));
            var historicoAprovacao = new HistoricoPreditivo(m.Id, null, EAcaoHistoricoPreditivo.Aprovado, m.ResponsavelId, null, "{}", tenantId, usuario);
            m.RegistrarHistorico(historicoAprovacao);
            // Filho novo em agregado já existente: Add explícito garante estado Added (evita UPDATE de linha inexistente).
            _context.HistoricosPreditivos.Add(historicoAprovacao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Monitoramento aprovado e ativado.", new { m.Id, Status = m.Status.ToString() });
        }

        public async Task<CommandResult> Handle(SuspenderMonitoramentoPreditivoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var m = await Carregar(request.MonitoramentoId, cancellationToken);
            if (m == null) return CommandResult.Falha("Monitoramento preditivo nao encontrado.");
            m.Suspender(usuario);
            if (!m.IsValid) return CommandResult.Falha(m.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Monitoramento suspenso.", new { m.Id, Status = m.Status.ToString() });
        }

        public async Task<CommandResult> Handle(EncerrarMonitoramentoPreditivoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var m = await Carregar(request.MonitoramentoId, cancellationToken);
            if (m == null) return CommandResult.Falha("Monitoramento preditivo nao encontrado.");
            m.Encerrar(request.Motivo, usuario);
            if (!m.IsValid) return CommandResult.Falha(m.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Monitoramento encerrado.", new { m.Id, Status = m.Status.ToString() });
        }
    }
}
