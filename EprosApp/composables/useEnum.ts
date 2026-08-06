import { ref, type Ref } from 'vue'
import { useState } from '#app'
import { useApi, extrairDados, extrairLista} from './useApi'

/** Item de enumeração de domínio retornado pela API. */
export interface Enum {
  id: number
  descricao: string
  descricaoFormatada?: string
}

/** Item de select padronizado para os campos compartilhados. */
export interface SelectOption {
  label: string
  value: number | string
}

/**
 * Composable de enums de domínio (selects).
 *
 * Busca listas de enumeração da API e mantém um cache em memória por URI (compartilhado
 * via `useState`) para evitar refetch. Consumido por `SelectField` e telas com selects de domínio.
 *
 * Contrato:
 *   const tipos = ref<Enum[]>([])
 *   const { carregarEnum, paraOpcoes } = useEnum()
 *   await carregarEnum('pessoas-enums/tipo-pessoa', tipos)
 *   const opcoes = paraOpcoes(tipos.value)  // => { label, value }[]
 */
/**
 * Domínios cujo segmento no front não bate com o nome do enum no backend.
 * O controller genérico `/api/v1/enums/{dominio}` resolve por reflexão sobre o nome do enum
 * (tolerante a caixa/prefixo E/separadores), então só precisamos mapear os que divergem de fato.
 */
const ALIAS_DOMINIO: Record<string, string> = {
  'situacao-titulo': 'situacao', // ContasAReceber/Pagar.Situacao -> ESituacao
  'venda-status-dfe': 'documento-fiscal-status' // Vendas -> EDocumentoFiscalStatus
}

/**
 * Canoniza a URI de enum para o controller genérico `enums/{dominio}`.
 * Aceita os padrões legados que o front usa: `<modulo>-enums/<dominio>` e `<algo>/enum-<dominio>`
 * (ex.: `estoque-enums/tipo-estoque`, `balancas/enum-tipo-valor-balanca`). URIs que já são
 * `enums/x` (ou qualquer outro endpoint) passam intactas.
 */
export function canonizarUriEnum(uri: string): string {
  const u = uri.replace(/^\/+/, '')
  let dominio: string | null = null
  const mSufixoEnums = u.match(/(?:^|\/)[a-z0-9]*-?enums\/(.+)$/i)
  const mPrefixoEnum = u.match(/\/enum-([a-z0-9-]+)$/i)
  if (mSufixoEnums) dominio = mSufixoEnums[1]
  else if (mPrefixoEnum) dominio = mPrefixoEnum[1]
  if (dominio == null) return u
  dominio = ALIAS_DOMINIO[dominio] ?? dominio
  return `enums/${dominio}`
}

export function useEnum() {
  const cache = useState<Record<string, Enum[]>>('epros-enum-cache', () => ({}))

  /** Busca (ou reutiliza do cache) um enum por URI e preenche o ref informado. */
  async function carregarEnum(uriBruta: string, destino: Ref<Enum[]>): Promise<Enum[]> {
    const uri = canonizarUriEnum(uriBruta)
    if (cache.value[uri]) {
      destino.value = cache.value[uri]
      return cache.value[uri]
    }
    try {
      const resposta = await useApi(uri)
      const lista = extrairLista<Enum>(resposta) ?? []
      cache.value = { ...cache.value, [uri]: lista }
      destino.value = lista
      return lista
    } catch (e) {
      console.error(`[useEnum:${uri}]`, e)
      destino.value = []
      return []
    }
  }

  /** Converte uma lista de Enum em opções { label, value } para os selects. */
  function paraOpcoes(itens: Enum[]): SelectOption[] {
    return itens.map((e) => ({
      label: e.descricaoFormatada ?? e.descricao,
      value: e.id
    }))
  }

  /** Busca um enum e já devolve as opções prontas para o select. */
  async function carregarOpcoes(uri: string): Promise<SelectOption[]> {
    const destino = ref<Enum[]>([])
    await carregarEnum(uri, destino)
    return paraOpcoes(destino.value)
  }

  return {
    carregarEnum,
    carregarOpcoes,
    paraOpcoes
  }
}
