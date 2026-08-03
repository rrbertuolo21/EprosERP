using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Manutencao.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmodulosConfiabilidadePreditiva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "observacao",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "hora_registro",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "numero_serie",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "setor",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "criticidade",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "manutencao",
                table: "equipamentos",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // Colunas de 'equipamentos' (descricao, estado_conservacao_id, funcao_operacional, local_id,
            // marca_id, numero_serie, responsavel_id, tipo_equipamento_id, versao) já foram adicionadas pela
            // migration anterior 20260725000000_AddSubmodulosPreventivaTrabalhoPecasParadasInducao.
            // AddColumn removido aqui para evitar colisão "column already exists" (fix aditivo/idempotente).

            migrationBuilder.CreateTable(
                name: "man_crv_evento_integracao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    destino_funcional = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    status_envio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tentativas = table.Column<int>(type: "integer", nullable: false),
                    ultimo_erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_man_crv_evento_integracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_parametro",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor_json = table.Column<string>(type: "jsonb", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_man_crv_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_revisao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    funcao_operacional = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado_conservacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    criticidade_operacional = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_submissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_aprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aprovador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    motivo_suspensao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    motivo_encerramento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_man_crv_revisao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_evento_integracao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade_origem = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entidade_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    direcao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mensagem_erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_man_pdt_evento_integracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_monitoramento",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_monitoramento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_parametro",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor_json = table.Column<string>(type: "jsonb", nullable: false),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_hora_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_anexo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    data_inclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_man_crv_anexo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_crv_anexo_man_crv_revisao_revisao_id",
                        column: x => x.revisao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_crv_revisao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_historico",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_origem = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    justificativa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("p_k_man_crv_historico", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_crv_historico_man_crv_revisao_revisao_id",
                        column: x => x.revisao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_crv_revisao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_indicador",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_indicador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    formula_aplicada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    origem_dados = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_calculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    calculado_por = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_man_crv_indicador", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_crv_indicador_man_crv_revisao_revisao_id",
                        column: x => x.revisao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_crv_revisao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_modo_falha",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    componente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    modo_falha = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    efeito_falha = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    causa_falha = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    controle_atual = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    severidade = table.Column<int>(type: "integer", nullable: true),
                    ocorrencia = table.Column<int>(type: "integer", nullable: true),
                    deteccao = table.Column<int>(type: "integer", nullable: true),
                    rpn = table.Column<int>(type: "integer", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_man_crv_modo_falha", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_crv_modo_falha_man_crv_revisao_revisao_id",
                        column: x => x.revisao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_crv_revisao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_crv_recomendacao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estrategia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    rpn_referencia = table.Column<int>(type: "integer", nullable: true),
                    mtbf_referencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    mttr_referencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    disponibilidade_referencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_recomendacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_man_crv_recomendacao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_crv_recomendacao_man_crv_revisao_revisao_id",
                        column: x => x.revisao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_crv_revisao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_alarme",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitoramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ponto_medicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    regra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leitura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora_disparo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    severidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    motivo_encerramento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("p_k_man_pdt_alarme", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_alarme_man_pdt_monitoramento_monitoramento_id",
                        column: x => x.monitoramento_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_monitoramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_anexo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitoramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_hora_vinculo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_anexo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_anexo_man_pdt_monitoramento_monitoramento_id",
                        column: x => x.monitoramento_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_monitoramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_historico",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitoramento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alarme_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_historico", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_historico_man_pdt_monitoramento_monitoramento_id",
                        column: x => x.monitoramento_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_monitoramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_item",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitoramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_man_pdt_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_item_man_pdt_monitoramento_monitoramento_id",
                        column: x => x.monitoramento_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_monitoramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_ponto_medicao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monitoramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_ponto = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    variavel = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    local_tecnico = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    periodicidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_ponto_medicao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_ponto_medicao_man_pdt_monitoramento_monitoramento_id",
                        column: x => x.monitoramento_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_monitoramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_ordem_trabalho_vinculo",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alarme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_trabalho_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora_solicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_retorno = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    payload_retorno = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("p_k_man_pdt_ordem_trabalho_vinculo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_ordem_trabalho_vinculo_man_pdt_alarme_alarme_id",
                        column: x => x.alarme_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_alarme",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_leitura_condicao",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ponto_medicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora_medicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    qualidade_dado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    origem = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("p_k_man_pdt_leitura_condicao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_leitura_condicao_man_pdt_ponto_medicao_ponto_medica~",
                        column: x => x.ponto_medicao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_ponto_medicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "man_pdt_regra_monitoramento",
                schema: "manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ponto_medicao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_regra = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    operador = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    limite_minimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    limite_maximo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    janela_avaliacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    severidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    acao_esperada = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_man_pdt_regra_monitoramento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_man_pdt_regra_monitoramento_man_pdt_ponto_medicao_ponto_med~",
                        column: x => x.ponto_medicao_id,
                        principalSchema: "manutencao",
                        principalTable: "man_pdt_ponto_medicao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_periodicidade_plano_id",
                schema: "manutencao",
                table: "man_prv_periodicidade",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_prv_kit_peca_plano_id",
                schema: "manutencao",
                table: "man_prv_kit_peca",
                column: "plano_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_anexo_revisao_id",
                schema: "manutencao",
                table: "man_crv_anexo",
                column: "revisao_id");

            migrationBuilder.CreateIndex(
                name: "ix__anexo_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anexo_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_evento_integracao_revisao_id_tipo_evento_status_env~",
                schema: "manutencao",
                table: "man_crv_evento_integracao",
                columns: new[] { "revisao_id", "tipo_evento", "status_envio" });

            migrationBuilder.CreateIndex(
                name: "ix__evento_integracao_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_evento_integracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__evento_integracao_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_evento_integracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_historico_revisao_id_data_hora",
                schema: "manutencao",
                table: "man_crv_historico",
                columns: new[] { "revisao_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_indicador_revisao_id_tipo_indicador",
                schema: "manutencao",
                table: "man_crv_indicador",
                columns: new[] { "revisao_id", "tipo_indicador" });

            migrationBuilder.CreateIndex(
                name: "ix__indicador_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__indicador_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_modo_falha_revisao_id_rpn",
                schema: "manutencao",
                table: "man_crv_modo_falha",
                columns: new[] { "revisao_id", "rpn" });

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_modo_falha_revisao_id_sequencia",
                schema: "manutencao",
                table: "man_crv_modo_falha",
                columns: new[] { "revisao_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__modo_falha_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_modo_falha",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__modo_falha_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_modo_falha",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_parametro_tenant_id_chave",
                schema: "manutencao",
                table: "man_crv_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_recomendacao_revisao_id_status",
                schema: "manutencao",
                table: "man_crv_recomendacao",
                columns: new[] { "revisao_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__recomendacao_estrategia_sync_id",
                schema: "manutencao",
                table: "man_crv_recomendacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__recomendacao_estrategia_tenant_id",
                schema: "manutencao",
                table: "man_crv_recomendacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_revisao_tenant_id_ativo_id",
                schema: "manutencao",
                table: "man_crv_revisao",
                columns: new[] { "tenant_id", "ativo_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_revisao_tenant_id_codigo",
                schema: "manutencao",
                table: "man_crv_revisao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_man_crv_revisao_tenant_id_status",
                schema: "manutencao",
                table: "man_crv_revisao",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__revisao_confiabilidade_sync_id",
                schema: "manutencao",
                table: "man_crv_revisao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__revisao_confiabilidade_tenant_id",
                schema: "manutencao",
                table: "man_crv_revisao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_alarme_monitoramento_id",
                schema: "manutencao",
                table: "man_pdt_alarme",
                column: "monitoramento_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_alarme_ponto_medicao_id",
                schema: "manutencao",
                table: "man_pdt_alarme",
                column: "ponto_medicao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_alarme_tenant_id_status_data_hora_disparo",
                schema: "manutencao",
                table: "man_pdt_alarme",
                columns: new[] { "tenant_id", "status", "data_hora_disparo" });

            migrationBuilder.CreateIndex(
                name: "ix__alarme_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_alarme",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__alarme_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_alarme",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_anexo_monitoramento_id",
                schema: "manutencao",
                table: "man_pdt_anexo",
                column: "monitoramento_id");

            migrationBuilder.CreateIndex(
                name: "ix__anexo_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anexo_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_evento_integracao_entidade_origem_entidade_origem_id",
                schema: "manutencao",
                table: "man_pdt_evento_integracao",
                columns: new[] { "entidade_origem", "entidade_origem_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_evento_integracao_tenant_id_tipo_evento_status",
                schema: "manutencao",
                table: "man_pdt_evento_integracao",
                columns: new[] { "tenant_id", "tipo_evento", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__evento_integracao_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_evento_integracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__evento_integracao_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_evento_integracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_historico_alarme_id",
                schema: "manutencao",
                table: "man_pdt_historico",
                column: "alarme_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_historico_monitoramento_id_data_hora",
                schema: "manutencao",
                table: "man_pdt_historico",
                columns: new[] { "monitoramento_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_item_monitoramento_id_sequencia",
                schema: "manutencao",
                table: "man_pdt_item",
                columns: new[] { "monitoramento_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__item_monitoramento_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_monitoramento_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_leitura_condicao_ponto_medicao_id_data_hora_medicao",
                schema: "manutencao",
                table: "man_pdt_leitura_condicao",
                columns: new[] { "ponto_medicao_id", "data_hora_medicao" });

            migrationBuilder.CreateIndex(
                name: "ix__leitura_condicao_sync_id",
                schema: "manutencao",
                table: "man_pdt_leitura_condicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__leitura_condicao_tenant_id",
                schema: "manutencao",
                table: "man_pdt_leitura_condicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_monitoramento_tenant_id_codigo",
                schema: "manutencao",
                table: "man_pdt_monitoramento",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_monitoramento_tenant_id_equipamento_id",
                schema: "manutencao",
                table: "man_pdt_monitoramento",
                columns: new[] { "tenant_id", "equipamento_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_monitoramento_tenant_id_status",
                schema: "manutencao",
                table: "man_pdt_monitoramento",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__monitoramento_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_monitoramento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__monitoramento_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_monitoramento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_ordem_trabalho_vinculo_alarme_id",
                schema: "manutencao",
                table: "man_pdt_ordem_trabalho_vinculo",
                column: "alarme_id");

            migrationBuilder.CreateIndex(
                name: "ix__vinculo_ordem_trabalho_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_ordem_trabalho_vinculo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__vinculo_ordem_trabalho_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_ordem_trabalho_vinculo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_parametro_tenant_id_chave",
                schema: "manutencao",
                table: "man_pdt_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_preditivo_sync_id",
                schema: "manutencao",
                table: "man_pdt_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_preditivo_tenant_id",
                schema: "manutencao",
                table: "man_pdt_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_ponto_medicao_monitoramento_id_variavel_local_tecni~",
                schema: "manutencao",
                table: "man_pdt_ponto_medicao",
                columns: new[] { "monitoramento_id", "variavel", "local_tecnico" });

            migrationBuilder.CreateIndex(
                name: "ix__ponto_medicao_sync_id",
                schema: "manutencao",
                table: "man_pdt_ponto_medicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ponto_medicao_tenant_id",
                schema: "manutencao",
                table: "man_pdt_ponto_medicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_man_pdt_regra_monitoramento_ponto_medicao_id_situacao",
                schema: "manutencao",
                table: "man_pdt_regra_monitoramento",
                columns: new[] { "ponto_medicao_id", "situacao" });

            migrationBuilder.CreateIndex(
                name: "ix__regra_monitoramento_sync_id",
                schema: "manutencao",
                table: "man_pdt_regra_monitoramento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__regra_monitoramento_tenant_id",
                schema: "manutencao",
                table: "man_pdt_regra_monitoramento",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_man_par_indicador_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_indicador",
                column: "parada_id",
                principalSchema: "manutencao",
                principalTable: "man_par_parada",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_par_item_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_item",
                column: "parada_id",
                principalSchema: "manutencao",
                principalTable: "man_par_parada",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_par_vinculo_os_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_vinculo_os",
                column: "parada_id",
                principalSchema: "manutencao",
                principalTable: "man_par_parada",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_pec_item_man_pec_registro_registro_id",
                schema: "manutencao",
                table: "man_pec_item",
                column: "registro_id",
                principalSchema: "manutencao",
                principalTable: "man_pec_registro",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_pec_movimento_man_pec_item_item_id",
                schema: "manutencao",
                table: "man_pec_movimento",
                column: "item_id",
                principalSchema: "manutencao",
                principalTable: "man_pec_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_pec_reserva_man_pec_item_item_id",
                schema: "manutencao",
                table: "man_pec_reserva",
                column: "item_id",
                principalSchema: "manutencao",
                principalTable: "man_pec_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_prv_checklist_item_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_checklist_item",
                column: "plano_id",
                principalSchema: "manutencao",
                principalTable: "man_prv_plano",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_prv_execucao_programada_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_execucao_programada",
                column: "plano_id",
                principalSchema: "manutencao",
                principalTable: "man_prv_plano",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_prv_kit_peca_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_kit_peca",
                column: "plano_id",
                principalSchema: "manutencao",
                principalTable: "man_prv_plano",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_prv_periodicidade_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_periodicidade",
                column: "plano_id",
                principalSchema: "manutencao",
                principalTable: "man_prv_plano",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_trb_os_equipamento_man_trb_ordem_servico_ordem_servico_~",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                column: "ordem_servico_id",
                principalSchema: "manutencao",
                principalTable: "man_trb_ordem_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_trb_os_evolucao_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                column: "ordem_servico_id",
                principalSchema: "manutencao",
                principalTable: "man_trb_ordem_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_trb_os_financeiro_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_financeiro",
                column: "ordem_servico_id",
                principalSchema: "manutencao",
                principalTable: "man_trb_ordem_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "f_k_man_trb_os_item_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_item",
                column: "ordem_servico_id",
                principalSchema: "manutencao",
                principalTable: "man_trb_ordem_servico",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_man_par_indicador_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_indicador");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_par_item_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_item");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_par_vinculo_os_man_par_parada_parada_id",
                schema: "manutencao",
                table: "man_par_vinculo_os");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_pec_item_man_pec_registro_registro_id",
                schema: "manutencao",
                table: "man_pec_item");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_pec_movimento_man_pec_item_item_id",
                schema: "manutencao",
                table: "man_pec_movimento");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_pec_reserva_man_pec_item_item_id",
                schema: "manutencao",
                table: "man_pec_reserva");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_prv_checklist_item_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_checklist_item");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_prv_execucao_programada_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_execucao_programada");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_prv_kit_peca_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_kit_peca");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_prv_periodicidade_man_prv_plano_plano_id",
                schema: "manutencao",
                table: "man_prv_periodicidade");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_trb_os_equipamento_man_trb_ordem_servico_ordem_servico_~",
                schema: "manutencao",
                table: "man_trb_os_equipamento");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_trb_os_evolucao_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_evolucao");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_trb_os_financeiro_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_financeiro");

            migrationBuilder.DropForeignKey(
                name: "f_k_man_trb_os_item_man_trb_ordem_servico_ordem_servico_id",
                schema: "manutencao",
                table: "man_trb_os_item");

            migrationBuilder.DropTable(
                name: "man_crv_anexo",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_evento_integracao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_historico",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_indicador",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_modo_falha",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_parametro",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_recomendacao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_anexo",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_evento_integracao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_historico",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_item",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_leitura_condicao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_ordem_trabalho_vinculo",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_parametro",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_regra_monitoramento",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_crv_revisao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_alarme",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_ponto_medicao",
                schema: "manutencao");

            migrationBuilder.DropTable(
                name: "man_pdt_monitoramento",
                schema: "manutencao");

            migrationBuilder.DropIndex(
                name: "i_x_man_prv_periodicidade_plano_id",
                schema: "manutencao",
                table: "man_prv_periodicidade");

            migrationBuilder.DropIndex(
                name: "i_x_man_prv_kit_peca_plano_id",
                schema: "manutencao",
                table: "man_prv_kit_peca");

            // DropColumn de 'equipamentos' removido: essas colunas pertencem à migration
            // 20260725000000 e não devem ser revertidas por esta (par simétrico do Up acima).

            migrationBuilder.AlterColumn<string>(
                name: "observacao",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "hora_registro",
                schema: "manutencao",
                table: "man_trb_os_evolucao",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "numero_serie",
                schema: "manutencao",
                table: "man_trb_os_equipamento",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "manutencao",
                table: "equipamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "setor",
                schema: "manutencao",
                table: "equipamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "nome",
                schema: "manutencao",
                table: "equipamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "criticidade",
                schema: "manutencao",
                table: "equipamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "manutencao",
                table: "equipamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);
        }
    }
}
