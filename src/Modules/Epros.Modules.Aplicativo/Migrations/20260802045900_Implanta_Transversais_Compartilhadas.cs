using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_Transversais_Compartilhadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentos_ged",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tamanho_bytes = table.Column<long>(type: "bigint", nullable: false),
                    mime_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    versao = table.Column<int>(type: "integer", nullable: false),
                    storage_ref = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    modulo_origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entidade_origem_tipo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    entidade_origem_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status_assinatura = table.Column<int>(type: "integer", nullable: false),
                    assinado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("p_k_documentos_ged", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registros_auditoria",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    entidade = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entidade_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    acao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valores_antes = table.Column<string>(type: "text", nullable: true),
                    valores_depois = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ip_origem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_registros_auditoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sequencias_numeracao",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ultimo_valor = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("p_k_sequencias_numeracao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__documento_ged_sync_id",
                schema: "aplicativo",
                table: "documentos_ged",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__documento_ged_tenant_id",
                schema: "aplicativo",
                table: "documentos_ged",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_documentos_ged_origem",
                schema: "aplicativo",
                table: "documentos_ged",
                columns: new[] { "entidade_origem_tipo", "entidade_origem_id" });

            migrationBuilder.CreateIndex(
                name: "ix_documentos_ged_tenant_hash",
                schema: "aplicativo",
                table: "documentos_ged",
                columns: new[] { "tenant_id", "hash" });

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_ocorrido_em",
                schema: "aplicativo",
                table: "registros_auditoria",
                column: "ocorrido_em");

            migrationBuilder.CreateIndex(
                name: "ix_registros_auditoria_tenant_entidade",
                schema: "aplicativo",
                table: "registros_auditoria",
                columns: new[] { "tenant_id", "entidade", "entidade_id" });

            migrationBuilder.CreateIndex(
                name: "ix__sequencia_numeracao_sync_id",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sequencia_numeracao_tenant_id",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_sequencias_numeracao_tenant_tipo",
                schema: "aplicativo",
                table: "sequencias_numeracao",
                columns: new[] { "tenant_id", "tipo_documento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos_ged",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "registros_auditoria",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "sequencias_numeracao",
                schema: "aplicativo");

        }
    }
}
