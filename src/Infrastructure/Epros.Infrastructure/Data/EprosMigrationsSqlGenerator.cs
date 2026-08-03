using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Epros.Infrastructure.Data
{
    public class EprosMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        public EprosMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            INpgsqlSingletonOptions npgsqlSingletonOptions)
            : base(dependencies, npgsqlSingletonOptions)
        {
        }

        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            base.Generate(operation, model, builder, terminate);

            // Verifica se a tabela possui a coluna "tenant_id"
            var hasTenantId = operation.Columns.Any(c => c.Name.Equals("tenant_id", StringComparison.OrdinalIgnoreCase));

            if (hasTenantId)
            {
                var tableName = operation.Name;
                var schemaName = operation.Schema;

                // Não habilitar RLS para tabelas de cadastro global
                if (tableName.Equals("paises", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("subdivisoes", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("municipios", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("formatos_codigo_postal", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("codigos_postais_cache", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("fusos_horarios", StringComparison.OrdinalIgnoreCase) ||
                    tableName.Equals("moedas", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var fullTableName = string.IsNullOrEmpty(schemaName) ? $"\"{tableName}\"" : $"\"{schemaName}\".\"{tableName}\"";

                // 1. Ativa Row-Level Security
                builder.AppendLine($"ALTER TABLE {fullTableName} ENABLE ROW LEVEL SECURITY;");
                builder.AppendLine($"ALTER TABLE {fullTableName} FORCE ROW LEVEL SECURITY;");

                // 2. Cria a política de isolamento de tenant baseada em session variable.
                //    USING protege a LEITURA (SELECT/UPDATE/DELETE enxergam só o tenant corrente);
                //    WITH CHECK explícito protege a ESCRITA (INSERT/UPDATE não podem gravar linha de
                //    outro tenant). O Postgres reaproveitaria o USING como WITH CHECK implicitamente,
                //    mas declarar explicitamente documenta a intenção e evita regressão silenciosa se
                //    a policy for futuramente restringida a FOR SELECT.
                builder.AppendLine($"CREATE POLICY tenant_isolation_policy ON {fullTableName} USING (tenant_id = current_setting('app.current_tenant_id', true)) WITH CHECK (tenant_id = current_setting('app.current_tenant_id', true));");

                builder.EndCommand();
            }
        }
    }
}
