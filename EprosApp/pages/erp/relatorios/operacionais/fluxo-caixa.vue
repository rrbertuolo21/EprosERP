<script setup lang="ts">
/**
 * Relatório — Fluxo de caixa (RPT-OPB).
 *
 * Fonte: GET /relatorios/operacionais/financeiro/fluxo-caixa (via useRelatorios.fluxoCaixa).
 * Entradas x saídas por dia + saldo. Filtro: período (padrão mês corrente) e incluir planejamento.
 */
import { ref, computed, onMounted } from 'vue'
import { useRelatorios, type FluxoCaixaRelatorio } from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

const rel = useRelatorios()
const { formatarMoeda, formatarData } = useHelper()
const toast = useToast()

const periodo = ref(rel.periodoMesCorrente())
const incluirPlanejamento = ref(false)
const dados = ref<FluxoCaixaRelatorio | null>(null)
const carregando = ref(false)

async function carregar() {
  carregando.value = true
  try {
    dados.value = await rel.fluxoCaixa({
      inicio: periodo.value.inicio,
      fim: periodo.value.fim,
      incluirPlanejamento: incluirPlanejamento.value
    })
  } catch (e) {
    console.error('[relatorios/fluxo-caixa]', e)
    toast.error('Não foi possível carregar o fluxo de caixa.')
  } finally {
    carregando.value = false
  }
}

const maxSerie = computed(() => {
  const vs = (dados.value?.linhas ?? []).flatMap((l) => [l.entradas, l.saidas])
  const m = Math.max(0, ...vs)
  return m > 0 ? m : 1
})
function alturaPct(v: number): number {
  return Math.round((v / maxSerie.value) * 100)
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Fluxo de caixa" subtitle="Entradas x saídas por dia no período" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="filtro-bar glass-panel">
      <DateTimeField v-model="periodo.inicio" label="Início" />
      <DateTimeField v-model="periodo.fim" label="Fim" />
      <label class="check">
        <input v-model="incluirPlanejamento" type="checkbox" />
        Incluir planejamento
      </label>
      <button type="button" class="btn btn-primary btn-sm filtro-btn" :disabled="carregando" @click="carregar">Buscar</button>
    </div>

    <section class="cards-grid">
      <div class="kpi glass-panel is-success">
        <span class="kpi-titulo">Total entradas</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.totalEntradas ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel is-danger">
        <span class="kpi-titulo">Total saídas</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.totalSaidas ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel" :class="(dados?.saldoLiquido ?? 0) >= 0 ? 'is-success' : 'is-danger'">
        <span class="kpi-titulo">Saldo líquido</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.saldoLiquido ?? 0) }}</span>
      </div>
    </section>

    <section class="glass-panel grafico-panel">
      <header class="painel-header">
        <h3 class="painel-titulo">Entradas x Saídas</h3>
        <div class="legenda">
          <span class="legenda-item"><i class="legenda-cor is-entrada" /> Entradas</span>
          <span class="legenda-item"><i class="legenda-cor is-saida" /> Saídas</span>
        </div>
      </header>
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="!dados || dados.linhas.length === 0" class="vazio">Sem movimentação no período.</div>
      <div v-else class="grafico">
        <div v-for="l in dados.linhas" :key="l.dia" class="grafico-col">
          <div class="grafico-barras">
            <div class="barra is-entrada" :style="{ height: `${alturaPct(l.entradas)}%` }" :title="`Entradas ${formatarMoeda(l.entradas)}`" />
            <div class="barra is-saida" :style="{ height: `${alturaPct(l.saidas)}%` }" :title="`Saídas ${formatarMoeda(l.saidas)}`" />
          </div>
          <span class="grafico-rotulo">{{ formatarData(l.dia).slice(0, 5) }}</span>
        </div>
      </div>
    </section>

    <div v-if="dados && dados.linhas.length" class="glass-panel tabela-panel">
      <table class="tabela">
        <thead><tr><th>Dia</th><th class="num">Entradas</th><th class="num">Saídas</th><th class="num">Saldo</th></tr></thead>
        <tbody>
          <tr v-for="l in dados.linhas" :key="l.dia">
            <td>{{ formatarData(l.dia) }}</td>
            <td class="num pos">{{ formatarMoeda(l.entradas) }}</td>
            <td class="num neg">{{ formatarMoeda(l.saidas) }}</td>
            <td class="num" :class="l.saldo >= 0 ? 'pos' : 'neg'">{{ formatarMoeda(l.saldo) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; flex-wrap: wrap; }
.check { display: flex; align-items: center; gap: 8px; font-size: 14px; color: var(--text-primary); cursor: pointer; }
.filtro-btn { align-self: flex-end; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 16px; }
.kpi { display: flex; flex-direction: column; gap: 8px; padding: 18px 20px; border-left: 3px solid var(--border-color); }
.kpi.is-danger { border-left-color: var(--danger); }
.kpi.is-success { border-left-color: var(--success); }
.kpi-titulo { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.kpi-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.grafico-panel { padding: 20px 24px; margin-bottom: 16px; }
.painel-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 18px; }
.painel-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); margin: 0; }
.legenda { display: flex; gap: 14px; }
.legenda-item { display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--text-secondary); }
.legenda-cor { width: 10px; height: 10px; border-radius: 3px; display: inline-block; }
.legenda-cor.is-entrada { background: var(--success); }
.legenda-cor.is-saida { background: var(--danger); }
.grafico { display: flex; align-items: flex-end; gap: 6px; height: 200px; overflow-x: auto; }
.grafico-col { display: flex; flex-direction: column; align-items: center; gap: 6px; height: 100%; justify-content: flex-end; min-width: 36px; }
.grafico-barras { display: flex; align-items: flex-end; gap: 3px; height: 100%; width: 100%; justify-content: center; }
.barra { width: 42%; min-height: 2px; border-radius: 4px 4px 0 0; transition: height 0.3s ease; }
.barra.is-entrada { background: var(--success); }
.barra.is-saida { background: var(--danger); }
.grafico-rotulo { font-size: 10px; color: var(--text-secondary); font-weight: 600; white-space: nowrap; }
.tabela-panel { padding: 8px 4px; overflow-x: auto; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.tabela { width: 100%; border-collapse: collapse; font-size: 13px; }
.tabela th, .tabela td { padding: 10px 12px; text-align: left; border-bottom: 1px solid var(--border-color); }
.tabela th { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.tabela .num { text-align: right; white-space: nowrap; }
.pos { color: var(--success); }
.neg { color: var(--danger); }
</style>
