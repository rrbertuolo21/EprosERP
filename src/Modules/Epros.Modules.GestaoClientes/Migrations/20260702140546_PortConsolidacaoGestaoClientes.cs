using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class PortConsolidacaoGestaoClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "pessoas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uf",
                schema: "plataforma",
                table: "municipios",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "contador_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "documento_do_recebedor",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_do_recebedor",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "contador_id",
                schema: "plataforma",
                table: "empresas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cpf",
                schema: "plataforma",
                table: "empresas",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "eh_industria",
                schema: "plataforma",
                table: "empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "empresas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_configuracao_estoque",
                schema: "plataforma",
                table: "empresas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "empresa_certificado",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificado_segredo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    senha_segredo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    serial = table.Column<string>(type: "text", nullable: false),
                    titular = table.Column<string>(type: "text", nullable: true),
                    informacao = table.Column<string>(type: "text", nullable: true),
                    cnpj = table.Column<string>(type: "text", nullable: true),
                    validade_inicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    validade_final = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_empresa_certificado", x => x.id);
                    table.ForeignKey(
                        name: "f_k_empresa_certificado_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalSchema: "plataforma",
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empresas_contatos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    tipo_telefone = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_empresas_contatos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_empresas_contatos_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalSchema: "plataforma",
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "empresas_parametros_dfe",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destacar_icms_st = table.Column<bool>(type: "boolean", nullable: false),
                    nfe_serie_producao = table.Column<int>(type: "integer", nullable: true),
                    nfe_proximo_nr_producao = table.Column<long>(type: "bigint", nullable: true),
                    nfe_serie_homologacao = table.Column<int>(type: "integer", nullable: true),
                    nfe_proximo_nr_homologacao = table.Column<long>(type: "bigint", nullable: true),
                    valor_aliquota_credito_icms = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    nfe_gerar_contingencia_em_homologacao = table.Column<bool>(type: "boolean", nullable: true),
                    indicador_st = table.Column<bool>(type: "boolean", nullable: true),
                    emitir_nfe_conjugada = table.Column<bool>(type: "boolean", nullable: true),
                    nfce_csc_homologacao = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    nfce_id_csc_homologacao = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    nfce_serie_homologacao = table.Column<int>(type: "integer", nullable: true),
                    nfce_proximo_nr_homologacao = table.Column<long>(type: "bigint", nullable: true),
                    nfce_gerar_contingencia_em_homologacao = table.Column<bool>(type: "boolean", nullable: true),
                    nfce_csc_producao = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    nfce_id_csc_producao = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    nfce_serie_producao = table.Column<int>(type: "integer", nullable: true),
                    nfce_proximo_nr_producao = table.Column<long>(type: "bigint", nullable: true),
                    tipo_ambiente_nfce = table.Column<string>(type: "text", nullable: false),
                    tipo_ambiente_nfe = table.Column<string>(type: "text", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_empresas_parametros_dfe", x => x.id);
                    table.ForeignKey(
                        name: "f_k_empresas_parametros_dfe_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalSchema: "plataforma",
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ie_sts",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uf = table.Column<string>(type: "text", nullable: false),
                    ie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ie_sts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ie_sts_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalSchema: "plataforma",
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_enderecos_tenant_empresa",
                schema: "plataforma",
                table: "enderecos_pessoas",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "ix__empresa_certificado_sync_id",
                schema: "plataforma",
                table: "empresa_certificado",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_certificado_tenant_id",
                schema: "plataforma",
                table: "empresa_certificado",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_certificado_empresa_serial_validade",
                schema: "plataforma",
                table: "empresa_certificado",
                columns: new[] { "empresa_id", "serial", "validade_final" });

            migrationBuilder.CreateIndex(
                name: "i_x_empresas_contatos_empresa_id",
                schema: "plataforma",
                table: "empresas_contatos",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix__empresa_contato_sync_id",
                schema: "plataforma",
                table: "empresas_contatos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_contato_tenant_id",
                schema: "plataforma",
                table: "empresas_contatos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresas_contatos_tenant_empresa",
                schema: "plataforma",
                table: "empresas_contatos",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_empresas_parametros_dfe_empresa_id",
                schema: "plataforma",
                table: "empresas_parametros_dfe",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_parametros_dfe_sync_id",
                schema: "plataforma",
                table: "empresas_parametros_dfe",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__empresa_parametros_dfe_tenant_id",
                schema: "plataforma",
                table: "empresas_parametros_dfe",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresas_parametros_dfe_tenant_empresa",
                schema: "plataforma",
                table: "empresas_parametros_dfe",
                columns: new[] { "tenant_id", "empresa_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_ie_sts_empresa_id",
                schema: "plataforma",
                table: "ie_sts",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ix__ie_st_sync_id",
                schema: "plataforma",
                table: "ie_sts",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__ie_st_tenant_id",
                schema: "plataforma",
                table: "ie_sts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_iests_tenant_empresa",
                schema: "plataforma",
                table: "ie_sts",
                columns: new[] { "tenant_id", "empresa_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empresa_certificado",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "empresas_contatos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "empresas_parametros_dfe",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "ie_sts",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "ix_enderecos_tenant_empresa",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "pessoas");

            migrationBuilder.DropColumn(
                name: "uf",
                schema: "plataforma",
                table: "municipios");

            migrationBuilder.DropColumn(
                name: "contador_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "documento_do_recebedor",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "nome_do_recebedor",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "contador_id",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "cpf",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "eh_industria",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "sequencia_exibicao",
                schema: "plataforma",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "tipo_configuracao_estoque",
                schema: "plataforma",
                table: "empresas");
        }
    }
}
