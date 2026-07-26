using System;
using System.Collections.Generic;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Application.Commands
{
    public record RegistrarVendaCommand(
        string CaixaId,
        decimal Total,
        string Status,
        EModeloDocumento? ModeloFiscal,
        string NaturezaOperacao,
        DateTime DataVenda,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisFisco,
        EModalidadeFrete? ModalidadeFrete,
        EVendaOrigem? VendaOrigem,
        bool IncluirFreteNoTotal,
        Guid? ClienteId,
        decimal ValorDesconto,
        decimal ValorFrete,
        string? FormaPagamento,
        List<VendaItemInput> Itens
    ) : ICommand;

    public record VendaItemInput(
        Guid ProdutoId,
        decimal Quantidade,
        decimal PrecoUnitario,
        string CodigoProduto,
        string? CodigoEan,
        string DescricaoProduto,
        string Ncm,
        Guid? CestId,
        string? Cest,
        Guid? CodigoAnpId,
        string? CodigoAnp,
        int Cfop,
        string UnidadeComercial,
        decimal ValorDesconto = 0m,
        decimal ValorFreteRateado = 0m,
        decimal ValorCusto = 0m
    );
}
