using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.RH.Migrations
{
    /// <inheritdoc />
    public partial class AddRHFrente12Submodulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rh_fol_adiantamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_comprovante = table.Column<string>(type: "text", nullable: false),
                    data_comprovante = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    serial = table.Column<int>(type: "integer", nullable: true),
                    narrativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_adiantamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_adicional",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_adicional_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_calculo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_adicional", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_afastamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_afastamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dias_afastado = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_afastamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_bonus_deducao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    competencia = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: true),
                    narrativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_bonus_deducao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_competencia",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    banco_pagamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    status_pagamento = table.Column<string>(type: "text", nullable: true),
                    valor_total_pagamento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_competencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_configuracao_mensal",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pacote_salarial_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_configuracao_mensal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_desconto",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_desconto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_calculo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_desconto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_emprestimo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_emprestimo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_calculo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_emprestimo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_ferias_coletivas",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dias_gozo = table.Column<int>(type: "integer", nullable: true),
                    abono_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    abono_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dias_abono = table.Column<int>(type: "integer", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_ferias_coletivas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_guia_acumulada",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gps_tipo = table.Column<string>(type: "text", nullable: false),
                    gps_competencia = table.Column<string>(type: "text", nullable: false),
                    gps_valor_inss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    gps_valor_outras_entidades = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    gps_data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    irrf_competencia = table.Column<string>(type: "text", nullable: false),
                    irrf_codigo_recolhimento = table.Column<int>(type: "integer", nullable: true),
                    irrf_valor_acumulado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    irrf_data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pis_competencia = table.Column<string>(type: "text", nullable: false),
                    pis_valor_acumulado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    pis_data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_guia_acumulada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_historico",
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
                    table.PrimaryKey("p_k_rh_fol_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_historico_salarial",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    salario_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_aumento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salario_novo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valido_a_partir = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_historico_salarial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_hora_extra",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_dias = table.Column<int>(type: "integer", nullable: true),
                    horas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    taxa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_hora_extra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_inss",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_inss", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_inss_retencao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inss_id = table.Column<Guid>(type: "uuid", nullable: true),
                    servico_inss_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_mensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor13 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_inss_retencao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_inss_servico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_inss_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_lancamento",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    salario_base = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_proventos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_descontos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_emprestimos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_horas_extras = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    dias_trabalhados = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_presentes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    meias_diarias = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_ausentes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_licenca_paga = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    dias_licenca_nao_paga = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_lancamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_lancamento_item",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lancamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubrica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    referencia = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    provento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_lancamento_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_pacote_item",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pacote_salarial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rubrica_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    narrativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_pacote_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_pacote_salarial",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    narrativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_pacote_salarial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_pagamento_diario",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_referencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    numero_comprovante = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_pagamento_diario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_pagamento_diario_item",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pagamento_diario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_pagamento_diario_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_parametro",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    contribui_pis = table.Column<string>(type: "text", nullable: false),
                    aliquota_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    discriminar_dsr = table.Column<string>(type: "text", nullable: false),
                    dia_pagamento = table.Column<string>(type: "text", nullable: false),
                    calculo_proporcionalidade = table.Column<string>(type: "text", nullable: false),
                    descontar_faltas13 = table.Column<string>(type: "text", nullable: false),
                    pagar_adicionais13 = table.Column<string>(type: "text", nullable: false),
                    mes_adiantamento13 = table.Column<string>(type: "text", nullable: false),
                    percentual_adiantamento13 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ferias_descontar_faltas = table.Column<string>(type: "text", nullable: false),
                    ferias_pagar_adicionais = table.Column<string>(type: "text", nullable: false),
                    ferias_adiantar13 = table.Column<string>(type: "text", nullable: false),
                    ferias_pagar_estagiarios = table.Column<string>(type: "text", nullable: false),
                    ferias_calc_justa_causa = table.Column<string>(type: "text", nullable: false),
                    ferias_movimento_mensal = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_plano_saude",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    operadora_plano_saude_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    beneficiario = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_plano_saude", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_presenca_diaria",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    narrativa = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_presenca_diaria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_rescisao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_demissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    data_aviso_previo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dias_aviso_previo = table.Column<int>(type: "integer", nullable: true),
                    comprovou_novo_emprego = table.Column<string>(type: "text", nullable: false),
                    dispensou_empregado = table.Column<string>(type: "text", nullable: false),
                    pensao_alimenticia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    pensao_alimenticia_fgts = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fgts_valor_rescisao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fgts_saldo_banco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fgts_complemento_saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fgts_codigo_afastamento = table.Column<string>(type: "text", nullable: false),
                    fgts_codigo_saque = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_fol_rescisao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_rubrica",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: false),
                    base_calculo = table.Column<string>(type: "text", nullable: false),
                    taxa = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_rubrica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_tipo_adicional",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_tipo_adicional", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_tipo_desconto",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_tipo_desconto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_tipo_emprestimo",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_tipo_emprestimo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_fol_vale_transporte",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itinerario_transporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: true),
                    percentual_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_fol_vale_transporte", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_abono",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: true),
                    utilizado = table.Column<int>(type: "integer", nullable: true),
                    saldo = table.Column<int>(type: "integer", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    inicio_utilizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_abono", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_banco_horas",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_trabalho = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    quantidade = table.Column<string>(type: "text", nullable: false),
                    situacao = table.Column<string>(type: "text", nullable: false),
                    fechamento_jornada_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_banco_horas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_classificacao_jornada",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    padrao = table.Column<string>(type: "text", nullable: false),
                    descontar_horas = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_classificacao_jornada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_escala",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    desconto_hora_dia = table.Column<string>(type: "text", nullable: false),
                    desconto_dsr = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_domingo = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_segunda = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_terca = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_quarta = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_quinta = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_sexta = table.Column<string>(type: "text", nullable: false),
                    codigo_horario_sabado = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_escala", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_evento_folha",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<string>(type: "text", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    origem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_evento_folha", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_fechamento_jornada",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    classificacao_jornada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_fechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dia_semana = table.Column<string>(type: "text", nullable: false),
                    codigo_horario = table.Column<string>(type: "text", nullable: false),
                    carga_horaria_esperada = table.Column<string>(type: "text", nullable: false),
                    carga_horaria_diurna = table.Column<string>(type: "text", nullable: false),
                    carga_horaria_noturna = table.Column<string>(type: "text", nullable: false),
                    carga_horaria_total = table.Column<string>(type: "text", nullable: false),
                    entrada01 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida01 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada02 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida02 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada03 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida03 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada04 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida04 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada05 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida05 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_inicio_jornada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fim_jornada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_extra01 = table.Column<string>(type: "text", nullable: false),
                    percentual_hora_extra01 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    modalidade_hora_extra01 = table.Column<string>(type: "text", nullable: false),
                    hora_extra02 = table.Column<string>(type: "text", nullable: false),
                    percentual_hora_extra02 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    modalidade_hora_extra02 = table.Column<string>(type: "text", nullable: false),
                    hora_extra03 = table.Column<string>(type: "text", nullable: false),
                    percentual_hora_extra03 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    modalidade_hora_extra03 = table.Column<string>(type: "text", nullable: false),
                    hora_extra04 = table.Column<string>(type: "text", nullable: false),
                    percentual_hora_extra04 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    modalidade_hora_extra04 = table.Column<string>(type: "text", nullable: false),
                    falta_atraso = table.Column<string>(type: "text", nullable: false),
                    compensar = table.Column<string>(type: "text", nullable: false),
                    banco_horas = table.Column<string>(type: "text", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_fechamento_jornada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_historico",
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
                    table.PrimaryKey("p_k_rh_pnt_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_horario",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo_trabalho = table.Column<string>(type: "text", nullable: false),
                    carga_horaria = table.Column<string>(type: "text", nullable: false),
                    entrada01 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida01 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada02 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida02 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada03 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida03 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada04 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida04 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    entrada05 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    saida05 = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_inicio_jornada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fim_jornada = table.Column<TimeSpan>(type: "interval", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_horario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_importacao_afd",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relogio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_referencia = table.Column<string>(type: "text", nullable: true),
                    data_importacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantidade_registros = table.Column<int>(type: "integer", nullable: true),
                    quantidade_pareados = table.Column<int>(type: "integer", nullable: true),
                    quantidade_pendentes = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_importacao_afd", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_marcacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relogio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nsr = table.Column<int>(type: "integer", nullable: true),
                    data_marcacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_marcacao = table.Column<TimeSpan>(type: "interval", nullable: false),
                    tipo_marcacao = table.Column<string>(type: "text", nullable: false),
                    tipo_registro = table.Column<string>(type: "text", nullable: false),
                    par_entrada_saida = table.Column<string>(type: "text", nullable: false),
                    justificativa = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_marcacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_parametro",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mes_ano = table.Column<string>(type: "text", nullable: false),
                    dia_inicial_apuracao = table.Column<int>(type: "integer", nullable: true),
                    hora_noturna_inicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_noturna_fim = table.Column<TimeSpan>(type: "interval", nullable: false),
                    periodo_minimo_interjornada = table.Column<string>(type: "text", nullable: false),
                    percentual_he_diurna = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_he_noturna = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    duracao_hora_noturna = table.Column<string>(type: "text", nullable: false),
                    tratamento_hora_mais = table.Column<string>(type: "text", nullable: false),
                    tratamento_hora_menos = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_periodo_apuracao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    fechado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exportado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_periodo_apuracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_presenca_diaria",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    turno_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hora_intervalo = table.Column<string>(type: "text", nullable: true),
                    total_horas = table.Column<string>(type: "text", nullable: true),
                    horas_extras = table.Column<string>(type: "text", nullable: true),
                    valor_hora_extra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_presenca_diaria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_relogio",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    localizacao = table.Column<string>(type: "text", nullable: false),
                    marca = table.Column<string>(type: "text", nullable: false),
                    fabricante = table.Column<string>(type: "text", nullable: false),
                    numero_serie = table.Column<string>(type: "text", nullable: false),
                    utilizacao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_relogio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_restricao_ip",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_pnt_restricao_ip", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_pnt_turma",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escala_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_pnt_turma", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_avaliacao_candidato",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_avaliacao = table.Column<string>(type: "text", nullable: false),
                    pontuacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pontuacao_maxima = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    resultado = table.Column<string>(type: "text", nullable: false),
                    comentarios = table.Column<string>(type: "text", nullable: true),
                    data_avaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conduzida_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_avaliacao_candidato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_candidato",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    primeiro_nome = table.Column<string>(type: "text", nullable: false),
                    sobrenome = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    genero = table.Column<string>(type: "text", nullable: true),
                    data_nascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pais = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    empresa_atual = table.Column<string>(type: "text", nullable: true),
                    cargo_atual = table.Column<string>(type: "text", nullable: true),
                    anos_experiencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    salario_atual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salario_pretendido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    aviso_previo = table.Column<string>(type: "text", nullable: true),
                    habilidades = table.Column<string>(type: "text", nullable: true),
                    escolaridade = table.Column<string>(type: "text", nullable: true),
                    portfolio_url = table.Column<string>(type: "text", nullable: true),
                    linkedin_url = table.Column<string>(type: "text", nullable: true),
                    caminho_foto = table.Column<string>(type: "text", nullable: true),
                    caminho_curriculo = table.Column<string>(type: "text", nullable: true),
                    caminho_carta_apresentacao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_candidatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    respostas_customizadas_json = table.Column<string>(type: "text", nullable: true),
                    protocolo_rastreio = table.Column<string>(type: "text", nullable: false),
                    vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fonte_candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_candidato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_checklist_item",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_tarefa = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    categoria = table.Column<string>(type: "text", nullable: true),
                    papel_responsavel = table.Column<string>(type: "text", nullable: true),
                    prazo_dias = table.Column<int>(type: "integer", nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_checklist_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_checklist_onboarding",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    padrao = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_checklist_onboarding", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_configuracao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: true),
                    criado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_rec_configuracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_entrevista",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_agendada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hora_agendada = table.Column<TimeSpan>(type: "interval", nullable: false),
                    duracao_minutos = table.Column<int>(type: "integer", nullable: false),
                    local = table.Column<string>(type: "text", nullable: true),
                    link_reuniao = table.Column<string>(type: "text", nullable: true),
                    entrevistadores_texto = table.Column<string>(type: "text", nullable: true),
                    entrevistadores_json = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    feedback_enviado = table.Column<bool>(type: "boolean", nullable: false),
                    candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_entrevista_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_entrevista", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_feedback_entrevista",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nota_tecnica = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nota_comunicacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nota_aderencia_cultural = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nota_geral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pontos_fortes = table.Column<string>(type: "text", nullable: true),
                    pontos_fracos = table.Column<string>(type: "text", nullable: true),
                    comentarios = table.Column<string>(type: "text", nullable: true),
                    recomendacao = table.Column<string>(type: "text", nullable: false),
                    entrevista_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrevistadores_json = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_feedback_entrevista", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_fonte_candidato",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_fonte_candidato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_historico",
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
                    table.PrimaryKey("p_k_rh_rec_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_local_vaga",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    trabalho_remoto = table.Column<bool>(type: "boolean", nullable: false),
                    endereco = table.Column<string>(type: "text", nullable: true),
                    cidade = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "text", nullable: true),
                    pais = table.Column<string>(type: "text", nullable: true),
                    cep = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_local_vaga", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_modelo_carta",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idioma = table.Column<string>(type: "text", nullable: false),
                    conteudo = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_modelo_carta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_oferta",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_oferta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    departamento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    salario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    bonus = table.Column<string>(type: "text", nullable: true),
                    participacao_societaria = table.Column<string>(type: "text", nullable: true),
                    beneficios = table.Column<string>(type: "text", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_expiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    caminho_carta_oferta = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_resposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    motivo_recusa = table.Column<string>(type: "text", nullable: true),
                    convertida_colaborador = table.Column<bool>(type: "boolean", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aprovado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status_aprovacao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_oferta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_onboarding_candidato",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsavel_acompanhamento_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_rec_onboarding_candidato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_pergunta_customizada",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pergunta = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    opcoes_json = table.Column<string>(type: "text", nullable: true),
                    obrigatoria = table.Column<bool>(type: "boolean", nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_pergunta_customizada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_round_entrevista",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_round_entrevista", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_tipo_entrevista",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_tipo_entrevista", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_tipo_vaga",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_rec_tipo_vaga", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_rec_vaga",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_interno = table.Column<string>(type: "text", nullable: false),
                    codigo_publico = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    posicoes = table.Column<int>(type: "integer", nullable: false),
                    prioridade = table.Column<int>(type: "integer", nullable: false),
                    experiencia_minima = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    experiencia_maxima = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salario_minimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    salario_maximo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    requisitos = table.Column<string>(type: "text", nullable: true),
                    habilidades = table.Column<string>(type: "text", nullable: false),
                    beneficios = table.Column<string>(type: "text", nullable: true),
                    termos_condicoes = table.Column<string>(type: "text", nullable: true),
                    exibir_termos_condicoes = table.Column<bool>(type: "boolean", nullable: true),
                    prazo_candidatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publicada = table.Column<bool>(type: "boolean", nullable: false),
                    data_publicacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    destaque = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    tipo_candidatura = table.Column<string>(type: "text", nullable: false),
                    url_candidatura = table.Column<string>(type: "text", nullable: true),
                    campos_candidato_json = table.Column<string>(type: "text", nullable: true),
                    visibilidade_json = table.Column<string>(type: "text", nullable: true),
                    filial_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_vaga_id = table.Column<Guid>(type: "uuid", nullable: false),
                    perguntas_customizadas_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_rec_vaga", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_aso",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppp_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_aso = table.Column<string>(type: "text", nullable: false),
                    data_aso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resultado = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_aso", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_bloqueio_alocacao",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_bloqueio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_resolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_bloqueio_alocacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_documento_programa",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    data_inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_sso_documento_programa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_entrega_epi",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    epi_descricao = table.Column<string>(type: "text", nullable: false),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_devolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_sso_entrega_epi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_exame_medico",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppp_id = table.Column<Guid>(type: "uuid", nullable: true),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_ultimo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    natureza = table.Column<string>(type: "text", nullable: false),
                    exame = table.Column<string>(type: "text", nullable: false),
                    indicacao_resultados = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_exame_medico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_historico",
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
                    table.PrimaryKey("p_k_rh_sso_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_indicador_acidente",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<string>(type: "text", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cat_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_acidentes = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_indicador_acidente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_ppp",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_ppp", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_ppp_atividade",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao_atividade = table.Column<string>(type: "text", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rh_sso_ppp_atividade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_ppp_cat",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_acidente = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    afastamento_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_rh_sso_ppp_cat", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_ppp_fator_risco",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppp_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fator_risco = table.Column<string>(type: "text", nullable: false),
                    intensidade = table.Column<string>(type: "text", nullable: true),
                    tecnica_medicao = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_rh_sso_ppp_fator_risco", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rh_sso_treinamento_nr",
                schema: "rh",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    norma = table.Column<string>(type: "text", nullable: false),
                    treinamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rh_sso_treinamento_nr", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__fol_adiantamento_sync_id",
                schema: "rh",
                table: "rh_fol_adiantamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_adiantamento_tenant_id",
                schema: "rh",
                table: "rh_fol_adiantamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_adicional_sync_id",
                schema: "rh",
                table: "rh_fol_adicional",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_adicional_tenant_id",
                schema: "rh",
                table: "rh_fol_adicional",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_afastamento_sync_id",
                schema: "rh",
                table: "rh_fol_afastamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_afastamento_tenant_id",
                schema: "rh",
                table: "rh_fol_afastamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_bonus_deducao_sync_id",
                schema: "rh",
                table: "rh_fol_bonus_deducao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_bonus_deducao_tenant_id",
                schema: "rh",
                table: "rh_fol_bonus_deducao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_competencia_sync_id",
                schema: "rh",
                table: "rh_fol_competencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_competencia_tenant_id",
                schema: "rh",
                table: "rh_fol_competencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_configuracao_mensal_sync_id",
                schema: "rh",
                table: "rh_fol_configuracao_mensal",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_configuracao_mensal_tenant_id",
                schema: "rh",
                table: "rh_fol_configuracao_mensal",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_desconto_sync_id",
                schema: "rh",
                table: "rh_fol_desconto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_desconto_tenant_id",
                schema: "rh",
                table: "rh_fol_desconto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_emprestimo_sync_id",
                schema: "rh",
                table: "rh_fol_emprestimo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_emprestimo_tenant_id",
                schema: "rh",
                table: "rh_fol_emprestimo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_ferias_coletivas_sync_id",
                schema: "rh",
                table: "rh_fol_ferias_coletivas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_ferias_coletivas_tenant_id",
                schema: "rh",
                table: "rh_fol_ferias_coletivas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_guia_acumulada_sync_id",
                schema: "rh",
                table: "rh_fol_guia_acumulada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_guia_acumulada_tenant_id",
                schema: "rh",
                table: "rh_fol_guia_acumulada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_historico_sync_id",
                schema: "rh",
                table: "rh_fol_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_historico_tenant_id",
                schema: "rh",
                table: "rh_fol_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_historico_salarial_sync_id",
                schema: "rh",
                table: "rh_fol_historico_salarial",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_historico_salarial_tenant_id",
                schema: "rh",
                table: "rh_fol_historico_salarial",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_hora_extra_sync_id",
                schema: "rh",
                table: "rh_fol_hora_extra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_hora_extra_tenant_id",
                schema: "rh",
                table: "rh_fol_hora_extra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_sync_id",
                schema: "rh",
                table: "rh_fol_inss",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_tenant_id",
                schema: "rh",
                table: "rh_fol_inss",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_retencao_sync_id",
                schema: "rh",
                table: "rh_fol_inss_retencao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_retencao_tenant_id",
                schema: "rh",
                table: "rh_fol_inss_retencao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_servico_sync_id",
                schema: "rh",
                table: "rh_fol_inss_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_inss_servico_tenant_id",
                schema: "rh",
                table: "rh_fol_inss_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_lancamento_sync_id",
                schema: "rh",
                table: "rh_fol_lancamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_lancamento_tenant_id",
                schema: "rh",
                table: "rh_fol_lancamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_lancamento_item_sync_id",
                schema: "rh",
                table: "rh_fol_lancamento_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_lancamento_item_tenant_id",
                schema: "rh",
                table: "rh_fol_lancamento_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_pacote_item_sync_id",
                schema: "rh",
                table: "rh_fol_pacote_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_pacote_item_tenant_id",
                schema: "rh",
                table: "rh_fol_pacote_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_pacote_salarial_sync_id",
                schema: "rh",
                table: "rh_fol_pacote_salarial",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_pacote_salarial_tenant_id",
                schema: "rh",
                table: "rh_fol_pacote_salarial",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_pagamento_diario_sync_id",
                schema: "rh",
                table: "rh_fol_pagamento_diario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_pagamento_diario_tenant_id",
                schema: "rh",
                table: "rh_fol_pagamento_diario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_pagamento_diario_item_sync_id",
                schema: "rh",
                table: "rh_fol_pagamento_diario_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_pagamento_diario_item_tenant_id",
                schema: "rh",
                table: "rh_fol_pagamento_diario_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_parametro_sync_id",
                schema: "rh",
                table: "rh_fol_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_parametro_tenant_id",
                schema: "rh",
                table: "rh_fol_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_plano_saude_sync_id",
                schema: "rh",
                table: "rh_fol_plano_saude",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_plano_saude_tenant_id",
                schema: "rh",
                table: "rh_fol_plano_saude",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_presenca_diaria_sync_id",
                schema: "rh",
                table: "rh_fol_presenca_diaria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_presenca_diaria_tenant_id",
                schema: "rh",
                table: "rh_fol_presenca_diaria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_rescisao_sync_id",
                schema: "rh",
                table: "rh_fol_rescisao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_rescisao_tenant_id",
                schema: "rh",
                table: "rh_fol_rescisao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_rubrica_sync_id",
                schema: "rh",
                table: "rh_fol_rubrica",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_rubrica_tenant_id",
                schema: "rh",
                table: "rh_fol_rubrica",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_adicional_sync_id",
                schema: "rh",
                table: "rh_fol_tipo_adicional",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_adicional_tenant_id",
                schema: "rh",
                table: "rh_fol_tipo_adicional",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_desconto_sync_id",
                schema: "rh",
                table: "rh_fol_tipo_desconto",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_desconto_tenant_id",
                schema: "rh",
                table: "rh_fol_tipo_desconto",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_emprestimo_sync_id",
                schema: "rh",
                table: "rh_fol_tipo_emprestimo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_tipo_emprestimo_tenant_id",
                schema: "rh",
                table: "rh_fol_tipo_emprestimo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__fol_vale_transporte_sync_id",
                schema: "rh",
                table: "rh_fol_vale_transporte",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fol_vale_transporte_tenant_id",
                schema: "rh",
                table: "rh_fol_vale_transporte",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_abono_sync_id",
                schema: "rh",
                table: "rh_pnt_abono",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_abono_tenant_id",
                schema: "rh",
                table: "rh_pnt_abono",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_banco_horas_sync_id",
                schema: "rh",
                table: "rh_pnt_banco_horas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_banco_horas_tenant_id",
                schema: "rh",
                table: "rh_pnt_banco_horas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_classificacao_jornada_sync_id",
                schema: "rh",
                table: "rh_pnt_classificacao_jornada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_classificacao_jornada_tenant_id",
                schema: "rh",
                table: "rh_pnt_classificacao_jornada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_escala_sync_id",
                schema: "rh",
                table: "rh_pnt_escala",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_escala_tenant_id",
                schema: "rh",
                table: "rh_pnt_escala",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_evento_folha_sync_id",
                schema: "rh",
                table: "rh_pnt_evento_folha",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_evento_folha_tenant_id",
                schema: "rh",
                table: "rh_pnt_evento_folha",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_fechamento_jornada_sync_id",
                schema: "rh",
                table: "rh_pnt_fechamento_jornada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_fechamento_jornada_tenant_id",
                schema: "rh",
                table: "rh_pnt_fechamento_jornada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_historico_sync_id",
                schema: "rh",
                table: "rh_pnt_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_historico_tenant_id",
                schema: "rh",
                table: "rh_pnt_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_horario_sync_id",
                schema: "rh",
                table: "rh_pnt_horario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_horario_tenant_id",
                schema: "rh",
                table: "rh_pnt_horario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_importacao_afd_sync_id",
                schema: "rh",
                table: "rh_pnt_importacao_afd",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_importacao_afd_tenant_id",
                schema: "rh",
                table: "rh_pnt_importacao_afd",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_marcacao_sync_id",
                schema: "rh",
                table: "rh_pnt_marcacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_marcacao_tenant_id",
                schema: "rh",
                table: "rh_pnt_marcacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_parametro_sync_id",
                schema: "rh",
                table: "rh_pnt_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_parametro_tenant_id",
                schema: "rh",
                table: "rh_pnt_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_periodo_apuracao_sync_id",
                schema: "rh",
                table: "rh_pnt_periodo_apuracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_periodo_apuracao_tenant_id",
                schema: "rh",
                table: "rh_pnt_periodo_apuracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_presenca_diaria_sync_id",
                schema: "rh",
                table: "rh_pnt_presenca_diaria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_presenca_diaria_tenant_id",
                schema: "rh",
                table: "rh_pnt_presenca_diaria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_relogio_sync_id",
                schema: "rh",
                table: "rh_pnt_relogio",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_relogio_tenant_id",
                schema: "rh",
                table: "rh_pnt_relogio",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_restricao_ip_sync_id",
                schema: "rh",
                table: "rh_pnt_restricao_ip",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_restricao_ip_tenant_id",
                schema: "rh",
                table: "rh_pnt_restricao_ip",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pnt_turma_sync_id",
                schema: "rh",
                table: "rh_pnt_turma",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pnt_turma_tenant_id",
                schema: "rh",
                table: "rh_pnt_turma",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_avaliacao_candidato_sync_id",
                schema: "rh",
                table: "rh_rec_avaliacao_candidato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_avaliacao_candidato_tenant_id",
                schema: "rh",
                table: "rh_rec_avaliacao_candidato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_candidato_sync_id",
                schema: "rh",
                table: "rh_rec_candidato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_candidato_tenant_id",
                schema: "rh",
                table: "rh_rec_candidato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_checklist_item_sync_id",
                schema: "rh",
                table: "rh_rec_checklist_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_checklist_item_tenant_id",
                schema: "rh",
                table: "rh_rec_checklist_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_checklist_onboarding_sync_id",
                schema: "rh",
                table: "rh_rec_checklist_onboarding",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_checklist_onboarding_tenant_id",
                schema: "rh",
                table: "rh_rec_checklist_onboarding",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_configuracao_sync_id",
                schema: "rh",
                table: "rh_rec_configuracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_configuracao_tenant_id",
                schema: "rh",
                table: "rh_rec_configuracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_entrevista_sync_id",
                schema: "rh",
                table: "rh_rec_entrevista",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_entrevista_tenant_id",
                schema: "rh",
                table: "rh_rec_entrevista",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_feedback_entrevista_sync_id",
                schema: "rh",
                table: "rh_rec_feedback_entrevista",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_feedback_entrevista_tenant_id",
                schema: "rh",
                table: "rh_rec_feedback_entrevista",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_fonte_candidato_sync_id",
                schema: "rh",
                table: "rh_rec_fonte_candidato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_fonte_candidato_tenant_id",
                schema: "rh",
                table: "rh_rec_fonte_candidato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_historico_sync_id",
                schema: "rh",
                table: "rh_rec_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_historico_tenant_id",
                schema: "rh",
                table: "rh_rec_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_local_vaga_sync_id",
                schema: "rh",
                table: "rh_rec_local_vaga",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_local_vaga_tenant_id",
                schema: "rh",
                table: "rh_rec_local_vaga",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_modelo_carta_sync_id",
                schema: "rh",
                table: "rh_rec_modelo_carta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_modelo_carta_tenant_id",
                schema: "rh",
                table: "rh_rec_modelo_carta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_oferta_sync_id",
                schema: "rh",
                table: "rh_rec_oferta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_oferta_tenant_id",
                schema: "rh",
                table: "rh_rec_oferta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_onboarding_candidato_sync_id",
                schema: "rh",
                table: "rh_rec_onboarding_candidato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_onboarding_candidato_tenant_id",
                schema: "rh",
                table: "rh_rec_onboarding_candidato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_pergunta_customizada_sync_id",
                schema: "rh",
                table: "rh_rec_pergunta_customizada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_pergunta_customizada_tenant_id",
                schema: "rh",
                table: "rh_rec_pergunta_customizada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_round_entrevista_sync_id",
                schema: "rh",
                table: "rh_rec_round_entrevista",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_round_entrevista_tenant_id",
                schema: "rh",
                table: "rh_rec_round_entrevista",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_tipo_entrevista_sync_id",
                schema: "rh",
                table: "rh_rec_tipo_entrevista",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_tipo_entrevista_tenant_id",
                schema: "rh",
                table: "rh_rec_tipo_entrevista",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_tipo_vaga_sync_id",
                schema: "rh",
                table: "rh_rec_tipo_vaga",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_tipo_vaga_tenant_id",
                schema: "rh",
                table: "rh_rec_tipo_vaga",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__rec_vaga_sync_id",
                schema: "rh",
                table: "rh_rec_vaga",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rec_vaga_tenant_id",
                schema: "rh",
                table: "rh_rec_vaga",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_aso_sync_id",
                schema: "rh",
                table: "rh_sso_aso",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_aso_tenant_id",
                schema: "rh",
                table: "rh_sso_aso",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_bloqueio_alocacao_sync_id",
                schema: "rh",
                table: "rh_sso_bloqueio_alocacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_bloqueio_alocacao_tenant_id",
                schema: "rh",
                table: "rh_sso_bloqueio_alocacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_documento_programa_sync_id",
                schema: "rh",
                table: "rh_sso_documento_programa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_documento_programa_tenant_id",
                schema: "rh",
                table: "rh_sso_documento_programa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_entrega_epi_sync_id",
                schema: "rh",
                table: "rh_sso_entrega_epi",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_entrega_epi_tenant_id",
                schema: "rh",
                table: "rh_sso_entrega_epi",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_exame_medico_sync_id",
                schema: "rh",
                table: "rh_sso_exame_medico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_exame_medico_tenant_id",
                schema: "rh",
                table: "rh_sso_exame_medico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_historico_sync_id",
                schema: "rh",
                table: "rh_sso_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_historico_tenant_id",
                schema: "rh",
                table: "rh_sso_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_indicador_acidente_sync_id",
                schema: "rh",
                table: "rh_sso_indicador_acidente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_indicador_acidente_tenant_id",
                schema: "rh",
                table: "rh_sso_indicador_acidente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_sync_id",
                schema: "rh",
                table: "rh_sso_ppp",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_tenant_id",
                schema: "rh",
                table: "rh_sso_ppp",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_atividade_sync_id",
                schema: "rh",
                table: "rh_sso_ppp_atividade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_atividade_tenant_id",
                schema: "rh",
                table: "rh_sso_ppp_atividade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_cat_sync_id",
                schema: "rh",
                table: "rh_sso_ppp_cat",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_cat_tenant_id",
                schema: "rh",
                table: "rh_sso_ppp_cat",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_fator_risco_sync_id",
                schema: "rh",
                table: "rh_sso_ppp_fator_risco",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_ppp_fator_risco_tenant_id",
                schema: "rh",
                table: "rh_sso_ppp_fator_risco",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__sso_treinamento_nr_sync_id",
                schema: "rh",
                table: "rh_sso_treinamento_nr",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sso_treinamento_nr_tenant_id",
                schema: "rh",
                table: "rh_sso_treinamento_nr",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rh_fol_adiantamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_adicional",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_afastamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_bonus_deducao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_competencia",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_configuracao_mensal",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_desconto",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_emprestimo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_ferias_coletivas",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_guia_acumulada",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_historico_salarial",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_hora_extra",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_inss",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_inss_retencao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_inss_servico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_lancamento",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_lancamento_item",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_pacote_item",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_pacote_salarial",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_pagamento_diario",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_pagamento_diario_item",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_parametro",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_plano_saude",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_presenca_diaria",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_rescisao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_rubrica",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_tipo_adicional",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_tipo_desconto",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_tipo_emprestimo",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_fol_vale_transporte",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_abono",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_banco_horas",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_classificacao_jornada",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_escala",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_evento_folha",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_fechamento_jornada",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_horario",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_importacao_afd",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_marcacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_parametro",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_periodo_apuracao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_presenca_diaria",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_relogio",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_restricao_ip",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_pnt_turma",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_avaliacao_candidato",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_candidato",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_checklist_item",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_checklist_onboarding",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_configuracao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_entrevista",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_feedback_entrevista",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_fonte_candidato",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_local_vaga",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_modelo_carta",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_oferta",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_onboarding_candidato",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_pergunta_customizada",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_round_entrevista",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_tipo_entrevista",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_tipo_vaga",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_rec_vaga",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_aso",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_bloqueio_alocacao",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_documento_programa",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_entrega_epi",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_exame_medico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_historico",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_indicador_acidente",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_ppp",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_ppp_atividade",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_ppp_cat",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_ppp_fator_risco",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "rh_sso_treinamento_nr",
                schema: "rh");
        }
    }
}
