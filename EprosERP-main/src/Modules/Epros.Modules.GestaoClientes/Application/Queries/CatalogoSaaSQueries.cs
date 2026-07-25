using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarFuncionalidadesQuery() : IQuery<CommandResult>;
    public record ListarAddOnsQuery(bool ApenasHabilitados = false, bool ApenasNaoAdmin = false) : IQuery<CommandResult>;

    /// <summary>APP-CAT 7.4: resolve o conjunto final de módulos autorizados para um usuário/contexto.</summary>
    public record ResolverModulosAtivosQuery(Guid UsuarioId) : IQuery<CommandResult>;

    public class ListarFuncionalidadesQueryHandler : IQueryHandler<ListarFuncionalidadesQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        public ListarFuncionalidadesQueryHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(ListarFuncionalidadesQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.Funcionalidades
                .OrderBy(f => f.Title)
                .Select(f => new { f.Id, f.Title, f.Description })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Funcionalidades listadas com sucesso.", itens);
        }
    }

    public class ListarAddOnsQueryHandler : IQueryHandler<ListarAddOnsQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        public ListarAddOnsQueryHandler(ContextGestaoClientes context) { _context = context; }

        public async Task<CommandResult> Handle(ListarAddOnsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AddOns.AsQueryable();
            if (request.ApenasHabilitados) query = query.Where(a => a.Habilitado);
            if (request.ApenasNaoAdmin) query = query.Where(a => !a.Admin);

            var itens = await query
                .OrderBy(a => a.NomeModulo)
                .Select(a => new { a.Id, a.NomeModulo, a.Alias, a.PrecoMensal, a.PrecoAnual, a.Habilitado, a.Admin, a.ParentAddOnId })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Add-ons listados com sucesso.", itens);
        }
    }

    public class ResolverModulosAtivosQueryHandler : IQueryHandler<ResolverModulosAtivosQuery, CommandResult>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        public ResolverModulosAtivosQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ResolverModulosAtivosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Catálogo habilitado (REG-012): apenas módulos tecnicamente habilitados entram no resultado.
            var habilitados = await _context.AddOns
                .Where(a => a.Habilitado)
                .Select(a => a.NomeModulo.ToLower())
                .ToListAsync(cancellationToken);
            var habilitadosSet = habilitados.ToHashSet();

            var resolvidos = new HashSet<string>();

            // 1) Baseline (REG-015): config global opcional com lista separada por vírgula.
            var baselineConfig = await _context.ConfiguracoesGlobais
                .FirstOrDefaultAsync(c => c.Chave == "catalogo.baseline_modulos" && c.TenantId == tenantId, cancellationToken);
            if (baselineConfig != null && !string.IsNullOrWhiteSpace(baselineConfig.Valor))
            {
                foreach (var m in baselineConfig.Valor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    resolvidos.Add(m.ToLower());
            }

            // 2) Módulos do plano do tenant (REG-015).
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);
            if (cliente != null)
            {
                var plano = await _context.Planos.Include(p => p.Modulos).FirstOrDefaultAsync(p => p.Id == cliente.PlanoId, cancellationToken);
                if (plano != null)
                {
                    foreach (var m in plano.Modulos)
                        resolvidos.Add(m.NomeModulo.ToLower());
                }
            }

            // 3) Módulos avulsos ativos do usuário (REG-015/017).
            var avulsos = await _context.ModulosAtivosUsuario
                .Where(m => m.TenantId == tenantId && m.UsuarioId == request.UsuarioId)
                .Select(m => m.Modulo.ToLower())
                .ToListAsync(cancellationToken);
            foreach (var m in avulsos) resolvidos.Add(m);

            // Interseção com catálogo habilitado, sem duplicidade (REG-012/017).
            var final = resolvidos.Where(m => habilitadosSet.Contains(m)).OrderBy(m => m).ToList();

            return CommandResult.Ok("Módulos ativos resolvidos com sucesso.", new { Modulos = final });
        }
    }
}
