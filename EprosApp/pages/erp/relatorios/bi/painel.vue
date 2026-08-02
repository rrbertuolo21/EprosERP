<script setup lang="ts">
/**
 * BI — Painel gerencial (RPT-ONM).
 *
 * Compõe a visão única do gestor a partir de:
 *   - GET /relatorios/bi/painel-gerencial  (faturamento, margem, inadimplência, ruptura, saldo caixa)
 *   - GET /relatorios/bi/serie/faturamento (série temporal do faturamento — gráfico de barras)
 *   - GET /relatorios/bi/top-produtos      (ranking por valor/quantidade)
 * Filtros: período (padrão mês corrente) e granularidade/critério dos painéis auxiliares.
 * Só leitura; nenhuma métrica é recalculada no front (a fórmula é do backend — RN-BI-002).
 */
import { ref, computed, onMounted } from 'vue'
import {
  useRelatorios,
  type PainelGerencial,
  type SerieTemporal,
  type TopProdutos
} from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'

definePageMeta({ layout: 'default' })

const rel = useRelatorios()
const { formatarMoeda, formatarNumero, formatarPorcentagem, formatarData } = useHelper()
const toast = useToast()

const periodo = ref(rel.periodoMesCorrente())
const granularidade = ref<'Dia' | 'Mes'>('Dia')
const criterio = ref<'Valor' | 'Quantidade'>('Valor')
const carregando = ref(false)

const painel = ref<PainelGerencial | null>(null)
const serie = ref<SerieTemporal | null>(null)
const top = ref<TopProdutos | null>(null)

const opcoesGranularidade = [
  { label: 'Diária', value: 'Dia' },
  { label: 'Mensal', value: 'Mes' }
]
const opcoesCriterio = [
  { label: 'Valor', value: 'Valor' },
  { label: 'Quantidade', value: 'Quantidade' }
]

async function carregar() {
  carregando.value = true
  const f = { inicio: periodo.value.inicio, fim: periodo.value.fim }
  try {
    const [p, s, t] = await Promise.all([
      rel.painelGerencial(f),
      rel.serieFaturamento({ ...f, granularidade: granularidade.value }),
      rel.topProdutos({ ...f, topN: 10, criterio: criterio.value })
    ])
    painel.value = p
    serie.value = s
    top.value = t
  } catch (e) {
    console.error('[relatorios/bi/painel]', e)
    toast.error('Não foi possível carregar o painel gerencial.')
  } finally {
    carregando.value = false
  }
}

const kpis = computed(() => {
  const p = painel.value
  return [
    { id: 'fat', titulo: 'Faturamento', valor: formatarMoeda(p?.faturamentoPeriodo ?? 0), cor: 'primary' },
    { id: 'mrg', titulo: 'Margem', valor: formatarPorcentagem(p?.margem.margemPercentual ?? 0), sub: formatarMoeda(p?.margem.margemValor ?? 0), cor: 'success' },
    { id: 'ina', titulo: 'Inadimplência', valor: formatarPorcentagem(p?.inadimplencia.percentualInadimplencia ?? 0), sub: formatarMoeda(p?.inadimplencia.saldoVencido ?? 0), cor: 'danger' },
    { id: 'rup', titulo: 'Ruptura', valor: formatarPorcentagem(p?.ruptura.percentualRuptura ?? 0), sub: `${p?.ruptura.itensEmRuptura ?? 0} de ${p?.ruptura.totalItens ?? 0} itens`, cor: 'warning' },
    { id: 'cx', titulo: 'Saldo de caixa', valor: formatarMoeda(p?.saldoCaixaLiquido ?? 0), cor: (p?.saldoCaixaLiquido ?? 0) >= 0 ? 'success' : 'danger' }
  ]
})

