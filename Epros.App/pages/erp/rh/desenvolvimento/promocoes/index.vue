<script setup lang="ts">
/**
 * Promoções — RH / Desenvolvimento.
 * Fonte: GET/POST /rh/desenvolvimento/promocoes + POST /{id}/aprovar e /{id}/rejeitar.
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

interface Promocao {
  id: string
  motivo?: string | null
  dataEfetiva?: string | null
  situacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Promocao, Filtros>('/rh/desenvolvimento/promocoes', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Promocao>[] = [
  { key: 'motivo', label: 'Motivo', sortable: false },
  { key: 'dataEfetiva', label: 'Data efetiva', sortable: true, align: 'center' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Motivo...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/desenvolvimento/promocoes/novo')
}

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
async function acao(item: Promocao, tipo: 'aprovar' | 'rejeitar') {
  const ok = await confirmRef.value!.open(
    tipo === 'aprovar' ? 'Aprovar promoção' : 'Rejeitar promoção',
    `Confirma ${tipo} esta promoção?`,
    { danger: tipo === 'rejeitar', textoConfirmar: tipo === 'aprovar' ? 'Aprovar' : 'Rejeitar' }
  )
  if (!ok) return
  try {
    await useApi(`/rh/desenvolvimento/promocoes/${item.id}/${tipo}`, { method: 'POST' })
    toast.success(tipo === 'aprovar' ? 'Promoção aprovada.' : 'Promoção rejeitada.')
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
    <PageToolbar title="Promoções" subtitle="Movimentações de promoção" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova promoção</button>
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
      empty-text="Nenhuma promoção encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataEfetiva="{ value }">
        <span>{{ formatarData(value as string) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click.stop="acao(row, 'aprovar')">Aprovar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Rejeitar" @click.stop="acao(row, 'rejeitar')">Rejeitar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
