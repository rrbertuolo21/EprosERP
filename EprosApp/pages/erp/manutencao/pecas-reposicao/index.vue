<script setup lang="ts">
/**
 * Listagem de Registros de Peças de Reposição — Manutenção / Peças de Reposição.
 * Fonte: GET /manutencao/pecas-reposicao.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { rotuloStatusRegistro } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface RegistroPeca {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
}

const router = useRouter()

const lista = useApiList<RegistroPeca>('/manutencao/pecas-reposicao', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<RegistroPeca>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' }
]

function novo() {
  router.push('/erp/manutencao/pecas-reposicao/novo')
}
function abrir(item: RegistroPeca) {
  router.push(`/erp/manutencao/pecas-reposicao/${item.id}`)
}
function irPoliticas() {
  router.push('/erp/manutencao/pecas-reposicao/politicas')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Peças de reposição" subtitle="Registros de peças, itens e políticas de estoque" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="irPoliticas">Política de estoque</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo registro</button>
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
      empty-text="Nenhum registro de peças encontrado."
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
