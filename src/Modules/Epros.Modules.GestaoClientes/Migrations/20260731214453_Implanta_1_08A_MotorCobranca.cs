using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_08A_MotorCobranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "trial_convertido_em",
                schema: "plataforma",
                table: "assinaturas_clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "recibos_pagamento",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fatura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pagamento_fatura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    meio_pagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    pagador_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    pagador_documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("p_k_recibos_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_recibos_pagamento_faturas_fatura_id",
                        column: x => x.fatura_id,
                        principalSchema: "plataforma",
                        principalTable: "faturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix__recibo_pagamento_sync_id",
                schema: "plataforma",
                table: "recibos_pagamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recibo_pagamento_tenant_id",
                schema: "plataforma",
                table: "recibos_pagamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_recibos_pagamento_fatura",
                schema: "plataforma",
                table: "recibos_pagamento",
                column: "fatura_id");

            migrationBuilder.CreateIndex(
                name: "ux_recibos_pagamento_numero",
                schema: "plataforma",
                table: "recibos_pagamento",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recibos_pagamento",
                schema: "plataforma");

            migrationBuilder.DropColumn(
                name: "trial_convertido_em",
                schema: "plataforma",
                table: "assinaturas_clientes");
        }
    }
}
