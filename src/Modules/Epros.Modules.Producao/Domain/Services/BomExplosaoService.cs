using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;

namespace Epros.Modules.Producao.Domain.Services
{
    /// <summary>
    /// PRD-BOM — Motor de explosão multinível da estrutura de produto (PD6).
    /// Regras cobertas:
    /// - Estrutura ativa/vigente por variação por período (CHK-BOM-001 · DP-BOM-002/003).
    /// - Validação circular (BOM-REG-019 · algoritmo DP-BOM-014): rejeita ciclo na montagem/explosão.
    /// - Explosão multinível com desperdício por linha, item fantasma (atravessa) e submontagem
    ///   (desce um nível quando existe estrutura vigente para o componente) — DP-BOM-010/013.
    ///
    /// Motor puro de domínio: recebe o catálogo de estruturas já carregado (todas as candidatas,
    /// com componentes), sem dependência de infraestrutura, para ser 100% testável.
    /// </summary>
    public sealed class BomExplosaoService
    {
        /// <summary>Necessidade de um item resultante da explosão (folha ou submontagem).</summary>
        public sealed class NecessidadeItem
        {
            public Guid ItemId { get; init; }              // VariacaoComponenteId do item requerido
            public int Nivel { get; init; }                // profundidade na árvore (0 = filho direto da raiz)
            public decimal QuantidadePorUnidade { get; init; } // quantidade por 1 unidade do produto raiz
            public decimal QuantidadeTotal { get; init; }  // quantidade para a quantidade solicitada da raiz
            public bool EhFolha { get; init; }             // true = não possui estrutura própria (matéria-prima)
            public Guid? EstruturaFilhaId { get; init; }   // estrutura vigente que explode este item (submontagem)
            public ETipoComponenteBom Tipo { get; init; }
        }

        public sealed class ResultadoExplosao
        {
            public bool CalculoIncompleto { get; init; }   // DP-BOM-006/007/008: componente/estrutura ausente
            public string? MotivoIncompleto { get; init; }
            public bool PossuiCiclo { get; init; }
            public IReadOnlyList<NecessidadeItem> Itens { get; init; } = Array.Empty<NecessidadeItem>();

