using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Queries;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ListarProgramasDivQueryHandler : IQueryHandler<ListarProgramasDivQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarProgramasDivQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarProgramasDivQuery request, CancellationToken ct)
        {
            var dados = await _context.ProgramasDiv.OrderBy(p => p.Codigo).ToListAsync(ct);
            return CommandResult.Ok("Programas sociais listados.", dados);
        }
    }

    public class ListarIndicadoresDivQueryHandler : IQueryHandler<ListarIndicadoresDivQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarIndicadoresDivQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarIndicadoresDivQuery request, CancellationToken ct)
        {
            var dados = await _context.IndicadoresDiv.OrderBy(i => i.Codigo).ThenBy(i => i.Versao).ToListAsync(ct);
            return CommandResult.Ok("Indicadores sociais listados.", dados);
        }
    }

    public class ListarMedicoesDivQueryHandler : IQueryHandler<ListarMedicoesDivQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarMedicoesDivQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarMedicoesDivQuery request, CancellationToken ct)
        {
            // Retorna apenas o agregado; medicoes suprimidas nao expoem ValorAgregado (NF-09/T1).
            var dados = await _context.MedicoesDiv
                .Where(m => m.IndicadorId == request.IndicadorId)
                .OrderByDescending(m => m.PeriodoInicio)
                .Select(m => new
                {
                    m.Id, m.IndicadorId, m.Dimensao, m.PeriodoInicio, m.PeriodoFim,
                    m.Suprimido, ValorAgregado = m.Suprimido ? null : m.ValorAgregado, m.Origem
                })
                .ToListAsync(ct);
            return CommandResult.Ok("Medicoes sociais listadas.", dados);
        }
    }
}
