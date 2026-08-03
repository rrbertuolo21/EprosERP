using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Epros.Infrastructure.Data
{
    /// <summary>
    /// Barreira de boot contra "entidade órfã" (REG-001, isolamento de dados).
    ///
    /// Toda entidade PERSISTENTE precisa ter uma decisão explícita de fronteira de tenant. São
    /// aceitas quatro formas de classificação:
    ///   1. herda <see cref="EntidadeSaaSBase"/> (tenantizada — coluna tenant_id + QueryFilter + RLS);
    ///   2. implementa <see cref="IGlobalEntity"/> (catálogo global, opt-out consciente do filtro);
    ///   3. possui a própria coluna <c>tenant_id</c> (ex.: <c>OutboxMessage</c> — RLS no banco);
    ///   4. é um read-model que aponta para uma TABELA de outro módulo já tenantizada (os
    ///      <c>*Lookup</c> read-only cross-módulo: a leitura passa pela RLS da tabela dona).
    ///
    /// Uma entidade que não se enquadre em nenhuma delas ficaria sem tenant_id, sem filtro e sem
    /// RLS — vazando silenciosamente entre tenants. Antes isso era só CONVENÇÃO; aqui vira BARREIRA:
    /// o startup FALHA, forçando a decisão no momento em que a entidade é introduzida (não em produção).
    /// </summary>
    public static class GuardaEntidadeOrfa
    {
        private const string ColunaTenant = "tenant_id";
        private const string PropriedadeTenant = "TenantId";

        private const string ProviderInMemory = "Microsoft.EntityFrameworkCore.InMemory";

        public static void ValidarModelos(IEnumerable<DbContext> contextos)
        {
            // A barreira valida o modelo RELACIONAL (onde tenant_id, RLS e schema existem de fato). O
            // provider InMemory (usado só por alguns testes) não carrega metadados relacionais de
            // forma consistente, então esses contextos são ignorados aqui — a validação plena roda no
            // boot real (Npgsql) e num teste dedicado sobre os contextos Npgsql.
            var listaContextos = contextos
                .Where(c => c.Database.ProviderName != ProviderInMemory)
                .ToList();

            // 1º passo: mapa de TODAS as tabelas que já têm fronteira de tenant no banco — donas
            // (EntidadeSaaSBase) ou qualquer entidade com coluna tenant_id. Um read-model que aponte
            // para uma dessas tabelas está protegido pela RLS da tabela dona.
            var tabelasTenantizadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var contexto in listaContextos)
            {
                foreach (var entityType in contexto.Model.GetEntityTypes())
                {
                    var clr = entityType.ClrType;
                    var temFronteira = clr != null
                        && (typeof(EntidadeSaaSBase).IsAssignableFrom(clr) || TemColunaTenant(entityType));
                    if (temFronteira)
                    {
                        var chave = ChaveTabela(entityType);
                        if (chave != null) tabelasTenantizadas.Add(chave);
                    }
                }
            }

            // 2º passo: procura entidades persistentes sem nenhuma classificação.
            var violacoes = new List<string>();
            foreach (var contexto in listaContextos)
            {
                var nomeContexto = contexto.GetType().Name;

                foreach (var entityType in contexto.Model.GetEntityTypes())
                {
                    // Não são "entidade persistente própria" e não precisam de tenant:
                    if (entityType.IsOwned()) continue;             // parte da tabela do dono
                    if (entityType.HasSharedClrType) continue;      // junção implícita (Dictionary)
                    if (entityType.FindPrimaryKey() == null) continue; // keyless (view/query type)

                    var clr = entityType.ClrType;
                    if (clr == null) continue;

                    if (typeof(EntidadeSaaSBase).IsAssignableFrom(clr)) continue; // (1)
                    if (typeof(IGlobalEntity).IsAssignableFrom(clr)) continue;    // (2)
                    if (TemColunaTenant(entityType)) continue;                    // (3)

                    var chave = ChaveTabela(entityType);
                    if (chave != null && tabelasTenantizadas.Contains(chave)) continue; // (4) read-model

                    var tabela = entityType.GetTableName() ?? clr.Name;
                    violacoes.Add($"{nomeContexto}: {clr.FullName} (tabela '{tabela}')");
                }
            }

            if (violacoes.Count > 0)
            {
                var lista = string.Join("\n  - ", violacoes.Distinct());
                throw new InvalidOperationException(
                    "Barreira de isolamento (REG-001): entidade(s) persistente(s) sem classificação de tenant — " +
                    "não herdam EntidadeSaaSBase, não implementam IGlobalEntity, não têm coluna tenant_id e " +
                    "não são read-model de tabela tenantizada:\n  - " + lista +
                    "\nClassifique cada uma: herde EntidadeSaaSBase (tenantizada) ou marque IGlobalEntity (catálogo global consciente).");
            }
        }

        private static bool TemColunaTenant(IEntityType entityType)
        {
            if (entityType.FindProperty(PropriedadeTenant) != null) return true;
            return entityType.GetProperties().Any(p =>
                string.Equals(p.Name, PropriedadeTenant, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.GetColumnName(), ColunaTenant, StringComparison.OrdinalIgnoreCase));
        }

        // Chave por NOME DE TABELA (sem schema). Read-models cross-módulo (os *Lookup) apontam para
        // a MESMA tabela física do módulo dono, mas o schema pode ser declarado de formas diferentes
        // em cada contexto; casar só pelo nome da tabela é robusto e suficiente (duas tabelas de nome
        // igual em schemas distintos seriam ambas tenantizadas de qualquer forma).
        private static string? ChaveTabela(IEntityType entityType)
        {
            var tabela = entityType.GetTableName();
            return string.IsNullOrEmpty(tabela) ? null : tabela;
        }
    }
}
