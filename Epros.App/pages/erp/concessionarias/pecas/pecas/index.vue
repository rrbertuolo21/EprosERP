<script setup lang="ts">
/**
 * Peças / Peças — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface Peca {
  id: string
  produtoId?: string
  familiaTecnica?: string | null
  criticidade?: string | null
}

const colunas: DataTableColumn<Peca>[] = [
  { key: 'produtoId', label: 'Produto' },
  { key: 'familiaTecnica', label: 'Família técnica' },
  { key: 'criticidade', label: 'Criticidade' }
]

const campos: CampoForm[] = [
  // TODO: sem endpoint de listagem de produtos no digest — UUID por ora.
  { key: 'produtoId', label: 'Produto (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do produto.' },
  { key: 'familiaTecnica', label: 'Família técnica', tipo: 'text' },
  { key: 'criticidade', label: 'Criticidade', tipo: 'text', hint: 'Ex.: alta, média, baixa.' }
]
</script>

<template>
  <RecursoLista
    title="Peças"
    subtitle="Gestão de peças — catálogo técnico"
    path="/concessionarias/pecas/pecas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova peça"
  />
</template>
