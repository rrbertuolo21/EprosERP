<script setup lang="ts">
/**
 * Vendas / Estoque de veículos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface EstoqueVeiculo {
  id: string
  veiculoId?: string
  chassiVin?: string | null
  custo?: number | null
  precoSugerido?: number | null
  dataEntrada?: string | null
}

const colunas: DataTableColumn<EstoqueVeiculo>[] = [
  { key: 'chassiVin', label: 'Chassi/VIN' },
  { key: 'custo', label: 'Custo', align: 'right', formatter: formatMoeda },
  { key: 'precoSugerido', label: 'Preço sugerido', align: 'right', formatter: formatMoeda },
  { key: 'dataEntrada', label: 'Entrada', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  { key: 'veiculoId', label: 'Veículo', tipo: 'fk', fkPath: '/veiculos', obrigatorio: true },
  { key: 'chassiVin', label: 'Chassi/VIN', tipo: 'text' },
  // TODO: sem endpoint de listagem de locais no digest — UUID por ora.
  { key: 'localId', label: 'Local (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do local.' },
  { key: 'custo', label: 'Custo', tipo: 'money' },
  { key: 'precoSugerido', label: 'Preço sugerido', tipo: 'money' },
  { key: 'dataEntrada', label: 'Data de entrada', tipo: 'datetime' }
]
</script>

<template>
  <RecursoLista
    title="Estoque de veículos"
    subtitle="Vendas — estoque para comercialização"
    path="/concessionarias/vendas/estoque"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo item de estoque"
  />
</template>
