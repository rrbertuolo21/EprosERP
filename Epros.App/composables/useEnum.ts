import { ref, type Ref } from 'vue'
import { useState } from '#app'
import { useApi, extrairDados } from './useApi'

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
export function useEnum() {
  const cache = useState<Record<string, Enum[]>>('epros-enum-cache', () => ({}))

  /** Busca (ou reutiliza do cache) um enum por URI e preenche o ref informado. */
  async function carregarEnum(uri: string, destino: Ref<Enum[]>): Promise<Enum[]> {
    if (cache.value[uri]) {
      destino.value = cache.value[uri]
      return cache.value[uri]
    }
    try {
      const resposta = await useApi(uri)
      const lista = extrairDados<Enum[]>(resposta) ?? []
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
