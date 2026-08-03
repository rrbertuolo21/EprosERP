<script setup lang="ts">
/**
 * Listagem de Contratos de Serviço — Imobiliária / Contratos de Serviço.
 *
 * Fonte: GET /imobiliaria/contratos-servico (lista completa, sem paginação/filtro server-side).
 * Ações: novo (POST) e excluir (DELETE /{id}). A API não expõe detalhe nem PUT — sem edição.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface ContratoServico {
  id: string
  proprietarioId?: string | null
  imovelId?: string | null
  descricao?: string | null
  vigenciaInicio?: string | null
  vigenciaFim?: string | null
  remuneracao?: number | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<ContratoServico>('/imobiliaria/contratos-servico', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<ContratoServico>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'vigenciaInicio', label: 'Início vigência', sortable: true, width: '150px' },
  { key: 'vigenciaFim', label: 'Fim vigência', sortable: true, width: '150px' },
  { key: 'remuneracao', label: 'Remuneração', sortable: true, align: 'right', width: '150px' }
]

function formatarData(v: unknown): string {
  if (!v) return '—'
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('pt-BR')
}

function formatarMoeda(v: unknown): string {
  if (v === null || v === undefined) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v))
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ContratoServico | null>(null)

function novoContrato() {
  router.push('/erp/imobiliaria/contratos-servico/novo')
}

function pedirExclusao(item: ContratoServico) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/imobiliaria/contratos-servico/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Contrato de serviço excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Contratos de Serviço" subtitle="Administração de imóveis contratada pelo proprietário" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoContrato">+ Novo contrato</button>
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
      empty-text="Nenhum contrato de serviço encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-vigenciaInicio="{ value }">{{ formatarData(value) }}</template>
      <template #cell-vigenciaFim="{ value }">{{ formatarData(value) }}</template>
      <template #cell-remuneracao="{ value }">{{ formatarMoeda(value) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
