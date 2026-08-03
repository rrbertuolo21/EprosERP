using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Qualidade.Migrations
{
    /// <inheritdoc />
    public partial class AddQualidadeRstRecall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qld_rst_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_anexo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_bloqueio",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_bloqueio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_campanha",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    gravidade = table.Column<int>(type: "integer", nullable: false),
                    etapa = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ncr_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_mercado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    conclusao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    motivo_cancelamento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_rst_campanha", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_comunicacao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    canal = table.Column<int>(type: "integer", nullable: false),
                    conteudo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_comunicacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_disposicao",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_disposicao = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_qld_rst_disposicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_evento",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    direcao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_rst_evento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_genealogia_no",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_no = table.Column<int>(type: "integer", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    lacuna = table.Column<bool>(type: "boolean", nullable: false),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_genealogia_no", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    acao = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_item_afetado",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    localizacao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_item_afetado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_origem",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    referencia_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_rst_origem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_parametro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_qld_rst_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_rst_recolhimento",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campanha_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_prevista = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantidade_recolhida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_qld_rst_recolhimento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_anexo_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_anexo",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_anexo_sync_id",
                schema: "qualidade",
                table: "qld_rst_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_rst_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_bloqueio_tenant_id_campanha_id_ativo",
                schema: "qualidade",
                table: "qld_rst_bloqueio",
                columns: new[] { "tenant_id", "campanha_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_bloqueio_sync_id",
                schema: "qualidade",
                table: "qld_rst_bloqueio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_bloqueio_tenant_id",
                schema: "qualidade",
                table: "qld_rst_bloqueio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_campanha_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_rst_campanha",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_campanha_tenant_id_ncr_id",
                schema: "qualidade",
                table: "qld_rst_campanha",
                columns: new[] { "tenant_id", "ncr_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_campanha_tenant_id_status_etapa",
                schema: "qualidade",
                table: "qld_rst_campanha",
                columns: new[] { "tenant_id", "status", "etapa" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_campanha_sync_id",
                schema: "qualidade",
                table: "qld_rst_campanha",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_campanha_tenant_id",
                schema: "qualidade",
                table: "qld_rst_campanha",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_comunicacao_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_comunicacao",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_comunicacao_sync_id",
                schema: "qualidade",
                table: "qld_rst_comunicacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_comunicacao_tenant_id",
                schema: "qualidade",
                table: "qld_rst_comunicacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_disposicao_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_disposicao",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_disposicao_sync_id",
                schema: "qualidade",
                table: "qld_rst_disposicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_disposicao_tenant_id",
                schema: "qualidade",
                table: "qld_rst_disposicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_evento_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_evento",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_evento_sync_id",
                schema: "qualidade",
                table: "qld_rst_evento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_evento_tenant_id",
                schema: "qualidade",
                table: "qld_rst_evento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_genealogia_no_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_genealogia_no",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_genealogia_no_tenant_id_pai_id",
                schema: "qualidade",
                table: "qld_rst_genealogia_no",
                columns: new[] { "tenant_id", "pai_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_genealogia_no_sync_id",
                schema: "qualidade",
                table: "qld_rst_genealogia_no",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_genealogia_no_tenant_id",
                schema: "qualidade",
                table: "qld_rst_genealogia_no",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_historico_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_historico",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_historico_sync_id",
                schema: "qualidade",
                table: "qld_rst_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_historico_tenant_id",
                schema: "qualidade",
                table: "qld_rst_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_item_afetado_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_item_afetado",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_item_afetado_sync_id",
                schema: "qualidade",
                table: "qld_rst_item_afetado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_item_afetado_tenant_id",
                schema: "qualidade",
                table: "qld_rst_item_afetado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_origem_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_origem",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_origem_sync_id",
                schema: "qualidade",
                table: "qld_rst_origem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_origem_tenant_id",
                schema: "qualidade",
                table: "qld_rst_origem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_rst_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_parametro_sync_id",
                schema: "qualidade",
                table: "qld_rst_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_rst_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_rst_recolhimento_tenant_id_campanha_id",
                schema: "qualidade",
                table: "qld_rst_recolhimento",
                columns: new[] { "tenant_id", "campanha_id" });

            migrationBuilder.CreateIndex(
                name: "ix__rst_recolhimento_sync_id",
                schema: "qualidade",
                table: "qld_rst_recolhimento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rst_recolhimento_tenant_id",
                schema: "qualidade",
                table: "qld_rst_recolhimento",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qld_rst_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_bloqueio",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_campanha",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_comunicacao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_disposicao",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_evento",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_genealogia_no",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_item_afetado",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_origem",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_rst_recolhimento",
                schema: "qualidade");
        }
    }
}
