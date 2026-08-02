<script setup lang="ts">
/**
 * PDV / Gestão de Caixa — abertura e fechamento, sangria e suprimento.
 *
 * Enriquece o módulo PDV com a operação de caixa que faltava (o `/erp/pdv` cobre só a venda/NFC-e).
 * Consome as rotas reais do controller de Vendas via `usePdvCaixa`:
 *   GET /vendas/caixas/status · GET /vendas/caixas/{id}/detalhado ·
 *   POST /vendas/caixas/abrir|fechar|movimentar.
 *
 * Apresentação apenas — nenhuma regra de negócio nova; o saldo calculado vem do backend.
 */
import { onMounted, ref } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { obterMensagemErro } from '~/composables/useApiList'
import {
  usePdvCaixa,
  type CaixaStatus,
  type CaixaDetalhado,
  type TipoMovimentoCaixa
} from '~/composables/usePdvCaixa'

definePageMeta({ layout: 'default', middleware: 'auth' })

const toast = useToast()
const { formatarMoeda, formatarDataHora } = useHelper()
const caixa = usePdvCaixa()

const operadorId = ref('')
const carregando = ref(false)
const status = ref<CaixaStatus | null>(null)
const detalhe = ref<CaixaDetalhado | null>(null)

const aberto = ref(false) // caixa aberto?

// --- Abertura
const dlgAbrir = ref(false)
const saldoAbertura = ref<number | null>(0)
const salvandoAbrir = ref(false)

// --- Fechamento
const dlgFechar = ref(false)
const saldoFechamento = ref<number | null>(0)
const salvandoFechar = ref(false)

// --- Movimento (sangria/suprimento)
const dlgMov = ref(false)
const movTipo = ref<TipoMovimentoCaixa>('Suprimento')
const movValor = ref<number | null>(0)
const movObs = ref('')
const salvandoMov = ref(false)

