<script setup lang="ts">
/**
 * DMS / Vendas de veículo — lista + criação (GET lista + POST criar).
 * Rota: /dms/vendas.
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface VendaVeiculo {
  id: string
  chassi?: string | null
  modelo?: string | null
  marca?: string | null
  anoModelo?: number
  precoVenda?: number
  clienteNome?: string | null
}

const colunas: DataTableColumn<VendaVeiculo>[] = [
  { key: 'chassi', label: 'Chassi' },
  { key: 'marca', label: 'Marca' },
  { key: 'modelo', label: 'Modelo' },
  { key: 'anoModelo', label: 'Ano', align: 'right' },
  { key: 'precoVenda', label: 'Preço de venda', align: 'right', formatter: formatMoeda },
  { key: 'clienteNome', label: 'Cliente' }
]

const campos: CampoForm[] = [
  { key: 'chassi', label: 'Chassi', tipo: 'text' },
  { key: 'modelo', label: 'Modelo', tipo: 'text' },
  { key: 'marca', label: 'Marca', tipo: 'text' },
  { key: 'anoModelo', label: 'Ano/modelo', tipo: 'int', obrigatorio: true },
  { key: 'precoVenda', label: 'Preço de venda', tipo: 'money', obrigatorio: true },
  { key: 'clienteNome', label: 'Nome do cliente', tipo: 'text' }
]
</script>

<template>
  <RecursoLista
    title="Vendas de veículo"
    subtitle="DMS — vendas registradas"
    path="/dms/vendas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova venda"
  />
</template>
