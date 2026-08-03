using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCRegCertificadoCofreObrigacaoAlertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_inicio",
                schema: "grc",
                table: "grc_reg_registro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_vencimento",
                schema: "grc",
                table: "grc_reg_registro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "framework",
                schema: "grc",
                table: "grc_reg_registro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "grc",
                table: "grc_reg_registro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "caminho_referencia_segura",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "resumo_certificado",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "senha_referencia_segura",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "validade_fim",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "validade_inicio",
                schema: "grc",
                table: "grc_reg_certificado_digital",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dias_alerta",
                schema: "grc",
                table: "grc_reg_calendario",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "responsavel_id",
                schema: "grc",
                table: "grc_reg_calendario",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tratamento",
                schema: "grc",
                table: "grc_reg_calendario",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_inicio",
                schema: "grc",
                table: "grc_reg_registro");

            migrationBuilder.DropColumn(
                name: "data_vencimento",
                schema: "grc",
                table: "grc_reg_registro");

            migrationBuilder.DropColumn(
                name: "framework",
                schema: "grc",
                table: "grc_reg_registro");

            migrationBuilder.DropColumn(
                name: "nome",
                schema: "grc",
                table: "grc_reg_registro");

            migrationBuilder.DropColumn(
                name: "caminho_referencia_segura",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "nome",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "resumo_certificado",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "senha_referencia_segura",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "validade_fim",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "validade_inicio",
                schema: "grc",
                table: "grc_reg_certificado_digital");

            migrationBuilder.DropColumn(
                name: "dias_alerta",
                schema: "grc",
                table: "grc_reg_calendario");

            migrationBuilder.DropColumn(
                name: "responsavel_id",
                schema: "grc",
                table: "grc_reg_calendario");

            migrationBuilder.DropColumn(
                name: "tratamento",
                schema: "grc",
                table: "grc_reg_calendario");
        }
    }
}
