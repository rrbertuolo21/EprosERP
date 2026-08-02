<script setup lang="ts">
/**
 * Períodos de ponto — RH / Ponto.
 * Fonte: GET/POST /rh/ponto/periodos + POST /{id}/fechar.
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

interface Periodo {
  id: string
  competencia?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  situacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Periodo, Filtros>('/rh/ponto/periodos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Periodo>[] = [
  { key: 'competencia', label: 'Competência', sortable: true },
  { key: 'dataInicio', label: 'Início', sortable: false, align: 'center' },
  { key: 'dataFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Competência...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/ponto/periodos/novo')
}

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
async function fechar(item: Periodo) {
  const ok = await confirmRef.value!.open(
    'Fechar período',
    `Confirma o fechamento do período ${item.competencia ?? ''}?`,
    { danger: true, textoConfirmar: 'Fechar' }
  )
  if (!ok) return
  try {
    await useApi(`/rh/ponto/periodos/${item.id}/fechar`, { method: 'POST' })
    toast.success('Período fechado.')
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
    <PageToolbar title="Períodos de ponto" subtitle="Períodos de apuração do ponto" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo período</button>
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
      empty-text="Nenhum período encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Fechar" @click.stop="fechar(row)">Fechar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
