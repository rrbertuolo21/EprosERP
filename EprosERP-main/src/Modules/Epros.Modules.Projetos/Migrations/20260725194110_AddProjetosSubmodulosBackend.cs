using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Projetos.Migrations
{
    /// <inheritdoc />
    public partial class AddProjetosSubmodulosBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "projetos",
                table: "wbs_itens",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "projetos",
                table: "wbs_itens",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "projetos",
                table: "projetos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "projetos",
                table: "projetos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "funcao",
                schema: "projetos",
                table: "alocacoes",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "prj_def_projeto_arquivo",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    caminho_arquivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_prj_def_projeto_arquivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_def_projeto_atividade",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tipo_atividade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_def_projeto_atividade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_def_projeto_cliente",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prj_def_projeto_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_def_projeto_membro",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("p_k_prj_def_projeto_membro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_def_tarefa_modelo",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tarefa_sequencial = table.Column<int>(type: "integer", nullable: false),
                    tarefa_pai_sequencial = table.Column<int>(type: "integer", nullable: true),
                    duracao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_duracao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    percentual_completo = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_def_tarefa_modelo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_faturamento_projeto",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    modalidade_faturamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    moeda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_faturamento_projeto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_orcamento_projeto",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    billing_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    billing_rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    estimated_hours = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    costs_estimate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_orcamento_projeto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_recurso_alocacao",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    papel_no_projeto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    carga_planejada_horas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_prj_recurso_alocacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_recurso_timesheet",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tarefa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    horas = table.Column<int>(type: "integer", nullable: false),
                    minutos = table.Column<int>(type: "integer", nullable: false),
                    notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_recurso_timesheet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_dependencia",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_dependente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_predecessora_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_dependencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bloqueada = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_rst_dependencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_estagio",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    indicador_conclusao = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_rst_estagio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_reuniao",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    departamento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    local = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    organizador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    relator_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_prj_rst_reuniao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_subtarefa",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_prevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concluido = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_prj_rst_subtarefa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_tarefa",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    estagio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    prioridade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_termino = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duracao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    esforco_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    esforco_realizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_concluido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tarefa_superior_id = table.Column<Guid>(type: "uuid", nullable: true),
                    indicador_marco = table.Column<bool>(type: "boolean", nullable: false),
                    visibilidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_rst_tarefa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_rst_timer",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duracao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_registro = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_rst_timer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_faturamento_projeto_item",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    faturamento_projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    tipo_item = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    origem_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_prj_faturamento_projeto_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_faturamento_projeto_item_prj_faturamento_projeto_fatura~",
                        column: x => x.faturamento_projeto_id,
                        principalSchema: "projetos",
                        principalTable: "prj_faturamento_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_orcamento_marco",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orcamento_projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    custo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resumo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    progresso = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_prj_orcamento_marco", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_orcamento_marco_prj_orcamento_projeto_orcamento_projeto~",
                        column: x => x.orcamento_projeto_id,
                        principalSchema: "projetos",
                        principalTable: "prj_orcamento_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prj_def_projeto_arquivo_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_def_projeto_arquivo",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_arquivo_sync_id",
                schema: "projetos",
                table: "prj_def_projeto_arquivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_arquivo_tenant_id",
                schema: "projetos",
                table: "prj_def_projeto_arquivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_def_projeto_atividade_tenant_id_projeto_id_tipo_ativida~",
                schema: "projetos",
                table: "prj_def_projeto_atividade",
                columns: new[] { "tenant_id", "projeto_id", "tipo_atividade" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_atividade_sync_id",
                schema: "projetos",
                table: "prj_def_projeto_atividade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_atividade_tenant_id",
                schema: "projetos",
                table: "prj_def_projeto_atividade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_def_projeto_cliente_tenant_id_projeto_id_cliente_id",
                schema: "projetos",
                table: "prj_def_projeto_cliente",
                columns: new[] { "tenant_id", "projeto_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_cliente_sync_id",
                schema: "projetos",
                table: "prj_def_projeto_cliente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_cliente_tenant_id",
                schema: "projetos",
                table: "prj_def_projeto_cliente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_def_projeto_membro_tenant_id_projeto_id_usuario_id",
                schema: "projetos",
                table: "prj_def_projeto_membro",
                columns: new[] { "tenant_id", "projeto_id", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_membro_sync_id",
                schema: "projetos",
                table: "prj_def_projeto_membro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_membro_tenant_id",
                schema: "projetos",
                table: "prj_def_projeto_membro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_def_tarefa_modelo_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_def_tarefa_modelo",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__projeto_tarefa_modelo_sync_id",
                schema: "projetos",
                table: "prj_def_tarefa_modelo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__projeto_tarefa_modelo_tenant_id",
                schema: "projetos",
                table: "prj_def_tarefa_modelo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_faturamento_projeto_tenant_id_codigo",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prj_faturamento_projeto_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__faturamento_projeto_sync_id",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__faturamento_projeto_tenant_id",
                schema: "projetos",
                table: "prj_faturamento_projeto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_faturamento_projeto_item_faturamento_projeto_id",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                column: "faturamento_projeto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_faturamento_projeto_item_tenant_id_faturamento_projeto_~",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                columns: new[] { "tenant_id", "faturamento_projeto_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__item_faturamento_projeto_sync_id",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_faturamento_projeto_tenant_id",
                schema: "projetos",
                table: "prj_faturamento_projeto_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_orcamento_marco_orcamento_projeto_id",
                schema: "projetos",
                table: "prj_orcamento_marco",
                column: "orcamento_projeto_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_orcamento_marco_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_orcamento_marco",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__marco_orcamentario_sync_id",
                schema: "projetos",
                table: "prj_orcamento_marco",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__marco_orcamentario_tenant_id",
                schema: "projetos",
                table: "prj_orcamento_marco",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_orcamento_projeto_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_orcamento_projeto",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_projeto_sync_id",
                schema: "projetos",
                table: "prj_orcamento_projeto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_projeto_tenant_id",
                schema: "projetos",
                table: "prj_orcamento_projeto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_recurso_alocacao_tenant_id_recurso_id_projeto_id",
                schema: "projetos",
                table: "prj_recurso_alocacao",
                columns: new[] { "tenant_id", "recurso_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__recurso_alocacao_sync_id",
                schema: "projetos",
                table: "prj_recurso_alocacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recurso_alocacao_tenant_id",
                schema: "projetos",
                table: "prj_recurso_alocacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_recurso_timesheet_tenant_id_projeto_id_tarefa_id",
                schema: "projetos",
                table: "prj_recurso_timesheet",
                columns: new[] { "tenant_id", "projeto_id", "tarefa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_prj_recurso_timesheet_tenant_id_usuario_id_data",
                schema: "projetos",
                table: "prj_recurso_timesheet",
                columns: new[] { "tenant_id", "usuario_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix__recurso_timesheet_sync_id",
                schema: "projetos",
                table: "prj_recurso_timesheet",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recurso_timesheet_tenant_id",
                schema: "projetos",
                table: "prj_recurso_timesheet",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_dependencia_tenant_id_tarefa_dependente_id",
                schema: "projetos",
                table: "prj_rst_dependencia",
                columns: new[] { "tenant_id", "tarefa_dependente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__dependencia_tarefa_sync_id",
                schema: "projetos",
                table: "prj_rst_dependencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dependencia_tarefa_tenant_id",
                schema: "projetos",
                table: "prj_rst_dependencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_estagio_tenant_id_ordem",
                schema: "projetos",
                table: "prj_rst_estagio",
                columns: new[] { "tenant_id", "ordem" });

            migrationBuilder.CreateIndex(
                name: "ix__estagio_tarefa_sync_id",
                schema: "projetos",
                table: "prj_rst_estagio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estagio_tarefa_tenant_id",
                schema: "projetos",
                table: "prj_rst_estagio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_reuniao_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_rst_reuniao",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__reuniao_acompanhamento_sync_id",
                schema: "projetos",
                table: "prj_rst_reuniao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reuniao_acompanhamento_tenant_id",
                schema: "projetos",
                table: "prj_rst_reuniao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_subtarefa_tenant_id_tarefa_id",
                schema: "projetos",
                table: "prj_rst_subtarefa",
                columns: new[] { "tenant_id", "tarefa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__subtarefa_checklist_sync_id",
                schema: "projetos",
                table: "prj_rst_subtarefa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__subtarefa_checklist_tenant_id",
                schema: "projetos",
                table: "prj_rst_subtarefa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_tarefa_tenant_id_projeto_id_estagio_id",
                schema: "projetos",
                table: "prj_rst_tarefa",
                columns: new[] { "tenant_id", "projeto_id", "estagio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__tarefa_projeto_sync_id",
                schema: "projetos",
                table: "prj_rst_tarefa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tarefa_projeto_tenant_id",
                schema: "projetos",
                table: "prj_rst_tarefa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_rst_timer_tenant_id_tarefa_id",
                schema: "projetos",
                table: "prj_rst_timer",
                columns: new[] { "tenant_id", "tarefa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__timer_operacional_sync_id",
                schema: "projetos",
                table: "prj_rst_timer",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__timer_operacional_tenant_id",
                schema: "projetos",
                table: "prj_rst_timer",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prj_def_projeto_arquivo",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_def_projeto_atividade",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_def_projeto_cliente",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_def_projeto_membro",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_def_tarefa_modelo",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_faturamento_projeto_item",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_orcamento_marco",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_recurso_alocacao",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_recurso_timesheet",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_dependencia",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_estagio",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_reuniao",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_subtarefa",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_tarefa",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_rst_timer",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_faturamento_projeto",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_orcamento_projeto",
                schema: "projetos");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "projetos",
                table: "wbs_itens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "projetos",
                table: "wbs_itens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "projetos",
                table: "projetos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "projetos",
                table: "projetos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "funcao",
                schema: "projetos",
                table: "alocacoes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);
        }
    }
}
