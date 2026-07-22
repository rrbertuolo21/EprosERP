<script setup lang="ts">
/**
 * NfcePagamentosPanel — formas de pagamento da NFC-e.
 *
 * Porta o comportamento de `components/pos/pagamentoLista.vue` + `pos/pagamento` do legado:
 * adicionar pagamentos por tipo, calcular total pago, saldo restante e troco (dinheiro),
 * e limpar pagamentos. O total final vem da página (itens - descontos + acréscimos).
 */
import { computed } from 'vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import { useHelper } from '~/composables/useHelper'
import { OPCOES_TIPO_PAGAMENTO, TipoPagamento, type NfcePagamento } from './types'

const props = defineProps<{
  pagamentos: NfcePagamento[]
  /** Total final da venda (itens - desconto + acréscimo). */
  totalFinal: number
  /** Bloqueia adicionar pagamentos (ex.: sem itens ou documento transmitido). */
  bloqueado?: boolean
  somenteLeitura?: boolean
}>()

const emit = defineEmits<{
  'update:pagamentos': [value: NfcePagamento[]]
}>()

const { formatarMoeda } = useHelper()

const round2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100

// --- Formulário de novo pagamento
const tipoSelecionado = defineModel<number | null>('tipo', { default: null })

const opcoesTipo = OPCOES_TIPO_PAGAMENTO

const totalPago = computed(() =>
  props.pagamentos.reduce((acc, p) => acc + p.valorPagamento, 0)
)

const saldoRestante = computed(() => {
  const s = round2(props.totalFinal - totalPago.value)
  return s > 0 ? s : 0
})

const troco = computed(() => {
  const excedente = round2(totalPago.value - props.totalFinal)
  return excedente > 0 ? excedente : 0
})

/** Valor sugerido ao adicionar (saldo restante). */
const valorSugerido = defineModel<number>('valor', { default: 0 })

function nomeTipo(tipo: number): string {
  return opcoesTipo.find((o) => o.value === tipo)?.label ?? `Tipo ${tipo}`
}

function adicionar() {
  if (props.bloqueado || props.somenteLeitura) return
  const tipo = tipoSelecionado.value
  if (tipo == null) return
  const valor = round2(valorSugerido.value || saldoRestante.value)
  if (valor <= 0) return

  const ehDinheiro = tipo === TipoPagamento.DINHEIRO
  const novoTotalPago = totalPago.value + valor
  const trocoLinha = ehDinheiro ? Math.max(0, round2(novoTotalPago - props.totalFinal)) : 0

  const novo: NfcePagamento = {
    tipoPagamento: tipo,
    valorPagamento: valor,
    valorTroco: trocoLinha
  }
  emit('update:pagamentos', [...props.pagamentos, novo])
  tipoSelecionado.value = null
  valorSugerido.value = 0
}

function remover(index: number) {
  if (props.somenteLeitura) return
  emit('update:pagamentos', props.pagamentos.filter((_, i) => i !== index))
}

function limpar() {
  if (props.somenteLeitura) return
  emit('update:pagamentos', [])
}

/** Preenche o campo de valor com o saldo restante. */
function preencherSaldo() {
  valorSugerido.value = saldoRestante.value
}
</script>

<template>
  <div class="pagamentos-panel glass-panel">
    <div class="panel-header">
      <span class="panel-titulo">Pagamentos</span>
      <button
        v-if="pagamentos.length && !somenteLeitura"
        type="button"
        class="btn btn-ghost btn-sm"
        @click="limpar"
      >
        Limpar
      </button>
    </div>

    <div v-if="!somenteLeitura" class="form-add">
      <SelectField
        v-model="tipoSelecionado"
        label="Forma de pagamento"
        placeholder="Selecione..."
        :options="opcoesTipo"
        :disabled="bloqueado"
        class="campo-tipo"
      />
      <div class="campo-valor">
        <MoneyInput v-model="valorSugerido" label="Valor" :disabled="bloqueado" />
        <button type="button" class="btn btn-ghost btn-sm link-saldo" :disabled="bloqueado" @click="preencherSaldo">
          Usar saldo
        </button>
      </div>
      <button
        type="button"
        class="btn btn-primary btn-add"
        :disabled="bloqueado || tipoSelecionado == null"
        @click="adicionar"
      >
        Adicionar
      </button>
    </div>

    <p v-if="bloqueado && !somenteLeitura" class="aviso-bloqueado">
      Adicione ao menos um item para lançar pagamentos.
    </p>

    <ul v-if="pagamentos.length" class="pagamentos-lista">
      <li v-for="(p, index) in pagamentos" :key="index" class="pagamento-item">
        <div class="pg-info">
          <span class="pg-tipo">{{ nomeTipo(p.tipoPagamento) }}</span>
          <span v-if="p.valorTroco > 0" class="pg-troco">Troco: {{ formatarMoeda(p.valorTroco) }}</span>
        </div>
        <span class="pg-valor">{{ formatarMoeda(p.valorPagamento) }}</span>
        <button
          v-if="!somenteLeitura"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Remover"
          @click="remover(index)"
        >
          ×
        </button>
      </li>
    </ul>

    <div class="resumo">
      <div class="resumo-linha">
        <span>Total da venda</span>
        <span>{{ formatarMoeda(totalFinal) }}</span>
      </div>
      <div class="resumo-linha">
        <span>Total pago</span>
        <span>{{ formatarMoeda(totalPago) }}</span>
      </div>
      <div class="resumo-linha destaque" :class="{ pendente: saldoRestante > 0 }">
        <span>{{ saldoRestante > 0 ? 'Saldo restante' : 'Troco' }}</span>
        <span>{{ formatarMoeda(saldoRestante > 0 ? saldoRestante : troco) }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pagamentos-panel { padding: 12px; display: flex; flex-direction: column; gap: 12px; }
.panel-header { display: flex; align-items: center; justify-content: space-between; }
.panel-titulo { font-weight: 600; font-size: 14px; }
.form-add {
  display: grid;
  grid-template-columns: 1.4fr 1.2fr auto;
  gap: 10px;
  align-items: end;
}
.campo-valor { display: flex; flex-direction: column; gap: 2px; }
.link-saldo { align-self: flex-start; padding: 0; font-size: 11px; }
.btn-add { height: 38px; }
.aviso-bloqueado { font-size: 12px; color: var(--text-muted); }
.pagamentos-lista { list-style: none; display: flex; flex-direction: column; gap: 6px; }
.pagamento-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
}
.pg-info { display: flex; flex-direction: column; gap: 2px; flex: 1; }
.pg-tipo { font-weight: 500; font-size: 13px; }
.pg-troco { font-size: 11px; color: var(--text-muted); }
.pg-valor { font-weight: 600; }
.resumo { border-top: 1px solid var(--border-color); padding-top: 10px; display: flex; flex-direction: column; gap: 6px; }
.resumo-linha { display: flex; justify-content: space-between; font-size: 13px; color: var(--text-secondary); }
.resumo-linha.destaque { font-size: 16px; font-weight: 700; color: var(--success); }
.resumo-linha.destaque.pendente { color: var(--danger); }
@media (max-width: 720px) {
  .form-add { grid-template-columns: 1fr; }
}
</style>
