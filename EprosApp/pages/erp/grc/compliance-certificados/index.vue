<script setup lang="ts">
/**
 * Listagem de Certificados Digitais — GRC / Compliance Regulatório.
 * Fonte: GET /api/v1/grc/compliance/certificados, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useMask } from '~/composables/useMask'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Certificado {
  id: string
  cnpj?: string | null
  serial?: string | null
  tipo?: string | null
  origem?: string | null
  dataValidade?: string | null
  status?: string | null
}

const router = useRouter()
const { maskCpfCnpj } = useMask()
const lista = useApiList<Certificado>('/grc/compliance/certificados', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Certificado>[] = [
  { key: 'cnpj', label: 'CNPJ', sortable: true, width: '170px' },
  { key: 'serial', label: 'Serial', sortable: true },
  { key: 'tipo', label: 'Tipo', align: 'center', width: '90px' },
  { key: 'origem', label: 'Origem', width: '160px' },
  { key: 'dataValidade', label: 'Validade', sortable: true, width: '140px' },
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
    <PageToolbar title="Certificados Digitais" subtitle="Certificados A1/A3 e sua validade" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/compliance-certificados/novo')">+ Novo certificado</button>
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
      empty-text="Nenhum certificado cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-cnpj="{ value }">
        <span v-if="value">{{ maskCpfCnpj(String(value)) }}</span>
      </template>
      <template #cell-dataValidade="{ value }">{{ formatarData(value) }}</template>
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
