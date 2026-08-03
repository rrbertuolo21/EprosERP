using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Enums;

namespace Epros.Shared.Domain.StatusCanonico
{
    /// <summary>
    /// TRANSVERSAL T3 — máquina de estados ÚNICA e reutilizável para a
    /// <see cref="ESituacaoCanonica"/>. Centraliza as transições permitidas para que
    /// front/relatórios/handlers de TODOS os módulos tenham comportamento e mensagens
    /// consistentes. Nenhum módulo redefine transições localmente — mapeia o seu ciclo
    /// aos estados canônicos e usa esta máquina para validar a mudança.
    /// </summary>
    public static class MaquinaSituacaoCanonica
    {
        // Grafo de transições permitidas (de → conjunto de destinos válidos).
        private static readonly IReadOnlyDictionary<ESituacaoCanonica, ESituacaoCanonica[]> _transicoes =
            new Dictionary<ESituacaoCanonica, ESituacaoCanonica[]>
            {
                [ESituacaoCanonica.Rascunho] = new[]
                {
                    ESituacaoCanonica.EmAnalise,
                    ESituacaoCanonica.Ativo,       // fluxo direto quando não há análise
                    ESituacaoCanonica.Cancelado
                },
                [ESituacaoCanonica.EmAnalise] = new[]
                {
                    ESituacaoCanonica.Ativo,
                    ESituacaoCanonica.Rascunho,    // devolução para correção
                    ESituacaoCanonica.Cancelado
                },
                [ESituacaoCanonica.Ativo] = new[]
                {
                    ESituacaoCanonica.Suspenso,
                    ESituacaoCanonica.Encerrado,
                    ESituacaoCanonica.Inativo
                },
                [ESituacaoCanonica.Suspenso] = new[]
                {
                    ESituacaoCanonica.Ativo,       // reativação
                    ESituacaoCanonica.Encerrado,
                    ESituacaoCanonica.Inativo
                },
                [ESituacaoCanonica.Inativo] = new[]
                {
                    ESituacaoCanonica.Ativo        // reativação governada
                },
                // Estados terminais: sem saída.
                [ESituacaoCanonica.Encerrado] = new ESituacaoCanonica[0],
                [ESituacaoCanonica.Cancelado] = new ESituacaoCanonica[0]
            };

        /// <summary>Estados terminais (sem transição de saída).</summary>
        public static bool EhTerminal(ESituacaoCanonica situacao) =>
            _transicoes.TryGetValue(situacao, out var destinos) && destinos.Length == 0;

        /// <summary>Conjunto de destinos válidos a partir de uma situação.</summary>
        public static IReadOnlyCollection<ESituacaoCanonica> TransicoesPermitidas(ESituacaoCanonica de) =>
            _transicoes.TryGetValue(de, out var destinos) ? destinos : new ESituacaoCanonica[0];

        /// <summary>True se a transição de → para é permitida pela máquina canônica.</summary>
        public static bool PodeTransicionar(ESituacaoCanonica de, ESituacaoCanonica para) =>
            de != para && _transicoes.TryGetValue(de, out var destinos) && destinos.Contains(para);

        /// <summary>
        /// Valida e realiza a transição. Retorna resultado com a nova situação em caso de sucesso,
        /// ou uma mensagem padronizada (consistente entre módulos) em caso de transição inválida.
        /// </summary>
        public static ResultadoTransicao Transicionar(ESituacaoCanonica de, ESituacaoCanonica para)
        {
            if (PodeTransicionar(de, para))
                return ResultadoTransicao.Sucesso(para);

            return ResultadoTransicao.Invalida(
                $"Transição inválida: '{de}' → '{para}'. Destinos permitidos a partir de '{de}': " +
                (TransicoesPermitidas(de).Any()
                    ? string.Join(", ", TransicoesPermitidas(de))
                    : "nenhum (estado terminal)") + ".");
        }

        public readonly struct ResultadoTransicao
        {
            public bool Ok { get; }
            public ESituacaoCanonica Situacao { get; }
            public string? Mensagem { get; }

            private ResultadoTransicao(bool ok, ESituacaoCanonica situacao, string? mensagem)
            {
                Ok = ok;
                Situacao = situacao;
                Mensagem = mensagem;
            }

            public static ResultadoTransicao Sucesso(ESituacaoCanonica destino) =>
                new(true, destino, null);

            public static ResultadoTransicao Invalida(string mensagem) =>
                new(false, default, mensagem);
        }
    }
}
