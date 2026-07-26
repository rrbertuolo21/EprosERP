<script setup lang="ts">
/**
 * Listagem de Regras de Segregação de Funções (SoD) — GRC.
 * Fonte: GET /api/v1/grc/sod/regras, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Regra {
  id: string
  codigo?: string | null
  criticidade?: string | null
  vigenciaInicio?: string | null
  vigenciaFim?: string | null
  status?: string | null
}

const router = useRouter()
const lista = useApiList<Regra>('/grc/sod/regras', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Regra>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '150px' },
  { key: 'criticidade', label: 'Criticidade', sortable: true, align: 'center', width: '130px' },
  { key: 'vigenciaInicio', label: 'Vigência início', sortable: true, width: '150px' },
  { key: 'vigenciaFim', label: 'Vigência fim', sortable: true, width: '150px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Regras de Segregação de Funções (SoD)" subtitle="Conflitos entre funções incompatíveis" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/sod-regras/novo')">+ Nova regra</button>
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
      empty-text="Nenhuma regra SoD cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-criticidade="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-vigenciaInicio="{ value }">{{ formatarData(value) }}</template>
      <template #cell-vigenciaFim="{ value }">{{ formatarData(value) }}</template>
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
