using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposRicosPlanoGrupoModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_fim",
                schema: "plataforma",
                table: "planos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_inicio",
                schema: "plataforma",
                table: "planos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao_completa",
                schema: "plataforma",
                table: "planos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao_curta",
                schema: "plataforma",
                table: "planos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                schema: "plataforma",
                table: "modulos_plano",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                schema: "plataforma",
                table: "modulos_plano",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modulo_geral_id",
                schema: "plataforma",
                table: "modulos_plano",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor",
                schema: "plataforma",
                table: "modulos_plano",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                schema: "plataforma",
                table: "grupo_planos",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_fim",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "data_inicio",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "descricao_completa",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "descricao_curta",
                schema: "plataforma",
                table: "planos");

            migrationBuilder.DropColumn(
                name: "ativo",
                schema: "plataforma",
                table: "modulos_plano");

            migrationBuilder.DropColumn(
                name: "descricao",
                schema: "plataforma",
                table: "modulos_plano");

            migrationBuilder.DropColumn(
                name: "modulo_geral_id",
                schema: "plataforma",
                table: "modulos_plano");

            migrationBuilder.DropColumn(
                name: "valor",
                schema: "plataforma",
                table: "modulos_plano");

            migrationBuilder.DropColumn(
                name: "ativo",
                schema: "plataforma",
                table: "grupo_planos");
        }
    }
}
