<script setup lang="ts">
/**
 * Listagem de Denúncias — GRC / Investigações e Denúncias.
 * Fonte: GET /api/v1/grc/denuncias. O cadastro usa POST /grc/denuncias/detalhada.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Denuncia {
  id: string
  codigoAcompanhamento?: string | null
  titulo?: string | null
  status?: string | null
  prioridade?: string | null
  anonima?: boolean
  dataRegistro?: string | null
}

const router = useRouter()
const lista = useApiList<Denuncia>('/grc/denuncias', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Denuncia>[] = [
  { key: 'codigoAcompanhamento', label: 'Código', sortable: true, width: '150px' },
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'prioridade', label: 'Prioridade', sortable: true, align: 'center', width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'anonima', label: 'Anônima', align: 'center', width: '100px' },
  { key: 'dataRegistro', label: 'Registro', sortable: true, width: '160px' }
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
    <PageToolbar title="Denúncias" subtitle="Canal de denúncias e investigações internas" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/denuncias/novo')">+ Nova denúncia</button>
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
      empty-text="Nenhuma denúncia registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-prioridade="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-anonima="{ value }">
        <span class="badge" :class="value ? 'badge-info' : 'badge-success'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #cell-dataRegistro="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
