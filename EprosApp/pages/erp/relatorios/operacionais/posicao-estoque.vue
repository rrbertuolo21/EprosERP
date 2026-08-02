<script setup lang="ts">
/**
 * Relatório — Posição de estoque (RPT-OPB).
 *
 * Fonte: GET /relatorios/operacionais/estoque/posicao (via useRelatorios.posicaoEstoque).
 * Apresenta os totais (itens, saldo, valor) e a lista de produtos com saldo/mínimo/reservado,
 * destacando os que estão abaixo do mínimo. Filtro: somente abaixo do mínimo.
 */
import { ref, onMounted } from 'vue'
import { useRelatorios, type PosicaoEstoqueRelatorio } from '~/composables/useRelatorios'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

const rel = useRelatorios()
const { formatarMoeda, formatarNumero } = useHelper()
const toast = useToast()

const somenteAbaixoMinimo = ref(false)
const dados = ref<PosicaoEstoqueRelatorio | null>(null)
const carregando = ref(false)

async function carregar() {
  carregando.value = true
  try {
    dados.value = await rel.posicaoEstoque({ somenteAbaixoMinimo: somenteAbaixoMinimo.value })
  } catch (e) {
    console.error('[relatorios/posicao-estoque]', e)
    toast.error('Não foi possível carregar a posição de estoque.')
  } finally {
    carregando.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Posição de estoque" subtitle="Saldo, mínimo, reservado e valor por produto" :loading="carregando">
      <template #actions>
        <NuxtLink to="/erp/relatorios" class="btn btn-secondary">Voltar</NuxtLink>
      </template>
    </PageToolbar>

    <div class="filtro-bar glass-panel">
      <label class="check">
        <input v-model="somenteAbaixoMinimo" type="checkbox" @change="carregar" />
        Somente abaixo do mínimo
      </label>
      <button type="button" class="btn btn-primary btn-sm" :disabled="carregando" @click="carregar">Atualizar</button>
    </div>

    <section class="cards-grid">
      <div class="kpi glass-panel">
        <span class="kpi-titulo">Itens</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarNumero(dados?.quantidadeItens ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel">
        <span class="kpi-titulo">Saldo total</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarNumero(dados?.totalSaldoEstoque ?? 0) }}</span>
      </div>
      <div class="kpi glass-panel is-primary">
        <span class="kpi-titulo">Valor em estoque</span>
        <span class="kpi-valor">{{ carregando ? '—' : formatarMoeda(dados?.totalValorSaldo ?? 0) }}</span>
      </div>
    </section>

    <div class="glass-panel tabela-panel">
      <div v-if="carregando" class="vazio">Carregando…</div>
      <div v-else-if="!dados || dados.linhas.length === 0" class="vazio">Nenhum produto encontrado.</div>
      <table v-else class="tabela">
        <thead>
          <tr>
            <th>SKU</th>
            <th>Produto</th>
            <th>Categoria</th>
            <th class="num">Saldo</th>
            <th class="num">Mínimo</th>
            <th class="num">Reservado</th>
            <th class="num">Custo médio</th>
            <th class="num">Valor</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="l in dados.linhas" :key="l.produtoId" :class="{ 'linha-alerta': l.abaixoDoMinimo }">
            <td class="mono">{{ l.sku }}</td>
            <td>{{ l.produto }}</td>
            <td>{{ l.categoria ?? '—' }}</td>
            <td class="num">
              {{ formatarNumero(l.saldoEstoque) }}
              <span v-if="l.abaixoDoMinimo" class="badge-alerta" title="Abaixo do mínimo">!</span>
            </td>
            <td class="num">{{ formatarNumero(l.estoqueMinimo) }}</td>
            <td class="num">{{ formatarNumero(l.estoqueReservado) }}</td>
            <td class="num">{{ formatarMoeda(l.custoMedio) }}</td>
            <td class="num">{{ formatarMoeda(l.valorSaldo) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.filtro-bar { display: flex; align-items: center; gap: 16px; padding: 14px 18px; margin-bottom: 16px; }
.check { display: flex; align-items: center; gap: 8px; font-size: 14px; color: var(--text-primary); cursor: pointer; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(190px, 1fr)); gap: 16px; margin-bottom: 16px; }
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
.mono { font-family: ui-monospace, monospace; }
.linha-alerta { background: color-mix(in srgb, var(--warning) 8%, transparent); }
.badge-alerta {
  display: inline-flex; align-items: center; justify-content: center;
  width: 16px; height: 16px; margin-left: 6px; border-radius: 50%;
  background: var(--warning); color: #fff; font-size: 11px; font-weight: 700;
}
</style>
