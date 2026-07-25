using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarProgramasSubsidioQuery(EEstadoProgramaSubsidio? Estado, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterProgramaSubsidioPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ConsultarSaldoProgramaQuery(Guid ProgramaSubsidioId) : IRequest<CommandResult>;
    public record ListarUtilizacoesProgramaQuery(Guid ProgramaSubsidioId) : IRequest<CommandResult>;

    public class SubsidiosFundosQueryHandlers :
        IRequestHandler<ListarProgramasSubsidioQuery, CommandResult>,
        IRequestHandler<ObterProgramaSubsidioPorIdQuery, CommandResult>,
        IRequestHandler<ConsultarSaldoProgramaQuery, CommandResult>,
        IRequestHandler<ListarUtilizacoesProgramaQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public SubsidiosFundosQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarProgramasSubsidioQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.ProgramasSubsidio.AsNoTracking().AsQueryable();
            if (request.Estado.HasValue) query = query.Where(p => p.Estado == request.Estado.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(p => p.Orgao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(p => new { p.Id, p.Orgao, p.ValorTotal, p.VigenciaInicio, p.VigenciaFim, p.Estado }).ToListAsync(ct);
            return CommandResult.Ok("Programas de subsídio listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterProgramaSubsidioPorIdQuery request, CancellationToken ct)
        {
            var p = await _context.ProgramasSubsidio.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            return p == null ? CommandResult.Falha("Programa de subsídio não encontrado.") : CommandResult.Ok("Programa encontrado.", p);
        }

        public async Task<CommandResult> Handle(ConsultarSaldoProgramaQuery request, CancellationToken ct)
        {
            var programa = await _context.ProgramasSubsidio.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.ProgramaSubsidioId, ct);
            if (programa == null) return CommandResult.Falha("Programa de subsídio não encontrado.");
            var utilizado = await _context.UtilizacoesSubsidio.AsNoTracking()
                .Where(u => u.ProgramaSubsidioId == request.ProgramaSubsidioId).SumAsync(u => (decimal?)u.ValorElegivel, ct) ?? 0m;
            return CommandResult.Ok("Saldo do programa consultado.", new { programa.Id, programa.ValorTotal, utilizado, saldo = programa.ValorTotal - utilizado });
        }

        public async Task<CommandResult> Handle(ListarUtilizacoesProgramaQuery request, CancellationToken ct)
        {
            var itens = await _context.UtilizacoesSubsidio.AsNoTracking().Where(u => u.ProgramaSubsidioId == request.ProgramaSubsidioId)
                .Select(u => new { u.Id, u.ProgramaSubsidioId, u.TituloPagarId, u.ValorElegivel, u.CriadoEm }).ToListAsync(ct);
            return CommandResult.Ok("Utilizações do programa listadas.", itens);
        }
    }
}
