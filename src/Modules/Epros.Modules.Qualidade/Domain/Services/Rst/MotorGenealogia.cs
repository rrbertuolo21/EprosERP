using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Qualidade.Domain.Services.Rst
{
    /// <summary>No de entrada para montar a arvore (id + pai + rotulo).</summary>
    public readonly struct NoGenealogia
    {
        public Guid Id { get; }
        public Guid? PaiId { get; }
        public string Rotulo { get; }
        public bool Lacuna { get; }
        public NoGenealogia(Guid id, Guid? paiId, string rotulo, bool lacuna)
        { Id = id; PaiId = paiId; Rotulo = rotulo; Lacuna = lacuna; }
    }

    /// <summary>No de saida da arvore (com filhos aninhados).</summary>
    public sealed class NoArvoreGenealogia
    {
        public Guid Id { get; }
        public string Rotulo { get; }
        public bool Lacuna { get; }
        public int Nivel { get; internal set; }
        public List<NoArvoreGenealogia> Filhos { get; } = new();
        public NoArvoreGenealogia(Guid id, string rotulo, bool lacuna, int nivel)
        { Id = id; Rotulo = rotulo; Lacuna = lacuna; Nivel = nivel; }
    }

    public sealed class ArvoreGenealogia
    {
        public List<NoArvoreGenealogia> Raizes { get; } = new();
        /// <summary>Genealogia com lacuna: segue so com justificativa+risco (RN-RST-011).</summary>
        public bool TemLacuna { get; init; }
        public int TotalNos { get; init; }
    }

    /// <summary>
    /// Motor de genealogia (QLD-RST): monta a arvore MP->WIP->PA a partir dos nos registrados.
    /// A genealogia REAL e um contrato de leitura com Estoque/Producao (D6/D24) — este motor apenas
    /// estrutura os nos ja capturados na campanha, sem duplicar saldo. Puro e testavel.
    /// </summary>
    public sealed class MotorGenealogia
    {
        /// <summary>Monta a arvore a partir dos nos (detecta lacunas; protege contra ciclos).</summary>
        public ArvoreGenealogia MontarArvore(IEnumerable<NoGenealogia> nos)
        {
            var lista = nos?.ToList() ?? new List<NoGenealogia>();
            var mapa = lista.ToDictionary(n => n.Id, n => new NoArvoreGenealogia(n.Id, n.Rotulo, n.Lacuna, 0));
            var arvore = new ArvoreGenealogia { TemLacuna = lista.Any(n => n.Lacuna), TotalNos = lista.Count };

            foreach (var no in lista)
            {
                var atual = mapa[no.Id];
                if (no.PaiId.HasValue && mapa.TryGetValue(no.PaiId.Value, out var pai) && pai.Id != atual.Id)
                    pai.Filhos.Add(atual);
                else
                    arvore.Raizes.Add(atual);
            }

            foreach (var raiz in arvore.Raizes)
                AtribuirNivel(raiz, 0, new HashSet<Guid>());

            return arvore;
        }

        private static void AtribuirNivel(NoArvoreGenealogia no, int nivel, HashSet<Guid> visitados)
        {
            if (!visitados.Add(no.Id)) return; // protege contra ciclo
            no.Nivel = nivel;
            foreach (var f in no.Filhos)
                AtribuirNivel(f, nivel + 1, visitados);
        }
    }
}
