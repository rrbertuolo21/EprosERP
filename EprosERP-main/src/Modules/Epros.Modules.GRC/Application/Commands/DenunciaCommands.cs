using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-DEN — Investigacoes e Denuncias (submodulo completo, EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1)

    // ---- Categoria (grc_den_categoria) ----
    public record CriarCategoriaDenunciaCommand(
        string Nome,
        string? Descricao,
        string? Cor,
        Guid? CriadorId
    ) : ICommand;

    public class CriarCategoriaDenunciaCommandValidator : AbstractValidator<CriarCategoriaDenunciaCommand>
    {
        public CriarCategoriaDenunciaCommandValidator()
        {
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome da categoria é obrigatório.");
        }
    }

    public record InativarCategoriaDenunciaCommand(Guid CategoriaId) : ICommand;

    // ---- Registro detalhado (grc_den_denuncia) ----
    /// <summary>7.1 Registro de denuncia. RN-DEN-003 (protocolo unico), RN-DEN-004 (categoria ativa).</summary>
    public record RegistrarDenunciaDetalhadaCommand(
        string? Titulo,
        string Relato,
        Guid? CategoriaId,
        string? Prioridade,
        bool Anonima
    ) : ICommand;

    public class RegistrarDenunciaDetalhadaCommandValidator : AbstractValidator<RegistrarDenunciaDetalhadaCommand>
    {
        public RegistrarDenunciaDetalhadaCommandValidator()
        {
            RuleFor(c => c.Relato).NotEmpty().WithMessage("O relato detalhado da denúncia é obrigatório.");
        }
    }

    // ---- Triagem ----
    /// <summary>7.3 Triagem e classificacao.</summary>
    public record TriarDenunciaCommand(
        Guid DenunciaId,
        Guid? CategoriaId,
        string? Prioridade
    ) : ICommand;

    public class TriarDenunciaCommandValidator : AbstractValidator<TriarDenunciaCommand>
    {
        public TriarDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
        }
    }

    // ---- Participantes ----
    public record AdicionarParticipanteDenunciaCommand(
        Guid DenunciaId,
        Guid? PessoaId,
        string Papel,
        string? NomeDeclarado,
        bool Sigiloso
    ) : ICommand;

    public class AdicionarParticipanteDenunciaCommandValidator : AbstractValidator<AdicionarParticipanteDenunciaCommand>
    {
        public AdicionarParticipanteDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
            RuleFor(c => c.Papel).NotEmpty().WithMessage("O papel do participante é obrigatório.");
        }
    }

    // ---- Investigacao segregada ----
    /// <summary>7.4 Investigacao segregada. RN-DEN-006: investigador != denunciado/beneficiario.</summary>
    public record AtribuirInvestigacaoCommand(
        Guid DenunciaId,
        Guid InvestigadorId,
        DateTime? PrazoSla
    ) : ICommand;

    public class AtribuirInvestigacaoCommandValidator : AbstractValidator<AtribuirInvestigacaoCommand>
    {
        public AtribuirInvestigacaoCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
            RuleFor(c => c.InvestigadorId).NotEmpty().WithMessage("O investigador é obrigatório.");
        }
    }

    public record ConcluirInvestigacaoCommand(
        Guid InvestigacaoId,
        string ConclusaoProposta,
        DateTime DataConclusao
    ) : ICommand;

    public class ConcluirInvestigacaoCommandValidator : AbstractValidator<ConcluirInvestigacaoCommand>
    {
        public ConcluirInvestigacaoCommandValidator()
        {
            RuleFor(c => c.InvestigacaoId).NotEmpty().WithMessage("A investigação é obrigatória.");
            RuleFor(c => c.ConclusaoProposta).NotEmpty().WithMessage("A conclusão proposta é obrigatória.");
        }
    }

    // ---- Respostas e anexos ----
    /// <summary>7.5 Respostas, mensagens e anexos. RN-DEN-005: interna nao visivel ao denunciante.</summary>
    public record ResponderDenunciaCommand(
        Guid DenunciaId,
        string Mensagem,
        bool Interna
    ) : ICommand;

    public class ResponderDenunciaCommandValidator : AbstractValidator<ResponderDenunciaCommand>
    {
        public ResponderDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
            RuleFor(c => c.Mensagem).NotEmpty().WithMessage("A mensagem não pode ser vazia.");
        }
    }

    /// <summary>RN-DEN-016: anexo de investigacao sigiloso por padrao.</summary>
    public record AnexarEvidenciaDenunciaCommand(
        Guid DenunciaId,
        Guid? RespostaId,
        Guid ArquivoId,
        bool Sigiloso
    ) : ICommand;

    public class AnexarEvidenciaDenunciaCommandValidator : AbstractValidator<AnexarEvidenciaDenunciaCommand>
    {
        public AnexarEvidenciaDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
            RuleFor(c => c.ArquivoId).NotEmpty().WithMessage("O arquivo é obrigatório.");
        }
    }

    // ---- Conclusao ----
    /// <summary>7.6 Conclusao e encerramento. RN-DEN-001: resolved_at deve ser data/hora valida.</summary>
    public record ConcluirDenunciaCommand(
        Guid DenunciaId,
        DateTime ResolvedAt,
        string? ParecerFinal
    ) : ICommand;

    public class ConcluirDenunciaCommandValidator : AbstractValidator<ConcluirDenunciaCommand>
    {
        public ConcluirDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("A denúncia é obrigatória.");
        }
    }

    // ---- Parametros ----
    public record DefinirParametroDenunciaCommand(
        string Chave,
        string ValorJson
    ) : ICommand;

    public class DefinirParametroDenunciaCommandValidator : AbstractValidator<DefinirParametroDenunciaCommand>
    {
        public DefinirParametroDenunciaCommandValidator()
        {
            RuleFor(c => c.Chave).NotEmpty().WithMessage("A chave do parâmetro é obrigatória.");
        }
    }
}
