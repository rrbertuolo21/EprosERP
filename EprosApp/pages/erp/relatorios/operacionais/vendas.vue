<script setup lang="ts">
/**
 * Relatório — Vendas (RPT-OPB), três cortes numa só tela.
 *
 * Fontes (via useRelatorios):
 *   - Por período  → GET /relatorios/operacionais/vendas/por-periodo  (+ gráfico de barras)
 *   - Por produto  → GET /relatorios/operacionais/vendas/por-produto
 *   - Por cliente  → GET /relatorios/operacionais/vendas/por-cliente
 * Filtros: período (padrão mês corrente) e incluir canceladas. Só apresentação.
 */
import { ref, computed, onMounted } from 'vue'
import {
  useRelatorios,
  type VendasPorPeriodoRelatorio,
  type VendasPorProdutoRelatorio,
  type VendasPorClienteRelatorio
} from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

type Corte = 'periodo' | 'produto' | 'cliente'

const rel = useRelatorios()
const { formatarMoeda, formatarNumero, formatarData } = useHelper()
const toast = useToast()

const corte = ref<Corte>('periodo')
const periodo = ref(rel.periodoMesCorrente())
const incluirCanceladas = ref(false)
const carregando = ref(false)

const dadosPeriodo = ref<VendasPorPeriodoRelatorio | null>(null)
const dadosProduto = ref<VendasPorProdutoRelatorio | null>(null)
const dadosCliente = ref<VendasPorClienteRelatorio | null>(null)

const cortes: { key: Corte; label: string }[] = [
  { key: 'periodo', label: 'Por período' },
  { key: 'produto', label: 'Por produto' },
  { key: 'cliente', label: 'Por cliente' }
]

async function carregar() {
  carregando.value = true
  const f = { inicio: periodo.value.inicio, fim: periodo.value.fim, incluirCanceladas: incluirCanceladas.value }
  try {
    if (corte.value === 'periodo') dadosPeriodo.value = await rel.vendasPorPeriodo(f)
    else if (corte.value === 'produto') dadosProduto.value = await rel.vendasPorProduto(f)
    else dadosCliente.value = await rel.vendasPorCliente(f)
  } catch (e) {
    console.error('[relatorios/vendas]', e)
    toast.error('Não foi possível carregar o relatório de vendas.')
  } finally {
    carregando.value = false
  }
}

function trocarCorte(c: Corte) {
  corte.value = c
  carregar()
}

// Escala do gráfico de barras (por período).
const maxValor = computed(() => {
  const vs = dadosPeriodo.value?.linhas.map((l) => l.valorTotal) ?? []
  const m = Math.max(0, ...vs)
  return m > 0 ? m : 1
})
function alturaPct(v: number): number {
  return Math.round((v / maxValor.value) * 100)
}

