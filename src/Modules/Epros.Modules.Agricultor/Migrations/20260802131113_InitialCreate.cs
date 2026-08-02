using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Agricultor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agricultor");

            migrationBuilder.CreateTable(
                name: "agr_anotacao_campo",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    talhao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    designado_a = table.Column<Guid>(type: "uuid", nullable: true),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    lat_lng = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_agr_anotacao_campo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_categoria_despesa",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    eh_folha_mao_de_obra = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_agr_categoria_despesa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_colaborador",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    propriedade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    papel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_agr_colaborador", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_cultura",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("p_k_agr_cultura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_despesa",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    propriedade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    talhao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    safra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cod_imovel_lcdpr = table.Column<int>(type: "integer", nullable: true),
                    cod_conta_lcdpr = table.Column<int>(type: "integer", nullable: true),
                    tipo_doc_lcdpr = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tipo_lanc_lcdpr = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    id_partic_lcdpr = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_agr_despesa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_fornecedor",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
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
                    table.PrimaryKey("p_k_agr_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_propriedade_rural",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_imovel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    matricula = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    car = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cad_itr_cafir = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    caepf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    tipo_exploracao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    participacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    codigo_municipio_sped = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    area_total_m2 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_agr_propriedade_rural", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_receita_producao",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    propriedade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    talhao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    safra_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    comprador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    num_nf = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cod_imovel_lcdpr = table.Column<int>(type: "integer", nullable: true),
                    cod_conta_lcdpr = table.Column<int>(type: "integer", nullable: true),
                    id_partic_lcdpr = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_agr_receita_producao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agr_safra",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    talhao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cultura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_plantio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_colheita = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_agr_safra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_escrituracao",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dt_ini = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dt_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ind_situacao_inicio_periodo = table.Column<int>(type: "integer", nullable: false),
                    situacao_especial = table.Column<int>(type: "integer", nullable: false),
                    forma_apuracao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    identificacao_nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    identificacao_cpf_cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
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
                    table.PrimaryKey("p_k_lcdpr_escrituracao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_param_obrigatoriedade",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ano = table.Column<int>(type: "integer", nullable: false),
                    limite_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    origem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_lcdpr_param_obrigatoriedade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "agr_talhao",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    propriedade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cultura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    area_m2 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    poligono_geo_json = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_agr_talhao", x => x.id);
                    table.ForeignKey(
                        name: "f_k_agr_talhao_agr_propriedade_rural_propriedade_id",
                        column: x => x.propriedade_id,
                        principalSchema: "agricultor",
                        principalTable: "agr_propriedade_rural",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_conta",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrituracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_conta = table.Column<int>(type: "integer", nullable: false),
                    banco = table.Column<int>(type: "integer", nullable: true),
                    agencia = table.Column<int>(type: "integer", nullable: true),
                    num_conta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("p_k_lcdpr_conta", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lcdpr_conta_lcdpr_escrituracao_escrituracao_id",
                        column: x => x.escrituracao_id,
                        principalSchema: "agricultor",
                        principalTable: "lcdpr_escrituracao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_dados_cadastrais",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrituracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cod_municipio = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("p_k_lcdpr_dados_cadastrais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lcdpr_dados_cadastrais_lcdpr_escrituracao_escrituracao_id",
                        column: x => x.escrituracao_id,
                        principalSchema: "agricultor",
                        principalTable: "lcdpr_escrituracao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_imovel",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrituracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_imovel = table.Column<int>(type: "integer", nullable: false),
                    cad_itr_cafir = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    caepf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    nome_imovel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cod_municipio = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    tipo_exploracao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    participacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_lcdpr_imovel", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lcdpr_imovel_lcdpr_escrituracao_escrituracao_id",
                        column: x => x.escrituracao_id,
                        principalSchema: "agricultor",
                        principalTable: "lcdpr_escrituracao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_lancamento",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrituracao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_imovel = table.Column<int>(type: "integer", nullable: false),
                    cod_conta = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_doc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    num_doc = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    historico = table.Column<string>(type: "text", nullable: true),
                    id_partic = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    tipo_lanc = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    vl_entrada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vl_saida = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_lcdpr_lancamento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lcdpr_lancamento_lcdpr_escrituracao_escrituracao_id",
                        column: x => x.escrituracao_id,
                        principalSchema: "agricultor",
                        principalTable: "lcdpr_escrituracao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lcdpr_terceiro",
                schema: "agricultor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_imovel = table.Column<int>(type: "integer", nullable: false),
                    tipo_contraparte = table.Column<int>(type: "integer", nullable: false),
                    id_contraparte = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    nome_contraparte = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    perc_contraparte = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_lcdpr_terceiro", x => x.id);
                    table.ForeignKey(
                        name: "f_k_lcdpr_terceiro_lcdpr_imovel_imovel_id",
                        column: x => x.imovel_id,
                        principalSchema: "agricultor",
                        principalTable: "lcdpr_imovel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_agr_anotacao_campo_tenant_id_talhao_id",
                schema: "agricultor",
                table: "agr_anotacao_campo",
                columns: new[] { "tenant_id", "talhao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__anotacao_campo_sync_id",
                schema: "agricultor",
                table: "agr_anotacao_campo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__anotacao_campo_tenant_id",
                schema: "agricultor",
                table: "agr_anotacao_campo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_categoria_despesa_tenant_id_nome",
                schema: "agricultor",
                table: "agr_categoria_despesa",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix__categoria_despesa_sync_id",
                schema: "agricultor",
                table: "agr_categoria_despesa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__categoria_despesa_tenant_id",
                schema: "agricultor",
                table: "agr_categoria_despesa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_colaborador_tenant_id_propriedade_id",
                schema: "agricultor",
                table: "agr_colaborador",
                columns: new[] { "tenant_id", "propriedade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__colaborador_sync_id",
                schema: "agricultor",
                table: "agr_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__colaborador_tenant_id",
                schema: "agricultor",
                table: "agr_colaborador",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_cultura_tenant_id_nome",
                schema: "agricultor",
                table: "agr_cultura",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix__cultura_sync_id",
                schema: "agricultor",
                table: "agr_cultura",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__cultura_tenant_id",
                schema: "agricultor",
                table: "agr_cultura",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_despesa_tenant_id_propriedade_id",
                schema: "agricultor",
                table: "agr_despesa",
                columns: new[] { "tenant_id", "propriedade_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_agr_despesa_tenant_id_safra_id",
                schema: "agricultor",
                table: "agr_despesa",
                columns: new[] { "tenant_id", "safra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__despesa_sync_id",
                schema: "agricultor",
                table: "agr_despesa",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__despesa_tenant_id",
                schema: "agricultor",
                table: "agr_despesa",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_fornecedor_tenant_id_nome",
                schema: "agricultor",
                table: "agr_fornecedor",
                columns: new[] { "tenant_id", "nome" });

            migrationBuilder.CreateIndex(
                name: "ix__fornecedor_sync_id",
                schema: "agricultor",
                table: "agr_fornecedor",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__fornecedor_tenant_id",
                schema: "agricultor",
                table: "agr_fornecedor",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_propriedade_rural_tenant_id_nome_imovel",
                schema: "agricultor",
                table: "agr_propriedade_rural",
                columns: new[] { "tenant_id", "nome_imovel" });

            migrationBuilder.CreateIndex(
                name: "ix__propriedade_rural_sync_id",
                schema: "agricultor",
                table: "agr_propriedade_rural",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__propriedade_rural_tenant_id",
                schema: "agricultor",
                table: "agr_propriedade_rural",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_receita_producao_tenant_id_propriedade_id",
                schema: "agricultor",
                table: "agr_receita_producao",
                columns: new[] { "tenant_id", "propriedade_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_agr_receita_producao_tenant_id_safra_id",
                schema: "agricultor",
                table: "agr_receita_producao",
                columns: new[] { "tenant_id", "safra_id" });

            migrationBuilder.CreateIndex(
                name: "ix__receita_producao_sync_id",
                schema: "agricultor",
                table: "agr_receita_producao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__receita_producao_tenant_id",
                schema: "agricultor",
                table: "agr_receita_producao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_safra_tenant_id_cultura_id",
                schema: "agricultor",
                table: "agr_safra",
                columns: new[] { "tenant_id", "cultura_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_agr_safra_tenant_id_talhao_id",
                schema: "agricultor",
                table: "agr_safra",
                columns: new[] { "tenant_id", "talhao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__safra_sync_id",
                schema: "agricultor",
                table: "agr_safra",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__safra_tenant_id",
                schema: "agricultor",
                table: "agr_safra",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_talhao_propriedade_id",
                schema: "agricultor",
                table: "agr_talhao",
                column: "propriedade_id");

            migrationBuilder.CreateIndex(
                name: "i_x_agr_talhao_tenant_id_propriedade_id",
                schema: "agricultor",
                table: "agr_talhao",
                columns: new[] { "tenant_id", "propriedade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__talhao_sync_id",
                schema: "agricultor",
                table: "agr_talhao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__talhao_tenant_id",
                schema: "agricultor",
                table: "agr_talhao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_conta_escrituracao_id",
                schema: "agricultor",
                table: "lcdpr_conta",
                column: "escrituracao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_conta_tenant_id_escrituracao_id_cod_conta",
                schema: "agricultor",
                table: "lcdpr_conta",
                columns: new[] { "tenant_id", "escrituracao_id", "cod_conta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_conta_sync_id",
                schema: "agricultor",
                table: "lcdpr_conta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_conta_tenant_id",
                schema: "agricultor",
                table: "lcdpr_conta",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_dados_cadastrais_escrituracao_id",
                schema: "agricultor",
                table: "lcdpr_dados_cadastrais",
                column: "escrituracao_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_dados_cadastrais_tenant_id_escrituracao_id",
                schema: "agricultor",
                table: "lcdpr_dados_cadastrais",
                columns: new[] { "tenant_id", "escrituracao_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_dados_cadastrais_sync_id",
                schema: "agricultor",
                table: "lcdpr_dados_cadastrais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_dados_cadastrais_tenant_id",
                schema: "agricultor",
                table: "lcdpr_dados_cadastrais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_escrituracao_tenant_id_cpf_dt_fin",
                schema: "agricultor",
                table: "lcdpr_escrituracao",
                columns: new[] { "tenant_id", "cpf", "dt_fin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_escrituracao_sync_id",
                schema: "agricultor",
                table: "lcdpr_escrituracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_escrituracao_tenant_id",
                schema: "agricultor",
                table: "lcdpr_escrituracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_imovel_escrituracao_id",
                schema: "agricultor",
                table: "lcdpr_imovel",
                column: "escrituracao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_imovel_tenant_id_escrituracao_id_cod_imovel",
                schema: "agricultor",
                table: "lcdpr_imovel",
                columns: new[] { "tenant_id", "escrituracao_id", "cod_imovel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_imovel_sync_id",
                schema: "agricultor",
                table: "lcdpr_imovel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_imovel_tenant_id",
                schema: "agricultor",
                table: "lcdpr_imovel",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_lancamento_escrituracao_id",
                schema: "agricultor",
                table: "lcdpr_lancamento",
                column: "escrituracao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_lancamento_tenant_id_escrituracao_id_data",
                schema: "agricultor",
                table: "lcdpr_lancamento",
                columns: new[] { "tenant_id", "escrituracao_id", "data" });

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_lancamento_sync_id",
                schema: "agricultor",
                table: "lcdpr_lancamento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_lancamento_tenant_id",
                schema: "agricultor",
                table: "lcdpr_lancamento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_param_obrigatoriedade_tenant_id_ano",
                schema: "agricultor",
                table: "lcdpr_param_obrigatoriedade",
                columns: new[] { "tenant_id", "ano" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_param_obrigatoriedade_sync_id",
                schema: "agricultor",
                table: "lcdpr_param_obrigatoriedade",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_param_obrigatoriedade_tenant_id",
                schema: "agricultor",
                table: "lcdpr_param_obrigatoriedade",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_terceiro_imovel_id",
                schema: "agricultor",
                table: "lcdpr_terceiro",
                column: "imovel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_lcdpr_terceiro_tenant_id_imovel_id",
                schema: "agricultor",
                table: "lcdpr_terceiro",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_terceiro_sync_id",
                schema: "agricultor",
                table: "lcdpr_terceiro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__lcdpr_terceiro_tenant_id",
                schema: "agricultor",
                table: "lcdpr_terceiro",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agr_anotacao_campo",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_categoria_despesa",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_colaborador",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_cultura",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_despesa",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_fornecedor",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_receita_producao",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_safra",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_talhao",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_conta",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_dados_cadastrais",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_lancamento",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_param_obrigatoriedade",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_terceiro",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "agr_propriedade_rural",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_imovel",
                schema: "agricultor");

            migrationBuilder.DropTable(
                name: "lcdpr_escrituracao",
                schema: "agricultor");
        }
    }
}
