<script setup lang="ts">
/**
 * Desenvolvimento de rede / Contratos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface ContratoRede {
  id: string
  redeNoId?: string | null
  tipo?: string | null
  inicio?: string
  fim?: string
}

const colunas: DataTableColumn<ContratoRede>[] = [
  { key: 'tipo', label: 'Tipo' },
  { key: 'redeNoId', label: 'Nó da rede' },
  { key: 'inicio', label: 'Início', formatter: formatDataHora },
  { key: 'fim', label: 'Fim', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'redeNoId', label: 'Nó da rede', tipo: 'fk', fkPath: '/concessionarias/desenvolvimento/rede' },
  { key: 'tipo', label: 'Tipo', tipo: 'text' },
  { key: 'inicio', label: 'Início', tipo: 'datetime', obrigatorio: true },
  { key: 'fim', label: 'Fim', tipo: 'datetime', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Contratos de rede"
    subtitle="Desenvolvimento de concessionárias — contratos"
    path="/concessionarias/desenvolvimento/contratos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo contrato"
  />
</template>
