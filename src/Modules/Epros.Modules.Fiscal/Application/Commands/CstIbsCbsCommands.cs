using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record ClassificacaoTributariaAnexoInput(
        int NroAnexo,
        string Codigo,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia
    );

    public record ClassificacaoTributariaInput(
        string Codigo,
        string Descricao,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia,
        bool IndNfe,
        bool IndNfce,
        bool IndCte,
        bool IndCteos,
        bool IndNfse,
        bool IndTribRegular,
        List<ClassificacaoTributariaAnexoInput>? Anexos
    );

    public record CriarCstIbsCbsCommand(
        string Cst,
        string Descricao,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia,
        List<ClassificacaoTributariaInput>? ClassesTributarias
    ) : ICommand;

    public record AtualizarCstIbsCbsCommand(
        Guid Id,
        string Cst,
        string Descricao,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia
    ) : ICommand;

    public record DeletarCstIbsCbsCommand(Guid Id) : ICommand;

    // Operações de negócio sobre os filhos (ClassificacaoTributaria)
    public record AdicionarClassificacaoTributariaCommand(Guid CstIbsCbsId, ClassificacaoTributariaInput Classe) : ICommand;
    public record RemoverClassificacaoTributariaCommand(Guid CstIbsCbsId, Guid ClassificacaoTributariaId) : ICommand;

    public class CriarCstIbsCbsCommandValidator : AbstractValidator<CriarCstIbsCbsCommand>
    {
        public CriarCstIbsCbsCommandValidator()
        {
            RuleFor(c => c.Cst).NotEmpty().WithMessage("O CST é obrigatório.")
                .MaximumLength(5).WithMessage("O CST deve possuir no máximo 5 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }

    public class AtualizarCstIbsCbsCommandValidator : AbstractValidator<AtualizarCstIbsCbsCommand>
    {
        public AtualizarCstIbsCbsCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.Cst).NotEmpty().WithMessage("O CST é obrigatório.")
                .MaximumLength(5).WithMessage("O CST deve possuir no máximo 5 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }
}
