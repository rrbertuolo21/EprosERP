using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class EnableRlsOnExistingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contratos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_recorrente = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultimo_faturamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_contratos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "empresas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    razao_social = table.Column<string>(type: "text", nullable: false),
                    nome_fantasia = table.Column<string>(type: "text", nullable: true),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    inscricao_estadual = table.Column<string>(type: "text", nullable: true),
                    inscricao_municipal = table.Column<string>(type: "text", nullable: true),
                    inscricao_suframa = table.Column<string>(type: "text", nullable: true),
                    cnae = table.Column<string>(type: "text", nullable: true),
                    regime_tributario = table.Column<int>(type: "integer", nullable: false),
                    regime_apuracao = table.Column<int>(type: "integer", nullable: false),
                    pessoa_grupo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    produto_grupo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plano_contas_financeiro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tributario_grupo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ncm_tributacao_id = table.Column<Guid>(type: "uuid", nullable: true),
                    certificado_digital_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_parametros_dfe_id = table.Column<Guid>(type: "uuid", nullable: true),
                    link_web_api_app_vendas = table.Column<string>(type: "text", nullable: true),
                    token_mercado_pago_pix = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "text", nullable: true),
                    logradouro = table.Column<string>(type: "text", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: false),
                    complemento = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: false),
                    cidade = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis_usuarios",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    departamento = table.Column<string>(type: "text", nullable: false),
                    limite_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_perfis_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoa_grupos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoa_grupos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pessoas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_pessoa = table.Column<int>(type: "integer", nullable: false),
                    tipo_indicador_ie = table.Column<int>(type: "integer", nullable: false),
                    pessoa_grupo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inscricao_suframa = table.Column<long>(type: "bigint", nullable: true),
                    titular_conta_bancaria = table.Column<string>(type: "text", nullable: true),
                    agencia_conta_bancaria = table.Column<string>(type: "text", nullable: true),
                    numero_conta_bancaria = table.Column<string>(type: "text", nullable: true),
                    tipo_pix = table.Column<int>(type: "integer", nullable: true),
                    chave_pix = table.Column<string>(type: "text", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    eh_cliente = table.Column<bool>(type: "boolean", nullable: false),
                    eh_fornecedor = table.Column<bool>(type: "boolean", nullable: false),
                    eh_transportadora = table.Column<bool>(type: "boolean", nullable: false),
                    eh_motorista = table.Column<bool>(type: "boolean", nullable: false),
                    eh_prestador_servico = table.Column<bool>(type: "boolean", nullable: false),
                    eh_funcionario = table.Column<bool>(type: "boolean", nullable: false),
                    eh_produtor_rural = table.Column<bool>(type: "boolean", nullable: false),
                    eh_inativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_pessoas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contrato_itens",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contrato_itens", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contrato_itens_contratos_contrato_id",
                        column: x => x.contrato_id,
                        principalSchema: "plataforma",
                        principalTable: "contratos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_permissoes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    perfil_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso = table.Column<string>(type: "text", nullable: false),
                    acao = table.Column<string>(type: "text", nullable: false),
                    permitido = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_usuarios_permissoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_usuarios_permissoes_perfis_usuarios_perfil_usuario_id",
                        column: x => x.perfil_usuario_id,
                        principalSchema: "plataforma",
                        principalTable: "perfis_usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enderecos_pessoas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_endereco = table.Column<int>(type: "integer", nullable: false),
                    pais_id = table.Column<long>(type: "bigint", nullable: false),
                    municipio_id = table.Column<long>(type: "bigint", nullable: false),
                    uf = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: true),
                    logradouro = table.Column<string>(type: "text", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: true),
                    complemento = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: false),
                    referencia = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_enderecos_pessoas", x => x.id);
                    table.ForeignKey(
                        name: "f_k_enderecos_pessoas__pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_clientes",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eh_consumidor_final = table.Column<bool>(type: "boolean", nullable: false),
                    tipo_contribuinte = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_pessoas_clientes", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_clientes_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_contatos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: true),
                    tipo_contato_telefonico = table.Column<int>(type: "integer", nullable: false),
                    numero_telefone = table.Column<string>(type: "text", nullable: true),
                    tipo_contato_email = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    eh_principal = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_contatos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pessoas_contatos_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_estrangeiros",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    identificacao_estrangeiro = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_pessoas_estrangeiros", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_estrangeiros_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_fisicas",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    sobrenome = table.Column<string>(type: "text", nullable: true),
                    rg_numero = table.Column<string>(type: "text", nullable: true),
                    rg_orgao_emissor = table.Column<string>(type: "text", nullable: true),
                    tipo_genero = table.Column<int>(type: "integer", nullable: false),
                    data_nascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_pessoas_fisicas", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_fisicas_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_funcionarios",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_cargo = table.Column<int>(type: "integer", nullable: false),
                    valor_percentual_comissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_pessoas_funcionarios", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_funcionarios_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_juridicas",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    razao_social = table.Column<string>(type: "text", nullable: false),
                    nome_fantasia = table.Column<string>(type: "text", nullable: true),
                    inscricao_estadual = table.Column<string>(type: "text", nullable: true),
                    inscricao_municipal = table.Column<string>(type: "text", nullable: true),
                    cnae = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_pessoas_juridicas", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_juridicas_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_motoristas",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_vinculo_motorista = table.Column<int>(type: "integer", nullable: false),
                    tipo_categoria_cnh = table.Column<int>(type: "integer", nullable: false),
                    data_emissao_cnh = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_vencimento_cnh = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rntrc = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_pessoas_motoristas", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_motoristas_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_prestadores_servico",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cei = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_pessoas_prestadores_servico", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_prestadores_servico_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_transportadoras",
                schema: "plataforma",
                columns: table => new
                {
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ciot = table.Column<string>(type: "text", nullable: true),
                    rntrc = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_pessoas_transportadoras", x => x.pessoa_id);
                    table.ForeignKey(
                        name: "f_k_pessoas_transportadoras_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pessoas_veiculos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_veiculo = table.Column<int>(type: "integer", nullable: false),
                    uf = table.Column<string>(type: "text", nullable: false),
                    placa = table.Column<string>(type: "text", nullable: false),
                    rntrc = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_pessoas_veiculos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_pessoas_veiculos_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalSchema: "plataforma",
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_contrato_itens_contrato_id",
                schema: "plataforma",
                table: "contrato_itens",
                column: "contrato_id");

            migrationBuilder.CreateIndex(
                name: "ix__contrato_item_sync_id",
                schema: "plataforma",
                table: "contrato_itens",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_item_tenant_id",
                schema: "plataforma",
                table: "contrato_itens",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__contrato_sync_id",
                schema: "plataforma",
                table: "contratos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_tenant_id",
                schema: "plataforma",
                table: "contratos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__empresa_sync_id",
                schema: "plataforma",
                table: "empresas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_tenant_id",
                schema: "plataforma",
                table: "empresas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_enderecos_pessoas_pessoa_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix__endereco_sync_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__endereco_tenant_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_sync_id",
                schema: "plataforma",
                table: "perfis_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_tenant_id",
                schema: "plataforma",
                table: "perfis_usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_grupo_sync_id",
                schema: "plataforma",
                table: "pessoa_grupos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_grupo_tenant_id",
                schema: "plataforma",
                table: "pessoa_grupos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_sync_id",
                schema: "plataforma",
                table: "pessoas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_tenant_id",
                schema: "plataforma",
                table: "pessoas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_cliente_sync_id",
                schema: "plataforma",
                table: "pessoas_clientes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_cliente_tenant_id",
                schema: "plataforma",
                table: "pessoas_clientes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pessoas_contatos_pessoa_id",
                schema: "plataforma",
                table: "pessoas_contatos",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_contato_sync_id",
                schema: "plataforma",
                table: "pessoas_contatos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_contato_tenant_id",
                schema: "plataforma",
                table: "pessoas_contatos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_estrangeiro_sync_id",
                schema: "plataforma",
                table: "pessoas_estrangeiros",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_estrangeiro_tenant_id",
                schema: "plataforma",
                table: "pessoas_estrangeiros",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_fisica_sync_id",
                schema: "plataforma",
                table: "pessoas_fisicas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_fisica_tenant_id",
                schema: "plataforma",
                table: "pessoas_fisicas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_funcionario_sync_id",
                schema: "plataforma",
                table: "pessoas_funcionarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_funcionario_tenant_id",
                schema: "plataforma",
                table: "pessoas_funcionarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_juridica_sync_id",
                schema: "plataforma",
                table: "pessoas_juridicas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_juridica_tenant_id",
                schema: "plataforma",
                table: "pessoas_juridicas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_motorista_sync_id",
                schema: "plataforma",
                table: "pessoas_motoristas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_motorista_tenant_id",
                schema: "plataforma",
                table: "pessoas_motoristas",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_prestador_servico_sync_id",
                schema: "plataforma",
                table: "pessoas_prestadores_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_prestador_servico_tenant_id",
                schema: "plataforma",
                table: "pessoas_prestadores_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_transportadora_sync_id",
                schema: "plataforma",
                table: "pessoas_transportadoras",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_transportadora_tenant_id",
                schema: "plataforma",
                table: "pessoas_transportadoras",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_pessoas_veiculos_pessoa_id",
                schema: "plataforma",
                table: "pessoas_veiculos",
                column: "pessoa_id");

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_veiculo_sync_id",
                schema: "plataforma",
                table: "pessoas_veiculos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pessoa_veiculo_tenant_id",
                schema: "plataforma",
                table: "pessoas_veiculos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_usuarios_permissoes_perfil_usuario_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                column: "perfil_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix__usuario_permissao_sync_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__usuario_permissao_tenant_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                column: "tenant_id");

            // Habilitar RLS e criar políticas para as tabelas pré-existentes da migração inicial
            migrationBuilder.Sql("ALTER TABLE plataforma.planos ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.planos;");
            migrationBuilder.Sql("CREATE POLICY tenant_isolation_policy ON plataforma.planos USING (tenant_id = current_setting('app.current_tenant_id', true));");

            migrationBuilder.Sql("ALTER TABLE plataforma.clientes ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.clientes;");
            migrationBuilder.Sql("CREATE POLICY tenant_isolation_policy ON plataforma.clientes USING (tenant_id = current_setting('app.current_tenant_id', true));");

            migrationBuilder.Sql("ALTER TABLE plataforma.faturas ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.faturas;");
            migrationBuilder.Sql("CREATE POLICY tenant_isolation_policy ON plataforma.faturas USING (tenant_id = current_setting('app.current_tenant_id', true));");

            migrationBuilder.Sql("ALTER TABLE plataforma.modulos_plano ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.modulos_plano;");
            migrationBuilder.Sql("CREATE POLICY tenant_isolation_policy ON plataforma.modulos_plano USING (tenant_id = current_setting('app.current_tenant_id', true));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contrato_itens",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "empresas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "enderecos_pessoas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoa_grupos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_clientes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_contatos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_estrangeiros",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_fisicas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_funcionarios",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_juridicas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_motoristas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_prestadores_servico",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_transportadoras",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas_veiculos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "usuarios_permissoes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "contratos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "pessoas",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "perfis_usuarios",
                schema: "plataforma");

            // Reverter RLS e políticas para as tabelas pré-existentes
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.planos;");
            migrationBuilder.Sql("ALTER TABLE plataforma.planos DISABLE ROW LEVEL SECURITY;");

            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.clientes;");
            migrationBuilder.Sql("ALTER TABLE plataforma.clientes DISABLE ROW LEVEL SECURITY;");

            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.faturas;");
            migrationBuilder.Sql("ALTER TABLE plataforma.faturas DISABLE ROW LEVEL SECURITY;");

            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation_policy ON plataforma.modulos_plano;");
            migrationBuilder.Sql("ALTER TABLE plataforma.modulos_plano DISABLE ROW LEVEL SECURITY;");
        }
    }
}
