using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Qualidade.Migrations
{
    /// <inheritdoc />
    public partial class AddQualidadeQpsFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qld_qps_acao_8d",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    disciplina = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_acao_8d", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_anexo",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_bloqueio",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_bloqueio = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    alcada_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_qps_bloqueio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_documento",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    numero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_qld_qps_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_evento",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_evento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_historico",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_indicador",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scorecard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    peso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fonte = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("p_k_qld_qps_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_parametro",
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
                    table.PrimaryKey("p_k_qld_qps_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_plano_8d",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    disciplina_atual = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    conclusao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("p_k_qld_qps_plano_8d", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_registro",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    parceiro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_parceiro = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    status_homologacao = table.Column<int>(type: "integer", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_homologacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_validade_homologacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_bloqueio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qld_qps_scorecard",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    score = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    abaixo_limite = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    calculado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_qld_qps_scorecard", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_acao_8d_tenant_id_plano_id",
                schema: "qualidade",
                table: "qld_qps_acao_8d",
                columns: new[] { "tenant_id", "plano_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_acao8d_sync_id",
                schema: "qualidade",
                table: "qld_qps_acao_8d",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_acao8d_tenant_id",
                schema: "qualidade",
                table: "qld_qps_acao_8d",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_anexo_tenant_id_registro_id",
                schema: "qualidade",
                table: "qld_qps_anexo",
                columns: new[] { "tenant_id", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_anexo_sync_id",
                schema: "qualidade",
                table: "qld_qps_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_anexo_tenant_id",
                schema: "qualidade",
                table: "qld_qps_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_bloqueio_tenant_id_registro_id_ativo",
                schema: "qualidade",
                table: "qld_qps_bloqueio",
                columns: new[] { "tenant_id", "registro_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_bloqueio_sync_id",
                schema: "qualidade",
                table: "qld_qps_bloqueio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_bloqueio_tenant_id",
                schema: "qualidade",
                table: "qld_qps_bloqueio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_documento_tenant_id_registro_id",
                schema: "qualidade",
                table: "qld_qps_documento",
                columns: new[] { "tenant_id", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_documento_sync_id",
                schema: "qualidade",
                table: "qld_qps_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_documento_tenant_id",
                schema: "qualidade",
                table: "qld_qps_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_evento_tenant_id_registro_id",
                schema: "qualidade",
                table: "qld_qps_evento",
                columns: new[] { "tenant_id", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_evento_sync_id",
                schema: "qualidade",
                table: "qld_qps_evento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_evento_tenant_id",
                schema: "qualidade",
                table: "qld_qps_evento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_historico_tenant_id_registro_id",
                schema: "qualidade",
                table: "qld_qps_historico",
                columns: new[] { "tenant_id", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_historico_sync_id",
                schema: "qualidade",
                table: "qld_qps_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_historico_tenant_id",
                schema: "qualidade",
                table: "qld_qps_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_indicador_tenant_id_scorecard_id",
                schema: "qualidade",
                table: "qld_qps_indicador",
                columns: new[] { "tenant_id", "scorecard_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_indicador_sync_id",
                schema: "qualidade",
                table: "qld_qps_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_indicador_tenant_id",
                schema: "qualidade",
                table: "qld_qps_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_parametro_tenant_id_chave",
                schema: "qualidade",
                table: "qld_qps_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_parametro_sync_id",
                schema: "qualidade",
                table: "qld_qps_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_parametro_tenant_id",
                schema: "qualidade",
                table: "qld_qps_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_plano_8d_tenant_id_registro_id",
                schema: "qualidade",
                table: "qld_qps_plano_8d",
                columns: new[] { "tenant_id", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_plano8d_sync_id",
                schema: "qualidade",
                table: "qld_qps_plano_8d",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_plano8d_tenant_id",
                schema: "qualidade",
                table: "qld_qps_plano_8d",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_registro_tenant_id_codigo",
                schema: "qualidade",
                table: "qld_qps_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_registro_tenant_id_parceiro_id",
                schema: "qualidade",
                table: "qld_qps_registro",
                columns: new[] { "tenant_id", "parceiro_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_registro_tenant_id_status_homologacao",
                schema: "qualidade",
                table: "qld_qps_registro",
                columns: new[] { "tenant_id", "status_homologacao" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_registro_sync_id",
                schema: "qualidade",
                table: "qld_qps_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_registro_tenant_id",
                schema: "qualidade",
                table: "qld_qps_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_qld_qps_scorecard_tenant_id_registro_id_periodo",
                schema: "qualidade",
                table: "qld_qps_scorecard",
                columns: new[] { "tenant_id", "registro_id", "periodo" });

            migrationBuilder.CreateIndex(
                name: "ix__qps_scorecard_sync_id",
                schema: "qualidade",
                table: "qld_qps_scorecard",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__qps_scorecard_tenant_id",
                schema: "qualidade",
                table: "qld_qps_scorecard",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qld_qps_acao_8d",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_anexo",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_bloqueio",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_documento",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_evento",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_historico",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_indicador",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_parametro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_plano_8d",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_registro",
                schema: "qualidade");

            migrationBuilder.DropTable(
                name: "qld_qps_scorecard",
                schema: "qualidade");

        }
    }
}
