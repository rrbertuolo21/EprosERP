using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "plataforma");

            migrationBuilder.CreateTable(
                name: "documentos_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    chave_acesso = table.Column<string>(type: "text", nullable: false),
                    recibo = table.Column<string>(type: "text", nullable: true),
                    protocolo = table.Column<string>(type: "text", nullable: true),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: true),
                    motivo_rejeicao_sefaz = table.Column<string>(type: "text", nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_autorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xml_envio = table.Column<string>(type: "text", nullable: true),
                    xml_retorno = table.Column<string>(type: "text", nullable: true),
                    pdf_caminho = table.Column<string>(type: "text", nullable: true),
                    xml_caminho = table.Column<string>(type: "text", nullable: true),
                    destinatario_cnpj_cpf = table.Column<string>(type: "text", nullable: false),
                    destinatario_nome = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_documentos_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eventos_documentos_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "text", nullable: false),
                    status_sefaz = table.Column<int>(type: "integer", nullable: false),
                    x_motivo = table.Column<string>(type: "text", nullable: true),
                    d_h_recebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    protocolo = table.Column<string>(type: "text", nullable: true),
                    sequencia_evento = table.Column<int>(type: "integer", nullable: false),
                    x_correcao = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_eventos_documentos_fiscais", x => x.id);
                });

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS plataforma.outbox_messages (
                    id uuid NOT NULL,
                    tenant_id text NOT NULL,
                    event_type text NOT NULL,
                    payload text NOT NULL,
                    criado_em timestamp with time zone NOT NULL,
                    processado_em timestamp with time zone,
                    erro text,
                    tentativas integer NOT NULL,
                    CONSTRAINT p_k_outbox_messages PRIMARY KEY (id)
                );
            ");

            migrationBuilder.CreateTable(
                name: "documento_fiscal_itens",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_fiscal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    nome_produto = table.Column<string>(type: "text", nullable: false),
                    cst = table.Column<string>(type: "text", nullable: false),
                    cfop = table.Column<int>(type: "integer", nullable: false),
                    ncm = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_documento_fiscal_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_documento_fiscal_itens_documentos_fiscais_documento_fiscal_~",
                        column: x => x.documento_fiscal_id,
                        principalSchema: "plataforma",
                        principalTable: "documentos_fiscais",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_documento_fiscal_itens_documento_fiscal_id",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                column: "documento_fiscal_id");

            migrationBuilder.CreateIndex(
                name: "ix__documento_fiscal_item_sync_id",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__documento_fiscal_item_tenant_id",
                schema: "plataforma",
                table: "documento_fiscal_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__documento_fiscal_sync_id",
                schema: "plataforma",
                table: "documentos_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__documento_fiscal_tenant_id",
                schema: "plataforma",
                table: "documentos_fiscais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__evento_documento_fiscal_sync_id",
                schema: "plataforma",
                table: "eventos_documentos_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__evento_documento_fiscal_tenant_id",
                schema: "plataforma",
                table: "eventos_documentos_fiscais",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_fiscal_itens",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "eventos_documentos_fiscais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "documentos_fiscais",
                schema: "plataforma");
        }
    }
}
