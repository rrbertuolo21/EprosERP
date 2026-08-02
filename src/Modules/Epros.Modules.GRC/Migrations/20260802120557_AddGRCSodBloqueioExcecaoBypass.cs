using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCSodBloqueioExcecaoBypass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "modo_tratamento",
                schema: "grc",
                table: "grc_sod_regra",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "controle_compensatorio_id",
                schema: "grc",
                table: "grc_sod_excecao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "renovacoes",
                schema: "grc",
                table: "grc_sod_excecao",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitante_id",
                schema: "grc",
                table: "grc_sod_excecao",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grc_sod_bypass_admin",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    regra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ator_eh_admin = table.Column<bool>(type: "boolean", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    controle_compensatorio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    controle_compensatorio = table.Column<string>(type: "text", nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("p_k_grc_sod_bypass_admin", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_bypass_admin_ator_id",
                schema: "grc",
                table: "grc_sod_bypass_admin",
                column: "ator_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_bypass_admin_tenant_id_regra_id",
                schema: "grc",
                table: "grc_sod_bypass_admin",
                columns: new[] { "tenant_id", "regra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__bypass_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_bypass_admin",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__bypass_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_bypass_admin",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_sod_bypass_admin",
                schema: "grc");

            migrationBuilder.DropColumn(
                name: "modo_tratamento",
                schema: "grc",
                table: "grc_sod_regra");

            migrationBuilder.DropColumn(
                name: "controle_compensatorio_id",
                schema: "grc",
                table: "grc_sod_excecao");

            migrationBuilder.DropColumn(
                name: "renovacoes",
                schema: "grc",
                table: "grc_sod_excecao");

            migrationBuilder.DropColumn(
                name: "solicitante_id",
                schema: "grc",
                table: "grc_sod_excecao");
        }
    }
}
