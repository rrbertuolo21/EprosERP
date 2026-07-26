<script setup lang="ts">
/**
 * Finanças / Contratos — lista + criação (GET lista + POST criar).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface ContratoFinanceiro {
  id: string
  propostaId?: string | null
  vendaId?: string
  numeroContrato?: string | null
  condicaoFinalJson?: string | null
}

const colunas: DataTableColumn<ContratoFinanceiro>[] = [
  { key: 'numeroContrato', label: 'Nº contrato' },
  { key: 'vendaId', label: 'Venda' },
  { key: 'propostaId', label: 'Proposta' }
]

const campos: CampoForm[] = [
  { key: 'propostaId', label: 'Proposta', tipo: 'fk', fkPath: '/concessionarias/vendas/propostas' },
  { key: 'vendaId', label: 'Venda', tipo: 'fk', fkPath: '/dms/vendas', obrigatorio: true },
  { key: 'numeroContrato', label: 'Número do contrato', tipo: 'text' },
  { key: 'condicaoFinalJson', label: 'Condição final (JSON)', tipo: 'textarea', hint: 'Payload JSON com as condições finais.' }
]
</script>

<template>
  <RecursoLista
    title="Contratos financeiros"
    subtitle="Finanças — contratos de financiamento"
    path="/concessionarias/financas/contratos"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo contrato"
  />
</template>
