using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // Porte fiel do legado ProdutoEspecificoController: dados específicos de combustível (GLP/gás natural) de um produto.
    public record OrigemCombustivelInput(
        EOrigemTributacaoCombustivel IndicadorImportacao,
        EEstado UfOrigem,
        decimal ValorPercentualUf
    );

    public record CriarProdutoEspecificoCommand(
        Guid ProdutoId,
        decimal ValorPercentualGlpDerivadoPetroleo,
        decimal ValorPercentualGasNaturalNacional,
        decimal ValorPercentualGasNaturalImportado,
        decimal ValorPartida,
        EEstado UfConsumo,
        List<OrigemCombustivelInput>? Origens
    ) : ICommand;

    public record AtualizarProdutoEspecificoCommand(
        Guid Id,
        decimal ValorPercentualGlpDerivadoPetroleo,
        decimal ValorPercentualGasNaturalNacional,
        decimal ValorPercentualGasNaturalImportado,
        decimal ValorPartida,
        EEstado UfConsumo,
        List<OrigemCombustivelInput>? Origens
    ) : ICommand;

    public record DeletarProdutoEspecificoCommand(Guid Id) : ICommand;
}
