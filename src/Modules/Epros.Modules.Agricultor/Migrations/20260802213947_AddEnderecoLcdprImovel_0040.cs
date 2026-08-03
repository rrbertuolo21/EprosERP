using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Agricultor.Migrations
{
    /// <inheritdoc />
    public partial class AddEnderecoLcdprImovel_0040 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bairro",
                schema: "agricultor",
                table: "lcdpr_imovel",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cep",
                schema: "agricultor",
                table: "lcdpr_imovel",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "compl",
                schema: "agricultor",
                table: "lcdpr_imovel",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "endereco",
                schema: "agricultor",
                table: "lcdpr_imovel",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "num",
                schema: "agricultor",
                table: "lcdpr_imovel",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bairro",
                schema: "agricultor",
                table: "lcdpr_imovel");

            migrationBuilder.DropColumn(
                name: "cep",
                schema: "agricultor",
                table: "lcdpr_imovel");

            migrationBuilder.DropColumn(
                name: "compl",
                schema: "agricultor",
                table: "lcdpr_imovel");

            migrationBuilder.DropColumn(
                name: "endereco",
                schema: "agricultor",
                table: "lcdpr_imovel");

            migrationBuilder.DropColumn(
                name: "num",
                schema: "agricultor",
                table: "lcdpr_imovel");
        }
    }
}
