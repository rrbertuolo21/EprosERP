using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarGrupoPlanoCommand(
        string Descricao,
        bool Ativo = true
    ) : ICommand;

    public class CriarGrupoPlanoCommandValidator : AbstractValidator<CriarGrupoPlanoCommand>
    {
        public CriarGrupoPlanoCommandValidator()
        {
            RuleFor(g => g.Descricao)
                .NotEmpty().WithMessage("A Descrição do grupo de plano é obrigatória.");
        }
    }

    public record AtualizarGrupoPlanoCommand(
        Guid Id,
        string Descricao,
        bool Ativo
    ) : ICommand;

    public class AtualizarGrupoPlanoCommandValidator : AbstractValidator<AtualizarGrupoPlanoCommand>
    {
        public AtualizarGrupoPlanoCommandValidator()
        {
            RuleFor(g => g.Id)
                .NotEmpty().WithMessage("O ID do grupo de plano é obrigatório.");

            RuleFor(g => g.Descricao)
                .NotEmpty().WithMessage("A Descrição do grupo de plano é obrigatória.");
        }
    }

    public record ExcluirGrupoPlanoCommand(Guid Id) : ICommand;
}
