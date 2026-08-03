using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <summary>
    /// 1.04 PASS 3 — Login social (OAuth 2.0 / OIDC, Google + Microsoft). Cria a entidade que
    /// faltava na 1.04: <c>identidades_externas</c>, que liga a conta social (par provedor + sub)
    /// à identidade global (<c>usuarios</c>). Índice único (provedor, subject_id) impede vínculo
    /// duplicado. Habilita leitura cross-tenant no callback social (política RLS
    /// <c>auth_cross_tenant_select</c>), pois o callback é anônimo quanto ao inquilino — mesmo
    /// bypass de SELECT já usado em <c>usuarios</c>/<c>empresas</c>/<c>acessos_usuario_tenant</c>.
    ///
    /// Observação: o RLS de isolamento por tenant (ENABLE/FORCE ROW LEVEL SECURITY +
    /// tenant_isolation_policy) é aplicado automaticamente pelo <c>EprosMigrationsSqlGenerator</c>
    /// a toda tabela criada com coluna <c>tenant_id</c> — por isso não é declarado aqui.
    /// </summary>
    public partial class Implanta_1_04_LoginSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identidades_externas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provedor = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email_provedor = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
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
                    table.PrimaryKey("p_k_identidades_externas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__identidade_externa_sync_id",
                schema: "aplicativo",
                table: "identidades_externas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__identidade_externa_tenant_id",
                schema: "aplicativo",
                table: "identidades_externas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_identidades_externas_provedor_subject",
                schema: "aplicativo",
                table: "identidades_externas",
                columns: new[] { "provedor", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identidades_externas_usuario",
                schema: "aplicativo",
                table: "identidades_externas",
                column: "usuario_id");

            // O callback social é anônimo quanto ao inquilino: para localizar a IdentidadeExterna pelo
            // par (provedor, sub) é preciso ler cross-tenant. Mesma política de bypass (só SELECT) usada
            // em usuarios/empresas/acessos_usuario_tenant, ativada por app.allow_cross_tenant_auth.
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS auth_cross_tenant_select ON aplicativo.identidades_externas;
                CREATE POLICY auth_cross_tenant_select ON aplicativo.identidades_externas
                  FOR SELECT
                  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A política auth_cross_tenant_select é removida junto com a tabela (DROP TABLE remove policies).
            migrationBuilder.DropTable(
                name: "identidades_externas",
                schema: "aplicativo");
        }
    }
}
