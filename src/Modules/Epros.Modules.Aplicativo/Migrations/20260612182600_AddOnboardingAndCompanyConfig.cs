using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingAndCompanyConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "forgot_password_token",
                schema: "aplicativo",
                table: "usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "forgot_password_token_expiry",
                schema: "aplicativo",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "anos_financeiros",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_anos_financeiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_empresas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    time_zone_id = table.Column<int>(type: "integer", nullable: false),
                    date_format = table.Column<string>(type: "text", nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    vat_percentage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_type = table.Column<int>(type: "integer", nullable: false),
                    currency_position = table.Column<int>(type: "integer", nullable: false),
                    footer_text = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "text", nullable: true),
                    favicon = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idiomas",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    country_code = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_idiomas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__ano_financeiro_sync_id",
                schema: "aplicativo",
                table: "anos_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ano_financeiro_tenant_id",
                schema: "aplicativo",
                table: "anos_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_empresa_sync_id",
                schema: "aplicativo",
                table: "configuracoes_empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_empresa_tenant_id",
                schema: "aplicativo",
                table: "configuracoes_empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_configuracoes_empresas_empresa",
                schema: "aplicativo",
                table: "configuracoes_empresas",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__idioma_sync_id",
                schema: "aplicativo",
                table: "idiomas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__idioma_tenant_id",
                schema: "aplicativo",
                table: "idiomas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_idiomas_tenant_code",
                schema: "aplicativo",
                table: "idiomas",
                columns: new[] { "tenant_id", "code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anos_financeiros",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "configuracoes_empresas",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "idiomas",
                schema: "aplicativo");

            migrationBuilder.DropColumn(
                name: "forgot_password_token",
                schema: "aplicativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "forgot_password_token_expiry",
                schema: "aplicativo",
                table: "usuarios");
        }
    }
}
