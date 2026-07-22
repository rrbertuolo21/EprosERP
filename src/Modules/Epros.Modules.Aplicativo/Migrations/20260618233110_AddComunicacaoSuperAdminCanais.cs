using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class AddComunicacaoSuperAdminCanais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "canais",
                schema: "aplicativo",
                table: "comunicacoes_super_admin",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "canais",
                schema: "aplicativo",
                table: "comunicacoes_super_admin");
        }
    }
}
