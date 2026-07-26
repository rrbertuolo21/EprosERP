<script setup lang="ts">
/**
 * CRM / Test-drives — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface TestDrive {
  id: string
  oportunidadeId?: string
  veiculoDemonstracaoId?: string
  inicio?: string
  fim?: string
}

const colunas: DataTableColumn<TestDrive>[] = [
  { key: 'oportunidadeId', label: 'Oportunidade' },
  { key: 'veiculoDemonstracaoId', label: 'Veículo demonstração' },
  { key: 'inicio', label: 'Início', formatter: formatDataHora },
  { key: 'fim', label: 'Fim', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'oportunidadeId', label: 'Oportunidade', tipo: 'fk', fkPath: '/concessionarias/crm/oportunidades', obrigatorio: true },
  { key: 'veiculoDemonstracaoId', label: 'Veículo de demonstração', tipo: 'fk', fkPath: '/veiculos', obrigatorio: true },
  { key: 'inicio', label: 'Início', tipo: 'datetime', obrigatorio: true },
  { key: 'fim', label: 'Fim', tipo: 'datetime', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Test-drives"
    subtitle="CRM — agendamento de test-drives"
    path="/concessionarias/crm/test-drives"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo test-drive"
  />
</template>
