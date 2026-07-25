using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPerfisAcessoQuery(
        string? Localizar,
        bool? Ativo,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterPerfilAcessoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class PerfilAcessoMenuDto
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public Guid? MenuItemNivel1Id { get; set; }
        public Guid? MenuItemNivel2Id { get; set; }
        public bool Ver { get; set; }
        public bool Editar { get; set; }
        public bool Excluir { get; set; }
    }

    public class PerfilAcessoDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public List<PerfilAcessoMenuDto> Acessos { get; set; } = new();
    }
}
