using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Emite um documento fiscal (NF-e/NFC-e) em CONTINGÊNCIA (tpEmis != Normal), quando a SEFAZ da UF
    /// está indisponível. Reusa o mesmo agregado/mapper/motor da emissão normal — apenas com a
    /// contingência aplicada (ver <c>EmitirDocumentoFiscalContingenciaCommandHandler</c>).
    ///
    /// tpEmis (código SEFAZ): 6=SVC-AN, 7=SVC-RS, 4=EPEC, 9=Offline-NFCe. Não aceita 1 (Normal) — para
    /// emissão normal use <c>EmitirDocumentoFiscalCommand</c> (o caminho normal permanece intacto).
    /// </summary>
    public record EmitirDocumentoFiscalContingenciaCommand(
        string Modelo,
        int Ambiente,
        int Serie,
        long Numero,
        string DestinatarioCnpjCpf,
        string DestinatarioNome,
        decimal Total,
        List<EmitirDocumentoFiscalItemDto> Itens,
        int TipoEmissao,
        string Justificativa,
        Guid? EmpresaId = null
    ) : ICommand;

    /// <summary>
    /// Reenvia à SEFAZ os documentos emitidos em contingência offline que ficaram pendentes de
    /// transmissão (Status = "PendenteContingencia"), tipicamente após a SEFAZ voltar a operar.
    /// Opcionalmente verifica o status do serviço antes; e pode retransmitir também os REJEITADOS.
    /// </summary>
    public record ReprocessarContingenciaCommand(
        Guid? EmpresaId = null,
        bool VerificarStatusServico = true,
        bool IncluirRejeitados = false
    ) : ICommand;

    public class EmitirDocumentoFiscalContingenciaCommandValidator : AbstractValidator<EmitirDocumentoFiscalContingenciaCommand>
    {
        // tpEmis de contingência aceitos.
        private static readonly int[] ContingenciasValidas = { 2, 3, 4, 5, 6, 7, 9 };

        public EmitirDocumentoFiscalContingenciaCommandValidator()
        {
            RuleFor(c => c.Modelo)
                .Must(m => m == "55" || m == "65").WithMessage("O modelo deve ser 55 (NF-e) ou 65 (NFC-e).");

            RuleFor(c => c.Ambiente)
                .Must(a => a == 1 || a == 2).WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");

            RuleFor(c => c.Serie).GreaterThan(0).WithMessage("A série do documento fiscal deve ser maior que zero.");
            RuleFor(c => c.Numero).GreaterThan(0).WithMessage("O número do documento fiscal deve ser maior que zero.");
            RuleFor(c => c.DestinatarioCnpjCpf).NotEmpty().WithMessage("O CPF/CNPJ do destinatário é obrigatório.");
            RuleFor(c => c.DestinatarioNome).NotEmpty().WithMessage("O nome do destinatário é obrigatório.");
            RuleFor(c => c.Total).GreaterThanOrEqualTo(0).WithMessage("O valor total do documento não pode ser negativo.");
            RuleFor(c => c.Itens).NotEmpty().WithMessage("O documento fiscal deve possuir ao menos um item.");

            RuleFor(c => c.TipoEmissao)
                .Must(t => Array.IndexOf(ContingenciasValidas, t) >= 0)
                .WithMessage("O tipo de emissão de contingência é inválido (use 6=SVC-AN, 7=SVC-RS, 4=EPEC ou 9=Offline-NFCe).");

            // A justificativa da contingência (xJust) exige entre 15 e 256 caracteres (regra SEFAZ/motor).
            RuleFor(c => c.Justificativa)
                .NotEmpty().WithMessage("A justificativa da contingência é obrigatória.")
                .MinimumLength(15).WithMessage("A justificativa da contingência deve conter no mínimo 15 caracteres.")
                .MaximumLength(256).WithMessage("A justificativa da contingência deve conter no máximo 256 caracteres.");

            // Offline (9) só se aplica a NFC-e (modelo 65).
            RuleFor(c => c)
                .Must(c => c.TipoEmissao != 9 || c.Modelo == "65")
                .WithMessage("A contingência offline (tpEmis=9) aplica-se apenas à NFC-e (modelo 65).");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Sku).NotEmpty().WithMessage("O SKU do produto é obrigatório.");
                item.RuleFor(i => i.NomeProduto).NotEmpty().WithMessage("O nome do produto é obrigatório.");
                item.RuleFor(i => i.Cst).NotEmpty().WithMessage("O CST do ICMS é obrigatório.");
                item.RuleFor(i => i.Cfop).GreaterThan(0).WithMessage("O CFOP deve ser válido.");
                item.RuleFor(i => i.Ncm).NotEmpty().WithMessage("O NCM do produto é obrigatório.");
                item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade do produto deve ser maior que zero.");
                item.RuleFor(i => i.ValorUnitario).GreaterThanOrEqualTo(0).WithMessage("O valor unitário não pode ser negativo.");
                item.RuleFor(i => i.AliquotaIcms).GreaterThanOrEqualTo(0).WithMessage("A alíquota de ICMS não pode ser negativa.");
            });
        }
    }
}
