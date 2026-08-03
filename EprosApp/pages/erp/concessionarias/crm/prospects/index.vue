<script setup lang="ts">
/**
 * CRM / Prospects — lista + criação (GET lista + POST criar).
 * FKs sem endpoint de listagem no digest (contato/unidade/vendedor) ficam como
 * entrada de UUID (TODO: trocar por SelectField quando houver rota de listagem).
 */
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ layout: 'default' })

interface Prospect {
  id: string
  contactId?: string
  unidadeId?: string
  origem?: string | null
  vendedorId?: string
}

const colunas: DataTableColumn<Prospect>[] = [
  { key: 'origem', label: 'Origem' },
  { key: 'contactId', label: 'Contato' },
  { key: 'unidadeId', label: 'Unidade' },
  { key: 'vendedorId', label: 'Vendedor' }
]

const campos: CampoForm[] = [
  // TODO: sem endpoint de listagem no digest — usar UUID por ora.
  { key: 'contactId', label: 'Contato (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do contato.' },
  { key: 'unidadeId', label: 'Unidade (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID da unidade.' },
  { key: 'origem', label: 'Origem', tipo: 'text' },
  { key: 'vendedorId', label: 'Vendedor (UUID)', tipo: 'text', obrigatorio: true, hint: 'UUID do vendedor.' }
]
</script>

<template>
  <RecursoLista
    title="Prospects"
    subtitle="CRM — prospecção de clientes"
    path="/concessionarias/crm/prospects"
    :columns="colunas"
    :campos="campos"
    rotulo-novo="+ Novo prospect"
  />
</template>
