using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCTaxonomiaNormativaUnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "riscos_corporativos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "grc_reg_registro",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "grc_pol_politica",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "controles_internos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grc_taxonomia_normativa",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    catalogo_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_grc_taxonomia_normativa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_taxonomia_vinculo",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_tipo = table.Column<string>(type: "text", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino_tipo = table.Column<string>(type: "text", nullable: false),
                    destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    natureza = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_grc_taxonomia_vinculo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_taxonomia_normativa_catalogo_pai_id",
                schema: "grc",
                table: "grc_taxonomia_normativa",
                column: "catalogo_pai_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_taxonomia_normativa_tenant_id_codigo",
                schema: "grc",
                table: "grc_taxonomia_normativa",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_grc_taxonomia_normativa_tenant_id_tipo",
                schema: "grc",
                table: "grc_taxonomia_normativa",
                columns: new[] { "tenant_id", "tipo" });

            migrationBuilder.CreateIndex(
                name: "ix__taxonomia_normativa_sync_id",
                schema: "grc",
                table: "grc_taxonomia_normativa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__taxonomia_normativa_tenant_id",
                schema: "grc",
                table: "grc_taxonomia_normativa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_taxonomia_vinculo_destino_tipo_destino_id",
                schema: "grc",
                table: "grc_taxonomia_vinculo",
                columns: new[] { "destino_tipo", "destino_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_taxonomia_vinculo_origem_tipo_origem_id",
                schema: "grc",
                table: "grc_taxonomia_vinculo",
                columns: new[] { "origem_tipo", "origem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__taxonomia_vinculo_sync_id",
                schema: "grc",
                table: "grc_taxonomia_vinculo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__taxonomia_vinculo_tenant_id",
                schema: "grc",
                table: "grc_taxonomia_vinculo",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_taxonomia_normativa",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_taxonomia_vinculo",
                schema: "grc");

            migrationBuilder.DropColumn(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "riscos_corporativos");

            migrationBuilder.DropColumn(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "grc_reg_registro");

            migrationBuilder.DropColumn(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "grc_pol_politica");

            migrationBuilder.DropColumn(
                name: "taxonomia_normativa_id",
                schema: "grc",
                table: "controles_internos");
        }
    }
}
