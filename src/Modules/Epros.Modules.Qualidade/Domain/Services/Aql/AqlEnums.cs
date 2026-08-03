namespace Epros.Modules.Qualidade.Domain.Services.Aql
{
    /// <summary>
    /// Nivel de inspecao (ISO 2859-1 / NBR 5426, Tab. I). Gerais I/II/III (II = padrao);
    /// especiais S-1..S-4 (amostras pequenas p/ ensaio caro/destrutivo).
    /// A ordem do enum casa com a ordem das colunas da Tabela I (indice direto).
    /// </summary>
    public enum ENivelInspecao
    {
        S1 = 0,
        S2 = 1,
        S3 = 2,
        S4 = 3,
        I = 4,
        II = 5,
        III = 6
    }

    /// <summary>Severidade corrente do regime de inspecao (comutacao NBR 5427 / MIL-STD-105 §8-§10).</summary>
    public enum ESeveridadeAql
    {
        Normal = 0,
        Severa = 1,
        Atenuada = 2
    }

    /// <summary>Decisao do lote apos contar defeituosos na amostra.</summary>
    public enum EDecisaoLote
    {
        /// <summary>d &lt;= Ac -> aceita.</summary>
        Aceita = 0,
        /// <summary>d &gt;= Re -> rejeita.</summary>
        Rejeita = 1,
        /// <summary>
        /// Atenuada: Ac &lt; d &lt; Re (vao entre Ac e Re) -> aceita o lote MAS forca retorno a
        /// inspecao normal no proximo (RN-05). Regra de negocio da norma, nao bug.
        /// </summary>
        AceitaERetornaNormal = 2
    }
}
