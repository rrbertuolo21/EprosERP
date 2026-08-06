<script setup lang="ts">
/**
 * Formulário de Perfil de acesso — árvore de permissões por menu (ver/editar/excluir).
 *
 * Porta `configuracoes/permissoes/perfil-usuarios/[id].vue` do legado.
 *
 * Contrato de rotas fixado (equalização): CRUD de perfis de acesso vive em
 * `api/v1/plataforma/perfis-acesso`.
 *
 * Endpoints consumidos:
 *   - `GET plataforma/perfil/menu` — árvore de menus disponíveis (`PerfilController`).
 *   - `GET plataforma/perfis-acesso/{id}` / `POST plataforma/perfis-acesso` /
 *     `PUT plataforma/perfis-acesso/{id}` — leitura e persistência do perfil e seus acessos
 *     (`PerfisAcessoController`, CRUD completo já implementado no backend).
 *
 * Se a árvore aparecer vazia, é sinal de que `GET plataforma/perfil/menu` não retornou itens
 * (ver `erroCarregamento` abaixo) — a tela mostra um estado de erro/vazio explícito em vez de
 * ficar em branco.
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, type CommandResult, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PermissaoMenuTree, { type MenuPermissaoItem } from '~/components/config/PermissaoMenuTree.vue'

definePageMeta({
  middleware: 'auth'
})

interface MenuBruto {
  id: number
  descricao: string
  itens?: MenuBruto[]
  ver?: boolean
  editar?: boolean
  excluir?: boolean
}

interface AcessoPerfil {
  menuId: number
  menuItemNivel1Id: number | null
  menuItemNivel2Id: number | null
  ver: boolean
  editar: boolean
  excluir: boolean
}

interface Perfil {
  id?: number
  descricao: string
  acessos: AcessoPerfil[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEditing = computed(() => route.params.id !== 'novo')

const perfil = reactive<Perfil>({ descricao: '', acessos: [] })
const menuItems = ref<MenuPermissaoItem[]>([])
const carregando = ref(false)
const salvando = ref(false)
const erroCarregamento = ref<string | null>(null)

/** Converte a árvore bruta de menus (com flags já aplicadas) para o formato da árvore de UI. */
function mapearMenu(itens: MenuBruto[]): MenuPermissaoItem[] {
  return itens.map((item) => ({
    id: item.id,
    titulo: item.descricao,
    ver: item.ver ?? false,
    editar: item.editar ?? false,
    excluir: item.excluir ?? false,
    filhos: item.itens && item.itens.length > 0 ? mapearMenu(item.itens) : undefined
  }))
}

/** Aplica os acessos salvos do perfil sobre a árvore bruta de menus (por nível). */
function aplicarAcessos(itens: MenuBruto[], acessos: AcessoPerfil[]): void {
  const porMenu = new Map<string, AcessoPerfil>()
  for (const acesso of acessos) {
    const chave = [acesso.menuId, acesso.menuItemNivel1Id, acesso.menuItemNivel2Id].filter((v) => v != null).join('.')
    porMenu.set(chave, acesso)
  }

  function percorrer(lista: MenuBruto[], prefixo: string): void {
    for (const item of lista) {
      const chave = prefixo ? `${prefixo}.${item.id}` : String(item.id)
      const encontrado = porMenu.get(chave)
      if (encontrado) {
        item.ver = true
        item.editar = encontrado.editar
        item.excluir = encontrado.excluir
      }
      if (item.itens && item.itens.length > 0) {
        percorrer(item.itens, chave)
      }
    }
  }
  percorrer(itens, '')
}

/** Extrai a lista plana de acessos marcados (ver=true) a partir da árvore de UI, em até 3 níveis. */
function extrairAcessos(itens: MenuPermissaoItem[]): AcessoPerfil[] {
  const acessos: AcessoPerfil[] = []

  for (const nivel1 of itens) {
    if (nivel1.ver) {
      acessos.push({
        menuId: nivel1.id,
        menuItemNivel1Id: null,
        menuItemNivel2Id: null,
        ver: true,
        editar: nivel1.editar,
        excluir: nivel1.excluir
      })
    }
    for (const nivel2 of nivel1.filhos ?? []) {
      if (nivel2.ver) {
        acessos.push({
          menuId: nivel1.id,
          menuItemNivel1Id: nivel2.id,
          menuItemNivel2Id: null,
          ver: true,
          editar: nivel2.editar,
          excluir: nivel2.excluir
        })
      }
      for (const nivel3 of nivel2.filhos ?? []) {
        if (nivel3.ver) {
          acessos.push({
            menuId: nivel1.id,
            menuItemNivel1Id: nivel2.id,
            menuItemNivel2Id: nivel3.id,
            ver: true,
            editar: nivel3.editar,
            excluir: nivel3.excluir
          })
        }
      }
    }
  }
  return acessos
}

