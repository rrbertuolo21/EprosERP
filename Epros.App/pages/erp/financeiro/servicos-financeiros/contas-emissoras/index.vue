<script setup lang="ts">
/**
 * Listagem de Contas Emissoras (boletos) — Serviços Financeiros.
 * GET /servicos-financeiros/contas-emissoras, POST, PUT/{id}, POST /{id}/ativar.
 * Sem GET/{id}: edição via listagem.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface ContaEmissora {
  id: string
  nomeBanco?: string | null
  agencia?: string | null
  conta?: string | null
  carteira?: string | null
  convenio?: string | null
  ativa?: boolean | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<ContaEmissora>('/servicos-financeiros/contas-emissoras', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<ContaEmissora>[] = [
  { key: 'nomeBanco', label: 'Banco', sortable: true },
  { key: 'agencia', label: 'Agência', sortable: false },
  { key: 'conta', label: 'Conta', sortable: false },
  { key: 'carteira', label: 'Carteira', sortable: false },
  { key: 'convenio', label: 'Convênio', sortable: false },
  { key: 'ativa', label: 'Ativa', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/servicos-financeiros/contas-emissoras/novo')
}
function editar(item: ContaEmissora) {
  router.push(`/erp/financeiro/servicos-financeiros/contas-emissoras/${item.id}`)
}
async function ativar(item: ContaEmissora) {
  try {
    await useApi('/servicos-financeiros/contas-emissoras/{id}/ativar', { method: 'POST', params: { id: item.id } })
    toast.success('Conta emissora ativada.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Contas Emissoras" subtitle="Configuração bancária para emissão de boletos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova conta emissora</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma conta emissora cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #cell-ativa="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Ativar" @click.stop="ativar(row)">Ativar</button>
      </template>
    </DataTable>
  </div>
</template>
