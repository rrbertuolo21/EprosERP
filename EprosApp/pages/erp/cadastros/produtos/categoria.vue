<script setup lang="ts">
/**
 * Cadastro de Categorias de Produtos.
 * Porta o comportamento de `cadastros/produto/categoria.vue` do legado (Vuetify) para o
 * design system novo. Listagem simples (sem filtros) + modal de criação/edição + exclusão
 * com confirmação.
 *
 * Endpoint: categorias-produtos (api/v1/categorias-produtos)
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface CategoriaProduto extends Record<string, unknown> {
  id: number
  descricao: string
  sequenciaTenantId?: number
}

interface CategoriaForm {
  id?: number
  descricao: string
}

const toast = useToast()

const lista = useApiList<CategoriaProduto>('/categorias-produtos', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<CategoriaProduto>[] = [
  { key: 'sequenciaTenantId', label: '#', width: '80px' },
  { key: 'descricao', label: 'Descrição', sortable: true }
]

// --- Modal de criação/edição
const dialogAberto = ref(false)
const salvando = ref(false)
const erroDescricao = ref('')
const form = reactive<CategoriaForm>({ descricao: '' })
const editando = ref(false)

function abrirNovo() {
  editando.value = false
  form.id = undefined
  form.descricao = ''
  erroDescricao.value = ''
  dialogAberto.value = true
}

function abrirEdicao(item: CategoriaProduto) {
  editando.value = true
  form.id = item.id
  form.descricao = item.descricao
  erroDescricao.value = ''
  dialogAberto.value = true
}

async function salvar() {
  if (!form.descricao.trim()) {
    erroDescricao.value = 'Informe a descrição'
    return
  }
  erroDescricao.value = ''
  salvando.value = true
  try {
    if (editando.value && form.id) {
      await useApi.put(`/categorias-produtos/{id}`, { ...form }, { params: { id: form.id } })
    } else {
      await useApi.post('/categorias-produtos', { descricao: form.descricao })
    }
    toast.success('Categoria salva com sucesso')
    dialogAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

// --- Exclusão
const itemParaExcluir = ref<CategoriaProduto | null>(null)
const excluindo = ref(false)

function solicitarExclusao(item: CategoriaProduto) {
  itemParaExcluir.value = item
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi.delete(`/categorias-produtos/{id}`, { params: { id: itemParaExcluir.value.id } })
    toast.success('Categoria excluída com sucesso')
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Cadastro de Categorias" subtitle="Categorias de produtos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Adicionar</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma categoria cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="solicitarExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="editando ? 'Editar Categoria' : 'Nova Categoria'" width="480px">
      <form class="form-grid" @submit.prevent="salvar">
        <TextField v-model="form.descricao" label="Descrição" required :error="erroDescricao" />
      </form>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogAberto = false">Fechar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert
      :model-value="!!itemParaExcluir"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @update:model-value="itemParaExcluir = null"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid { display: flex; flex-direction: column; gap: 12px; }
</style>
