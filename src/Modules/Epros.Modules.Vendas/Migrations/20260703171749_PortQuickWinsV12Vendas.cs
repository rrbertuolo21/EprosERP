using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Vendas.Migrations
{
    /// <inheritdoc />
    public partial class PortQuickWinsV12Vendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Caixa.Status mudou de string (text) para int (enum ECaixaStatus).
            // Postgres nao converte varchar->integer automaticamente; e preciso um USING explicito.
            // Banco zerado (sem dados) => USING (0) e seguro. Substitui o AlterColumn scaffolded.
            migrationBuilder.Sql("ALTER TABLE vendas.caixas ALTER COLUMN status TYPE integer USING (0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "vendas",
                table: "caixas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
