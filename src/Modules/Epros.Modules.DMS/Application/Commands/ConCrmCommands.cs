using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarProspectShowroomCommand(
        Guid ContactId,
        Guid UnidadeId,
        string Origem,
        Guid VendedorId
    ) : ICommand;

    public class CriarProspectShowroomCommandValidator : AbstractValidator<CriarProspectShowroomCommand>
    {
        public CriarProspectShowroomCommandValidator()
        {
            RuleFor(c => c.ContactId).NotEmpty();
            RuleFor(c => c.UnidadeId).NotEmpty();
            RuleFor(c => c.Origem).NotEmpty();
            RuleFor(c => c.VendedorId).NotEmpty();
        }
    }

    public record CriarOportunidadeConcessionariaCommand(
        Guid ProspectId,
        decimal? ValorEstimado,
        decimal? Probabilidade
    ) : ICommand;

    public class CriarOportunidadeConcessionariaCommandValidator : AbstractValidator<CriarOportunidadeConcessionariaCommand>
    {
        public CriarOportunidadeConcessionariaCommandValidator()
        {
            RuleFor(c => c.ProspectId).NotEmpty();
        }
    }

    public record CriarTestDriveCommand(
        Guid OportunidadeId,
        Guid VeiculoDemonstracaoId,
        DateTime Inicio,
        DateTime Fim
    ) : ICommand;

    public class CriarTestDriveCommandValidator : AbstractValidator<CriarTestDriveCommand>
    {
        public CriarTestDriveCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.VeiculoDemonstracaoId).NotEmpty();
            RuleFor(c => c.Fim).GreaterThan(c => c.Inicio).WithMessage("A data/hora de término deve ser posterior ao início.");
        }
    }
}
