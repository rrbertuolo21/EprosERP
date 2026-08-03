<script setup lang="ts">
/**
 * Headcount — RH / Planejamento.
 * Fonte: GET /rh/planejamento/headcount (versões/plano) + POST /rh/planejamento/headcount/itens.
 * Não há GET/{id}/PUT/DELETE — a tela lista e permite adicionar itens ao headcount.
 * versaoId/departamentoId/cargoId não têm endpoint de listagem no digest — UUID manual.
 */
import { ref, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'

definePageMeta({ layout: 'default' })

interface Headcount {
  id: string
  versao?: string | null
  departamentoId?: string | null
  quantidadeAutorizada?: number | null
  custoPrevisto?: number | null
  observacao?: string | null
}
interface Filtros { busca?: string }

const toast = useToast()
const lista = useApiList<Headcount, Filtros>('/rh/planejamento/headcount', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Headcount>[] = [
  { key: 'versao', label: 'Versão', sortable: false },
  { key: 'quantidadeAutorizada', label: 'Qtd. autorizada', sortable: false, align: 'right' },
  { key: 'custoPrevisto', label: 'Custo previsto', sortable: false, align: 'right' },
  { key: 'observacao', label: 'Observação', sortable: false }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Versão...', grow: true }
]
const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

const itemVisivel = ref(false)
const salvandoItem = ref(false)
const item = ref<{
  versaoId: string
  departamentoId: string | null
  cargoId: string | null
  quantidadeAutorizada: number | null
  custoPrevisto: number | null
  observacao: string | null
}>({
  versaoId: '',
  departamentoId: null,
  cargoId: null,
  quantidadeAutorizada: null,
  custoPrevisto: null,
  observacao: null
})

function abrirItem() {
  item.value = { versaoId: '', departamentoId: null, cargoId: null, quantidadeAutorizada: null, custoPrevisto: null, observacao: null }
  itemVisivel.value = true
}

async function salvarItem() {
  if (!item.value.versaoId) {
    toast.error('Informe a versão (UUID) do headcount.')
    return
  }
  salvandoItem.value = true
  try {
    await useApi('/rh/planejamento/headcount/itens', {
      method: 'POST',
      body: {
        ...item.value,
        quantidadeAutorizada: item.value.quantidadeAutorizada != null ? Number(item.value.quantidadeAutorizada) : null
      }
    })
    toast.success('Item de headcount adicionado.')
    itemVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoItem.value = false
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Headcount" subtitle="Planejamento de quadro autorizado" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirItem">+ Adicionar item</button>
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
      row-key="id"
      empty-text="Nenhum item de headcount encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-custoPrevisto="{ value }">
        <span v-if="value != null">{{ brl.format(Number(value)) }}</span>
      </template>
    </DataTable>

    <AppDialog v-model="itemVisivel" title="Adicionar item de headcount" width="480px" persistent>
      <div class="dlg-form">
        <!-- TODO: sem endpoint de listagem para Versão/Departamento/Cargo no digest — UUID manual. -->
        <TextField v-model="item.versaoId" label="Versão (UUID)" required placeholder="UUID" />
        <TextField v-model="item.departamentoId" label="Departamento (UUID)" placeholder="UUID" />
        <TextField v-model="item.cargoId" label="Cargo (UUID)" placeholder="UUID" />
        <TextField v-model="item.quantidadeAutorizada" label="Quantidade autorizada" type="number" />
        <MoneyInput v-model="item.custoPrevisto" label="Custo previsto" />
        <TextField v-model="item.observacao" label="Observação" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoItem" @click="itemVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoItem" @click="salvarItem">
          <span v-if="salvandoItem" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
