using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>Comandos do submódulo Gestão de Armazém WMS (EST-WMS). EF §7. Sobe desabilitado (ABAC nega por padrão).</summary>
    public record CriarWmsArmazemCommand(
        string Nome,
        string Endereco,
        string Cidade,
        string Cep,
        string? Telefone = null,
        string? Email = null,
        bool? Ativo = null,
        Guid? UsuarioDonoId = null
    ) : ICommand;

    public record AtualizarWmsArmazemCommand(
        Guid Id,
        string Nome,
        string Endereco,
        string Cidade,
        string Cep,
        string? Telefone,
        string? Email,
        bool Ativo
    ) : ICommand;

    public record ExcluirWmsArmazemCommand(Guid Id) : ICommand;
}
