using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    // =============================== CT-e ===============================

    /// <summary>Emite um CT-e (Conhecimento de Transporte Eletrônico). Fiel ao legado <c>cte/emitir</c>.</summary>
    public record EmitirCteCommand(
        int Serie,
        long Numero,
        string RemetenteDocumento,
        string DestinatarioDocumento,
        decimal ValorTotal,
        decimal ValorReceber,
        int Ambiente = 2,
        int TipoCte = 0,
        int Modal = 1
    ) : ICommand;

    public class EmitirCteCommandValidator : AbstractValidator<EmitirCteCommand>
    {
        public EmitirCteCommandValidator()
        {
            RuleFor(c => c.RemetenteDocumento).NotEmpty().WithMessage("O documento do remetente é obrigatório.");
            RuleFor(c => c.DestinatarioDocumento).NotEmpty().WithMessage("O documento do destinatário é obrigatório.");
            RuleFor(c => c.ValorTotal).GreaterThanOrEqualTo(0).WithMessage("O valor total não pode ser negativo.");
            RuleFor(c => c.Ambiente).Must(a => a == 1 || a == 2).WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");
        }
    }

    /// <summary>Cancela um CT-e autorizado pela chave. Fiel ao legado <c>cte/cancelar/{chave}</c>.</summary>
    public record CancelarCteCommand(string Chave, string Justificativa) : ICommand;

    public class CancelarCteCommandValidator : AbstractValidator<CancelarCteCommand>
    {
        public CancelarCteCommandValidator()
        {
            RuleFor(c => c.Chave).NotEmpty().Length(44).WithMessage("A chave de acesso deve ter 44 dígitos.");
            RuleFor(c => c.Justificativa).NotEmpty().MinimumLength(15)
                .WithMessage("A justificativa de cancelamento deve conter no mínimo 15 caracteres.");
        }
    }

    // =============================== MDF-e ===============================

    /// <summary>Emite um MDF-e (Manifesto Eletrônico de Documentos Fiscais). Fiel ao legado <c>mdfe/emitir</c>.</summary>
    public record EmitirMdfeCommand(
        int Serie,
        long Numero,
        string UfInicio,
        string UfFim,
        int QuantidadeCarregados,
        decimal ValorCarga,
        int Ambiente = 2,
        int Modal = 1,
        int TipoEmitente = 1
    ) : ICommand;

    public class EmitirMdfeCommandValidator : AbstractValidator<EmitirMdfeCommand>
    {
        public EmitirMdfeCommandValidator()
        {
            RuleFor(c => c.UfInicio).NotEmpty().Length(2).WithMessage("A UF de início deve ter 2 caracteres.");
            RuleFor(c => c.UfFim).NotEmpty().Length(2).WithMessage("A UF de fim deve ter 2 caracteres.");
            RuleFor(c => c.ValorCarga).GreaterThanOrEqualTo(0).WithMessage("O valor da carga não pode ser negativo.");
            RuleFor(c => c.Ambiente).Must(a => a == 1 || a == 2).WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");
        }
    }

    /// <summary>Encerra um MDF-e autorizado. Fiel ao legado <c>mdfe/encerrar/{chave}/{codigoMunicipio}</c>.</summary>
    public record EncerrarMdfeCommand(string Chave, string CodigoMunicipio) : ICommand;

    public class EncerrarMdfeCommandValidator : AbstractValidator<EncerrarMdfeCommand>
    {
        public EncerrarMdfeCommandValidator()
        {
            RuleFor(c => c.Chave).NotEmpty().Length(44).WithMessage("A chave de acesso deve ter 44 dígitos.");
            RuleFor(c => c.CodigoMunicipio).NotEmpty().WithMessage("O código do município de encerramento é obrigatório.");
        }
    }
}
