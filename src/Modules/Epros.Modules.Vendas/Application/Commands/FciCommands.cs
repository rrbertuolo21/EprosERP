using System;
using System.Collections.Generic;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Faturamento Comercial Internacional (VEN-FCI) =====================
    // Fonte: EF_7_VENDAS_FATURAMENTO_COMERCIAL_INTERNACIONAL_V1. Documento COMERCIAL (não fiscal BR).

    /// <summary>Linha de documento comercial (input). FCI-024.</summary>
    public record FciItemInput(
        Guid ProdutoId,
        Guid UnidadeId,
        decimal Quantidade,
        decimal ValorUnitario,
        Guid? LoteId,
        decimal Desconto,
        Guid? ImpostoId,
        decimal? AliquotaImposto,
        Guid ContaReceitaId,
        Guid? ProjetoId);

    /// <summary>FCI-014: cria documento (fatura 9 / nota de crédito 10) como rascunho — sem estoque nem razão.</summary>
    public record CriarFciDocumentoCommand(
        EFciTipoDocumento TipoDocumento,
        string? Numero,
        DateTime DataDocumento,
        Guid ClienteId,
        Guid ArmazemId,
        int AnoFinanceiroId,
        Guid? DocumentoOrigemId,
        decimal PercentualDesconto,
        EFciTipoDesconto TipoDesconto,
        decimal ValorFrete,
        string? Moeda,
        decimal? TaxaCambio,
        string? Incoterm,
        string? Referencia,
        string? Observacao,
        List<FciItemInput> Itens) : ICommand;

    /// <summary>FCI-015: aprova o documento (grava estoque e razão em transação).</summary>
    public record AprovarFciDocumentoCommand(Guid Id) : ICommand;

    /// <summary>FCI-039..043: edita documento (recria linhas/estoque/razão quando aprovado).</summary>
    public record EditarFciDocumentoCommand(
        Guid Id,
        DateTime DataDocumento,
        decimal PercentualDesconto,
        EFciTipoDesconto TipoDesconto,
        decimal ValorFrete,
        string? Moeda,
        decimal? TaxaCambio,
        string? Incoterm,
        string? Referencia,
        string? Observacao,
        List<FciItemInput> Itens) : ICommand;

    /// <summary>FCI-044: registra pagamento; nota de crédito com saldo zero vira Paga.</summary>
    public record RegistrarPagamentoFciDocumentoCommand(Guid Id, decimal ValorPago) : ICommand;

    /// <summary>FCI-047/048: exclui documento (e efeitos vinculados).</summary>
    public record ExcluirFciDocumentoCommand(Guid Id) : ICommand;

    // ----- Impostos comerciais (FCI-049..052) -----
    public record CriarFciImpostoCommand(string Nome, decimal Aliquota, bool Ativo = true) : ICommand;
    public record AtualizarFciImpostoCommand(Guid Id, string Nome, decimal Aliquota, bool Ativo) : ICommand;
    public record ExcluirFciImpostoCommand(Guid Id) : ICommand;

    // ----- Preferências gerais (FCI-053..055) -----
    public record SalvarFciPreferenciaCommand(
        bool MostrarMoeda,
        bool PermitirCaixaNegativo,
        bool PermitirEstoqueNegativo,
        string? ModoCalculoEstoque,
        bool ControlarLimiteCredito,
        bool PermitirDesconto,
        bool ImpostoNaCompra,
        bool ImpostoNaVenda) : ICommand;
}
