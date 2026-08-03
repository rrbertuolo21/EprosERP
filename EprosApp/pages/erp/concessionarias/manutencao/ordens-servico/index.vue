<script setup lang="ts">
/**
 * Manutenção / Ordens de serviço — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatDataHora, formatNumero } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface OrdemServico {
  id: string
  veiculoId?: string
  chassiVin?: string | null
  placa?: string | null
  quilometragemEntrada?: number
  dataAbertura?: string
  previsaoEntrega?: string | null
}

const colunas: DataTableColumn<OrdemServico>[] = [
  { key: 'placa', label: 'Placa' },
  { key: 'chassiVin', label: 'Chassi/VIN' },
  { key: 'quilometragemEntrada', label: 'Km entrada', align: 'right', formatter: formatNumero },
  { key: 'dataAbertura', label: 'Abertura', formatter: formatDataHora },
  { key: 'previsaoEntrega', label: 'Previsão', formatter: formatDataHora }
]

const campos: CampoForm[] = [
  // TODO: sem endpoint de listagem no digest — UUID por ora (pessoa/consultor/unidade/produto).
  { key: 'pessoaId', label: 'Cliente (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID da pessoa/cliente.' },
  { key: 'produtoId', label: 'Produto (UUID)', tipo: 'text', hint: 'UUID do produto (opcional).' },
  { key: 'veiculoId', label: 'Veículo', tipo: 'fk', fkPath: '/veiculos', obrigatorio: true },
  { key: 'chassiVin', label: 'Chassi/VIN', tipo: 'text' },
  { key: 'placa', label: 'Placa', tipo: 'text' },
  { key: 'quilometragemEntrada', label: 'Km de entrada', tipo: 'quantity', obrigatorio: true },
  { key: 'consultorId', label: 'Consultor (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do consultor.' },
  { key: 'unidadeId', label: 'Unidade (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID da unidade.' },
  { key: 'dataAbertura', label: 'Data de abertura', tipo: 'datetime', obrigatorio: true },
  { key: 'previsaoEntrega', label: 'Previsão de entrega', tipo: 'datetime' }
]
</script>

<template>
  <RecursoLista
    title="Ordens de serviço"
    subtitle="Manutenção — ordens de serviço da oficina"
    path="/concessionarias/manutencao/ordens-servico"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova ordem"
  />
</template>