async function carregar() {
  operadorId.value = caixa.operadorAtual()
  if (!operadorId.value) {
    toast.warning('Não foi possível identificar o operador logado.')
    return
  }
  carregando.value = true
  try {
    const st = await caixa.obterStatus(operadorId.value)
    status.value = st
    aberto.value = st.status === 'Aberto'
    detalhe.value = aberto.value && st.id ? await caixa.obterDetalhado(st.id) : null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function confirmarAbrir() {
  salvandoAbrir.value = true
  try {
    await caixa.abrir(operadorId.value, saldoAbertura.value ?? 0)
    toast.success('Caixa aberto.')
    dlgAbrir.value = false
    saldoAbertura.value = 0
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAbrir.value = false
  }
}

async function confirmarFechar() {
  if (!status.value?.id) return
  salvandoFechar.value = true
  try {
    await caixa.fechar(status.value.id, saldoFechamento.value ?? 0)
    toast.success('Caixa fechado.')
    dlgFechar.value = false
    saldoFechamento.value = 0
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFechar.value = false
  }
}

function abrirMovimento(tipo: TipoMovimentoCaixa) {
  movTipo.value = tipo
  movValor.value = 0
  movObs.value = ''
  dlgMov.value = true
}

async function confirmarMovimento() {
  if (!status.value?.id) return
  if (!movValor.value || movValor.value <= 0) {
    toast.warning('Informe um valor maior que zero.')
    return
  }
  salvandoMov.value = true
  try {
    await caixa.movimentar(status.value.id, movTipo.value, movValor.value, movObs.value || null)
    toast.success(`${movTipo.value} registrado.`)
    dlgMov.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoMov.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Caixa" subtitle="Abertura, fechamento, sangria e suprimento" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="carregando" @click="carregar">Atualizar</button>
        <button v-if="!aberto" type="button" class="btn btn-primary" @click="dlgAbrir = true">Abrir caixa</button>
        <template v-else>
          <button type="button" class="btn btn-secondary" @click="abrirMovimento('Suprimento')">+ Suprimento</button>
          <button type="button" class="btn btn-secondary" @click="abrirMovimento('Sangria')">- Sangria</button>
          <button type="button" class="btn btn-danger" @click="dlgFechar = true">Fechar caixa</button>
        </template>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="status-linha">
        <span class="badge" :class="aberto ? 'badge-success' : 'badge-danger'">
          {{ aberto ? 'Caixa aberto' : 'Caixa fechado' }}
        </span>
        <span class="op">Operador: <strong>{{ operadorId || '—' }}</strong></span>
        <span v-if="status?.criadoEm">Aberto em: <strong>{{ formatarDataHora(status.criadoEm) }}</strong></span>
      </div>
    </div>

    <div v-if="aberto && detalhe" class="cards">
      <div class="glass-panel card">
        <span class="card-label">Saldo de abertura</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.saldoAbertura ?? 0) }}</strong>
      </div>
      <div class="glass-panel card">
        <span class="card-label">Vendas em dinheiro</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.totalVendasDinheiro ?? 0) }}</strong>
      </div>
      <div class="glass-panel card">
        <span class="card-label">Suprimentos</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.totalSuprimentos ?? 0) }}</strong>
      </div>
      <div class="glass-panel card">
        <span class="card-label">Sangrias</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.totalSangrias ?? 0) }}</strong>
      </div>
      <div class="glass-panel card card-destaque">
        <span class="card-label">Saldo calculado (dinheiro)</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.saldoCalculadoDinheiro ?? 0) }}</strong>
      </div>
      <div class="glass-panel card">
        <span class="card-label">Total de vendas</span>
        <strong class="card-valor">{{ formatarMoeda(detalhe.totalVendas ?? 0) }}</strong>
      </div>
    </div>

    <div v-if="aberto && detalhe" class="glass-panel form-panel">
      <h3 class="secao-titulo">Movimentos do caixa</h3>
      <table class="admin-table">
        <thead>
          <tr><th>Data</th><th>Tipo</th><th class="td-right">Valor</th><th>Observação</th></tr>
        </thead>
        <tbody>
          <tr v-if="!detalhe.movimentos || detalhe.movimentos.length === 0">
            <td colspan="4"><div class="table-empty">Nenhum movimento registrado.</div></td>
          </tr>
          <tr v-for="m in detalhe.movimentos" v-else :key="m.id">
            <td>{{ m.criadoEm ? formatarDataHora(m.criadoEm) : '' }}</td>
            <td><span class="badge" :class="m.tipo === 'Sangria' ? 'badge-danger' : 'badge-success'">{{ m.tipo }}</span></td>
            <td class="td-right">{{ formatarMoeda(m.valor) }}</td>
            <td>{{ m.observacao || '' }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Abrir caixa -->
    <AppDialog v-model="dlgAbrir" title="Abrir caixa" width="420px" persistent>
      <div class="form-grid">
        <MoneyInput v-model="saldoAbertura" label="Saldo de abertura" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoAbrir" @click="dlgAbrir = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAbrir" @click="confirmarAbrir">
          <span v-if="salvandoAbrir" class="spinner"></span><span v-else>Abrir</span>
        </button>
      </template>
    </AppDialog>

    <!-- Fechar caixa -->
    <AppDialog v-model="dlgFechar" title="Fechar caixa" width="420px" persistent>
      <div class="form-grid">
        <MoneyInput v-model="saldoFechamento" label="Saldo conferido (fechamento)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoFechar" @click="dlgFechar = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="salvandoFechar" @click="confirmarFechar">
          <span v-if="salvandoFechar" class="spinner"></span><span v-else>Fechar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Sangria / Suprimento -->
    <AppDialog v-model="dlgMov" :title="movTipo" width="440px" persistent>
      <div class="form-grid">
        <MoneyInput v-model="movValor" :label="`Valor da ${movTipo.toLowerCase()}`" />
        <TextField v-model="movObs" label="Observação" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoMov" @click="dlgMov = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoMov" @click="confirmarMovimento">
          <span v-if="salvandoMov" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.secao-titulo { font-size: 15px; margin-bottom: 14px; }
.status-linha { display: flex; align-items: center; gap: 18px; flex-wrap: wrap; font-size: 14px; }
.status-linha .op { color: var(--text-secondary); }
.cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 12px; margin-top: 12px; }
.card { padding: 16px; display: flex; flex-direction: column; gap: 6px; }
.card-label { font-size: 12px; text-transform: uppercase; letter-spacing: 0.03em; color: var(--text-secondary); }
.card-valor { font-size: 22px; font-weight: 800; }
.card-destaque { border-color: var(--primary); }
.admin-table { width: 100%; }
</style>
