using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Qualidade.Application.Queries.Ins
{
    /// <summary>
    /// Calcula um plano de amostragem AQL (ISO 2859-1 / NBR 5426) sem persistir: dado N + nivel + AQL +
    /// severidade, retorna letra-codigo, tamanho da amostra, Ac/Re e se cai em inspecao 100%.
    /// Consulta pura sobre o motor de dominio (simulador para a UI/inspetor).
    /// </summary>
    public record CalcularPlanoAmostragemQuery(
        long TamanhoLote,
        ENivelInspecao Nivel,
        decimal Aql,
        ESeveridadeAql Severidade = ESeveridadeAql.Normal
    ) : IQuery<CommandResult>;
}
