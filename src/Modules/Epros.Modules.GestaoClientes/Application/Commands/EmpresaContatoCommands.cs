using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AdicionarEmpresaContatoCommand(
        Guid EmpresaId,
        string Nome,
        string? Email,
        ETipoContatoTelefonico Tipo,
        string? Numero
    ) : ICommand;

    public record AtualizarEmpresaContatoCommand(
        Guid EmpresaId,
        Guid ContatoId,
        string Nome,
        string? Email,
        ETipoContatoTelefonico Tipo,
        string? Numero
    ) : ICommand;

    public record DeletarEmpresaContatoCommand(Guid EmpresaId, Guid ContatoId) : ICommand;

    public record AdicionarIeStCommand(Guid EmpresaId, EEstado Uf, string Ie) : ICommand;

    public record AtualizarIeStCommand(Guid EmpresaId, Guid IeStId, EEstado Uf, string Ie) : ICommand;

    public record DeletarIeStCommand(Guid EmpresaId, Guid IeStId) : ICommand;
}
