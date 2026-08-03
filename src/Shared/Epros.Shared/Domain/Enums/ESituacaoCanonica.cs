using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    /// <summary>
    /// TRANSVERSAL T3 — SITUAÇÃO CANÔNICA ÚNICA da plataforma.
    /// Máquina de estados única para TODO agregado que tem ciclo de vida com workflow.
    /// Cada módulo mapeia o seu ciclo próprio a este domínio; NÃO se cria um enum divergente
    /// por módulo (ver RELATORIOS/DECISOES-TRANSVERSAIS.md · T3).
    ///
    /// Fluxo de referência: Rascunho → EmAnalise → Ativo → Suspenso → Encerrado → Inativo/Cancelado.
    /// As transições permitidas vivem em <see cref="Epros.Shared.Domain.StatusCanonico.MaquinaSituacaoCanonica"/>.
    ///
    /// OBS.: distinta de <see cref="ESituacao"/> (situação de título financeiro Aberto/Pago/…),
    /// que é um enum de domínio específico e continua válido para Contas a Pagar/Receber.
    /// </summary>
    public enum ESituacaoCanonica
    {
        /// <summary>Recém-criado, ainda em edição; não produz efeito.</summary>
        Rascunho = 1,

        [Description("Em Análise")]
        EmAnalise = 2,

        /// <summary>Vigente/operante.</summary>
        Ativo = 3,

        /// <summary>Temporariamente paralisado; pode voltar a Ativo.</summary>
        Suspenso = 4,

        /// <summary>Concluído pelo fim natural do ciclo (terminal).</summary>
        Encerrado = 5,

        /// <summary>Desativado sem conclusão de ciclo; pode ser reativado.</summary>
        Inativo = 6,

        /// <summary>Cancelado/anulado (terminal).</summary>
        Cancelado = 7
    }
}
