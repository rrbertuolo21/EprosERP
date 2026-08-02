using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Ins;
using Epros.Modules.Qualidade.Application.Queries.Ins;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers.Ins
{
    // ============ Query: simulador do motor AQL ============
    public class CalcularPlanoAmostragemQueryHandler : IQueryHandler<CalcularPlanoAmostragemQuery, CommandResult>
    {
        private readonly MotorAql _motor;
        public CalcularPlanoAmostragemQueryHandler(MotorAql motor) => _motor = motor;

        public Task<CommandResult> Handle(CalcularPlanoAmostragemQuery request, CancellationToken cancellationToken)
        {
            if (request.TamanhoLote < 1)
                return Task.FromResult(CommandResult.Falha("O tamanho do lote deve ser >= 1."));

            var plano = _motor.CalcularPlano(request.TamanhoLote, request.Nivel, request.Aql, request.Severidade);
            return Task.FromResult(CommandResult.Ok("Plano de amostragem calculado.", new
            {
                plano.TamanhoLote,
                Nivel = plano.Nivel.ToString(),
                plano.Aql,
                Severidade = plano.Severidade.ToString(),
                LetraCodigo = plano.LetraCodigo.ToString(),
                plano.TamanhoAmostra,
                plano.NumeroAceitacao,
                plano.NumeroRejeicao,
                plano.InspecaoTotal
            }));
        }
    }

    // ============ Comandos: caracteristica / regra / ativacao / status / execucao ============
    public class AdicionarCaracteristicaPlanoCommandHandler : ICommandHandler<AdicionarCaracteristicaPlanoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarCaracteristicaPlanoCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarCaracteristicaPlanoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var plano = await _context.PlanosInspecao.FirstOrDefaultAsync(p => p.Id == request.PlanoId, ct);
            if (plano is null) return CommandResult.Falha("Plano de inspecao nao encontrado.", block: true);
            if (plano.Status == EStatusRegistroQualidade.Ativo || plano.Status == EStatusRegistroQualidade.Encerrado)
                return CommandResult.Falha("Nao e possivel adicionar caracteristica a plano ativo/encerrado.", block: true);

            if (await _context.CaracteristicasPlano.AnyAsync(c => c.PlanoId == request.PlanoId && c.Sequencia == request.Sequencia, ct))
                return CommandResult.Falha($"Ja existe uma caracteristica com a sequencia {request.Sequencia} neste plano.", block: true);

            var carac = new CaracteristicaPlano(request.PlanoId, request.Sequencia, request.Nome, request.TipoCaracteristica,
                request.TipoDado, request.Obrigatoria, request.AtributoId, request.UnidadeMedidaId, request.ValorNominal,
                request.LimiteInferior, request.LimiteSuperior, request.CriterioQualitativo, tenantId, usuario);
            if (!carac.IsValid) return CommandResult.Falha(carac.Notifications.Select(n => n.Message));

            _context.CaracteristicasPlano.Add(carac);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Caracteristica adicionada ao plano.", new { carac.Id, carac.Sequencia });
        }
    }

    public class AdicionarRegraAmostragemCommandHandler : ICommandHandler<AdicionarRegraAmostragemCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarRegraAmostragemCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarRegraAmostragemCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var plano = await _context.PlanosInspecao.FirstOrDefaultAsync(p => p.Id == request.PlanoId, ct);
            if (plano is null) return CommandResult.Falha("Plano de inspecao nao encontrado.", block: true);

            var regra = new RegraAmostragem(request.PlanoId, request.TipoAmostragem, request.CaracteristicaId,
                request.NivelInspecao, request.Aql, request.FaixaLoteMin, request.FaixaLoteMax, request.TamanhoAmostra,
                request.CriterioAceite, request.CriterioRejeicao, request.Severidade, tenantId, usuario);
            if (!regra.IsValid) return CommandResult.Falha(regra.Notifications.Select(n => n.Message));

            _context.RegrasAmostragem.Add(regra);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Regra de amostragem adicionada ao plano.", new { regra.Id, TipoAmostragem = regra.TipoAmostragem.ToString() });
        }
    }

    public class AtivarPlanoInspecaoCommandHandler : ICommandHandler<AtivarPlanoInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ICurrentUser _user;
        public AtivarPlanoInspecaoCommandHandler(ContextQualidade context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtivarPlanoInspecaoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var plano = await _context.PlanosInspecao.FirstOrDefaultAsync(p => p.Id == request.PlanoId, ct);
            if (plano is null) return CommandResult.Falha("Plano de inspecao nao encontrado.", block: true);

            // Caracteristicas sao persistidas em tabela propria (nav Ignore): reidrata p/ a invariante de Ativar.
            var caracteristicas = await _context.CaracteristicasPlano.Where(c => c.PlanoId == request.PlanoId).ToListAsync(ct);
            foreach (var c in caracteristicas) plano.AdicionarCaracteristica(c);

            plano.Ativar(usuario);
            if (!plano.IsValid) return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Plano de inspecao ativado.", new { plano.Id, Status = plano.Status.ToString() });
        }
    }

    public class AlterarStatusPlanoInspecaoCommandHandler : ICommandHandler<AlterarStatusPlanoInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ICurrentUser _user;
        public AlterarStatusPlanoInspecaoCommandHandler(ContextQualidade context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AlterarStatusPlanoInspecaoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var plano = await _context.PlanosInspecao.FirstOrDefaultAsync(p => p.Id == request.PlanoId, ct);
            if (plano is null) return CommandResult.Falha("Plano de inspecao nao encontrado.", block: true);

            plano.AlterarStatus(request.NovoStatus, request.Motivo, usuario);
            if (!plano.IsValid) return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Status do plano alterado.", new { plano.Id, Status = plano.Status.ToString() });
        }
    }

    public class ExecutarInspecaoCommandHandler : ICommandHandler<ExecutarInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly MotorAql _motor;
        public ExecutarInspecaoCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user, MotorAql motor)
        { _context = context; _tenant = tenant; _user = user; _motor = motor; }

        public async Task<CommandResult> Handle(ExecutarInspecaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var plano = await _context.PlanosInspecao.FirstOrDefaultAsync(p => p.Id == request.PlanoId, ct);
            if (plano is null) return CommandResult.Falha("Plano de inspecao nao encontrado.", block: true);
            // RN-INS-004/007: so plano Ativo executa.
            if (plano.Status != EStatusRegistroQualidade.Ativo)
                return CommandResult.Falha("Somente planos ativos podem gerar execucao de inspecao.", block: true);

            var exec = new ExecucaoInspecao(request.PlanoId, request.ReferenciaTipo, request.ReferenciaId,
                request.QuantidadeLote, request.InspetorId, tenantId, usuario);
            if (!exec.IsValid) return CommandResult.Falha(exec.Notifications.Select(n => n.Message));

            // Resolve amostragem: AQL explicito > regra AQL do plano > tamanho fixo de regra.
            var regras = await _context.RegrasAmostragem.Where(r => r.PlanoId == request.PlanoId).ToListAsync(ct);
            var (amostra, planoAql) = ResolverAmostra(request, regras);
            if (amostra.HasValue)
            {
                exec.DefinirAmostraCalculada(amostra.Value, usuario);
                if (!exec.IsValid) return CommandResult.Falha(exec.Notifications.Select(n => n.Message));
            }

            _context.ExecucoesInspecao.Add(exec);
            await _context.SaveChangesAsync(ct);

            object dados = planoAql is null
                ? new { exec.Id, Status = exec.Status.ToString(), exec.TamanhoAmostraCalculado }
                : new
                {
                    exec.Id,
                    Status = exec.Status.ToString(),
                    exec.TamanhoAmostraCalculado,
                    LetraCodigo = planoAql.LetraCodigo.ToString(),
                    planoAql.NumeroAceitacao,
                    planoAql.NumeroRejeicao,
                    planoAql.InspecaoTotal
                };
            return CommandResult.Ok("Execucao de inspecao aberta.", dados);
        }

        private (int? amostra, PlanoAmostragemResultado? planoAql) ResolverAmostra(
            ExecutarInspecaoCommand request, System.Collections.Generic.List<RegraAmostragem> regras)
        {
            var regraAql = regras.FirstOrDefault(r => r.TipoAmostragem == ETipoAmostragem.AQL);

            decimal? aql = request.Aql;
            string? nivelStr = request.NivelInspecao;
            string? sevStr = request.Severidade;
            if (aql is null && regraAql != null)
            {
                if (decimal.TryParse(regraAql.Aql, NumberStyles.Any, CultureInfo.InvariantCulture, out var aqlRegra))
                    aql = aqlRegra;
                nivelStr ??= regraAql.NivelInspecao;
                sevStr ??= regraAql.Severidade;
            }

            if (aql.HasValue && Enum.TryParse<ENivelInspecao>(nivelStr, true, out var nivel))
            {
                var sev = Enum.TryParse<ESeveridadeAql>(sevStr, true, out var s) ? s : ESeveridadeAql.Normal;
                var resultado = _motor.CalcularPlano((long)request.QuantidadeLote, nivel, aql.Value, sev);
                return (resultado.TamanhoAmostra, resultado);
            }

            var regraFixa = regras.FirstOrDefault(r => r.TamanhoAmostra.HasValue);
            return (regraFixa?.TamanhoAmostra, null);
        }
    }

    // ============ Comando: registrar medicao (secao 11.6) ============
    public class RegistrarMedicaoCommandHandler : ICommandHandler<RegistrarMedicaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarMedicaoCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarMedicaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var exec = await _context.ExecucoesInspecao.FirstOrDefaultAsync(e => e.Id == request.ExecucaoId, ct);
            if (exec is null) return CommandResult.Falha("Execucao de inspecao nao encontrada.", block: true);
            // So se mede enquanto a execucao esta aberta/em coleta (RN-INS: nao mede execucao finalizada).
            if (exec.Status == EStatusExecucaoInspecao.Concluida || exec.Status == EStatusExecucaoInspecao.Cancelada)
                return CommandResult.Falha("Execucao ja finalizada — nao aceita novas medicoes.", block: true);

            // A caracteristica precisa pertencer ao plano da execucao.
            var caracOk = await _context.CaracteristicasPlano
                .AnyAsync(c => c.Id == request.CaracteristicaId && c.PlanoId == exec.PlanoId, ct);
            if (!caracOk) return CommandResult.Falha("Caracteristica nao pertence ao plano desta execucao.", block: true);

            if (request.AmostraId.HasValue)
            {
                var amostraOk = await _context.AmostrasInspecionadas
                    .AnyAsync(a => a.Id == request.AmostraId.Value && a.ExecucaoId == request.ExecucaoId, ct);
                if (!amostraOk) return CommandResult.Falha("Amostra nao pertence a esta execucao.", block: true);
            }

            var medicao = new Medicao(request.ExecucaoId, request.CaracteristicaId, request.Resultado, request.MedidoPor,
                request.AmostraId, request.ValorDecimal, request.ValorTexto, request.ValorBooleano, request.Desvio,
                request.Observacao, tenantId, usuario);
            if (!medicao.IsValid) return CommandResult.Falha(medicao.Notifications.Select(n => n.Message));

            _context.Medicoes.Add(medicao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Medicao registrada.", new
            {
                medicao.Id,
                Resultado = medicao.Resultado.ToString(),
                Desvio = medicao.Resultado == EResultadoMedicao.NaoConforme
            });
        }
    }

    // ============ Comando: concluir inspecao (secao 11.7) + gatilho ACR/NCR ============
    public class ConcluirInspecaoCommandHandler : ICommandHandler<ConcluirInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public ConcluirInspecaoCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(ConcluirInspecaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var exec = await _context.ExecucoesInspecao.FirstOrDefaultAsync(e => e.Id == request.ExecucaoId, ct);
            if (exec is null) return CommandResult.Falha("Execucao de inspecao nao encontrada.", block: true);
            if (exec.Status == EStatusExecucaoInspecao.Concluida || exec.Status == EStatusExecucaoInspecao.Cancelada)
                return CommandResult.Falha("Execucao ja finalizada.", block: true);
            if (await _context.ResultadosInspecao.AnyAsync(r => r.ExecucaoId == request.ExecucaoId, ct))
                return CommandResult.Falha("Ja existe resultado consolidado para esta execucao.", block: true);

            // Consolida os numeros a partir das medicoes persistidas (estrutura/agregacao — sim).
            var medicoes = await _context.Medicoes.Where(m => m.ExecucaoId == request.ExecucaoId).ToListAsync(ct);
            var totalDesvios = medicoes.Count(m => m.Resultado == EResultadoMedicao.NaoConforme);
            var totalAmostras = await _context.AmostrasInspecionadas.CountAsync(a => a.ExecucaoId == request.ExecucaoId, ct);
            if (totalAmostras == 0) totalAmostras = medicoes.Select(m => m.AmostraId).Where(id => id.HasValue).Distinct().Count();
            if (totalAmostras == 0 && medicoes.Count > 0) totalAmostras = medicoes.Count;

            // O criterio Ac/Re (numero de aceitacao vs. desvios) e da norma AQL: // valida (PDF ABNT NBR 5426).
            // Sem criterio explicito informado, aplica o default seguro: 0 desvio = Aprovado; caso contrario Reprovado.
            var resultado = request.Resultado ??
                (totalDesvios == 0 ? EResultadoInspecaoConsolidado.Aprovado : EResultadoInspecaoConsolidado.Reprovado);

            var gerarNcr = resultado == EResultadoInspecaoConsolidado.Reprovado || resultado == EResultadoInspecaoConsolidado.Inconclusivo;
            var gerarAcr = true; // toda inspecao concluida alimenta a analise de aceitacao/rejeicao (ACR).

            var res = new ResultadoInspecao(request.ExecucaoId, resultado, totalAmostras, totalDesvios, gerarAcr, gerarNcr,
                request.ConcluidoPor, request.CriterioAceiteAplicado, request.Conclusao, tenantId, usuario);
            if (!res.IsValid) return CommandResult.Falha(res.Notifications.Select(n => n.Message));
            _context.ResultadosInspecao.Add(res);

            // Reflete no ciclo de vida da execucao (Concluir valida transicao).
            var preliminar = resultado switch
            {
                EResultadoInspecaoConsolidado.Aprovado => EResultadoPreliminar.Conforme,
                EResultadoInspecaoConsolidado.AprovadoComRestricao => EResultadoPreliminar.Alerta,
                EResultadoInspecaoConsolidado.Inconclusivo => EResultadoPreliminar.Inconclusivo,
                _ => EResultadoPreliminar.NaoConforme
            };
            exec.Concluir(preliminar, request.Conclusao, usuario);
            if (!exec.IsValid) return CommandResult.Falha(exec.Notifications.Select(n => n.Message));

            // Gatilho: alimenta ACR (sempre) e, quando reprovado, SUGERE NCR — via Outbox (nao cria a revelia).
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.InsInspecaoConcluida,
                JsonSerializer.Serialize(new
                {
                    execucaoId = exec.Id, planoId = exec.PlanoId, resultadoId = res.Id,
                    resultado = resultado.ToString(), totalAmostras, totalDesvios,
                    referenciaTipo = exec.ReferenciaTipo.ToString(), referenciaId = exec.ReferenciaId,
                    gerarAcr, gerarNcr, tenantId
                })));

            if (gerarNcr)
            {
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.InsNcrSolicitada,
                    JsonSerializer.Serialize(new
                    {
                        execucaoId = exec.Id, resultadoId = res.Id, resultado = resultado.ToString(),
                        totalDesvios, referenciaTipo = exec.ReferenciaTipo.ToString(), referenciaId = exec.ReferenciaId, tenantId
                    })));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Inspecao concluida.", new
            {
                exec.Id,
                ResultadoId = res.Id,
                Resultado = resultado.ToString(),
                Status = exec.Status.ToString(),
                totalAmostras,
                totalDesvios,
                GerouSugestaoNcr = gerarNcr,
                AlimentaAcr = gerarAcr
            });
        }
    }

    // ============ Comando: comutacao de severidade PERSISTIDA (NBR 5427, RN-01..RN-06) ============
    public class RegistrarLoteComutacaoCommandHandler : ICommandHandler<RegistrarLoteComutacaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly MotorComutacao _motor;
        public RegistrarLoteComutacaoCommandHandler(ContextQualidade context, ITenantProvider tenant, ICurrentUser user, MotorComutacao motor)
        { _context = context; _tenant = tenant; _user = user; _motor = motor; }

        public async Task<CommandResult> Handle(RegistrarLoteComutacaoCommand request, CancellationToken ct)
        {
            var (estado, jaSuspensa) = await ServicoComutacaoInspecao.RegistrarLoteAsync(
                _context, _tenant.GetTenantId(), _user.GetUserId() ?? "system", _motor,
                request.FornecedorId, request.ProdutoId, request.Aql, request.Decisao, request.Defeituosos,
                new OpcoesComutacao
                {
                    AtenuadaHabilitada = request.AtenuadaHabilitada,
                    ProducaoEstavel = request.ProducaoEstavel,
                    LimiteDefeituososAtenuada = request.LimiteDefeituososAtenuada ?? int.MaxValue
                }, ct);
            if (!estado.IsValid) return CommandResult.Falha(estado.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(
                jaSuspensa
                    ? "Regime suspenso (RN-06): fornecedor deve corrigir o processo antes de retomar."
                    : "Lote registrado; estado de comutacao persistido.",
                new
                {
                    estado.Id,
                    SeveridadeProximoLote = estado.Severidade.ToString(),
                    estado.Suspensa,
                    estado.ConsecutivosAceitosNormal,
                    estado.ConsecutivosAceitosSevera,
                    estado.RejeitadosAcumuladosSevera,
                    estado.LotesProcessados
                });
        }
    }

    /// <summary>
    /// Servico de aplicacao da comutacao de severidade: recupera o estado persistido por
    /// (fornecedor x produto x AQL), aciona o <see cref="MotorComutacao"/> (puro) e grava de volta.
    /// Ponto unico de "acionar na inspeccao" — chamado pelo comando dedicado e reutilizavel pelo
    /// fluxo de recebimento/ACR ao decidir aceitar/rejeitar um lote.
    /// </summary>
    internal static class ServicoComutacaoInspecao
    {
        public static async Task<(EstadoComutacaoInspecao estado, bool suspensaAntes)> RegistrarLoteAsync(
            ContextQualidade context, string tenantId, string usuario, MotorComutacao motor,
            Guid fornecedorId, Guid produtoId, string aql, EDecisaoLote decisao, int defeituosos,
            OpcoesComutacao opcoes, CancellationToken ct)
        {
            var entidade = await context.EstadosComutacaoInspecao
                .FirstOrDefaultAsync(e => e.FornecedorId == fornecedorId && e.ProdutoId == produtoId && e.Aql == aql, ct);

            if (entidade is null)
            {
                entidade = new EstadoComutacaoInspecao(fornecedorId, produtoId, aql, tenantId, usuario);
                if (!entidade.IsValid) return (entidade, false);
                context.EstadosComutacaoInspecao.Add(entidade);
            }

            var suspensaAntes = entidade.Suspensa;
            var estadoMotor = entidade.ParaEstadoMotor();
            motor.Registrar(estadoMotor, decisao, defeituosos, opcoes);
            entidade.AplicarEstadoMotor(estadoMotor, usuario);
            return (entidade, suspensaAntes);
        }
    }
}
