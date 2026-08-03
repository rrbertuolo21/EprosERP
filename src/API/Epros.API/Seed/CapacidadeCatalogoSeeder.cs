using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Epros.API.Seed
{
    /// <summary>
    /// 1.09 (USUARIOS_E_PAPEIS) — Semeador AUTORITATIVO do catálogo de permissões RBAC.
    ///
    /// Descobre, por reflexão, todos os pares <c>Recurso×Ação</c> declarados nos controllers/actions via
    /// <see cref="AbacAuthorizeAttribute"/> e os materializa como <see cref="Capacidade"/> do tenant de
    /// sistema ("system"). Esse catálogo passa a ser a FONTE da verdade das permissões que o
    /// <c>AbacFilter</c> cobra (antes o filtro lia strings livres de <c>PerfilColaborador.Permissoes</c>,
    /// nunca semeadas — LC-2). Também cria/mantém o papel de sistema <b>Administrador</b> ligado a TODAS
    /// as capacidades (conserta o LC-1: o admin do self-register recebe esse papel e deixa de ser barrado).
    ///
    /// Idempotente: cada capacidade é chaveada por Nome ("{recurso}:{acao}") no tenant "system"; rodar o
    /// boot N vezes não duplica. Convenção de nome idêntica à cobrada pelo AbacFilter.
    /// </summary>
    public static class CapacidadeCatalogoSeeder
    {
        /// <summary>Tenant do catálogo global de permissões (mesma marca "system" do landlord).</summary>
        public const string TenantSistema = "system";

        /// <summary>Nome canônico do papel de sistema com todas as capacidades.</summary>
        public const string PapelAdministradorNome = "Administrador";

        private const string CriadoPor = "seed-rbac-1.09";

        /// <summary>
        /// Nome canônico de uma capacidade a partir de (recurso, ação). É a MESMA chave que o
        /// <c>AbacFilter</c> monta ao autorizar, para casar o requisito do atributo com o catálogo.
        /// </summary>
        public static string NomeCapacidade(string recurso, string acao)
            => $"{recurso}:{acao}".ToLowerInvariant();

        /// <summary>
        /// Descobre os pares (recurso, ação) de todos os <see cref="AbacAuthorizeAttribute"/> aplicados
        /// em classes/métodos de controllers da assembly informada (default: a própria API).
        /// </summary>
        public static IReadOnlyCollection<(string Recurso, string Acao)> DescobrirCapacidades(Assembly? assembly = null)
        {
            assembly ??= typeof(CapacidadeCatalogoSeeder).Assembly;
            var pares = new HashSet<(string, string)>();

            foreach (var tipo in assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract))
            {
                // Atributos de nível de classe.
                foreach (var attr in tipo.GetCustomAttributes<AbacAuthorizeAttribute>(inherit: true))
                {
                    if (!string.IsNullOrWhiteSpace(attr.Recurso) && !string.IsNullOrWhiteSpace(attr.Acao))
                        pares.Add((attr.Recurso, attr.Acao));
                }

                // Atributos de nível de método (actions).
                foreach (var metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    foreach (var attr in metodo.GetCustomAttributes<AbacAuthorizeAttribute>(inherit: true))
                    {
                        if (!string.IsNullOrWhiteSpace(attr.Recurso) && !string.IsNullOrWhiteSpace(attr.Acao))
                            pares.Add((attr.Recurso, attr.Acao));
                    }
                }
            }

            return pares.ToList();
        }

        /// <summary>
        /// Semeia o catálogo de capacidades (system) e o papel Administrador (system) com todas as
        /// capacidades. Idempotente. Retorna o Id do papel Administrador.
        /// </summary>
        public static async Task<Guid> SeedAsync(ContextGestaoClientes ctx, Assembly? controllersAssembly = null, CancellationToken ct = default)
        {
            var descobertas = DescobrirCapacidades(controllersAssembly);

            // 1) Capacidades existentes (system) por nome.
            var existentes = await ctx.Capacidades
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == TenantSistema && c.DeletadoEm == null)
                .ToListAsync(ct);

            var porNome = existentes.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            var novas = new List<Capacidade>();
            foreach (var (recurso, acao) in descobertas)
            {
                var nome = NomeCapacidade(recurso, acao);
                if (porNome.ContainsKey(nome)) continue;

                var cap = new Capacidade(
                    name: nome,
                    label: $"{recurso} — {acao}",
                    module: recurso,
                    addOn: null,
                    permissionKey: nome,
                    tenantId: TenantSistema,
                    criadoPor: CriadoPor);

                if (cap.IsValid)
                {
                    novas.Add(cap);
                    porNome[nome] = cap;
                }
                else
                {
                    Log.Warning("[SeedRbac] Capacidade inválida ({Nome}): {Erros}", nome,
                        string.Join("; ", cap.Notifications.Select(n => n.Message)));
                }
            }

            if (novas.Count > 0)
            {
                ctx.Capacidades.AddRange(novas);
                await ctx.SaveChangesAsync(ct);
                Log.Information("[SeedRbac] {Qtd} capacidade(s) nova(s) semeada(s) no catálogo do sistema.", novas.Count);
            }

            // 2) Papel de sistema Administrador (system), protegido e não editável.
            var admin = await ctx.Papeis
                .IgnoreQueryFilters()
                .Include(p => p.Capacidades)
                .FirstOrDefaultAsync(p => p.TenantId == TenantSistema && p.Name == PapelAdministradorNome && p.DeletadoEm == null, ct);

            if (admin == null)
            {
                admin = new Papel(
                    name: PapelAdministradorNome,
                    label: "Administrador (sistema)",
                    guardName: null,
                    editable: false,
                    roleSystem: true,
                    roleType: null,
                    roleHomepage: null,
                    modules: null,
                    tenantId: TenantSistema,
                    criadoPor: CriadoPor);

                if (!admin.IsValid)
                {
                    Log.Warning("[SeedRbac] Papel Administrador inválido: {Erros}",
                        string.Join("; ", admin.Notifications.Select(n => n.Message)));
                    return Guid.Empty;
                }

                ctx.Papeis.Add(admin);
                await ctx.SaveChangesAsync(ct);
            }

            // 3) Liga o Administrador a TODAS as capacidades do catálogo (só as que faltam — idempotente).
            var todasCaps = await ctx.Capacidades
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == TenantSistema && c.DeletadoEm == null)
                .Select(c => c.Id)
                .ToListAsync(ct);

            var jaLigadas = await ctx.PapeisCapacidades
                .IgnoreQueryFilters()
                .Where(pc => pc.PapelId == admin.Id && pc.DeletadoEm == null)
                .Select(pc => pc.CapacidadeId)
                .ToListAsync(ct);

            var faltantes = todasCaps.Except(jaLigadas).ToList();
            if (faltantes.Count > 0)
            {
                var links = faltantes.Select(cid => new PapelCapacidade(admin.Id, cid, TenantSistema, CriadoPor)).ToList();
                ctx.PapeisCapacidades.AddRange(links);
                await ctx.SaveChangesAsync(ct);
                Log.Information("[SeedRbac] Papel Administrador ligado a {Qtd} capacidade(s) adicional(is).", faltantes.Count);
            }

            return admin.Id;
        }

        /// <summary>
        /// Resolve o Id do papel de sistema Administrador (para atribuir a um usuário admin). Não cria —
        /// retorna null se o catálogo ainda não foi semeado (chamador decide o fallback).
        /// </summary>
        public static async Task<Guid?> ObterPapelAdministradorIdAsync(ContextGestaoClientes ctx, CancellationToken ct = default)
        {
            var id = await ctx.Papeis
                .IgnoreQueryFilters()
                .Where(p => p.TenantId == TenantSistema && p.Name == PapelAdministradorNome && p.DeletadoEm == null)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);
            return id;
        }
    }
}
