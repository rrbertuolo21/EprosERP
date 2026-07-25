using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Conta Financeira -----
    public record CriarContaFinanceiraCommand(string Nome, string? NumeroConta, Guid? TipoContaId, string? Nota, decimal? SaldoAbertura) : IRequest<CommandResult>;
    public record AtualizarContaFinanceiraCommand(Guid Id, string Nome, string? NumeroConta, Guid? TipoContaId, string? Nota) : IRequest<CommandResult>;
    public record FecharContaFinanceiraCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Transação de Conta Financeira -----
    public record RegistrarTransacaoContaCommand(Guid ContaFinanceiraId, decimal Valor, ETipoTransacaoConta Tipo, string? Subtipo, DateTime DataOperacao, string? Nota, Guid? TransacaoOrigemId, Guid? PagamentoTransacaoId) : IRequest<CommandResult>;
    public record RegistrarTransferenciaCommand(Guid ContaOrigemId, Guid ContaDestinoId, decimal Valor, DateTime DataOperacao, string? Nota) : IRequest<CommandResult>;

    // ----- Movimento Financeiro -----
    public record RegistrarMovimentoFinanceiroCommand(Guid? EmpresaId, DateTime Emissao, Guid? CaixaId, Guid? ContaId, decimal Credito, decimal Debito, Guid? ChequeId, Guid? ContasPagarId, Guid? ContasReceberId, Guid? PagamentoId, bool Planejamento) : IRequest<CommandResult>;
    public record ConciliarMovimentoCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Cheque -----
    public record RegistrarChequeCommand(Guid? EmpresaId, int Tipo, int TipoPessoa, Guid? PessoaId, DateTime Emissao, DateTime Vencimento, decimal Valor, Guid? ContaId, Guid? CaixaId) : IRequest<CommandResult>;
    public record AtualizarSituacaoChequeCommand(Guid Id, ESituacaoCheque Situacao) : IRequest<CommandResult>;

    // ----- Caixa Operacional -----
    public record AbrirCaixaOperacionalCommand(Guid? LocalId, Guid? UsuarioId, decimal ValorInicial) : IRequest<CommandResult>;
    public record FecharCaixaOperacionalCommand(Guid Id, decimal? ValorFechamento, decimal? TotalComprovantesCartao, decimal? TotalCheques, string? Observacao) : IRequest<CommandResult>;
}
