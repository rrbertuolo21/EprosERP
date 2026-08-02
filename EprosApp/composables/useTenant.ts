import { computed } from 'vue'
import { useState } from '#app'
import { useAuth, type Empresa } from './useAuth'

const EMPRESA_KEY = 'epros_empresa'

/**
 * Composable de tenant/empresa ativa.
 *
 * Expõe a empresa selecionada (multiempresa) e o `empresaId` que as telas usam para
 * filtrar dados por empresa. A seleção é persistida em `epros_empresa` e compartilhada
 * entre componentes via `useState` (estado global do Nuxt, sem Pinia).
 */
export function useTenant() {
  const { getUser } = useAuth()

  // Estado global compartilhado (evita múltiplas leituras dessincronizadas do localStorage).
  const empresaAtiva = useState<Empresa | null>('epros-empresa-ativa', () => {
    if (import.meta.server) return null
    const raw = localStorage.getItem(EMPRESA_KEY)
    if (raw) {
      try {
        return JSON.parse(raw) as Empresa
      } catch {
        /* ignore */
      }
    }
    // Default: primeira empresa do usuário logado, se houver.
    const empresas = getUser()?.empresas
    return empresas && empresas.length > 0 ? empresas[0] : null
  })

  /** Lista de empresas às quais o usuário logado tem acesso. */
  const empresas = computed<Empresa[]>(() => getUser()?.empresas ?? [])

  /** ID da empresa ativa (0 quando nenhuma selecionada). */
  const empresaId = computed<number>(() => empresaAtiva.value?.id ?? 0)

  /** Tenant do usuário logado. */
  const tenantId = computed<string | undefined>(() => getUser()?.tenantId)

  /** Seleciona/troca a empresa ativa e persiste a escolha. */
  function selecionarEmpresa(empresa: Empresa): void {
    empresaAtiva.value = empresa
    if (import.meta.client) {
      localStorage.setItem(EMPRESA_KEY, JSON.stringify(empresa))
    }
  }

  /** Seleciona a empresa ativa pelo id (busca na lista do usuário). */
  function selecionarEmpresaPorId(id: number): void {
    const alvo = empresas.value.find((e) => e.id === id)
    if (alvo) selecionarEmpresa(alvo)
  }

  return {
    empresaAtiva,
    empresas,
    empresaId,
    tenantId,
    selecionarEmpresa,
    selecionarEmpresaPorId
  }
}
