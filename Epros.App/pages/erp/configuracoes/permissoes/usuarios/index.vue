<script setup lang="ts">
/**
 * Usuários — listagem paginada de usuários do aplicativo.
 *
 * Porta o comportamento de `configuracoes/permissoes/usuarios/index.vue` do legado,
 * consumindo `aplicativo/usuarios` (GET lista / DELETE).
 */
import { onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({
  middleware: 'auth'
})

interface UsuarioLinha {
  id: string
  sequenciaTenantId?: number
  nome: string
  login: string
  email: string
  ativo: boolean
  [key: string]: unknown
}

interface UsuarioFiltros {
  search?: string
}

const toast = useToast()

const lista = useApiList<UsuarioLinha, UsuarioFiltros>('/aplicativo/usuarios', {
  filtrosIniciais: { search: '' },
  tamanhoPaginaInicial: 20
})

const filtroFields: FilterField[] = [
  { key: 'search', label: 'Buscar', type: 'text', placeholder: 'Nome, login ou e-mail...', grow: true }
]

const colunas: DataTableColumn<UsuarioLinha>[] = [
  { key: 'sequenciaTenantId', label: '#', width: '70px' },
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'login', label: 'Login', sortable: true },
  { key: 'email', label: 'E-mail', sortable: true },
  { key: 'ativo', label: 'Ativo', align: 'center', width: '90px' }
]

const excluirVisivel = ref(false)
const excluirAlvo = ref<UsuarioLinha | null>(null)
const excluindo = ref(false)

function novoUsuario() {
  navigateTo('/erp/configuracoes/permissoes/usuarios/novo')
}

function editarUsuario(item: UsuarioLinha) {
  navigateTo(`/erp/configuracoes/permissoes/usuarios/${item.id}`)
}

function pedirExclusao(item: UsuarioLinha) {
  excluirAlvo.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!excluirAlvo.value) return
  excluindo.value = true
  try {
    await useApi(`/aplicativo/usuarios/{id}`, { method: 'DELETE', params: { id: excluirAlvo.value.id } })
    toast.success('Usuário excluído com sucesso.')
    excluirVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Usuários" subtitle="Usuários com acesso ao ERP" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoUsuario">+ Novo usuário</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="filtroFields"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as never)"
      @clear="lista.limpar"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      row-key="id"
      empty-text="Nenhum usuário encontrado."
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(s) => lista.buscar({ tamanhoPagina: s, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editarUsuario"
    >
      <template #cell-ativo="{ row }">
        <span class="badge" :class="row.ativo ? 'badge-paga' : 'badge-cancelada'">
          {{ row.ativo ? 'Sim' : 'Não' }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarUsuario(row)">✏️</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑️</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="excluirAlvo?.nome"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
