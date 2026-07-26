using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoSaaSAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "add_ons",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    alias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    preco_mensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_anual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    midia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false),
                    admin = table.Column<bool>(type: "boolean", nullable: false),
                    parent_add_on_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_add_ons", x => x.id);
                    table.ForeignKey(
                        name: "f_k_add_ons_add_ons_parent_add_on_id",
                        column: x => x.parent_add_on_id,
                        principalSchema: "plataforma",
                        principalTable: "add_ons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "candidatos_duplicata",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_candidatos_duplicata", x => x.id);
                    table.ForeignKey(
                        name: "f_k_candidatos_duplicata__pessoas_pessoa_a_id",
                        column: x => x.pessoa_a_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_candidatos_duplicata__pessoas_pessoa_b_id",
                        column: x => x.pessoa_b_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "capacidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    add_on = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    permission_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_capacidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consentimentos_titular",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    finalidade = table.Column<int>(type: "integer", nullable: false),
                    base_legal = table.Column<int>(type: "integer", nullable: false),
                    data_consentimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_revogacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    canal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_consentimentos_titular", x => x.id);
                    table.ForeignKey(
                        name: "f_k_consentimentos_titular__pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empresa_grupos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("p_k_empresa_grupos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funcionalidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_funcionalidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identificadores_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    validado = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_identificadores_fiscais", x => x.id);
                    table.ForeignKey(
                        name: "f_k_identificadores_fiscais__pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modulos_ativos_usuario",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    origem = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_modulos_ativos_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "niveis_usuario",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    level_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    can_upload = table.Column<bool>(type: "boolean", nullable: false),
                    wait_between_downloads = table.Column<int>(type: "integer", nullable: false),
                    download_speed = table.Column<int>(type: "integer", nullable: false),
                    max_storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    show_site_adverts = table.Column<bool>(type: "boolean", nullable: false),
                    show_upgrade_screen = table.Column<bool>(type: "boolean", nullable: false),
                    days_to_keep_inactive_files = table.Column<int>(type: "integer", nullable: false),
                    concurrent_uploads = table.Column<int>(type: "integer", nullable: false),
                    concurrent_downloads = table.Column<int>(type: "integer", nullable: false),
                    downloads_per24_hours = table.Column<int>(type: "integer", nullable: false),
                    max_download_filesize_allowed = table.Column<long>(type: "bigint", nullable: false),
                    max_remote_download_urls = table.Column<int>(type: "integer", nullable: false),
                    max_upload_size = table.Column<long>(type: "bigint", nullable: false),
                    level_type = table.Column<int>(type: "integer", nullable: false),
                    on_upgrade_page = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_niveis_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "papeis",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    guard_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    editable = table.Column<bool>(type: "boolean", nullable: false),
                    role_system = table.Column<bool>(type: "boolean", nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: true),
                    role_homepage = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    modules = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_papeis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_compradores",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_compradores", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_compradores_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_contadores",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    crc = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_contadores", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_contadores_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_fornecedores",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    grupo_fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    optante_simples_nacional = table.Column<bool>(type: "boolean", nullable: true),
                    localizacao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    sofre_retencao = table.Column<bool>(type: "boolean", nullable: true),
                    cheque_nominal_a = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    observacao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    conta_remetente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    prazo_medio_entrega = table.Column<int>(type: "integer", nullable: true),
                    gera_faturamento = table.Column<bool>(type: "boolean", nullable: true),
                    num_dias_primeiro_vencimento = table.Column<int>(type: "integer", nullable: true),
                    num_dias_intervalo = table.Column<int>(type: "integer", nullable: true),
                    quantidade_parcelas = table.Column<int>(type: "integer", nullable: true),
                    pay_term_number = table.Column<int>(type: "integer", nullable: true),
                    pay_term_type = table.Column<int>(type: "integer", nullable: true),
                    ultima_compra = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_fornecedores", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_fornecedores_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_historico_estado",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_anterior = table.Column<int>(type: "integer", nullable: true),
                    estado_novo = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_historico_estado", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pessoas_historico_estado_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_importacao_lote",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    layout_versao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_linhas = table.Column<int>(type: "integer", nullable: false),
                    linhas_aceitas = table.Column<int>(type: "integer", nullable: false),
                    linhas_rejeitadas = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_importacao_lote", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_log_auditoria",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valor_anterior = table.Column<string>(type: "text", nullable: true),
                    valor_novo = table.Column<string>(type: "text", nullable: true),
                    usuario_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_evento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_log_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_vendedores",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    senha_a_p_p = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    meta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    gestor = table.Column<bool>(type: "boolean", nullable: false),
                    forma_desconto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tipo_desconto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_vendedores", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_vendedores_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regras_deduplicacao",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estrategia = table.Column<int>(type: "integer", nullable: false),
                    peso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    limiar_bloqueio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    limiar_alerta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_regras_deduplicacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relacionamentos_parceiro",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_origem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_relacao = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_relacionamentos_parceiro", x => x.id);
                    table.ForeignKey(
                        name: "f_k_relacionamentos_parceiro_pessoas_pessoa_destino_id",
                        column: x => x.pessoa_destino_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_relacionamentos_parceiro_pessoas_pessoa_origem_id",
                        column: x => x.pessoa_origem_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_titular",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_solicitacoes_titular", x => x.id);
                    table.ForeignKey(
                        name: "f_k_solicitacoes_titular_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_papeis",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_usuarios_papeis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_capacidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capacidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_usuarios_capacidades", x => x.id);
                    table.ForeignKey(
                        name: "f_k_usuarios_capacidades_capacidades_capacidade_id",
                        column: x => x.capacidade_id,
                        principalSchema: "plataforma",
                        principalTable: "capacidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "precos_nivel_usuario",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pricing_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    package_pricing_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    download_allowance = table.Column<long>(type: "bigint", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_precos_nivel_usuario", x => x.id);
                    table.ForeignKey(
                        name: "f_k_precos_nivel_usuario_niveis_usuario_nivel_usuario_id",
                        column: x => x.nivel_usuario_id,
                        principalSchema: "plataforma",
                        principalTable: "niveis_usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "papeis_capacidades",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capacidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_papeis_capacidades", x => x.id);
                    table.ForeignKey(
                        name: "f_k_papeis_capacidades_papeis_papel_id",
                        column: x => x.papel_id,
                        principalSchema: "plataforma",
                        principalTable: "papeis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_importacao_linha",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_linha = table.Column<int>(type: "integer", nullable: false),
                    dados_originais = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    mensagem_erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    pessoa_id_gerada = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_importacao_linha", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pessoas_importacao_linha__pessoas_importacao_lote_lote_id",
                        column: x => x.lote_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas_importacao_lote",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_add_ons_parent_add_on_id",
                schema: "plataforma",
                table: "add_ons",
                column: "parent_add_on_id");

            migrationBuilder.CreateIndex(
                name: "ix__add_on_sync_id",
                schema: "plataforma",
                table: "add_ons",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__add_on_tenant_id",
                schema: "plataforma",
                table: "add_ons",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_addons_nome_modulo",
                schema: "plataforma",
                table: "add_ons",
                column: "nome_modulo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_candidatos_duplicata_pessoa_a_id",
                schema: "plataforma",
                table: "candidatos_duplicata",
                column: "pessoa_a_id");

            migrationBuilder.CreateIndex(
                name: "i_x_candidatos_duplicata_pessoa_b_id",
                schema: "plataforma",
                table: "candidatos_duplicata",
                column: "pessoa_b_id");

            migrationBuilder.CreateIndex(
                name: "ix__candidato_duplicata_sync_id",
                schema: "plataforma",
                table: "candidatos_duplicata",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__candidato_duplicata_tenant_id",
                schema: "plataforma",
                table: "candidatos_duplicata",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidatos_duplicata_tenant_status_pessoas_score",
                schema: "plataforma",
                table: "candidatos_duplicata",
                columns: new[] { "tenant_id", "status", "pessoa_a_id", "pessoa_b_id", "score" });

            migrationBuilder.CreateIndex(
                name: "ix__capacidade_sync_id",
                schema: "plataforma",
                table: "capacidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__capacidade_tenant_id",
                schema: "plataforma",
                table: "capacidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_capacidades_tenant_name",
                schema: "plataforma",
                table: "capacidades",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__consentimento_titular_sync_id",
                schema: "plataforma",
                table: "consentimentos_titular",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__consentimento_titular_tenant_id",
                schema: "plataforma",
                table: "consentimentos_titular",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_consentimentos_titular_pessoa_finalidade",
                schema: "plataforma",
                table: "consentimentos_titular",
                columns: new[] { "pessoa_id", "finalidade", "base_legal", "data_revogacao" });

            migrationBuilder.CreateIndex(
                name: "ix__empresa_grupo_sync_id",
                schema: "plataforma",
                table: "empresa_grupos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_grupo_tenant_id",
                schema: "plataforma",
                table: "empresa_grupos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_grupos_tenant_nome",
                schema: "plataforma",
                table: "empresa_grupos",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__funcionalidade_sync_id",
                schema: "plataforma",
                table: "funcionalidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__funcionalidade_tenant_id",
                schema: "plataforma",
                table: "funcionalidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionalidades_title",
                schema: "plataforma",
                table: "funcionalidades",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_identificadores_fiscais_pessoa_id",
                schema: "plataforma",
                table: "identificadores_fiscais",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix__identificador_fiscal_sync_id",
                schema: "plataforma",
                table: "identificadores_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__identificador_fiscal_tenant_id",
                schema: "plataforma",
                table: "identificadores_fiscais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_identificadores_fiscais_tenant_pais_tipo_valor",
                schema: "plataforma",
                table: "identificadores_fiscais",
                columns: new[] { "tenant_id", "pais_id", "tipo", "valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__modulo_ativo_usuario_sync_id",
                schema: "plataforma",
                table: "modulos_ativos_usuario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__modulo_ativo_usuario_tenant_id",
                schema: "plataforma",
                table: "modulos_ativos_usuario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_modulos_ativos_usuario_tenant_usuario_modulo",
                schema: "plataforma",
                table: "modulos_ativos_usuario",
                columns: new[] { "tenant_id", "usuario_id", "modulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__nivel_usuario_sync_id",
                schema: "plataforma",
                table: "niveis_usuario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__nivel_usuario_tenant_id",
                schema: "plataforma",
                table: "niveis_usuario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_niveis_usuario_tenant_level",
                schema: "plataforma",
                table: "niveis_usuario",
                columns: new[] { "tenant_id", "level_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__papel_sync_id",
                schema: "plataforma",
                table: "papeis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__papel_tenant_id",
                schema: "plataforma",
                table: "papeis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_papeis_tenant_name",
                schema: "plataforma",
                table: "papeis",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__papel_capacidade_sync_id",
                schema: "plataforma",
                table: "papeis_capacidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__papel_capacidade_tenant_id",
                schema: "plataforma",
                table: "papeis_capacidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_papeis_capacidades_papel_capacidade",
                schema: "plataforma",
                table: "papeis_capacidades",
                columns: new[] { "papel_id", "capacidade_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_comprador_sync_id",
                schema: "plataforma",
                table: "pessoas_compradores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_comprador_tenant_id",
                schema: "plataforma",
                table: "pessoas_compradores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_contador_sync_id",
                schema: "plataforma",
                table: "pessoas_contadores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_contador_tenant_id",
                schema: "plataforma",
                table: "pessoas_contadores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_fornecedor_sync_id",
                schema: "plataforma",
                table: "pessoas_fornecedores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_fornecedor_tenant_id",
                schema: "plataforma",
                table: "pessoas_fornecedores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoas_fornecedores_comprador",
                schema: "plataforma",
                table: "pessoas_fornecedores",
                column: "comprador_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoas_fornecedores_grupo",
                schema: "plataforma",
                table: "pessoas_fornecedores",
                column: "grupo_fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_historico_estado_sync_id",
                schema: "plataforma",
                table: "pessoas_historico_estado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_historico_estado_tenant_id",
                schema: "plataforma",
                table: "pessoas_historico_estado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoas_historico_estado_pessoa_data",
                schema: "plataforma",
                table: "pessoas_historico_estado",
                columns: new[] { "pessoa_id", "data_evento" });

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_importacao_linha_sync_id",
                schema: "plataforma",
                table: "pessoas_importacao_linha",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_importacao_linha_tenant_id",
                schema: "plataforma",
                table: "pessoas_importacao_linha",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoas_importacao_linha_lote",
                schema: "plataforma",
                table: "pessoas_importacao_linha",
                column: "lote_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_importacao_lote_sync_id",
                schema: "plataforma",
                table: "pessoas_importacao_lote",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_importacao_lote_tenant_id",
                schema: "plataforma",
                table: "pessoas_importacao_lote",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_log_auditoria_sync_id",
                schema: "plataforma",
                table: "pessoas_log_auditoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_log_auditoria_tenant_id",
                schema: "plataforma",
                table: "pessoas_log_auditoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_pessoas_log_auditoria_tenant_entidade_data",
                schema: "plataforma",
                table: "pessoas_log_auditoria",
                columns: new[] { "tenant_id", "entidade", "entidade_id", "data_evento" });

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_vendedor_sync_id",
                schema: "plataforma",
                table: "pessoas_vendedores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_vendedor_tenant_id",
                schema: "plataforma",
                table: "pessoas_vendedores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_precos_nivel_usuario_nivel_usuario_id",
                schema: "plataforma",
                table: "precos_nivel_usuario",
                column: "nivel_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix__preco_nivel_usuario_sync_id",
                schema: "plataforma",
                table: "precos_nivel_usuario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__preco_nivel_usuario_tenant_id",
                schema: "plataforma",
                table: "precos_nivel_usuario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__regra_deduplicacao_sync_id",
                schema: "plataforma",
                table: "regras_deduplicacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__regra_deduplicacao_tenant_id",
                schema: "plataforma",
                table: "regras_deduplicacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_relacionamentos_parceiro_pessoa_destino_id",
                schema: "plataforma",
                table: "relacionamentos_parceiro",
                column: "pessoa_destino_id");

            migrationBuilder.CreateIndex(
                name: "i_x_relacionamentos_parceiro_pessoa_origem_id",
                schema: "plataforma",
                table: "relacionamentos_parceiro",
                column: "pessoa_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix__relacionamento_parceiro_sync_id",
                schema: "plataforma",
                table: "relacionamentos_parceiro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__relacionamento_parceiro_tenant_id",
                schema: "plataforma",
                table: "relacionamentos_parceiro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_relacionamentos_parceiro_origem_destino_tipo",
                schema: "plataforma",
                table: "relacionamentos_parceiro",
                columns: new[] { "tenant_id", "pessoa_origem_id", "pessoa_destino_id", "tipo_relacao" });

            migrationBuilder.CreateIndex(
                name: "i_x_solicitacoes_titular_pessoa_id",
                schema: "plataforma",
                table: "solicitacoes_titular",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_titular_sync_id",
                schema: "plataforma",
                table: "solicitacoes_titular",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__solicitacao_titular_tenant_id",
                schema: "plataforma",
                table: "solicitacoes_titular",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacoes_titular_tenant_pessoa_status",
                schema: "plataforma",
                table: "solicitacoes_titular",
                columns: new[] { "tenant_id", "pessoa_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_usuarios_capacidades_capacidade_id",
                schema: "plataforma",
                table: "usuarios_capacidades",
                column: "capacidade_id");

            migrationBuilder.CreateIndex(
                name: "ix__usuario_capacidade_sync_id",
                schema: "plataforma",
                table: "usuarios_capacidades",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_capacidade_tenant_id",
                schema: "plataforma",
                table: "usuarios_capacidades",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_capacidades_tenant_usuario_capacidade",
                schema: "plataforma",
                table: "usuarios_capacidades",
                columns: new[] { "tenant_id", "usuario_id", "capacidade_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_papel_sync_id",
                schema: "plataforma",
                table: "usuarios_papeis",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_papel_tenant_id",
                schema: "plataforma",
                table: "usuarios_papeis",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_papeis_usuario_papel",
                schema: "plataforma",
                table: "usuarios_papeis",
                columns: new[] { "usuario_id", "papel_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "add_ons",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "candidatos_duplicata",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "consentimentos_titular",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "empresa_grupos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "funcionalidades",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "identificadores_fiscais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "modulos_ativos_usuario",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "papeis_capacidades",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_compradores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_contadores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_fornecedores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_historico_estado",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_importacao_linha",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_log_auditoria",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_vendedores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "precos_nivel_usuario",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "regras_deduplicacao",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "relacionamentos_parceiro",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "solicitacoes_titular",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "usuarios_capacidades",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "usuarios_papeis",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "papeis",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_importacao_lote",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "niveis_usuario",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "capacidades",
                schema: "plataforma");
        }
    }
}
