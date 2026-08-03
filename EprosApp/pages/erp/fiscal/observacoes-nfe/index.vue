<script setup lang="ts">
/**
 * Observações de NF-e — Fiscal / Observações NF-e.
 *
 * Porta o comportamento de `fiscal/observacoes-nfe/index.vue` do legado: listagem paginada
 * server-side de textos de observação reutilizáveis na emissão de NF-e/NFC-e, com CRUD em
 * modal simples (apenas descrição).
 * Endpoint: `fiscal/observacoes-nfe`.
 */
import { reactive, ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface ObservacaoNfe {
  id: string
  descricao: string
}

interface ObservacaoNfeFiltros {
  localizar?: string
}

const toast = useToast()

const lista = useApiList<ObservacaoNfe, ObservacaoNfeFiltros>('/fiscal/observacoes-nfe', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<ObservacaoNfe>[] = [
  { key: 'id', label: '#', sortable: true, width: '80px' },
  { key: 'descricao', label: 'Descrição', sortable: true }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Descrição da observação...', grow: true }
]

const dialogAberto = ref(false)
const salvando = ref(false)
const form = reactive<{ id?: string; descricao: string }>({ id: undefined, descricao: '' })

function abrirNovo() {
  form.id = undefined
  form.descricao = ''
  dialogAberto.value = true
}

function abrirEdicao(item: ObservacaoNfe) {
  form.id = item.id
  form.descricao = item.descricao
  dialogAberto.value = true
}

async function salvar() {
  if (!form.descricao.trim()) {
    toast.error('Informe a descrição da observação.')
    return
  }
  salvando.value = true
  try {
    if (form.id) {
      await useApi(`/fiscal/observacoes-nfe/${form.id}`, { method: 'PUT', body: { id: form.id, descricao: form.descricao } })
    } else {
      await useApi('/fiscal/observacoes-nfe', { method: 'POST', body: { descricao: form.descricao } })
    }
    toast.success('Observação salva com sucesso!')
    dialogAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ObservacaoNfe | null>(null)

function pedirExclusao(item: ObservacaoNfe) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/fiscal/observacoes-nfe/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Observação removida com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
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
    <PageToolbar title="Cadastro de Observações" subtitle="Textos padrão reutilizáveis na emissão de notas fiscais" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Novo</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma observação cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrirEdicao"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="form.id ? 'Editando Observação' : 'Nova Observação'" width="560px">
      <form class="vertical-form" @submit.prevent="salvar">
        <TextField v-model="form.descricao" label="Descrição" required maxlength="2000" />
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
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
