using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;
using System;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record AtualizarContadorCommand(
        Guid Id,
        string? RazaoSocial,
        string? NomeContador,
        string? Cpf,
        string? Cnpj,
        string NumeroCrc,
        EEstado UfCrc,
        DateTime DataVencimentoCrc,
        string? Qualificacao,
        string? Funcao,
        string? Telefone,
        string? Email,
        EPermissaoTransmissao PermissaoTransmissao,
        bool Ativo,
        string Logradouro,
        string Numero,
        string? Complemento,
        string Bairro,
        string Cep,
        int MunicipioId,
        EEstado Uf
    ) : ICommand;

    public class AtualizarContadorCommandValidator : AbstractValidator<AtualizarContadorCommand>
    {
        public AtualizarContadorCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID do contador é obrigatório.");

            RuleFor(c => c.NumeroCrc)
                .NotEmpty().WithMessage("O número do CRC é obrigatório.")
                .MaximumLength(15).WithMessage("O CRC deve ter no máximo 15 caracteres.");

            RuleFor(c => c.UfCrc)
                .IsInEnum().WithMessage("UF do CRC inválida.");

            RuleFor(c => c.PermissaoTransmissao)
                .IsInEnum().WithMessage("Permissão de transmissão inválida.");

            RuleFor(c => c.Logradouro)
                .NotEmpty().WithMessage("O logradouro é obrigatório.")
                .MaximumLength(100).WithMessage("O logradouro deve ter no máximo 100 caracteres.");

            RuleFor(c => c.Numero)
                .NotEmpty().WithMessage("O número é obrigatório.")
                .MaximumLength(10).WithMessage("O número deve ter no máximo 10 caracteres.");

            RuleFor(c => c.Bairro)
                .NotEmpty().WithMessage("O bairro é obrigatório.")
                .MaximumLength(60).WithMessage("O bairro deve ter no máximo 60 caracteres.");

            RuleFor(c => c.Cep)
                .NotEmpty().WithMessage("O CEP é obrigatório.")
                .MaximumLength(8).WithMessage("O CEP deve ter no máximo 8 caracteres.");

            RuleFor(c => c.MunicipioId)
                .GreaterThan(0).WithMessage("O município é obrigatório.");

            RuleFor(c => c.Uf)
                .IsInEnum().WithMessage("UF do endereço inválida.");
        }
    }
}
