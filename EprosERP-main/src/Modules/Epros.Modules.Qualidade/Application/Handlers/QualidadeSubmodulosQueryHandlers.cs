using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    public class ListarNcrsQueryHandler : IQueryHandler<ListarNcrsQuery, CommandResult>
    {
        private readonly ContextQualidade _context;
        public ListarNcrsQueryHandler(ContextQualidade context) => _context = context;

        public async Task<CommandResult> Handle(ListarNcrsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.NcrRegistros.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EStatusRegistroQualidade>(request.Status, true, out var st))
                query = query.Where(n => n.StatusRegistro == st);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(n => n.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("NCRs listadas com sucesso!", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ListarPlanosInspecaoQueryHandler : IQueryHandler<ListarPlanosInspecaoQuery, CommandResult>
    {
        private readonly ContextQualidade _context;
        public ListarPlanosInspecaoQueryHandler(ContextQualidade context) => _context = context;

        public async Task<CommandResult> Handle(ListarPlanosInspecaoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PlanosInspecao.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EStatusRegistroQualidade>(request.Status, true, out var st))
                query = query.Where(p => p.Status == st);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Planos de inspecao listados com sucesso!", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ListarAnalisesAcrQueryHandler : IQueryHandler<ListarAnalisesAcrQuery, CommandResult>
    {
        private readonly ContextQualidade _context;
        public ListarAnalisesAcrQueryHandler(ContextQualidade context) => _context = context;

        public async Task<CommandResult> Handle(ListarAnalisesAcrQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AcrAnalises.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EStatusRegistroQualidade>(request.Status, true, out var st))
                query = query.Where(a => a.Status == st);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(a => a.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Analises de aceite/rejeicao listadas com sucesso!", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ListarRegistrosAdmQueryHandler : IQueryHandler<ListarRegistrosAdmQuery, CommandResult>
    {
        private readonly ContextQualidade _context;
        public ListarRegistrosAdmQueryHandler(ContextQualidade context) => _context = context;

        public async Task<CommandResult> Handle(ListarRegistrosAdmQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AdmQualidades.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EStatusRegistroQualidade>(request.Status, true, out var st))
                query = query.Where(a => a.Status == st);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(a => a.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Registros de administracao da qualidade listados com sucesso!", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ListarAtributosQueryHandler : IQueryHandler<ListarAtributosQuery, CommandResult>
    {
        private readonly ContextQualidade _context;
        public ListarAtributosQueryHandler(ContextQualidade context) => _context = context;

        public async Task<CommandResult> Handle(ListarAtributosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AtrAtributos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EStatusRegistroQualidade>(request.Status, true, out var st))
                query = query.Where(a => a.Status == st);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(a => a.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Atributos listados com sucesso!", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }
}
