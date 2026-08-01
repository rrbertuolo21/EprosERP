using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCSubmodulosPolRegRisCiaSod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grc_cia_achado",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teste_controle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    severidade = table.Column<string>(type: "text", nullable: false),
                    prazo_remediacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_encerramento = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_cia_achado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_cia_plano_acao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    achado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_grc_cia_plano_acao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_cia_plano_auditoria",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    ciclo = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_ultima_transicao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_cia_plano_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_cia_teste_controle",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plano_auditoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_teste = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resultado = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    motivo_reabertura = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_cia_teste_controle", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_pol_aceite",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    politica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora_aceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: true),
                    declaracao_aceite = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_pol_aceite", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_pol_politica",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo_aplicavel = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    motivo_ultima_transicao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_pol_politica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_pol_publico_alvo",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    politica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_alvo = table.Column<string>(type: "text", nullable: false),
                    alvo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exige_aceite = table.Column<bool>(type: "boolean", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_grc_pol_publico_alvo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_pol_versao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    politica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_versao = table.Column<string>(type: "text", nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resumo_alteracoes = table.Column<string>(type: "text", nullable: true),
                    motivo_versao = table.Column<string>(type: "text", nullable: true),
                    documento_oficial_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_grc_pol_versao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_reg_calendario",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_grc_reg_calendario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_reg_certificado_digital",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    serial = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_revogacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_reg_certificado_digital", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_reg_registro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    norma = table.Column<string>(type: "text", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_ultima_transicao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_reg_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_reg_validacao_certificado",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificado_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resultado = table.Column<string>(type: "text", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_reg_validacao_certificado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_avaliacao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    probabilidade = table.Column<int>(type: "integer", nullable: false),
                    impacto = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    data_avaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_grc_ris_avaliacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_controle_mitigador",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    controle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    efetividade = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_ris_controle_mitigador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_kri",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: true),
                    limite_alerta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    limite_critico = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_grc_ris_kri", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_kri_leitura",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kri_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_leitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false),
                    situacao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_ris_kri_leitura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_ris_plano_acao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_grc_ris_plano_acao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_excecao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    violacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: false),
                    aprovador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    controle_compensatorio = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_sod_excecao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_funcao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_sod_funcao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_regra",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: true),
                    funcao_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcao_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criticidade = table.Column<string>(type: "text", nullable: false),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    motivo_ultima_transicao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_sod_regra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_simulacao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    alvo = table.Column<string>(type: "text", nullable: false),
                    data_simulacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantidade_violacoes = table.Column<int>(type: "integer", nullable: false),
                    resumo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_grc_sod_simulacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_sod_violacao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    simulacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    regra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_deteccao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_grc_sod_violacao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_achado_severidade_status",
                schema: "grc",
                table: "grc_cia_achado",
                columns: new[] { "severidade", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__achado_sync_id",
                schema: "grc",
                table: "grc_cia_achado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__achado_tenant_id",
                schema: "grc",
                table: "grc_cia_achado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_plano_acao_responsavel_id_status",
                schema: "grc",
                table: "grc_cia_plano_acao",
                columns: new[] { "responsavel_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__plano_acao_auditoria_sync_id",
                schema: "grc",
                table: "grc_cia_plano_acao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_acao_auditoria_tenant_id",
                schema: "grc",
                table: "grc_cia_plano_acao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_plano_auditoria_tenant_id_codigo",
                schema: "grc",
                table: "grc_cia_plano_auditoria",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_auditoria_sync_id",
                schema: "grc",
                table: "grc_cia_plano_auditoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_auditoria_tenant_id",
                schema: "grc",
                table: "grc_cia_plano_auditoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_cia_teste_controle_controle_id",
                schema: "grc",
                table: "grc_cia_teste_controle",
                column: "controle_id");

            migrationBuilder.CreateIndex(
                name: "ix__teste_controle_sync_id",
                schema: "grc",
                table: "grc_cia_teste_controle",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__teste_controle_tenant_id",
                schema: "grc",
                table: "grc_cia_teste_controle",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_aceite_tenant_id_versao_id_usuario_id",
                schema: "grc",
                table: "grc_pol_aceite",
                columns: new[] { "tenant_id", "versao_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_aceite_sync_id",
                schema: "grc",
                table: "grc_pol_aceite",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_aceite_tenant_id",
                schema: "grc",
                table: "grc_pol_aceite",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_politica_tenant_id_codigo",
                schema: "grc",
                table: "grc_pol_politica",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_politica_tenant_id_status",
                schema: "grc",
                table: "grc_pol_politica",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__politica_sync_id",
                schema: "grc",
                table: "grc_pol_politica",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_tenant_id",
                schema: "grc",
                table: "grc_pol_politica",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_publico_alvo_politica_id",
                schema: "grc",
                table: "grc_pol_publico_alvo",
                column: "politica_id");

            migrationBuilder.CreateIndex(
                name: "ix__politica_publico_alvo_sync_id",
                schema: "grc",
                table: "grc_pol_publico_alvo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_publico_alvo_tenant_id",
                schema: "grc",
                table: "grc_pol_publico_alvo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_pol_versao_politica_id_numero_versao",
                schema: "grc",
                table: "grc_pol_versao",
                columns: new[] { "politica_id", "numero_versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_versao_sync_id",
                schema: "grc",
                table: "grc_pol_versao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__politica_versao_tenant_id",
                schema: "grc",
                table: "grc_pol_versao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_reg_calendario_data_vencimento_status",
                schema: "grc",
                table: "grc_reg_calendario",
                columns: new[] { "data_vencimento", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__calendario_regulatorio_sync_id",
                schema: "grc",
                table: "grc_reg_calendario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__calendario_regulatorio_tenant_id",
                schema: "grc",
                table: "grc_reg_calendario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_reg_certificado_digital_cnpj_serial_empresa_id",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                columns: new[] { "cnpj", "serial", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__certificado_digital_sync_id",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__certificado_digital_tenant_id",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_reg_registro_tenant_id_codigo",
                schema: "grc",
                table: "grc_reg_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_regulatorio_sync_id",
                schema: "grc",
                table: "grc_reg_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_regulatorio_tenant_id",
                schema: "grc",
                table: "grc_reg_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_reg_validacao_certificado_certificado_id_data_hora",
                schema: "grc",
                table: "grc_reg_validacao_certificado",
                columns: new[] { "certificado_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "ix__validacao_certificado_sync_id",
                schema: "grc",
                table: "grc_reg_validacao_certificado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__validacao_certificado_tenant_id",
                schema: "grc",
                table: "grc_reg_validacao_certificado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_avaliacao_risco_id",
                schema: "grc",
                table: "grc_ris_avaliacao",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "ix__avaliacao_risco_sync_id",
                schema: "grc",
                table: "grc_ris_avaliacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__avaliacao_risco_tenant_id",
                schema: "grc",
                table: "grc_ris_avaliacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_controle_mitigador_risco_id_controle_id",
                schema: "grc",
                table: "grc_ris_controle_mitigador",
                columns: new[] { "risco_id", "controle_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_mitigador_sync_id",
                schema: "grc",
                table: "grc_ris_controle_mitigador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__controle_mitigador_tenant_id",
                schema: "grc",
                table: "grc_ris_controle_mitigador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_kri_risco_id",
                schema: "grc",
                table: "grc_ris_kri",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "ix__kri_sync_id",
                schema: "grc",
                table: "grc_ris_kri",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__kri_tenant_id",
                schema: "grc",
                table: "grc_ris_kri",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_kri_leitura_kri_id",
                schema: "grc",
                table: "grc_ris_kri_leitura",
                column: "kri_id");

            migrationBuilder.CreateIndex(
                name: "ix__kri_leitura_sync_id",
                schema: "grc",
                table: "grc_ris_kri_leitura",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__kri_leitura_tenant_id",
                schema: "grc",
                table: "grc_ris_kri_leitura",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_ris_plano_acao_risco_id",
                schema: "grc",
                table: "grc_ris_plano_acao",
                column: "risco_id");

            migrationBuilder.CreateIndex(
                name: "ix__plano_acao_risco_sync_id",
                schema: "grc",
                table: "grc_ris_plano_acao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_acao_risco_tenant_id",
                schema: "grc",
                table: "grc_ris_plano_acao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_excecao_violacao_id",
                schema: "grc",
                table: "grc_sod_excecao",
                column: "violacao_id");

            migrationBuilder.CreateIndex(
                name: "ix__excecao_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_excecao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__excecao_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_excecao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_funcao_tenant_id_codigo",
                schema: "grc",
                table: "grc_sod_funcao",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__funcao_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_funcao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__funcao_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_funcao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_regra_tenant_id_funcao_a_id_funcao_b_id_vigencia_in~",
                schema: "grc",
                table: "grc_sod_regra",
                columns: new[] { "tenant_id", "funcao_a_id", "funcao_b_id", "vigencia_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__regra_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_regra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__regra_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_regra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__simulacao_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_simulacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__simulacao_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_simulacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_sod_violacao_regra_id_status",
                schema: "grc",
                table: "grc_sod_violacao",
                columns: new[] { "regra_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__violacao_so_d_sync_id",
                schema: "grc",
                table: "grc_sod_violacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__violacao_so_d_tenant_id",
                schema: "grc",
                table: "grc_sod_violacao",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_cia_achado",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_cia_plano_acao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_cia_plano_auditoria",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_cia_teste_controle",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_pol_aceite",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_pol_politica",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_pol_publico_alvo",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_pol_versao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_reg_calendario",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_reg_certificado_digital",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_reg_registro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_reg_validacao_certificado",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_avaliacao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_controle_mitigador",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_kri",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_kri_leitura",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_ris_plano_acao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_excecao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_funcao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_regra",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_simulacao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_sod_violacao",
                schema: "grc");
        }
    }
}
