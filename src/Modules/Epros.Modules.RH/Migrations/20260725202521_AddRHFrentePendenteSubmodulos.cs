using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.RH.Migrations
{
    /// <inheritdoc />
    public partial class AddRHFrentePendenteSubmodulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rh_dev_advertencia",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_advertencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assunto = table.Column<string>(type: "text", nullable: true),
                    severidade = table.Column<string>(type: "text", nullable: true),
                    data_advertencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    advertido_por = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    resposta_colaborador = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_advertencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_ciencia_documento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    reconhecido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    atribuido_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_ciencia_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_comunicado",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prioridade = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_comunicado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_comunicado_categoria",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_comunicado_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_comunicado_departamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comunicado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_dev_comunicado_departamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_desligamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_desligamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_aviso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_desligamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_desligamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_evento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    tipo_evento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: true),
                    hora_fim = table.Column<TimeSpan>(type: "interval", nullable: true),
                    local = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_evento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_historico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "text", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detalhe = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_pedido_desligamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ultimo_dia_trabalho = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_pedido_desligamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_premio",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_premio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_premio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    certificado = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_premio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_promocao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_anterior_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_anterior_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_anterior_id = table.Column<Guid>(type: "uuid", nullable: true),
                    filial_atual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_atual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_atual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_efetiva = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_promocao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_reclamacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    contra_colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_reclamacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assunto = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    data_reclamacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    resolvido_por = table.Column<Guid>(type: "uuid", nullable: true),
                    data_resolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_reclamacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_tipo_advertencia",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_tipo_advertencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_tipo_desligamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_tipo_desligamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_tipo_premio",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_tipo_premio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_dev_tipo_reclamacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_dev_tipo_reclamacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_alerta_certificacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dias_antecedencia = table.Column<int>(type: "integer", nullable: false),
                    data_alerta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_alerta_certificacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_certificacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    treinamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_certificacao = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_certificacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_feedback",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tarefa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_alvo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota = table.Column<int>(type: "integer", nullable: false),
                    comentarios = table.Column<string>(type: "text", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dono_funcional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_feedback", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_historico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "text", nullable: false),
                    valor_anterior_json = table.Column<string>(type: "text", nullable: true),
                    valor_novo_json = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_lms_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_tarefa",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    treinamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_limite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsavel_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dono_funcional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_tarefa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_tipo_treinamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dono_funcional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_tipo_treinamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_treinador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    contato = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    experiencia = table.Column<string>(type: "text", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    especialidade = table.Column<string>(type: "text", nullable: true),
                    qualificacao = table.Column<string>(type: "text", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dono_funcional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_treinador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_lms_treinamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    tipo_treinamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    treinador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fim = table.Column<TimeSpan>(type: "interval", nullable: false),
                    local = table.Column<string>(type: "text", nullable: true),
                    capacidade_maxima = table.Column<int>(type: "integer", nullable: true),
                    custo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dono_funcional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_lms_treinamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_cenario_movimento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_movimento = table.Column<string>(type: "text", nullable: true),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade = table.Column<int>(type: "integer", nullable: true),
                    impacto_financeiro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_cenario_movimento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_dia_trabalho",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dia_semana = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_dia_trabalho", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_escala",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    turno_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_escala", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_feriado",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo_feriado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    remunerado = table.Column<bool>(type: "boolean", nullable: true),
                    sincronizar_calendario_google = table.Column<bool>(type: "boolean", nullable: true),
                    sincronizar_calendario_outlook = table.Column<bool>(type: "boolean", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_feriado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_headcount_item",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_autorizada = table.Column<int>(type: "integer", nullable: true),
                    custo_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_headcount_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_headcount_versao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: true),
                    cenario = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_headcount_versao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_historico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    detalhe = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_tipo_feriado",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_tipo_feriado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_turma",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: true),
                    nome = table.Column<string>(type: "text", nullable: true),
                    escala_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_turma", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pln_turno",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: true),
                    hora_fim = table.Column<TimeSpan>(type: "interval", nullable: true),
                    intervalo_inicio = table.Column<TimeSpan>(type: "interval", nullable: true),
                    intervalo_fim = table.Column<TimeSpan>(type: "interval", nullable: true),
                    turno_noturno = table.Column<bool>(type: "boolean", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pln_turno", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_acompanhamento_objetivo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    objetivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_acompanhamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_anterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_contribuicao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_progresso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_restantes = table.Column<int>(type: "integer", nullable: true),
                    data_conclusao_projetada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_andamento = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_acompanhamento_objetivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_avaliacao_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    avaliador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ciclo_avaliacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_avaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notas_json = table.Column<string>(type: "text", nullable: true),
                    media_nota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    pontos_fortes = table.Column<string>(type: "text", nullable: true),
                    pontos_melhoria = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_avaliacao_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_categoria_indicador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_categoria_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_categoria_objetivo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    codigo = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_categoria_objetivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_ciclo_avaliacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    frequencia = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_ciclo_avaliacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_contribuicao_objetivo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    objetivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_contribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_contribuicao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_contribuicao = table.Column<string>(type: "text", nullable: true),
                    tipo_referencia = table.Column<string>(type: "text", nullable: true),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_contribuicao_objetivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_historico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "text", nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    detalhe = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_indicador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    unidade_medida = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_meta_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_meta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alvo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    progresso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_meta_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_nine_box",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ciclo_avaliacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eixo_desempenho = table.Column<string>(type: "text", nullable: true),
                    eixo_potencial = table.Column<string>(type: "text", nullable: true),
                    quadrante = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_nine_box", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_nota_indicador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avaliacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota = table.Column<int>(type: "integer", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_nota_indicador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_objetivo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_objetivo = table.Column<string>(type: "text", nullable: true),
                    valor_alvo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_alvo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prioridade = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    conta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_objetivo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_plano_retencao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    acao = table.Column<string>(type: "text", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_plano_retencao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_solicitacao_licenca",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_licenca_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_dias = table.Column<int>(type: "integer", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    anexo = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    comentario_aprovador = table.Column<string>(type: "text", nullable: true),
                    aprovado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aprovado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_solicitacao_licenca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_sucessao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    posicao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    colaborador_atual_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sucessor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prontidao_meses = table.Column<int>(type: "integer", nullable: true),
                    risco_perda = table.Column<string>(type: "text", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_sucessao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_tipo_licenca",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    dias_maximos_ano = table.Column<int>(type: "integer", nullable: true),
                    remunerada = table.Column<bool>(type: "boolean", nullable: true),
                    cor = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_tlt_tipo_licenca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_tlt_tipo_meta",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_tlt_tipo_meta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_cargo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    cbo = table.Column<string>(type: "text", nullable: true),
                    tipo_cargo = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_cargo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_categoria_documento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_categoria_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matricula = table.Column<string>(type: "text", nullable: false),
                    codigo_colaborador = table.Column<string>(type: "text", nullable: true),
                    foto_referencia = table.Column<string>(type: "text", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_admissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_nascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    genero = table.Column<string>(type: "text", nullable: true),
                    tipo_emprego = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    turno_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conta_contabil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    salario_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    tipo_remuneracao = table.Column<string>(type: "text", nullable: true),
                    valor_hora = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    horas_por_dia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_por_semana = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_comissao_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_cargo = table.Column<string>(type: "text", nullable: false),
                    valor_percentual_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_rh_wfm_comissao_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_conta_bancaria_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    banco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo_conta = table.Column<string>(type: "text", nullable: true),
                    numero_conta = table.Column<string>(type: "text", nullable: true),
                    codigo_banco = table.Column<string>(type: "text", nullable: true),
                    agencia = table.Column<string>(type: "text", nullable: true),
                    principal = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_conta_bancaria_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_dados_pagamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pagamento_forma = table.Column<string>(type: "text", nullable: true),
                    pagamento_banco = table.Column<string>(type: "text", nullable: true),
                    pagamento_agencia = table.Column<string>(type: "text", nullable: true),
                    pagamento_agencia_digito = table.Column<string>(type: "text", nullable: true),
                    pagamento_conta = table.Column<string>(type: "text", nullable: true),
                    pagamento_conta_digito = table.Column<string>(type: "text", nullable: true),
                    titular_conta = table.Column<string>(type: "text", nullable: true),
                    codigo_bancario = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_dados_pagamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_dados_trabalhistas",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fgts_optante = table.Column<string>(type: "text", nullable: true),
                    fgts_data_opcao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fgts_conta = table.Column<int>(type: "integer", nullable: true),
                    pis_numero = table.Column<string>(type: "text", nullable: true),
                    pis_data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pis_banco = table.Column<string>(type: "text", nullable: true),
                    pis_agencia = table.Column<string>(type: "text", nullable: true),
                    pis_agencia_digito = table.Column<string>(type: "text", nullable: true),
                    ctps_numero = table.Column<string>(type: "text", nullable: true),
                    ctps_serie = table.Column<string>(type: "text", nullable: true),
                    ctps_data_expedicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ctps_uf = table.Column<string>(type: "text", nullable: true),
                    sai_na_rais = table.Column<string>(type: "text", nullable: true),
                    categoria_sefip = table.Column<string>(type: "text", nullable: true),
                    ocorrencia_sefip = table.Column<int>(type: "integer", nullable: true),
                    codigo_admissao_caged = table.Column<int>(type: "integer", nullable: true),
                    codigo_demissao_caged = table.Column<int>(type: "integer", nullable: true),
                    codigo_demissao_sefip = table.Column<int>(type: "integer", nullable: true),
                    data_demissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    codigo_turma_ponto = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_dados_trabalhistas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_deducao_recorrente",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deducao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_rh_wfm_deducao_recorrente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_departamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    centro_custo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    gestor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_departamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_documento_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_referencia = table.Column<string>(type: "text", nullable: true),
                    data_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_documento_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_exame_medico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_ultimo_exame = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento_exame = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_exame_medico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_filial",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_filial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_historico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evento = table.Column<string>(type: "text", nullable: false),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    detalhe = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_presenca_basica",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tempo_permanencia = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_presenca_basica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_renda_recorrente",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    renda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_rh_wfm_renda_recorrente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_servico_colaborador",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_wfm_servico_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_tipo_documento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_tipo_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_wfm_transferencia",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_transferencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_efetiva = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    filial_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    filial_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    departamento_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo_destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_wfm_transferencia", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__dev_advertencia_sync_id",
                schema: "rh",
                table: "rh_dev_advertencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_advertencia_tenant_id",
                schema: "rh",
                table: "rh_dev_advertencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_ciencia_documento_sync_id",
                schema: "rh",
                table: "rh_dev_ciencia_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_ciencia_documento_tenant_id",
                schema: "rh",
                table: "rh_dev_ciencia_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_sync_id",
                schema: "rh",
                table: "rh_dev_comunicado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_tenant_id",
                schema: "rh",
                table: "rh_dev_comunicado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_categoria_sync_id",
                schema: "rh",
                table: "rh_dev_comunicado_categoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_categoria_tenant_id",
                schema: "rh",
                table: "rh_dev_comunicado_categoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_departamento_sync_id",
                schema: "rh",
                table: "rh_dev_comunicado_departamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_comunicado_departamento_tenant_id",
                schema: "rh",
                table: "rh_dev_comunicado_departamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_desligamento_sync_id",
                schema: "rh",
                table: "rh_dev_desligamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_desligamento_tenant_id",
                schema: "rh",
                table: "rh_dev_desligamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_evento_sync_id",
                schema: "rh",
                table: "rh_dev_evento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_evento_tenant_id",
                schema: "rh",
                table: "rh_dev_evento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_historico_sync_id",
                schema: "rh",
                table: "rh_dev_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_historico_tenant_id",
                schema: "rh",
                table: "rh_dev_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_pedido_desligamento_sync_id",
                schema: "rh",
                table: "rh_dev_pedido_desligamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_pedido_desligamento_tenant_id",
                schema: "rh",
                table: "rh_dev_pedido_desligamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_premio_sync_id",
                schema: "rh",
                table: "rh_dev_premio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_premio_tenant_id",
                schema: "rh",
                table: "rh_dev_premio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_promocao_sync_id",
                schema: "rh",
                table: "rh_dev_promocao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_promocao_tenant_id",
                schema: "rh",
                table: "rh_dev_promocao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_reclamacao_sync_id",
                schema: "rh",
                table: "rh_dev_reclamacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_reclamacao_tenant_id",
                schema: "rh",
                table: "rh_dev_reclamacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_advertencia_sync_id",
                schema: "rh",
                table: "rh_dev_tipo_advertencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_advertencia_tenant_id",
                schema: "rh",
                table: "rh_dev_tipo_advertencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_desligamento_sync_id",
                schema: "rh",
                table: "rh_dev_tipo_desligamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_desligamento_tenant_id",
                schema: "rh",
                table: "rh_dev_tipo_desligamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_premio_sync_id",
                schema: "rh",
                table: "rh_dev_tipo_premio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_premio_tenant_id",
                schema: "rh",
                table: "rh_dev_tipo_premio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_reclamacao_sync_id",
                schema: "rh",
                table: "rh_dev_tipo_reclamacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dev_tipo_reclamacao_tenant_id",
                schema: "rh",
                table: "rh_dev_tipo_reclamacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_alerta_certificacao_sync_id",
                schema: "rh",
                table: "rh_lms_alerta_certificacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_alerta_certificacao_tenant_id",
                schema: "rh",
                table: "rh_lms_alerta_certificacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_certificacao_sync_id",
                schema: "rh",
                table: "rh_lms_certificacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_certificacao_tenant_id",
                schema: "rh",
                table: "rh_lms_certificacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_feedback_sync_id",
                schema: "rh",
                table: "rh_lms_feedback",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_feedback_tenant_id",
                schema: "rh",
                table: "rh_lms_feedback",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_historico_sync_id",
                schema: "rh",
                table: "rh_lms_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_historico_tenant_id",
                schema: "rh",
                table: "rh_lms_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_tarefa_sync_id",
                schema: "rh",
                table: "rh_lms_tarefa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_tarefa_tenant_id",
                schema: "rh",
                table: "rh_lms_tarefa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_tipo_treinamento_sync_id",
                schema: "rh",
                table: "rh_lms_tipo_treinamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_tipo_treinamento_tenant_id",
                schema: "rh",
                table: "rh_lms_tipo_treinamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_treinador_sync_id",
                schema: "rh",
                table: "rh_lms_treinador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_treinador_tenant_id",
                schema: "rh",
                table: "rh_lms_treinador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__lms_treinamento_sync_id",
                schema: "rh",
                table: "rh_lms_treinamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lms_treinamento_tenant_id",
                schema: "rh",
                table: "rh_lms_treinamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_cenario_movimento_sync_id",
                schema: "rh",
                table: "rh_pln_cenario_movimento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_cenario_movimento_tenant_id",
                schema: "rh",
                table: "rh_pln_cenario_movimento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_dia_trabalho_sync_id",
                schema: "rh",
                table: "rh_pln_dia_trabalho",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_dia_trabalho_tenant_id",
                schema: "rh",
                table: "rh_pln_dia_trabalho",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_escala_sync_id",
                schema: "rh",
                table: "rh_pln_escala",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_escala_tenant_id",
                schema: "rh",
                table: "rh_pln_escala",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_feriado_sync_id",
                schema: "rh",
                table: "rh_pln_feriado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_feriado_tenant_id",
                schema: "rh",
                table: "rh_pln_feriado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_headcount_item_sync_id",
                schema: "rh",
                table: "rh_pln_headcount_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_headcount_item_tenant_id",
                schema: "rh",
                table: "rh_pln_headcount_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_headcount_versao_sync_id",
                schema: "rh",
                table: "rh_pln_headcount_versao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_headcount_versao_tenant_id",
                schema: "rh",
                table: "rh_pln_headcount_versao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_historico_sync_id",
                schema: "rh",
                table: "rh_pln_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_historico_tenant_id",
                schema: "rh",
                table: "rh_pln_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_tipo_feriado_sync_id",
                schema: "rh",
                table: "rh_pln_tipo_feriado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_tipo_feriado_tenant_id",
                schema: "rh",
                table: "rh_pln_tipo_feriado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_turma_sync_id",
                schema: "rh",
                table: "rh_pln_turma",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_turma_tenant_id",
                schema: "rh",
                table: "rh_pln_turma",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pln_turno_sync_id",
                schema: "rh",
                table: "rh_pln_turno",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pln_turno_tenant_id",
                schema: "rh",
                table: "rh_pln_turno",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_acompanhamento_objetivo_sync_id",
                schema: "rh",
                table: "rh_tlt_acompanhamento_objetivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_acompanhamento_objetivo_tenant_id",
                schema: "rh",
                table: "rh_tlt_acompanhamento_objetivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_avaliacao_colaborador_sync_id",
                schema: "rh",
                table: "rh_tlt_avaliacao_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_avaliacao_colaborador_tenant_id",
                schema: "rh",
                table: "rh_tlt_avaliacao_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_categoria_indicador_sync_id",
                schema: "rh",
                table: "rh_tlt_categoria_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_categoria_indicador_tenant_id",
                schema: "rh",
                table: "rh_tlt_categoria_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_categoria_objetivo_sync_id",
                schema: "rh",
                table: "rh_tlt_categoria_objetivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_categoria_objetivo_tenant_id",
                schema: "rh",
                table: "rh_tlt_categoria_objetivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_ciclo_avaliacao_sync_id",
                schema: "rh",
                table: "rh_tlt_ciclo_avaliacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_ciclo_avaliacao_tenant_id",
                schema: "rh",
                table: "rh_tlt_ciclo_avaliacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_contribuicao_objetivo_sync_id",
                schema: "rh",
                table: "rh_tlt_contribuicao_objetivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_contribuicao_objetivo_tenant_id",
                schema: "rh",
                table: "rh_tlt_contribuicao_objetivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_historico_sync_id",
                schema: "rh",
                table: "rh_tlt_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_historico_tenant_id",
                schema: "rh",
                table: "rh_tlt_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_indicador_sync_id",
                schema: "rh",
                table: "rh_tlt_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_indicador_tenant_id",
                schema: "rh",
                table: "rh_tlt_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_meta_colaborador_sync_id",
                schema: "rh",
                table: "rh_tlt_meta_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_meta_colaborador_tenant_id",
                schema: "rh",
                table: "rh_tlt_meta_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_nine_box_sync_id",
                schema: "rh",
                table: "rh_tlt_nine_box",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_nine_box_tenant_id",
                schema: "rh",
                table: "rh_tlt_nine_box",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_nota_indicador_sync_id",
                schema: "rh",
                table: "rh_tlt_nota_indicador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_nota_indicador_tenant_id",
                schema: "rh",
                table: "rh_tlt_nota_indicador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_objetivo_sync_id",
                schema: "rh",
                table: "rh_tlt_objetivo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_objetivo_tenant_id",
                schema: "rh",
                table: "rh_tlt_objetivo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_plano_retencao_sync_id",
                schema: "rh",
                table: "rh_tlt_plano_retencao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_plano_retencao_tenant_id",
                schema: "rh",
                table: "rh_tlt_plano_retencao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_solicitacao_licenca_sync_id",
                schema: "rh",
                table: "rh_tlt_solicitacao_licenca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_solicitacao_licenca_tenant_id",
                schema: "rh",
                table: "rh_tlt_solicitacao_licenca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_sucessao_sync_id",
                schema: "rh",
                table: "rh_tlt_sucessao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_sucessao_tenant_id",
                schema: "rh",
                table: "rh_tlt_sucessao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_tipo_licenca_sync_id",
                schema: "rh",
                table: "rh_tlt_tipo_licenca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_tipo_licenca_tenant_id",
                schema: "rh",
                table: "rh_tlt_tipo_licenca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__tlt_tipo_meta_sync_id",
                schema: "rh",
                table: "rh_tlt_tipo_meta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tlt_tipo_meta_tenant_id",
                schema: "rh",
                table: "rh_tlt_tipo_meta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_cargo_sync_id",
                schema: "rh",
                table: "rh_wfm_cargo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_cargo_tenant_id",
                schema: "rh",
                table: "rh_wfm_cargo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_categoria_documento_sync_id",
                schema: "rh",
                table: "rh_wfm_categoria_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_categoria_documento_tenant_id",
                schema: "rh",
                table: "rh_wfm_categoria_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_colaborador_sync_id",
                schema: "rh",
                table: "rh_wfm_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_colaborador_tenant_id",
                schema: "rh",
                table: "rh_wfm_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_comissao_colaborador_sync_id",
                schema: "rh",
                table: "rh_wfm_comissao_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_comissao_colaborador_tenant_id",
                schema: "rh",
                table: "rh_wfm_comissao_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_conta_bancaria_colaborador_sync_id",
                schema: "rh",
                table: "rh_wfm_conta_bancaria_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_conta_bancaria_colaborador_tenant_id",
                schema: "rh",
                table: "rh_wfm_conta_bancaria_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_dados_pagamento_sync_id",
                schema: "rh",
                table: "rh_wfm_dados_pagamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_dados_pagamento_tenant_id",
                schema: "rh",
                table: "rh_wfm_dados_pagamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_dados_trabalhistas_sync_id",
                schema: "rh",
                table: "rh_wfm_dados_trabalhistas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_dados_trabalhistas_tenant_id",
                schema: "rh",
                table: "rh_wfm_dados_trabalhistas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_deducao_recorrente_sync_id",
                schema: "rh",
                table: "rh_wfm_deducao_recorrente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_deducao_recorrente_tenant_id",
                schema: "rh",
                table: "rh_wfm_deducao_recorrente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_departamento_sync_id",
                schema: "rh",
                table: "rh_wfm_departamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_departamento_tenant_id",
                schema: "rh",
                table: "rh_wfm_departamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_documento_colaborador_sync_id",
                schema: "rh",
                table: "rh_wfm_documento_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_documento_colaborador_tenant_id",
                schema: "rh",
                table: "rh_wfm_documento_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_exame_medico_sync_id",
                schema: "rh",
                table: "rh_wfm_exame_medico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_exame_medico_tenant_id",
                schema: "rh",
                table: "rh_wfm_exame_medico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_filial_sync_id",
                schema: "rh",
                table: "rh_wfm_filial",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_filial_tenant_id",
                schema: "rh",
                table: "rh_wfm_filial",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_historico_sync_id",
                schema: "rh",
                table: "rh_wfm_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_historico_tenant_id",
                schema: "rh",
                table: "rh_wfm_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_presenca_basica_sync_id",
                schema: "rh",
                table: "rh_wfm_presenca_basica",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_presenca_basica_tenant_id",
                schema: "rh",
                table: "rh_wfm_presenca_basica",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_renda_recorrente_sync_id",
                schema: "rh",
                table: "rh_wfm_renda_recorrente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_renda_recorrente_tenant_id",
                schema: "rh",
                table: "rh_wfm_renda_recorrente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_servico_colaborador_sync_id",
                schema: "rh",
                table: "rh_wfm_servico_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_servico_colaborador_tenant_id",
                schema: "rh",
                table: "rh_wfm_servico_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_tipo_documento_sync_id",
                schema: "rh",
                table: "rh_wfm_tipo_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_tipo_documento_tenant_id",
                schema: "rh",
                table: "rh_wfm_tipo_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__wfm_transferencia_sync_id",
                schema: "rh",
                table: "rh_wfm_transferencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__wfm_transferencia_tenant_id",
                schema: "rh",
                table: "rh_wfm_transferencia",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rh_dev_advertencia",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_ciencia_documento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_comunicado",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_comunicado_categoria",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_comunicado_departamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_desligamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_evento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_pedido_desligamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_premio",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_promocao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_reclamacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_tipo_advertencia",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_tipo_desligamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_tipo_premio",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_dev_tipo_reclamacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_alerta_certificacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_certificacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_feedback",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_tarefa",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_tipo_treinamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_treinador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_lms_treinamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_cenario_movimento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_dia_trabalho",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_escala",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_feriado",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_headcount_item",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_headcount_versao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_tipo_feriado",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_turma",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pln_turno",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_acompanhamento_objetivo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_avaliacao_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_categoria_indicador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_categoria_objetivo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_ciclo_avaliacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_contribuicao_objetivo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_indicador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_meta_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_nine_box",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_nota_indicador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_objetivo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_plano_retencao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_solicitacao_licenca",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_sucessao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_tipo_licenca",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_tlt_tipo_meta",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_cargo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_categoria_documento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_comissao_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_conta_bancaria_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_dados_pagamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_dados_trabalhistas",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_deducao_recorrente",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_departamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_documento_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_exame_medico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_filial",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_presenca_basica",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_renda_recorrente",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_servico_colaborador",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_tipo_documento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_wfm_transferencia",
                schema: "rh");
        }
    }
}
