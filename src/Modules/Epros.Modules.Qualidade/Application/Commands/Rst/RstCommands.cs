using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands.Rst
{
    /// <summary>Abre uma campanha de rastreabilidade/recall.</summary>
    public record CriarCampanhaRecallCommand(
        string Codigo, string Titulo, ERstGravidade Gravidade, Guid ResponsavelId,
        string? Descricao, Guid? ProdutoId, Guid? NcrId) : ICommand;

    public class CriarCampanhaRecallCommandValidator : AbstractValidator<CriarCampanhaRecallCommand>
    {
        public CriarCampanhaRecallCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Titulo).NotEmpty().MaximumLength(255);
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    public record AdicionarItemAfetadoRecallCommand(
        Guid CampanhaId, decimal Quantidade, Guid? ProdutoId, string? Lote, string? Serial, string? Localizacao) : ICommand;

    public class AdicionarItemAfetadoRecallCommandValidator : AbstractValidator<AdicionarItemAfetadoRecallCommand>
    {
        public AdicionarItemAfetadoRecallCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.Quantidade).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Registra um no da genealogia (MP/WIP/PA). Lacuna exige justificativa (RN-RST-011).</summary>
    public record RegistrarGenealogiaNoCommand(
        Guid CampanhaId, ERstTipoNoGenealogia TipoNo, int Nivel, Guid? PaiId, Guid? ProdutoId,
        string? Lote, string? Serial, bool Lacuna, string? Justificativa) : ICommand;

    public class RegistrarGenealogiaNoCommandValidator : AbstractValidator<RegistrarGenealogiaNoCommand>
    {
        public RegistrarGenealogiaNoCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.Nivel).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Contencao: solicita bloqueio de lote/serie ao Estoque (Outbox). Nao movimenta saldo.</summary>
    public record SolicitarBloqueioRecallCommand(
        Guid CampanhaId, decimal Quantidade, string? Lote, string? Serial, string? Motivo) : ICommand;

    public class SolicitarBloqueioRecallCommandValidator : AbstractValidator<SolicitarBloqueioRecallCommand>
    {
        public SolicitarBloqueioRecallCommandValidator() => RuleFor(c => c.CampanhaId).NotEmpty();
    }

    public record RegistrarComunicacaoRecallCommand(
        Guid CampanhaId, ERstCanalComunicacao Canal, string Conteudo, bool Aprovar, Guid? AprovadoPor) : ICommand;

    public class RegistrarComunicacaoRecallCommandValidator : AbstractValidator<RegistrarComunicacaoRecallCommand>
    {
        public RegistrarComunicacaoRecallCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.Conteudo).NotEmpty();
        }
    }

    public record RegistrarRecolhimentoRecallCommand(Guid CampanhaId, decimal QuantidadePrevista, decimal QuantidadeRecolhida) : ICommand;

    public class RegistrarRecolhimentoRecallCommandValidator : AbstractValidator<RegistrarRecolhimentoRecallCommand>
    {
        public RegistrarRecolhimentoRecallCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.QuantidadePrevista).GreaterThanOrEqualTo(0);
        }
    }

    public record RegistrarDisposicaoRecallCommand(Guid CampanhaId, ERstTipoDisposicao TipoDisposicao, decimal Quantidade, string? Observacao) : ICommand;

    public class RegistrarDisposicaoRecallCommandValidator : AbstractValidator<RegistrarDisposicaoRecallCommand>
    {
        public RegistrarDisposicaoRecallCommandValidator() => RuleFor(c => c.CampanhaId).NotEmpty();
    }

    public record AvancarEtapaRecallCommand(Guid CampanhaId, ERstEtapaCampanha NovaEtapa) : ICommand;

    public class AvancarEtapaRecallCommandValidator : AbstractValidator<AvancarEtapaRecallCommand>
    {
        public AvancarEtapaRecallCommandValidator() => RuleFor(c => c.CampanhaId).NotEmpty();
    }

    public record EncerrarRecallCommand(Guid CampanhaId, string Conclusao) : ICommand;

    public class EncerrarRecallCommandValidator : AbstractValidator<EncerrarRecallCommand>
    {
        public EncerrarRecallCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.Conclusao).NotEmpty();
        }
    }

    public record CancelarRecallCommand(Guid CampanhaId, string Motivo) : ICommand;

    public class CancelarRecallCommandValidator : AbstractValidator<CancelarRecallCommand>
    {
        public CancelarRecallCommandValidator()
        {
            RuleFor(c => c.CampanhaId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty();
        }
    }
}
