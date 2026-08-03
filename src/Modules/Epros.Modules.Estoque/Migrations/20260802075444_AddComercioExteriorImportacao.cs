using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddComercioExteriorImportacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "incoterm",
                schema: "estoque",
                table: "compras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "moeda",
                schema: "estoque",
                table: "compras",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "taxa_cambio",
                schema: "estoque",
                table: "compras",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "com_importacao_rateio_config",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    incluir_tributos = table.Column<bool>(type: "boolean", nullable: false),
                    incluir_frete = table.Column<bool>(type: "boolean", nullable: false),
                    incluir_despesas = table.Column<bool>(type: "boolean", nullable: false),
                    metodo = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_com_importacao_rateio_config", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_com_importacao_rateio_config_tenant_id_empresa_id",
                schema: "estoque",
                table: "com_importacao_rateio_config",
                columns: new[] { "tenant_id", "empresa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compras_importacao_rateio_config_sync_id",
                schema: "estoque",
                table: "com_importacao_rateio_config",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__compras_importacao_rateio_config_tenant_id",
                schema: "estoque",
                table: "com_importacao_rateio_config",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "com_importacao_rateio_config",
                schema: "estoque");

            migrationBuilder.DropColumn(
                name: "incoterm",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "moeda",
                schema: "estoque",
                table: "compras");

            migrationBuilder.DropColumn(
                name: "taxa_cambio",
                schema: "estoque",
                table: "compras");
        }
    }
}
