<script setup lang="ts">
/**
 * Relatórios de Compras (analytics) — ComprasRelatoriosController.
 *
 * Contrato (todos GET, retornam CommandResult com dados):
 *   /compras-relatorios/curva-abc-fornecedor?dataInicio&dataFim
 *   /compras-relatorios/savings-cotacao
 *   /compras-relatorios/lead-time?fornecedorId
 *   /compras-relatorios/aderencia-alcada
 */
import { ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

const toast = useToast()

type Row = Record<string, unknown>
interface Relatorio { titulo: string; rota: string; query?: Record<string, unknown>; linhas: Row[]; carregando: boolean; carregado: boolean }

const dataInicio = ref<string | null>(null)
const dataFim = ref<string | null>(null)

const relatorios = ref<Relatorio[]>([
  { titulo: 'Curva ABC de Fornecedores', rota: '/compras-relatorios/curva-abc-fornecedor', linhas: [], carregando: false, carregado: false },
  { titulo: 'Savings de Cotação', rota: '/compras-relatorios/savings-cotacao', linhas: [], carregando: false, carregado: false },
  { titulo: 'Lead Time de Fornecedores', rota: '/compras-relatorios/lead-time', linhas: [], carregando: false, carregado: false },
  { titulo: 'Aderência de Alçada', rota: '/compras-relatorios/aderencia-alcada', linhas: [], carregando: false, carregado: false }
])

/** Normaliza a resposta em uma lista de linhas (aceita array, {itens}, ou objeto único). */
function paraLinhas(dados: unknown): Row[] {
  if (Array.isArray(dados)) return dados as Row[]
  if (dados && typeof dados === 'object') {
    const o = dados as { itens?: Row[]; linhas?: Row[] }
    if (Array.isArray(o.itens)) return o.itens
    if (Array.isArray(o.linhas)) return o.linhas
    return [dados as Row]
  }
  return []
}

function colunas(linhas: Row[]): string[] {
  return linhas.length ? Object.keys(linhas[0]) : []
}

function formatarCelula(v: unknown): string {
  if (v == null) return '—'
  if (typeof v === 'number') return v.toLocaleString('pt-BR', { maximumFractionDigits: 2 })
  if (typeof v === 'boolean') return v ? 'Sim' : 'Não'
  return String(v)
}

function rotulo(chave: string): string {
  return chave.replace(/([A-Z])/g, ' $1').replace(/^./, (c) => c.toUpperCase()).trim()
}

async function carregar(rel: Relatorio) {
  rel.carregando = true
  try {
    const query: Record<string, unknown> = {}
    if (rel.rota.includes('curva-abc')) {
      if (dataInicio.value) query.dataInicio = dataInicio.value
      if (dataFim.value) query.dataFim = dataFim.value
    }
    const resp = await useApi(rel.rota, { query })
    rel.linhas = paraLinhas(extrairDados(resp))
    rel.carregado = true
  } catch (e) {
    toast.error(obterMensagemErro(e))
    rel.linhas = []
  } finally {
    rel.carregando = false
  }
}

async function carregarTodos() {
  for (const rel of relatorios.value) await carregar(rel)
}
</script>

<template>
  <div>
    <PageToolbar title="Relatórios de Compras" subtitle="Curva ABC, savings de cotação, lead time e aderência de alçada">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="carregarTodos">Gerar relatórios</button>
      </template>
    </PageToolbar>

    <div class="filtros">
      <DateTimeField v-model="dataInicio" label="Data início (Curva ABC)" />
      <DateTimeField v-model="dataFim" label="Data fim (Curva ABC)" />
    </div>

    <div v-for="rel in relatorios" :key="rel.rota" class="card">
      <div class="card-head">
        <h3 class="card-title">{{ rel.titulo }}</h3>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="rel.carregando" @click="carregar(rel)">
          {{ rel.carregando ? 'Carregando…' : 'Atualizar' }}
        </button>
      </div>
      <div v-if="!rel.carregado && !rel.carregando" class="empty">Clique em “Gerar relatórios” ou “Atualizar”.</div>
      <div v-else-if="rel.carregando" class="empty">Carregando…</div>
      <div v-else-if="!rel.linhas.length" class="empty">Sem dados no período.</div>
      <div v-else class="table-wrap">
        <table class="rel-table">
          <thead><tr><th v-for="c in colunas(rel.linhas)" :key="c">{{ rotulo(c) }}</th></tr></thead>
          <tbody>
            <tr v-for="(linha, i) in rel.linhas" :key="i">
              <td v-for="c in colunas(rel.linhas)" :key="c">{{ formatarCelula(linha[c]) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.filtros { display: flex; gap: 1rem; flex-wrap: wrap; margin-bottom: 1rem; max-width: 560px; }
.filtros > * { flex: 1 1 220px; }
.card { background: var(--surface, #fff); border: 1px solid var(--border, #e5e7eb); border-radius: 10px; padding: 1.25rem; margin-bottom: 1rem; }
.card-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: .75rem; }
.card-title { margin: 0; font-size: 1rem; font-weight: 600; }
.empty { color: var(--text-muted, #9ca3af); padding: .5rem 0; font-size: .9rem; }
.table-wrap { overflow-x: auto; }
.rel-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.rel-table th, .rel-table td { padding: .5rem .75rem; border-bottom: 1px solid var(--border, #eef0f2); text-align: left; white-space: nowrap; }
.rel-table th { font-weight: 600; color: var(--text-muted, #6b7280); }
</style>