const maxSerie = computed(() => {
  const vs = serie.value?.pontos.map((p) => p.valor) ?? []
  const m = Math.max(0, ...vs)
  return m > 0 ? m : 1
})
function alturaPct(v: number): number {
  return Math.round((v / maxSerie.value) * 100)
}
function rotuloPeriodo(p: string): string {
  const d = formatarData(p)
  return granularidade.value === 'Mes' ? d.slice(3) : d.slice(0, 5)
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Painel gerencial" subtitle="KPIs de faturamento, margem, inadimplência e ruptura" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="filtro-bar glass-panel">
      <DateTimeField v-model="periodo.inicio" label="Início" />
      <DateTimeField v-model="periodo.fim" label="Fim" />
      <SelectField v-model="granularidade" label="Granularidade da série" :options="opcoesGranularidade" />
      <SelectField v-model="criterio" label="Top produtos por" :options="opcoesCriterio" />
      <button type="button" class="btn btn-primary btn-sm filtro-btn" :disabled="carregando" @click="carregar">Atualizar</button>
    </div>

    <section class="cards-grid">
      <div v-for="k in kpis" :key="k.id" class="kpi glass-panel" :class="`is-${k.cor}`">
        <span class="kpi-titulo">{{ k.titulo }}</span>
        <span class="kpi-valor">{{ carregando ? '—' : k.valor }}</span>
        <span v-if="k.sub" class="kpi-sub">{{ carregando ? '' : k.sub }}</span>
      </div>
    </section>

    <div class="painel-linha">
      <!-- Série de faturamento -->
      <section class="glass-panel grafico-panel">
        <h3 class="painel-titulo">Faturamento no tempo</h3>
        <div v-if="carregando" class="vazio">Carregando…</div>
        <div v-else-if="!serie || serie.pontos.length === 0" class="vazio">Sem dados no período.</div>
        <div v-else class="grafico">
          <div v-for="p in serie.pontos" :key="p.periodo" class="grafico-col">
            <div class="grafico-barras">
              <div class="barra" :style="{ height: `${alturaPct(p.valor)}%` }" :title="`${formatarData(p.periodo)} — ${formatarMoeda(p.valor)}`" />
            </div>
            <span class="grafico-rotulo">{{ rotuloPeriodo(p.periodo) }}</span>
          </div>
        </div>
        <p v-if="serie && serie.pontos.length" class="serie-total">Total: <strong>{{ formatarMoeda(serie.total) }}</strong></p>
      </section>

      <!-- Top produtos -->
      <section class="glass-panel top-panel">
        <h3 class="painel-titulo">Top produtos <span class="top-criterio">({{ criterio.toLowerCase() }})</span></h3>
        <div v-if="carregando" class="vazio">Carregando…</div>
        <div v-else-if="!top || top.itens.length === 0" class="vazio">Sem produtos no período.</div>
        <ol v-else class="top-lista">
          <li v-for="(item, i) in top.itens" :key="item.produtoId" class="top-item">
            <span class="top-rank">{{ i + 1 }}</span>
            <span class="top-nome">{{ item.produto }}</span>
            <span class="top-valor">
              {{ criterio === 'Valor' ? formatarMoeda(item.valorTotal) : formatarNumero(item.quantidade) }}
            </span>
          </li>
        </ol>
      </section>
    </div>
  </div>
</template>

<style scoped>
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; flex-wrap: wrap; }
.filtro-btn { align-self: flex-end; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(190px, 1fr)); gap: 16px; margin-bottom: 20px; }
.kpi { display: flex; flex-direction: column; gap: 6px; padding: 18px 20px; border-left: 3px solid var(--border-color); }
.kpi.is-primary { border-left-color: var(--primary); }
.kpi.is-success { border-left-color: var(--success); }
.kpi.is-danger { border-left-color: var(--danger); }
.kpi.is-warning { border-left-color: var(--warning); }
.kpi-titulo { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.kpi-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.kpi-sub { font-size: 12px; color: var(--text-secondary); }
.painel-linha { display: grid; grid-template-columns: 1.5fr 1fr; gap: 16px; }
@media (max-width: 900px) { .painel-linha { grid-template-columns: 1fr; } }
.grafico-panel, .top-panel { padding: 20px 24px; }
.painel-titulo { font-size: 15px; font-weight: 700; color: var(--text-primary); margin: 0 0 18px; }
.top-criterio { font-weight: 500; color: var(--text-secondary); font-size: 13px; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.grafico { display: flex; align-items: flex-end; gap: 6px; height: 200px; overflow-x: auto; }
.grafico-col { display: flex; flex-direction: column; align-items: center; gap: 6px; height: 100%; justify-content: flex-end; min-width: 34px; }
.grafico-barras { display: flex; align-items: flex-end; height: 100%; width: 100%; justify-content: center; }
.barra { width: 60%; min-height: 2px; border-radius: 4px 4px 0 0; background: var(--primary); transition: height 0.3s ease; }
.grafico-rotulo { font-size: 10px; color: var(--text-secondary); font-weight: 600; white-space: nowrap; }
.serie-total { margin: 14px 0 0; font-size: 13px; color: var(--text-secondary); }
.top-lista { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; }
.top-item { display: flex; align-items: center; gap: 12px; padding: 10px 0; border-bottom: 1px solid var(--border-color); }
.top-item:last-child { border-bottom: none; }
.top-rank {
  display: flex; align-items: center; justify-content: center; width: 24px; height: 24px;
  border-radius: 50%; background: color-mix(in srgb, var(--primary) 16%, transparent);
  color: var(--primary); font-weight: 700; font-size: 12px; flex-shrink: 0;
}
.top-nome { flex: 1; font-size: 13px; font-weight: 600; color: var(--text-primary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.top-valor { font-size: 13px; font-weight: 700; color: var(--text-primary); white-space: nowrap; }
</style>
