<script setup lang="ts">
/**
 * Preferências Gerais (Parâmetros Operacionais) — config genuína do tenant.
 *
 * Contrato (ParametrosOperacionaisController, rota `api/v1/configuracoes`):
 *   GET /configuracoes/preferencias
 *   PUT /configuracoes/preferencias  (AtualizarPreferenciasCommand)
 */
import { reactive, ref, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const toast = useToast()

interface Preferencias {
  showCurrency: boolean
  negativeCash: boolean
  negativeStock: boolean
  stockCalculationMode: number
  creditLimit: boolean
  discount: boolean
  vatOnPurchase: boolean
  vatOnSales: boolean
}

const form = reactive<Preferencias>({
  showCurrency: true, negativeCash: false, negativeStock: false,
  stockCalculationMode: 1, creditLimit: false, discount: true,
  vatOnPurchase: false, vatOnSales: false
})
const carregando = ref(false)
const salvando = ref(false)

const modosCusteio: SelectOption[] = [
  { label: 'Custo médio', value: 1 },
  { label: 'FIFO (PEPS)', value: 2 },
  { label: 'Último custo', value: 3 }
]

const flags: { key: keyof Preferencias; label: string; hint: string }[] = [
  { key: 'showCurrency', label: 'Exibir símbolo da moeda', hint: 'Mostra R$ nos valores monetários' },
  { key: 'negativeCash', label: 'Permitir caixa negativo', hint: 'Aceita saldo de caixa abaixo de zero' },
  { key: 'negativeStock', label: 'Permitir estoque negativo', hint: 'Aceita saída sem saldo suficiente' },
  { key: 'creditLimit', label: 'Controlar limite de crédito', hint: 'Bloqueia venda acima do limite do cliente' },
  { key: 'discount', label: 'Permitir desconto', hint: 'Libera desconto em vendas' },
  { key: 'vatOnPurchase', label: 'Destacar imposto na compra', hint: 'Calcula/mostra imposto nas entradas' },
  { key: 'vatOnSales', label: 'Destacar imposto na venda', hint: 'Calcula/mostra imposto nas saídas' }
]

async function carregar() {
  carregando.value = true
  try {
    const resp = await useApi('/configuracoes/preferencias')
    const d = extrairDados<Partial<Preferencias>>(resp)
    if (d) Object.assign(form, d)
  } catch (e) {
    // 404-de-dado: primeira vez sem preferências salvas — mantém os defaults.
    console.warn('[preferencias] sem registro; usando defaults', e)
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  salvando.value = true
  try {
    await useApi('/configuracoes/preferencias', { method: 'PUT', body: { ...form } })
    toast.success('Preferências salvas.')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar title="Preferências Gerais" subtitle="Comportamentos operacionais do tenant (custeio, estoque negativo, impostos, descontos)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">{{ salvando ? 'Salvando…' : 'Salvar' }}</button>
      </template>
    </PageToolbar>

    <div class="card">
      <h3 class="card-title">Custeio de estoque</h3>
      <SelectField v-model="form.stockCalculationMode" :options="modosCusteio" label="Modo de cálculo do custo" :clearable="false" />
    </div>

    <div class="card">
      <h3 class="card-title">Comportamentos</h3>
      <div class="flags">
        <label v-for="f in flags" :key="f.key" class="flag">
          <input type="checkbox" v-model="(form as any)[f.key]" />
          <span class="ftx"><b>{{ f.label }}</b><small>{{ f.hint }}</small></span>
        </label>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card { background: var(--surface, #fff); border: 1px solid var(--border, #e5e7eb); border-radius: 10px; padding: 1.25rem; margin-bottom: 1rem; max-width: 720px; }
.card-title { margin: 0 0 1rem; font-size: 1rem; font-weight: 600; }
.flags { display: grid; grid-template-columns: 1fr 1fr; gap: .85rem 1.5rem; }
@media (max-width: 700px) { .flags { grid-template-columns: 1fr; } }
.flag { display: flex; align-items: flex-start; gap: .6rem; cursor: pointer; }
.flag input { margin-top: .2rem; }
.ftx { display: flex; flex-direction: column; }
.ftx b { font-size: .88rem; font-weight: 600; }
.ftx small { font-size: .75rem; color: var(--text-muted, #6b7280); }
</style>
