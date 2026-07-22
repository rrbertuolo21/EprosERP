<script setup lang="ts">
/**
 * EntradaPagamentosCard — formas de pagamento e duplicatas ao fornecedor (NF-e de entrada).
 *
 * Porta o card "Recebimentos/Pagamentos" do legado (`compras/emissao/nfe-entrada`) para a
 * fatia 13. Permite registrar as formas de pagamento (dinheiro/cartão/boleto/PIX/etc.) e as
 * duplicatas (parcelas) que originam o contas a pagar ao fornecedor. Espelha
 * `vendas-nfe/NfeRecebimentosCard.vue`, adaptado ao `EntradaForm`.
 */
import { computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppIcon from '~/components/AppIcon.vue'
import {
  MODALIDADES_PAGAMENTO,
  criarPagamentoVazio,
  criarDuplicataVazia,
  type EntradaForm
} from './tipos'

const props = defineProps<{
  modelValue: EntradaForm
  /** Total da nota, para conferência do pagamento. */
  totalNota: number
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntradaForm]
}>()

const { formatarMoeda } = useHelper()

const form = computed(() => props.modelValue)
const opcoesPagamento = MODALIDADES_PAGAMENTO

const totalPago = computed(() => form.value.pagamentos.reduce((a, p) => a + (p.valorPagamento || 0), 0))
const totalDuplicatas = computed(() => form.value.duplicatas.reduce((a, d) => a + (d.valor || 0), 0))
const diferenca = computed(() => props.totalNota - totalPago.value)

function atualizar(patch: Partial<EntradaForm>) {
  emit('update:modelValue', { ...props.modelValue, ...patch })
}

// --- Pagamentos ---
function adicionarPagamento() {
  atualizar({ pagamentos: [...form.value.pagamentos, criarPagamentoVazio()] })
}
function removerPagamento(uid: string) {
  atualizar({ pagamentos: form.value.pagamentos.filter((p) => p._uid !== uid) })
}
function definirPagamento(uid: string, campo: 'tipoPagamento' | 'valorPagamento' | 'valorTroco', valor: number) {
  atualizar({
    pagamentos: form.value.pagamentos.map((p) => (p._uid === uid ? { ...p, [campo]: valor } : p))
  })
}

// --- Duplicatas ---
function adicionarDuplicata() {
  atualizar({ duplicatas: [...form.value.duplicatas, criarDuplicataVazia()] })
}
function removerDuplicata(uid: string) {
  atualizar({ duplicatas: form.value.duplicatas.filter((d) => d._uid !== uid) })
}
function definirDuplicata(uid: string, campo: 'numero' | 'dataVencimento' | 'valor', valor: string | number) {
  atualizar({
    duplicatas: form.value.duplicatas.map((d) => (d._uid === uid ? { ...d, [campo]: valor } : d))
  })
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Pagamentos ao fornecedor</h2>
    </header>

    <!-- Pagamentos -->
    <div class="rec-bloco">
      <div class="rec-bloco-header">
        <h3 class="rec-subtitulo">Formas de pagamento</h3>
        <button v-if="!readonly" type="button" class="btn btn-secondary btn-sm" @click="adicionarPagamento">
          + Pagamento
        </button>
      </div>

      <p v-if="form.pagamentos.length === 0" class="rec-vazio">Nenhuma forma de pagamento informada.</p>

      <div v-for="p in form.pagamentos" :key="p._uid" class="rec-linha">
        <SelectField
          :model-value="p.tipoPagamento"
          :options="opcoesPagamento"
          :clearable="false"
          :disabled="readonly"
          @update:model-value="definirPagamento(p._uid, 'tipoPagamento', ($event ?? 1) as number)"
        />
        <MoneyInput
          :model-value="p.valorPagamento"
          :disabled="readonly"
          @update:model-value="definirPagamento(p._uid, 'valorPagamento', ($event ?? 0) as number)"
        />
        <MoneyInput
          :model-value="p.valorTroco"
          :disabled="readonly"
          @update:model-value="definirPagamento(p._uid, 'valorTroco', ($event ?? 0) as number)"
        />
        <button v-if="!readonly" type="button" class="btn btn-ghost btn-sm" @click="removerPagamento(p._uid)">
          <AppIcon name="trash" :size="15" />
        </button>
      </div>
    </div>

    <!-- Duplicatas -->
    <div class="rec-bloco">
      <div class="rec-bloco-header">
        <h3 class="rec-subtitulo">Duplicatas (contas a pagar)</h3>
        <button v-if="!readonly" type="button" class="btn btn-secondary btn-sm" @click="adicionarDuplicata">
          + Duplicata
        </button>
      </div>

      <p v-if="form.duplicatas.length === 0" class="rec-vazio">Nenhuma duplicata informada.</p>

      <div v-for="d in form.duplicatas" :key="d._uid" class="rec-linha rec-linha-dup">
        <TextField
          :model-value="d.numero"
          placeholder="Número"
          :disabled="readonly"
          @update:model-value="definirDuplicata(d._uid, 'numero', $event)"
        />
        <DateTimeField
          :model-value="d.dataVencimento"
          :disabled="readonly"
          @update:model-value="definirDuplicata(d._uid, 'dataVencimento', $event ?? '')"
        />
        <MoneyInput
          :model-value="d.valor"
          :disabled="readonly"
          @update:model-value="definirDuplicata(d._uid, 'valor', ($event ?? 0) as number)"
        />
        <button v-if="!readonly" type="button" class="btn btn-ghost btn-sm" @click="removerDuplicata(d._uid)">
          <AppIcon name="trash" :size="15" />
        </button>
      </div>
    </div>

    <!-- Resumo -->
    <div class="rec-resumo">
      <div><span>Total da nota</span><strong>{{ formatarMoeda(totalNota) }}</strong></div>
      <div><span>Total pago</span><strong>{{ formatarMoeda(totalPago) }}</strong></div>
      <div><span>Total duplicatas</span><strong>{{ formatarMoeda(totalDuplicatas) }}</strong></div>
      <div :class="{ 'rec-dif': Math.abs(diferenca) > 0.009 }">
        <span>Diferença</span><strong>{{ formatarMoeda(diferenca) }}</strong>
      </div>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }

.rec-bloco { margin-bottom: 18px; }
.rec-bloco-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }
.rec-subtitulo { font-size: 12.5px; font-weight: 600; color: var(--text-secondary); }
.rec-vazio { font-size: 12.5px; color: var(--text-muted); padding: 4px 0; }

.rec-linha { display: grid; grid-template-columns: 2fr 1.2fr 1.2fr auto; gap: 10px; align-items: end; margin-bottom: 10px; }
.rec-linha-dup { grid-template-columns: 1.5fr 1.2fr 1.2fr auto; }
@media (max-width: 700px) {
  .rec-linha, .rec-linha-dup { grid-template-columns: 1fr 1fr; }
}

.rec-resumo {
  display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px;
  padding: 12px 14px; border-radius: 8px; background: var(--surface-raised);
  border: 1px solid var(--border-color);
}
.rec-resumo > div { display: flex; flex-direction: column; gap: 2px; font-size: 12px; color: var(--text-muted); }
.rec-resumo strong { font-size: 14px; color: var(--text-primary); }
.rec-dif strong { color: var(--warning); }
@media (max-width: 700px) { .rec-resumo { grid-template-columns: 1fr 1fr; } }
</style>
