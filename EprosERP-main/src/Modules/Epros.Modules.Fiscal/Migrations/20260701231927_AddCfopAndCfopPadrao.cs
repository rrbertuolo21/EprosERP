using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddCfopAndCfopPadrao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cfop_padroes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cfop_codigo = table.Column<int>(type: "integer", nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    natureza_operacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    cfop_correlacao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    integra_faturamento = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_nfe = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_comunicacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_transporte = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_devolucao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_retorno = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_anulacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_remessa = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_combustivel = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_transferencia = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_nfce = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_ciap = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_uso_consumo = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_uso_sem_operacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_st = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_mei = table.Column<bool>(type: "boolean", nullable: false),
                    incidencia_simples = table.Column<int>(type: "integer", nullable: false),
                    cfop_devolucao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
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
                    table.PrimaryKey("p_k_cfop_padroes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cfops",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cfop_codigo = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    natureza_operacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    cfop_correlacao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    integra_faturamento = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_nfe = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_comunicacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_transporte = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_devolucao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_retorno = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_anulacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_remessa = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_combustivel = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_transferencia = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_nfce = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_ciap = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_uso_consumo = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_uso_sem_operacao = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_st = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_mei = table.Column<bool>(type: "boolean", nullable: false),
                    incidencia_simples = table.Column<int>(type: "integer", nullable: false),
                    cfop_devolucao = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
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
                    table.PrimaryKey("p_k_cfops", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__cfop_padrao_sync_id",
                schema: "plataforma",
                table: "cfop_padroes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cfop_padrao_tenant_id",
                schema: "plataforma",
                table: "cfop_padroes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__cfop_sync_id",
                schema: "plataforma",
                table: "cfops",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cfop_tenant_id",
                schema: "plataforma",
                table: "cfops",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cfop_padroes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "cfops",
                schema: "plataforma");
        }
    }
}
