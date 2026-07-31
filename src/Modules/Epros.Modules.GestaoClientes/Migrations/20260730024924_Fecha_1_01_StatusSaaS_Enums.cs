using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class Fecha_1_01_StatusSaaS_Enums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1.01 — Materialização dos enums de status (persistidos como string via HasConversion).
            // Não há mudança de schema (colunas seguem 'text'); apenas alinhamento dos valores legados
            // aos nomes dos novos enums. Aditiva e idempotente (WHERE pelo valor antigo).

            // StatusSaaS (Cliente.StatusSaaS): "Active" -> "Ativo".
            migrationBuilder.Sql(
                "UPDATE plataforma.clientes SET status_saa_s = 'Ativo' WHERE status_saa_s = 'Active';");

            // AssinaturaStatus (AssinaturaCliente.Status): "Aprovada" -> "Ativa", "Aguardando" -> "AguardandoAprovacao".
            migrationBuilder.Sql(
                "UPDATE plataforma.assinaturas_clientes SET status = 'Ativa' WHERE status = 'Aprovada';");
            migrationBuilder.Sql(
                "UPDATE plataforma.assinaturas_clientes SET status = 'AguardandoAprovacao' WHERE status = 'Aguardando';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte os valores para a nomenclatura anterior (best-effort).
            migrationBuilder.Sql(
                "UPDATE plataforma.clientes SET status_saa_s = 'Active' WHERE status_saa_s = 'Ativo';");
            migrationBuilder.Sql(
                "UPDATE plataforma.assinaturas_clientes SET status = 'Aprovada' WHERE status = 'Ativa';");
            migrationBuilder.Sql(
                "UPDATE plataforma.assinaturas_clientes SET status = 'Aguardando' WHERE status = 'AguardandoAprovacao';");
        }
    }
}
