<script setup lang="ts">
/**
 * Monitor de Importações — acompanhamento das execuções de importação tabular (upl_*).
 *
 * Contrato (UploadController, rota `api/v1/upload`):
 *   GET /upload/importacoes?tipoImportacao&status&pagina&tamanhoPagina
 *   GET /upload/importacoes/{id}
 *   GET /upload/importacoes/erros/{referenciaErro}
 */
import { ref, computed, onMounted } from 'vue'
import { useApi, extrairDados, extrairLista } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const toast = useToast()
const { formatarDataHora } = useHelper()

interface Importacao {
  id: string
  tipoImportacao?: string | null
  status: number
  totalLinhas?: number | null
  linhasProcessadas?: number | null
  linhasComErro?: number | null
  referenciaErro?: string | null
  criadoEm?: string | null
}

const STATUS: Record<number, string> = {
  0: 'Criada', 1: 'Validando', 2: 'Processando', 3: 'Parcial', 4: 'Finalizada', 5: 'Erro', 6: 'Desfeita'
}

const itens = ref<Importacao[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroStatus = ref<number | null>(null)

const detalhe = ref<Importacao | null>(null)
const erros = ref<Record<string, unknown>[]>([])
const carregandoErros = ref(false)

const colunas: DataTableColumn<Importacao>[] = [
  { key: 'tipoImportacao', label: 'Tipo', sortable: false },
  { key: 'criadoEm', label: 'Criada em', sortable: false, width: '170px' },
  { key: 'totalLinhas', label: 'Linhas', sortable: false, align: 'right', width: '100px' },
  { key: 'linhasComErro', label: 'Erros', sortable: false, align: 'right', width: '100px' },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '140px' }
]

const opcoesStatus: SelectOption[] = Object.entries(STATUS).map(([v, label]) => ({ label, value: Number(v) }))

async function buscar() {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroStatus.value !== null) query.status = filtroStatus.value
    const resp = await useApi('/upload/importacoes', { query })
    const d = extrairDados<{ itens?: Importacao[]; total?: number }>(resp)
    itens.value = Array.isArray(d) ? (d as Importacao[]) : d?.itens ?? []
    total.value = (d && !Array.isArray(d) ? d.total : undefined) ?? itens.value.length
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []; total.value = 0
  } finally {
    carregando.value = false
  }
}

async function abrirDetalhe(item: Importacao) {
  detalhe.value = item
  erros.value = []
  if (item.referenciaErro && (item.linhasComErro ?? 0) > 0) {
    carregandoErros.value = true
    try {
      const resp = await useApi(`/upload/importacoes/erros/{referenciaErro}`, { params: { referenciaErro: item.referenciaErro } })
      erros.value = extrairLista<Record<string, unknown>>(resp)
    } catch (e) {
      console.error('[importacoes] erros', e)
    } finally {
      carregandoErros.value = false
    }
  }
}

onMounted(buscar)
</script>

<template>
  <div>
    <PageToolbar title="Importações" subtitle="Acompanhamento das execuções de importação de dados" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="buscar">Atualizar</button>
      </template>
    </PageToolbar>

    <div class="filtros">
      <SelectField v-model="filtroStatus" :options="opcoesStatus" label="Status" placeholder="Todos" @update:model-value="pagina = 1; buscar()" />
    </div>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhuma importação registrada."
      @update:page="pagina = $event; buscar()"
      @update:page-size="tamanhoPagina = $event; pagina = 1; buscar()"
      @row-click="abrirDetalhe"
    >
      <template #cell-criadoEm="{ value }">{{ value ? formatarDataHora(String(value)) : '—' }}</template>
      <template #cell-tipoImportacao="{ value }">{{ value ?? '—' }}</template>
      <template #cell-status="{ value }">
        <span class="badge" :class="Number(value) === 4 ? 'badge-success' : Number(value) === 5 ? 'badge-danger' : 'badge-secondary'">
          {{ STATUS[Number(value)] ?? value }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirDetalhe(row)">Detalhes</button>
      </template>
    </DataTable>

    <div v-if="detalhe" class="drawer" @click.self="detalhe = null">
      <div class="drawer-body">
        <div class="drawer-head">
          <h3>Importação — {{ detalhe.tipoImportacao ?? 'tabular' }}</h3>
          <button type="button" class="btn btn-ghost btn-sm" @click="detalhe = null">Fechar</button>
        </div>
        <dl class="kv">
          <dt>Status</dt><dd>{{ STATUS[detalhe.status] ?? detalhe.status }}</dd>
          <dt>Total de linhas</dt><dd>{{ detalhe.totalLinhas ?? '—' }}</dd>
          <dt>Processadas</dt><dd>{{ detalhe.linhasProcessadas ?? '—' }}</dd>
          <dt>Com erro</dt><dd>{{ detalhe.linhasComErro ?? 0 }}</dd>
        </dl>
        <h4 v-if="(detalhe.linhasComErro ?? 0) > 0">Erros</h4>
        <div v-if="carregandoErros" class="empty">Carregando erros…</div>
        <table v-else-if="erros.length" class="mini-table">
          <thead><tr><th v-for="c in Object.keys(erros[0])" :key="c">{{ c }}</th></tr></thead>
          <tbody>
            <tr v-for="(e, i) in erros" :key="i"><td v-for="c in Object.keys(erros[0])" :key="c">{{ e[c] ?? '—' }}</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.filtros { display: flex; gap: 1rem; margin-bottom: 1rem; max-width: 320px; }
.drawer { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; justify-content: flex-end; z-index: 50; }
.drawer-body { width: min(560px, 100%); background: var(--surface, #fff); height: 100%; padding: 1.5rem; overflow-y: auto; }
.drawer-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
.drawer-head h3 { margin: 0; font-size: 1.05rem; }
.kv { display: grid; grid-template-columns: max-content 1fr; gap: .35rem 1rem; margin: 0 0 1rem; }
.kv dt { color: var(--text-muted, #6b7280); }
.kv dd { margin: 0; font-weight: 500; }
.mini-table { width: 100%; border-collapse: collapse; font-size: .8rem; }
.mini-table th, .mini-table td { padding: .4rem .5rem; border-bottom: 1px solid var(--border, #eef0f2); text-align: left; }
.empty { color: var(--text-muted, #9ca3af); }
</style>
