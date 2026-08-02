<script setup lang="ts">
/**
 * Grupos de Consolidação — Consolidação / Grupos.
 *
 * Contrato:
 *   GET  /consolidacao/grupos    (lista paginada)
 *   POST /consolidacao/grupos
 *   GET  /consolidacao/grupos/{id}
 *   PUT  /consolidacao/grupos/{id}
 * Não há DELETE de grupo. Empresas, balancetes, demonstrativos e eliminações são
 * geridos no detalhe do grupo (`consolidacao/[id].vue`).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { estadoConsolidacaoLabel, estadoConsolidacaoClasse } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface GrupoConsolidacao {
  id: string
  codigo?: string | null
  nome?: string | null
  descricao?: string | null
  situacao: number
}

const router = useRouter()

const lista = useApiList<GrupoConsolidacao>('/consolidacao/grupos', {
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<GrupoConsolidacao>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'nome', label: 'Nome', sortable: false },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center', width: '130px' }
]

function novo() {
  router.push('/erp/contabilidade/consolidacao/novo')
}

function editar(item: GrupoConsolidacao) {
  router.push(`/erp/contabilidade/consolidacao/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Grupos de Consolidação"
      subtitle="Grupos de empresas para consolidação contábil (balancetes, demonstrativos e eliminações)"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo grupo</button>
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
      empty-text="Nenhum grupo de consolidação encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="editar"
    >
      <template #cell-situacao="{ value }">
        <span class="badge" :class="`badge-${estadoConsolidacaoClasse(Number(value))}`">
          {{ estadoConsolidacaoLabel(Number(value)) }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="editar(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
