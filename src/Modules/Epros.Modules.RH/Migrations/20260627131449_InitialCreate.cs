using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.RH.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rh");

            migrationBuilder.CreateTable(
                name: "colaboradores",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    cpf = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    departamento = table.Column<string>(type: "text", nullable: false),
                    salario_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_admissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_demissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_colaboradores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "folhas_pagamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mes_competencia = table.Column<int>(type: "integer", nullable: false),
                    ano_competencia = table.Column<int>(type: "integer", nullable: false),
                    salario_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descontos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    salario_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_processamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_folhas_pagamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "rh",
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
                name: "timesheets",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    horas_trabalhadas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descricao_atividade = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_timesheets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "folhas_pagamento_verbas",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    folha_pagamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_folhas_pagamento_verbas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_folhas_pagamento_verbas_folhas_pagamento_folha_pagamento_id",
                        column: x => x.folha_pagamento_id,
                        principalSchema: "rh",
                        principalTable: "folhas_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_colaboradores_tenant_id_cpf",
                schema: "rh",
                table: "colaboradores",
                columns: new[] { "tenant_id", "cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__colaborador_sync_id",
                schema: "rh",
                table: "colaboradores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__colaborador_tenant_id",
                schema: "rh",
                table: "colaboradores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_folhas_pagamento_tenant_id_colaborador_id_mes_competencia_a~",
                schema: "rh",
                table: "folhas_pagamento",
                columns: new[] { "tenant_id", "colaborador_id", "mes_competencia", "ano_competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__folha_pagamento_sync_id",
                schema: "rh",
                table: "folhas_pagamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__folha_pagamento_tenant_id",
                schema: "rh",
                table: "folhas_pagamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_folhas_pagamento_verbas_folha_pagamento_id",
                schema: "rh",
                table: "folhas_pagamento_verbas",
                column: "folha_pagamento_id");

            migrationBuilder.CreateIndex(
                name: "ix__folha_pagamento_verba_sync_id",
                schema: "rh",
                table: "folhas_pagamento_verbas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__folha_pagamento_verba_tenant_id",
                schema: "rh",
                table: "folhas_pagamento_verbas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_timesheets_tenant_id_colaborador_id_data",
                schema: "rh",
                table: "timesheets",
                columns: new[] { "tenant_id", "colaborador_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix__timesheet_sync_id",
                schema: "rh",
                table: "timesheets",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__timesheet_tenant_id",
                schema: "rh",
                table: "timesheets",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "colaboradores",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "folhas_pagamento_verbas",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "timesheets",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "folhas_pagamento",
                schema: "rh");
        }
    }
}
