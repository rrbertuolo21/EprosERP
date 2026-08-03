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
    /// MAN-PAR — D16: calcula MTTR/MTBF/Disponibilidade pelo motor unico (mesmo de CRV) lendo as
    /// paradas do equipamento no periodo e persiste snapshot em man_par_indicador (nao entrada manual).
    /// A parada informada e a ancora do snapshot; o equipamento vem dela (ou explicito).
    /// </summary>
    public record CalcularIndicadoresParadaCommand(
        Guid ParadaId,
        DateTime PeriodoInicio,
        DateTime PeriodoFim,
        Guid? EquipamentoId) : ICommand;

    public class CalcularIndicadoresParadaCommandValidator : AbstractValidator<CalcularIndicadoresParadaCommand>
    {
        public CalcularIndicadoresParadaCommandValidator()
        {
            RuleFor(c => c.ParadaId).NotEmpty();
            RuleFor(c => c.PeriodoFim).GreaterThan(c => c.PeriodoInicio);
        }
    }

    public class CalcularIndicadoresParadaCommandHandler : ICommandHandler<CalcularIndicadoresParadaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CalcularIndicadoresParadaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CalcularIndicadoresParadaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.PeriodoFim <= request.PeriodoInicio)
                return CommandResult.Falha("O fim do periodo deve ser posterior ao inicio.");

            var parada = await _context.Paradas
                .FirstOrDefaultAsync(p => p.Id == request.ParadaId, cancellationToken);
            if (parada == null)
                return CommandResult.Falha("Parada nao encontrada.");

            var equipamentoId = request.EquipamentoId ?? parada.EquipamentoId;
            if (equipamentoId == null || equipamentoId == Guid.Empty)
                return CommandResult.Falha("Informe o equipamento (a parada nao tem equipamento vinculado).");

            var paradas = await _context.Paradas
                .Where(p => p.EquipamentoId == equipamentoId
                            && p.DataHoraInicio < request.PeriodoFim
                            && (p.DataHoraFim == null || p.DataHoraFim > request.PeriodoInicio))
                .ToListAsync(cancellationToken);

            var entradas = paradas.Select(p => new MotorIndicadoresConfiabilidade.ParadaCalculo(
                p.DataHoraInicio, p.DataHoraFim, p.TipoParada == ETipoParada.NaoPlanejada));

            var r = MotorIndicadoresConfiabilidade.Calcular(entradas, request.PeriodoInicio, request.PeriodoFim);

            var origem = $"man_par_parada (equip {equipamentoId}); {paradas.Count} parada(s); {r.QuantidadeFalhas} falha(s).";

            // Snapshot append: grava os indicadores diretamente (carregam o FK ParadaId); nao suja o
            // agregado raiz. Substitui a entrada manual (D16).
            if (r.MttrHoras.HasValue)
                _context.ParadaIndicadores.Add(new ParadaIndicador(parada.Id, ETipoIndicadorParada.MTTR, r.MttrHoras.Value, "h",
                    MotorIndicadoresConfiabilidade.FormulaMttr, request.PeriodoInicio, request.PeriodoFim, tenantId, usuario));

            if (r.MtbfHoras.HasValue)
                _context.ParadaIndicadores.Add(new ParadaIndicador(parada.Id, ETipoIndicadorParada.MTBF, r.MtbfHoras.Value, "h",
                    MotorIndicadoresConfiabilidade.FormulaMtbf, request.PeriodoInicio, request.PeriodoFim, tenantId, usuario));

            _context.ParadaIndicadores.Add(new ParadaIndicador(parada.Id, ETipoIndicadorParada.Disponibilidade, r.DisponibilidadePercentual, "%",
                MotorIndicadoresConfiabilidade.FormulaDisponibilidade, request.PeriodoInicio, request.PeriodoFim, tenantId, usuario));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Indicadores de parada calculados pelo motor.", new
            {
                parada.Id,
                EquipamentoId = equipamentoId,
                Mttr = r.MttrHoras,
                Mtbf = r.MtbfHoras,
                Disponibilidade = r.DisponibilidadePercentual,
                r.QuantidadeFalhas
            });
        }
    }
}
