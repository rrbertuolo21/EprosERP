using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <summary>
    /// 1.04 PASS 2 — Acesso multi-tenant. Cria a membership N:N identidade global × tenant
    /// (<c>acessos_usuario_tenant</c>), habilita a leitura cross-tenant no login (política RLS
    /// <c>auth_cross_tenant_select</c>) e faz o backfill: cada usuário existente (single-tenant)
    /// ganha um vínculo com seu tenant de origem, para não quebrar o login atual.
    ///
    /// Observação: o RLS de isolamento por tenant (ENABLE/FORCE ROW LEVEL SECURITY +
    /// tenant_isolation_policy) é aplicado automaticamente pelo <c>EprosMigrationsSqlGenerator</c>
    /// a toda tabela criada com coluna <c>tenant_id</c> — por isso não é declarado aqui.
    /// </summary>
    public partial class Implanta_1_04_MultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acessos_usuario_tenant",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_acessos_usuario_tenant", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__acesso_usuario_tenant_sync_id",
                schema: "aplicativo",
                table: "acessos_usuario_tenant",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acesso_usuario_tenant_tenant_id",
                schema: "aplicativo",
                table: "acessos_usuario_tenant",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_acessos_usuario_tenant_usuario_tenant",
                schema: "aplicativo",
                table: "acessos_usuario_tenant",
                columns: new[] { "usuario_id", "tenant_id" },
                unique: true);

            // Login é anônimo quanto ao inquilino: para listar os tenants da identidade global é preciso
            // ler a membership cross-tenant. Mesma política de bypass (só SELECT) usada em usuarios/empresas.
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS auth_cross_tenant_select ON aplicativo.acessos_usuario_tenant;
                CREATE POLICY auth_cross_tenant_select ON aplicativo.acessos_usuario_tenant
                  FOR SELECT
                  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');
                """);

            // Backfill (data migration): todo usuário existente recebe um vínculo (Proprietario=1, ativo)
            // com seu tenant de origem. Idempotente. O loop por tenant satisfaz o WITH CHECK do
            // tenant_isolation_policy (INSERT exige tenant_id = app.current_tenant_id) mesmo quando a
            // migration roda sob um papel que NÃO faz bypass de RLS; a enumeração dos tenants usa o
            // bypass de SELECT (app.allow_cross_tenant_auth) já existente em aplicativo.usuarios.
            migrationBuilder.Sql("""
                SELECT set_config('app.allow_cross_tenant_auth', 'true', true);
                DO $$
                DECLARE r RECORD;
                BEGIN
                  FOR r IN SELECT DISTINCT u.tenant_id AS tid FROM aplicativo.usuarios u WHERE u.deletado_em IS NULL LOOP
                    PERFORM set_config('app.current_tenant_id', r.tid, true);
                    INSERT INTO aplicativo.acessos_usuario_tenant
                        (id, sync_id, tenant_id, usuario_id, papel, ativo, sync_version, criado_em, criado_por)
                    SELECT gen_random_uuid(), gen_random_uuid(), u.tenant_id, u.id, 1, true, 0, now(), 'data-migration-1.04'
                    FROM aplicativo.usuarios u
                    WHERE u.tenant_id = r.tid AND u.deletado_em IS NULL
                      AND NOT EXISTS (
                        SELECT 1 FROM aplicativo.acessos_usuario_tenant a
                        WHERE a.usuario_id = u.id AND a.tenant_id = u.tenant_id
                      );
                  END LOOP;
                END $$;
                SELECT set_config('app.allow_cross_tenant_auth', 'false', true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A política auth_cross_tenant_select é removida junto com a tabela (DROP TABLE remove policies).
            migrationBuilder.DropTable(
                name: "acessos_usuario_tenant",
                schema: "aplicativo");
        }
    }
}
