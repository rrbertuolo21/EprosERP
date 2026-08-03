using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.ESG.Migrations
{
    /// <summary>
    /// NF-01/A-01 (esg-carbono): rastreabilidade do fator parametrico na emissao de carbono.
    /// Remove a dependencia de fator hardcoded — a emissao passa a guardar codigo/versao/fonte do
    /// fator do catalogo esg.ghg_fator_emissao usado, ou o marcador "pendente de fator" (Regra #0).
    /// </summary>
    public partial class AddEmissaoCarbonoFatorParametrico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fator_codigo",
                schema: "esg",
                table: "emissoes_carbono",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fator_fonte",
                schema: "esg",
                table: "emissoes_carbono",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "fator_pendente",
                schema: "esg",
                table: "emissoes_carbono",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "fator_versao",
                schema: "esg",
                table: "emissoes_carbono",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "i_x_emissoes_carbono_tenant_id_fator_pendente",
                schema: "esg",
                table: "emissoes_carbono",
                columns: new[] { "tenant_id", "fator_pendente" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_emissoes_carbono_tenant_id_fator_pendente",
                schema: "esg",
                table: "emissoes_carbono");

            migrationBuilder.DropColumn(
                name: "fator_codigo",
                schema: "esg",
                table: "emissoes_carbono");

            migrationBuilder.DropColumn(
                name: "fator_fonte",
                schema: "esg",
                table: "emissoes_carbono");

            migrationBuilder.DropColumn(
                name: "fator_pendente",
                schema: "esg",
                table: "emissoes_carbono");

            migrationBuilder.DropColumn(
                name: "fator_versao",
                schema: "esg",
                table: "emissoes_carbono");
        }
    }
}
