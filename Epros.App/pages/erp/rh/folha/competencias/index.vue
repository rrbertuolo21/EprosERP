<script setup lang="ts">
/**
 * Competências de folha — RH / Folha.
 * Fonte: GET/POST /rh/folha/competencias + POST /{id}/fechar.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface Competencia {
  id: string
  competencia?: string | null
  tipo?: string | null
  periodoInicio?: string | null
  periodoFim?: string | null
  situacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Competencia, Filtros>('/rh/folha/competencias', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Competencia>[] = [
  { key: 'competencia', label: 'Competência', sortable: true },
  { key: 'tipo', label: 'Tipo', sortable: false },
  { key: 'periodoInicio', label: 'Início', sortable: false, align: 'center' },
  { key: 'periodoFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Competência...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/folha/competencias/novo')
}

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
async function fechar(item: Competencia) {
  const ok = await confirmRef.value!.open(
    'Fechar competência',
    `Confirma o fechamento da competência ${item.competencia ?? ''}? Após fechada não poderá ser reaberta.`,
    { danger: true, textoConfirmar: 'Fechar' }
  )
  if (!ok) return
  try {
    await useApi(`/rh/folha/competencias/${item.id}/fechar`, { method: 'POST' })
    toast.success('Competência fechada.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Competências de folha" subtitle="Períodos de apuração da folha" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova competência</button>
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
      row-key="id"
      empty-text="Nenhuma competência encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-periodoInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-periodoFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Fechar" @click.stop="fechar(row)">Fechar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
