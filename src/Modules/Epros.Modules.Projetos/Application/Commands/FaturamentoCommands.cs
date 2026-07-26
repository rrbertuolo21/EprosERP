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
        Guid? OrigemId
    ) : ICommand;

    public record SubmeterFaturamentoProjetoCommand(Guid FaturamentoProjetoId) : ICommand;

    /// <summary>RN-FAT-006/008: aprovar (Ativo) publica evento financeiro via Outbox.</summary>
    public record AprovarFaturamentoProjetoCommand(Guid FaturamentoProjetoId) : ICommand;

    public record RejeitarFaturamentoProjetoCommand(Guid FaturamentoProjetoId, string Motivo) : ICommand;
}
