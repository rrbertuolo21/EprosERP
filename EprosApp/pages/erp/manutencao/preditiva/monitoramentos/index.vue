<script setup lang="ts">
/**
 * Listagem de Monitoramentos Preditivos — Manutenção / Preditiva / Monitoramentos.
 * Fonte: GET /manutencao/preditiva/monitoramentos.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { rotuloStatusRegistro } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface Monitoramento {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
}

const router = useRouter()

const lista = useApiList<Monitoramento>('/manutencao/preditiva/monitoramentos', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Monitoramento>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' }
]

function novo() {
  router.push('/erp/manutencao/preditiva/monitoramentos/novo')
}
function abrir(item: Monitoramento) {
  router.push(`/erp/manutencao/preditiva/monitoramentos/${item.id}`)
}
function irAlarmes() {
  router.push('/erp/manutencao/preditiva/alarmes')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Monitoramentos preditivos" subtitle="Pontos de medição, regras, leituras e alarmes" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="irAlarmes">Alarmes</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo monitoramento</button>
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
      empty-text="Nenhum monitoramento encontrado."
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
