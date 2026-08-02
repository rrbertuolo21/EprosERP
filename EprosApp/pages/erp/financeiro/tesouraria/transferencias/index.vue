<script setup lang="ts">
/**
 * Transferência entre contas (Tesouraria).
 * A API expõe apenas POST /tesouraria/transferencias (não há listagem). Tela de operação.
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

interface TransferenciaForm {
  contaOrigemId: string | null
  contaDestinoId: string | null
  valor: number | null
  dataOperacao: string | null
  nota: string | null
}

const toast = useToast()
const salvando = ref(false)
const opcoesConta = ref<SelectOption[]>([])

const form = reactive<TransferenciaForm>({
  contaOrigemId: null,
  contaDestinoId: null,
  valor: null,
  dataOperacao: null,
  nota: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.contaOrigemId) erros.contaOrigemId = 'Conta de origem é obrigatória.'
  if (!form.contaDestinoId) erros.contaDestinoId = 'Conta de destino é obrigatória.'
  if (form.contaOrigemId && form.contaOrigemId === form.contaDestinoId) erros.contaDestinoId = 'Destino deve ser diferente da origem.'
  if (form.valor == null || form.valor <= 0) erros.valor = 'Valor deve ser maior que zero.'
  if (!form.dataOperacao) erros.dataOperacao = 'Data da operação é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/tesouraria/transferencias', { method: 'POST', body: { ...form } })
    toast.success('Transferência realizada com sucesso!')
    form.contaOrigemId = null
    form.contaDestinoId = null
    form.valor = null
    form.dataOperacao = null
    form.nota = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(async () => {
  opcoesConta.value = await carregarOpcoesDe('/tesouraria/contas', ['nome', 'numeroConta'])
})
</script>

<template>
  <div>
    <PageToolbar title="Transferência entre contas" subtitle="Transferência de valores entre contas financeiras" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Transferir</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.contaOrigemId" label="Conta de origem" required :options="opcoesConta" :error="erros.contaOrigemId" />
          <SelectField v-model="form.contaDestinoId" label="Conta de destino" required :options="opcoesConta" :error="erros.contaDestinoId" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <DateTimeField v-model="form.dataOperacao" label="Data da operação" mode="datetime" required :error="erros.dataOperacao" />
          <TextField v-model="form.nota" label="Nota" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
