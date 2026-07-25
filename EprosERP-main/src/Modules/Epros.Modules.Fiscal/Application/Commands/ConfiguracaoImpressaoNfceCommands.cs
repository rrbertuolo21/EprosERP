using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarConfiguracaoImpressaoNfceCommand(
        Guid EmpresaId,
        ENfceDetalheVendaNormal? DetalheVendaNormal,
        ENfceDetalheVendaContingencia? DetalheVendaContingencia,
        bool? ImprimeDescontoItem,
        bool? ImprimeFoneEmitente,
        float? MargemEsquerda,
        float? MargemDireita,
        ENfceModoImpressao? ModoImpressao,
        ENfceLayoutQrCode? NfceLayoutQrCode,
        EVersaoQrCode? VersaoQrCode,
        bool? SegundaViaContingencia
    ) : ICommand;

    public record AtualizarConfiguracaoImpressaoNfceCommand(
        Guid Id,
        ENfceDetalheVendaNormal? DetalheVendaNormal,
        ENfceDetalheVendaContingencia? DetalheVendaContingencia,
        bool? ImprimeDescontoItem,
        bool? ImprimeFoneEmitente,
        float? MargemEsquerda,
        float? MargemDireita,
        ENfceModoImpressao? ModoImpressao,
        ENfceLayoutQrCode? NfceLayoutQrCode,
        EVersaoQrCode? VersaoQrCode,
        bool? SegundaViaContingencia
    ) : ICommand;

    public record DeletarConfiguracaoImpressaoNfceCommand(Guid Id) : ICommand;

    public class CriarConfiguracaoImpressaoNfceCommandValidator : AbstractValidator<CriarConfiguracaoImpressaoNfceCommand>
    {
        public CriarConfiguracaoImpressaoNfceCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty().WithMessage("A empresa é obrigatória.");
        }
    }

    public class AtualizarConfiguracaoImpressaoNfceCommandValidator : AbstractValidator<AtualizarConfiguracaoImpressaoNfceCommand>
    {
        public AtualizarConfiguracaoImpressaoNfceCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
        }
    }
}
