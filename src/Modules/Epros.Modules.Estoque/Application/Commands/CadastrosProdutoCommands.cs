using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // ---------- CategoriaProduto ----------
    public record CriarCategoriaProdutoCommand(string Descricao, Guid? ProdutoGrupoId) : ICommand;
    public record AtualizarCategoriaProdutoCommand(Guid Id, string Descricao, Guid? ProdutoGrupoId) : ICommand;
    public record DeletarCategoriaProdutoCommand(Guid Id) : ICommand;

    // ---------- MarcaProduto ----------
    public record CriarMarcaProdutoCommand(string Descricao, Guid? ProdutoGrupoId) : ICommand;
    public record AtualizarMarcaProdutoCommand(Guid Id, string Descricao, Guid? ProdutoGrupoId) : ICommand;
    public record DeletarMarcaProdutoCommand(Guid Id) : ICommand;

    // ---------- UnidadeMedidaComercial ----------
    public record CriarUnidadeMedidaComercialCommand(string UnidadeMedida, string Descricao, decimal Fator, Guid? ProdutoGrupoId) : ICommand;
    public record AtualizarUnidadeMedidaComercialCommand(Guid Id, string UnidadeMedida, string Descricao, decimal Fator, Guid? ProdutoGrupoId) : ICommand;
    public record DeletarUnidadeMedidaComercialCommand(Guid Id) : ICommand;
}
