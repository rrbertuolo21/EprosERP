/**
 * useNfeTransmissao — fluxo de gravação e transmissão da NF-e à SEFAZ.
 *
 * Porta `composables/vendas/useNfeTransmissao.ts` do legado: valida os dados obrigatórios
 * (datas, produtos, destinatário), garante uma forma de pagamento padrão (Dinheiro à vista)
 * quando a nota não tem pagamento algum lançado, grava a venda (POST/PUT conforme já exista
 * id) e, se o status pedido for "salvar e transmitir", encadeia a chamada de transmissão à
 * SEFAZ e o download do DANFE.
 *
 * Adaptação de plataforma: o legado orquestrava esse fluxo via hub SignalR
 * (`ObterVendaCompletaParaNfe`) que calculava tudo no servidor e devolvia por eventos
 * assíncronos. O backend novo expõe REST direto: aqui chamamos `useApi` (padrão
 * `CommandResult`) na rota `vendas-fiscal/{id}/nfe(+transmitir)`, mesma usada por
 * `useNfeEmissao`. Este composable fica mais enxuto que `useNfeEmissao.transmitir`: ele isola
 * as regras de negócio (validação, pagamento padrão) para reuso/teste, delegando a
 * persistência ao callback `gravar`/`transmitirNaApi` injetado por quem o instancia.
 */
import { computed, ref, type Ref } from 'vue'
import type { NfeDuplicata, NfeForm, NfeItem, NfePagamento } from './nfeTypes'
import { criarPagamentoVazio } from './nfeTypes'

export interface UseNfeTransmissaoOptions {
  nfe: Ref<NfeForm> | NfeForm
  itens: Ref<NfeItem[]>
  /** Valor total a receber (após desconto/frete) — normalmente `totais.value.valorNotaFiscal`. */
  valorTotalVenda: Ref<number> | (() => number)
  /** Persiste o rascunho (POST/PUT). Retorna o id da venda gravada, ou null em falha. */
  gravar: () => Promise<string | null>
  /** Chama a transmissão à SEFAZ para o id informado. */
  transmitirNaApi: (vendaId: string) => Promise<boolean>
}

/** Etapas exibidas no overlay de transmissão (mesmas de `useNfeEmissao.PASSOS_TRANSMISSAO`). */
export const PASSOS_TRANSMISSAO_NFE = [
  'Validando dados da nota fiscal...',
  'Transmitindo para a SEFAZ...',
  'Gerando contas a receber...',
  'Baixando estoque...',
  'Processando pagamentos...',
  'Finalizando operação...'
]

export function useNfeTransmissao(options: UseNfeTransmissaoOptions) {
  const { nfe, itens, valorTotalVenda, gravar, transmitirNaApi } = options

  const transmitindo = ref(false)
  const erroValidacao = ref<string | null>(null)
  const overlayPasso = ref(0)

  function obterNfe(): NfeForm {
    return 'value' in nfe ? nfe.value : nfe
  }

  function obterValorTotal(): number {
    return typeof valorTotalVenda === 'function' ? valorTotalVenda() : valorTotalVenda.value
  }

  const podeTransmitir = computed(
    () => obterNfe().situacao !== 'Autorizada' && obterNfe().situacao !== 'Cancelada'
  )

  /** Valida datas obrigatórias (emissão) — mesma regra do legado `validarDatas`. */
  function validarDatas(): boolean {
    const form = obterNfe()
    if (!form.dataEmissao) {
      erroValidacao.value = 'A data de emissão é obrigatória!'
      return false
    }
    return true
  }

  /** Valida presença de destinatário — mesma regra do legado `transmitirNFe`. */
  function validarDestinatario(): boolean {
    const form = obterNfe()
    if (!form.destinatario.pessoaId) {
      erroValidacao.value = 'É necessário selecionar um destinatário para transmitir a NFe'
      return false
    }
    return true
  }

  /**
   * Garante uma forma de pagamento padrão quando a nota não tem nenhuma lançada e há valor a
   * receber — porta o fallback "pagamento em dinheiro" do legado (que pedia confirmação ao
   * usuário antes; aqui a decisão de confirmar fica a cargo de quem chama, via `confirmar`).
   */
  async function garantirPagamentoPadrao(
    confirmar: (mensagem: string) => Promise<boolean>
  ): Promise<boolean> {
    const form = obterNfe()
    const valorRecebimento = obterValorTotal()
    if (form.pagamentos.length > 0 || valorRecebimento <= 0) return true

    const aceitou = await confirmar(
      'Ao continuar, será assumido pagamento à vista (Dinheiro) conforme padrão. Deseja prosseguir?'
    )
    if (!aceitou) return false

    const pagamentoPadrao: NfePagamento = {
      ...criarPagamentoVazio(),
      tipoPagamento: 1, // Dinheiro
      valorPagamento: valorRecebimento,
      valorTroco: 0
    }
    form.pagamentos = [pagamentoPadrao]

    if (form.duplicatas.length === 0) {
      const duplicataUnica: NfeDuplicata = {
        _uid: `d-${Math.random().toString(36).slice(2, 10)}`,
        numero: '001',
        dataVencimento: form.dataEmissao,
        valor: valorRecebimento
      }
      form.duplicatas = [duplicataUnica]
    }
    return true
  }

  /**
   * Executa o fluxo completo: valida → garante pagamento padrão → grava rascunho →
   * (se `transmitirAposGravar`) transmite à SEFAZ. Retorna o id da venda gravada.
   */
  async function processar(options: {
    transmitirAposGravar: boolean
    confirmarPagamentoPadrao: (mensagem: string) => Promise<boolean>
  }): Promise<{ ok: boolean; vendaId: string | null }> {
    erroValidacao.value = null

    if (!validarDatas() || !validarDestinatario()) {
      return { ok: false, vendaId: null }
    }
    if (itens.value.length === 0) {
      erroValidacao.value = 'Adicione ao menos um produto antes de transmitir a nota fiscal'
      return { ok: false, vendaId: null }
    }

    const pagamentoOk = await garantirPagamentoPadrao(options.confirmarPagamentoPadrao)
    if (!pagamentoOk) return { ok: false, vendaId: null }

    transmitindo.value = true
    overlayPasso.value = 0
    try {
      const vendaId = await gravar()
      if (!vendaId) return { ok: false, vendaId: null }

      if (!options.transmitirAposGravar) {
        return { ok: true, vendaId }
      }

      overlayPasso.value = 1
      const transmitiu = await transmitirNaApi(vendaId)
      overlayPasso.value = PASSOS_TRANSMISSAO_NFE.length - 1
      return { ok: transmitiu, vendaId }
    } finally {
      transmitindo.value = false
    }
  }

  return {
    transmitindo,
    erroValidacao,
    overlayPasso,
    podeTransmitir,
    passosTransmissao: PASSOS_TRANSMISSAO_NFE,
    validarDatas,
    validarDestinatario,
    garantirPagamentoPadrao,
    processar
  }
}
