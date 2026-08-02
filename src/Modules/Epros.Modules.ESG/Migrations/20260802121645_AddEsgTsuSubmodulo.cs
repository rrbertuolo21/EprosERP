using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.ESG.Migrations
{
    /// <inheritdoc />
    public partial class AddEsgTsuSubmodulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tsu_calculo",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trecho_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tkm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    resultado_c_o2e = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    intensidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    formula_versao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fator_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    fator_versao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    fator_fonte = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fator_pendente = table.Column<bool>(type: "boolean", nullable: false),
                    memoria_calculo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("p_k_tsu_calculo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_medicao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta_modal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    participacao_realizada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_tsu_medicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_meta_modal",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_tsu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    participacao_alvo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_tsu_meta_modal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_operacao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_tsu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    data_operacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_tsu_operacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_parametro",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_json = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("p_k_tsu_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_registro",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_tsu_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tsu_trecho",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    distancia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_distancia = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    massa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_massa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    energia_combustivel = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unidade_energia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_tsu_trecho", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_calculo_tenant_id_fator_pendente",
                schema: "esg",
                table: "tsu_calculo",
                columns: new[] { "tenant_id", "fator_pendente" });

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_calculo_tenant_id_trecho_id",
                schema: "esg",
                table: "tsu_calculo",
                columns: new[] { "tenant_id", "trecho_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tsu_calculo_sync_id",
                schema: "esg",
                table: "tsu_calculo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_calculo_tenant_id",
                schema: "esg",
                table: "tsu_calculo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_medicao_tenant_id_meta_modal_id",
                schema: "esg",
                table: "tsu_medicao",
                columns: new[] { "tenant_id", "meta_modal_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tsu_medicao_sync_id",
                schema: "esg",
                table: "tsu_medicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_medicao_tenant_id",
                schema: "esg",
                table: "tsu_medicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_meta_modal_tenant_id_registro_tsu_id",
                schema: "esg",
                table: "tsu_meta_modal",
                columns: new[] { "tenant_id", "registro_tsu_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tsu_meta_modal_sync_id",
                schema: "esg",
                table: "tsu_meta_modal",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_meta_modal_tenant_id",
                schema: "esg",
                table: "tsu_meta_modal",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_operacao_tenant_id_registro_tsu_id",
                schema: "esg",
                table: "tsu_operacao",
                columns: new[] { "tenant_id", "registro_tsu_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tsu_operacao_sync_id",
                schema: "esg",
                table: "tsu_operacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_operacao_tenant_id",
                schema: "esg",
                table: "tsu_operacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_parametro_tenant_id_chave",
                schema: "esg",
                table: "tsu_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_parametro_sync_id",
                schema: "esg",
                table: "tsu_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_parametro_tenant_id",
                schema: "esg",
                table: "tsu_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_registro_tenant_id_codigo",
                schema: "esg",
                table: "tsu_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_registro_sync_id",
                schema: "esg",
                table: "tsu_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_registro_tenant_id",
                schema: "esg",
                table: "tsu_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_tsu_trecho_tenant_id_operacao_id",
                schema: "esg",
                table: "tsu_trecho",
                columns: new[] { "tenant_id", "operacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tsu_trecho_sync_id",
                schema: "esg",
                table: "tsu_trecho",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tsu_trecho_tenant_id",
                schema: "esg",
                table: "tsu_trecho",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tsu_calculo",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_medicao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_meta_modal",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_operacao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_parametro",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_registro",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "tsu_trecho",
                schema: "esg");

        }
    }
}
