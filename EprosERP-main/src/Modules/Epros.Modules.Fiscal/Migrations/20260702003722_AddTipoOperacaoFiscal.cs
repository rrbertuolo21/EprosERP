using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoOperacaoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipos_operacoes_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tributario_grupo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cfop_nfe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cfop_nfce_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    sobescreve_tributacao_ncm = table.Column<bool>(type: "boolean", nullable: false),
                    finalidade = table.Column<int>(type: "integer", nullable: false),
                    atendimento = table.Column<int>(type: "integer", nullable: false),
                    tipo_frete = table.Column<int>(type: "integer", nullable: false),
                    tipo_movimento = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_tipos_operacoes_fiscais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_tipos_operacoes_fiscais_cfops_cfop_nfce_id",
                        column: x => x.cfop_nfce_id,
                        principalSchema: "plataforma",
                        principalTable: "cfops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_tipos_operacoes_fiscais_cfops_cfop_nfe_id",
                        column: x => x.cfop_nfe_id,
                        principalSchema: "plataforma",
                        principalTable: "cfops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_tipos_operacoes_fiscais_cfop_nfce_id",
                schema: "plataforma",
                table: "tipos_operacoes_fiscais",
                column: "cfop_nfce_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tipos_operacoes_fiscais_cfop_nfe_id",
                schema: "plataforma",
                table: "tipos_operacoes_fiscais",
                column: "cfop_nfe_id");

            migrationBuilder.CreateIndex(
                name: "ix__tipo_operacao_fiscal_sync_id",
                schema: "plataforma",
                table: "tipos_operacoes_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tipo_operacao_fiscal_tenant_id",
                schema: "plataforma",
                table: "tipos_operacoes_fiscais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipos_operacoes_fiscais",
                schema: "plataforma");
        }
    }
}
