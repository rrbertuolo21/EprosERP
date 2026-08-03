using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Producao.Domain.Services
{
    /// <summary>
    /// PRD-ESC — Motor de sequenciamento/APS com capacidade finita (PD5 · DP-ESC-003/007/008/009).
    /// Ordena operações por precedência (topológica) + prioridade, respeitando janela e setup/teardown,
    /// com CAPACIDADE FINITA por centro de trabalho (fila finita — padrão do escopo máximo): cada centro
    /// processa uma operação por vez; uma operação só inicia quando (a) todas as precedências terminaram,
    /// (b) o centro está livre e (c) a janela permite. Detecta ciclo de precedência.
    ///
    /// Motor puro de domínio (opera sobre DTOs), 100% testável e reutilizável pelo handler de programação.
    /// </summary>
    public sealed class SequenciamentoService
    {
        public sealed record OperacaoEntrada(
            Guid Id,
            Guid CentroTrabalhoId,
            int Prioridade,                 // menor = mais prioritária
            decimal DuracaoMinutos,
            decimal SetupMinutos,
            IReadOnlyCollection<Guid> Precedencias,
            DateTime? JanelaInicio = null);

        public sealed record OperacaoAgendada(
            Guid Id,
            Guid CentroTrabalhoId,
            int Ordem,                      // ordem global de despacho (1..n)
            DateTime Inicio,
            DateTime Fim,
            decimal SetupMinutos);

        /// <summary>
        /// Sequencia as operações a partir de <paramref name="inicioHorizonte"/>.
        /// Lança <see cref="InvalidOperationException"/> se houver ciclo de precedência (indispachável).
        /// </summary>
        public IReadOnlyList<OperacaoAgendada> Sequenciar(
            IEnumerable<OperacaoEntrada> operacoes,
            DateTime inicioHorizonte)
        {
            var ops = operacoes?.ToList() ?? throw new ArgumentNullException(nameof(operacoes));
            if (ops.Count == 0) return Array.Empty<OperacaoAgendada>();

            var porId = ops.ToDictionary(o => o.Id);
            var pendentes = new HashSet<Guid>(ops.Select(o => o.Id));
            var fimOperacao = new Dictionary<Guid, DateTime>();
            var centroLivre = new Dictionary<Guid, DateTime>();
            var resultado = new List<OperacaoAgendada>();
            var ordem = 0;

            DateTime CentroLivreEm(Guid centro) =>
                centroLivre.TryGetValue(centro, out var t) ? t : inicioHorizonte;

            bool PrecedenciasProntas(OperacaoEntrada o) =>
                o.Precedencias == null || o.Precedencias.All(p => !porId.ContainsKey(p) || fimOperacao.ContainsKey(p));

            DateTime InicioMaisCedo(OperacaoEntrada o)
            {
                var t = CentroLivreEm(o.CentroTrabalhoId);
                if (o.JanelaInicio.HasValue && o.JanelaInicio.Value > t) t = o.JanelaInicio.Value;
                if (o.Precedencias != null)
                    foreach (var p in o.Precedencias)
                        if (fimOperacao.TryGetValue(p, out var fp) && fp > t) t = fp;
                return t;
            }

            while (pendentes.Count > 0)
            {
                var prontas = pendentes
                    .Select(id => porId[id])
                    .Where(PrecedenciasProntas)
                    .ToList();

                if (prontas.Count == 0)
                    throw new InvalidOperationException(
                        "Ciclo de precedência entre operações — sequência indispachável (DP-ESC-009).");

                // escolhe: menor início possível, depois prioridade, depois menor duração (SPT)
                var escolhida = prontas
                    .OrderBy(o => InicioMaisCedo(o))
                    .ThenBy(o => o.Prioridade)
                    .ThenBy(o => o.DuracaoMinutos)
                    .ThenBy(o => o.Id)
                    .First();

                var inicio = InicioMaisCedo(escolhida);
                var fim = inicio
                    .AddMinutes((double)escolhida.SetupMinutos)
                    .AddMinutes((double)escolhida.DuracaoMinutos);

                centroLivre[escolhida.CentroTrabalhoId] = fim;
                fimOperacao[escolhida.Id] = fim;
                pendentes.Remove(escolhida.Id);

                resultado.Add(new OperacaoAgendada(
                    escolhida.Id, escolhida.CentroTrabalhoId, ++ordem, inicio, fim, escolhida.SetupMinutos));
            }

            return resultado;
        }
    }
}
