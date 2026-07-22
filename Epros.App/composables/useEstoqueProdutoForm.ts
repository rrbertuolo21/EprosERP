/**
 * useEstoqueProdutoForm — estado do formulário de posição de estoque de um produto.
 *
 * Porta `composables/estoque/useEstoqueProdutoForm.ts` do legado: mantém o formulário
 * reativo (saldo, mínimo, máximo, reservado, custeio), carrega um registro existente por
 * `estoqueId` ou por `produtoId` (busca na listagem, já que a API pode não expor filtro
 * direto — mesma observação do legado) e resolve create-or-update ao salvar.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useRules } from './useRules'
import {
  useEstoqueProduto,
  type EstoqueProduto,
  type EstoqueProdutoCreateDto,
  type EstoqueProdutoUpdateDto,
  type TipoCusteioEstoque
} from './useEstoqueProduto'

/** Tamanho de página ao buscar estoque por produtoId (filtro no cliente até a API expor o parâmetro). */
const BUSCA_POR_PRODUTO_TAMANHO_PAGINA = 100

export interface EstoqueProdutoForm {
  id: string | null
  empresaId: string | null
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  valorSaldo: number
  valorCustoMedio: number | null
  tipoCusteioEstoque: TipoCusteioEstoque
}

export function estoqueProdutoFormInicial(): EstoqueProdutoForm {
  return {
    id: null,
    empresaId: null,
    produtoId: '',
    quantidadeSaldoEstoque: 0,
    quantidadeEstoqueMinimo: 0,
    quantidadeEstoqueMaximo: 0,
    quantidadeEstoqueReservado: 0,
    valorSaldo: 0,
    valorCustoMedio: null,
    tipoCusteioEstoque: 0
  }
}

function paraForm(dto: EstoqueProduto): EstoqueProdutoForm {
  return {
    id: dto.id,
    empresaId: dto.empresaId,
    produtoId: dto.produtoId,
    quantidadeSaldoEstoque: dto.quantidadeSaldoEstoque ?? 0,
    quantidadeEstoqueMinimo: dto.quantidadeEstoqueMinimo ?? 0,
    quantidadeEstoqueMaximo: dto.quantidadeEstoqueMaximo ?? 0,
    quantidadeEstoqueReservado: dto.quantidadeEstoqueReservado ?? 0,
    valorSaldo: dto.valorSaldo ?? 0,
    valorCustoMedio: dto.valorCustoMedio ?? null,
    tipoCusteioEstoque: dto.tipoCusteioEstoque ?? 0
  }
}

function paraCreateDto(form: EstoqueProdutoForm): EstoqueProdutoCreateDto {
  return {
    produtoId: form.produtoId,
    quantidadeSaldoEstoque: form.quantidadeSaldoEstoque,
    quantidadeEstoqueMinimo: form.quantidadeEstoqueMinimo,
    quantidadeEstoqueMaximo: form.quantidadeEstoqueMaximo,
    quantidadeEstoqueReservado: form.quantidadeEstoqueReservado,
    valorSaldo: form.valorSaldo,
    tipoCusteioEstoque: form.tipoCusteioEstoque
  }
}

function paraUpdateDto(form: EstoqueProdutoForm, id: string): EstoqueProdutoUpdateDto {
  return { ...paraCreateDto(form), id }
}

/**
 * Valida o formulário de posição de estoque antes de salvar (gap L6):
 * estoque mínimo não pode ser maior que o máximo (quando máximo > 0) e o saldo não
 * pode ser menor que o reservado. Retorna a mensagem de erro ou `null` se válido.
 */
export function validarEstoqueProdutoForm(form: EstoqueProdutoForm): string | null {
  const rules = useRules()

  const erroMinMax = rules.estoqueMinimoNaoMaiorQueMaximo(
    form.quantidadeEstoqueMinimo,
    form.quantidadeEstoqueMaximo
  )
  if (erroMinMax !== true) return erroMinMax

  const erroSaldoReservado = rules.saldoNaoMenorQueReservado(
    form.quantidadeSaldoEstoque,
    form.quantidadeEstoqueReservado
  )
  if (erroSaldoReservado !== true) return erroSaldoReservado

  return null
}

export function useEstoqueProdutoForm() {
  const form = ref<EstoqueProdutoForm>(estoqueProdutoFormInicial())
  const carregando = ref(false)
  const estoqueRegistroId = ref<string | null>(null)
  const erroValidacao = ref<string | null>(null)

  const { criar, atualizar, buscarPorId } = useEstoqueProduto()

  function resetarForm(): void {
    form.value = estoqueProdutoFormInicial()
    estoqueRegistroId.value = null
  }

  function aplicarResultado(dto: EstoqueProduto): void {
    form.value = paraForm(dto)
    estoqueRegistroId.value = dto.id ?? null
  }

  async function carregarPorEstoqueId(estoqueId: string): Promise<EstoqueProduto | null> {
    carregando.value = true
    try {
      const dados = await buscarPorId(estoqueId)
      if (dados) aplicarResultado(dados)
      return dados
    } finally {
      carregando.value = false
    }
  }

  /**
   * Localiza registro de estoque pelo produtoId.
   * Quando o backend expuser `produtoId` na query de GET, ajustar aqui para filtrar server-side.
   */
  async function carregarPorProdutoId(produtoId: string): Promise<void> {
    carregando.value = true
    try {
      const resposta = await useApi<CommandResult<{ itens?: EstoqueProduto[]; Itens?: EstoqueProduto[] }>>(
        '/estoque/produtos',
        { query: { pagina: 1, tamanhoPagina: BUSCA_POR_PRODUTO_TAMANHO_PAGINA } }
      )
      const dados = extrairDados<{ itens?: EstoqueProduto[]; Itens?: EstoqueProduto[] }>(resposta)
      const lista = dados?.itens ?? dados?.Itens ?? []
      const encontrado = lista.find((item) => item.produtoId === produtoId)

      if (encontrado) {
        aplicarResultado(encontrado)
      } else {
        resetarForm()
        form.value.produtoId = produtoId
      }
    } catch (e) {
      console.error('[useEstoqueProdutoForm.carregarPorProdutoId]', e)
      resetarForm()
      form.value.produtoId = produtoId
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function salvar(produtoId: string): Promise<{ ok: boolean }> {
    form.value.produtoId = produtoId

    erroValidacao.value = validarEstoqueProdutoForm(form.value)
    if (erroValidacao.value) {
      return { ok: false }
    }

    if (estoqueRegistroId.value) {
      const ok = await atualizar(paraUpdateDto(form.value, estoqueRegistroId.value))
      return { ok }
    }

    const ok = await criar(paraCreateDto(form.value))
    if (ok) await carregarPorProdutoId(produtoId)
    return { ok }
  }

  return {
    form,
    erroValidacao,
    carregando,
    estoqueRegistroId,
    resetarForm,
    carregarPorEstoqueId,
    carregarPorProdutoId,
    salvar
  }
}
