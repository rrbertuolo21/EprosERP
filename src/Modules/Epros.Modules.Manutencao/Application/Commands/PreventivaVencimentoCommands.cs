using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Domain.Services;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    /// <summary>
    /// MAN-PRV — D7/D8/D9: motor de vencimento sob demanda. Avalia os planos ATIVOS (ou um plano
    /// especifico) por calendario e/ou contador; ao vencer, cria UMA execucao programada por janela
    /// (deduplicacao RN-PRV-010/011) e avanca a base da periodicidade.
    /// T5 — cada execucao vencida CRIA a OT canonica (man_trb_ordem_servico, origem=Preventiva) como
    /// ordem interna e marca a execucao como OrdemGerada, referenciando a OS criada.
    /// </summary>
    public record AvaliarVencimentoPreventivaCommand(
        Guid? PlanoId,
        DateTime? DataReferencia,
        decimal? ContadorAtual) : ICommand;

    public class AvaliarVencimentoPreventivaCommandHandler : ICommandHandler<AvaliarVencimentoPreventivaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        private static readonly EStatusExecucaoPreventiva[] StatusAtivos =
        {
            EStatusExecucaoPreventiva.Prevista,
            EStatusExecucaoPreventiva.Elegivel,
            EStatusExecucaoPreventiva.OrdemGerada,
            EStatusExecucaoPreventiva.EmExecucao,
            EStatusExecucaoPreventiva.Atrasada
        };

        public AvaliarVencimentoPreventivaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AvaliarVencimentoPreventivaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var dataRef = request.DataReferencia ?? DateTime.UtcNow;

            var planosQuery = _context.PlanosPreventivos.Where(p => p.Status == EStatusRegistroManutencao.Ativo);
            if (request.PlanoId.HasValue)
                planosQuery = planosQuery.Where(p => p.Id == request.PlanoId.Value);

            var planoIds = await planosQuery.Select(p => p.Id).ToListAsync(cancellationToken);
            if (planoIds.Count == 0)
                return CommandResult.Ok("Nenhum plano preventivo ativo elegivel para avaliacao.", new { GeradasElegiveis = 0, Avaliadas = 0 });

            var periodicidades = await _context.PlanoPreventivoPeriodicidades
                .Where(pp => planoIds.Contains(pp.PlanoId) && pp.Situacao == ESituacaoPeriodicidade.Ativo)
                .ToListAsync(cancellationToken);

            var periodicidadeIds = periodicidades.Select(p => p.Id).ToList();
            var execucoes = await _context.PlanoPreventivoExecucoes
                .Where(e => periodicidadeIds.Contains(e.PeriodicidadeId))
                .ToListAsync(cancellationToken);

            int geradas = 0;
            var detalhes = new List<object>();

            foreach (var per in periodicidades)
            {
                var av = MotorVencimentoPreventiva.Avaliar(
                    per.TipoPeriodicidade, per.DataInicio, per.Intervalo, per.UnidadeIntervalo,
                    per.ProximaExecucao, per.Tolerancia, per.ContadorProximo, request.ContadorAtual, dataRef);

                if (!av.Vencido) continue;

                // D8 — deduplicacao: uma execucao por janela (mesma periodicidade + mesmo alvo ativo).
                var jaExiste = execucoes.Any(e =>
                    e.PeriodicidadeId == per.Id
                    && StatusAtivos.Contains(e.Status)
                    && DatasIguais(e.DataPrevista, av.DataAlvo)
                    && e.ContadorPrevisto == av.ContadorAlvo);

                if (jaExiste)
                {
                    detalhes.Add(new { PeriodicidadeId = per.Id, Resultado = "duplicada-ignorada", av.Detalhe });
                    continue;
                }

                var exec = new PlanoPreventivoExecucao(per.PlanoId, per.Id, av.DataAlvo, av.ContadorAlvo, null, tenantId, usuario);
                exec.MarcarElegivel(usuario);
                _context.PlanoPreventivoExecucoes.Add(exec);
                execucoes.Add(exec); // evita duplicar dentro do mesmo lote

                // T5 — a execucao elegivel passa a CRIAR a OT canonica (man_trb_ordem_servico)
                // como ordem interna (origem=Preventiva, sem cliente), e a execucao referencia essa OS.
                var os = OrdemServico.CriarInterna(
                    EOrigemOrdemServico.Preventiva, exec.Id, dataRef, tenantId, usuario,
                    observacaoAbertura: $"OS preventiva gerada por vencimento do plano {per.PlanoId} (execucao {exec.Id}).");
                _context.OrdensServico.Add(os);
                exec.RegistrarOrdemGerada(os.Id, usuario);

                // D9 — avanca a base para o proximo vencimento.
                per.AvancarVencimento(av.ProximaData, av.ProximoContador, usuario);

                geradas++;
                detalhes.Add(new { PeriodicidadeId = per.Id, ExecucaoId = exec.Id, OrdemServicoId = os.Id, Resultado = "ordem-gerada", av.Motivo, av.DataAlvo });
            }

            if (geradas > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                $"Avaliacao de vencimento concluida: {geradas} execucao(oes) elegivel(is) gerada(s).",
                new { GeradasElegiveis = geradas, Avaliadas = periodicidades.Count, Detalhes = detalhes });
        }

        private static bool DatasIguais(DateTime? a, DateTime? b)
        {
            if (!a.HasValue && !b.HasValue) return true;
            if (a.HasValue != b.HasValue) return false;
            return a!.Value.Date == b!.Value.Date;
        }
    }
}
