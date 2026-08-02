<script setup lang="ts">
/**
 * Dimensões Analíticas — Contabilidade Gerencial / Dimensões.
 *
 * Contrato:
 *   GET  /contabilidade-gerencial/dimensoes?tipo=
 *   POST /contabilidade-gerencial/dimensoes    (tipo, valor)
 * Só há lista + criação (sem GET/{id}, PUT ou DELETE): criação por diálogo.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface DimensaoAnalitica {
  id: string
  tipo?: string | null
  valor?: string | null
}

interface DimensaoFiltros {
  tipo?: string | null
}

const toast = useToast()

const lista = useApiList<DimensaoAnalitica, DimensaoFiltros>('/contabilidade-gerencial/dimensoes', {
  filtrosIniciais: { tipo: '' },
  tamanhoPaginaInicial: 100
})

const colunas: DataTableColumn<DimensaoAnalitica>[] = [
  { key: 'tipo', label: 'Tipo', sortable: false, width: '260px' },
  { key: 'valor', label: 'Valor', sortable: false }
]

const camposFiltro: FilterField[] = [
  { key: 'tipo', label: 'Tipo', type: 'text', placeholder: 'Filtrar por tipo...', grow: true }
]

/* ----- Criar dimensão ----- */
const criarVisivel = ref(false)
const salvando = ref(false)
const nova = reactive<{ tipo: string; valor: string }>({ tipo: '', valor: '' })

function abrirCriar() {
  nova.tipo = ''
  nova.valor = ''
  criarVisivel.value = true
}

async function salvarCriar() {
  if (!nova.tipo.trim() || !nova.valor.trim()) {
    toast.error('Tipo e valor são obrigatórios.')
    return
  }
  salvando.value = true
  try {
    await useApi('/contabilidade-gerencial/dimensoes', { method: 'POST', body: { tipo: nova.tipo, valor: nova.valor } })
    toast.success('Dimensão analítica criada com sucesso!')
    criarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Dimensões Analíticas"
      subtitle="Dimensões (tipo/valor) para análise gerencial das alocações"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirCriar">+ Nova dimensão</button>
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
      empty-text="Nenhuma dimensão analítica encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    />

    <AppDialog v-model="criarVisivel" title="Nova dimensão analítica" width="480px">
      <div class="dialog-form">
        <TextField v-model="nova.tipo" label="Tipo" required maxlength="60" placeholder="Ex.: Projeto, Filial, Produto" />
        <TextField v-model="nova.valor" label="Valor" required maxlength="120" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="criarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarCriar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-form { display: flex; flex-direction: column; gap: 14px; }
</style>
