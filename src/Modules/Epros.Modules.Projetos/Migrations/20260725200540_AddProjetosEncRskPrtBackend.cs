using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Projetos.Migrations
{
    /// <inheritdoc />
    public partial class AddProjetosEncRskPrtBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prj_enc_encerramento",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_final_projeto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prj_enc_encerramento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_enc_parametro",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prj_enc_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_portfolio",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_portfolio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    justificativa = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    score_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prj_portfolio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_portfolio_parametro",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prj_portfolio_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_estagio",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    completo = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    criador_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_prj_risco_estagio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_parametro",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_prj_risco_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_projeto",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    prioridade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    estagio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    probabilidade = table.Column<int>(type: "integer", nullable: true),
                    impacto = table.Column<int>(type: "integer", nullable: true),
                    resposta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    risco_residual = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_risco_projeto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prj_enc_encerramento_anexo",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    encerramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prj_enc_encerramento_anexo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_enc_encerramento_anexo_prj_enc_encerramento_encerrament~",
                        column: x => x.encerramento_id,
                        principalSchema: "projetos",
                        principalTable: "prj_enc_encerramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_enc_encerramento_historico",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    encerramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prj_enc_encerramento_historico", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_enc_encerramento_historico_prj_enc_encerramento_encerra~",
                        column: x => x.encerramento_id,
                        principalSchema: "projetos",
                        principalTable: "prj_enc_encerramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_enc_encerramento_item",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    encerramento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_prj_enc_encerramento_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_enc_encerramento_item_prj_enc_encerramento_encerramento~",
                        column: x => x.encerramento_id,
                        principalSchema: "projetos",
                        principalTable: "prj_enc_encerramento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_portfolio_anexo",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_anexo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_prj_portfolio_anexo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_portfolio_anexo_prj_portfolio_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "projetos",
                        principalTable: "prj_portfolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_portfolio_historico",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    motivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prj_portfolio_historico", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_portfolio_historico_prj_portfolio_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "projetos",
                        principalTable: "prj_portfolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_portfolio_item",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    tipo_item = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    programa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    valor_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    esforco_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    capacidade_requerida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    npv = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    payback = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    alinhamento_estrategico = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    risco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    score = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    justificativa_prioridade = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    observacao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("p_k_prj_portfolio_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_portfolio_item_prj_portfolio_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "projetos",
                        principalTable: "prj_portfolio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_anexo",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("p_k_prj_risco_anexo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_risco_anexo_prj_risco_projeto_risco_id",
                        column: x => x.risco_id,
                        principalSchema: "projetos",
                        principalTable: "prj_risco_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_comentario",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comentario = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prj_risco_comentario", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_risco_comentario_prj_risco_projeto_risco_id",
                        column: x => x.risco_id,
                        principalSchema: "projetos",
                        principalTable: "prj_risco_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_historico",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    ip = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_prj_risco_historico", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_risco_historico_prj_risco_projeto_risco_id",
                        column: x => x.risco_id,
                        principalSchema: "projetos",
                        principalTable: "prj_risco_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prj_risco_responsavel",
                schema: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_prj_risco_responsavel", x => x.id);
                    table.ForeignKey(
                        name: "f_k_prj_risco_responsavel_prj_risco_projeto_risco_id",
                        column: x => x.risco_id,
                        principalSchema: "projetos",
                        principalTable: "prj_risco_projeto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_tenant_id_codigo",
                schema: "projetos",
                table: "prj_enc_encerramento",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_tenant_id_projeto_id",
                schema: "projetos",
                table: "prj_enc_encerramento",
                columns: new[] { "tenant_id", "projeto_id" });

            migrationBuilder.CreateIndex(
                name: "ix__encerramento_projeto_sync_id",
                schema: "projetos",
                table: "prj_enc_encerramento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__encerramento_projeto_tenant_id",
                schema: "projetos",
                table: "prj_enc_encerramento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_anexo_encerramento_id",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo",
                column: "encerramento_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_anexo_tenant_id_encerramento_id",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo",
                columns: new[] { "tenant_id", "encerramento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__anexo_encerramento_sync_id",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anexo_encerramento_tenant_id",
                schema: "projetos",
                table: "prj_enc_encerramento_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_historico_encerramento_id",
                schema: "projetos",
                table: "prj_enc_encerramento_historico",
                column: "encerramento_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_historico_tenant_id_encerramento_id",
                schema: "projetos",
                table: "prj_enc_encerramento_historico",
                columns: new[] { "tenant_id", "encerramento_id" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_encerramento_sync_id",
                schema: "projetos",
                table: "prj_enc_encerramento_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_encerramento_tenant_id",
                schema: "projetos",
                table: "prj_enc_encerramento_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_item_encerramento_id",
                schema: "projetos",
                table: "prj_enc_encerramento_item",
                column: "encerramento_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_encerramento_item_tenant_id_encerramento_id_sequenc~",
                schema: "projetos",
                table: "prj_enc_encerramento_item",
                columns: new[] { "tenant_id", "encerramento_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__item_encerramento_sync_id",
                schema: "projetos",
                table: "prj_enc_encerramento_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_encerramento_tenant_id",
                schema: "projetos",
                table: "prj_enc_encerramento_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_enc_parametro_tenant_id_chave",
                schema: "projetos",
                table: "prj_enc_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_encerramento_sync_id",
                schema: "projetos",
                table: "prj_enc_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_encerramento_tenant_id",
                schema: "projetos",
                table: "prj_enc_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_tenant_id_codigo",
                schema: "projetos",
                table: "prj_portfolio",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portfolio_sync_id",
                schema: "projetos",
                table: "prj_portfolio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portfolio_tenant_id",
                schema: "projetos",
                table: "prj_portfolio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_anexo_portfolio_id",
                schema: "projetos",
                table: "prj_portfolio_anexo",
                column: "portfolio_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_anexo_tenant_id_portfolio_id",
                schema: "projetos",
                table: "prj_portfolio_anexo",
                columns: new[] { "tenant_id", "portfolio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__anexo_portfolio_sync_id",
                schema: "projetos",
                table: "prj_portfolio_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anexo_portfolio_tenant_id",
                schema: "projetos",
                table: "prj_portfolio_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_historico_portfolio_id",
                schema: "projetos",
                table: "prj_portfolio_historico",
                column: "portfolio_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_historico_tenant_id_portfolio_id",
                schema: "projetos",
                table: "prj_portfolio_historico",
                columns: new[] { "tenant_id", "portfolio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_portfolio_sync_id",
                schema: "projetos",
                table: "prj_portfolio_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_portfolio_tenant_id",
                schema: "projetos",
                table: "prj_portfolio_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_item_portfolio_id",
                schema: "projetos",
                table: "prj_portfolio_item",
                column: "portfolio_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_item_tenant_id_portfolio_id_sequencia",
                schema: "projetos",
                table: "prj_portfolio_item",
                columns: new[] { "tenant_id", "portfolio_id", "sequencia" });

            migrationBuilder.CreateIndex(
                name: "ix__portfolio_item_sync_id",
                schema: "projetos",
                table: "prj_portfolio_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__portfolio_item_tenant_id",
                schema: "projetos",
                table: "prj_portfolio_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_portfolio_parametro_tenant_id_chave",
                schema: "projetos",
                table: "prj_portfolio_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_portfolio_sync_id",
                schema: "projetos",
                table: "prj_portfolio_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_portfolio_tenant_id",
                schema: "projetos",
                table: "prj_portfolio_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_anexo_risco_id",
                schema: "projetos",
                table: "prj_risco_anexo",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_anexo_tenant_id_risco_id",
                schema: "projetos",
                table: "prj_risco_anexo",
                columns: new[] { "tenant_id", "risco_id" });

            migrationBuilder.CreateIndex(
                name: "ix__anexo_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anexo_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_comentario_risco_id",
                schema: "projetos",
                table: "prj_risco_comentario",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_comentario_tenant_id_risco_id",
                schema: "projetos",
                table: "prj_risco_comentario",
                columns: new[] { "tenant_id", "risco_id" });

            migrationBuilder.CreateIndex(
                name: "ix__comentario_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_comentario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__comentario_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_comentario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_estagio_tenant_id_ordem",
                schema: "projetos",
                table: "prj_risco_estagio",
                columns: new[] { "tenant_id", "ordem" });

            migrationBuilder.CreateIndex(
                name: "ix__estagio_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_estagio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estagio_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_estagio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_historico_risco_id",
                schema: "projetos",
                table: "prj_risco_historico",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_historico_tenant_id_risco_id",
                schema: "projetos",
                table: "prj_risco_historico",
                columns: new[] { "tenant_id", "risco_id" });

            migrationBuilder.CreateIndex(
                name: "ix__historico_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__historico_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_parametro_tenant_id_chave",
                schema: "projetos",
                table: "prj_risco_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__parametro_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_projeto_tenant_id_projeto_id_estagio_id",
                schema: "projetos",
                table: "prj_risco_projeto",
                columns: new[] { "tenant_id", "projeto_id", "estagio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__risco_projeto_sync_id",
                schema: "projetos",
                table: "prj_risco_projeto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__risco_projeto_tenant_id",
                schema: "projetos",
                table: "prj_risco_projeto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_responsavel_risco_id",
                schema: "projetos",
                table: "prj_risco_responsavel",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "i_x_prj_risco_responsavel_tenant_id_risco_id_usuario_id",
                schema: "projetos",
                table: "prj_risco_responsavel",
                columns: new[] { "tenant_id", "risco_id", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__responsavel_risco_sync_id",
                schema: "projetos",
                table: "prj_risco_responsavel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__responsavel_risco_tenant_id",
                schema: "projetos",
                table: "prj_risco_responsavel",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prj_enc_encerramento_anexo",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_enc_encerramento_historico",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_enc_encerramento_item",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_enc_parametro",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_portfolio_anexo",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_portfolio_historico",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_portfolio_item",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_portfolio_parametro",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_anexo",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_comentario",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_estagio",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_historico",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_parametro",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_responsavel",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_enc_encerramento",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_portfolio",
                schema: "projetos");

            migrationBuilder.DropTable(
                name: "prj_risco_projeto",
                schema: "projetos");
        }
    }
}
