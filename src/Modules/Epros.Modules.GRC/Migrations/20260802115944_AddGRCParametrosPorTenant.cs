using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCParametrosPorTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grc_cia_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_cia_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_pol_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_pol_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_reg_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_reg_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_ris_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_sod_parametro", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_cia_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_parametro_sync_id",
                schema: "grc",
                table: "grc_cia_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_parametro_tenant_id",
                schema: "grc",
                table: "grc_cia_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_pol_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_parametro_sync_id",
                schema: "grc",
                table: "grc_pol_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_parametro_tenant_id",
                schema: "grc",
                table: "grc_pol_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_reg_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_reg_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compliance_parametro_sync_id",
                schema: "grc",
                table: "grc_reg_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compliance_parametro_tenant_id",
                schema: "grc",
                table: "grc_reg_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_ris_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__risco_parametro_sync_id",
                schema: "grc",
                table: "grc_ris_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__risco_parametro_tenant_id",
                schema: "grc",
                table: "grc_ris_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_sod_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__segregacao_parametro_sync_id",
                schema: "grc",
                table: "grc_sod_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__segregacao_parametro_tenant_id",
                schema: "grc",
                table: "grc_sod_parametro",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_cia_parametro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_pol_parametro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_reg_parametro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_parametro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_parametro",
                schema: "grc");

        }
    }
}
