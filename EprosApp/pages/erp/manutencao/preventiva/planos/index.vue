<script setup lang="ts">
/**
 * Listagem de Planos Preventivos — Manutenção / Preventiva / Planos.
 * Fonte: GET /manutencao/preventiva/planos.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { rotuloStatusRegistro } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface Plano {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
  alvoTipo?: string | null
}

const router = useRouter()

const lista = useApiList<Plano>('/manutencao/preventiva/planos', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Plano>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'alvoTipo', label: 'Alvo', sortable: true, width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' }
]

function novo() {
  router.push('/erp/manutencao/preventiva/planos/novo')
}
function abrir(item: Plano) {
  router.push(`/erp/manutencao/preventiva/planos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Planos preventivos" subtitle="Periodicidades, kit de peças e ativação de planos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo plano</button>
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
      empty-text="Nenhum plano encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }"><span class="badge badge-info">{{ rotuloStatusRegistro(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
