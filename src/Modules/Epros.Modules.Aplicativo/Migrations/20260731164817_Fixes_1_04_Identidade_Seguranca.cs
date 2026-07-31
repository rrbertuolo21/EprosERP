using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <summary>
    /// Fixes 1.04 IDENTIDADE_E_CONTEXTO_TENANT (PASS 1) — parte de schema.
    /// Adiciona a coluna <c>jti</c> à <c>sessoes_usuarios</c>, ligando a SessaoUsuario ao claim jti
    /// do JWT emitido no login (logout/revogação — REG-013), com índice de apoio a consultas.
    /// </summary>
    public partial class Fixes_1_04_Identidade_Seguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "jti",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_jti",
                schema: "aplicativo",
                table: "sessoes_usuarios",
                column: "jti");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sessoes_jti",
                schema: "aplicativo",
                table: "sessoes_usuarios");

            migrationBuilder.DropColumn(
                name: "jti",
                schema: "aplicativo",
                table: "sessoes_usuarios");
        }
    }
}
