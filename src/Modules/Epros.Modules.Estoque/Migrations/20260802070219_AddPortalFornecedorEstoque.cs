using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalFornecedorEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pfo_convites_fornecedor",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_convite = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pfo_convites_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_cotacoes_publicadas",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    prazo_resposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pfo_cotacoes_publicadas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_documentos_fornecedor",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia_tipo = table.Column<int>(type: "integer", nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_pfo_documentos_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_pre_avisos_embarque",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_prevista_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_pfo_pre_avisos_embarque", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_respostas_cotacao",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cotacao_publicada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    enviada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pfo_respostas_cotacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_usuarios_fornecedor",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acesso_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pfo_usuarios_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pfo_pre_aviso_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pre_aviso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_compra_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_prevista = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
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
                    table.PrimaryKey("p_k_pfo_pre_aviso_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pfo_pre_aviso_itens_pfo_pre_avisos_embarque_pre_aviso_id",
                        column: x => x.pre_aviso_id,
                        principalSchema: "estoque",
                        principalTable: "pfo_pre_avisos_embarque",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pfo_resposta_cotacao_itens",
                schema: "estoque",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resposta_cotacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    prazo_entrega_dias = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_pfo_resposta_cotacao_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pfo_resposta_cotacao_itens_pfo_respostas_cotacao_resposta_c~",
                        column: x => x.resposta_cotacao_id,
                        principalSchema: "estoque",
                        principalTable: "pfo_respostas_cotacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_convites_fornecedor_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_convites_fornecedor",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_convites_fornecedor_tenant_id_status",
                schema: "estoque",
                table: "pfo_convites_fornecedor",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_convite_fornecedor_sync_id",
                schema: "estoque",
                table: "pfo_convites_fornecedor",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_convite_fornecedor_tenant_id",
                schema: "estoque",
                table: "pfo_convites_fornecedor",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_cotacoes_publicadas_tenant_id_cotacao_origem_id",
                schema: "estoque",
                table: "pfo_cotacoes_publicadas",
                columns: new[] { "tenant_id", "cotacao_origem_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_cotacoes_publicadas_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_cotacoes_publicadas",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_cotacao_publicada_sync_id",
                schema: "estoque",
                table: "pfo_cotacoes_publicadas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_cotacao_publicada_tenant_id",
                schema: "estoque",
                table: "pfo_cotacoes_publicadas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_documentos_fornecedor_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_documentos_fornecedor",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_documentos_fornecedor_tenant_id_referencia_tipo_referen~",
                schema: "estoque",
                table: "pfo_documentos_fornecedor",
                columns: new[] { "tenant_id", "referencia_tipo", "referencia_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_documento_fornecedor_sync_id",
                schema: "estoque",
                table: "pfo_documentos_fornecedor",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_documento_fornecedor_tenant_id",
                schema: "estoque",
                table: "pfo_documentos_fornecedor",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_pre_aviso_itens_pre_aviso_id",
                schema: "estoque",
                table: "pfo_pre_aviso_itens",
                column: "pre_aviso_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_pre_aviso_itens_tenant_id_pre_aviso_id",
                schema: "estoque",
                table: "pfo_pre_aviso_itens",
                columns: new[] { "tenant_id", "pre_aviso_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_pre_aviso_item_sync_id",
                schema: "estoque",
                table: "pfo_pre_aviso_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_pre_aviso_item_tenant_id",
                schema: "estoque",
                table: "pfo_pre_aviso_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_pre_avisos_embarque_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_pre_avisos_embarque",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_pre_avisos_embarque_tenant_id_pedido_compra_id",
                schema: "estoque",
                table: "pfo_pre_avisos_embarque",
                columns: new[] { "tenant_id", "pedido_compra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_pre_aviso_embarque_sync_id",
                schema: "estoque",
                table: "pfo_pre_avisos_embarque",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_pre_aviso_embarque_tenant_id",
                schema: "estoque",
                table: "pfo_pre_avisos_embarque",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_resposta_cotacao_itens_resposta_cotacao_id",
                schema: "estoque",
                table: "pfo_resposta_cotacao_itens",
                column: "resposta_cotacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_resposta_cotacao_itens_tenant_id_resposta_cotacao_id",
                schema: "estoque",
                table: "pfo_resposta_cotacao_itens",
                columns: new[] { "tenant_id", "resposta_cotacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_resposta_cotacao_item_sync_id",
                schema: "estoque",
                table: "pfo_resposta_cotacao_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_resposta_cotacao_item_tenant_id",
                schema: "estoque",
                table: "pfo_resposta_cotacao_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_respostas_cotacao_tenant_id_cotacao_publicada_id",
                schema: "estoque",
                table: "pfo_respostas_cotacao",
                columns: new[] { "tenant_id", "cotacao_publicada_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_respostas_cotacao_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_respostas_cotacao",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_resposta_cotacao_sync_id",
                schema: "estoque",
                table: "pfo_respostas_cotacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_resposta_cotacao_tenant_id",
                schema: "estoque",
                table: "pfo_respostas_cotacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_usuarios_fornecedor_tenant_id_fornecedor_id",
                schema: "estoque",
                table: "pfo_usuarios_fornecedor",
                columns: new[] { "tenant_id", "fornecedor_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_pfo_usuarios_fornecedor_tenant_id_usuario_id",
                schema: "estoque",
                table: "pfo_usuarios_fornecedor",
                columns: new[] { "tenant_id", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pfo_usuario_fornecedor_sync_id",
                schema: "estoque",
                table: "pfo_usuarios_fornecedor",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pfo_usuario_fornecedor_tenant_id",
                schema: "estoque",
                table: "pfo_usuarios_fornecedor",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pfo_convites_fornecedor",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_cotacoes_publicadas",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_documentos_fornecedor",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_pre_aviso_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_resposta_cotacao_itens",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_usuarios_fornecedor",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_pre_avisos_embarque",
                schema: "estoque");

            migrationBuilder.DropTable(
                name: "pfo_respostas_cotacao",
                schema: "estoque");
        }
    }
}