            /// <summary>Necessidades de matéria-prima (folhas) agregadas por item.</summary>
            public IReadOnlyDictionary<Guid, decimal> NecessidadesFolha =>
                Itens.Where(i => i.EhFolha)
                     .GroupBy(i => i.ItemId)
                     .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantidadeTotal));
        }

        /// <summary>
        /// CHK-BOM-001 · DP-BOM-002/003 — seleciona a única estrutura ativa e vigente na data de
        /// referência para o produto/variação. Retorna null se nenhuma; lança se houver mais de uma
        /// vigente sobreposta (violação de "uma ativa por variação por período").
        /// </summary>
        public BomEstrutura? SelecionarVigente(
            IEnumerable<BomEstrutura> catalogo,
            Guid produtoId,
            Guid variacaoId,
            DateTime dataReferencia)
        {
            var candidatas = catalogo
                .Where(e => e.Status == EStatusWorkflowProducao.Ativo)
                .Where(e => e.ProdutoId == produtoId && (variacaoId == Guid.Empty || e.VariacaoId == variacaoId))
                .Where(e => VigenteEm(e, dataReferencia))
                .ToList();

            if (candidatas.Count > 1)
                throw new InvalidOperationException(
                    $"Mais de uma estrutura ativa vigente para o produto {produtoId}/variação {variacaoId} na data {dataReferencia:d}. " +
                    "Viola 'uma estrutura ativa por variação por período' (CHK-BOM-001).");

            return candidatas.FirstOrDefault();
        }

        private static bool VigenteEm(BomEstrutura e, DateTime data)
        {
            var iniOk = !e.InicioVigencia.HasValue || e.InicioVigencia.Value.Date <= data.Date;
            var fimOk = !e.FimVigencia.HasValue || e.FimVigencia.Value.Date >= data.Date;
            return iniOk && fimOk;
        }

        /// <summary>
        /// BOM-REG-019 · DP-BOM-014 — detecta se a estrutura raiz contém um ciclo (um componente que,
        /// direta ou transitivamente, retorna ao próprio produto da raiz). Resolve submontagens pela
        /// estrutura vigente do componente na data de referência.
        /// </summary>
        public bool PossuiCiclo(
            BomEstrutura raiz,
            IReadOnlyCollection<BomEstrutura> catalogo,
            DateTime dataReferencia)
        {
            var caminho = new HashSet<Guid>();
            return DetectarCiclo(raiz, catalogo, dataReferencia, caminho);
        }

        private bool DetectarCiclo(
            BomEstrutura estrutura,
            IReadOnlyCollection<BomEstrutura> catalogo,
            DateTime dataReferencia,
            HashSet<Guid> caminho)
        {
            // marca o produto desta estrutura no caminho atual
            if (!caminho.Add(estrutura.ProdutoId))
                return true; // produto já presente na ancestralidade → ciclo

            foreach (var comp in estrutura.Componentes)
            {
                var filha = SelecionarVigenteSemLancar(catalogo, comp.VariacaoComponenteId, dataReferencia);
                if (filha != null && DetectarCiclo(filha, catalogo, dataReferencia, caminho))
                    return true;
            }

            caminho.Remove(estrutura.ProdutoId);
            return false;
        }

        // busca a estrutura vigente cujo produto/variação corresponde ao id do componente (submontagem)
        private BomEstrutura? SelecionarVigenteSemLancar(
            IReadOnlyCollection<BomEstrutura> catalogo,
            Guid componenteId,
            DateTime dataReferencia)
        {
            return catalogo
                .Where(e => e.Status == EStatusWorkflowProducao.Ativo)
                .Where(e => e.ProdutoId == componenteId || e.VariacaoId == componenteId)
                .Where(e => VigenteEm(e, dataReferencia))
                .OrderBy(e => e.FimVigencia.HasValue ? 0 : 1)
                .FirstOrDefault();
        }

        /// <summary>
        /// Explode a estrutura raiz para <paramref name="quantidadeSolicitada"/> unidades do produto final,
        /// descendo por submontagens vigentes e atravessando itens fantasma (DP-BOM-013).
        /// Aplica desperdício por linha (já embutido em QuantidadeFinal) e escala pela produtividade
        /// da estrutura (QuantidadeTotal = rendimento). Protege contra ciclo (BOM-REG-019).
        /// </summary>
        public ResultadoExplosao Explodir(
            BomEstrutura raiz,
            decimal quantidadeSolicitada,
            IReadOnlyCollection<BomEstrutura> catalogo,
            DateTime dataReferencia)
        {
            if (raiz == null) throw new ArgumentNullException(nameof(raiz));
            if (quantidadeSolicitada <= 0m)
                return new ResultadoExplosao { CalculoIncompleto = true, MotivoIncompleto = "Quantidade solicitada deve ser maior que zero." };

            if (PossuiCiclo(raiz, catalogo, dataReferencia))
                return new ResultadoExplosao { PossuiCiclo = true, MotivoIncompleto = "Estrutura contém ciclo (BOM-REG-019)." };

            var itens = new List<NecessidadeItem>();
            var incompleto = false;
            string? motivo = null;

            void Descer(BomEstrutura estrutura, decimal fatorAcumulado, int nivel, HashSet<Guid> caminho)
            {
                caminho.Add(estrutura.ProdutoId);

                var rendimento = estrutura.QuantidadeTotal > 0m ? estrutura.QuantidadeTotal : 1m;

                foreach (var comp in estrutura.Componentes)
                {
                    var qtdLinha = comp.QuantidadeFinal ?? comp.Quantidade; // já com desperdício
                    var porUnidade = qtdLinha / rendimento;                 // por 1 unidade do produto da estrutura
                    var fatorFilho = fatorAcumulado * porUnidade;

                    var filha = SelecionarVigenteSemLancar(catalogo, comp.VariacaoComponenteId, dataReferencia);

                    if (comp.EhFantasma)
                    {
                        // item fantasma: não gera necessidade própria; atravessa para a submontagem
                        if (filha != null)
                        {
                            if (caminho.Contains(filha.ProdutoId)) { incompleto = true; motivo = "Ciclo em submontagem fantasma."; continue; }
                            Descer(filha, fatorFilho, nivel + 1, caminho);
                        }
                        else
                        {
                            // fantasma sem estrutura: não há o que atravessar → cálculo incompleto (DP-BOM-008)
                            incompleto = true;
                            motivo = "Item fantasma sem submontagem vigente para explodir (DP-BOM-008).";
                        }
                        continue;
                    }

                    var ehFolha = filha == null;
                    itens.Add(new NecessidadeItem
                    {
                        ItemId = comp.VariacaoComponenteId,
                        Nivel = nivel,
                        QuantidadePorUnidade = porUnidade,
                        QuantidadeTotal = fatorFilho * quantidadeSolicitada,
                        EhFolha = ehFolha,
                        EstruturaFilhaId = filha?.Id,
                        Tipo = comp.TipoComponente
                    });

                    if (filha != null && !caminho.Contains(filha.ProdutoId))
                        Descer(filha, fatorFilho, nivel + 1, caminho);
                }

                caminho.Remove(estrutura.ProdutoId);
            }

            Descer(raiz, 1m, 0, new HashSet<Guid>());

            return new ResultadoExplosao
            {
                Itens = itens,
                CalculoIncompleto = incompleto,
                MotivoIncompleto = motivo
            };
        }
    }
}
