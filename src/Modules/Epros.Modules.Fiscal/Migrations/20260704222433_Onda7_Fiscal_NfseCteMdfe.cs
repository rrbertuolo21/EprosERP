using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Fiscal.Migrations
{
    /// <inheritdoc />
    public partial class Onda7_Fiscal_NfseCteMdfe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conhecimentos_transporte_eletronicos",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    tipo_cte = table.Column<int>(type: "integer", nullable: false),
                    modal = table.Column<int>(type: "integer", nullable: false),
                    remetente_documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    destinatario_documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_receber = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    chave_acesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_sefaz = table.Column<int>(type: "integer", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xml_envio = table.Column<string>(type: "text", nullable: true),
                    xml_retorno = table.Column<string>(type: "text", nullable: true),
                    justificativa_cancelamento = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    data_autorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_conhecimentos_transporte_eletronicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manifestos_eletronicos_documentos_fiscais",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    serie = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    modal = table.Column<int>(type: "integer", nullable: false),
                    tipo_emitente = table.Column<int>(type: "integer", nullable: false),
                    uf_inicio = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    uf_fim = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    quantidade_carregados = table.Column<int>(type: "integer", nullable: false),
                    valor_carga = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    chave_acesso = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_sefaz = table.Column<int>(type: "integer", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xml_envio = table.Column<string>(type: "text", nullable: true),
                    xml_retorno = table.Column<string>(type: "text", nullable: true),
                    municipio_encerramento_ibge = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    protocolo_encerramento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_encerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_autorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_manifestos_eletronicos_documentos_fiscais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notas_servico_eletronicas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequencia_exibicao = table.Column<long>(type: "bigint", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rps_numero = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    rps_serie = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    rps_tipo = table.Column<int>(type: "integer", nullable: false),
                    numero_lote = table.Column<int>(type: "integer", nullable: false),
                    ambiente = table.Column<int>(type: "integer", nullable: false),
                    natureza_operacao = table.Column<int>(type: "integer", nullable: false),
                    regime_especial_tributacao = table.Column<int>(type: "integer", nullable: false),
                    optante_simples_nacional = table.Column<bool>(type: "boolean", nullable: false),
                    incentivo_fiscal = table.Column<bool>(type: "boolean", nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestador_documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    prestador_inscricao_municipal = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    prestador_codigo_municipio_ibge = table.Column<int>(type: "integer", nullable: false),
                    tomador_documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    tomador_razao_social = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    item_lista_servico = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    codigo_tributacao_municipio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    codigo_cnae = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    codigo_nbs = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    discriminacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    codigo_municipio_ibge = table.Column<int>(type: "integer", nullable: false),
                    exigibilidade_iss = table.Column<int>(type: "integer", nullable: false),
                    municipio_incidencia = table.Column<int>(type: "integer", nullable: false),
                    valor_servicos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_deducoes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_iss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_iss_retido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    aliquota_iss = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_incondicionado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_condicionado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    iss_retido = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_nfse = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    codigo_verificacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    protocolo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_prefeitura = table.Column<int>(type: "integer", nullable: true),
                    motivo_rejeicao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_autorizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_cancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xml_envio = table.Column<string>(type: "text", nullable: true),
                    xml_retorno = table.Column<string>(type: "text", nullable: true),
                    pdf_caminho = table.Column<string>(type: "text", nullable: true),
                    xml_caminho = table.Column<string>(type: "text", nullable: true),
                    data_emissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notas_servico_eletronicas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_conhecimentos_transporte_eletronicos_tenant_id_chave_acesso",
                schema: "plataforma",
                table: "conhecimentos_transporte_eletronicos",
                columns: new[] { "tenant_id", "chave_acesso" });

            migrationBuilder.CreateIndex(
                name: "i_x_conhecimentos_transporte_eletronicos_tenant_id_status",
                schema: "plataforma",
                table: "conhecimentos_transporte_eletronicos",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__conhecimento_transporte_eletronico_sync_id",
                schema: "plataforma",
                table: "conhecimentos_transporte_eletronicos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__conhecimento_transporte_eletronico_tenant_id",
                schema: "plataforma",
                table: "conhecimentos_transporte_eletronicos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_manifestos_eletronicos_documentos_fiscais_tenant_id_chave_a~",
                schema: "plataforma",
                table: "manifestos_eletronicos_documentos_fiscais",
                columns: new[] { "tenant_id", "chave_acesso" });

            migrationBuilder.CreateIndex(
                name: "i_x_manifestos_eletronicos_documentos_fiscais_tenant_id_status",
                schema: "plataforma",
                table: "manifestos_eletronicos_documentos_fiscais",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__manifesto_eletronico_documentos_fiscais_sync_id",
                schema: "plataforma",
                table: "manifestos_eletronicos_documentos_fiscais",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__manifesto_eletronico_documentos_fiscais_tenant_id",
                schema: "plataforma",
                table: "manifestos_eletronicos_documentos_fiscais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_notas_servico_eletronicas_tenant_id_rps_numero_rps_serie",
                schema: "plataforma",
                table: "notas_servico_eletronicas",
                columns: new[] { "tenant_id", "rps_numero", "rps_serie" });

            migrationBuilder.CreateIndex(
                name: "i_x_notas_servico_eletronicas_tenant_id_status",
                schema: "plataforma",
                table: "notas_servico_eletronicas",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix__nota_servico_eletronica_sync_id",
                schema: "plataforma",
                table: "notas_servico_eletronicas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__nota_servico_eletronica_tenant_id",
                schema: "plataforma",
                table: "notas_servico_eletronicas",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conhecimentos_transporte_eletronicos",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "manifestos_eletronicos_documentos_fiscais",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "notas_servico_eletronicas",
                schema: "plataforma");
        }
    }
}
