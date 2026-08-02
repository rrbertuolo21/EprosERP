using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Agricultor.Application.Queries
{
    // ===================== Painel =====================
    public record ListarPropriedadesQuery() : IQuery<CommandResult>;
    public record ObterPropriedadeQuery(Guid PropriedadeId) : IQuery<CommandResult>;
    public record ListarSafrasQuery(Guid? TalhaoId) : IQuery<CommandResult>;
    public record ListarDespesasQuery(Guid? PropriedadeId, Guid? SafraId) : IQuery<CommandResult>;
    public record ListarReceitasQuery(Guid? PropriedadeId, Guid? SafraId) : IQuery<CommandResult>;
    public record ListarCulturasQuery() : IQuery<CommandResult>;
    public record ListarFornecedoresQuery() : IQuery<CommandResult>;
    public record ListarCategoriasDespesaQuery() : IQuery<CommandResult>;

    /// <summary>
    /// PAINEL DO AGRICULTOR — visão consolidada (AGR-PAINEL): saldos (receitas − despesas), resultado por
    /// atividade/safra e indicadores. Filtro opcional por propriedade e ano.
    /// </summary>
    public record PainelConsolidadoQuery(Guid? PropriedadeId, int? Ano) : IQuery<CommandResult>;

    // ===================== LCDPR =====================
    public record ListarEscrituracoesQuery() : IQuery<CommandResult>;
    public record ObterEscrituracaoQuery(Guid EscrituracaoId) : IQuery<CommandResult>;

    /// <summary>Roda o validador próprio do LCDPR (bloqueantes + alertas) sem exportar.</summary>
    public record ValidarEscrituracaoQuery(Guid EscrituracaoId) : IQuery<CommandResult>;

    /// <summary>Pré-visualiza o conteúdo do arquivo .txt sem alterar o status da escrituração.</summary>
    public record PreviewArquivoLcdprQuery(Guid EscrituracaoId) : IQuery<CommandResult>;
}
