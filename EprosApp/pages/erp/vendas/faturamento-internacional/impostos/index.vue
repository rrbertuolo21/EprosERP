<script setup lang="ts">
/**
 * Faturamento Comercial Internacional (FCI) — Impostos.
 * Contrato real: GET/POST `/vendas/faturamento-internacional/impostos`
 * (CriarFciImpostoCommand: { nome, aliquota, ativo }). Lista + criação via RecursoLista.
 * Apresentação — sem regra nova.
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Imposto {
  id: string
  nome?: string | null
  aliquota?: number | null
  ativo?: boolean | null
}

const colunas: DataTableColumn<Imposto>[] = [
  { key: 'nome', label: 'Imposto' },
  { key: 'aliquota', label: 'Alíquota (%)', align: 'right' },
  { key: 'ativo', label: 'Situação', formatter: (v) => (v ? 'Ativo' : 'Inativo') }
]

const campos: CampoForm[] = [
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'aliquota', label: 'Alíquota (%)', tipo: 'percent', obrigatorio: true },
  { key: 'ativo', label: 'Ativo', tipo: 'boolean' }
]
</script>

<template>
  <RecursoLista
    title="Faturamento internacional — impostos"
    subtitle="FCI — impostos aplicáveis"
    path="/vendas/faturamento-internacional/impostos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo imposto"
    :com-busca="false"
  />
</template>
