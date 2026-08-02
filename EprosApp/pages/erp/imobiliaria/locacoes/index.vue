<script setup lang="ts">
/**
 * Listagem de Locações — Imobiliária / Locações.
 *
 * Fonte: GET /imobiliaria/locacoes?periodoDe&periodoAte (filtro server-side por período — RN-017).
 * Ações: novo (POST), excluir (DELETE /{id}) e Resumo do aluguel (GET /{id}/resumo-aluguel).
 * A API não expõe detalhe editável nem PUT — sem edição.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'default' })

interface Locacao {
  id: string
  imovelId?: string | null
  periodoInicial?: string | null
  periodoFinal?: string | null
  valor?: number | null
  vencimento?: number | null
  status?: number | string | null
}

interface LocacaoFiltros {
  periodoDe?: string
  periodoAte?: string
}

interface ResumoAluguel {
  locacaoId: string
  imovelId?: string | null
  valor?: number | null
  vencimento?: number | null
  periodoInicial?: string | null
  periodoFinal?: string | null
  status?: string | null
  locatarios?: string[]
  fiadores?: string[]
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Locacao, LocacaoFiltros>('/imobiliaria/locacoes', {
  filtrosIniciais: { periodoDe: '', periodoAte: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Locacao>[] = [
  { key: 'periodoInicial', label: 'Início', sortable: true, width: '130px' },
  { key: 'periodoFinal', label: 'Fim', sortable: true, width: '130px' },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', width: '140px' },
  { key: 'vencimento', label: 'Vencimento (dia)', sortable: true, align: 'center', width: '150px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

const camposFiltro: FilterField[] = [
  { key: 'periodoDe', label: 'Período de', type: 'date' },
  { key: 'periodoAte', label: 'Período até', type: 'date' }
]

const STATUS_LOCACAO: Record<string, string> = {
  '0': 'Em elaboração',
  '1': 'Vigente',
  '2': 'Encerrada',
  '3': 'Cancelada',
  EmElaboracao: 'Em elaboração',
  Vigente: 'Vigente',
  Encerrada: 'Encerrada',
  Cancelada: 'Cancelada'
}

function rotularStatus(v: unknown): string {
  if (v === null || v === undefined) return '—'
  return STATUS_LOCACAO[String(v)] ?? String(v)
}

function formatarData(v: unknown): string {
  if (!v) return '—'
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('pt-BR')
}

function formatarMoeda(v: unknown): string {
  if (v === null || v === undefined) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(v))
}

// ---- Exclusão ----
const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Locacao | null>(null)

function pedirExclusao(item: Locacao) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/imobiliaria/locacoes/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Locação excluída com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

// ---- Resumo do aluguel ----
const resumoVisivel = ref(false)
const carregandoResumo = ref(false)
const resumo = ref<ResumoAluguel | null>(null)

async function abrirResumo(item: Locacao) {
  resumoVisivel.value = true
  carregandoResumo.value = true
  resumo.value = null
  try {
    const resposta = await useApi(`/imobiliaria/locacoes/${item.id}/resumo-aluguel`)
    resumo.value = extrairDados<ResumoAluguel>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
    resumoVisivel.value = false
  } finally {
    carregandoResumo.value = false
  }
}

function novaLocacao() {
  router.push('/erp/imobiliaria/locacoes/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Locações" subtitle="Contratos de aluguel: período, valor, vencimento e partes" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaLocacao">+ Nova locação</button>
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
      empty-text="Nenhuma locação encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-periodoInicial="{ value }">{{ formatarData(value) }}</template>
      <template #cell-periodoFinal="{ value }">{{ formatarData(value) }}</template>
      <template #cell-valor="{ value }">{{ formatarMoeda(value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge">{{ rotularStatus(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Resumo do aluguel" @click.stop="abrirResumo(row)">Resumo</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.id || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />

    <AppDialog v-model="resumoVisivel" title="Resumo do aluguel" width="520px">
      <div v-if="carregandoResumo" class="resumo-loading">Carregando...</div>
      <dl v-else-if="resumo" class="resumo-grid">
        <div><dt>Valor</dt><dd>{{ formatarMoeda(resumo.valor) }}</dd></div>
        <div><dt>Vencimento (dia)</dt><dd>{{ resumo.vencimento ?? '—' }}</dd></div>
        <div><dt>Início</dt><dd>{{ formatarData(resumo.periodoInicial) }}</dd></div>
        <div><dt>Fim</dt><dd>{{ formatarData(resumo.periodoFinal) }}</dd></div>
        <div><dt>Status</dt><dd>{{ rotularStatus(resumo.status) }}</dd></div>
        <div><dt>Locatários</dt><dd>{{ resumo.locatarios?.length ?? 0 }}</dd></div>
        <div><dt>Fiadores</dt><dd>{{ resumo.fiadores?.length ?? 0 }}</dd></div>
      </dl>
      <p v-else class="resumo-loading">Nenhum dado disponível.</p>
    </AppDialog>
  </div>
</template>

<style scoped>
.resumo-loading { color: var(--text-secondary); font-size: 14px; padding: 8px 0; }
.resumo-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 14px; margin: 0; }
.resumo-grid dt { font-size: 12px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.03em; }
.resumo-grid dd { margin: 2px 0 0; font-size: 15px; font-weight: 600; color: var(--text-primary); }
</style>
