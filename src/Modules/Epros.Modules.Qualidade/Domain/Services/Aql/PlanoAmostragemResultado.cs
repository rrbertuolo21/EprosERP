namespace Epros.Modules.Qualidade.Domain.Services.Aql
{
    /// <summary>
    /// Resultado do calculo de um plano de amostragem simples (ISO 2859-1 / NBR 5426):
    /// letra-codigo, tamanho da amostra, numeros de aceitacao/rejeicao e regime aplicado.
    /// </summary>
    public sealed class PlanoAmostragemResultado
    {
        /// <summary>Letra-codigo efetiva do tamanho da amostra (A..R, sem "I"). Pode diferir da
        /// letra bruta quando houve substituicao por seta (↓/↑) na tabela mestra.</summary>
        public char LetraCodigo { get; }

        /// <summary>Tamanho da amostra (n) a inspecionar.</summary>
        public int TamanhoAmostra { get; }

        /// <summary>Numero de aceitacao (Ac): d &lt;= Ac -> aceita.</summary>
        public int NumeroAceitacao { get; }

        /// <summary>Numero de rejeicao (Re): d &gt;= Re -> rejeita. Em amostragem simples Re = Ac+1
        /// (excecao: atenuada, onde ha um vao entre Ac e Re).</summary>
        public int NumeroRejeicao { get; }

        /// <summary>Quando true, nao ha plano de amostra valido (seta ↓ com n &gt;= N): inspeciona 100%
        /// do lote. Nesse caso Ac/Re/n refletem o lote inteiro.</summary>
        public bool InspecaoTotal { get; }

        /// <summary>Severidade do regime usado no calculo.</summary>
        public ESeveridadeAql Severidade { get; }

        /// <summary>Nivel de inspecao usado.</summary>
        public ENivelInspecao Nivel { get; }

        /// <summary>Tamanho do lote (N) que originou o calculo.</summary>
        public long TamanhoLote { get; }

        /// <summary>AQL (NQA) usado.</summary>
        public decimal Aql { get; }

        public PlanoAmostragemResultado(
            char letraCodigo, int tamanhoAmostra, int numeroAceitacao, int numeroRejeicao,
            bool inspecaoTotal, ESeveridadeAql severidade, ENivelInspecao nivel, long tamanhoLote, decimal aql)
        {
            LetraCodigo = letraCodigo;
            TamanhoAmostra = tamanhoAmostra;
            NumeroAceitacao = numeroAceitacao;
            NumeroRejeicao = numeroRejeicao;
            InspecaoTotal = inspecaoTotal;
            Severidade = severidade;
            Nivel = nivel;
            TamanhoLote = tamanhoLote;
            Aql = aql;
        }

        /// <summary>Decide o destino do lote dado o numero de defeituosos observados na amostra.</summary>
        public EDecisaoLote Decidir(int defeituosos)
        {
            if (defeituosos <= NumeroAceitacao) return EDecisaoLote.Aceita;
            if (defeituosos >= NumeroRejeicao) return EDecisaoLote.Rejeita;
            // Vao entre Ac e Re -> so ocorre em atenuada: aceita mas volta a normal (RN-05).
            return EDecisaoLote.AceitaERetornaNormal;
        }
    }
}
