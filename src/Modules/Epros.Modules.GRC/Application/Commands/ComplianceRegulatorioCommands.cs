using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-REG — Compliance Regulatorio

    public record RegistrarRegistroRegulatorioCommand(
        string Codigo,
        string Descricao,
        string? Norma,
        Guid ResponsavelId
    ) : ICommand;

    public class RegistrarRegistroRegulatorioCommandValidator : AbstractValidator<RegistrarRegistroRegulatorioCommand>
    {
        public RegistrarRegistroRegulatorioCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O codigo do registro regulatorio e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao do registro regulatorio e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
        }
    }

    public record CadastrarCertificadoDigitalCommand(
        Guid EmpresaId,
        string Cnpj,
        string Serial,
        string Tipo,   // A1, A3
        string Origem, // Empresa, EscritorioContador
        DateTime DataValidade
    ) : ICommand;

    public class CadastrarCertificadoDigitalCommandValidator : AbstractValidator<CadastrarCertificadoDigitalCommand>
    {
        public CadastrarCertificadoDigitalCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty().WithMessage("A empresa e obrigatoria.");
            RuleFor(c => c.Cnpj).NotEmpty().WithMessage("O CNPJ e obrigatorio.");
            RuleFor(c => c.Serial).NotEmpty().WithMessage("O serial e obrigatorio.");
            RuleFor(c => c.Tipo).Must(t => t == "A1" || t == "A3").WithMessage("O tipo deve ser 'A1' ou 'A3'.");
            RuleFor(c => c.Origem).Must(o => o == "Empresa" || o == "EscritorioContador")
                .WithMessage("A origem deve ser 'Empresa' ou 'EscritorioContador'.");
        }
    }

    public record RegistrarValidacaoCertificadoCommand(
        Guid CertificadoId,
        string Resultado, // Sucesso, Falha
        string? Mensagem
    ) : ICommand;

    public class RegistrarValidacaoCertificadoCommandValidator : AbstractValidator<RegistrarValidacaoCertificadoCommand>
    {
        public RegistrarValidacaoCertificadoCommandValidator()
        {
            RuleFor(c => c.CertificadoId).NotEmpty().WithMessage("O certificado e obrigatorio.");
            RuleFor(c => c.Resultado).Must(r => r == "Sucesso" || r == "Falha")
                .WithMessage("O resultado deve ser 'Sucesso' ou 'Falha'.");
        }
    }
}
