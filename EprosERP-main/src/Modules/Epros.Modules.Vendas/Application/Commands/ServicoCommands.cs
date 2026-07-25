using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Catálogo de serviços (VEN-GSV) =====================

    public record CriarServicoCatalogoCommand(
        string Nome,
        string? Descricao,
        decimal Valor,
        decimal Taxa,
        bool PermiteCamposCustomizados,
        string? GrupoPrecoLocalidadeJson,
        decimal? TaxaEmbalagemEntrega,
        string? TipoTaxaEmbalagemEntrega) : ICommand;

    public record AtualizarServicoCatalogoCommand(
        Guid Id,
        string Nome,
        string? Descricao,
        decimal Valor,
        decimal Taxa,
        bool PermiteCamposCustomizados,
        string? GrupoPrecoLocalidadeJson,
        decimal? TaxaEmbalagemEntrega,
        string? TipoTaxaEmbalagemEntrega) : ICommand;

    public record ExcluirServicoCatalogoCommand(Guid Id) : ICommand;
    public record ReativarServicoCatalogoCommand(Guid Id) : ICommand;

    // ===================== Fatura de serviço (VEN-GSV) =====================

    public record ServicoFaturaLinhaInput(
        Guid ServicoId,
        string? NomeServico,
        string? Descricao,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal DescontoPercentual);

    public record CriarServicoFaturaCommand(
        Guid EmpresaId,
        Guid ClienteId,
        Guid? FuncionarioId,
        Guid? ContaPagamentoId,
        Guid? UsuarioId,
        DateTime? DataFatura,
        decimal DescontoCabecalho,
        decimal CustoEnvioEntrega,
        decimal ValorPago,
        string? Detalhes,
        List<ServicoFaturaLinhaInput> Linhas) : ICommand;

    public record AtualizarServicoFaturaCommand(
        Guid Id,
        Guid? FuncionarioId,
        Guid? ContaPagamentoId,
        decimal DescontoCabecalho,
        decimal CustoEnvioEntrega,
        decimal ValorPago,
        string? Detalhes,
        List<ServicoFaturaLinhaInput> Linhas) : ICommand;

    public record ConfirmarServicoFaturaCommand(Guid Id) : ICommand;
    public record FaturarServicoFaturaCommand(Guid Id) : ICommand;
    public record CancelarServicoFaturaCommand(Guid Id) : ICommand;
    public record ExcluirServicoFaturaCommand(Guid Id) : ICommand;
}
