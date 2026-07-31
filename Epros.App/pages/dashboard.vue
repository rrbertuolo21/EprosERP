<script setup lang="ts">
/**
 * Dashboard operacional do tenant (submódulo 1.03 DASHBOARD_E_LAYOUT).
 *
 * Substitui o antigo redirect para `/erp/acesso-rapido`. Consome o endpoint
 * `GET /api/v1/dashboard/operacional`, que compõe indicadores financeiros-raiz a
 * partir dos agregados fiéis do Financeiro (ContasAReceber / ContasAPagar):
 * cards (a receber, a pagar, receita/despesa do mês, saldo), gráfico dos últimos
 * 6 meses (barras CSS, sem lib externa) e as últimas transações.
 *
 * O conteúdo legado (~7k linhas, mockado) segue preservado em `dashboard.vue.bak`.
 */
import { ref, computed, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface DashboardMes {
  ano: number
  mes: number
  rotulo: string
  receita: number
  despesa: number
}
interface DashboardTransacao {
  data: string
  descricao: string
  valor: number
  tipo: string
}
interface DashboardOperacional {
  totalAReceber: number
  totalAPagar: number
  receitaMes: number
  despesaMes: number
  saldo: number
  serieMensal: DashboardMes[]
  ultimasTransacoes: DashboardTransacao[]
}

const { formatarMoeda, formatarData } = useHelper()

const dados = ref<DashboardOperacional | null>(null)
const carregando = ref(true)
const erro = ref<string | null>(null)

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const resposta = await useApi('/dashboard/operacional')
    dados.value = extrairDados<DashboardOperacional>(resposta) ?? null
  } catch (e) {
    console.error('[dashboard] operacional', e)
    erro.value = 'Não foi possível carregar o painel operacional.'
  } finally {
    carregando.value = false
  }
}

onMounted(carregar)

const cards = computed(() => {
  const d = dados.value
  return [
    { id: 'receber', titulo: 'A Receber', valor: d?.totalAReceber ?? 0, cor: 'success' },
    { id: 'pagar', titulo: 'A Pagar', valor: d?.totalAPagar ?? 0, cor: 'danger' },
    { id: 'receita', titulo: 'Receita do mês', valor: d?.receitaMes ?? 0, cor: 'primary' },
    { id: 'despesa', titulo: 'Despesa do mês', valor: d?.despesaMes ?? 0, cor: 'warning' },
    { id: 'saldo', titulo: 'Saldo do mês', valor: d?.saldo ?? 0, cor: (d?.saldo ?? 0) >= 0 ? 'success' : 'danger' }
  ]
})

const serie = computed(() => dados.value?.serieMensal ?? [])

// Escala do gráfico: maior valor entre receitas e despesas dos meses exibidos.
const maxSerie = computed(() => {
  const valores = serie.value.flatMap(m => [m.receita, m.despesa])
  const max = Math.max(0, ...valores)
  return max > 0 ? max : 1
})

function alturaPct(valor: number): number {
  return Math.round((valor / maxSerie.value) * 100)
}

const transacoes = computed(() => dados.value?.ultimasTransacoes ?? [])
</script>

<template>
  <div>
    <PageToolbar title="Painel operacional" subtitle="Visão financeira do dia a dia da sua empresa" />

    <div v-if="erro" class="estado-erro glass-panel">
      <p>{{ erro }}</p>
      <button type="button" class="btn-recarregar" @click="carregar">Tentar novamente</button>
    </div>

    <template v-else>
      <!-- Cards de indicadores -->
      <section class="cards-grid">
        <div v-for="card in cards" :key="card.id" class="indicador-card glass-panel" :class="`is-${card.cor}`">
          <span class="indicador-titulo">{{ card.titulo }}</span>
          <span class="indicador-valor">
            <template v-if="carregando">—</template>
            <template v-else>{{ formatarMoeda(card.valor) }}</template>
          </span>
        </div>
      </section>

      <div class="painel-linha">
        <!-- Gráfico mensal -->
        <section class="grafico-panel glass-panel">
          <header class="painel-header">
            <h3 class="painel-titulo">Receitas x Despesas</h3>
            <div class="legenda">
              <span class="legenda-item"><i class="legenda-cor is-receita" /> Receita</span>
              <span class="legenda-item"><i class="legenda-cor is-despesa" /> Despesa</span>
            </div>
          </header>

          <div v-if="carregando" class="painel-vazio">Carregando…</div>
          <div v-else-if="serie.length === 0" class="painel-vazio">Sem dados no período.</div>
          <div v-else class="grafico">
            <div v-for="mes in serie" :key="`${mes.ano}-${mes.mes}`" class="grafico-mes">
              <div class="grafico-barras">
                <div
                  class="barra is-receita"
                  :style="{ height: `${alturaPct(mes.receita)}%` }"
                  :title="`Receita ${formatarMoeda(mes.receita)}`"
                />
                <div
                  class="barra is-despesa"
                  :style="{ height: `${alturaPct(mes.despesa)}%` }"
                  :title="`Despesa ${formatarMoeda(mes.despesa)}`"
                />
              </div>
              <span class="grafico-rotulo">{{ mes.rotulo }}</span>
            </div>
          </div>
        </section>

        <!-- Últimas transações -->
        <section class="transacoes-panel glass-panel">
          <header class="painel-header">
            <h3 class="painel-titulo">Últimas transações</h3>
          </header>

          <div v-if="carregando" class="painel-vazio">Carregando…</div>
          <div v-else-if="transacoes.length === 0" class="painel-vazio">Nenhuma transação recente.</div>
          <ul v-else class="transacoes-lista">
            <li v-for="(t, i) in transacoes" :key="i" class="transacao-item">
              <span class="transacao-tipo" :class="t.tipo === 'Receita' ? 'is-receita' : 'is-despesa'">
                {{ t.tipo === 'Receita' ? '+' : '−' }}
              </span>
              <div class="transacao-info">
                <span class="transacao-desc">{{ t.descricao }}</span>
                <span class="transacao-data">{{ formatarData(t.data) }}</span>
              </div>
              <span class="transacao-valor" :class="t.tipo === 'Receita' ? 'is-receita' : 'is-despesa'">
                {{ formatarMoeda(t.valor) }}
              </span>
            </li>
          </ul>
        </section>
      </div>
    </template>
  </div>
