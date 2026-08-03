using System;
using System.Collections.Generic;
using Epros.Modules.Vendas.Domain.Enums;
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
        List<ServicoFaturaLinhaInput> Linhas,
        // ISS (valida-contador / NF-03): alíquota ISS por município/atividade entra como PARÂMETRO.
        // Sem parâmetro → stub 0% Exclusivo (não inventar alíquota). Ver ServicoFaturaHandlerBase.
        decimal? AliquotaIssPercentual = null,
        ETipoImpostoServico? TipoImpostoIss = null) : ICommand;

    public record AtualizarServicoFaturaCommand(
        Guid Id,
        Guid? FuncionarioId,
        Guid? ContaPagamentoId,
        decimal DescontoCabecalho,
        decimal CustoEnvioEntrega,
        decimal ValorPago,
        string? Detalhes,
        List<ServicoFaturaLinhaInput> Linhas,
        decimal? AliquotaIssPercentual = null,
        ETipoImpostoServico? TipoImpostoIss = null) : ICommand;

    public record ConfirmarServicoFaturaCommand(Guid Id) : ICommand;
    public record FaturarServicoFaturaCommand(Guid Id) : ICommand;
    public record CancelarServicoFaturaCommand(Guid Id) : ICommand;
    /// <summary>EC-08: estorno de fatura FATURADA (transição distinta de Cancelar), reverte lançamentos.</summary>
    public record EstornarServicoFaturaCommand(Guid Id) : ICommand;
    public record ExcluirServicoFaturaCommand(Guid Id) : ICommand;
}
