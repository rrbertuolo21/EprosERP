using System;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ─────────────────────────────────────────────────────
    // ContasAPagar (agregado fiel do legado)
    // ─────────────────────────────────────────────────────
    public record CriarContasAPagarCommand(
        Guid PessoaId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid FatoGeradorFinanceiroId,
        string? NomePessoa,
        ESituacao Situacao,
        DateTime DataVencimento,
        DateTime DataEmissao,
        DateTime? DataBaixa,
        string Documento,
        decimal ValorTitulo,
        decimal ValorInicialDesconto,
        decimal ValorInicialMulta,
        decimal ValorInicialJuros,
        decimal ValorInicialAcrescimo,
        int NumeroParcela,
        string? Detalhamento,
        string? JustificativaCancelamento
    ) : IRequest<CommandResult>;

    public record AtualizarContasAPagarCommand(
        Guid Id,
        Guid PessoaId,
        Guid PlanoDeContasFinanceiroItemId,
        string? NomePessoa,
        ESituacao Situacao,
        DateTime DataVencimento,
        DateTime DataEmissao,
        DateTime? DataBaixa,
        string Documento,
        decimal ValorTitulo,
        decimal ValorInicialDesconto,
        decimal ValorInicialMulta,
        decimal ValorInicialJuros,
        decimal ValorInicialAcrescimo,
        int NumeroParcela,
        string? Detalhamento,
        string? JustificativaCancelamento
    ) : IRequest<CommandResult>;

    public record DeletarContasAPagarCommand(Guid Id) : IRequest<CommandResult>;

    public record CancelarContasAPagarCommand(Guid Id, string? JustificativaCancelamento) : IRequest<CommandResult>;

    public record BaixarContasAPagarConciliadoCommand(Guid Id, decimal ValorPago) : IRequest<CommandResult>;

    public record EstornarContasAPagarConciliadoCommand(Guid Id) : IRequest<CommandResult>;

    public record IncluirContasAPagarItemCommand(
        Guid ContasAPagarId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataPagamento
    ) : IRequest<CommandResult>;

    public record AlterarContasAPagarItemCommand(
        Guid ContasAPagarId,
        Guid ContasAPagarItemId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataPagamento
    ) : IRequest<CommandResult>;

    public record DeletarContasAPagarItemCommand(Guid ContasAPagarId, Guid ContasAPagarItemId) : IRequest<CommandResult>;

    // ─────────────────────────────────────────────────────
    // ContasAReceber (agregado fiel do legado)
    // ─────────────────────────────────────────────────────
    public record CriarContasAReceberCommand(
        Guid PessoaId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid FatoGeradorFinanceiroId,
        string? NomePessoa,
        ESituacao Situacao,
        DateTime DataVencimento,
        DateTime DataEmissao,
        DateTime? DataBaixa,
        string Documento,
        decimal ValorTitulo,
        decimal ValorInicialDesconto,
        decimal ValorInicialMulta,
        decimal ValorInicialJuros,
        decimal ValorInicialAcrescimo,
        int NumeroParcela,
        string? Detalhamento,
        string? JustificativaCancelamento
    ) : IRequest<CommandResult>;

    public record AtualizarContasAReceberCommand(
        Guid Id,
        Guid PessoaId,
        Guid PlanoDeContasFinanceiroItemId,
        string? NomePessoa,
        ESituacao Situacao,
        DateTime DataVencimento,
        DateTime DataEmissao,
        DateTime? DataBaixa,
        string Documento,
        decimal ValorTitulo,
        decimal ValorInicialDesconto,
        decimal ValorInicialMulta,
        decimal ValorInicialJuros,
        decimal ValorInicialAcrescimo,
        int NumeroParcela,
        string? Detalhamento,
        string? JustificativaCancelamento
    ) : IRequest<CommandResult>;

    public record DeletarContasAReceberCommand(Guid Id) : IRequest<CommandResult>;

    public record CancelarContasAReceberCommand(Guid Id, string? JustificativaCancelamento) : IRequest<CommandResult>;

    public record BaixarContasAReceberConciliadoCommand(Guid Id, decimal ValorRecebido) : IRequest<CommandResult>;

    public record EstornarContasAReceberConciliadoCommand(Guid Id) : IRequest<CommandResult>;

    public record IncluirContasAReceberItemCommand(
        Guid ContasAReceberId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataRecebimento
    ) : IRequest<CommandResult>;

    public record AlterarContasAReceberItemCommand(
        Guid ContasAReceberId,
        Guid ContasAReceberItemId,
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataRecebimento
    ) : IRequest<CommandResult>;

    public record DeletarContasAReceberItemCommand(Guid ContasAReceberId, Guid ContasAReceberItemId) : IRequest<CommandResult>;
}
