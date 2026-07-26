using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Epros.Modules.Aplicativo.Migrations
{
    /// <inheritdoc />
    public partial class PortEqualizacaoAplicativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfis_usuarios_acessos",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "menu_itens_nivel2",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "perfis_usuarios",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "menu_itens_nivel1",
                schema: "aplicativo");

            migrationBuilder.DropTable(
                name: "menus",
                schema: "aplicativo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menus",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfis_usuarios",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_perfis_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu_itens_nivel1",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menu_itens_nivel1", x => x.id);
                    table.ForeignKey(
                        name: "f_k_menu_itens_nivel1_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "aplicativo",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "menu_itens_nivel2",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_nivel1_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    to = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_menu_itens_nivel2", x => x.id);
                    table.ForeignKey(
                        name: "f_k_menu_itens_nivel2_menu_itens_nivel1_menu_item_nivel1_id",
                        column: x => x.menu_item_nivel1_id,
                        principalSchema: "aplicativo",
                        principalTable: "menu_itens_nivel1",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfis_usuarios_acessos",
                schema: "aplicativo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_nivel1_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_item_nivel2_id = table.Column<Guid>(type: "uuid", nullable: true),
                    perfil_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    alterado_por = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    criado_por = table.Column<string>(type: "text", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    editar = table.Column<bool>(type: "boolean", nullable: false),
                    excluir = table.Column<bool>(type: "boolean", nullable: false),
                    sync_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_version = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<string>(type: "text", nullable: false),
                    ver = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_perfis_usuarios_acessos", x => x.id);
                    table.ForeignKey(
                        name: "f_k_perfis_usuarios_acessos_menu_itens_nivel1_menu_item_nivel1_~",
                        column: x => x.menu_item_nivel1_id,
                        principalSchema: "aplicativo",
                        principalTable: "menu_itens_nivel1",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_perfis_usuarios_acessos_menu_itens_nivel2_menu_item_nivel2_~",
                        column: x => x.menu_item_nivel2_id,
                        principalSchema: "aplicativo",
                        principalTable: "menu_itens_nivel2",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_perfis_usuarios_acessos_menus_menu_id",
                        column: x => x.menu_id,
                        principalSchema: "aplicativo",
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_perfis_usuarios_acessos_perfis_usuarios_perfil_usuario_id",
                        column: x => x.perfil_usuario_id,
                        principalSchema: "aplicativo",
                        principalTable: "perfis_usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_menu_itens_nivel1_menu_id",
                schema: "aplicativo",
                table: "menu_itens_nivel1",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "ix__menu_item_nivel1_sync_id",
                schema: "aplicativo",
                table: "menu_itens_nivel1",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__menu_item_nivel1_tenant_id",
                schema: "aplicativo",
                table: "menu_itens_nivel1",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_itens_n1_tenant_menu",
                schema: "aplicativo",
                table: "menu_itens_nivel1",
                columns: new[] { "tenant_id", "menu_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_menu_itens_nivel2_menu_item_nivel1_id",
                schema: "aplicativo",
                table: "menu_itens_nivel2",
                column: "menu_item_nivel1_id");

            migrationBuilder.CreateIndex(
                name: "ix__menu_item_nivel2_sync_id",
                schema: "aplicativo",
                table: "menu_itens_nivel2",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__menu_item_nivel2_tenant_id",
                schema: "aplicativo",
                table: "menu_itens_nivel2",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_menu_itens_n2_tenant_n1",
                schema: "aplicativo",
                table: "menu_itens_nivel2",
                columns: new[] { "tenant_id", "menu_item_nivel1_id" });

            migrationBuilder.CreateIndex(
                name: "ix__menu_sync_id",
                schema: "aplicativo",
                table: "menus",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__menu_tenant_id",
                schema: "aplicativo",
                table: "menus",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_menus_tenant_ordem",
                schema: "aplicativo",
                table: "menus",
                columns: new[] { "tenant_id", "ordem" });

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_sync_id",
                schema: "aplicativo",
                table: "perfis_usuarios",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_tenant_id",
                schema: "aplicativo",
                table: "perfis_usuarios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_usuarios_tenant_descricao",
                schema: "aplicativo",
                table: "perfis_usuarios",
                columns: new[] { "tenant_id", "descricao" });

            migrationBuilder.CreateIndex(
                name: "i_x_perfis_usuarios_acessos_menu_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "i_x_perfis_usuarios_acessos_menu_item_nivel1_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "menu_item_nivel1_id");

            migrationBuilder.CreateIndex(
                name: "i_x_perfis_usuarios_acessos_menu_item_nivel2_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "menu_item_nivel2_id");

            migrationBuilder.CreateIndex(
                name: "i_x_perfis_usuarios_acessos_perfil_usuario_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "perfil_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_acesso_sync_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "sync_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix__perfil_usuario_acesso_tenant_id",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_perfis_usuarios_acessos_tenant_perfil",
                schema: "aplicativo",
                table: "perfis_usuarios_acessos",
                columns: new[] { "tenant_id", "perfil_usuario_id" });
        }
    }
}
