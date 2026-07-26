using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.ESG.Migrations
{
    /// <inheritdoc />
    public partial class AddEsgSubmodulosGhgEhsRelEco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eco_destino",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    triagem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_destino = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data_execucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evidencia_arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_eco_destino", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_devolucao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    natureza_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor_integral = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_devolvido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    devolucao_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    chave_nf_entrada = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero_nf = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    chave_gerada = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    numero_gerado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    business_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    valor_seguro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_outro = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    sequencia_cce = table.Column<int>(type: "integer", nullable: true),
                    transportadora_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transportadora_cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    transportadora_uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    transportadora_cpf_cnpj = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transportadora_ie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transportadora_endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    frete_quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    frete_especie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    frete_marca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    frete_numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    frete_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    veiculo_placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    veiculo_uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    frete_peso_bruto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    frete_peso_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_eco_devolucao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_fluxo",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    devolucao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_eco_fluxo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_medicao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fluxo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_indicador = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    numerador = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    denominador = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fonte = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_apuracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_eco_medicao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_meta",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fluxo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_indicador = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_meta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    formula = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_eco_meta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_triagem",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fluxo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_devolucao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantidade_recebida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    condicao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    destino_proposto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_triagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_eco_triagem", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_acao_corretiva",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incidente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fator_risco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    causa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    eficacia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    evidencia_arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ehs_acao_corretiva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_atividade",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_ehs_id = table.Column<Guid>(type: "uuid", nullable: true),
                    id_folha_ppp = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("p_k_ehs_atividade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_cat",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    incidente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    id_folha_ppp = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_cat = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    data_afastamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_ehs_cat", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_condicionante",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licenca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    periodicidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    evidencia_arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ehs_condicionante", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_fator_risco",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    atividade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    id_folha_ppp = table.Column<Guid>(type: "uuid", nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fator_risco_descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    intensidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tecnica_utilizada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    epc_eficaz = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    epi_eficaz = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ca_epi = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    atendimento_nr061 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atendimento_nr062 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atendimento_nr063 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atendimento_nr064 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    atendimento_nr065 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_ehs_fator_risco", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_incidente",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_ehs_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    gravidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    impacto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_cat = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("p_k_ehs_incidente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_licenca",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_ehs_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    autoridade = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_ehs_licenca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_registro",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ehs_registro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ehs_residuo_movimento",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registro_ehs_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_residuo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    classificacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_movimento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    destino_id = table.Column<Guid>(type: "uuid", nullable: true),
                    evidencia_arquivo_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_ehs_residuo_movimento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_calculo",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dado_atividade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fator_emissao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    formula_versao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resultado_gas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    resultado_c_o2e = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    memoria_calculo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("p_k_ghg_calculo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_consolidacao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimensao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total_c_o2e = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    gerado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_ghg_consolidacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_dado_atividade",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fonte_emissao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_referencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    origem_dado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    referencia_operacional = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_ghg_dado_atividade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_fator_emissao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    versao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fonte_referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unidade_entrada = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unidade_saida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_ghg_fator_emissao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_fonte_emissao",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    escopo = table.Column<int>(type: "integer", nullable: false),
                    categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unidade_organizacional_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ghg_fonte_emissao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ghg_inventario",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    periodo_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criterio_fronteira = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metodologia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_ghg_inventario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_framework",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    versao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_rel_framework", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_indicador_referencia",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_dominio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    origem_entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    origem_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    codigo_indicador = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    regra_composicao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_rel_indicador_referencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_item",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relatorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia = table.Column<int>(type: "integer", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    observacao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    requisito_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_conteudo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status_preenchimento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    justificativa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_rel_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_pendencia",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    relatorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criticidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resolucao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("p_k_rel_pendencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_requisito",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    framework_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_resposta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_rel_requisito", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rel_snapshot",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    indicador_referencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_versao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_corte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_numerico = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_texto = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    unidade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    dimensoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status_origem = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    hash_conteudo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("p_k_rel_snapshot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eco_devolucao_item",
                schema: "esg",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    devolucao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ncm = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    cfop = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    item_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    codigo_barras = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    unidade_medida = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    cst_csosn = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    cst_pis = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    cst_cofins = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    cst_ipi = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    percentual_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_ipi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_reducao_bc = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_base_calculo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_glp = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_gnn = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_gni = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    codigo_anp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    descricao_anp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uf_consumo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    valor_partida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    unidade_tributavel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    quantidade_tributavel = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_bc_st_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_frete = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    modalidade_bc_st = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    valor_bc_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_icms_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_icms_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    percentual_mva_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    origem = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    percentual_st = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_icms_substituto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    valor_icms_st_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_eco_devolucao_item", x => x.id);
                    table.ForeignKey(
                        name: "f_k_eco_devolucao_item_eco_devolucao_devolucao_id",
                        column: x => x.devolucao_id,
                        principalSchema: "esg",
                        principalTable: "eco_devolucao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_eco_destino_tenant_id_triagem_id",
                schema: "esg",
                table: "eco_destino",
                columns: new[] { "tenant_id", "triagem_id" });

            migrationBuilder.CreateIndex(
                name: "ix__destino_eco_sync_id",
                schema: "esg",
                table: "eco_destino",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__destino_eco_tenant_id",
                schema: "esg",
                table: "eco_destino",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_devolucao_tenant_id_chave_nf_entrada",
                schema: "esg",
                table: "eco_devolucao",
                columns: new[] { "tenant_id", "chave_nf_entrada" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_eco_sync_id",
                schema: "esg",
                table: "eco_devolucao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_eco_tenant_id",
                schema: "esg",
                table: "eco_devolucao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_devolucao_item_devolucao_id",
                schema: "esg",
                table: "eco_devolucao_item",
                column: "devolucao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_devolucao_item_tenant_id_devolucao_id",
                schema: "esg",
                table: "eco_devolucao_item",
                columns: new[] { "tenant_id", "devolucao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_item_eco_sync_id",
                schema: "esg",
                table: "eco_devolucao_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__devolucao_item_eco_tenant_id",
                schema: "esg",
                table: "eco_devolucao_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_fluxo_tenant_id_codigo",
                schema: "esg",
                table: "eco_fluxo",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fluxo_circular_sync_id",
                schema: "esg",
                table: "eco_fluxo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fluxo_circular_tenant_id",
                schema: "esg",
                table: "eco_fluxo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_medicao_tenant_id_fluxo_id",
                schema: "esg",
                table: "eco_medicao",
                columns: new[] { "tenant_id", "fluxo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__medicao_circular_sync_id",
                schema: "esg",
                table: "eco_medicao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__medicao_circular_tenant_id",
                schema: "esg",
                table: "eco_medicao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_meta_tenant_id_fluxo_id",
                schema: "esg",
                table: "eco_meta",
                columns: new[] { "tenant_id", "fluxo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__meta_circular_sync_id",
                schema: "esg",
                table: "eco_meta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__meta_circular_tenant_id",
                schema: "esg",
                table: "eco_meta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_eco_triagem_tenant_id_fluxo_id",
                schema: "esg",
                table: "eco_triagem",
                columns: new[] { "tenant_id", "fluxo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__triagem_eco_sync_id",
                schema: "esg",
                table: "eco_triagem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__triagem_eco_tenant_id",
                schema: "esg",
                table: "eco_triagem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_acao_corretiva_tenant_id_incidente_id",
                schema: "esg",
                table: "ehs_acao_corretiva",
                columns: new[] { "tenant_id", "incidente_id" });

            migrationBuilder.CreateIndex(
                name: "ix__acao_corretiva_sync_id",
                schema: "esg",
                table: "ehs_acao_corretiva",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__acao_corretiva_tenant_id",
                schema: "esg",
                table: "ehs_acao_corretiva",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_atividade_tenant_id_registro_ehs_id",
                schema: "esg",
                table: "ehs_atividade",
                columns: new[] { "tenant_id", "registro_ehs_id" });

            migrationBuilder.CreateIndex(
                name: "ix__atividade_ocupacional_sync_id",
                schema: "esg",
                table: "ehs_atividade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__atividade_ocupacional_tenant_id",
                schema: "esg",
                table: "ehs_atividade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_cat_tenant_id_numero_cat",
                schema: "esg",
                table: "ehs_cat",
                columns: new[] { "tenant_id", "numero_cat" });

            migrationBuilder.CreateIndex(
                name: "ix__cat_sync_id",
                schema: "esg",
                table: "ehs_cat",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cat_tenant_id",
                schema: "esg",
                table: "ehs_cat",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_condicionante_tenant_id_licenca_id",
                schema: "esg",
                table: "ehs_condicionante",
                columns: new[] { "tenant_id", "licenca_id" });

            migrationBuilder.CreateIndex(
                name: "ix__condicionante_sync_id",
                schema: "esg",
                table: "ehs_condicionante",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__condicionante_tenant_id",
                schema: "esg",
                table: "ehs_condicionante",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_fator_risco_tenant_id_atividade_id",
                schema: "esg",
                table: "ehs_fator_risco",
                columns: new[] { "tenant_id", "atividade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__fator_risco_sync_id",
                schema: "esg",
                table: "ehs_fator_risco",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fator_risco_tenant_id",
                schema: "esg",
                table: "ehs_fator_risco",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_incidente_tenant_id_registro_ehs_id",
                schema: "esg",
                table: "ehs_incidente",
                columns: new[] { "tenant_id", "registro_ehs_id" });

            migrationBuilder.CreateIndex(
                name: "ix__incidente_sync_id",
                schema: "esg",
                table: "ehs_incidente",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__incidente_tenant_id",
                schema: "esg",
                table: "ehs_incidente",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_licenca_tenant_id_numero_autoridade",
                schema: "esg",
                table: "ehs_licenca",
                columns: new[] { "tenant_id", "numero", "autoridade" });

            migrationBuilder.CreateIndex(
                name: "ix__licenca_ambiental_sync_id",
                schema: "esg",
                table: "ehs_licenca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__licenca_ambiental_tenant_id",
                schema: "esg",
                table: "ehs_licenca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_registro_tenant_id_codigo",
                schema: "esg",
                table: "ehs_registro",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_ehs_sync_id",
                schema: "esg",
                table: "ehs_registro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__registro_ehs_tenant_id",
                schema: "esg",
                table: "ehs_registro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ehs_residuo_movimento_tenant_id_registro_ehs_id",
                schema: "esg",
                table: "ehs_residuo_movimento",
                columns: new[] { "tenant_id", "registro_ehs_id" });

            migrationBuilder.CreateIndex(
                name: "ix__residuo_movimento_sync_id",
                schema: "esg",
                table: "ehs_residuo_movimento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__residuo_movimento_tenant_id",
                schema: "esg",
                table: "ehs_residuo_movimento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_calculo_tenant_id_dado_atividade_id",
                schema: "esg",
                table: "ghg_calculo",
                columns: new[] { "tenant_id", "dado_atividade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__calculo_gee_sync_id",
                schema: "esg",
                table: "ghg_calculo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__calculo_gee_tenant_id",
                schema: "esg",
                table: "ghg_calculo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_consolidacao_tenant_id_inventario_id",
                schema: "esg",
                table: "ghg_consolidacao",
                columns: new[] { "tenant_id", "inventario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__consolidacao_gee_sync_id",
                schema: "esg",
                table: "ghg_consolidacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__consolidacao_gee_tenant_id",
                schema: "esg",
                table: "ghg_consolidacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_dado_atividade_tenant_id_fonte_emissao_id",
                schema: "esg",
                table: "ghg_dado_atividade",
                columns: new[] { "tenant_id", "fonte_emissao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__dado_atividade_gee_sync_id",
                schema: "esg",
                table: "ghg_dado_atividade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__dado_atividade_gee_tenant_id",
                schema: "esg",
                table: "ghg_dado_atividade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_fator_emissao_tenant_id_codigo_versao",
                schema: "esg",
                table: "ghg_fator_emissao",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fator_emissao_gee_sync_id",
                schema: "esg",
                table: "ghg_fator_emissao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fator_emissao_gee_tenant_id",
                schema: "esg",
                table: "ghg_fator_emissao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_fonte_emissao_tenant_id_inventario_id_escopo_categoria",
                schema: "esg",
                table: "ghg_fonte_emissao",
                columns: new[] { "tenant_id", "inventario_id", "escopo", "categoria" });

            migrationBuilder.CreateIndex(
                name: "ix__fonte_emissao_gee_sync_id",
                schema: "esg",
                table: "ghg_fonte_emissao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fonte_emissao_gee_tenant_id",
                schema: "esg",
                table: "ghg_fonte_emissao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ghg_inventario_tenant_id_codigo_versao",
                schema: "esg",
                table: "ghg_inventario",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_gee_sync_id",
                schema: "esg",
                table: "ghg_inventario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__inventario_gee_tenant_id",
                schema: "esg",
                table: "ghg_inventario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_framework_tenant_id_codigo_versao",
                schema: "esg",
                table: "rel_framework",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__framework_rel_sync_id",
                schema: "esg",
                table: "rel_framework",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__framework_rel_tenant_id",
                schema: "esg",
                table: "rel_framework",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_indicador_referencia_tenant_id_item_id",
                schema: "esg",
                table: "rel_indicador_referencia",
                columns: new[] { "tenant_id", "item_id" });

            migrationBuilder.CreateIndex(
                name: "ix__indicador_referencia_sync_id",
                schema: "esg",
                table: "rel_indicador_referencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__indicador_referencia_tenant_id",
                schema: "esg",
                table: "rel_indicador_referencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_item_tenant_id_relatorio_id_sequencia",
                schema: "esg",
                table: "rel_item",
                columns: new[] { "tenant_id", "relatorio_id", "sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_relatorio_esg_sync_id",
                schema: "esg",
                table: "rel_item",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__item_relatorio_esg_tenant_id",
                schema: "esg",
                table: "rel_item",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_pendencia_tenant_id_relatorio_id",
                schema: "esg",
                table: "rel_pendencia",
                columns: new[] { "tenant_id", "relatorio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__pendencia_relatorio_sync_id",
                schema: "esg",
                table: "rel_pendencia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pendencia_relatorio_tenant_id",
                schema: "esg",
                table: "rel_pendencia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_requisito_tenant_id_framework_id_codigo",
                schema: "esg",
                table: "rel_requisito",
                columns: new[] { "tenant_id", "framework_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__requisito_rel_sync_id",
                schema: "esg",
                table: "rel_requisito",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__requisito_rel_tenant_id",
                schema: "esg",
                table: "rel_requisito",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rel_snapshot_tenant_id_indicador_referencia_id",
                schema: "esg",
                table: "rel_snapshot",
                columns: new[] { "tenant_id", "indicador_referencia_id" });

            migrationBuilder.CreateIndex(
                name: "ix__snapshot_indicador_sync_id",
                schema: "esg",
                table: "rel_snapshot",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__snapshot_indicador_tenant_id",
                schema: "esg",
                table: "rel_snapshot",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eco_destino",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_devolucao_item",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_fluxo",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_medicao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_meta",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_triagem",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_acao_corretiva",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_atividade",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_cat",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_condicionante",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_fator_risco",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_incidente",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_licenca",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_registro",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ehs_residuo_movimento",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_calculo",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_consolidacao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_dado_atividade",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_fator_emissao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_fonte_emissao",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "ghg_inventario",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_framework",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_indicador_referencia",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_item",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_pendencia",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_requisito",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "rel_snapshot",
                schema: "esg");

            migrationBuilder.DropTable(
                name: "eco_devolucao",
                schema: "esg");
        }
    }
}
