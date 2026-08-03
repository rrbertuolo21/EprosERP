using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Imobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "imobiliaria");

            migrationBuilder.CreateTable(
                name: "imo_contrato_servico",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proprietario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remuneracao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_imo_contrato_servico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_imovel",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    municipio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    test1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    test2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_imo_imovel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    periodo_inicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    periodo_final = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vencimento = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("p_k_imo_locacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imo_imovel_custo",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    competencia = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_imo_imovel_custo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_imovel_custo_imo_imovel_imovel_id",
                        column: x => x.imovel_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_imovel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_imovel_imagem",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_imo_imovel_imagem", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_imovel_imagem_imo_imovel_imovel_id",
                        column: x => x.imovel_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_imovel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_imovel_proprietario",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("p_k_imo_imovel_proprietario", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_imovel_proprietario_imo_imovel_imovel_id",
                        column: x => x.imovel_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_imovel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_imovel_vistoria",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    imovel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    data_vistoria = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_imo_imovel_vistoria", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_imovel_vistoria_imo_imovel_imovel_id",
                        column: x => x.imovel_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_imovel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_custo",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    custo_imovel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("p_k_imo_locacao_custo", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_locacao_custo_imo_locacao_locacao_id",
                        column: x => x.locacao_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_locacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_documento",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    nome_arquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("p_k_imo_locacao_documento", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_locacao_documento_imo_locacao_locacao_id",
                        column: x => x.locacao_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_locacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "imo_locacao_parte",
                schema: "imobiliaria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    locacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    papel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("p_k_imo_locacao_parte", x => x.id);
                    table.ForeignKey(
                        name: "f_k_imo_locacao_parte_imo_locacao_locacao_id",
                        column: x => x.locacao_id,
                        principalSchema: "imobiliaria",
                        principalTable: "imo_locacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_contrato_servico_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_contrato_servico",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_contrato_servico_tenant_id_proprietario_id",
                schema: "imobiliaria",
                table: "imo_contrato_servico",
                columns: new[] { "tenant_id", "proprietario_id" });

            migrationBuilder.CreateIndex(
                name: "ix__contrato_servico_sync_id",
                schema: "imobiliaria",
                table: "imo_contrato_servico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__contrato_servico_tenant_id",
                schema: "imobiliaria",
                table: "imo_contrato_servico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_tenant_id_municipio_id",
                schema: "imobiliaria",
                table: "imo_imovel",
                columns: new[] { "tenant_id", "municipio_id" });

            migrationBuilder.CreateIndex(
                name: "ix__imovel_sync_id",
                schema: "imobiliaria",
                table: "imo_imovel",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_tenant_id",
                schema: "imobiliaria",
                table: "imo_imovel",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_custo_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_custo",
                column: "imovel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_custo_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_custo",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__imovel_custo_sync_id",
                schema: "imobiliaria",
                table: "imo_imovel_custo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_custo_tenant_id",
                schema: "imobiliaria",
                table: "imo_imovel_custo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_imagem_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_imagem",
                column: "imovel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_imagem_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_imagem",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__imovel_imagem_sync_id",
                schema: "imobiliaria",
                table: "imo_imovel_imagem",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_imagem_tenant_id",
                schema: "imobiliaria",
                table: "imo_imovel_imagem",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_proprietario_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_proprietario",
                column: "imovel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_proprietario_tenant_id_imovel_id_pessoa_id",
                schema: "imobiliaria",
                table: "imo_imovel_proprietario",
                columns: new[] { "tenant_id", "imovel_id", "pessoa_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_proprietario_sync_id",
                schema: "imobiliaria",
                table: "imo_imovel_proprietario",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_proprietario_tenant_id",
                schema: "imobiliaria",
                table: "imo_imovel_proprietario",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_vistoria_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_vistoria",
                column: "imovel_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_imovel_vistoria_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_imovel_vistoria",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "ix__imovel_vistoria_sync_id",
                schema: "imobiliaria",
                table: "imo_imovel_vistoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__imovel_vistoria_tenant_id",
                schema: "imobiliaria",
                table: "imo_imovel_vistoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_tenant_id_imovel_id",
                schema: "imobiliaria",
                table: "imo_locacao",
                columns: new[] { "tenant_id", "imovel_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_tenant_id_periodo_inicial_periodo_final",
                schema: "imobiliaria",
                table: "imo_locacao",
                columns: new[] { "tenant_id", "periodo_inicial", "periodo_final" });

            migrationBuilder.CreateIndex(
                name: "ix__locacao_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_custo_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_custo",
                column: "locacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_custo_tenant_id_custo_imovel_id",
                schema: "imobiliaria",
                table: "imo_locacao_custo",
                columns: new[] { "tenant_id", "custo_imovel_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_custo_tenant_id_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_custo",
                columns: new[] { "tenant_id", "locacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__locacao_custo_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_custo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_custo_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_custo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_documento_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_documento",
                column: "locacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_documento_tenant_id_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_documento",
                columns: new[] { "tenant_id", "locacao_id" });

            migrationBuilder.CreateIndex(
                name: "ix__locacao_documento_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_documento",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_documento_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_documento",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_parte_locacao_id",
                schema: "imobiliaria",
                table: "imo_locacao_parte",
                column: "locacao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_imo_locacao_parte_tenant_id_locacao_id_pessoa_id_papel",
                schema: "imobiliaria",
                table: "imo_locacao_parte",
                columns: new[] { "tenant_id", "locacao_id", "pessoa_id", "papel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_parte_sync_id",
                schema: "imobiliaria",
                table: "imo_locacao_parte",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__locacao_parte_tenant_id",
                schema: "imobiliaria",
                table: "imo_locacao_parte",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imo_contrato_servico",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_imovel_custo",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_imovel_imagem",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_imovel_proprietario",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_imovel_vistoria",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_custo",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_documento",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao_parte",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_imovel",
                schema: "imobiliaria");

            migrationBuilder.DropTable(
                name: "imo_locacao",
                schema: "imobiliaria");
        }
    }
}
