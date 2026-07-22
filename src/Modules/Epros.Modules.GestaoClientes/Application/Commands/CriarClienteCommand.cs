using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarClienteCommand(
        string RazaoSocial,
        string Cnpj,
        string Email,
        Guid PlanoId,
        string? Telefone = null,
        string? NomeContato = null,
        bool IsDemo = false,
        string? TokenAcesso = null
    ) : ICommand;

    public class CriarClienteCommandValidator : AbstractValidator<CriarClienteCommand>
    {
        public CriarClienteCommandValidator()
        {
            RuleFor(c => c.RazaoSocial)
                .NotEmpty().WithMessage("A Razão Social é obrigatória.");

            RuleFor(c => c.Cnpj)
                .NotEmpty().WithMessage("O CNPJ é obrigatório.");

            RuleFor(c => c.Email)
                .EmailAddress().WithMessage("O E-mail fornecido não é válido.");

            RuleFor(c => c.PlanoId)
                .NotEmpty().WithMessage("O ID do Plano é obrigatório.");
        }
    }
}
