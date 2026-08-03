<script setup lang="ts">
/**
 * TMS — detalhe do transporte (erp/compras/tms/[id]).
 *
 * Camada de apresentação sobre `TmsTransportesController`:
 *   - exibe transportadora / veículo / volumes / reboques do transporte;
 *   - confirmar / cancelar o transporte;
 *   - ratear frete de entrada sobre a compra (NF-04 — valor factual + método).
 *
 * Endpoints: estoque-tms-transportes/{id} (GET), {id}/confirmar, {id}/cancelar (POST),
 *   estoque-tms-transportes/compras/{compraId}/ratear-frete (POST).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'
import { ufSigla } from '~/composables/compras/ufOptions'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface Volume { id: string; quantidadeVolumes: number | null; pesoLiquido: number | null; pesoBruto: number | null }
interface Reboque { id: string; placa: string | null; uf: number | null }
interface TransporteDetalhe {
  id: string
  compraId: string | null
  situacao: number | string | null
  transportadora: { id: string; uf: number | null } | null
  veiculo: { id: string; placa: string | null; uf: number | null } | null
  volumes: Volume[]
  reboques: Reboque[]
}

const route = useRoute()
const toast = useToast()

const transporteId = computed(() => String(route.params.id))
const carregando = ref(false)
const acaoEmCurso = ref(false)
const transporte = ref<TransporteDetalhe | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const STATUS_TEXTO: Record<string, string> = { '0': 'Rascunho', '1': 'Confirmado', '2': 'Faturado', '3': 'Cancelado', '4': 'Estornado' }
const statusTexto = computed(() => STATUS_TEXTO[String(transporte.value?.situacao)] ?? String(transporte.value?.situacao ?? '-'))
const ehRascunho = computed(() => String(transporte.value?.situacao) === '0')
const cancelavel = computed(() => ['0', '1'].includes(String(transporte.value?.situacao)))

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi('/estoque-tms-transportes/{id}', { params: { id: transporteId.value } })
    transporte.value = extrairDados<TransporteDetalhe>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function confirmar() {
  acaoEmCurso.value = true
  try {
    await useApi('/estoque-tms-transportes/{id}/confirmar', { method: 'POST', params: { id: transporteId.value } })
    toast.success('Transporte confirmado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

async function cancelar() {
  const ok = await confirmRef.value?.open('Cancelar transporte', 'Deseja cancelar este transporte?', {
    danger: true, textoConfirmar: 'Cancelar', textoCancelar: 'Voltar'
  })
  if (!ok) return
  acaoEmCurso.value = true
  try {
    await useApi('/estoque-tms-transportes/{id}/cancelar', { method: 'POST', params: { id: transporteId.value } })
    toast.success('Transporte cancelado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmCurso.value = false
  }
}

// --- Ratear frete ---
const METODO_OPTIONS: SelectOption[] = [
  { label: 'Por valor', value: 0 },
  { label: 'Por peso', value: 1 },
  { label: 'Por quantidade', value: 2 }
]
const valorFrete = ref<number | null>(null)
const metodo = ref<number>(0)
const rateando = ref(false)

async function ratearFrete() {
  if (!transporte.value?.compraId) {
    toast.error('Transporte sem compra vinculada')
    return
  }
  if (!valorFrete.value || valorFrete.value <= 0) {
    toast.error('Informe o valor do frete')
    return
  }
  rateando.value = true
  try {
    await useApi('/estoque-tms-transportes/compras/{compraId}/ratear-frete', {
      method: 'POST',
      params: { compraId: transporte.value.compraId },
      body: { valorFrete: valorFrete.value, metodo: metodo.value }
    })
    toast.success('Frete rateado sobre os itens da compra')
    valorFrete.value = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    rateando.value = false
  }
}

function voltar() {
  navigateTo('/erp/compras/tms')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar :title="`Transporte ${statusTexto}`" subtitle="TMS / Frete" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button v-if="ehRascunho" type="button" class="btn btn-primary" :disabled="acaoEmCurso" @click="confirmar">Confirmar</button>
        <button v-if="cancelavel" type="button" class="btn btn-danger" :disabled="acaoEmCurso" @click="cancelar">Cancelar</button>
      </template>
    </PageToolbar>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Dados do Transporte</h3>
      <div class="form-grid">
        <div class="col-6"><TextField :model-value="transporte?.compraId ? `Compra ${String(transporte.compraId).slice(0, 8)}` : ''" label="Compra vinculada" readonly /></div>
        <div class="col-6"><TextField :model-value="statusTexto" label="Situação" readonly /></div>
        <div class="col-6"><TextField :model-value="transporte?.veiculo?.placa ?? '-'" label="Placa do veículo" readonly /></div>
        <div class="col-6"><TextField :model-value="ufSigla(transporte?.veiculo?.uf)" label="UF do veículo" readonly /></div>
      </div>
    </section>

    <section v-if="transporte?.volumes?.length" class="glass-panel bloco">
      <h3 class="bloco-titulo">Volumes</h3>
      <table class="tab">
        <thead><tr><th>Qtd.</th><th class="num">Peso líquido</th><th class="num">Peso bruto</th></tr></thead>
        <tbody>
          <tr v-for="v in transporte.volumes" :key="v.id">
            <td>{{ v.quantidadeVolumes ?? '-' }}</td>
            <td class="num">{{ v.pesoLiquido ?? '-' }}</td>
            <td class="num">{{ v.pesoBruto ?? '-' }}</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class="glass-panel bloco">
      <h3 class="bloco-titulo">Ratear Frete de Entrada (NF-04)</h3>
      <p class="aviso">O valor do frete é factual (CT-e/transportadora); a apropriação por valor/peso/quantidade compõe o custo dos itens.</p>
      <div class="form-grid">
        <div class="col-4"><MoneyInput v-model="valorFrete" label="Valor do frete" /></div>
        <div class="col-4"><SelectField v-model="metodo" :options="METODO_OPTIONS" label="Método de rateio" :clearable="false" /></div>
        <div class="col-4 acao-inline">
          <button type="button" class="btn btn-primary" :disabled="rateando || !transporte?.compraId" @click="ratearFrete">
            <span v-if="rateando" class="spinner"></span>
            <span v-else>Ratear frete</span>
          </button>
        </div>
      </div>
    </section>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
section.bloco { padding: 16px; }
.bloco-titulo { font-size: 15px; font-weight: 600; margin: 0 0 12px; }
.aviso { color: var(--text-secondary); font-size: 13px; margin: 0 0 12px; }
.tab { width: 100%; border-collapse: collapse; font-size: 13px; }
.tab th, .tab td { padding: 8px 10px; border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08)); text-align: left; }
.tab th.num, .tab td.num { text-align: right; }
.acao-inline { display: flex; align-items: flex-end; }
</style>
