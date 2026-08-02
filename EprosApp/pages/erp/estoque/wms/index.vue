<script setup lang="ts">
/**
 * WMS — Armazéns (erp/estoque/wms).
 * Listagem paginada — `GET /estoque-wms-armazens?nome&cidade&ativo` (WmsArmazensController).
 * CRUD do armazém no detalhe [id].vue. O submódulo WMS sobe desabilitado no backend (ABAC nega
 * por padrão até a permissão ser semeada) — a tela consome os endpoints reais mesmo assim.
 *
 * Nota: endereços ricos de armazém e tarefas de separação (picking) ainda não têm endpoint próprio
 * neste controller; ficam pendentes de API (ver relatório).
 */
import { onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import { ref } from 'vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface Armazem {
  id: string
  nome: string
  endereco: string
  cidade: string
  cep: string
  telefone: string | null
  email: string | null
  ativo: boolean
  usuarioDonoId: string | null
  criadoEm: string
}
interface ArmazemFiltros extends Record<string, unknown> { nome?: string; cidade?: string; ativo?: boolean | null }

const toast = useToast()
const { formatarDataHora } = useHelper()

const lista = useApiList<Armazem, ArmazemFiltros>('/estoque-wms-armazens', { tamanhoPaginaInicial: 25 })

const camposFiltro: FilterField[] = [
  { key: 'nome', label: 'Nome', type: 'text', placeholder: 'Nome do armazém', grow: true },
  { key: 'cidade', label: 'Cidade', type: 'text', placeholder: 'Cidade' },
  { key: 'ativo', label: 'Ativo', type: 'boolean', placeholder: 'Somente ativos' }
]
const colunas: DataTableColumn<Armazem>[] = [
  { key: 'nome', label: 'Nome', sortable: false },
  { key: 'cidade', label: 'Cidade' },
  { key: 'endereco', label: 'Endereço' },
  { key: 'telefone', label: 'Telefone', formatter: (v) => (v as string) || '-' },
  { key: 'criadoEm', label: 'Criado em', width: '150px', formatter: (v) => formatarDataHora(v as string) },
  { key: 'ativo', label: 'Ativo', align: 'center', width: '90px' }
]
function normalizar(v: Record<string, unknown>): Partial<ArmazemFiltros> {
  return {
    nome: (v.nome as string) || undefined,
    cidade: (v.cidade as string) || undefined,
    ativo: v.ativo ? true : undefined
  }
}
function novo() { navigateTo('/erp/estoque/wms/novo') }
function editar(item: Armazem) { navigateTo(`/erp/estoque/wms/${item.id}`) }

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Armazem | null>(null)
function pedirExclusao(item: Armazem) { itemParaExcluir.value = item; excluirVisivel.value = true }
async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/estoque-wms-armazens/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Armazém excluído.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="WMS — Armazéns" subtitle="Cadastro de armazéns da gestão de armazém" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo armazém</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="(v) => lista.aplicarFiltros(normalizar(v as Record<string, unknown>))"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      row-key="id"
      empty-text="Nenhum armazém encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @row-click="editar"
    >
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-muted'">{{ value ? 'Ativo' : 'Inativo' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">✎</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">🗑</button>
      </template>
    </DataTable>

    <DeleteAlert v-model="excluirVisivel" :item-label="itemParaExcluir?.nome ?? ''" :loading="excluindo" @confirm="confirmarExclusao" />
  </div>
</template>
