import { ref, type Ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import type { SelectOption } from '~/composables/useEnum'

/**
 * useOpcoesRh — carregadores de opções (SelectField) reutilizados pelas telas do RH.
 *
 * Só cobre os FKs cujo endpoint de listagem existe no digest da API do módulo:
 *   - colaboradorId → GET /rh/colaboradores
 *   - turnoId       → GET /rh/planejamento/turnos
 *   - vagaId        → GET /rh/recrutamento/vagas
 *
 * Os demais FKs (cargoId, departamentoId, filialId, empresaId, tipo*Id, usuários…) NÃO têm
 * rota de listagem no digest; nas telas eles ficam como TextField (UUID) com TODO comentado.
 */
type Registro = Record<string, unknown>

function primeiroTexto(item: Registro, chaves: string[]): string | undefined {
  for (const c of chaves) {
    const v = item[c]
    if (typeof v === 'string' && v.trim()) return v
  }
  return undefined
}

async function carregarLista(path: string): Promise<Registro[]> {
  const resposta = await useApi(path, { query: { pagina: 1, tamanhoPagina: 500 } })
  const dados = extrairDados<unknown>(resposta)
  if (Array.isArray(dados)) return dados as Registro[]
  if (dados && typeof dados === 'object') {
    const o = dados as { itens?: Registro[]; Itens?: Registro[] }
    return o.itens ?? o.Itens ?? []
  }
  return []
}

export function useOpcoesRh() {
  const colaboradores = ref<SelectOption[]>([]) as Ref<SelectOption[]>
  const turnos = ref<SelectOption[]>([]) as Ref<SelectOption[]>
  const vagas = ref<SelectOption[]>([]) as Ref<SelectOption[]>

  async function carregarColaboradores(): Promise<void> {
    try {
      const lista = await carregarLista('/rh/colaboradores')
      colaboradores.value = lista.map((c) => ({
        label: primeiroTexto(c, ['nome', 'nomeCompleto', 'nomeColaborador']) ?? String(c.id ?? ''),
        value: (c.id as string) ?? ''
      }))
    } catch (e) {
      console.error('[useOpcoesRh] colaboradores', e)
      colaboradores.value = []
    }
  }

  async function carregarTurnos(): Promise<void> {
    try {
      const lista = await carregarLista('/rh/planejamento/turnos')
      turnos.value = lista.map((t) => ({
        label: primeiroTexto(t, ['nome']) ?? String(t.id ?? ''),
        value: (t.id as string) ?? ''
      }))
    } catch (e) {
      console.error('[useOpcoesRh] turnos', e)
      turnos.value = []
    }
  }

  async function carregarVagas(): Promise<void> {
    try {
      const lista = await carregarLista('/rh/recrutamento/vagas')
      vagas.value = lista.map((v) => ({
        label: primeiroTexto(v, ['titulo']) ?? String(v.id ?? ''),
        value: (v.id as string) ?? ''
      }))
    } catch (e) {
      console.error('[useOpcoesRh] vagas', e)
      vagas.value = []
    }
  }

  return {
    colaboradores,
    turnos,
    vagas,
    carregarColaboradores,
    carregarTurnos,
    carregarVagas
  }
}
