using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddParametrosOperacionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "currency_id",
                schema: "plataforma",
                table: "empresas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "date_format",
                schema: "plataforma",
                table: "empresas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "time_zone_id",
                schema: "plataforma",
                table: "empresas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "armazens",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    pais = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_armazens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    added_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_email",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    host = table.Column<string>(type: "text", nullable: true),
                    port = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true),
                    from_email = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_configuracoes_email", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exercicios_financeiros",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    to_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fiscal_year = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_exercicios_financeiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fusos_horarios",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    offset = table.Column<string>(type: "text", nullable: false),
                    codigo_i_a_n_a = table.Column<string>(type: "text", nullable: true),
                    fuso_horario_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_fusos_horarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "impostos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_impostos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "logs_auditoria_configuracao",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campo = table.Column<string>(type: "text", nullable: false),
                    valor_anterior = table.Column<string>(type: "text", nullable: true),
                    valor_novo = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_logs_auditoria_configuracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moedas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_i_s_o = table.Column<string>(type: "text", nullable: false),
                    simbolo = table.Column<string>(type: "text", nullable: false),
                    casas_decimais = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_moedas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "preferencias_gerais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    show_currency = table.Column<bool>(type: "boolean", nullable: false),
                    negative_cash = table.Column<bool>(type: "boolean", nullable: false),
                    negative_stock = table.Column<bool>(type: "boolean", nullable: false),
                    stock_calculation_mode = table.Column<string>(type: "text", nullable: false),
                    credit_limit = table.Column<bool>(type: "boolean", nullable: false),
                    discount = table.Column<bool>(type: "boolean", nullable: false),
                    vat_on_purchase = table.Column<bool>(type: "boolean", nullable: false),
                    vat_on_sales = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_preferencias_gerais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projetos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_projetos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unidades_medida",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    codigo_u_n_e_c_e = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_unidades_medida", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "upgrades_planos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    order_no = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_upgrades_planos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_upgrades_planos_planos_plano_id",
                        column: x => x.plano_id,
                        principalSchema: "plataforma",
                        principalTable: "planos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conversoes_unidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fator = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_conversoes_unidades", x => x.id);
                    table.ForeignKey(
                        name: "f_k_conversoes_unidades__unidades_medida_unidade_destino_id",
                        column: x => x.unidade_destino_id,
                        principalSchema: "plataforma",
                        principalTable: "unidades_medida",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_conversoes_unidades__unidades_medida_unidade_origem_id",
                        column: x => x.unidade_origem_id,
                        principalSchema: "plataforma",
                        principalTable: "unidades_medida",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_empresas_currency_id",
                schema: "plataforma",
                table: "empresas",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "i_x_empresas_time_zone_id",
                schema: "plataforma",
                table: "empresas",
                column: "time_zone_id");

            migrationBuilder.CreateIndex(
                name: "ix__armazem_sync_id",
                schema: "plataforma",
                table: "armazens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__armazem_tenant_id",
                schema: "plataforma",
                table: "armazens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_armazens_tenant_nome",
                schema: "plataforma",
                table: "armazens",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__categoria_sync_id",
                schema: "plataforma",
                table: "categorias",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__categoria_tenant_id",
                schema: "plataforma",
                table: "categorias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_tenant_nome",
                schema: "plataforma",
                table: "categorias",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_email_sync_id",
                schema: "plataforma",
                table: "configuracoes_email",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__configuracao_email_tenant_id",
                schema: "plataforma",
                table: "configuracoes_email",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_conversoes_unidades_unidade_destino_id",
                schema: "plataforma",
                table: "conversoes_unidades",
                column: "unidade_destino_id");

            migrationBuilder.CreateIndex(
                name: "i_x_conversoes_unidades_unidade_origem_id",
                schema: "plataforma",
                table: "conversoes_unidades",
                column: "unidade_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix__conversao_unidade_sync_id",
                schema: "plataforma",
                table: "conversoes_unidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conversao_unidade_tenant_id",
                schema: "plataforma",
                table: "conversoes_unidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversoes_unidades_tenant_origem_destino",
                schema: "plataforma",
                table: "conversoes_unidades",
                columns: new[] { "tenant_id", "unidade_origem_id", "unidade_destino_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__exercicio_financeiro_sync_id",
                schema: "plataforma",
                table: "exercicios_financeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__exercicio_financeiro_tenant_id",
                schema: "plataforma",
                table: "exercicios_financeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fuso_horario_sync_id",
                schema: "plataforma",
                table: "fusos_horarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fuso_horario_tenant_id",
                schema: "plataforma",
                table: "fusos_horarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_fusos_horarios_fuso_id",
                schema: "plataforma",
                table: "fusos_horarios",
                column: "fuso_horario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imposto_sync_id",
                schema: "plataforma",
                table: "impostos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imposto_tenant_id",
                schema: "plataforma",
                table: "impostos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_impostos_tenant_nome",
                schema: "plataforma",
                table: "impostos",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__log_auditoria_configuracao_sync_id",
                schema: "plataforma",
                table: "logs_auditoria_configuracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__log_auditoria_configuracao_tenant_id",
                schema: "plataforma",
                table: "logs_auditoria_configuracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__moeda_sync_id",
                schema: "plataforma",
                table: "moedas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__moeda_tenant_id",
                schema: "plataforma",
                table: "moedas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_moedas_codigo_iso",
                schema: "plataforma",
                table: "moedas",
                column: "codigo_i_s_o",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__preferencia_geral_sync_id",
                schema: "plataforma",
                table: "preferencias_gerais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__preferencia_geral_tenant_id",
                schema: "plataforma",
                table: "preferencias_gerais",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_sync_id",
                schema: "plataforma",
                table: "projetos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_tenant_id",
                schema: "plataforma",
                table: "projetos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_projetos_tenant_nome",
                schema: "plataforma",
                table: "projetos",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_sync_id",
                schema: "plataforma",
                table: "unidades_medida",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__unidade_medida_tenant_id",
                schema: "plataforma",
                table: "unidades_medida",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidades_medida_tenant_nome",
                schema: "plataforma",
                table: "unidades_medida",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_upgrades_planos_plano_id",
                schema: "plataforma",
                table: "upgrades_planos",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "ix__upgrade_plano_sync_id",
                schema: "plataforma",
                table: "upgrades_planos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__upgrade_plano_tenant_id",
                schema: "plataforma",
                table: "upgrades_planos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_upgrades_planos_order_no",
                schema: "plataforma",
                table: "upgrades_planos",
                column: "order_no",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "f_k_empresas__fusos_horarios_time_zone_id",
                schema: "plataforma",
                table: "empresas",
                column: "time_zone_id",
                principalSchema: "plataforma",
                principalTable: "fusos_horarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "f_k_empresas__moedas_currency_id",
                schema: "plataforma",
                table: "empresas",
                column: "currency_id",
                principalSchema: "plataforma",
                principalTable: "moedas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Seed de Moedas
            migrationBuilder.Sql(@"
                INSERT INTO plataforma.moedas (id, codigo_i_s_o, simbolo, casas_decimais, sync_id, tenant_id, sync_version, criado_em)
                VALUES 
                    ('" + Guid.NewGuid() + @"', 'BRL', 'R$', 2, '" + Guid.NewGuid() + @"', 'system', 1, NOW()),
                    ('" + Guid.NewGuid() + @"', 'USD', '$', 2, '" + Guid.NewGuid() + @"', 'system', 1, NOW()),
                    ('" + Guid.NewGuid() + @"', 'EUR', '€', 2, '" + Guid.NewGuid() + @"', 'system', 1, NOW())
                ON CONFLICT (codigo_i_s_o) DO NOTHING;
            ");

            // Seed de Fusos Horários
            migrationBuilder.Sql(@"
                INSERT INTO plataforma.fusos_horarios (id, nome, ""offset"", codigo_i_a_n_a, fuso_horario_id, sync_id, tenant_id, sync_version, criado_em)
                VALUES 
                    ('" + Guid.NewGuid() + @"', 'Brasília (GMT-3)', 'GMT-3', 'America/Sao_Paulo', 1, '" + Guid.NewGuid() + @"', 'system', 1, NOW()),
                    ('" + Guid.NewGuid() + @"', 'Manaus (GMT-4)', 'GMT-4', 'America/Manaus', 2, '" + Guid.NewGuid() + @"', 'system', 1, NOW()),
                    ('" + Guid.NewGuid() + @"', 'Fernando de Noronha (GMT-2)', 'GMT-2', 'America/Noronha', 3, '" + Guid.NewGuid() + @"', 'system', 1, NOW()),
                    ('" + Guid.NewGuid() + @"', 'Rio Branco (GMT-5)', 'GMT-5', 'America/Rio_Branco', 4, '" + Guid.NewGuid() + @"', 'system', 1, NOW())
                ON CONFLICT (fuso_horario_id) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_empresas__fusos_horarios_time_zone_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropForeignKey(
                name: "f_k_empresas__moedas_currency_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropTable(
                name: "armazens",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "categorias",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "configuracoes_email",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "conversoes_unidades",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "exercicios_financeiros",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "fusos_horarios",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "impostos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "logs_auditoria_configuracao",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "moedas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "preferencias_gerais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "projetos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "upgrades_planos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "unidades_medida",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "i_x_empresas_currency_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropIndex(
                name: "i_x_empresas_time_zone_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "currency_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "date_format",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "time_zone_id",
                schema: "plataforma",
                table: "empresas");
        }
    }
}
