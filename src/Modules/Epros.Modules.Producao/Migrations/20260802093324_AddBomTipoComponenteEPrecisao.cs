using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Producao.Migrations
{
    /// <inheritdoc />
    public partial class AddBomTipoComponenteEPrecisao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_componente",
                schema: "producao",
                table: "prd_bom_componente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo_componente",
                schema: "producao",
                table: "prd_bom_componente");

        }
    }
}