</template>

<style scoped>
.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(190px, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}
.indicador-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 18px 20px;
  border-left: 3px solid var(--border-color);
}
.indicador-card.is-success { border-left-color: var(--success); }
.indicador-card.is-danger { border-left-color: var(--danger); }
.indicador-card.is-primary { border-left-color: var(--primary); }
.indicador-card.is-warning { border-left-color: var(--warning); }
.indicador-titulo {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
}
.indicador-valor {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary);
}

.painel-linha {
  display: grid;
  grid-template-columns: 1.4fr 1fr;
  gap: 16px;
}
@media (max-width: 900px) {
  .painel-linha { grid-template-columns: 1fr; }
}

.painel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 18px;
}
.painel-titulo {
  font-size: 15px;
  font-weight: 700;
  color: var(--text-primary);
}
.painel-vazio {
  padding: 40px 0;
  text-align: center;
  color: var(--text-secondary);
  font-size: 13px;
}

.grafico-panel,
.transacoes-panel {
  padding: 20px 24px;
}

.legenda {
  display: flex;
  gap: 14px;
}
.legenda-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--text-secondary);
}
.legenda-cor {
  width: 10px;
  height: 10px;
  border-radius: 3px;
  display: inline-block;
}
.legenda-cor.is-receita { background: var(--success); }
.legenda-cor.is-despesa { background: var(--danger); }

.grafico {
  display: flex;
  align-items: flex-end;
  justify-content: space-around;
  gap: 12px;
  height: 200px;
}
.grafico-mes {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  flex: 1;
  height: 100%;
  justify-content: flex-end;
}
.grafico-barras {
  display: flex;
  align-items: flex-end;
  gap: 4px;
  height: 100%;
  width: 100%;
  justify-content: center;
}
.barra {
  width: 26%;
  min-height: 2px;
  border-radius: 4px 4px 0 0;
  transition: height 0.3s ease;
}
.barra.is-receita { background: var(--success); }
.barra.is-despesa { background: var(--danger); }
.grafico-rotulo {
  font-size: 11px;
  color: var(--text-secondary);
  font-weight: 600;
}

.transacoes-lista {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
}
.transacao-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid var(--border-color);
}
.transacao-item:last-child { border-bottom: none; }
.transacao-tipo {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  border-radius: 50%;
  font-weight: 700;
  font-size: 14px;
  flex-shrink: 0;
}
.transacao-tipo.is-receita { background: color-mix(in srgb, var(--success) 18%, transparent); color: var(--success); }
.transacao-tipo.is-despesa { background: color-mix(in srgb, var(--danger) 18%, transparent); color: var(--danger); }
.transacao-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
  flex: 1;
}
.transacao-desc {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.transacao-data {
  font-size: 11px;
  color: var(--text-secondary);
}
.transacao-valor {
  font-size: 13px;
  font-weight: 700;
  white-space: nowrap;
}
.transacao-valor.is-receita { color: var(--success); }
.transacao-valor.is-despesa { color: var(--danger); }

.estado-erro {
  padding: 24px;
  text-align: center;
  color: var(--text-secondary);
}
.btn-recarregar {
  margin-top: 12px;
  padding: 8px 18px;
  border-radius: 8px;
  border: 1px solid var(--primary);
  background: transparent;
  color: var(--primary);
  font-weight: 600;
  cursor: pointer;
}
</style>
