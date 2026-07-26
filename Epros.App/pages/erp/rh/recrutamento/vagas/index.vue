<script setup lang="ts">
/**
 * Vagas — RH / Recrutamento.
 * Fonte: GET/POST /rh/recrutamento/vagas + POST /{id}/publicar.
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

interface Vaga {
  id: string
  titulo?: string | null
  posicoes?: number | null
  prioridade?: number | null
  situacao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Vaga, Filtros>('/rh/recrutamento/vagas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Vaga>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'posicoes', label: 'Posições', sortable: false, align: 'right', width: '110px' },
  { key: 'prioridade', label: 'Prioridade', sortable: false, align: 'right', width: '110px' },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Título...', grow: true }
]
function novo() {
  router.push('/erp/rh/recrutamento/vagas/novo')
}

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
async function publicar(item: Vaga) {
  const ok = await confirmRef.value!.open(
    'Publicar vaga',
    `Confirma a publicação da vaga "${item.titulo ?? ''}"?`,
    { textoConfirmar: 'Publicar' }
  )
  if (!ok) return
  try {
    await useApi(`/rh/recrutamento/vagas/${item.id}/publicar`, { method: 'POST' })
    toast.success('Vaga publicada.')
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
    <PageToolbar title="Vagas" subtitle="Vagas de recrutamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova vaga</button>
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
      empty-text="Nenhuma vaga encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Publicar" @click.stop="publicar(row)">Publicar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
