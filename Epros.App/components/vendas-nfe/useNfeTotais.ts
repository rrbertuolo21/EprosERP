/**
 * useNfeTotais — estado e cálculo dos totais da NF-e em edição.
 *
 * Porta `composables/vendas/useNfeTotais.ts` do legado: valores manuais de frete/seguro/
 * acréscimo/desconto, flags de "embutir" cada um deles na base de cálculo do ICMS/IPI, e a
 * flag `cobrarFrete`. No legado essas mudanças eram debounced e enviadas ao hub SignalR
 * (`DefinirValoresVenda`/`DefinirFrete`/`DefinirValoresEmbutir`) que recalculava tudo no
 * servidor. Como o backend novo é REST, aqui o cálculo é feito no cliente (`calcularTotais`)
 * a partir dos itens (`NfeItem[]`) + os ajustes manuais — mesma fórmula de `useNfeEmissao`,
 * extraída para reuso isolado e testável.
 */
import { computed, ref, type Ref } from 'vue'
import type { NfeItem, NfeTotais } from './nfeTypes'

export interface UseNfeTotaisOptions {
  /** Itens atuais da NF-e (lista viva, ex.: `produtos` de `useNfeProdutos`). */
  itens: Ref<NfeItem[]>
  /** Valores manuais iniciais (retomados ao carregar uma venda existente). */
  valoresIniciais?: Partial<{
    descontoManual: number
    freteManual: number
    seguroManual: number
    outroManual: number
  }>
}

export function useNfeTotais(options: UseNfeTotaisOptions) {
  const { itens, valoresIniciais } = options

  const descontoManual = ref(valoresIniciais?.descontoManual ?? 0)
  const freteManual = ref(valoresIniciais?.freteManual ?? 0)
  const seguroManual = ref(valoresIniciais?.seguroManual ?? 0)
  const outroManual = ref(valoresIniciais?.outroManual ?? 0)

  /** Modalidade de frete (0-9, mesmos códigos SEFAZ — ver `MODALIDADES_FRETE`). */
  const modalidadeFrete = ref(9)
  /** Se o frete deve ser cobrado do destinatário (soma no total da nota). */
  const cobrarFrete = ref(false)

  // Flags de "embutir" — quando true, o valor correspondente entra na base de cálculo
  // do ICMS/IPI dos itens (rateio); quando false, é só informativo/complementar.
  const embuteFrete = ref(true)
  const embuteSeguro = ref(true)
  const embuteAcrescimo = ref(true)
  const embuteOutro = ref(true)

  const valorTotalItens = computed(() =>
    arred(itens.value.reduce((acc, item) => acc + ((item.quantidade || 0) * (item.valorUnitario || 0)), 0))
  )

  const valorTotalDescontoItens = computed(() =>
    arred(itens.value.reduce((acc, item) => acc + (item.descontoValor || 0), 0))
  )

  const valorTotalIcms = computed(() =>
    arred(itens.value.reduce((acc, item) => {
      const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
      // Arredonda o item a 4 casas antes de agregar (paridade com o legado, gap L7).
      const liquido = Math.max(0, arred4(bruto - (item.descontoValor || 0)))
      return acc + liquido * ((item.aliquotaIcms || 0) / 100)
    }, 0))
  )

  const valorTotalIpi = computed(() =>
    arred(itens.value.reduce((acc, item) => {
      const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
      const liquido = Math.max(0, arred4(bruto - (item.descontoValor || 0)))
      return acc + liquido * ((item.aliquotaIpi || 0) / 100)
    }, 0))
  )

  /** Totais consolidados da NF-e, no formato consumido por `NfeForm`/tela de emissão. */
  const totais = computed<NfeTotais>(() => {
    const valorProduto = valorTotalItens.value
    const valorDesconto = arred(valorTotalDescontoItens.value + (descontoManual.value || 0))
    const valorFrete = arred(freteManual.value || 0)
    const valorSeguro = arred(seguroManual.value || 0)
    const valorOutro = arred(outroManual.value || 0)
    const valorIcms = valorTotalIcms.value
    const valorIpi = valorTotalIpi.value
    const valorNotaFiscal = Math.max(
      0,
      arred(valorProduto - valorDesconto + valorFrete + valorSeguro + valorOutro + valorIpi)
    )
    return {
      valorProduto,
      valorDesconto,
      valorFrete,
      valorSeguro,
      valorOutro,
      valorIcms,
      valorIpi,
      valorNotaFiscal,
      valorRecebimento: 0 // preenchido por quem consome (soma dos pagamentos — ver useNfeEmissao)
    }
  })

  /** Recarrega os valores manuais (ex.: ao abrir uma venda existente para edição). */
  function definirValoresManuais(valores: {
    descontoManual?: number
    freteManual?: number
    seguroManual?: number
    outroManual?: number
  }): void {
    if (valores.descontoManual !== undefined) descontoManual.value = valores.descontoManual
    if (valores.freteManual !== undefined) freteManual.value = valores.freteManual
    if (valores.seguroManual !== undefined) seguroManual.value = valores.seguroManual
    if (valores.outroManual !== undefined) outroManual.value = valores.outroManual
  }

  function resetar(): void {
    descontoManual.value = 0
    freteManual.value = 0
    seguroManual.value = 0
    outroManual.value = 0
    modalidadeFrete.value = 9
    cobrarFrete.value = false
    embuteFrete.value = true
    embuteSeguro.value = true
    embuteAcrescimo.value = true
    embuteOutro.value = true
  }

  return {
    // ajustes manuais
    descontoManual,
    freteManual,
    seguroManual,
    outroManual,
    // frete
    modalidadeFrete,
    cobrarFrete,
    // embutir
    embuteFrete,
    embuteSeguro,
    embuteAcrescimo,
    embuteOutro,
    // derivados
    valorTotalItens,
    valorTotalDescontoItens,
    valorTotalIcms,
    valorTotalIpi,
    totais,
    // ações
    definirValoresManuais,
    resetar
  }
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

/**
 * Arredonda para 4 casas decimais (gap L7 — paridade com o legado
 * `useNfeProdutos.ts:157`, que usa 4 casas no item antes de somar os totais).
 */
function arred4(v: number): number {
  return Math.round((v + Number.EPSILON) * 10000) / 10000
}
