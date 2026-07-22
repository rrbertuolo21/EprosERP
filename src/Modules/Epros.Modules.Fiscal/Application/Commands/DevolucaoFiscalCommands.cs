using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Cria a devolução fiscal em estado NOVO (EF_DEVOLUCAO_FISCAL, 8.1/8.2/8.3). Exige a chave da NF de
    /// entrada (referência fiscal, REG-DEV-001/007) e ao menos um item com NCM/CFOP/CST (REG-DEV-011).
    /// </summary>
    public record CriarDevolucaoFiscalCommand(
        string Modelo,
        int Ambiente,
        int Serie,
        string ChaveNfEntrada,
        string Motivo,
        string DestinatarioCnpjCpf,
        string DestinatarioNome,
        decimal Total,
        List<DevolucaoFiscalItemDto> Itens,
        Guid? EmpresaId = null,
        Guid? DocumentoOrigemId = null,
        string? XmlEntrada = null
    ) : ICommand;

    public record DevolucaoFiscalItemDto(
        string Sku,
        string NomeProduto,
        string Ncm,
        int Cfop,
        string Cst,
        decimal Quantidade,
        decimal ValorUnitario,
        decimal AliquotaIcms,
        Guid? ProdutoId = null
    );

    /// <summary>Transmite a devolução à SEFAZ (8.4). Só NOVO/REJEITADO com itens e referência (REG-DEV-012/013/018).</summary>
    public record TransmitirDevolucaoFiscalCommand(Guid Id) : ICommand;

    /// <summary>Cancela a devolução (8.5). Preserva as chaves rastreáveis (REG-DEV-019).</summary>
    public record CancelarDevolucaoFiscalCommand(Guid Id, string Justificativa) : ICommand;

    /// <summary>Registra correção da devolução (8.6). Mantém a rastreabilidade do documento original (REG-DEV-016).</summary>
    public record CorrigirDevolucaoFiscalCommand(Guid Id, string TextoCorrecao) : ICommand;

    public class CriarDevolucaoFiscalCommandValidator : AbstractValidator<CriarDevolucaoFiscalCommand>
    {
        public CriarDevolucaoFiscalCommandValidator()
        {
            RuleFor(c => c.Modelo).Must(m => m == "55" || m == "65")
                .WithMessage("O modelo deve ser 55 (NF-e) ou 65 (NFC-e).");
            RuleFor(c => c.Ambiente).Must(a => a == 1 || a == 2)
                .WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");
            RuleFor(c => c.Serie).GreaterThan(0).WithMessage("A série deve ser maior que zero.");

            // REG-DEV-001/007: referência fiscal obrigatória (chave de 44 dígitos).
            RuleFor(c => c.ChaveNfEntrada)
                .NotEmpty().WithMessage("A chave da NF de entrada (referência fiscal) é obrigatória.")
                .Length(44).WithMessage("A chave da NF de entrada deve ter 44 dígitos.");

            RuleFor(c => c.DestinatarioCnpjCpf).NotEmpty().WithMessage("O CPF/CNPJ do destinatário da devolução é obrigatório.");
            RuleFor(c => c.DestinatarioNome).NotEmpty().WithMessage("O nome do destinatário da devolução é obrigatório.");
            RuleFor(c => c.Total).GreaterThanOrEqualTo(0).WithMessage("O total da devolução não pode ser negativo.");

            // REG-DEV-018: devolução sem itens não deve ser transmitida — exigimos itens já na criação.
            RuleFor(c => c.Itens).NotEmpty().WithMessage("A devolução deve possuir ao menos um item.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Sku).NotEmpty().WithMessage("O SKU/código do produto devolvido é obrigatório.");
                item.RuleFor(i => i.NomeProduto).NotEmpty().WithMessage("O nome do produto devolvido é obrigatório.");
                item.RuleFor(i => i.Ncm).NotEmpty().WithMessage("O NCM da linha é obrigatório.");
                item.RuleFor(i => i.Cfop).GreaterThan(0).WithMessage("O CFOP da linha deve ser informado.");
                item.RuleFor(i => i.Cst).NotEmpty().WithMessage("O CST/CSOSN da linha é obrigatório.");
                item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade devolvida deve ser maior que zero.");
                item.RuleFor(i => i.ValorUnitario).GreaterThanOrEqualTo(0).WithMessage("O valor unitário não pode ser negativo.");
                item.RuleFor(i => i.AliquotaIcms).GreaterThanOrEqualTo(0).WithMessage("A alíquota de ICMS não pode ser negativa.");
            });
        }
    }

    public class CancelarDevolucaoFiscalCommandValidator : AbstractValidator<CancelarDevolucaoFiscalCommand>
    {
        public CancelarDevolucaoFiscalCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O identificador da devolução é obrigatório.");
            RuleFor(c => c.Justificativa)
                .NotEmpty().WithMessage("A justificativa do cancelamento é obrigatória.")
                .MinimumLength(15).WithMessage("A justificativa do cancelamento deve conter no mínimo 15 caracteres.")
                .MaximumLength(255).WithMessage("A justificativa do cancelamento deve conter no máximo 255 caracteres.");
        }
    }

    public class CorrigirDevolucaoFiscalCommandValidator : AbstractValidator<CorrigirDevolucaoFiscalCommand>
    {
        public CorrigirDevolucaoFiscalCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O identificador da devolução é obrigatório.");
            RuleFor(c => c.TextoCorrecao)
                .NotEmpty().WithMessage("O texto da correção é obrigatório.")
                .MinimumLength(15).WithMessage("O texto da correção deve conter no mínimo 15 caracteres.")
                .MaximumLength(1000).WithMessage("O texto da correção deve conter no máximo 1000 caracteres.");
        }
    }
}
