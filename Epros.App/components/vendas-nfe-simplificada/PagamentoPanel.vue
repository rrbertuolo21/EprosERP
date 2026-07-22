<script setup lang="ts">
/**
 * PagamentoPanel — formas de pagamento da NF-e simplificada.
 *
 * Porta o comportamento de `components/pos/pagamento.vue` + `pagamentoLista.vue`
 * do legado, sem Vuetify: adiciona pagamentos por tipo/valor, calcula saldo restante
 * e troco, e lista os pagamentos informados.
 *
 * Contrato:
 *   props:
 *     pagamentos: PagamentoVenda[]   (v-model:pagamentos)
 *     totalFinal: number             (total a pagar)
 *     bloqueado?: boolean            (sem itens → não permite pagamento)
 *   emits:
 *     'update:pagamentos': [lista]
 */
import { ref, computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import { TIPOS_PAGAMENTO, type PagamentoVenda } from './tipos'

const props = defineProps<{
  pagamentos: PagamentoVenda[]
  totalFinal: number
  bloqueado?: boolean
}>()

const emit = defineEmits<{
  'update:pagamentos': [lista: PagamentoVenda[]]
}>()

const { formatarMoeda } = useHelper()
const toast = useToast()

const tipoSelecionado = ref<number>(TIPOS_PAGAMENTO[0].value)
const valorInformado = ref<number>(0)

const totalPago = computed(() =>
  props.pagamentos.reduce((acc, p) => acc + p.valorPagamento, 0)
)

const saldoRestante = computed(() => {
  const restante = props.totalFinal - totalPago.value
  return restante > 0 ? Math.round((restante + Number.EPSILON) * 100) / 100 : 0
})

const troco = computed(() => {
  const t = totalPago.value - props.totalFinal
  return t > 0 ? Math.round((t + Number.EPSILON) * 100) / 100 : 0
})

const rotuloTipo = (tipo: number) =>
  TIPOS_PAGAMENTO.find((t) => t.value === tipo)?.label ?? String(tipo)

function preencherSaldo() {
  valorInformado.value = saldoRestante.value
}

function adicionar() {
  if (props.bloqueado) {
    toast.warning('Adicione itens antes de informar o pagamento.')
    return
  }
  if (valorInformado.value <= 0) {
    toast.warning('Informe um valor de pagamento.')
    return
  }
  const novo: PagamentoVenda = {
    tipoPagamento: tipoSelecionado.value,
    valorPagamento: valorInformado.value,
    valorTroco: 0
  }
  emit('update:pagamentos', [...props.pagamentos, novo])
  valorInformado.value = 0
}

function remover(indice: number) {
  emit(
    'update:pagamentos',
    props.pagamentos.filter((_, i) => i !== indice)
  )
}

function limpar() {
  emit('update:pagamentos', [])
  valorInformado.value = 0
}
</script>

<template>
  <div class="pagamento-panel glass-panel">
    <div class="pp-header">
      <span class="pp-title">Pagamento</span>
      <button
        v-if="pagamentos.length"
        type="button"
        class="btn btn-ghost btn-sm"
        @click="limpar"
      >
        Limpar
      </button>
    </div>

    <div class="pp-resumo">
      <div class="pp-linha">
        <span>Total da nota</span>
        <strong>{{ formatarMoeda(totalFinal) }}</strong>
      </div>
      <div class="pp-linha">
        <span>Pago</span>
        <strong>{{ formatarMoeda(totalPago) }}</strong>
      </div>
      <div class="pp-linha" :class="{ 'pp-pendente': saldoRestante > 0 }">
        <span>Saldo restante</span>
        <strong>{{ formatarMoeda(saldoRestante) }}</strong>
      </div>
      <div v-if="troco > 0" class="pp-linha pp-troco">
        <span>Troco</span>
        <strong>{{ formatarMoeda(troco) }}</strong>
      </div>
    </div>

    <div class="pp-form">
      <SelectField
        :model-value="tipoSelecionado"
        label="Forma de pagamento"
        :options="TIPOS_PAGAMENTO"
        :clearable="false"
        @update:model-value="(v) => (tipoSelecionado = Number(v))"
      />
      <MoneyInput v-model="valorInformado" label="Valor" />
      <div class="pp-form-acoes">
        <button type="button" class="btn btn-secondary btn-sm" @click="preencherSaldo">Saldo</button>
        <button type="button" class="btn btn-primary btn-sm" :disabled="bloqueado" @click="adicionar">
          Adicionar
        </button>
      </div>
    </div>

    <ul v-if="pagamentos.length" class="pp-lista">
      <li v-for="(p, i) in pagamentos" :key="i" class="pp-item">
        <span class="pp-item-tipo">{{ rotuloTipo(p.tipoPagamento) }}</span>
        <span class="pp-item-valor">{{ formatarMoeda(p.valorPagamento) }}</span>
        <button type="button" class="btn btn-ghost btn-sm" @click="remover(i)">×</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
.pagamento-panel { padding: 14px; display: flex; flex-direction: column; gap: 12px; }
.pp-header { display: flex; align-items: center; justify-content: space-between; }
.pp-title { font-weight: 600; font-size: 14px; }
.pp-resumo {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 10px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
}
.pp-linha { display: flex; justify-content: space-between; font-size: 13px; }
.pp-linha.pp-pendente strong { color: var(--warning); }
.pp-troco strong { color: var(--success); }
.pp-form { display: grid; grid-template-columns: 1.2fr 1fr auto; gap: 10px; align-items: flex-end; }
.pp-form-acoes { display: flex; gap: 6px; }
.pp-lista { list-style: none; display: flex; flex-direction: column; gap: 6px; }
.pp-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  font-size: 13px;
}
.pp-item-tipo { flex: 1; }
.pp-item-valor { font-weight: 600; }
@media (max-width: 640px) {
  .pp-form { grid-template-columns: 1fr; }
}
</style>
