using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC — Parametrizacao por tenant (D-TEC-04). Uma unica porta de escrita que roteia
    // para a tabela do submodulo (grc_{pol,reg,ris,cia,sod}_parametro).

    /// <summary>Submodulos que aceitam parametrizacao.</summary>
    public static class SubmoduloGrc
    {
        public const string Politicas = "POL";
        public const string Compliance = "REG";
        public const string Riscos = "RIS";
        public const string ControlesAuditoria = "CIA";
        public const string SegregacaoFuncoes = "SOD";

        // DEN ja possui a sua propria porta (DefinirParametroDenunciaCommand / grc_den_parametro),
        // por isso nao entra nesta porta generica dos 5 submodulos que faltavam (D-TEC-04).
        public static bool EhValido(string s) =>
            s == Politicas || s == Compliance || s == Riscos ||
            s == ControlesAuditoria || s == SegregacaoFuncoes;
    }

    /// <summary>Define (cria ou atualiza) um parametro de um submodulo GRC para o tenant atual.</summary>
    public record DefinirParametroGrcCommand(
        string Submodulo, // POL, REG, RIS, CIA, SOD
        string Chave,
        string ValorJson,
        bool Ativo = true
    ) : ICommand;

    public class DefinirParametroGrcCommandValidator : AbstractValidator<DefinirParametroGrcCommand>
    {
        public DefinirParametroGrcCommandValidator()
        {
            RuleFor(c => c.Submodulo).Must(SubmoduloGrc.EhValido)
                .WithMessage("O submodulo deve ser 'POL', 'REG', 'RIS', 'CIA' ou 'SOD'.");
            RuleFor(c => c.Chave).NotEmpty().WithMessage("A chave do parametro e obrigatoria.");
        }
    }
}
