/**
 * Helper local do módulo Concessionárias — carrega opções para campos de chave
 * estrangeira (`*Id (uuid)`) a partir do endpoint de listagem correspondente.
 *
 * Como os DTOs de listagem não expõem um campo de rótulo padronizado, usamos a
 * melhor tentativa (nome/código/descrição/número/…); no pior caso caímos no `id`.
 * IO exclusivamente por `useApi` (SEM prefixo /api/v1).
 */
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'

type Registro = Record<string, unknown>

/** Escolhe o melhor rótulo disponível para um item de listagem. */
function rotuloItem(o: Registro): string {
  const candidato =
    o.nome ??
    o.codigo ??
    o.descricao ??
    o.numeroContrato ??
    o.numeroOs ??
    o.numero ??
    o.protocolo ??
    o.chassiVin ??
    o.chassi ??
    o.placa ??
    o.modelo ??
    o.origem ??
    o.id ??
    o.Id
  return String(candidato ?? '')
}

/**
 * Busca as opções de um endpoint de listagem paginado do módulo.
 * @param path Rota da API (SEM /api/v1). Ex.: '/concessionarias/crm/oportunidades'.
 */
export async function carregarOpcoesFk(path: string): Promise<SelectOption[]> {
  try {
    const resposta = await useApi(path, { query: { pagina: 1, tamanhoPagina: 100 } })
    const dados = extrairDados<unknown>(resposta)
    const arr: unknown[] = Array.isArray(dados)
      ? dados
      : ((dados as { itens?: unknown[]; Itens?: unknown[] })?.itens ??
         (dados as { Itens?: unknown[] })?.Itens ??
         [])
    return (arr as Registro[])
      .map((o) => ({
        value: (o.id ?? o.Id ?? '') as string,
        label: rotuloItem(o)
      }))
      .filter((o) => o.value !== '')
  } catch {
    return []
  }
}
