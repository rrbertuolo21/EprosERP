using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>APE-012: altera mínimo, máximo e custeio SEM tocar em saldo/valor/reserva. Auditoria antes/depois.</summary>
    public record ParametrizarPosicaoEstoqueCommand(
        Guid EstoqueProdutoId,
        decimal QuantidadeEstoqueMinimo,
        decimal QuantidadeEstoqueMaximo,
        ETipoCusteioEstoque TipoCusteioEstoque,
        string? Justificativa = null
    ) : ICommand;

    /// <summary>APE-010/011: reprocessa alertas de reposição (saldo ≤ mínimo) e excesso (saldo &gt; máximo).</summary>
    public record ReprocessarAlertasEstoqueCommand(Guid? EmpresaId = null) : ICommand;

    /// <summary>Resolve ou ignora um alerta aberto.</summary>
    public record ResolverAlertaEstoqueCommand(Guid AlertaId, bool Ignorar = false) : ICommand;
}
