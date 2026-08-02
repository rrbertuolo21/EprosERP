import { ref } from 'vue'
import { useApi } from './useApi'
import type { AppIconName } from '../components/AppIcon.vue'
import type { MenuGroup, MenuItem } from '../components/menu'

/**
 * 1.10 (PERMISSOES_DE_MENU) — consumo do menu do usuário logado a partir do endpoint canônico
 * `GET /api/v1/menu`. O servidor devolve APENAS os itens visíveis, projetados das CAPACIDADES
 * efetivas do RBAC (mesma fonte do gate `[AbacAuthorize]`/AbacFilter), já respeitando a empresa
 * corrente (1.09) e o entitlement de plano (1.06). Substitui a árvore estática `erpMenu` (LC-3):
 * o sidebar passa a renderizar só o que veio, em vez de "mostrar tudo e a API nega".
 *
 * Defense-in-depth: esconder no menu NÃO substitui o gate — o [AbacAuthorize] segue protegendo a API.
 */

/** Item de menu no contrato do servidor (MenuVisivelItemDto / MenuVisivelDto). */
interface MenuServidorItem {
  sub?: string
  menu?: string
  icon?: string | null
  to?: string | null
  ordem?: number
  capacidadeRequerida?: string | null
  itens?: MenuServidorItem[]
}

interface MenuServidorResposta {
  menu: MenuServidorItem[]
  isAdmin: boolean
  tenantId: string
  empresaId: string
}

interface CommandResultMenu {
  sucesso: boolean
  dados?: MenuServidorResposta
}

/** Achata os níveis 1/2 de um grupo em uma lista plana de itens navegáveis (com rota). */
function achatarItens(itens: MenuServidorItem[] | undefined): MenuItem[] {
  const saida: MenuItem[] = []
  for (const it of itens ?? []) {
    const label = it.sub ?? it.menu ?? ''
    if (it.to) {
      saida.push({ label, to: it.to, icon: (it.icon as AppIconName | undefined) ?? undefined })
    }
    if (it.itens && it.itens.length > 0) {
      saida.push(...achatarItens(it.itens))
    }
  }
  return saida
}

/** Mapeia a árvore do servidor para o contrato do sidebar (MenuGroup[]). */
function mapearParaGrupos(resp: MenuServidorResposta | undefined): MenuGroup[] {
  if (!resp?.menu) return []
  return resp.menu.map((m) => {
    const itens = achatarItens(m.itens)
    // Menu-folha (tem rota própria e nenhum filho navegável): vira um grupo com um único item.
    if (itens.length === 0 && m.to) {
      itens.push({ label: m.menu ?? '', to: m.to, icon: (m.icon as AppIconName | undefined) ?? undefined })
    }
    return {
      label: m.menu ?? '',
      icon: (m.icon as AppIconName | undefined) ?? undefined,
      itens
    }
  })
}

/**
 * Composable do menu do usuário. `carregar()` busca `/menu`; enquanto isso `grupos` fica vazio e o
 * chamador pode exibir um fallback. `isAdmin` reflete o papel de sistema Administrador (informativo).
 */
export function useMenu() {
  const grupos = ref<MenuGroup[]>([])
  const isAdmin = ref(false)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  async function carregar(): Promise<MenuGroup[]> {
    carregando.value = true
    erro.value = null
    try {
      const resp = await useApi.get<CommandResultMenu>('/menu')
      const dados = resp?.dados
      grupos.value = mapearParaGrupos(dados)
      isAdmin.value = dados?.isAdmin ?? false
      return grupos.value
    } catch (e) {
      erro.value = e instanceof Error ? e.message : 'Falha ao carregar o menu.'
      console.error('[useMenu.carregar]', e)
      grupos.value = []
      return grupos.value
    } finally {
      carregando.value = false
    }
  }

  return { grupos, isAdmin, carregando, erro, carregar }
}
