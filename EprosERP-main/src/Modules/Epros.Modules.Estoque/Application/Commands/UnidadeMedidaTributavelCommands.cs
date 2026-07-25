using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // Porte fiel do legado UnidadeMedidaTributavelController: tabela de vigência de unidade tributável por NCM.
    public record CriarUnidadeMedidaTributavelCommand(
        string CodigoNcm,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia,
        string UnidadeMedida,
        string Descricao
    ) : ICommand;

    public record AtualizarUnidadeMedidaTributavelCommand(
        Guid Id,
        string CodigoNcm,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia,
        string UnidadeMedida,
        string Descricao
    ) : ICommand;

    public record DeletarUnidadeMedidaTributavelCommand(Guid Id) : ICommand;
}
