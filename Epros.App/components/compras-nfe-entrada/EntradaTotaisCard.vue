<script setup lang="ts">
/**
 * EntradaTotaisCard — cálculo/totais e ajustes manuais da NF-e de entrada.
 *
 * Porta o `NfeCalculoImpostosCard` do legado (modo entrada): mostra os totais calculados no cliente
 * (produtos, ICMS, IPI, nota) e permite ajustes manuais de frete/seguro/outros/desconto.
 *
 * v-model recebe o `EntradaForm` (ajustes manuais) e os totais vêm calculados da página.
 */
import { useHelper } from '~/composables/useHelper'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { EntradaForm, EntradaTotais } from './tipos'

const props = defineProps<{
  modelValue: EntradaForm
  totais: EntradaTotais
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntradaForm]
}>()

const { formatarMoeda } = useHelper()

function set<K extends keyof EntradaForm>(chave: K, valor: EntradaForm[K]) {
  props.modelValue[chave] = valor
  emit('update:modelValue', props.modelValue)
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Totais e impostos</h2>
    </header>

    <div class="totais-grid">
      <div class="ajustes">
        <MoneyInput
          :model-value="modelValue.freteManual"
          label="Frete"
          :disabled="readonly"
          @update:model-value="set('freteManual', $event ?? 0)"
        />
        <MoneyInput
          :model-value="modelValue.seguroManual"
          label="Seguro"
          :disabled="readonly"
          @update:model-value="set('seguroManual', $event ?? 0)"
        />
        <MoneyInput
          :model-value="modelValue.outroManual"
          label="Outras despesas"
          :disabled="readonly"
          @update:model-value="set('outroManual', $event ?? 0)"
        />
        <MoneyInput
          :model-value="modelValue.descontoManual"
          label="Desconto (nota)"
          :disabled="readonly"
          @update:model-value="set('descontoManual', $event ?? 0)"
        />
      </div>

      <div class="resumo">
        <div class="linha"><span>Total produtos</span><strong>{{ formatarMoeda(totais.valorProduto) }}</strong></div>
        <div class="linha"><span>Desconto</span><strong>- {{ formatarMoeda(totais.valorDesconto) }}</strong></div>
        <div class="linha"><span>Frete</span><strong>{{ formatarMoeda(totais.valorFrete) }}</strong></div>
        <div class="linha"><span>Seguro</span><strong>{{ formatarMoeda(totais.valorSeguro) }}</strong></div>
        <div class="linha"><span>Outras despesas</span><strong>{{ formatarMoeda(totais.valorOutro) }}</strong></div>
        <div class="linha"><span>Total ICMS</span><strong>{{ formatarMoeda(totais.valorIcms) }}</strong></div>
        <div class="linha"><span>Total IPI</span><strong>{{ formatarMoeda(totais.valorIpi) }}</strong></div>
        <div class="linha total-nota">
          <span>Total da nota</span><strong>{{ formatarMoeda(totais.valorNotaFiscal) }}</strong>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.totais-grid { display: grid; grid-template-columns: 1.4fr 1fr; gap: 24px; }
.ajustes { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px 14px; align-content: start; }
.resumo { display: flex; flex-direction: column; gap: 6px; padding: 14px 16px; background: rgba(255,255,255,0.03); border-radius: 10px; }
.linha { display: flex; justify-content: space-between; font-size: 13px; color: var(--text-secondary); }
.linha strong { color: var(--text-primary); }
.total-nota { margin-top: 6px; padding-top: 10px; border-top: 1px solid var(--border-color); font-size: 15px; }
.total-nota span, .total-nota strong { font-weight: 700; }
.total-nota strong { color: var(--primary); }
@media (max-width: 900px) {
  .totais-grid { grid-template-columns: 1fr; }
}
</style>
