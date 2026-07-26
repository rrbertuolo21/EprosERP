<script setup lang="ts">
/**
 * Listagem de Revisões de Confiabilidade — Manutenção / Confiabilidade / Revisões.
 * Fonte: GET /manutencao/confiabilidade/revisoes.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { rotuloStatusRegistro } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface Revisao {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
  criticidadeOperacional?: string | null
}

const router = useRouter()

const lista = useApiList<Revisao>('/manutencao/confiabilidade/revisoes', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Revisao>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'criticidadeOperacional', label: 'Criticidade', sortable: true, width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' }
]

function novo() {
  router.push('/erp/manutencao/confiabilidade/revisoes/novo')
}
function abrir(item: Revisao) {
  router.push(`/erp/manutencao/confiabilidade/revisoes/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Revisões de confiabilidade" subtitle="RCM/FMEA: modos de falha, indicadores e recomendações" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova revisão</button>
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
      empty-text="Nenhuma revisão encontrada."
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
