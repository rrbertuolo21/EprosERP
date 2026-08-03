using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Epros.Modules.Aplicativo.Infrastructure.Data;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ContextAplicativo))]
    [Migration("20260708193000_AddAuthCrossTenantRlsPolicies")]
    public partial class AddAuthCrossTenantRlsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS auth_cross_tenant_select ON aplicativo.usuarios;
                CREATE POLICY auth_cross_tenant_select ON aplicativo.usuarios
                  FOR SELECT
                  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');
                """);

            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS auth_cross_tenant_select ON plataforma.empresas;
                CREATE POLICY auth_cross_tenant_select ON plataforma.empresas
                  FOR SELECT
                  USING (current_setting('app.allow_cross_tenant_auth', true) = 'true');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP POLICY IF EXISTS auth_cross_tenant_select ON aplicativo.usuarios;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS auth_cross_tenant_select ON plataforma.empresas;");
        }
    }
}
