using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using System.Collections.Generic;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterMenuDinamicoQuery(string UserId) : IQuery<CommandResult>;

    public class MenuDinamicoDto
    {
        public string Modulo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
        public string Rota { get; set; } = string.Empty;
        public List<string> SubMenus { get; set; } = new();
    }
}
