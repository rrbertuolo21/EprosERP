using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Qualidade.Domain.Services.Aql
{
    /// <summary>
    /// Opcoes externas da comutacao (parametros do overlay/cliente, NAO da norma pura).
    /// Atenuada exige condicoes de negocio alem do contador (produzir estavel, ser desejavel).
    /// </summary>
    public sealed class OpcoesComutacao
    {
        /// <summary>(d) Inspecao atenuada e desejavel pela autoridade responsavel (RN-04). Default: desligado.</summary>
        public bool AtenuadaHabilitada { get; init; } = false;

        /// <summary>(c) Producao estavel/regular. Se falso, atenuada nao inicia e, se ativa, volta a normal (RN-05).</summary>
        public bool ProducaoEstavel { get; init; } = true;

        /// <summary>(b) Numero-limite de defeituosos nos 10 lotes p/ ir a atenuada (Tab. VIII).
        /// // valida (PDF ABNT NBR 5426 — Tab. VIII). Default MaxValue = so o contador decide.</summary>
        public int LimiteDefeituososAtenuada { get; init; } = int.MaxValue;
    }

    /// <summary>
    /// Estado persistivel da comutacao por (fornecedor x produto x caracteristica/AQL).
    /// Guarda a severidade corrente e os contadores do historico recente (skill: "Estado que o
    /// motor precisa persistir").
    /// </summary>
    public sealed class EstadoComutacaoAql
    {
        public ESeveridadeAql Severidade { get; internal set; } = ESeveridadeAql.Normal;

        /// <summary>Inspecao suspensa (RN-06): fornecedor deve corrigir o processo antes de retomar (em severa).</summary>
        public bool Suspensa { get; internal set; }

        // Contadores (expostos como get p/ persistencia/inspecao; set interno controlado pelo motor).
        public int ConsecutivosAceitosNormal { get; internal set; }
        public int SomaDefeituososNormal { get; internal set; }
        public int ConsecutivosAceitosSevera { get; internal set; }
        public int RejeitadosAcumuladosSevera { get; internal set; }

        /// <summary>Janela dos ultimos (ate 5) resultados em normal: true = lote rejeitado. Regra 2-de-5.</summary>
        public List<bool> JanelaNormalRejeicoes { get; internal set; } = new();

        public static EstadoComutacaoAql Inicial() => new(); // RN-01: inicia em normal.

        /// <summary>
        /// Reconstroi o estado a partir de valores persistidos (ou para testes). Unico ponto publico
        /// que define a severidade; a evolucao normal e feita pelo <see cref="MotorComutacao"/>.
        /// </summary>
        public static EstadoComutacaoAql Restaurar(
            ESeveridadeAql severidade, bool suspensa = false,
            int consecutivosAceitosNormal = 0, int somaDefeituososNormal = 0,
            int consecutivosAceitosSevera = 0, int rejeitadosAcumuladosSevera = 0,
            IEnumerable<bool>? janelaNormalRejeicoes = null)
            => new()
            {
                Severidade = severidade,
                Suspensa = suspensa,
                ConsecutivosAceitosNormal = consecutivosAceitosNormal,
                SomaDefeituososNormal = somaDefeituososNormal,
                ConsecutivosAceitosSevera = consecutivosAceitosSevera,
                RejeitadosAcumuladosSevera = rejeitadosAcumuladosSevera,
                JanelaNormalRejeicoes = janelaNormalRejeicoes != null ? new List<bool>(janelaNormalRejeicoes) : new List<bool>()
            };
    }

    /// <summary>
    /// Motor de comutacao de severidade (NBR 5427 / MIL-STD-105 §8-§10). Puro: dado o estado corrente
    /// e o resultado de UM lote, computa a severidade do PROXIMO lote e detecta suspensao.
    /// Regras RN-01..RN-06 da skill canonica `qualidade`.
    /// </summary>
    public sealed class MotorComutacao
    {
        /// <summary>
        /// Registra o resultado de um lote e evolui o estado. Retorna o mesmo estado (mutado) para encadear.
        /// </summary>
        public EstadoComutacaoAql Registrar(
            EstadoComutacaoAql estado, EDecisaoLote decisao, int defeituosos, OpcoesComutacao? opcoes = null)
        {
            opcoes ??= new OpcoesComutacao();
            if (estado.Suspensa) return estado; // suspensa: nao processa mais lotes ate correcao manual.

            switch (estado.Severidade)
            {
                case ESeveridadeAql.Normal:
                    ProcessarNormal(estado, decisao, defeituosos, opcoes);
                    break;
                case ESeveridadeAql.Severa:
                    ProcessarSevera(estado, decisao);
                    break;
                case ESeveridadeAql.Atenuada:
                    ProcessarAtenuada(estado, decisao, opcoes);
                    break;
            }
            return estado;
        }

        private static void ProcessarNormal(EstadoComutacaoAql e, EDecisaoLote decisao, int defeituosos, OpcoesComutacao opcoes)
        {
            bool rejeitado = decisao == EDecisaoLote.Rejeita;

            // Janela 2-de-5 (ultimos 5 lotes em normal).
            e.JanelaNormalRejeicoes.Add(rejeitado);
            if (e.JanelaNormalRejeicoes.Count > 5)
                e.JanelaNormalRejeicoes.RemoveAt(0);

            if (rejeitado)
            {
                e.ConsecutivosAceitosNormal = 0;
                e.SomaDefeituososNormal = 0;

                // RN-02: 2 (ou mais) de 5 consecutivos rejeitados em normal -> severa.
                if (e.JanelaNormalRejeicoes.Count(x => x) >= 2)
                {
                    IrParaSevera(e);
                }
                return;
            }

            // Aceito.
            e.ConsecutivosAceitosNormal++;
            e.SomaDefeituososNormal += defeituosos;

            // RN-04: 10 consecutivos aceitos + soma <= limite + producao estavel + atenuada desejavel.
            if (opcoes.AtenuadaHabilitada && opcoes.ProducaoEstavel
                && e.ConsecutivosAceitosNormal >= 10
                && e.SomaDefeituososNormal <= opcoes.LimiteDefeituososAtenuada)
            {
                e.Severidade = ESeveridadeAql.Atenuada;
                e.ConsecutivosAceitosNormal = 0;
                e.SomaDefeituososNormal = 0;
                e.JanelaNormalRejeicoes.Clear();
            }
        }

        private static void ProcessarSevera(EstadoComutacaoAql e, EDecisaoLote decisao)
        {
            if (decisao == EDecisaoLote.Rejeita)
            {
                e.ConsecutivosAceitosSevera = 0;
                e.RejeitadosAcumuladosSevera++;
                // RN-06: 5 rejeitados acumulados em severa -> suspende (retoma em severa apos correcao).
                if (e.RejeitadosAcumuladosSevera >= 5)
                    e.Suspensa = true;
                return;
            }

            // Aceito (inclui o caso-limite; em severa nao ha vao).
            e.ConsecutivosAceitosSevera++;
            // RN-03: 5 consecutivos aceitos em severa -> normal.
            if (e.ConsecutivosAceitosSevera >= 5)
                VoltarParaNormal(e);
        }

        private static void ProcessarAtenuada(EstadoComutacaoAql e, EDecisaoLote decisao, OpcoesComutacao opcoes)
        {
            // RN-05: rejeitado, OU d entre Ac e Re (AceitaERetornaNormal), OU producao irregular -> volta a normal.
            if (decisao == EDecisaoLote.Rejeita
                || decisao == EDecisaoLote.AceitaERetornaNormal
                || !opcoes.ProducaoEstavel)
            {
                VoltarParaNormal(e);
            }
            // d <= Ac com producao estavel: permanece em atenuada.
        }

        private static void IrParaSevera(EstadoComutacaoAql e)
        {
            e.Severidade = ESeveridadeAql.Severa;
            e.ConsecutivosAceitosSevera = 0;
            e.RejeitadosAcumuladosSevera = 0;
            e.JanelaNormalRejeicoes.Clear();
        }

        private static void VoltarParaNormal(EstadoComutacaoAql e)
        {
            e.Severidade = ESeveridadeAql.Normal;
            e.ConsecutivosAceitosNormal = 0;
            e.SomaDefeituososNormal = 0;
            e.ConsecutivosAceitosSevera = 0;
            e.RejeitadosAcumuladosSevera = 0;
            e.JanelaNormalRejeicoes.Clear();
        }
    }
}
