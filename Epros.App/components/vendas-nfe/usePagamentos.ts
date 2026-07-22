/**
 * usePagamentos — geração de parcelas/duplicatas e pagamentos da NF-e.
 *
 * Porta `composables/vendas/usePagamentos.ts` do legado: três estratégias de geração de
 * parcelas (dias fixos "0 15 30", intervalo em dias, dia fixo do mês), troco automático
 * quando a modalidade é dinheiro e o valor informado excede o restante, e o agrupamento
 * final das parcelas por modalidade de pagamento (`confirmarPagamentos`).
 *
 * Adaptação de plataforma: o legado usava `date-fns` (`addDays`/`setDate`/`addMonths`) e
 * tipos gerados de OpenAPI (`Fatura`/`Pagamento`/`Duplicata`). Aqui usamos `Date` nativo
 * (sem trazer nova dependência) e os tipos locais de `nfeTypes.ts` (`NfeDuplicata`/
 * `NfePagamento`). O restante da lógica (fórmulas de rateio de diferença de arredondamento
 * na primeira parcela, cálculo de troco) é fiel ao original.
 */
import { computed, ref, watch, type ComputedRef, type Ref } from 'vue'
import type { NfeDuplicata, NfePagamento } from './nfeTypes'
import { criarDuplicataVazia } from './nfeTypes'

export type TipoParcelamento = 'fixed' | 'interval' | 'short'

/** Parcela em edição (antes de virar `NfeDuplicata` + `NfePagamento` confirmados). */
export interface ParcelaGerada {
  numeroDuplicata: string
  dataVencimento: string
  valorDuplicata: number
  modalidadePagamento: number
  troco: number
}

export interface UsePagamentosOptions {
  /** Duplicatas confirmadas da NF-e (saída — ligar a `nfe.duplicatas`). */
  duplicatas: Ref<NfeDuplicata[]>
  /** Pagamentos confirmados da NF-e (saída — ligar a `nfe.pagamentos`). */
  pagamentos: Ref<NfePagamento[]>
  /** Valor total a receber (produtos - desconto + frete/seguro/outro + impostos). */
  totalRecebimento: ComputedRef<number> | Ref<number>
}

