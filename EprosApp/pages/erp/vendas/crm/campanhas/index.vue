<script setup lang="ts">
/**
 * CRM Comercial — Campanhas. Contrato: GET/POST `/vendas/crm/campanhas` (lista + criação).
 * Reaproveita o RecursoLista (lista paginada + diálogo de criação declarativo).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatMoeda, formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Campanha {
  id: string
  nome?: string | null
  tipo?: string | null
  status?: string | null
  dataFim?: string | null
  orcamento?: number | null
}

const colunas: DataTableColumn<Campanha>[] = [
  { key: 'nome', label: 'Campanha' },
  { key: 'tipo', label: 'Tipo' },
  { key: 'status', label: 'Status' },
  { key: 'dataFim', label: 'Encerra em', formatter: formatData },
  { key: 'orcamento', label: 'Orçamento', align: 'right', formatter: formatMoeda }
]

const campos: CampoForm[] = [
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'tipo', label: 'Tipo', tipo: 'text', obrigatorio: true, hint: 'Ex.: E-mail, Evento, Anúncio.' },
  { key: 'status', label: 'Status', tipo: 'text', obrigatorio: true, hint: 'Ex.: Planejada, Ativa, Encerrada.' },
  { key: 'dataInicio', label: 'Início', tipo: 'date' },
  { key: 'dataFim', label: 'Fim', tipo: 'date', obrigatorio: true },
  { key: 'frequencia', label: 'Frequência', tipo: 'text' },
  { key: 'orcamento', label: 'Orçamento', tipo: 'money' },
  { key: 'receitaEsperada', label: 'Receita esperada', tipo: 'money' },
  { key: 'objetivo', label: 'Objetivo', tipo: 'textarea' }
]
</script>

<template>
  <RecursoLista
    title="Campanhas"
    subtitle="CRM Comercial — campanhas de marketing"
    path="/vendas/crm/campanhas"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Nova campanha"
    :com-busca="false"
  />
</template>
