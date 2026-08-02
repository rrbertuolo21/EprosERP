<script setup lang="ts">
/**
 * Relatório — Giro de estoque (RPT-OPB).
 *
 * Fonte: GET /relatorios/operacionais/estoque/giro (via useRelatorios.giroEstoque).
 * Giro ≈ saídas / saldo atual por produto no período. Filtro: período (padrão mês corrente).
 */
import { ref, onMounted } from 'vue'
import { useRelatorios, type GiroEstoqueRelatorio } from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

const rel = useRelatorios()
const { formatarNumero } = useHelper()
const toast = useToast()

const periodo = ref(rel.periodoMesCorrente())
const dados = ref<GiroEstoqueRelatorio | null>(null)
const carregando = ref(false)

async function carregar() {
  carregando.value = true
  try {
    dados.value = await rel.giroEstoque({ inicio: periodo.value.inicio, fim: periodo.value.fim })
  } catch (e) {
    console.error('[relatorios/giro-estoque]', e)
    toast.error('Não foi possível carregar o giro de estoque.')
  } finally {
    carregando.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Giro de estoque" subtitle="Saídas sobre saldo por produto no período" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="filtro-bar glass-panel">
      <DateTimeField v-model="periodo.inicio" label="Início" />
      <DateTimeField v-model="periodo.fim" label="Fim" />
      <button type="button" class="btn btn-primary btn-sm filtro-btn" :disabled="carregando" @click="carregar">Buscar</button>
    </div>

    <section class="cards-grid">
      <div class="kpi glass-panel is-primary">
        <span class="kpi-titulo">Total de saídas (período)</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarNumero(dados?.totalSaida ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel">
        <span class="kpi-titulo">Produtos</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarNumero(dados?.linhas.length ?? 0) }}</span>
      </div>
    </section>

    <div class="glass-panel tabela-panel">
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="!dados || dados.linhas.length === 0" class="vazio">Sem movimentação no período.</div>
      <table v-else class="tabela">
        <thead>
          <tr>
            <th>Produto</th>
            <th class="num">Qtd. saída</th>
            <th class="num">Saldo atual</th>
            <th class="num">Giro</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="l in dados.linhas" :key="l.produtoId">
            <td>{{ l.produto }}</td>
            <td class="num">{{ formatarNumero(l.quantidadeSaida) }}</td>
            <td class="num">{{ formatarNumero(l.saldoAtual) }}</td>
            <td class="num"><strong>{{ formatarNumero(l.giro, 2, 2) }}</strong></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.filtro-bar { display: flex; align-items: flex-end; gap: 16px; padding: 16px 18px; margin-bottom: 16px; flex-wrap: wrap; }
.filtro-btn { align-self: flex-end; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; margin-bottom: 16px; }
.kpi { display: flex; flex-direction: column; gap: 8px; padding: 18px 20px; border-left: 3px solid var(--border-color); }
.kpi.is-primary { border-left-color: var(--primary); }
.kpi-titulo { font-size: 13px; font-weight: 600; color: var(--text-secondary); }
.kpi-valor { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.tabela-panel { padding: 8px 4px; overflow-x: auto; }
.vazio { padding: 40px 0; text-align: center; color: var(--text-secondary); font-size: 13px; }
.tabela { width: 100%; border-collapse: collapse; font-size: 13px; }
.tabela th, .tabela td { padding: 10px 12px; text-align: left; border-bottom: 1px solid var(--border-color); }
.tabela th { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.tabela .num { text-align: right; white-space: nowrap; }
</style>
