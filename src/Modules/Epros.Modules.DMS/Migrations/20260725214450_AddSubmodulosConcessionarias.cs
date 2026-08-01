using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.DMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmodulosConcessionarias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "chassi",
                schema: "concessionarias",
                table: "vendas_veiculos",
                type: "character varying(17)",
                maxLength: 17,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "con_crm_oportunidade",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prospect_id = table.Column<Guid>(type: "uuid", nullable: false),
                    etapa = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    valor_estimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    probabilidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_perda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_crm_oportunidade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_crm_prospect_showroom",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    vendedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_crm_prospect_showroom", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_crm_test_drive",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_demonstracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    termo_documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resultado = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_crm_test_drive", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_dev_contrato",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rede_no_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("p_k_con_dev_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_dev_rede_no",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo_no = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    pai_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pessoa_empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_con_dev_rede_no", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_fin_contrato",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_contrato = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    condicao_final_json = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_fin_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_fin_jornada",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_con_fin_jornada", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_fin_simulacao",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jornada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave_idempotencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    preco_veiculo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    entrada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    prazo_quantidade = table.Column<int>(type: "integer", nullable: false),
                    prazo_unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    origem_versao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    memoria_json = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_fin_simulacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_gar_plano",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    duracao = table.Column<int>(type: "integer", nullable: false),
                    duracao_tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("p_k_con_gar_plano", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_gar_solicitacao",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_garantia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocolo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data_ocorrencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quilometragem = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sintoma = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    relato_cliente = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_gar_solicitacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_gar_veiculo_garantia",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    venda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chassi_vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    plano_versao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inicio_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim_vigencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quilometragem_inicio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    quilometragem_limite = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_con_gar_veiculo_garantia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_mnt_orcamento",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    validade = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_con_mnt_orcamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_mnt_os_extensao",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chassi_vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    quilometragem_entrada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tipo_atendimento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    consultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_oficina = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    data_abertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    previsao_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_mnt_os_extensao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_pes_demanda",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origem_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    prioridade = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("p_k_con_pes_demanda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_pes_peca",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    familia_tecnica = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    criticidade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("p_k_con_pes_peca", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_pes_reserva",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    demanda_id = table.Column<Guid>(type: "uuid", nullable: false),
                    peca_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade_reservada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_pes_reserva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_srv_operacao",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_servico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    tmo_quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tmo_unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    natureza_padrao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_srv_operacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_srv_pacote",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("p_k_con_srv_pacote", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_srv_tipo_servico",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("p_k_con_srv_tipo_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_ven_estoque_veiculo",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chassi_vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    custo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    preco_sugerido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    data_entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_ven_estoque_veiculo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_ven_proposta",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estoque_veiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    valida_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_veiculo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_final = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_con_ven_proposta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "con_ven_reserva",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estoque_veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    oportunidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_con_ven_reserva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "concessionarias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "i_x_con_crm_oportunidade_tenant_id_prospect_id",
                schema: "concessionarias",
                table: "con_crm_oportunidade",
                columns: new[] { "tenant_id", "prospect_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_con_crm_oportunidade_tenant_id_venda_id",
                schema: "concessionarias",
                table: "con_crm_oportunidade",
                columns: new[] { "tenant_id", "venda_id" });

            migrationBuilder.CreateIndex(
                name: "ix__oportunidade_concessionaria_sync_id",
                schema: "concessionarias",
                table: "con_crm_oportunidade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__oportunidade_concessionaria_tenant_id",
                schema: "concessionarias",
                table: "con_crm_oportunidade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_crm_prospect_showroom_tenant_id_contact_id_unidade_id",
                schema: "concessionarias",
                table: "con_crm_prospect_showroom",
                columns: new[] { "tenant_id", "contact_id", "unidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__prospect_showroom_sync_id",
                schema: "concessionarias",
                table: "con_crm_prospect_showroom",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__prospect_showroom_tenant_id",
                schema: "concessionarias",
                table: "con_crm_prospect_showroom",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_crm_test_drive_tenant_id_veiculo_demonstracao_id_inicio~",
                schema: "concessionarias",
                table: "con_crm_test_drive",
                columns: new[] { "tenant_id", "veiculo_demonstracao_id", "inicio", "fim" });

            migrationBuilder.CreateIndex(
                name: "ix__test_drive_sync_id",
                schema: "concessionarias",
                table: "con_crm_test_drive",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__test_drive_tenant_id",
                schema: "concessionarias",
                table: "con_crm_test_drive",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_dev_contrato_tenant_id_rede_no_id",
                schema: "concessionarias",
                table: "con_dev_contrato",
                columns: new[] { "tenant_id", "rede_no_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_rede_sync_id",
                schema: "concessionarias",
                table: "con_dev_contrato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_rede_tenant_id",
                schema: "concessionarias",
                table: "con_dev_contrato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_dev_rede_no_tenant_id_codigo",
                schema: "concessionarias",
                table: "con_dev_rede_no",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rede_no_sync_id",
                schema: "concessionarias",
                table: "con_dev_rede_no",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__rede_no_tenant_id",
                schema: "concessionarias",
                table: "con_dev_rede_no",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_fin_contrato_tenant_id_venda_id",
                schema: "concessionarias",
                table: "con_fin_contrato",
                columns: new[] { "tenant_id", "venda_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_fin_sync_id",
                schema: "concessionarias",
                table: "con_fin_contrato",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_fin_tenant_id",
                schema: "concessionarias",
                table: "con_fin_contrato",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_fin_jornada_tenant_id_oportunidade_id",
                schema: "concessionarias",
                table: "con_fin_jornada",
                columns: new[] { "tenant_id", "oportunidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__jornada_fin_sync_id",
                schema: "concessionarias",
                table: "con_fin_jornada",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__jornada_fin_tenant_id",
                schema: "concessionarias",
                table: "con_fin_jornada",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_fin_simulacao_tenant_id_chave_idempotencia",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                columns: new[] { "tenant_id", "chave_idempotencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__simulacao_fin_sync_id",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__simulacao_fin_tenant_id",
                schema: "concessionarias",
                table: "con_fin_simulacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_gar_plano_tenant_id_codigo",
                schema: "concessionarias",
                table: "con_gar_plano",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_garantia_sync_id",
                schema: "concessionarias",
                table: "con_gar_plano",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__plano_garantia_tenant_id",
                schema: "concessionarias",
                table: "con_gar_plano",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_gar_solicitacao_tenant_id_protocolo",
                schema: "concessionarias",
                table: "con_gar_solicitacao",
                columns: new[] { "tenant_id", "protocolo" });

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_garantia_sync_id",
                schema: "concessionarias",
                table: "con_gar_solicitacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_garantia_tenant_id",
                schema: "concessionarias",
                table: "con_gar_solicitacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_gar_veiculo_garantia_tenant_id_veiculo_id",
                schema: "concessionarias",
                table: "con_gar_veiculo_garantia",
                columns: new[] { "tenant_id", "veiculo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__veiculo_garantia_sync_id",
                schema: "concessionarias",
                table: "con_gar_veiculo_garantia",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__veiculo_garantia_tenant_id",
                schema: "concessionarias",
                table: "con_gar_veiculo_garantia",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_mnt_orcamento_tenant_id_ordem_servico_id_versao",
                schema: "concessionarias",
                table: "con_mnt_orcamento",
                columns: new[] { "tenant_id", "ordem_servico_id", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_manutencao_sync_id",
                schema: "concessionarias",
                table: "con_mnt_orcamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__orcamento_manutencao_tenant_id",
                schema: "concessionarias",
                table: "con_mnt_orcamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_mnt_os_extensao_tenant_id_veiculo_id",
                schema: "concessionarias",
                table: "con_mnt_os_extensao",
                columns: new[] { "tenant_id", "veiculo_id" });

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_manutencao_sync_id",
                schema: "concessionarias",
                table: "con_mnt_os_extensao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ordem_servico_manutencao_tenant_id",
                schema: "concessionarias",
                table: "con_mnt_os_extensao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_pes_demanda_tenant_id_origem_tipo_origem_id_item_origem~",
                schema: "concessionarias",
                table: "con_pes_demanda",
                columns: new[] { "tenant_id", "origem_tipo", "origem_id", "item_origem_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_peca_sync_id",
                schema: "concessionarias",
                table: "con_pes_demanda",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__demanda_peca_tenant_id",
                schema: "concessionarias",
                table: "con_pes_demanda",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_pes_peca_tenant_id_produto_id",
                schema: "concessionarias",
                table: "con_pes_peca",
                columns: new[] { "tenant_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__peca_reposicao_sync_id",
                schema: "concessionarias",
                table: "con_pes_peca",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__peca_reposicao_tenant_id",
                schema: "concessionarias",
                table: "con_pes_peca",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_pes_reserva_tenant_id_demanda_id",
                schema: "concessionarias",
                table: "con_pes_reserva",
                columns: new[] { "tenant_id", "demanda_id" });

            migrationBuilder.CreateIndex(
                name: "ix__reserva_peca_sync_id",
                schema: "concessionarias",
                table: "con_pes_reserva",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reserva_peca_tenant_id",
                schema: "concessionarias",
                table: "con_pes_reserva",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_srv_operacao_tenant_id_codigo_versao",
                schema: "concessionarias",
                table: "con_srv_operacao",
                columns: new[] { "tenant_id", "codigo", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__operacao_servico_sync_id",
                schema: "concessionarias",
                table: "con_srv_operacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__operacao_servico_tenant_id",
                schema: "concessionarias",
                table: "con_srv_operacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_srv_pacote_tenant_id_codigo",
                schema: "concessionarias",
                table: "con_srv_pacote",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pacote_servico_sync_id",
                schema: "concessionarias",
                table: "con_srv_pacote",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pacote_servico_tenant_id",
                schema: "concessionarias",
                table: "con_srv_pacote",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_srv_tipo_servico_tenant_id_codigo",
                schema: "concessionarias",
                table: "con_srv_tipo_servico",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tipo_servico_concessionaria_sync_id",
                schema: "concessionarias",
                table: "con_srv_tipo_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__tipo_servico_concessionaria_tenant_id",
                schema: "concessionarias",
                table: "con_srv_tipo_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_ven_estoque_veiculo_tenant_id_chassi_vin",
                schema: "concessionarias",
                table: "con_ven_estoque_veiculo",
                columns: new[] { "tenant_id", "chassi_vin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_veiculo_sync_id",
                schema: "concessionarias",
                table: "con_ven_estoque_veiculo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__estoque_veiculo_tenant_id",
                schema: "concessionarias",
                table: "con_ven_estoque_veiculo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_ven_proposta_tenant_id_oportunidade_id_versao",
                schema: "concessionarias",
                table: "con_ven_proposta",
                columns: new[] { "tenant_id", "oportunidade_id", "versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__proposta_venda_sync_id",
                schema: "concessionarias",
                table: "con_ven_proposta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__proposta_venda_tenant_id",
                schema: "concessionarias",
                table: "con_ven_proposta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_con_ven_reserva_tenant_id_estoque_veiculo_id_status",
                schema: "concessionarias",
                table: "con_ven_reserva",
                columns: new[] { "tenant_id", "estoque_veiculo_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__reserva_veiculo_sync_id",
                schema: "concessionarias",
                table: "con_ven_reserva",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__reserva_veiculo_tenant_id",
                schema: "concessionarias",
                table: "con_ven_reserva",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_outbox_messages_processado_em",
                schema: "concessionarias",
                table: "outbox_messages",
                column: "processado_em");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "con_crm_oportunidade",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_crm_prospect_showroom",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_crm_test_drive",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_dev_contrato",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_dev_rede_no",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_fin_contrato",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_fin_jornada",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_fin_simulacao",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_gar_plano",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_gar_solicitacao",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_gar_veiculo_garantia",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_mnt_orcamento",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_mnt_os_extensao",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_pes_demanda",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_pes_peca",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_pes_reserva",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_srv_operacao",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_srv_pacote",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_srv_tipo_servico",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_ven_estoque_veiculo",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_ven_proposta",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "con_ven_reserva",
                schema: "concessionarias");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "concessionarias");

            migrationBuilder.AlterColumn<string>(
                name: "chassi",
                schema: "concessionarias",
                table: "vendas_veiculos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(17)",
                oldMaxLength: 17);
        }
    }
}
