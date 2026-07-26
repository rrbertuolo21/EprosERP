<script setup lang="ts">
/**
 * Listagem de Sacados — Serviços Financeiros.
 * GET /servicos-financeiros/sacados, POST, PUT/{id}. Ação: bloqueio (POST /{id}/bloqueio).
 * Sem GET/{id}: edição via listagem.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface Sacado {
  id: string
  nome?: string | null
  documento?: string | null
  cidade?: string | null
  uf?: string | null
  email?: string | null
  bloqueado?: boolean | null
}
interface SacadoFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Sacado, SacadoFiltros>('/servicos-financeiros/sacados', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Sacado>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'documento', label: 'Documento', sortable: false },
  { key: 'cidade', label: 'Cidade', sortable: false },
  { key: 'uf', label: 'UF', sortable: false, width: '70px' },
  { key: 'email', label: 'E-mail', sortable: false }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome ou documento...', grow: true }
]

function novo() {
  router.push('/erp/financeiro/servicos-financeiros/sacados/novo')
}
function editar(item: Sacado) {
  router.push(`/erp/financeiro/servicos-financeiros/sacados/${item.id}`)
}
async function bloquear(item: Sacado) {
  const ok = await confirmRef.value!.open('Bloquear/desbloquear sacado', 'Confirma a alteração de bloqueio deste sacado?')
  if (!ok) return
  try {
    await useApi('/servicos-financeiros/sacados/{id}/bloqueio', { method: 'POST', params: { id: item.id } })
    toast.success('Bloqueio atualizado.')
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
    <PageToolbar title="Sacados" subtitle="Sacados das cobranças" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo sacado</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum sacado cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Bloqueio" @click.stop="bloquear(row)">Bloqueio</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
