using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-FAT (Faturamento de Projeto) =====

    public record CriarFaturamentoProjetoCommand(
        string Codigo,
        string Descricao,
        Guid ProjetoId,
        Guid ResponsavelId,
        Guid? ClienteId,
        EModalidadeFaturamento? ModalidadeFaturamento,
        string? Moeda,
        DateTime? DataVencimento
    ) : ICommand;

    public class CriarFaturamentoProjetoCommandValidator : AbstractValidator<CriarFaturamentoProjetoCommand>
    {
        public CriarFaturamentoProjetoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo do faturamento e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao do faturamento e obrigatoria (max 500).");
            RuleFor(c => c.ProjetoId).NotEmpty().WithMessage("O projeto e obrigatorio.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
            // DP-FAT-002/003: cliente e moeda obrigatórios no faturamento.
            RuleFor(c => c.ClienteId).NotNull().NotEqual(Guid.Empty).WithMessage("O cliente do faturamento e obrigatorio.");
            RuleFor(c => c.Moeda).NotEmpty().WithMessage("A moeda do faturamento e obrigatoria.");
        }
    }

    public record AdicionarItemFaturamentoCommand(
        Guid FaturamentoProjetoId,
        int Sequencia,
        decimal? Quantidade,
        string? Observacao,
        ETipoItemFaturamento? TipoItem,
        decimal? ValorUnitario,
        decimal? ValorTotal,
        string? OrigemTipo,
        Guid? OrigemId,
        bool Reembolsavel = false
    ) : ICommand;

    public record SubmeterFaturamentoProjetoCommand(Guid FaturamentoProjetoId) : ICommand;

    /// <summary>RN-FAT-006/008: aprovar (Ativo) publica evento financeiro via Outbox.</summary>
    public record AprovarFaturamentoProjetoCommand(Guid FaturamentoProjetoId) : ICommand;

    public record RejeitarFaturamentoProjetoCommand(Guid FaturamentoProjetoId, string Motivo) : ICommand;

    /// <summary>DP-FAT-004/008: aplica tributos/retenções fiscais ao faturamento. // valida-contador (valores/alíquotas).</summary>
    public record AplicarTributacaoFaturamentoCommand(
        Guid FaturamentoProjetoId,
        decimal? ValorIss,
        decimal? ValorIrrf,
        decimal? ValorInss,
        decimal? ValorPis,
        decimal? ValorCofins,
        decimal? ValorCsll
    ) : ICommand;
}
