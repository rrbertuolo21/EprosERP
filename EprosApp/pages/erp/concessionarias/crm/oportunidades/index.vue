<script setup lang="ts">
/**
 * CRM / Oportunidades — lista + criação (GET lista + POST criar).
 * Contrato: /concessionarias/crm/oportunidades. Sem GET/{id}, PUT ou DELETE.
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface Oportunidade {
  id: string
  prospectId?: string
  valorEstimado?: number | null
  probabilidade?: number | null
}

const colunas: DataTableColumn<Oportunidade>[] = [
  { key: 'prospectId', label: 'Prospect' },
  { key: 'valorEstimado', label: 'Valor estimado', align: 'right', formatter: formatMoeda },
  { key: 'probabilidade', label: 'Probabilidade (%)', align: 'right' }
]

const campos: CampoForm[] = [
  { key: 'prospectId', label: 'Prospect', tipo: 'fk', fkPath: '/concessionarias/crm/prospects', obrigatorio: true },
  { key: 'valorEstimado', label: 'Valor estimado', tipo: 'money' },
  { key: 'probabilidade', label: 'Probabilidade (%)', tipo: 'percent', hint: 'Percentual estimado de conversão.' }
]
</script>

<template>
  <RecursoLista
    title="Oportunidades"
    subtitle="CRM — oportunidades de venda"
    path="/concessionarias/crm/oportunidades"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova oportunidade"
  />
</template>
