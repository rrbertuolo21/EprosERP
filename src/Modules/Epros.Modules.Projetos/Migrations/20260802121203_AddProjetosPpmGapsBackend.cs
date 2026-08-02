using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Projetos.Migrations
{
    /// <inheritdoc />
    public partial class AddProjetosPpmGapsBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MM-a: migra os rótulos de status legados (string inventada) para o enum canônico (T3).
            // Idempotente e seguro para bases sem dados legados.
            migrationBuilder.Sql(@"UPDATE projetos.projetos SET status = CASE status
                WHEN 'Planejado' THEN 'Rascunho'
                WHEN 'EmAndamento' THEN 'Ativo'
                WHEN 'Concluido' THEN 'Encerrado'
                ELSE status END
                WHERE status IN ('Planejado','EmAndamento','Concluido');");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "wbs_itens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "projetos",
                table: "projetos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "projetos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_timer",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_tarefa",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_subtarefa",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_reuniao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_estagio",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_dependencia",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_responsavel",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_projeto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_estagio",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_comentario",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_recurso_timesheet",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_recurso_alocacao",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_item",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "baseline_atual",
                schema: "projetos",
                table: "prj_orcamento_projeto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_orcamento_projeto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_orcamento_marco",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "reembolsavel",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_cofins",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_csll",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_inss",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_irrf",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_iss",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_liquido",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_pis",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_retencoes",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_parametro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_item",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_historico",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_tarefa_modelo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_membro",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_cliente",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_atividade",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_arquivo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "projetos",
                table: "alocacoes",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "prj_orcamento_baseline",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orcamento_projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_baseline = table.Column<int>(type: "integer", nullable: false),
                    budget_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    custo_marcos_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    marcos_count = table.Column<int>(type: "integer", nullable: false),
                    marcos_snapshot_json = table.Column<string>(type: "text", nullable: false),
                    data_congelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prj_orcamento_baseline", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_programa",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_prj_programa", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prj_orcamento_baseline_tenant_id_orcamento_projeto_id_numer~",
                schema: "projetos",
                table: "prj_orcamento_baseline",
                columns: new[] { "tenant_id", "orcamento_projeto_id", "numero_baseline" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__baseline_orcamento_sync_id",
                schema: "projetos",
                table: "prj_orcamento_baseline",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__baseline_orcamento_tenant_id",
                schema: "projetos",
                table: "prj_orcamento_baseline",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_programa_tenant_id_codigo",
                schema: "projetos",
                table: "prj_programa",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prj_programa_tenant_id_portfolio_id",
                schema: "projetos",
                table: "prj_programa",
                columns: new[] { "tenant_id", "portfolio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__programa_sync_id",
                schema: "projetos",
                table: "prj_programa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__programa_tenant_id",
                schema: "projetos",
                table: "prj_programa",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prj_orcamento_baseline",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_programa",
                schema: "projetos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "wbs_itens");

            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "projetos",
                table: "projetos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "projetos");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_timer");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_tarefa");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_subtarefa");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_reuniao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_estagio");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_rst_dependencia");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_responsavel");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_projeto");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_estagio");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_comentario");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_risco_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_recurso_timesheet");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_recurso_alocacao");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_item");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_portfolio");

            migrationBuilder.DropColumn(
                name: "baseline_atual",
                schema: "projetos",
                table: "prj_orcamento_projeto");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_orcamento_projeto");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_orcamento_marco");

            migrationBuilder.DropColumn(
                name: "reembolsavel",
                schema: "projetos",
                table: "prj_faturamento_projeto_item");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_faturamento_projeto_item");

            migrationBuilder.DropColumn(
                name: "valor_cofins",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_csll",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_inss",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_irrf",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_iss",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_liquido",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_pis",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "valor_retencoes",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_faturamento_projeto");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_parametro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_item");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_historico");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_enc_encerramento");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_tarefa_modelo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_membro");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_cliente");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_atividade");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "prj_def_projeto_arquivo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "projetos",
                table: "alocacoes");
        }
    }
}
