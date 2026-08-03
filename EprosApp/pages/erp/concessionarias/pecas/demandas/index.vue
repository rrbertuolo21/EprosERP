<script setup lang="ts">
/**
 * Peças / Demandas — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatNumero } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface DemandaPeca {
  id: string
  pecaId?: string
  origemTipo?: string | null
  quantidade?: number
  prazo?: string
}

const colunas: DataTableColumn<DemandaPeca>[] = [
  { key: 'pecaId', label: 'Peça' },
  { key: 'origemTipo', label: 'Origem' },
  { key: 'quantidade', label: 'Quantidade', align: 'right', formatter: formatNumero },
  { key: 'prazo', label: 'Prazo', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'pecaId', label: 'Peça', tipo: 'fk', fkPath: '/concessionarias/pecas/pecas', obrigatorio: true },
  // TODO: sem endpoint de listagem no digest — UUID por ora (local/origem/item).
  { key: 'localId', label: 'Local (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do local.' },
  { key: 'origemTipo', label: 'Tipo de origem', tipo: 'text' },
  { key: 'origemId', label: 'Origem (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID da origem.' },
  { key: 'itemOrigemId', label: 'Item de origem (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do item de origem.' },
  { key: 'quantidade', label: 'Quantidade', tipo: 'quantity', obrigatorio: true },
  { key: 'prazo', label: 'Prazo', tipo: 'datetime', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Demandas de peças"
    subtitle="Gestão de peças — demandas"
    path="/concessionarias/pecas/demandas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova demanda"
  />
</template>
