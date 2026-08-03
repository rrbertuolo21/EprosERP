/**
 * Composable local da fatia 11 — Devolução/Retorno de NF-e.
 *
 * Porta o comportamento de `vendas/emissao/devolucao-retorno/nfe/[[id]].vue` do legado.
 * Reutiliza integralmente o motor de emissão da fatia 7 (`useNfeEmissao`, READ-ONLY) —
 * dados básicos, destinatário, produtos, totais, transporte, recebimentos, transmissão em
 * tempo real e DANFE — e adiciona apenas o que é específico da devolução/retorno:
 *
 *   1. Finalidade da NF-e = "Devolução/Retorno" (código 4 da SEFAZ), fixa nesta tela.
 *   2. Chaves de NF-e referenciadas (a(s) nota(s) de origem que estão sendo devolvidas),
 *      persistidas em `vendas-fiscal/{id}/nfe/referenciadas`.
 *   3. Natureza da operação padrão "Devolução de mercadoria".
 *
 * O estado de origem (venda devolvida) pode chegar por state global (fluxo a partir do
 * monitor de transmissões) ou ser preenchido manualmente na própria tela.
 *
 * IO exclusivamente pelo cliente compartilhado `useApi` (padrão CommandResult).
 * Endpoints: `vendas`, `vendas-fiscal/{id}/nfe(+transmitir)`, `.../nfe/referenciadas`.
 */
import { computed, ref, watch } from 'vue'
import { useNfeEmissao } from '~/components/vendas-nfe/useNfeEmissao'
import { useMask } from '~/composables/useMask'
import { useToast } from '~/composables/useToast'

/** Finalidade da emissão da NF-e (códigos SEFAZ). */
export const FINALIDADES_NFE: { label: string; value: number }[] = [
  { label: '1 - NF-e normal', value: 1 },
  { label: '2 - NF-e complementar', value: 2 },
  { label: '3 - NF-e de ajuste', value: 3 },
  { label: '4 - Devolução/Retorno', value: 4 }
]

/** Código de finalidade "Devolução/Retorno". */
export const FINALIDADE_DEVOLUCAO = 4

/** Natureza de operação sugerida para devolução/retorno. */
const NATUREZA_PADRAO = 'Devolução de mercadoria'

export function useDevolucaoRetorno() {
  // Reaproveita todo o motor de emissão de NF-e da fatia 7 (contrato read-only).
  const emissao = useNfeEmissao()
  const { somenteDigitos } = useMask()
  const toast = useToast()

  // Finalidade fixa em devolução/retorno nesta tela.
  const finalidade = ref<number>(FINALIDADE_DEVOLUCAO)

  // Chave(s) da(s) nota(s) de origem sendo devolvida(s).
  const chavesOrigem = ref<string[]>([])

  const chaveValida = (v: string) => somenteDigitos(v).length === 44

  /** Adiciona uma chave de NF-e de origem (44 dígitos). */
  function adicionarChaveOrigem(chave: string): boolean {
    const limpa = somenteDigitos(chave)
    if (!chaveValida(limpa)) {
      toast.warning('A chave da NF-e deve ter 44 dígitos')
      return false
    }
    if (chavesOrigem.value.includes(limpa)) {
      toast.warning('Chave já adicionada')
      return false
    }
    chavesOrigem.value = [...chavesOrigem.value, limpa]
    sincronizarReferenciadas()
    return true
  }

  /** Remove uma chave de origem pelo índice. */
  function removerChaveOrigem(index: number): void {
    const nova = [...chavesOrigem.value]
    nova.splice(index, 1)
    chavesOrigem.value = nova
    sincronizarReferenciadas()
  }

  /** Reflete as chaves de origem nas chaves referenciadas do formulário de NF-e. */
  function sincronizarReferenciadas(): void {
    // As notas de origem são, por definição, as referenciadas da devolução.
    emissao.nfe.chavesReferenciadas = [...chavesOrigem.value]
  }

  // Garante a natureza padrão quando ainda não informada.
  watch(
    () => emissao.nfe.naturezaOperacao,
    (v) => {
      if (!v || !v.trim()) emissao.nfe.naturezaOperacao = NATUREZA_PADRAO
    },
    { immediate: true }
  )

  const titulo = computed(() =>
    emissao.nfe.id
      ? `Devolução/Retorno de NF-e ${emissao.nfe.numero ? `#${emissao.nfe.numero}` : ''}`.trim()
      : 'Nova Devolução/Retorno de NF-e'
  )

  /**
   * Pré-carrega o formulário a partir de uma venda de origem (ex.: iniciado pelo monitor
   * de transmissões). Aproveita `carregarVenda` para trazer destinatário/itens e marca a
   * chave da nota de origem como referenciada.
   */
  async function iniciarDeOrigem(vendaOrigemId: string, chaveOrigem?: string | null): Promise<void> {
    await emissao.carregarVenda(vendaOrigemId)
    // A devolução é uma NOVA nota: descarta o id/emissão da origem, preservando itens/destino.
    emissao.nfe.id = null
    emissao.nfe.numero = null
    emissao.nfe.serie = null
    emissao.nfe.chave = null
    emissao.nfe.situacao = null
    emissao.nfe.naturezaOperacao = NATUREZA_PADRAO
    if (chaveOrigem) {
      chavesOrigem.value = []
      adicionarChaveOrigem(chaveOrigem)
    }
  }

  /** Persiste as chaves referenciadas (após salvar o rascunho, quando já há id). */
  async function salvarReferenciadas(): Promise<void> {
    await emissao.salvarReferenciadas(chavesOrigem.value)
  }

  return {
    // motor reaproveitado (read-only) — repassado para a página
    ...emissao,
    // específicos da devolução/retorno
    finalidade,
    finalidades: FINALIDADES_NFE,
    chavesOrigem,
    titulo,
    adicionarChaveOrigem,
    removerChaveOrigem,
    sincronizarReferenciadas,
    iniciarDeOrigem,
    salvarReferenciadas
  }
}
