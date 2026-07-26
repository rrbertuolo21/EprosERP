<script setup lang="ts">
/**
 * Listagem de Parâmetros do Canal de Denúncias — GRC.
 * Fonte: GET /api/v1/grc/denuncias/parametros, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Parametro {
  id: string
  chave?: string | null
  valorJson?: string | null
  ativo?: boolean
  dataAlteracao?: string | null
}

const router = useRouter()
const lista = useApiList<Parametro>('/grc/denuncias/parametros', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Parametro>[] = [
  { key: 'chave', label: 'Chave', sortable: true, width: '220px' },
  { key: 'valorJson', label: 'Valor (JSON)' },
  { key: 'ativo', label: 'Status', align: 'center', width: '110px' },
  { key: 'dataAlteracao', label: 'Alteração', sortable: true, width: '160px' }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Parâmetros do Canal de Denúncias" subtitle="Configurações (chave/valor) do canal" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/denuncia-parametros/novo')">+ Novo parâmetro</button>
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
      empty-text="Nenhum parâmetro configurado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Ativo' : 'Inativo' }}</span>
      </template>
      <template #cell-dataAlteracao="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
