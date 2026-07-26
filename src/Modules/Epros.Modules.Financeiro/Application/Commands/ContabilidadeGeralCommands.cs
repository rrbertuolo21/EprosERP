using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Conta Contábil -----
    public record CriarContaContabilCommand(
        string CodigoConta,
        string NomeConta,
        Guid? ContaPaiId,
        int Nivel,
        ETipoContaContabil TipoConta,
        bool AceitaLancamento,
        bool ParticipaContabilidadeGeral,
        bool ParticipaOrcamento,
        bool ParticipaDepreciacao
    ) : IRequest<CommandResult>;

    public record AtualizarContaContabilCommand(
        Guid Id,
        string CodigoConta,
        string NomeConta,
        Guid? ContaPaiId,
        int Nivel,
        ETipoContaContabil TipoConta,
        bool AceitaLancamento,
        bool ParticipaContabilidadeGeral,
        bool ParticipaOrcamento,
        bool ParticipaDepreciacao
    ) : IRequest<CommandResult>;

    public record DeletarContaContabilCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Período Contábil -----
    public record CriarPeriodoContabilCommand(int AnoFiscal, DateTime? DataInicio, DateTime? DataFim) : IRequest<CommandResult>;
    public record IniciarFechamentoPeriodoCommand(Guid Id) : IRequest<CommandResult>;
    public record FecharPeriodoContabilCommand(Guid Id, Guid? UsuarioFechamentoId, DateTime DataFechamento) : IRequest<CommandResult>;
    public record ReabrirPeriodoContabilCommand(Guid Id, Guid? UsuarioReaberturaId, string Motivo) : IRequest<CommandResult>;

    // ----- Lançamento Contábil -----
    public record LancamentoLinhaInput(Guid ContaContabilId, decimal Debito, decimal Credito, string? Historico);

    public record CriarLancamentoContabilCommand(
        Guid? PeriodoContabilId,
        string? NumeroLancamento,
        DateTime Data,
        string? Historico,
        IReadOnlyList<LancamentoLinhaInput> Linhas
    ) : IRequest<CommandResult>;

    public record ConfirmarLancamentoContabilCommand(Guid Id) : IRequest<CommandResult>;
    public record EstornarLancamentoContabilCommand(Guid Id) : IRequest<CommandResult>;
    public record CancelarLancamentoContabilCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Saldo de Abertura -----
    public record RegistrarSaldoAberturaCommand(
        string? Numero,
        DateTime Data,
        Guid ContaContabilId,
        string? CodigoConta,
        ETipoSaldoContabil TipoSaldo,
        decimal Valor,
        string Historico
    ) : IRequest<CommandResult>;
}
