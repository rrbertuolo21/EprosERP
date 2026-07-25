using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Verifica o status do serviço da SEFAZ (NfeStatusServico). Fiel ao legado
    /// <c>NfceNfeController.VerificarStatus</c>. Não persiste — é uma sondagem online.
    /// </summary>
    public record VerificarStatusServicoCommand(
        string Documento,
        string Uf,
        int Ambiente = 2,
        int ModeloDocumento = 55
    ) : ICommand;

    public class VerificarStatusServicoCommandValidator : AbstractValidator<VerificarStatusServicoCommand>
    {
        public VerificarStatusServicoCommandValidator()
        {
            RuleFor(c => c.Documento).NotEmpty().WithMessage("O documento do emitente é obrigatório.");
            RuleFor(c => c.ModeloDocumento).Must(m => m == 55 || m == 65)
                .WithMessage("O modelo do documento deve ser 55 (NF-e) ou 65 (NFC-e).");
            RuleFor(c => c.Ambiente).Must(a => a == 1 || a == 2)
                .WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");
        }
    }

    /// <summary>
    /// Consulta o protocolo/situação de uma NF-e/NFC-e pela chave (NfeConsultaProtocolo). Fiel ao
    /// legado <c>NfceNfeController.ConsultaProtocolo</c>.
    /// </summary>
    public record ConsultarProtocoloCommand(
        string Chave,
        int Ambiente = 2,
        int ModeloDocumento = 55
    ) : ICommand;

    public class ConsultarProtocoloCommandValidator : AbstractValidator<ConsultarProtocoloCommand>
    {
        public ConsultarProtocoloCommandValidator()
        {
            RuleFor(c => c.Chave).NotEmpty().WithMessage("A chave de acesso é obrigatória.")
                .Length(44).WithMessage("A chave de acesso deve ter 44 dígitos.");
            RuleFor(c => c.Ambiente).Must(a => a == 1 || a == 2)
                .WithMessage("O ambiente deve ser 1 (Produção) ou 2 (Homologação).");
        }
    }
}
