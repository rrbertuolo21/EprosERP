using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPapeisQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterPapelPorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ListarCapacidadesQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ListarNiveisUsuarioQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarPapeisQueryHandler : IQueryHandler<ListarPapeisQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPapeisQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarPapeisQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            var query = _context.Papeis.Where(p => p.TenantId == tenantId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.Name)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(p => new { p.Id, p.Name, p.Label, p.Editable, p.RoleSystem, CountCapacidades = p.Capacidades.Count })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Papéis listados com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }

    public class ObterPapelPorIdQueryHandler : IQueryHandler<ObterPapelPorIdQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterPapelPorIdQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterPapelPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var papel = await _context.Papeis.Include(p => p.Capacidades)
                .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken);
            if (papel == null)
                return CommandResult.Falha(new[] { "Papel não encontrado." });

            var dto = new
            {
                papel.Id,
                papel.Name,
                papel.Label,
                papel.GuardName,
                papel.Editable,
                papel.RoleSystem,
                RoleType = papel.RoleType.HasValue ? papel.RoleType.Value.ToString() : null,
                papel.RoleHomepage,
                papel.Modules,
                Capacidades = papel.Capacidades.Select(c => c.CapacidadeId).ToList()
            };
            return CommandResult.Ok("Papel obtido com sucesso.", dto);
        }
    }

    public class ListarCapacidadesQueryHandler : IQueryHandler<ListarCapacidadesQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCapacidadesQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCapacidadesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            var query = _context.Capacidades.Where(c => c.TenantId == tenantId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.Name)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(c => new { c.Id, c.Name, c.Label, c.Module, c.AddOn, c.PermissionKey })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Capacidades listadas com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }

    public class ListarNiveisUsuarioQueryHandler : IQueryHandler<ListarNiveisUsuarioQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarNiveisUsuarioQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarNiveisUsuarioQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            var query = _context.NiveisUsuario.Where(n => n.TenantId == tenantId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(n => n.LevelId)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(n => new { n.Id, n.LevelId, n.Label, LevelType = n.LevelType.ToString(), n.MaxStorageBytes })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Níveis de usuário listados com sucesso.", new { Total = total, Pagina = pagina, Itens = itens });
        }
    }
}
