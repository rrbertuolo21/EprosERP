using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddDevolucaoContingenciaEventoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "justificativa",
                schema: "plataforma",
                table: "eventos_documentos_fiscais",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_caminho",
                schema: "plataforma",
                table: "eventos_documentos_fiscais",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "xml_caminho",
                schema: "plataforma",
                table: "eventos_documentos_fiscais",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chave_referenciada",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "character varying(44)",
                maxLength: 44,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_hora_contingencia",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "finalidade",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "justificativa_contingencia",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tipo_emissao",
                schema: "plataforma",
                table: "documentos_fiscais",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "devolucoes_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    modelo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    chave_nf_entrada = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    chave_gerada = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero_gerado = table.Column<long>(type: "bigint", nullable: true),
                    protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    mensagem_retorno = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    xml_entrada = table.Column<string>(type: "text", nullable: true),
                    xml_retorno = table.Column<string>(type: "text", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    documento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destinatario_cnpj_cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    destinatario_nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_transmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_devolucoes_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "devolucao_fiscal_itens",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    devolucao_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sku = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nome_produto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ncm = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    cfop = table.Column<int>(type: "integer", nullable: false),
                    cst = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_devolucao_fiscal_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_devolucao_fiscal_itens_devolucoes_fiscais_devolucao_fiscal_~",
                        column: x => x.devolucao_fiscal_id,
                        principalSchema: "plataforma",
                        principalTable: "devolucoes_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_devolucao_fiscal_itens_devolucao_fiscal_id",
                schema: "plataforma",
                table: "devolucao_fiscal_itens",
                column: "devolucao_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "i_x_devolucao_fiscal_itens_tenant_id_devolucao_fiscal_id",
                schema: "plataforma",
                table: "devolucao_fiscal_itens",
                columns: new[] { "tenant_id", "devolucao_fiscal_id" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_fiscal_item_sync_id",
                schema: "plataforma",
                table: "devolucao_fiscal_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_fiscal_item_tenant_id",
                schema: "plataforma",
                table: "devolucao_fiscal_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_devolucoes_fiscais_tenant_id_chave_nf_entrada",
                schema: "plataforma",
                table: "devolucoes_fiscais",
                columns: new[] { "tenant_id", "chave_nf_entrada" });

            migrationBuilder.CreateIndex(
                name: "i_x_devolucoes_fiscais_tenant_id_estado",
                schema: "plataforma",
                table: "devolucoes_fiscais",
                columns: new[] { "tenant_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_fiscal_sync_id",
                schema: "plataforma",
                table: "devolucoes_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_fiscal_tenant_id",
                schema: "plataforma",
                table: "devolucoes_fiscais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devolucao_fiscal_itens",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "devolucoes_fiscais",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "justificativa",
                schema: "plataforma",
                table: "eventos_documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "pdf_caminho",
                schema: "plataforma",
                table: "eventos_documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "xml_caminho",
                schema: "plataforma",
                table: "eventos_documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "chave_referenciada",
                schema: "plataforma",
                table: "documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "data_hora_contingencia",
                schema: "plataforma",
                table: "documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "finalidade",
                schema: "plataforma",
                table: "documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "justificativa_contingencia",
                schema: "plataforma",
                table: "documentos_fiscais");

            migrationBuilder.DropColumn(
                name: "tipo_emissao",
                schema: "plataforma",
                table: "documentos_fiscais");
        }
    }
}