const totalAtual = computed(() => {
  if (corte.value === 'periodo') return dadosPeriodo.value?.valorTotal ?? 0
  if (corte.value === 'produto') return dadosProduto.value?.valorTotal ?? 0
  return dadosCliente.value?.valorTotal ?? 0
})

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Vendas" subtitle="Análise de vendas por período, produto e cliente" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="segment">
      <button
        v-for="c in cortes"
        :key="c.key"
        type="button"
        class="segment-btn"
        :class="{ ativo: corte === c.key }"
        @click="trocarCorte(c.key)"
      >
        {{ c.label }}
      </button>
    </div>

    <div class="filtro-bar glass-panel">
      <DateTimeField v-model="periodo.inicio" label="Início" />
      <DateTimeField v-model="periodo.fim" label="Fim" />
      <label class="check">
        <input v-model="incluirCanceladas" type="checkbox" />
        Incluir canceladas
      </label>
      <button type="button" class="btn btn-primary btn-sm filtro-btn" :disabled="carregando" @click="carregar">Buscar</button>
    </div>

    <section class="cards-grid">
      <div class="kpi glass-panel is-primary">
        <span class="kpi-titulo">Valor total</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(totalAtual) }}</span>
      </div>
      <div v-if="corte === 'periodo'" class="kpi glass-panel">
        <span class="kpi-titulo">Qtd. vendas</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarNumero(dadosPeriodo?.quantidadeVendas ?? 0) }}</span>
      </div>
    </section>

    <!-- CORTE: PERÍODO -->
    <template v-if="corte === 'periodo'">
      <section class="glass-panel grafico-panel">
        <h3 class="painel-titulo">Vendas por dia</h3>
        <div v-if="carregando" class="vazio">Carregando…</div>
        <div v-else-if="!dadosPeriodo || dadosPeriodo.linhas.length === 0" class="vazio">Sem vendas no período.</div>
        <div v-else class="grafico">
          <div v-for="l in dadosPeriodo.linhas" :key="l.dia" class="grafico-col">
            <div class="grafico-barras">
              <div class="barra" :style="{ height: `${alturaPct(l.valorTotal)}%` }" :title="`${formatarData(l.dia)} — ${formatarMoeda(l.valorTotal)}`" />
            </div>
            <span class="grafico-rotulo">{{ formatarData(l.dia).slice(0, 5) }}</span>
          </div>
        </div>
      </section>

      <div v-if="dadosPeriodo && dadosPeriodo.linhas.length" class="glass-panel tabela-panel">
        <table class="tabela">
          <thead><tr><th>Dia</th><th class="num">Qtd. vendas</th><th class="num">Valor</th></tr></thead>
          <tbody>
            <tr v-for="l in dadosPeriodo.linhas" :key="l.dia">
              <td>{{ formatarData(l.dia) }}</td>
              <td class="num">{{ formatarNumero(l.quantidadeVendas) }}</td>
              <td class="num">{{ formatarMoeda(l.valorTotal) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- CORTE: PRODUTO -->
    <template v-else-if="corte === 'produto'">
      <div class="glass-panel tabela-panel">
        <div v-if="carregando" class="vazio">Carregando…</div>
        <div v-else-if="!dadosProduto || dadosProduto.linhas.length === 0" class="vazio">Sem vendas no período.</div>
        <table v-else class="tabela">
          <thead><tr><th>Produto</th><th class="num">Quantidade</th><th class="num">Valor</th></tr></thead>
          <tbody>
            <tr v-for="l in dadosProduto.linhas" :key="l.produtoId">
              <td>{{ l.produto }}</td>
              <td class="num">{{ formatarNumero(l.quantidade) }}</td>
              <td class="num">{{ formatarMoeda(l.valorTotal) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- CORTE: CLIENTE -->
    <template v-else>
      <div class="glass-panel tabela-panel">
        <div v-if="carregando" class="vazio">Carregando…</div>
        <div v-else-if="!dadosCliente || dadosCliente.linhas.length === 0" class="vazio">Sem vendas no período.</div>
        <table v-else class="tabela">
          <thead><tr><th>Cliente</th><th class="num">Qtd. vendas</th><th class="num">Valor</th></tr></thead>
          <tbody>
            <tr v-for="(l, i) in dadosCliente.linhas" :key="l.clienteId ?? i">
              <td class="mono">{{ l.clienteId ?? 'Consumidor final' }}</td>
              <td class="num">{{ formatarNumero(l.quantidadeVendas) }}</td>
              <td class="num">{{ formatarMoeda(l.valorTotal) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<style scoped>
.segment { display: inline-flex; gap: 4px; padding: 4px; margin-bottom: 16px; background: var(--surface-2, rgba(0,0,0,0.04)); border-radius: 10px; }
.segment-btn {
  padding: 8px 16px; border: none; background: transparent; border-radius: 8px;
  font-size: 13px; font-weight: 600; color: var(--text-secondary); cursor: pointer;
}
.segment-btn.ativo { background: var(--primary); color: #fff; }
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; flex-wrap: wrap; }
.check { display: flex; align-items: center; gap: 8px; font-size: 14px; color: var(--text-primary); cursor: pointer; }
.filtro-btn { align-self: flex-end; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 16px; }
.kpi { display: flex; flex-direction: column; gap: 8px; padding: 18px 20px; border-left: 3px solid var(--border-color); }
.kpi.is-primary { border-left-color: var(--primary); }
.kpi-titulo { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.kpi-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.grafico-panel { padding: 20px 24px; margin-bottom: 16px; }
.painel-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); margin: 0 0 18px; }
.grafico { display: flex; align-items: flex-end; gap: 6px; height: 200px; overflow-x: auto; }
.grafico-col { display: flex; flex-direction: column; align-items: center; gap: 6px; height: 100%; justify-content: flex-end; min-width: 34px; }
.grafico-barras { display: flex; align-items: flex-end; height: 100%; width: 100%; justify-content: center; }
.barra { width: 60%; min-height: 2px; border-radius: 4px 4px 0 0; background: var(--primary); transition: height 0.3s ease; }
.grafico-rotulo { font-size: 10px; color: var(--text-secondary); font-weight: 600; white-space: nowrap; }
.tabela-panel { padding: 8px 4px; overflow-x: auto; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.tabela { width: 100%; border-collapse: collapse; font-size: 13px; }
.tabela th, .tabela td { padding: 10px 12px; text-align: left; border-bottom: 1px solid var(--border-color); }
.tabela th { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.tabela .num { text-align: right; white-space: nowrap; }
.mono { font-family: ui-monospace, monospace; font-size: 12px; }
</style>
