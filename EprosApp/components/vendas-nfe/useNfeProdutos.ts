/**
 * useNfeProdutos — gestão da lista de produtos (itens) da NF-e em edição.
 *
 * Porta o comportamento de `composables/vendas/useNfeProdutos.ts` do legado: adicionar,
 * editar e remover linhas de produto, com confirmação antes de excluir e recálculo do total
 * da linha. No legado, cada mutação era espelhada em tempo real para o hub SignalR
 * (`AdicionarProduto`/`AtualizarProduto`/`RemoverProduto`) que fazia o cálculo tributário
 * server-side. Como o backend novo é REST puro, aqui a lista fica só em estado local — o
 * cálculo de totais é feito por `useNfeTotais`/`calcularTotais` (client-side) e a persistência
 * definitiva ocorre no `salvarRascunho`/`transmitir` de `useNfeEmissao`.
 *
 * Efeito colateral portado do legado: ao adicionar o primeiro item, se a natureza de operação
 * da NF-e ainda estiver vazia, herda a `naturezaOperacao` do produto/CFOP selecionado; ao
 * remover o último item, a natureza volta a ficar vazia.
 */
import { ref, type Ref } from 'vue'
import type { NfeForm, NfeItem } from './nfeTypes'
import { criarItemVazio, gerarUid } from './nfeTypes'

export interface UseNfeProdutosOptions {
  /** Formulário reativo da NF-e (para sincronizar `naturezaOperacao`). */
  nfe?: Ref<NfeForm> | NfeForm
  /** Confirmação antes de excluir um item (padrão: `window.confirm`). */
  confirmarExclusao?: (mensagem: string) => Promise<boolean>
}

export function useNfeProdutos(options: UseNfeProdutosOptions = {}) {
  const { nfe, confirmarExclusao } = options

  const produtos = ref<NfeItem[]>([])
  const produtoSelecionado = ref<NfeItem | null>(null)
  const produtoIndex = ref<number | null>(null)

  function obterNfe(): NfeForm | undefined {
    if (!nfe) return undefined
    return 'value' in nfe ? nfe.value : nfe
  }

  /** Recalcula o total da linha (quantidade * unitário - desconto), sem permitir negativo. */
  function recalcularTotalItem(item: NfeItem): number {
    const bruto = (item.quantidade || 0) * (item.valorUnitario || 0)
    const liquido = bruto - (item.descontoValor || 0)
    return Math.max(0, arred(liquido))
  }

  function sincronizarNaturezaOperacaoAoAdicionar(naturezaOperacaoProduto?: string | null): void {
    const form = obterNfe()
    if (produtos.value.length === 1 && naturezaOperacaoProduto && form && !form.naturezaOperacao) {
      form.naturezaOperacao = naturezaOperacaoProduto
    }
  }

  function sincronizarNaturezaOperacaoAoEsvaziar(): void {
    const form = obterNfe()
    if (produtos.value.length === 0 && form) {
      form.naturezaOperacao = ''
    }
  }

  /** Adiciona um item novo à lista (recalcula total antes de inserir). */
  function adicionarProduto(item: NfeItem, naturezaOperacaoProduto?: string | null): NfeItem {
    const novo: NfeItem = { ...item, _uid: item._uid || gerarUid() }
    novo.total = recalcularTotalItem(novo)
    produtos.value.push(novo)
    sincronizarNaturezaOperacaoAoAdicionar(naturezaOperacaoProduto)
    return novo
  }

  /** Substitui o item no índice informado (edição de linha existente). */
  function editarProduto(index: number, item: NfeItem): void {
    if (index < 0 || index >= produtos.value.length) return
    produtoIndex.value = index
    const atualizado: NfeItem = { ...item }
    atualizado.total = recalcularTotalItem(atualizado)
    produtos.value.splice(index, 1, atualizado)
  }

  /** Remove o item no índice informado, pedindo confirmação (padrão do legado). */
  async function removerProduto(index: number): Promise<boolean> {
    if (index < 0 || index >= produtos.value.length) return false
    const confirmar = confirmarExclusao ?? confirmarExclusaoPadrao
    const ok = await confirmar('Tem certeza que deseja excluir o item venda?')
    if (!ok) return false
    produtos.value.splice(index, 1)
    sincronizarNaturezaOperacaoAoEsvaziar()
    return true
  }

  /** Remove todos os itens da lista (limpa também a natureza de operação). */
  function removerTodosProdutos(): void {
    produtos.value = []
    sincronizarNaturezaOperacaoAoEsvaziar()
  }

  /** Valida se há pelo menos um item e se todos têm produto/CFOP preenchidos. */
  function validarProdutos(): { valido: boolean; mensagem?: string } {
    if (produtos.value.length === 0) {
      return { valido: false, mensagem: 'Adicione ao menos um produto antes de transmitir a nota fiscal' }
    }
    for (const item of produtos.value) {
      if (!item.produtoId && !item.nomeProduto.trim()) {
        return { valido: false, mensagem: 'Há produtos incompletos na lista' }
      }
      if (!item.cfop) {
        return { valido: false, mensagem: 'Informe o CFOP de todos os produtos' }
      }
    }
    return { valido: true }
  }

  function selecionarParaEdicao(index: number): void {
    const item = produtos.value[index]
    if (!item) return
    produtoIndex.value = index
    produtoSelecionado.value = { ...item }
  }

  function limparSelecao(): void {
    produtoIndex.value = null
    produtoSelecionado.value = null
  }

  function novoProdutoVazio(): NfeItem {
    return criarItemVazio()
  }

  return {
    produtos,
    produtoSelecionado,
    produtoIndex,
    recalcularTotalItem,
    adicionarProduto,
    editarProduto,
    removerProduto,
    removerTodosProdutos,
    validarProdutos,
    selecionarParaEdicao,
    limparSelecao,
    novoProdutoVazio
  }
}

/** Confirmação padrão (sem dependência de lib de UI) — usa `window.confirm` no cliente. */
async function confirmarExclusaoPadrao(mensagem: string): Promise<boolean> {
  if (typeof window === 'undefined') return true
  return window.confirm(mensagem)
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}
