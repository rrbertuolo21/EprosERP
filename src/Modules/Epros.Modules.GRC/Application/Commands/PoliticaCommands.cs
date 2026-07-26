using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-POL — Gestao de Politicas

    public record CriarPoliticaCommand(
        string Codigo,
        string Titulo,
        string Descricao,
        string? Categoria,
        Guid OwnerId,
        string? ModuloAplicavel
    ) : ICommand;

    public class CriarPoliticaCommandValidator : AbstractValidator<CriarPoliticaCommand>
    {
        public CriarPoliticaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O codigo da politica e obrigatorio.");
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo da politica e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao da politica e obrigatoria.");
            RuleFor(c => c.OwnerId).NotEmpty().WithMessage("O owner da politica e obrigatorio.");
        }
    }

    public record CriarVersaoPoliticaCommand(
        Guid PoliticaId,
        string NumeroVersao,
        DateTime DataInicioVigencia,
        DateTime? DataFimVigencia,
        string? ResumoAlteracoes,
        string? MotivoVersao
    ) : ICommand;

    public class CriarVersaoPoliticaCommandValidator : AbstractValidator<CriarVersaoPoliticaCommand>
    {
        public CriarVersaoPoliticaCommandValidator()
        {
            RuleFor(c => c.PoliticaId).NotEmpty().WithMessage("A politica e obrigatoria.");
            RuleFor(c => c.NumeroVersao).NotEmpty().WithMessage("O numero da versao e obrigatorio.");
        }
    }

    /// <summary>RN-POL-004: politica publicada deve possuir versao vigente publicada.</summary>
    public record PublicarPoliticaCommand(Guid PoliticaId, Guid VersaoId) : ICommand;

    public class PublicarPoliticaCommandValidator : AbstractValidator<PublicarPoliticaCommand>
    {
        public PublicarPoliticaCommandValidator()
        {
            RuleFor(c => c.PoliticaId).NotEmpty().WithMessage("A politica e obrigatoria.");
            RuleFor(c => c.VersaoId).NotEmpty().WithMessage("A versao a publicar e obrigatoria.");
        }
    }

    /// <summary>RN-POL-006/007: aceite so em versao publicada, imutavel; RN-POL-005 unicidade por versao/usuario.</summary>
    public record RegistrarAceitePoliticaCommand(
        Guid PoliticaId,
        Guid VersaoId,
        Guid UsuarioId,
        string Ip,
        string? Origem,
        string? DeclaracaoAceite
    ) : ICommand;

    public class RegistrarAceitePoliticaCommandValidator : AbstractValidator<RegistrarAceitePoliticaCommand>
    {
        public RegistrarAceitePoliticaCommandValidator()
        {
            RuleFor(c => c.PoliticaId).NotEmpty().WithMessage("A politica e obrigatoria.");
            RuleFor(c => c.VersaoId).NotEmpty().WithMessage("A versao e obrigatoria.");
            RuleFor(c => c.UsuarioId).NotEmpty().WithMessage("O usuario e obrigatorio.");
            RuleFor(c => c.Ip).NotEmpty().WithMessage("O IP do aceite e obrigatorio.");
        }
    }
}
