using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddSincronizacaoGeografica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sincronizacoes_geograficas",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    versao_arquivo = table.Column<string>(type: "text", nullable: false),
                    inseridos = table.Column<int>(type: "integer", nullable: false),
                    atualizados = table.Column<int>(type: "integer", nullable: false),
                    inativados = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    inicio_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mensagem_erro = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("p_k_sincronizacoes_geograficas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__sincronizacao_geografica_sync_id",
                schema: "plataforma",
                table: "sincronizacoes_geograficas",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__sincronizacao_geografica_tenant_id",
                schema: "plataforma",
                table: "sincronizacoes_geograficas",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sincronizacoes_geograficas",
                schema: "plataforma");
        }
    }
}
