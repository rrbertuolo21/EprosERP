using System;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Programa de Subsídio/Fundo -----
    public record CriarProgramaSubsidioCommand(string Orgao, decimal ValorTotal, DateTime VigenciaInicio, DateTime? VigenciaFim) : IRequest<CommandResult>;
    public record AtualizarProgramaSubsidioCommand(Guid Id, string Orgao, decimal ValorTotal, DateTime VigenciaInicio, DateTime? VigenciaFim) : IRequest<CommandResult>;
    public record IniciarPrestacaoContasProgramaCommand(Guid Id) : IRequest<CommandResult>;
    public record EncerrarProgramaSubsidioCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Utilização de Subsídio -----
    public record VincularDespesaElegivelCommand(Guid ProgramaSubsidioId, Guid TituloPagarId, decimal ValorElegivel) : IRequest<CommandResult>;
    public record RemoverUtilizacaoSubsidioCommand(Guid Id) : IRequest<CommandResult>;
}
