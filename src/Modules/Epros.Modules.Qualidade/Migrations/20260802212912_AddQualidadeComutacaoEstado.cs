using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Qualidade.Migrations
{
    /// <inheritdoc />
    public partial class AddQualidadeComutacaoEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "qld_ins_comutacao_estado",
                schema: "qualidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aql = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    severidade = table.Column<int>(type: "integer", nullable: false),
                    suspensa = table.Column<bool>(type: "boolean", nullable: false),
                    consecutivos_aceitos_normal = table.Column<int>(type: "integer", nullable: false),
                    soma_defeituosos_normal = table.Column<int>(type: "integer", nullable: false),
                    consecutivos_aceitos_severa = table.Column<int>(type: "integer", nullable: false),
                    rejeitados_acumulados_severa = table.Column<int>(type: "integer", nullable: false),
                    janela_normal_rejeicoes = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lotes_processados = table.Column<int>(type: "integer", nullable: false),
                    ultimo_lote_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_qld_ins_comutacao_estado", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_qld_ins_comutacao_estado_tenant_id_fornecedor_id_produto_id~",
                schema: "qualidade",
                table: "qld_ins_comutacao_estado",
                columns: new[] { "tenant_id", "fornecedor_id", "produto_id", "aql" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estado_comutacao_inspecao_sync_id",
                schema: "qualidade",
                table: "qld_ins_comutacao_estado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estado_comutacao_inspecao_tenant_id",
                schema: "qualidade",
                table: "qld_ins_comutacao_estado",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "qld_ins_comutacao_estado",
                schema: "qualidade");
        }
    }
}
