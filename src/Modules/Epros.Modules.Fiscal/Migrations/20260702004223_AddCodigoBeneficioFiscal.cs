using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoBeneficioFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "codigos_beneficios_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uf = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_codigos_beneficios_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "codigos_beneficios_fiscais_csosn",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_beneficio_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    csosn = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_codigos_beneficios_fiscais_csosn", x => x.id);
                    table.ForeignKey(
                        name: "f_k_codigos_beneficios_fiscais_csosn_codigos_beneficios_fiscais~",
                        column: x => x.codigo_beneficio_fiscal_id,
                        principalSchema: "plataforma",
                        principalTable: "codigos_beneficios_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "codigos_beneficios_fiscais_cst",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_beneficio_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cst = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_codigos_beneficios_fiscais_cst", x => x.id);
                    table.ForeignKey(
                        name: "f_k_codigos_beneficios_fiscais_cst_codigos_beneficios_fiscais_c~",
                        column: x => x.codigo_beneficio_fiscal_id,
                        principalSchema: "plataforma",
                        principalTable: "codigos_beneficios_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_sync_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_tenant_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_codigos_beneficios_fiscais_csosn_codigo_beneficio_fiscal_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_csosn",
                column: "codigo_beneficio_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_csosn_sync_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_csosn",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_csosn_tenant_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_csosn",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_codigos_beneficios_fiscais_cst_codigo_beneficio_fiscal_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_cst",
                column: "codigo_beneficio_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_cst_sync_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_cst",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_beneficio_fiscal_cst_tenant_id",
                schema: "plataforma",
                table: "codigos_beneficios_fiscais_cst",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codigos_beneficios_fiscais_csosn",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "codigos_beneficios_fiscais_cst",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "codigos_beneficios_fiscais",
                schema: "plataforma");
        }
    }
}
