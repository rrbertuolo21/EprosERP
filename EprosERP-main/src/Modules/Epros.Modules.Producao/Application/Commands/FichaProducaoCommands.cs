using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarFichaProducaoCommand(
        Guid VendaId,
        Guid ItemVendaId,
        Guid PessoaId,
        ELogomarcaFichaProducao Logomarca,
        int LateraisPorta,
        int ApoioCabeca,
        DateTime? Entrada = null,
        DateTime? Saida = null,
        string? Transportadora = null,
        string? AnoModelo = null,
        string? CorCouro = null,
        string? Costura = null,
        string? TipoAcento = null,
        string? TipoEncosto = null,
        string? Abd = null,
        string? Abt = null,
        string? Observacao = null
    ) : ICommand;

    public class CriarFichaProducaoCommandValidator : AbstractValidator<CriarFichaProducaoCommand>
    {
        public CriarFichaProducaoCommandValidator()
        {
            RuleFor(c => c.VendaId).NotEmpty().WithMessage("A venda é obrigatória.");
            RuleFor(c => c.ItemVendaId).NotEmpty().WithMessage("O item da venda é obrigatório.");
            RuleFor(c => c.PessoaId).NotEmpty().WithMessage("A pessoa é obrigatória.");
        }
    }

    public record AtualizarConfiguracaoFichaProducaoCommand(
        Guid Id,
        ELogomarcaFichaProducao Logomarca,
        int LateraisPorta,
        int ApoioCabeca,
        string? Transportadora = null,
        string? AnoModelo = null,
        string? CorCouro = null,
        string? Costura = null,
        string? TipoAcento = null,
        string? TipoEncosto = null,
        string? Abd = null,
        string? Abt = null,
        string? Observacao = null
    ) : ICommand;

    public record IniciarProducaoFichaCommand(Guid Id) : ICommand;
    public record ConcluirFichaProducaoCommand(Guid Id) : ICommand;
}
