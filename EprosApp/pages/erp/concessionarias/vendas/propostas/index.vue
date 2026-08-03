<script setup lang="ts">
/**
 * Vendas / Propostas — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface Proposta {
  id: string
  oportunidadeId?: string
  clienteId?: string
  estoqueVeiculoId?: string | null
  validaAte?: string
  valorVeiculo?: number
  desconto?: number
}

const colunas: DataTableColumn<Proposta>[] = [
  { key: 'oportunidadeId', label: 'Oportunidade' },
  { key: 'validaAte', label: 'Válida até', formatter: formatDataHora },
  { key: 'valorVeiculo', label: 'Valor veículo', align: 'right', formatter: formatMoeda },
  { key: 'desconto', label: 'Desconto', align: 'right', formatter: formatMoeda }
]

const campos: CampoForm[] = [
  { key: 'oportunidadeId', label: 'Oportunidade', tipo: 'fk', fkPath: '/concessionarias/crm/oportunidades', obrigatorio: true },
  // TODO: sem endpoint de listagem de clientes no digest — UUID por ora.
  { key: 'clienteId', label: 'Cliente (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do cliente.' },
  { key: 'estoqueVeiculoId', label: 'Veículo em estoque', tipo: 'fk', fkPath: '/concessionarias/vendas/estoque' },
  { key: 'validaAte', label: 'Válida até', tipo: 'datetime', obrigatorio: true },
  { key: 'valorVeiculo', label: 'Valor do veículo', tipo: 'money', obrigatorio: true },
  { key: 'desconto', label: 'Desconto', tipo: 'money', obrigatorio: true }
]
</script>

<template>
  <RecursoLista
    title="Propostas"
    subtitle="Vendas — propostas comerciais"
    path="/concessionarias/vendas/propostas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova proposta"
  />
</template>
