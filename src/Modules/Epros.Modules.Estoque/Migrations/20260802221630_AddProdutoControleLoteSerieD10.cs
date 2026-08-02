using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Estoque.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoControleLoteSerieD10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "controla_lote",
                schema: "estoque",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "exige_serializacao",
                schema: "estoque",
                table: "produtos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "controla_lote",
                schema: "estoque",
                table: "produtos");

            migrationBuilder.DropColumn(
                name: "exige_serializacao",
                schema: "estoque",
                table: "produtos");
        }
    }
}
