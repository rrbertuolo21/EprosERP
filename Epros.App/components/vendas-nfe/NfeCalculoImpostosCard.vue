<script setup lang="ts">
/**
 * NfeCalculoImpostosCard — ajustes manuais de desconto/frete/seguro/outras despesas e
 * resumo dos totais calculados (porta NfeCalculoImpostosCard do legado).
 *
 * Os ajustes manuais entram no v-model do formulário; os totais consolidados (produtos,
 * impostos, nota) são exibidos a partir da prop `totais` calculada na página.
 */
import { computed } from 'vue'
import { useHelper } from '~/composables/useHelper'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { NfeForm, NfeTotais } from './nfeTypes'

const props = defineProps<{
  modelValue: NfeForm
  totais: NfeTotais
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfeForm]
}>()

const { formatarMoeda } = useHelper()

const form = computed(() => props.modelValue)

function definir<K extends keyof NfeForm>(chave: K, valor: NfeForm[K]) {
  emit('update:modelValue', { ...props.modelValue, [chave]: valor })
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Cálculo / Totais</h2>
    </header>

    <div class="form-grid ajustes-grid">
      <MoneyInput
        :model-value="form.descontoManual"
        label="Desconto (adicional)"
        :disabled="readonly"
        @update:model-value="definir('descontoManual', ($event ?? 0) as number)"
      />
      <MoneyInput
        :model-value="form.freteManual"
        label="Frete"
        :disabled="readonly"
        @update:model-value="definir('freteManual', ($event ?? 0) as number)"
      />
      <MoneyInput
        :model-value="form.seguroManual"
        label="Seguro"
        :disabled="readonly"
        @update:model-value="definir('seguroManual', ($event ?? 0) as number)"
      />
      <MoneyInput
        :model-value="form.outroManual"
        label="Outras despesas"
        :disabled="readonly"
        @update:model-value="definir('outroManual', ($event ?? 0) as number)"
      />
    </div>

    <div class="totais-resumo">
      <div class="totais-linha"><span>Produtos</span><strong>{{ formatarMoeda(totais.valorProduto) }}</strong></div>
      <div class="totais-linha"><span>Desconto</span><strong>- {{ formatarMoeda(totais.valorDesconto) }}</strong></div>
      <div class="totais-linha"><span>Frete</span><strong>{{ formatarMoeda(totais.valorFrete) }}</strong></div>
      <div class="totais-linha"><span>Seguro</span><strong>{{ formatarMoeda(totais.valorSeguro) }}</strong></div>
      <div class="totais-linha"><span>Outras despesas</span><strong>{{ formatarMoeda(totais.valorOutro) }}</strong></div>
      <div class="totais-linha"><span>ICMS</span><strong>{{ formatarMoeda(totais.valorIcms) }}</strong></div>
      <div class="totais-linha"><span>IPI</span><strong>{{ formatarMoeda(totais.valorIpi) }}</strong></div>
      <div class="totais-linha total-nota">
        <span>Total da Nota</span><strong>{{ formatarMoeda(totais.valorNotaFiscal) }}</strong>
      </div>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.ajustes-grid { grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 16px; }
@media (max-width: 900px) { .ajustes-grid { grid-template-columns: 1fr 1fr; } }

.totais-resumo {
  display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px 24px;
  padding: 14px 16px; border-radius: 8px; background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
}
.totais-linha { display: flex; align-items: center; justify-content: space-between; font-size: 13px; color: var(--text-secondary); }
.totais-linha strong { color: var(--text-primary); font-weight: 600; }
.total-nota { grid-column: 1 / -1; margin-top: 6px; padding-top: 10px; border-top: 1px solid var(--border-color); font-size: 15px; }
.total-nota strong { color: var(--primary); font-size: 18px; }
</style>
