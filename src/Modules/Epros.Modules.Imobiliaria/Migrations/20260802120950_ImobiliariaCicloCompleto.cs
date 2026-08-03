using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Imobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class ImobiliariaCicloCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "imo_cobranca_aluguel",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vencimento = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_pago = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    receber_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_baixa = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_imo_cobranca_aluguel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_garantia",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_limite = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    substitui_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fiador_pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_imo_locacao_garantia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_reajuste",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    indice = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    data_base = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual_aplicado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_imo_locacao_reajuste", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_rescisao",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    data_rescisao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    aviso_previo_dias = table.Column<int>(type: "integer", nullable: true),
                    multa_proporcional = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vistoria_saida_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_imo_locacao_rescisao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_proposta",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_proposto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    contraproposta_de_id = table.Column<Guid>(type: "uuid", nullable: true),
                    locacao_gerada_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_imo_proposta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_recibo_aluguel",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cobranca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    documento_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_imo_recibo_aluguel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    erro = table.Column<string>(type: "text", nullable: true),
                    tentativas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_proposta_parte",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_imo_proposta_parte", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_proposta_parte_imo_proposta_proposta_id",
                        column: x => x.proposta_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_proposta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_cobranca_aluguel_tenant_id_locacao_id_competencia_tipo",
                schema: "imobiliaria",
                table: "imo_cobranca_aluguel",
                columns: new[] { "tenant_id", "locacao_id", "competencia", "tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cobranca_aluguel_sync_id",
                schema: "imobiliaria",
                table: "imo_cobranca_aluguel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cobranca_aluguel_tenant_id",
                schema: "imobiliaria",
                table: "imo_cobranca_aluguel",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_garantia_tenant_id_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_garantia",
                columns: new[] { "tenant_id", "locacao_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_garantia_tenant_id_substitui_id",
                schema: "imobiliaria",
                table: "imo_locacao_garantia",
                columns: new[] { "tenant_id", "substitui_id" });

            migrationBuilder.CreateIndex(
                name: "ix__locacao_garantia_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_garantia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_garantia_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_garantia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_reajuste_tenant_id_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_reajuste",
                columns: new[] { "tenant_id", "locacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__locacao_reajuste_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_reajuste",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_reajuste_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_reajuste",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_rescisao_tenant_id_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_rescisao",
                columns: new[] { "tenant_id", "locacao_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_rescisao_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_rescisao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_rescisao_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_rescisao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_proposta_tenant_id_contraproposta_de_id",
                schema: "imobiliaria",
                table: "imo_proposta",
                columns: new[] { "tenant_id", "contraproposta_de_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_proposta_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_proposta",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__proposta_sync_id",
                schema: "imobiliaria",
                table: "imo_proposta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__proposta_tenant_id",
                schema: "imobiliaria",
                table: "imo_proposta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_proposta_parte_proposta_id",
                schema: "imobiliaria",
                table: "imo_proposta_parte",
                column: "proposta_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_proposta_parte_tenant_id_proposta_id_pessoa_id_papel",
                schema: "imobiliaria",
                table: "imo_proposta_parte",
                columns: new[] { "tenant_id", "proposta_id", "pessoa_id", "papel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__proposta_parte_sync_id",
                schema: "imobiliaria",
                table: "imo_proposta_parte",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__proposta_parte_tenant_id",
                schema: "imobiliaria",
                table: "imo_proposta_parte",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_recibo_aluguel_tenant_id_cobranca_id",
                schema: "imobiliaria",
                table: "imo_recibo_aluguel",
                columns: new[] { "tenant_id", "cobranca_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_imo_recibo_aluguel_tenant_id_numero",
                schema: "imobiliaria",
                table: "imo_recibo_aluguel",
                columns: new[] { "tenant_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recibo_aluguel_sync_id",
                schema: "imobiliaria",
                table: "imo_recibo_aluguel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recibo_aluguel_tenant_id",
                schema: "imobiliaria",
                table: "imo_recibo_aluguel",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imo_cobranca_aluguel",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_garantia",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_reajuste",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_rescisao",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_proposta_parte",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_recibo_aluguel",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_proposta",
                schema: "imobiliaria");

        }
    }
}
