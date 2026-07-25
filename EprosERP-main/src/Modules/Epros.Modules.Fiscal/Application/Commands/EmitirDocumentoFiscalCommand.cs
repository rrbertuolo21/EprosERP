using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record EmitirDocumentoFiscalCommand(
        string Modelo,
        int Ambiente,
        int Serie,
        long Numero,
        string DestinatarioCnpjCpf,
        string DestinatarioNome,
        decimal Total,
        List<EmitirDocumentoFiscalItemDto> Itens,
        // Empresa emitente: resolve certificado A1 + parâmetros DF-e para o cálculo e a transmissão.
        // Opcional para preservar chamadores existentes; sem ela, a transmissão degrada de forma controlada.
        Guid? EmpresaId = null
    ) : ICommand;

    public record EmitirDocumentoFiscalItemDto(
        string Sku,
        string NomeProduto,
        string Cst,
        int Cfop,
        string Ncm,
        decimal Quantidade,
        decimal ValorUnitario,
        decimal AliquotaIcms
    );

    public class EmitirDocumentoFiscalCommandValidator : AbstractValidator<EmitirDocumentoFiscalCommand>
    {
        public EmitirDocumentoFiscalCommandValidator()
        {
            RuleFor(c => c.Modelo)
                .Must(m => m == "55" || m == "65").WithMessage("O modelo deve ser 55 (NF-e) ou 65 (NFC-e).");

            RuleFor(c => c.Ambiente)
                .Must(a => a == 1 || a == 2).WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");

            RuleFor(c => c.Serie)
                .GreaterThan(0).WithMessage("A série do documento fiscal deve ser maior que zero.");

            RuleFor(c => c.Numero)
                .GreaterThan(0).WithMessage("O número do documento fiscal deve ser maior que zero.");

            RuleFor(c => c.DestinatarioCnpjCpf)
                .NotEmpty().WithMessage("O CPF/CNPJ do destinatário é obrigatório.");

            RuleFor(c => c.DestinatarioNome)
                .NotEmpty().WithMessage("O nome do destinatário é obrigatório.");

            RuleFor(c => c.Total)
                .GreaterThanOrEqualTo(0).WithMessage("O valor total do documento não pode ser negativo.");

            RuleFor(c => c.Itens)
                .NotEmpty().WithMessage("O documento fiscal deve possuir ao menos um item.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Sku).NotEmpty().WithMessage("O SKU do produto é obrigatório.");
                item.RuleFor(i => i.NomeProduto).NotEmpty().WithMessage("O nome do produto é obrigatório.");
                item.RuleFor(i => i.Cst).NotEmpty().WithMessage("O CST do ICMS é obrigatório.");
                item.RuleFor(i => i.Cfop).GreaterThan(0).WithMessage("O CFOP deve ser válido.");
                item.RuleFor(i => i.Ncm).NotEmpty().WithMessage("O NCM do produto é obrigatório.");
                item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade do produto deve ser maior que zero.");
                item.RuleFor(i => i.ValorUnitario).GreaterThanOrEqualTo(0).WithMessage("O valor unitário do produto não pode ser de valor negativo.");
                item.RuleFor(i => i.AliquotaIcms).GreaterThanOrEqualTo(0).WithMessage("A alíquota de ICMS não pode ser de valor negativo.");
            });
        }
    }
}
