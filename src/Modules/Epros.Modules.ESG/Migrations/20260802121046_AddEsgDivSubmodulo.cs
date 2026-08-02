using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.ESG.Migrations
{
    /// <inheritdoc />
    public partial class AddEsgDivSubmodulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "div_acao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resultado = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_div_acao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "div_indicador",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fonte = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    formula = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_div_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "div_medicao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dimensao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dimensoes_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    qtd_individuos = table.Column<int>(type: "integer", nullable: false),
                    valor_agregado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    suprimido = table.Column<bool>(type: "boolean", nullable: false),
                    origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("p_k_div_medicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "div_meta",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_esperado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_div_meta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "div_parametro",
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
                    table.PrimaryKey("p_k_div_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "div_programa",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_div_programa", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_div_acao_tenant_id_programa_id",
                schema: "esg",
                table: "div_acao",
                columns: new[] { "tenant_id", "programa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__div_acao_sync_id",
                schema: "esg",
                table: "div_acao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_acao_tenant_id",
                schema: "esg",
                table: "div_acao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_div_indicador_tenant_id_codigo_versao",
                schema: "esg",
                table: "div_indicador",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_indicador_sync_id",
                schema: "esg",
                table: "div_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_indicador_tenant_id",
                schema: "esg",
                table: "div_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_div_medicao_tenant_id_indicador_id_dimensoes_hash",
                schema: "esg",
                table: "div_medicao",
                columns: new[] { "tenant_id", "indicador_id", "dimensoes_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_medicao_sync_id",
                schema: "esg",
                table: "div_medicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_medicao_tenant_id",
                schema: "esg",
                table: "div_medicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_div_meta_tenant_id_indicador_id",
                schema: "esg",
                table: "div_meta",
                columns: new[] { "tenant_id", "indicador_id" });

            migrationBuilder.CreateIndex(
                name: "ix__div_meta_sync_id",
                schema: "esg",
                table: "div_meta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_meta_tenant_id",
                schema: "esg",
                table: "div_meta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_div_parametro_tenant_id_chave",
                schema: "esg",
                table: "div_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_parametro_sync_id",
                schema: "esg",
                table: "div_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_parametro_tenant_id",
                schema: "esg",
                table: "div_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_div_programa_tenant_id_codigo",
                schema: "esg",
                table: "div_programa",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_programa_sync_id",
                schema: "esg",
                table: "div_programa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__div_programa_tenant_id",
                schema: "esg",
                table: "div_programa",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "div_acao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "div_indicador",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "div_medicao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "div_meta",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "div_parametro",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "div_programa",
                schema: "esg");

        }
    }
}
