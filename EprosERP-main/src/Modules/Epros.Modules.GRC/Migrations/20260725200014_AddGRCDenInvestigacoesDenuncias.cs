using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GRC.Migrations
{
    /// <inheritdoc />
    public partial class AddGRCDenInvestigacoesDenuncias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "anonima",
                schema: "grc",
                table: "denuncias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "categoria_id",
                schema: "grc",
                table: "denuncias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prioridade",
                schema: "grc",
                table: "denuncias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "resolved_at",
                schema: "grc",
                table: "denuncias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "titulo",
                schema: "grc",
                table: "denuncias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_acompanhamento_hash",
                schema: "grc",
                table: "denuncias",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grc_den_anexo",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    denuncia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resposta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    arquivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sigiloso = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_anexo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_categoria",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    cor = table.Column<string>(type: "text", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    criador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_historico",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    denuncia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip = table.Column<string>(type: "text", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_historico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_investigacao",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    denuncia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investigador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prazo_sla = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    conclusao_proposta = table.Column<string>(type: "text", nullable: true),
                    data_conclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_investigacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_parametro",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_parametro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_participante",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    denuncia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    papel = table.Column<string>(type: "text", nullable: false),
                    nome_declarado = table.Column<string>(type: "text", nullable: true),
                    sigiloso = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_participante", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grc_den_resposta",
                schema: "grc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    denuncia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mensagem = table.Column<string>(type: "text", nullable: false),
                    interna = table.Column<bool>(type: "boolean", nullable: false),
                    criado_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grc_den_resposta", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_denuncias_categoria_id",
                schema: "grc",
                table: "denuncias",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "i_x_denuncias_tenant_id_status",
                schema: "grc",
                table: "denuncias",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_anexo_denuncia_id",
                schema: "grc",
                table: "grc_den_anexo",
                column: "denuncia_id");

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_anexo_sync_id",
                schema: "grc",
                table: "grc_den_anexo",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_anexo_tenant_id",
                schema: "grc",
                table: "grc_den_anexo",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_categoria_tenant_id_nome",
                schema: "grc",
                table: "grc_den_categoria",
                columns: new[] { "tenant_id", "nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_categoria_sync_id",
                schema: "grc",
                table: "grc_den_categoria",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_categoria_tenant_id",
                schema: "grc",
                table: "grc_den_categoria",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_historico_denuncia_id_data_hora",
                schema: "grc",
                table: "grc_den_historico",
                columns: new[] { "denuncia_id", "data_hora" });

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_historico_sync_id",
                schema: "grc",
                table: "grc_den_historico",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_historico_tenant_id",
                schema: "grc",
                table: "grc_den_historico",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_investigacao_denuncia_id_status",
                schema: "grc",
                table: "grc_den_investigacao",
                columns: new[] { "denuncia_id", "status" });

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_investigacao_investigador_id",
                schema: "grc",
                table: "grc_den_investigacao",
                column: "investigador_id");

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_investigacao_sync_id",
                schema: "grc",
                table: "grc_den_investigacao",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_investigacao_tenant_id",
                schema: "grc",
                table: "grc_den_investigacao",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_parametro_tenant_id_chave",
                schema: "grc",
                table: "grc_den_parametro",
                columns: new[] { "tenant_id", "chave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_parametro_sync_id",
                schema: "grc",
                table: "grc_den_parametro",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_parametro_tenant_id",
                schema: "grc",
                table: "grc_den_parametro",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_participante_denuncia_id_papel",
                schema: "grc",
                table: "grc_den_participante",
                columns: new[] { "denuncia_id", "papel" });

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_participante_sync_id",
                schema: "grc",
                table: "grc_den_participante",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_participante_tenant_id",
                schema: "grc",
                table: "grc_den_participante",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "i_x_grc_den_resposta_denuncia_id_interna",
                schema: "grc",
                table: "grc_den_resposta",
                columns: new[] { "denuncia_id", "interna" });

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_resposta_sync_id",
                schema: "grc",
                table: "grc_den_resposta",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__denuncia_resposta_tenant_id",
                schema: "grc",
                table: "grc_den_resposta",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grc_den_anexo",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_categoria",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_historico",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_investigacao",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_parametro",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_participante",
                schema: "grc");

            migrationBuilder.DropTable(
                name: "grc_den_resposta",
                schema: "grc");

            migrationBuilder.DropIndex(
                name: "i_x_denuncias_categoria_id",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropIndex(
                name: "i_x_denuncias_tenant_id_status",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "anonima",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "prioridade",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "resolved_at",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "titulo",
                schema: "grc",
                table: "denuncias");

            migrationBuilder.DropColumn(
                name: "token_acompanhamento_hash",
                schema: "grc",
                table: "denuncias");
        }
    }
}
