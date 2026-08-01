using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterLgpdPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "consentimento_l_g_p_d",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "data_consentimento",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "desativado_em",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_registro",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "termos_versao",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "token_descadastro",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_subscribers_token_descadastro",
                schema: "aplicativo",
                table: "newsletter_subscribers",
                column: "token_descadastro",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_newsletter_subscribers_token_descadastro",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "consentimento_l_g_p_d",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "data_consentimento",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "desativado_em",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "ip_registro",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "termos_versao",
                schema: "aplicativo",
                table: "newsletter_subscribers");

            migrationBuilder.DropColumn(
                name: "token_descadastro",
                schema: "aplicativo",
                table: "newsletter_subscribers");
        }
    }
}
