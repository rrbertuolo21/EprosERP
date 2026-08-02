using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Manutencao.Migrations
{
    /// <summary>
    /// T5 — Unificacao da ordem de servico canonica (man_trb_ordem_servico):
    /// rastreio de origem (OrigemTipo/OrigemId) para as OS geradas por PRV/PDT/PAR/CRV
    /// e relaxamento de pessoa_id para nullable (ordem interna nao exige cliente).
    /// </summary>
    /// <inheritdoc />
    public partial class AddOrigemOrdemServicoT5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "pessoa_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "origem_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origem_tipo",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.CreateIndex(
                name: "i_x_man_trb_ordem_servico_tenant_id_origem_tipo_origem_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                columns: new[] { "tenant_id", "origem_tipo", "origem_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_man_trb_ordem_servico_tenant_id_origem_tipo_origem_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico");

            migrationBuilder.DropColumn(
                name: "origem_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico");

            migrationBuilder.DropColumn(
                name: "origem_tipo",
                schema: "manutencao",
                table: "man_trb_ordem_servico");

            migrationBuilder.AlterColumn<Guid>(
                name: "pessoa_id",
                schema: "manutencao",
                table: "man_trb_ordem_servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
