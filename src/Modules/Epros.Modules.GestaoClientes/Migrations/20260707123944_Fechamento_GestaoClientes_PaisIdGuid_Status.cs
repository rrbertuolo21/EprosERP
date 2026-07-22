using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Fechamento_GestaoClientes_PaisIdGuid_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // D4: pais_id long->Guid. O Postgres nao converte bigint->uuid nem com USING trivial;
            // a tabela pessoas_veiculos esta vazia (verificado), entao recria-se a coluna.
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos DROP COLUMN pais_id;");
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos ADD COLUMN pais_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';");
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos ALTER COLUMN pais_id DROP DEFAULT;");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pedidos_saa_s",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "faturas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "i_x_pessoas_veiculos_pais_id",
                schema: "plataforma",
                table: "pessoas_veiculos",
                column: "pais_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_pessoas_veiculos_paises_pais_id",
                schema: "plataforma",
                table: "pessoas_veiculos",
                column: "pais_id",
                principalSchema: "plataforma",
                principalTable: "paises",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_pessoas_veiculos_paises_pais_id",
                schema: "plataforma",
                table: "pessoas_veiculos");

            migrationBuilder.DropIndex(
                name: "i_x_pessoas_veiculos_pais_id",
                schema: "plataforma",
                table: "pessoas_veiculos");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "sessoes_pagamentos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos DROP COLUMN pais_id;");
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos ADD COLUMN pais_id bigint NOT NULL DEFAULT 0;");
            migrationBuilder.Sql(
                "ALTER TABLE plataforma.pessoas_veiculos ALTER COLUMN pais_id DROP DEFAULT;");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pedidos_saa_s",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pagamentos_transferencias",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "pagamentos_faturas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "plataforma",
                table: "faturas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
