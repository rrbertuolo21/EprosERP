<script setup lang="ts">
/**
 * Relatório — Aging de títulos CP/CR (RPT-OPB).
 *
 * Fontes (via useRelatorios):
 *   - A receber → GET /relatorios/operacionais/financeiro/aging-receber
 *   - A pagar   → GET /relatorios/operacionais/financeiro/aging-pagar
 * Faixas: A vencer, 1-30, 31-60, 61-90, Acima de 90 (RN-016). Filtro: data de referência.
 */
import { ref, computed, onMounted } from 'vue'
import { useRelatorios, type AgingRelatorio } from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

type Natureza = 'receber' | 'pagar'

const rel = useRelatorios()
const { formatarMoeda, formatarNumero } = useHelper()
const toast = useToast()

const natureza = ref<Natureza>('receber')
const dataReferencia = ref<string | null>(null)
const dados = ref<AgingRelatorio | null>(null)
const carregando = ref(false)

async function carregar() {
  carregando.value = true
  try {
    dados.value = natureza.value === 'receber'
      ? await rel.agingReceber(dataReferencia.value)
      : await rel.agingPagar(dataReferencia.value)
  } catch (e) {
    console.error('[relatorios/aging]', e)
    toast.error('Não foi possível carregar o aging.')
  } finally {
    carregando.value = false
  }
}

function trocar(n: Natureza) {
  natureza.value = n
  carregar()
}

const maxFaixa = computed(() => {
  const vs = dados.value?.faixas.map((f) => f.saldoDevido) ?? []
  const m = Math.max(0, ...vs)
  return m > 0 ? m : 1
})
function larguraPct(v: number): number {
  return Math.round((v / maxFaixa.value) * 100)
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Aging de títulos" subtitle="Faixas de vencimento de contas a pagar e a receber" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="segment">
      <button type="button" class="segment-btn" :class="{ ativo: natureza === 'receber' }" @click="trocar('receber')">A receber</button>
      <button type="button" class="segment-btn" :class="{ ativo: natureza === 'pagar' }" @click="trocar('pagar')">A pagar</button>
    </div>

    <div class="filtro-bar glass-panel">
      <DateTimeField v-model="dataReferencia" label="Data de referência" />
      <button type="button" class="btn btn-primary btn-sm filtro-btn" :disabled="carregando" @click="carregar">Buscar</button>
    </div>

    <section class="cards-grid">
      <div class="kpi glass-panel is-primary">
        <span class="kpi-titulo">Saldo total</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.saldoTotal ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel is-danger">
        <span class="kpi-titulo">Saldo vencido</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.saldoVencido ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel is-success">
        <span class="kpi-titulo">A vencer</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.saldoAVencer ?? 0) }}</span>
      </div>
    </section>

    <div class="glass-panel tabela-panel">
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="!dados || dados.faixas.length === 0" class="vazio">Sem títulos.</div>
      <ul v-else class="faixas">
        <li v-for="f in dados.faixas" :key="f.faixa" class="faixa">
          <div class="faixa-topo">
            <span class="faixa-nome">{{ f.faixa }}</span>
            <span class="faixa-qtd">{{ formatarNumero(f.quantidadeTitulos) }} títulos</span>
            <span class="faixa-valor">{{ formatarMoeda(f.saldoDevido) }}</span>
          </div>
          <div class="faixa-barra-bg">
            <div class="faixa-barra" :style="{ width: `${larguraPct(f.saldoDevido)}%` }" />
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.segment { display: inline-flex; gap: 4px; padding: 4px; margin-bottom: 16px; background: var(--surface-2, rgba(0,0,0,0.04)); border-radius: 10px; }
.segment-btn { padding: 8px 16px; border: none; background: transparent; border-radius: 8px; font-size: 13px; font-weight: 600; color: var(--text-secondary); cursor: pointer; }
.segment-btn.ativo { background: var(--primary); color: #fff; }
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; flex-wrap: wrap; }
.filtro-btn { align-self: flex-end; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 16px; }
.kpi { display: flex; flex-direction: column; gap: 8px; padding: 18px 20px; border-left: 3px solid var(--border-color); }
.kpi.is-primary { border-left-color: var(--primary); }
.kpi.is-danger { border-left-color: var(--danger); }
.kpi.is-success { border-left-color: var(--success); }
.kpi-titulo { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.kpi-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.tabela-panel { padding: 18px 22px; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.faixas { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 16px; }
.faixa-topo { display: flex; align-items: baseline; gap: 12px; margin-bottom: 6px; }
.faixa-nome { font-size: 13px; font-weight: 700; color: var(--text-primary); min-width: 90px; }
.faixa-qtd { font-size: 12px; color: var(--text-secondary); flex: 1; }
.faixa-valor { font-size: 14px; font-weight: 700; color: var(--text-primary); }
.faixa-barra-bg { height: 10px; border-radius: 6px; background: var(--border-color); overflow: hidden; }
.faixa-barra { height: 100%; border-radius: 6px; background: var(--primary); transition: width 0.3s ease; }
</style>