export function usePagamentos(options: UsePagamentosOptions) {
  const { duplicatas, pagamentos, totalRecebimento } = options

  const tipoParcelamento = ref<TipoParcelamento>('interval')
  const diaFixo = ref(20)
  const intervaloDias = ref(30)
  const numeroParcelas = ref(1)
  const textoParcelasCurtas = ref('')
  const parcelasGeradas = ref<ParcelaGerada[]>([])
  const valor = ref(0)
  const troco = ref(0)
  const modalidadePadrao = ref(1) // Dinheiro

  const temPagamentos = computed(
    () => parcelasGeradas.value.length > 0 || duplicatas.value.length > 0
  )

  const totalParcelado = computed(() =>
    parcelasGeradas.value.reduce((acc, p) => acc + (p.valorDuplicata - (p.troco || 0)), 0)
  )

  const valorRestante = computed(() => totalRecebimento.value - totalParcelado.value)

  const isDinheiro = computed(() => modalidadePadrao.value === 1)

  /** Parcelável = qualquer modalidade exceto dinheiro/sem-pagamento (regra simplificada e fiel ao uso do legado: só dinheiro não permite mais de 1 parcela). */
  function ehParcelavel(modalidade: number): boolean {
    return modalidade !== 1
  }

  watch(
    valorRestante,
    (restante) => {
      if (restante > 0 && valor.value === 0) {
        valor.value = restante
      } else if (parcelasGeradas.value.length > 0) {
        valor.value = restante > 0 ? restante : 0
      }
    },
    { immediate: true }
  )

  watch(totalRecebimento, (novoValor) => {
    if (novoValor > 0 && parcelasGeradas.value.length === 0) {
      valor.value = novoValor
    }
  })

  watch(modalidadePadrao, (nova) => {
    if (nova !== 1) troco.value = 0
  })

  function calcularTrocoAutomatico(): void {
    if (isDinheiro.value && valor.value > 0 && valorRestante.value > 0) {
      troco.value = valor.value > valorRestante.value ? arred2(valor.value - valorRestante.value) : 0
    }
  }

  watch(valor, calcularTrocoAutomatico)
  watch(valorRestante, calcularTrocoAutomatico)

  /** Redistribui a diferença de arredondamento entre as demais parcelas quando a 1ª muda. */
  watch(
    () => parcelasGeradas.value[0]?.valorDuplicata,
    (novoValor, valorAntigo) => {
      if (
        parcelasGeradas.value.length > 1 &&
        novoValor !== undefined &&
        valorAntigo !== undefined &&
        novoValor !== valorAntigo
      ) {
        const somaDemais = parcelasGeradas.value
          .slice(1)
          .reduce((acc, p) => acc + (p.valorDuplicata || 0), 0)
        const totalOriginal = valorAntigo + somaDemais
        const restanteDemais = totalOriginal - novoValor
        const numeroDemais = parcelasGeradas.value.length - 1
        if (restanteDemais > 0 && numeroDemais > 0) {
          const valorPorParcela = arred2(restanteDemais / numeroDemais)
          const diferenca = arred2(restanteDemais - valorPorParcela * numeroDemais)
          for (let i = 1; i < parcelasGeradas.value.length; i++) {
            const parcela = parcelasGeradas.value[i]
            if (parcela) {
              parcela.valorDuplicata = i === 1 ? arred2(valorPorParcela + diferenca) : valorPorParcela
            }
          }
        }
      }
    }
  )

  function calcularTrocoParaGeracao(): number {
    if (isDinheiro.value && valor.value > valorRestante.value) {
      return arred2(valor.value - valorRestante.value)
    }
    return 0
  }

  /** Gera parcelas com dias explícitos, ex.: "0 15 30". */
  function gerarParcelasCurtas(mensagemErro: (m: string) => void): boolean {
    if (!textoParcelasCurtas.value.trim()) {
      mensagemErro('Informe os dias das parcelas. Ex: 0 15 30')
      return false
    }
    const dias = textoParcelasCurtas.value
      .trim()
      .split(/\s+/)
      .map((d) => parseInt(d, 10))
      .filter((d) => !Number.isNaN(d))
    if (dias.length === 0) {
      mensagemErro('Formato inválido. Informe os dias separados por espaço. Ex: 0 15 30')
      return false
    }

    duplicatas.value = []
    pagamentos.value = []
    parcelasGeradas.value = []

    const valorPorParcela = arred2(valor.value / dias.length)
    const diferenca = arred2(valor.value - valorPorParcela * dias.length)
    const trocoCalculado = calcularTrocoParaGeracao()

    dias.forEach((dia, index) => {
      const dataVencimento = new Date()
      dataVencimento.setDate(dataVencimento.getDate() + dia)
      const valorParcela = index === 0 ? arred2(valorPorParcela + diferenca) : valorPorParcela
      parcelasGeradas.value.push({
        numeroDuplicata: String(parcelasGeradas.value.length + 1).padStart(3, '0'),
        dataVencimento: dataVencimento.toISOString(),
        valorDuplicata: valorParcela,
        modalidadePagamento: modalidadePadrao.value,
        troco: trocoCalculado
      })
    })
    return true
  }

  /** Gera parcelas em intervalo fixo de dias entre elas. */
  function gerarParcelasIntervalo(): boolean {
    duplicatas.value = []
    pagamentos.value = []
    parcelasGeradas.value = []

    const valorParcela = arred2(valor.value / numeroParcelas.value)
    const diferenca = arred2(valor.value - valorParcela * numeroParcelas.value)
    const trocoCalculado = calcularTrocoParaGeracao()
    const dataAtual = new Date()

    for (let i = 1; i <= numeroParcelas.value; i++) {
      const vencimento = new Date(dataAtual)
      vencimento.setDate(vencimento.getDate() + intervaloDias.value * i)
      const valorAtual = i === 1 ? arred2(valorParcela + diferenca) : valorParcela
      parcelasGeradas.value.push({
        numeroDuplicata: String(parcelasGeradas.value.length + 1).padStart(3, '0'),
        dataVencimento: vencimento.toISOString(),
        valorDuplicata: valorAtual,
        modalidadePagamento: modalidadePadrao.value,
        troco: trocoCalculado
      })
    }
    return true
  }

  /** Gera parcelas em dia fixo do mês (ex.: dia 20 de cada mês). */
  function gerarParcelasDiaFixo(): boolean {
    duplicatas.value = []
    pagamentos.value = []
    parcelasGeradas.value = []

    const valorParcela = arred2(valor.value / numeroParcelas.value)
    const diferenca = arred2(valor.value - valorParcela * numeroParcelas.value)
    const trocoCalculado = calcularTrocoParaGeracao()
    const dataAtual = new Date()

    for (let i = 1; i <= numeroParcelas.value; i++) {
      let alvo = setDia(dataAtual, diaFixo.value)
      if (i === 1 && alvo <= dataAtual) {
        alvo = somarMeses(alvo, 1)
      } else if (i > 1) {
        alvo = somarMeses(setDia(dataAtual, diaFixo.value), i - 1)
      }
      const valorAtual = i === 1 ? arred2(valorParcela + diferenca) : valorParcela
      parcelasGeradas.value.push({
        numeroDuplicata: String(parcelasGeradas.value.length + 1).padStart(3, '0'),
        dataVencimento: alvo.toISOString(),
        valorDuplicata: valorAtual,
        modalidadePagamento: modalidadePadrao.value,
        troco: trocoCalculado
      })
    }
    return true
  }

  /** Gera parcelas de acordo com `tipoParcelamento`, validando valor/quantidade antes. */
  function gerarParcelas(mensagemErro: (m: string) => void): boolean {
    if (tipoParcelamento.value !== 'short' && (numeroParcelas.value <= 0 || valor.value <= 0)) {
      mensagemErro('Por favor, verifique o número de parcelas e o valor.')
      return false
    }
    if (parcelasGeradas.value.some((p) => ehParcelavel(p.modalidadePagamento))) {
      mensagemErro('Só pode selecionar uma forma de parcelamento.')
      return false
    }
    if (valor.value <= 0) {
      mensagemErro('Valor deve ser maior que zero.')
      return false
    }
    if (valor.value > totalRecebimento.value) {
      mensagemErro('Valor a parcelar não pode ser maior que o total da NFe.')
      return false
    }

    let sucesso = false
    switch (tipoParcelamento.value) {
      case 'short':
        sucesso = gerarParcelasCurtas(mensagemErro)
        break
      case 'interval':
        sucesso = gerarParcelasIntervalo()
        break
      case 'fixed':
        sucesso = gerarParcelasDiaFixo()
        break
    }
    if (sucesso) {
      valor.value = valorRestante.value > 0 ? valorRestante.value : 0
    }
    return sucesso
  }

  /** Adiciona uma parcela avulsa com o valor atual do campo `valor`. */
  function adicionarParcela(mensagemErro: (m: string) => void): void {
    if (valor.value <= 0) {
      mensagemErro('Valor deve ser maior que zero.')
      return
    }
    const trocoCalculado = calcularTrocoParaGeracao()
    parcelasGeradas.value.push({
      numeroDuplicata: String(parcelasGeradas.value.length + 1).padStart(3, '0'),
      dataVencimento: new Date().toISOString(),
      valorDuplicata: valor.value,
      modalidadePagamento: modalidadePadrao.value,
      troco: trocoCalculado
    })
    valor.value = valorRestante.value > 0 ? valorRestante.value : 0
    troco.value = 0
  }

  function removerParcela(index: number): void {
    parcelasGeradas.value.splice(index, 1)
    parcelasGeradas.value.forEach((p, i) => {
      p.numeroDuplicata = String(i + 1).padStart(3, '0')
    })
    numeroParcelas.value = parcelasGeradas.value.length || 1
    valor.value = valorRestante.value > 0 ? valorRestante.value : 0
  }

  /** Converte as parcelas geradas em `duplicatas`/`pagamentos` confirmados da NF-e. */
  function confirmarPagamentos(): void {
    if (parcelasGeradas.value.length === 0) return

    const parcelavel = ehParcelavel(parcelasGeradas.value[0]?.modalidadePagamento ?? 0)

    if (parcelavel) {
      duplicatas.value = parcelasGeradas.value.map((p) => ({
        _uid: `d-${Math.random().toString(36).slice(2, 10)}`,
        numero: p.numeroDuplicata,
        dataVencimento: p.dataVencimento,
        valor: p.valorDuplicata
      }))
    } else {
      duplicatas.value = []
    }

    const agrupados = new Map<number, NfePagamento>()
    for (const parcela of parcelasGeradas.value) {
      const existente = agrupados.get(parcela.modalidadePagamento)
      if (existente) {
        existente.valorPagamento = arred2(existente.valorPagamento + parcela.valorDuplicata)
        existente.valorTroco = arred2(existente.valorTroco + (parcela.troco || 0))
      } else {
        agrupados.set(parcela.modalidadePagamento, {
          _uid: `p-${Math.random().toString(36).slice(2, 10)}`,
          tipoPagamento: parcela.modalidadePagamento,
          valorPagamento: parcela.valorDuplicata,
          valorTroco: parcela.troco || 0
        })
      }
    }
    pagamentos.value = Array.from(agrupados.values())
  }

  function limparPagamentos(): void {
    duplicatas.value = []
    pagamentos.value = []
    parcelasGeradas.value = []
    valor.value = totalRecebimento.value
    troco.value = 0
    modalidadePadrao.value = 1
    tipoParcelamento.value = 'interval'
    numeroParcelas.value = 1
    textoParcelasCurtas.value = ''
  }

  /** Carrega parcelas/pagamentos a partir de uma venda existente (edição). */
  function carregarDeNfe(dados: { duplicatas?: NfeDuplicata[]; pagamentos?: NfePagamento[] }): void {
    if (dados.duplicatas && dados.duplicatas.length > 0) {
      duplicatas.value = [...dados.duplicatas]
      parcelasGeradas.value = dados.duplicatas.map((d, index) => {
        const pagamento = dados.pagamentos?.[index] ?? dados.pagamentos?.[0]
        return {
          numeroDuplicata: d.numero,
          dataVencimento: d.dataVencimento,
          valorDuplicata: d.valor,
          troco: pagamento?.valorTroco ?? 0,
          modalidadePagamento: pagamento?.tipoPagamento ?? 1
        }
      })
      const primeira = parcelasGeradas.value[0]
      if (primeira) modalidadePadrao.value = primeira.modalidadePagamento
    }

    if (dados.pagamentos && dados.pagamentos.length > 0) {
      pagamentos.value = [...dados.pagamentos]
      if (!dados.duplicatas?.length) {
        parcelasGeradas.value = dados.pagamentos.map((p, index) => ({
          numeroDuplicata: String(index + 1).padStart(3, '0'),
          dataVencimento: new Date().toISOString(),
          valorDuplicata: p.valorPagamento,
          troco: p.valorTroco,
          modalidadePagamento: p.tipoPagamento
        }))
        const primeira = parcelasGeradas.value[0]
        if (primeira) modalidadePadrao.value = primeira.modalidadePagamento
      }
    }
  }

  return {
    // configuração
    tipoParcelamento,
    diaFixo,
    intervaloDias,
    numeroParcelas,
    textoParcelasCurtas,
    // estado
    parcelasGeradas,
    valor,
    troco,
    modalidadePadrao,
    // derivados
    temPagamentos,
    totalParcelado,
    valorRestante,
    isDinheiro,
    ehParcelavel,
    // ações
    gerarParcelas,
    adicionarParcela,
    removerParcela,
    confirmarPagamentos,
    limparPagamentos,
    carregarDeNfe
  }
}

/** Cria uma duplicata vazia (reexportado para telas que montam a lista manualmente). */
export { criarDuplicataVazia }

function setDia(data: Date, dia: number): Date {
  const nova = new Date(data)
  nova.setDate(dia)
  return nova
}

function somarMeses(data: Date, meses: number): Date {
  const nova = new Date(data)
  nova.setMonth(nova.getMonth() + meses)
  return nova
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred2(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}
