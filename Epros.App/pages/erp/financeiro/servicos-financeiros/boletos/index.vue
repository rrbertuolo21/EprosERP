<script setup lang="ts">
/**
 * Boleto — Serviços Financeiros.
 * A API expõe apenas POST /servicos-financeiros/boletos (não há listagem). Tela de emissão.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface BoletoForm {
  faturaCobrancaId: string | null
  contaEmissoraId: string | null
  numeroDocumento: string | null
  valor: number | null
  dataVencimento: string | null
  linhaDigitavel: string | null
  arquivo: string | null
  multa: number | null
  juros: number | null
  instrucao1: string | null
  instrucao2: string | null
  instrucao3: string | null
  instrucao4: string | null
}

const toast = useToast()
const salvando = ref(false)
const opcoesFatura = ref<SelectOption[]>([])
const opcoesConta = ref<SelectOption[]>([])

const form = reactive<BoletoForm>({
  faturaCobrancaId: null,
  contaEmissoraId: null,
  numeroDocumento: null,
  valor: null,
  dataVencimento: null,
  linhaDigitavel: null,
  arquivo: null,
  multa: 0,
  juros: 0,
  instrucao1: null,
  instrucao2: null,
  instrucao3: null,
  instrucao4: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.faturaCobrancaId) erros.faturaCobrancaId = 'Fatura é obrigatória.'
  if (!form.contaEmissoraId) erros.contaEmissoraId = 'Conta emissora é obrigatória.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  if (!form.dataVencimento) erros.dataVencimento = 'Vencimento é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/servicos-financeiros/boletos', { method: 'POST', body: { ...form } })
    toast.success('Boleto emitido com sucesso!')
    form.numeroDocumento = null
    form.valor = null
    form.linhaDigitavel = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(async () => {
  const [faturas, contas] = await Promise.all([
    carregarOpcoesDe('/servicos-financeiros/faturas', ['numeroDocumento', 'referencia', 'sacadoNome']),
    carregarOpcoesDe('/servicos-financeiros/contas-emissoras', ['nomeBanco', 'conta'])
  ])
  opcoesFatura.value = faturas
  opcoesConta.value = contas
})
</script>

<template>
  <div>
    <PageToolbar title="Emitir Boleto" subtitle="Emissão de boleto a partir da fatura de cobrança" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Emitir</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.faturaCobrancaId" label="Fatura de cobrança" required :options="opcoesFatura" :error="erros.faturaCobrancaId" />
          <SelectField v-model="form.contaEmissoraId" label="Conta emissora" required :options="opcoesConta" :error="erros.contaEmissoraId" />
          <TextField v-model="form.numeroDocumento" label="Número do documento" maxlength="30" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <DateTimeField v-model="form.dataVencimento" label="Vencimento" mode="datetime" required :error="erros.dataVencimento" />
          <MoneyInput v-model="form.multa" label="Multa" />
          <MoneyInput v-model="form.juros" label="Juros" />
          <TextField v-model="form.linhaDigitavel" label="Linha digitável" maxlength="60" />
          <TextField v-model="form.arquivo" label="Arquivo (URL)" maxlength="200" />
          <TextField v-model="form.instrucao1" label="Instrução 1" maxlength="120" />
          <TextField v-model="form.instrucao2" label="Instrução 2" maxlength="120" />
          <TextField v-model="form.instrucao3" label="Instrução 3" maxlength="120" />
          <TextField v-model="form.instrucao4" label="Instrução 4" maxlength="120" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
