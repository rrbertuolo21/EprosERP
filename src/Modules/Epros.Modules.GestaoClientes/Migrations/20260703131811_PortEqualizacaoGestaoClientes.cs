using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.GestaoClientes.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoGestaoClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_usuarios_permissoes_perfis_usuarios_perfil_usuario_id",
                schema: "plataforma",
                table: "usuarios_permissoes");

            migrationBuilder.DropTable(
                name: "perfis_usuarios",
                schema: "plataforma");

            migrationBuilder.RenameColumn(
                name: "perfil_usuario_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                newName: "perfil_colaborador_id");

            migrationBuilder.RenameIndex(
                name: "i_x_usuarios_permissoes_perfil_usuario_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                newName: "i_x_usuarios_permissoes_perfil_colaborador_id");

            migrationBuilder.CreateTable(
                name: "perfil_colaborador",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    departamento = table.Column<string>(type: "text", nullable: false),
                    limite_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("p_k_perfil_colaborador", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__perfil_colaborador_sync_id",
                schema: "plataforma",
                table: "perfil_colaborador",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_colaborador_tenant_id",
                schema: "plataforma",
                table: "perfil_colaborador",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_usuarios_permissoes_perfil_colaborador_perfil_colaborador_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                column: "perfil_colaborador_id",
                principalSchema: "plataforma",
                principalTable: "perfil_colaborador",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "f_k_usuarios_permissoes_perfil_colaborador_perfil_colaborador_id",
                schema: "plataforma",
                table: "usuarios_permissoes");

            migrationBuilder.DropTable(
                name: "perfil_colaborador",
                schema: "plataforma");

            migrationBuilder.RenameColumn(
                name: "perfil_colaborador_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                newName: "perfil_usuario_id");

            migrationBuilder.RenameIndex(
                name: "i_x_usuarios_permissoes_perfil_colaborador_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                newName: "i_x_usuarios_permissoes_perfil_usuario_id");

            migrationBuilder.CreateTable(
                name: "perfis_usuarios",
                schema: "plataforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    departamento = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    limite_desconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_perfis_usuarios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_sync_id",
                schema: "plataforma",
                table: "perfis_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_tenant_id",
                schema: "plataforma",
                table: "perfis_usuarios",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "f_k_usuarios_permissoes_perfis_usuarios_perfil_usuario_id",
                schema: "plataforma",
                table: "usuarios_permissoes",
                column: "perfil_usuario_id",
                principalSchema: "plataforma",
                principalTable: "perfis_usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
