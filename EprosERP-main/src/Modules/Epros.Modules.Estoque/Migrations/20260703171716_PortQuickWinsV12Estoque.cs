using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class PortQuickWinsV12Estoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "importacoes_arquivo_xml_saida",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    qtd_xmls = table.Column<int>(type: "integer", nullable: false),
                    qtd_xmls_invalidos = table.Column<int>(type: "integer", nullable: false),
                    qtd_produtos_localizados = table.Column<int>(type: "integer", nullable: false),
                    qtd_clientes_localizados = table.Column<int>(type: "integer", nullable: false),
                    qtd_produtos_importados = table.Column<int>(type: "integer", nullable: false),
                    qtd_clientes_importados = table.Column<int>(type: "integer", nullable: false),
                    mensagem_erro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_importacoes_arquivo_xml_saida", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "importacoes_xml",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: false),
                    tipo_de_xml = table.Column<int>(type: "integer", nullable: false),
                    nfe_id = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    status_importacao_xml = table.Column<int>(type: "integer", nullable: false),
                    mensagem_erro_importacao_xml = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_cadastro = table.Column<int>(type: "integer", nullable: false),
                    mensagem_erro_cadastro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_salvar_pdf = table.Column<int>(type: "integer", nullable: false),
                    mensagem_erro_salvar_pdf = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    codigo_sefaz = table.Column<int>(type: "integer", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
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
                    table.PrimaryKey("p_k_importacoes_xml", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_arquivo_xml_saida_tenant_id_status",
                schema: "estoque",
                table: "importacoes_arquivo_xml_saida",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__importacao_arquivo_xml_saida_sync_id",
                schema: "estoque",
                table: "importacoes_arquivo_xml_saida",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__importacao_arquivo_xml_saida_tenant_id",
                schema: "estoque",
                table: "importacoes_arquivo_xml_saida",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_xml_tenant_id_empresa_id",
                schema: "estoque",
                table: "importacoes_xml",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_importacoes_xml_tenant_id_nfe_id",
                schema: "estoque",
                table: "importacoes_xml",
                columns: new[] { "tenant_id", "nfe_id" });

            migrationBuilder.CreateIndex(
                name: "ix__importacao_xml_sync_id",
                schema: "estoque",
                table: "importacoes_xml",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__importacao_xml_tenant_id",
                schema: "estoque",
                table: "importacoes_xml",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "importacoes_arquivo_xml_saida",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "importacoes_xml",
                schema: "estoque");
        }
    }
}
