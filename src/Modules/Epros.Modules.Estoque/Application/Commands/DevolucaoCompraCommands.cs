using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos do submódulo Devolução de Compra (CD4 / EF DEVOLUCAO_DE_COMPRA). Submódulo NOVO (0 código
    /// legado). ABAC nega por padrão. Rascunho não gera efeito (DEV-003); confirmação publica saída de
    /// estoque (motor único D1) + estorno financeiro idempotente por compra (DEV-006) via Outbox.
    /// CFOP e sentido = valida-contador (NF-06): o CFOP é parametrizado, nunca fixo em código.
    /// </summary>
    public record DevolucaoCompraItemInput(
        Guid ProdutoId,
        decimal Quantidade,
        decimal ValorUnitario,
        Guid? CompraItemOrigemId = null
    );

    public record CriarDevolucaoCompraCommand(
        Guid CompraOrigemId,
        Guid FornecedorId,
        string Numero,
        DateTime DataDevolucao,
        ETipoDevolucaoCompra Tipo,
        List<DevolucaoCompraItemInput> Itens,
        string? Motivo = null,
        string? Cfop = null
    ) : ICommand;

    public record AtualizarDevolucaoCompraCommand(
        Guid Id,
        DateTime DataDevolucao,
        ETipoDevolucaoCompra Tipo,
        List<DevolucaoCompraItemInput> Itens,
        string? Motivo = null,
        string? Cfop = null
    ) : ICommand;

    public record ConfirmarDevolucaoCompraCommand(Guid Id) : ICommand;

    public record CancelarDevolucaoCompraCommand(Guid Id) : ICommand;

    public record ExcluirDevolucaoCompraCommand(Guid Id) : ICommand;
}
