using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddGeografiaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.AddColumn<Guid>(
                name: "pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<string>(
                name: "codigo_postal_internacional",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linha_endereco1",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linha_endereco2",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "paises",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    codigo_iso_alpha2 = table.Column<string>(type: "text", nullable: false),
                    codigo_iso_alpha3 = table.Column<string>(type: "text", nullable: false),
                    codigo_numerico = table.Column<string>(type: "text", nullable: false),
                    capital = table.Column<string>(type: "text", nullable: true),
                    codigo_discagem = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_paises", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "zonas_entrega",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    cep_inicio = table.Column<string>(type: "text", nullable: false),
                    cep_fim = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("p_k_zonas_entrega", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "formatos_codigo_postal",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    regex = table.Column<string>(type: "text", nullable: false),
                    mascara = table.Column<string>(type: "text", nullable: false),
                    exemplo = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_formatos_codigo_postal", x => x.id);
                    table.ForeignKey(
                        name: "f_k_formatos_codigo_postal__paises_pais_id",
                        column: x => x.pais_id,
                        principalSchema: "plataforma",
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subdivisoes",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_i_s_o31662 = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    territorio_pai_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("p_k_subdivisoes", x => x.id);
                    table.ForeignKey(
                        name: "f_k_subdivisoes_paises_pais_id",
                        column: x => x.pais_id,
                        principalSchema: "plataforma",
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_subdivisoes_subdivisoes_territorio_pai_id",
                        column: x => x.territorio_pai_id,
                        principalSchema: "plataforma",
                        principalTable: "subdivisoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "municipios",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subdivisao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    codigo_ibge = table.Column<long>(type: "bigint", nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("p_k_municipios", x => x.id);
                    table.ForeignKey(
                        name: "f_k_municipios__paises_pais_id",
                        column: x => x.pais_id,
                        principalSchema: "plataforma",
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_municipios__subdivisoes_subdivisao_id",
                        column: x => x.subdivisao_id,
                        principalSchema: "plataforma",
                        principalTable: "subdivisoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "codigos_postais_cache",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pais_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_postal = table.Column<string>(type: "text", nullable: false),
                    logradouro = table.Column<string>(type: "text", nullable: true),
                    bairro = table.Column<string>(type: "text", nullable: true),
                    municipio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    uf = table.Column<string>(type: "text", nullable: true),
                    provedor = table.Column<string>(type: "text", nullable: true),
                    consultado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    falhou = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_falha = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_codigos_postais_cache", x => x.id);
                    table.ForeignKey(
                        name: "f_k_codigos_postais_cache__municipios_municipio_id",
                        column: x => x.municipio_id,
                        principalSchema: "plataforma",
                        principalTable: "municipios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_codigos_postais_cache__paises_pais_id",
                        column: x => x.pais_id,
                        principalSchema: "plataforma",
                        principalTable: "paises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_enderecos_pessoas_municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "i_x_enderecos_pessoas_pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "i_x_enderecos_pessoas_subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "subdivisao_id");

            migrationBuilder.CreateIndex(
                name: "i_x_codigos_postais_cache_municipio_id",
                schema: "plataforma",
                table: "codigos_postais_cache",
                column: "municipio_id");

            migrationBuilder.CreateIndex(
                name: "ix__codigo_postal_cache_sync_id",
                schema: "plataforma",
                table: "codigos_postais_cache",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__codigo_postal_cache_tenant_id",
                schema: "plataforma",
                table: "codigos_postais_cache",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_codigos_postais_cache_pais_cep",
                schema: "plataforma",
                table: "codigos_postais_cache",
                columns: new[] { "pais_id", "codigo_postal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_formatos_codigo_postal_pais_id",
                schema: "plataforma",
                table: "formatos_codigo_postal",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "ix__formato_codigo_postal_sync_id",
                schema: "plataforma",
                table: "formatos_codigo_postal",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__formato_codigo_postal_tenant_id",
                schema: "plataforma",
                table: "formatos_codigo_postal",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_municipios_pais_id",
                schema: "plataforma",
                table: "municipios",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "i_x_municipios_subdivisao_id",
                schema: "plataforma",
                table: "municipios",
                column: "subdivisao_id");

            migrationBuilder.CreateIndex(
                name: "ix__municipio_sync_id",
                schema: "plataforma",
                table: "municipios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__municipio_tenant_id",
                schema: "plataforma",
                table: "municipios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_municipios_codigo_ibge",
                schema: "plataforma",
                table: "municipios",
                column: "codigo_ibge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pais_sync_id",
                schema: "plataforma",
                table: "paises",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__pais_tenant_id",
                schema: "plataforma",
                table: "paises",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_paises_codigo_iso_alpha_2",
                schema: "plataforma",
                table: "paises",
                column: "codigo_iso_alpha2",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paises_codigo_iso_alpha_3",
                schema: "plataforma",
                table: "paises",
                column: "codigo_iso_alpha3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_subdivisoes_pais_id",
                schema: "plataforma",
                table: "subdivisoes",
                column: "pais_id");

            migrationBuilder.CreateIndex(
                name: "i_x_subdivisoes_territorio_pai_id",
                schema: "plataforma",
                table: "subdivisoes",
                column: "territorio_pai_id");

            migrationBuilder.CreateIndex(
                name: "ix__subdivisao_sync_id",
                schema: "plataforma",
                table: "subdivisoes",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__subdivisao_tenant_id",
                schema: "plataforma",
                table: "subdivisoes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix__zona_entrega_sync_id",
                schema: "plataforma",
                table: "zonas_entrega",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__zona_entrega_tenant_id",
                schema: "plataforma",
                table: "zonas_entrega",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_enderecos_pessoas__municipios_municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "municipio_id",
                principalSchema: "plataforma",
                principalTable: "municipios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "f_k_enderecos_pessoas__paises_pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "pais_id",
                principalSchema: "plataforma",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "f_k_enderecos_pessoas__subdivisoes_subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                column: "subdivisao_id",
                principalSchema: "plataforma",
                principalTable: "subdivisoes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_enderecos_pessoas__municipios_municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropForeignKey(
                name: "f_k_enderecos_pessoas__paises_pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropForeignKey(
                name: "f_k_enderecos_pessoas__subdivisoes_subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropTable(
                name: "codigos_postais_cache",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "formatos_codigo_postal",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "zonas_entrega",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "municipios",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "subdivisoes",
                schema: "plataforma");

            migrationBuilder.DropTable(
                name: "paises",
                schema: "plataforma");

            migrationBuilder.DropIndex(
                name: "i_x_enderecos_pessoas_municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropIndex(
                name: "i_x_enderecos_pessoas_pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropIndex(
                name: "i_x_enderecos_pessoas_subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "codigo_postal_internacional",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "latitude",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "linha_endereco1",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "linha_endereco2",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "longitude",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "subdivisao_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.DropColumn(
                name: "municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas");

            migrationBuilder.AddColumn<long>(
                name: "pais_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "municipio_id",
                schema: "plataforma",
                table: "enderecos_pessoas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