function todosOsItens(itens: MenuPermissaoItem[]): MenuPermissaoItem[] {
  let todos: MenuPermissaoItem[] = []
  for (const item of itens) {
    todos.push(item)
    if (item.filhos) todos = todos.concat(todosOsItens(item.filhos))
  }
  return todos
}

const marcarTodosVer = ref(false)
const marcarTodasPermissoes = ref(false)

function alternarSelecaoTotal(): void {
  const todos = todosOsItens(menuItems.value)
  for (const item of todos) item.ver = !marcarTodosVer.value
  marcarTodosVer.value = !marcarTodosVer.value
}

function alternarPermissoesTotal(): void {
  const todos = todosOsItens(menuItems.value)
  for (const item of todos) {
    if (!item.filhos) {
      item.editar = !marcarTodasPermissoes.value
      item.excluir = !marcarTodasPermissoes.value
    }
  }
  marcarTodasPermissoes.value = !marcarTodasPermissoes.value
}

async function carregarMenusEPerfil(): Promise<void> {
  carregando.value = true
  erroCarregamento.value = null
  try {
    const respostaMenu = await useApi<CommandResult<MenuBruto[]> | MenuBruto[]>('/plataforma/perfil/menu')
    const menus = extrairLista<MenuBruto>(respostaMenu) ?? []

    if (isEditing.value) {
      try {
        const respostaPerfil = await useApi<CommandResult<Perfil> | Perfil>('/plataforma/perfis-acesso/{id}', {
          params: { id: route.params.id as string }
        })
        const dados = extrairDados<Perfil>(respostaPerfil)
        if (dados) {
          perfil.id = dados.id
          perfil.descricao = dados.descricao
          perfil.acessos = dados.acessos ?? []
          aplicarAcessos(menus, perfil.acessos)
        }
      } catch (e) {
        toast.error('Não foi possível carregar o perfil (endpoint ainda não disponível no backend).')
      }
    }

    menuItems.value = mapearMenu(menus)
    if (menuItems.value.length === 0) {
      erroCarregamento.value = 'Nenhum item de menu retornado pela API (plataforma/perfil/menu).'
    }
  } catch (e) {
    erroCarregamento.value = obterMensagemErro(e)
    toast.error(erroCarregamento.value)
  } finally {
    carregando.value = false
  }
}

async function salvar(): Promise<void> {
  if (!perfil.descricao.trim()) {
    toast.error('Informe a descrição do perfil.')
    return
  }

  salvando.value = true
  try {
    const acessos = extrairAcessos(menuItems.value)
    const payload = { ...perfil, acessos }

    if (isEditing.value) {
      await useApi('/plataforma/perfis-acesso/{id}', {
        method: 'PUT',
        params: { id: route.params.id as string },
        body: payload
      })
    } else {
      await useApi('/plataforma/perfis-acesso', { method: 'POST', body: payload })
    }
    toast.success('Perfil salvo com sucesso.')
    router.push('/erp/configuracoes/permissoes/perfis')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar(): void {
  router.push('/erp/configuracoes/permissoes/perfis')
}

onMounted(() => {
  void carregarMenusEPerfil()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEditing ? 'Editar perfil' : 'Novo perfil'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <section class="glass-panel section">
      <div class="form-grid">
        <div class="col-6">
          <TextField v-model="perfil.descricao" label="Descrição" required />
        </div>
      </div>
    </section>

    <section class="glass-panel section">
      <div v-if="!carregando && menuItems.length > 0" class="toolbar-selecao">
        <button type="button" class="btn btn-secondary btn-sm" @click="alternarSelecaoTotal">
          {{ marcarTodosVer ? 'Desmarcar todos' : 'Selecionar todos' }}
        </button>
        <button type="button" class="btn btn-secondary btn-sm" @click="alternarPermissoesTotal">
          {{ marcarTodasPermissoes ? 'Desmarcar todas permissões' : 'Selecionar todas permissões' }}
        </button>
      </div>

      <div v-if="carregando" class="table-loading"><span class="spinner"></span> Carregando permissões...</div>
      <div v-else-if="erroCarregamento && menuItems.length === 0" class="permissoes-erro">
        <span class="chip chip-danger">Falha ao carregar</span>
        <p>{{ erroCarregamento }}</p>
      </div>
      <div v-else-if="menuItems.length === 0" class="table-empty">Nenhum item de menu disponível.</div>
      <PermissaoMenuTree v-else :items="menuItems" />
    </section>
  </div>
</template>

<style scoped>
.section { margin-top: 16px; padding: 20px; }
.toolbar-selecao { display: flex; gap: 10px; margin-bottom: 16px; }
.permissoes-erro { padding: 24px 16px; text-align: center; color: var(--text-secondary); }
.permissoes-erro p { margin-top: 10px; font-size: 13px; }
</style>
