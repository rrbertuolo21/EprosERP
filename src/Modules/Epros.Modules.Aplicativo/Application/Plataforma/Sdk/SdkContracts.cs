using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Sdk
{
    // ===================== Commands =====================

    /// <summary>Gera uma chave de API. O segredo em claro é devolvido UMA ÚNICA VEZ no resultado.</summary>
    public record GerarChaveApiCommand(string Nome, List<string> Escopos, DateTime? ExpiraEm) : ICommand;

    public class GerarChaveApiCommandValidator : AbstractValidator<GerarChaveApiCommand>
    {
        public GerarChaveApiCommandValidator()
        {
            RuleFor(c => c.Nome).NotEmpty();
            RuleFor(c => c.Escopos).NotEmpty().WithMessage("Conceda ao menos um escopo.");
        }
    }

    public record RevogarChaveApiCommand(Guid ChaveId) : ICommand;

    /// <summary>Registra o uso de uma chave (atualiza último uso) — chamado pelo gateway após validar.</summary>
    public record RegistrarUsoChaveApiCommand(Guid ChaveId) : ICommand;

    // ===================== Queries =====================

    public record ObterChavesApiQuery(bool ApenasAtivas = false) : IQuery<IReadOnlyList<ChaveApiDto>>;

    public record ObterEscoposDisponiveisQuery() : IQuery<IReadOnlyList<string>>;

    /// <summary>Valida um segredo apresentado contra o hash, expiração, revogação e escopo requerido.</summary>
    public record ValidarChaveApiQuery(string Chave, string EscopoRequerido) : IQuery<ResultadoValidacaoChaveDto>;

    // ===================== DTOs =====================

    public record ChaveApiDto(Guid Id, string Nome, string Prefixo, IReadOnlyList<string> Escopos,
        bool Ativa, DateTime? ExpiraEm, DateTime? UltimoUsoEm, DateTime? RevogadaEm, DateTime CriadoEm);

    public record ResultadoValidacaoChaveDto(bool Valida, Guid? ChaveId, string? Motivo);
}
