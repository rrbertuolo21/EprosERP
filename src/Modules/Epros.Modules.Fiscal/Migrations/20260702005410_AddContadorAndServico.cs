using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class AddContadorAndServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "codigos_servicos_sefaz",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_codigos_servicos_sefaz", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contadores",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    razao_social = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nome_contador = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    numero_crc = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    uf_crc = table.Column<int>(type: "integer", nullable: false),
                    data_vencimento_crc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    qualificacao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    funcao = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    telefone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    permissao_transmissao = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    logradouro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    municipio_id = table.Column<int>(type: "integer", nullable: false),
                    uf = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contadores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_medida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_servico_sefaz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    informacao_adicional = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    servico_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    cnae = table.Column<int>(type: "integer", nullable: false),
                    codigo_nbs = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    indicador_iss = table.Column<bool>(type: "boolean", nullable: false),
                    indicador_incentivo = table.Column<bool>(type: "boolean", nullable: false),
                    cst_ibs_cbs = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    c_class_trib = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    aliquota_iss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_iss_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_irrf_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_inss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cst_pis_cofins = table.Column<int>(type: "integer", nullable: false),
                    aliquota_pis = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_cofins = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    calcular_retencao = table.Column<bool>(type: "boolean", nullable: false),
                    anexo_simples_nacional = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_servicos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_servicos_codigos_servicos_sefaz_codigo_servico_sefaz_id",
                        column: x => x.codigo_servico_sefaz_id,
                        principalSchema: "plataforma",
                        principalTable: "codigos_servicos_sefaz",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix__codigo_servico_sefaz_sync_id",
                schema: "plataforma",
                table: "codigos_servicos_sefaz",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_servico_sefaz_tenant_id",
                schema: "plataforma",
                table: "codigos_servicos_sefaz",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__contador_sync_id",
                schema: "plataforma",
                table: "contadores",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contador_tenant_id",
                schema: "plataforma",
                table: "contadores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_servicos_codigo_servico_sefaz_id",
                schema: "plataforma",
                table: "servicos",
                column: "codigo_servico_sefaz_id");

            migrationBuilder.CreateIndex(
                name: "ix__servico_sync_id",
                schema: "plataforma",
                table: "servicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__servico_tenant_id",
                schema: "plataforma",
                table: "servicos",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contadores",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "servicos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "codigos_servicos_sefaz",
                schema: "plataforma");
        }
    }
}
