<script setup lang="ts">
/**
 * Finanças / Simulações — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default' })

interface Simulacao {
  id: string
  jornadaId?: string
  precoVeiculo?: number
  entrada?: number
  saldo?: number
  prazoQuantidade?: number
  prazoUnidade?: string | null
}

const colunas: DataTableColumn<Simulacao>[] = [
  { key: 'jornadaId', label: 'Jornada' },
  { key: 'precoVeiculo', label: 'Preço veículo', align: 'right', formatter: formatMoeda },
  { key: 'entrada', label: 'Entrada', align: 'right', formatter: formatMoeda },
  { key: 'saldo', label: 'Saldo', align: 'right', formatter: formatMoeda },
  { key: 'prazoQuantidade', label: 'Prazo', align: 'right' }
]

const campos: CampoForm[] = [
  { key: 'jornadaId', label: 'Jornada', tipo: 'fk', fkPath: '/concessionarias/financas/jornadas', obrigatorio: true },
  { key: 'chaveIdempotencia', label: 'Chave de idempotência', tipo: 'text' },
  { key: 'precoVeiculo', label: 'Preço do veículo', tipo: 'money', obrigatorio: true },
  { key: 'entrada', label: 'Entrada', tipo: 'money', obrigatorio: true },
  { key: 'saldo', label: 'Saldo a financiar', tipo: 'money', obrigatorio: true },
  { key: 'prazoQuantidade', label: 'Prazo (qtd.)', tipo: 'int', obrigatorio: true },
  { key: 'prazoUnidade', label: 'Unidade do prazo', tipo: 'text', hint: 'Ex.: meses.' },
  { key: 'origemVersao', label: 'Origem/versão', tipo: 'text' },
  { key: 'memoriaJson', label: 'Memória de cálculo (JSON)', tipo: 'textarea' }
]
</script>

<template>
  <RecursoLista
    title="Simulações"
    subtitle="Finanças — simulações de financiamento"
    path="/concessionarias/financas/simulacoes"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova simulação"
  />
</template>
