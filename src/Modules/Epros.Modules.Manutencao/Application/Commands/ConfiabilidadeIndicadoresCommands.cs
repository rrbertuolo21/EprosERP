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
    /// <summary>
    /// MAN-CRV — D24: calcula MTTR/MTBF/Disponibilidade pelo MOTOR (lendo Paradas do equipamento no
    /// periodo) em vez de receber o valor pronto no POST. Persiste snapshot em man_crv_indicador com
    /// CalculadoPor=Sistema, formula e origem auditaveis. Substitui a entrada manual (fecha EF sec.9 x codigo).
    /// </summary>
    public record CalcularIndicadoresConfiabilidadeCommand(
        Guid RevisaoId,
        DateTime PeriodoInicio,
        DateTime PeriodoFim,
        Guid? EquipamentoId) : ICommand;

    public class CalcularIndicadoresConfiabilidadeCommandValidator : AbstractValidator<CalcularIndicadoresConfiabilidadeCommand>
    {
        public CalcularIndicadoresConfiabilidadeCommandValidator()
        {
            RuleFor(c => c.RevisaoId).NotEmpty();
            RuleFor(c => c.PeriodoFim).GreaterThan(c => c.PeriodoInicio);
        }
    }

    public class CalcularIndicadoresConfiabilidadeCommandHandler : ICommandHandler<CalcularIndicadoresConfiabilidadeCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CalcularIndicadoresConfiabilidadeCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CalcularIndicadoresConfiabilidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.PeriodoFim <= request.PeriodoInicio)
                return CommandResult.Falha("O fim do periodo deve ser posterior ao inicio.");

            var revisao = await _context.RevisoesConfiabilidade
                .FirstOrDefaultAsync(r => r.Id == request.RevisaoId, cancellationToken);
            if (revisao == null)
                return CommandResult.Falha("Revisao de confiabilidade nao encontrada.");

            var equipamentoId = request.EquipamentoId ?? revisao.AtivoId;
            if (equipamentoId == null || equipamentoId == Guid.Empty)
                return CommandResult.Falha("Informe o equipamento (ou vincule o ativo a revisao) para calcular os indicadores.");

            // Le as paradas do equipamento que tocam o periodo (fonte de verdade do calculo).
            var paradas = await _context.Paradas
                .Where(p => p.EquipamentoId == equipamentoId
                            && p.DataHoraInicio < request.PeriodoFim
                            && (p.DataHoraFim == null || p.DataHoraFim > request.PeriodoInicio))
                .ToListAsync(cancellationToken);

            var entradas = paradas.Select(p => new MotorIndicadoresConfiabilidade.ParadaCalculo(
                p.DataHoraInicio, p.DataHoraFim, p.TipoParada == ETipoParada.NaoPlanejada));

            var r = MotorIndicadoresConfiabilidade.Calcular(entradas, request.PeriodoInicio, request.PeriodoFim);

            var origem = $"man_par_parada (equip {equipamentoId}); {paradas.Count} parada(s); {r.QuantidadeFalhas} falha(s); calendario {r.TempoCalendarioMinutos:0.##} min; indisponibilidade {r.DowntimeTotalMinutos:0.##} min.";

            // Snapshot append: grava os indicadores calculados pelo Sistema diretamente (carregam o FK
            // RevisaoId). Substitui a entrada manual (D24). CalculadoPor=Sistema, formula/origem auditaveis.
            if (r.MttrHoras.HasValue)
                _context.IndicadoresConfiabilidade.Add(new IndicadorConfiabilidade(revisao.Id, ETipoIndicadorConfiabilidade.Mttr,
                    request.PeriodoInicio, request.PeriodoFim, r.MttrHoras.Value, "h",
                    MotorIndicadoresConfiabilidade.FormulaMttr, origem, ECalculadoPorConfiabilidade.Sistema, tenantId, usuario));

            if (r.MtbfHoras.HasValue)
                _context.IndicadoresConfiabilidade.Add(new IndicadorConfiabilidade(revisao.Id, ETipoIndicadorConfiabilidade.Mtbf,
                    request.PeriodoInicio, request.PeriodoFim, r.MtbfHoras.Value, "h",
                    MotorIndicadoresConfiabilidade.FormulaMtbf, origem, ECalculadoPorConfiabilidade.Sistema, tenantId, usuario));

            _context.IndicadoresConfiabilidade.Add(new IndicadorConfiabilidade(revisao.Id, ETipoIndicadorConfiabilidade.Disponibilidade,
                request.PeriodoInicio, request.PeriodoFim, r.DisponibilidadePercentual, "%",
                MotorIndicadoresConfiabilidade.FormulaDisponibilidade, origem, ECalculadoPorConfiabilidade.Sistema, tenantId, usuario));

            _context.HistoricosConfiabilidade.Add(new HistoricoConfiabilidade(revisao.Id, EAcaoHistoricoConfiabilidade.Parametrizado,
                revisao.ResponsavelId, null, "Indicadores calculados pelo motor (MTTR/MTBF/Disponibilidade).", "{}", tenantId, usuario));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Indicadores de confiabilidade calculados pelo motor.", new
            {
                revisao.Id,
                EquipamentoId = equipamentoId,
                Mttr = r.MttrHoras,
                Mtbf = r.MtbfHoras,
                Disponibilidade = r.DisponibilidadePercentual,
                r.QuantidadeFalhas,
                r.DowntimeTotalMinutos,
                r.TempoOperacionalMinutos
            });
        }
    }
}
