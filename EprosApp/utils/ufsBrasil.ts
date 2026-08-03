import type { SelectOption } from '~/composables/useEnum'

/**
 * UFs do Brasil — dado de REFERÊNCIA estático (federativo, não muda em runtime).
 *
 * A API NÃO expõe uma rota `/geografia/ufs` (os municípios são chaveados pela sigla da UF,
 * não há entidade UF listável), então formulários de endereço devem usar esta constante em
 * vez de fazer fetch — evita o 404 que quebrava o carregamento das listas de apoio (Classe C).
 * Fonte única para dedupe das cópias locais espalhadas pelas telas fiscais/cadastros.
 */
export const SIGLAS_UF: readonly string[] = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]

/** UFs no formato de opções de select (`{ label, value }`). */
export const ufsBrasil: SelectOption[] = SIGLAS_UF.map((uf) => ({ label: uf, value: uf }))
