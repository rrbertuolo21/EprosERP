using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Plano de Contrato -----
    public record CriarPlanoContratoCommand(string Descricao, decimal Valor, EPeriodicidadeContrato Periodicidade) : IRequest<CommandResult>;
    public record AtualizarPlanoContratoCommand(Guid Id, string Descricao, decimal Valor, EPeriodicidadeContrato Periodicidade) : IRequest<CommandResult>;

    // ----- Contrato Financeiro -----
    public record CriarContratoFinanceiroCommand(Guid PlanoId, Guid PessoaId, DateTime DataInicio) : IRequest<CommandResult>;
    public record SuspenderContratoFinanceiroCommand(Guid Id) : IRequest<CommandResult>;
    public record ReativarContratoFinanceiroCommand(Guid Id) : IRequest<CommandResult>;
    public record CancelarContratoFinanceiroCommand(Guid Id, string Motivo, DateTime? DataFim) : IRequest<CommandResult>;
    public record ReajustarContratoFinanceiroCommand(Guid Id, DateTime? DataReajuste, decimal? ValorNovo, string? Motivo, Guid? AprovacaoId) : IRequest<CommandResult>;

    // ----- Fatura Recorrente -----
    public record GerarFaturaRecorrenteCommand(Guid ContratoId, string Competencia, decimal? Valor) : IRequest<CommandResult>;
    public record CancelarFaturaRecorrenteCommand(Guid Id) : IRequest<CommandResult>;
}
