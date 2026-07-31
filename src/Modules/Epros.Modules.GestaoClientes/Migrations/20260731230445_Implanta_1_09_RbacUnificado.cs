using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Implanta_1_09_RbacUnificado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_papeis_usuario_papel",
                schema: "plataforma",
                table: "usuarios_papeis");

            migrationBuilder.AddColumn<Guid>(
                name: "empresa_id",
                schema: "plataforma",
                table: "usuarios_papeis",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_papeis_usuario_papel_empresa",
                schema: "plataforma",
                table: "usuarios_papeis",
                columns: new[] { "usuario_id", "papel_id", "empresa_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuarios_papeis_usuario_papel_empresa",
                schema: "plataforma",
                table: "usuarios_papeis");

            migrationBuilder.DropColumn(
                name: "empresa_id",
                schema: "plataforma",
                table: "usuarios_papeis");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_papeis_usuario_papel",
                schema: "plataforma",
                table: "usuarios_papeis",
                columns: new[] { "usuario_id", "papel_id" },
                unique: true);
        }
    }
}
