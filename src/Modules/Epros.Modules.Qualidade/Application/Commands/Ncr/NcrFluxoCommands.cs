using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands.Ncr
{
    /// <summary>Registra a investigacao de causa raiz (5 Porques/Ishikawa) — pre-requisito da CAPA (RN-NCR-008).</summary>
    public record AdicionarCausaRaizNcrCommand(
        Guid NcrId,
        ENcrMetodoCausa Metodo,
        string DescricaoAnalise,
        string CausaIdentificada,
        string Conclusao
    ) : ICommand;

    public class AdicionarCausaRaizNcrCommandValidator : AbstractValidator<AdicionarCausaRaizNcrCommand>
    {
        public AdicionarCausaRaizNcrCommandValidator()
        {
            RuleFor(c => c.NcrId).NotEmpty();
            RuleFor(c => c.DescricaoAnalise).NotEmpty();
            RuleFor(c => c.CausaIdentificada).NotEmpty();
            RuleFor(c => c.Conclusao).NotEmpty();
        }
    }

    /// <summary>Adiciona uma acao CAPA (corretiva/preventiva/contencao) com prazo e responsavel (RN-NCR-009).</summary>
    public record AdicionarAcaoCapaNcrCommand(
        Guid NcrId,
        ENcrTipoAcao TipoAcao,
        string Descricao,
        Guid ResponsavelId,
        DateTime Prazo,
        bool EvidenciaObrigatoria
    ) : ICommand;

    public class AdicionarAcaoCapaNcrCommandValidator : AbstractValidator<AdicionarAcaoCapaNcrCommand>
    {
        public AdicionarAcaoCapaNcrCommandValidator()
        {
            RuleFor(c => c.NcrId).NotEmpty();
            RuleFor(c => c.Descricao).NotEmpty();
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    /// <summary>Conclui uma acao CAPA (evidencia obrigatoria exige resultado).</summary>
    public record ConcluirAcaoCapaNcrCommand(Guid AcaoId, string Resultado) : ICommand;

    public class ConcluirAcaoCapaNcrCommandValidator : AbstractValidator<ConcluirAcaoCapaNcrCommand>
    {
        public ConcluirAcaoCapaNcrCommandValidator() => RuleFor(c => c.AcaoId).NotEmpty();
    }

    /// <summary>Registra a verificacao de eficacia da CAPA. Reprovada reabre a NCR para a etapa CAPA (RN-NCR-012).</summary>
    public record RegistrarVerificacaoEficaciaNcrCommand(
        Guid NcrId,
        Guid? AcaoCapaId,
        string Criterio,
        ENcrResultadoVerificacao Resultado,
        string DescricaoResultado,
        Guid VerificadoPor,
        string? ProximaAcao
    ) : ICommand;

    public class RegistrarVerificacaoEficaciaNcrCommandValidator : AbstractValidator<RegistrarVerificacaoEficaciaNcrCommand>
    {
        public RegistrarVerificacaoEficaciaNcrCommandValidator()
        {
            RuleFor(c => c.NcrId).NotEmpty();
            RuleFor(c => c.Criterio).NotEmpty();
            RuleFor(c => c.DescricaoResultado).NotEmpty();
            RuleFor(c => c.VerificadoPor).NotEmpty();
        }
    }

    /// <summary>Avanca a etapa da NCR (Triagem/Investigacao/CAPA/Verificacao) com guardas de pre-requisito.</summary>
    public record AvancarEtapaNcrCommand(Guid NcrId, ENcrEtapa NovaEtapa) : ICommand;

    public class AvancarEtapaNcrCommandValidator : AbstractValidator<AvancarEtapaNcrCommand>
    {
        public AvancarEtapaNcrCommandValidator() => RuleFor(c => c.NcrId).NotEmpty();
    }

    /// <summary>Encerra a NCR (exige conclusao; nao encerra com acao aberta; exige verificacao quando ha CAPA).</summary>
    public record EncerrarNcrCommand(Guid NcrId, string Conclusao) : ICommand;

    public class EncerrarNcrCommandValidator : AbstractValidator<EncerrarNcrCommand>
    {
        public EncerrarNcrCommandValidator()
        {
            RuleFor(c => c.NcrId).NotEmpty();
            RuleFor(c => c.Conclusao).NotEmpty();
        }
    }

    /// <summary>Cancela a NCR (exige motivo).</summary>
    public record CancelarNcrCommand(Guid NcrId, string Motivo) : ICommand;

    public class CancelarNcrCommandValidator : AbstractValidator<CancelarNcrCommand>
    {
        public CancelarNcrCommandValidator()
        {
            RuleFor(c => c.NcrId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty();
        }
    }
}
