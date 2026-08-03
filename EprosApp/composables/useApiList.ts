import { ref, computed, type Ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'

/**
 * Ordenação server-side. Casa com o padrão do backend (`ordenarPor` + `ordemAscendente`).
 */
export interface SortDescriptor {
  key: string
  order: 'asc' | 'desc'
}

export interface UseApiListOptions<F extends object> {
  /** Filtros iniciais (estado base do formulário de filtros). */
  filtrosIniciais?: F
  /** Tamanho de página inicial. Default 20. */
  tamanhoPaginaInicial?: number
  /** Se true, dispara a primeira busca imediatamente. Default false (a tela chama `buscar`). */
  imediato?: boolean
  /** Ref de loading externo, para compartilhar o mesmo flag entre toolbar/tabela/ações. */
  loading?: Ref<boolean>
}

/**
 * Helper de listagem paginada server-side.
 *
 * Monta a query no padrão da API EprosERP: `{ ...filtros, pagina, tamanhoPagina, ordenarPor?, ordemAscendente? }`
 * e lê a resposta no padrão CommandResult `{ sucesso, mensagem, dados, totalRegistros }`
 * (também aceita `data` como alias de `dados`).
 *
 * Contrato de consumo (telas `index.vue`):
 *   const lista = useApiList<Pessoa, PessoaFiltros>('/cadastros/pessoas', { filtrosIniciais })
 *   onMounted(() => lista.buscar())
 *   // v-model:page="lista.pagina", :total="lista.total", :items="lista.itens", :loading="lista.carregando"
 *
 * @param path  Rota da API (SEM o prefixo /api/v1; ele é adicionado por useApi).
 */
export function useApiList<T = Record<string, unknown>, F extends object = Record<string, unknown>>(
  path: string,
  options: UseApiListOptions<F> = {}
) {
  const {
    filtrosIniciais = {} as F,
    tamanhoPaginaInicial = 20,
    imediato = false,
    loading
  } = options

  const itens = ref<T[]>([]) as Ref<T[]>
  const total = ref(0)
  const pagina = ref(1)
  const tamanhoPagina = ref(tamanhoPaginaInicial)
  const ordenacao = ref<SortDescriptor | null>(null)
  const filtros = ref<F>({ ...filtrosIniciais }) as Ref<F>
  const carregandoInterno = ref(false)
  const carregando = loading ?? carregandoInterno
  const erro = ref<string | null>(null)

  const totalPaginas = computed(() =>
    tamanhoPagina.value > 0 ? Math.ceil(total.value / tamanhoPagina.value) : 0
  )

  /**
   * Executa a busca com a página/tamanho/ordenação/filtros atuais.
   * Aceita override pontual de página (ex.: vindo do `@update:page` da DataTable).
   */
  async function buscar(overrides?: { pagina?: number; tamanhoPagina?: number; ordenacao?: SortDescriptor | null }): Promise<void> {
    if (overrides?.pagina !== undefined) pagina.value = overrides.pagina
    if (overrides?.tamanhoPagina !== undefined) tamanhoPagina.value = overrides.tamanhoPagina
    if (overrides?.ordenacao !== undefined) ordenacao.value = overrides.ordenacao

    carregando.value = true
    erro.value = null
    try {
      const query: Record<string, unknown> = {
        ...limparFiltros(filtros.value),
        pagina: pagina.value,
        tamanhoPagina: tamanhoPagina.value
      }
      if (ordenacao.value) {
        query.ordenarPor = ordenacao.value.key
        query.ordemAscendente = ordenacao.value.order === 'asc'
      }

      const resposta = await useApi<CommandResult<unknown>>(path, { query })
      // Os handlers de listagem retornam dados no formato { total, pagina, itens }.
      // Também aceitamos dados como array direto (endpoints que já devolvem lista) por robustez.
      const d = extrairDados<unknown>(resposta) as unknown
      if (Array.isArray(d)) {
        itens.value = d as T[]
        total.value = (resposta as { totalRegistros?: number } | null)?.totalRegistros ?? (d as T[]).length
      } else if (d && typeof d === 'object') {
        const o = d as { itens?: T[]; Itens?: T[]; total?: number; Total?: number }
        itens.value = o.itens ?? o.Itens ?? []
        total.value = o.total ?? o.Total ?? itens.value.length
      } else {
        itens.value = []
        total.value = 0
      }
    } catch (e) {
      erro.value = obterMensagemErro(e)
      itens.value = []
      total.value = 0
      console.error(`[useApiList:${path}]`, e)
    } finally {
      carregando.value = false
    }
  }

  /** Aplica novos filtros e volta para a página 1. */
  async function aplicarFiltros(novosFiltros: Partial<F>): Promise<void> {
    filtros.value = { ...filtros.value, ...novosFiltros }
    pagina.value = 1
    await buscar()
  }

  /** Reseta filtros para o estado inicial e recarrega. */
  async function limpar(): Promise<void> {
    filtros.value = { ...filtrosIniciais }
    pagina.value = 1
    ordenacao.value = null
    await buscar()
  }

  /** Muda de página e recarrega. */
  async function irParaPagina(novaPagina: number): Promise<void> {
    await buscar({ pagina: novaPagina })
  }

  if (imediato) {
    void buscar()
  }

  return {
    // estado
    itens,
    total,
    totalPaginas,
    pagina,
    tamanhoPagina,
    ordenacao,
    filtros,
    carregando,
    erro,
    // ações
    buscar,
    aplicarFiltros,
    limpar,
    irParaPagina
  }
}

/** Remove chaves vazias (undefined, null, '') para não poluir a query string. */
function limparFiltros<F extends object>(filtros: F): Record<string, unknown> {
  const resultado: Record<string, unknown> = {}
  for (const [chave, valor] of Object.entries(filtros)) {
    if (valor !== undefined && valor !== null && valor !== '') {
      resultado[chave] = valor
    }
  }
  return resultado
}

/** Extrai uma mensagem amigável do erro do ofetch (CommandResult.mensagem quando existir). */
export function obterMensagemErro(e: unknown): string {
  if (e && typeof e === 'object') {
    const err = e as { data?: CommandResult; message?: string }
    const msg = err.data?.mensagem
    if (msg) return Array.isArray(msg) ? msg.join('\n') : String(msg)
    if (err.message) return err.message
  }
  return 'Ocorreu um erro inesperado. Tente novamente.'
}
