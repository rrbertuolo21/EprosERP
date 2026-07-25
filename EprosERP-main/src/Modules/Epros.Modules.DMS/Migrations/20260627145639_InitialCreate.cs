using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.DMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "concessionarias");

            migrationBuilder.CreateTable(
                name: "garantias_montadora",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_dms_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chassi = table.Column<string>(type: "text", nullable: false),
                    peca_reclamada = table.Column<string>(type: "text", nullable: false),
                    valor_reclamado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    parecer_montadora = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_garantias_montadora", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico_dms",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_os = table.Column<string>(type: "text", nullable: false),
                    veiculo_chassi = table.Column<string>(type: "text", nullable: false),
                    descricao_inconveniente = table.Column<string>(type: "text", nullable: false),
                    valor_pecas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_mao_de_obra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reclamacao_garantia = table.Column<bool>(type: "boolean", nullable: false),
                    status_garantia = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_ordens_servico_dms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendas_veiculos",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chassi = table.Column<string>(type: "text", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    marca = table.Column<string>(type: "text", nullable: false),
                    ano_modelo = table.Column<int>(type: "integer", nullable: false),
                    preco_venda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cliente_nome = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_vendas_veiculos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_garantias_montadora_tenant_id_ordem_servico_dms_id",
                schema: "concessionarias",
                table: "garantias_montadora",
                columns: new[] { "tenant_id", "ordem_servico_dms_id" });

            migrationBuilder.CreateIndex(
                name: "ix__garantia_montadora_sync_id",
                schema: "concessionarias",
                table: "garantias_montadora",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__garantia_montadora_tenant_id",
                schema: "concessionarias",
                table: "garantias_montadora",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ordens_servico_dms_tenant_id_numero_os",
                schema: "concessionarias",
                table: "ordens_servico_dms",
                columns: new[] { "tenant_id", "numero_os" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_dms_sync_id",
                schema: "concessionarias",
                table: "ordens_servico_dms",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_dms_tenant_id",
                schema: "concessionarias",
                table: "ordens_servico_dms",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_vendas_veiculos_tenant_id_chassi",
                schema: "concessionarias",
                table: "vendas_veiculos",
                columns: new[] { "tenant_id", "chassi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_veiculo_sync_id",
                schema: "concessionarias",
                table: "vendas_veiculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__venda_veiculo_tenant_id",
                schema: "concessionarias",
                table: "vendas_veiculos",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "garantias_montadora",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "ordens_servico_dms",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "vendas_veiculos",
                schema: "concessionarias");
        }
    }
}
