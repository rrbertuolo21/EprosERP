<script setup lang="ts">
/**
 * Nova Cobrança por E-mail — Serviços Financeiros.
 * A API expõe apenas POST /servicos-financeiros/cobrancas-email (sem edição;
 * o avanço de estado é feito pela ação Transicionar na listagem).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface CobrancaForm {
  sacadoId: string | null
  nome: string | null
  valor: number | null
  periodo: string | null
  servicos: string | null
  conta: string | null
  linkExterno: string | null
  observacao: string | null
  emails: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesSacado = ref<SelectOption[]>([])

const form = reactive<CobrancaForm>({
  sacadoId: null,
  nome: null,
  valor: null,
  periodo: null,
  servicos: null,
  conta: null,
  linkExterno: null,
  observacao: null,
  emails: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  if (!form.emails) erros.emails = 'Informe ao menos um e-mail.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/servicos-financeiros/cobrancas-email', { method: 'POST', body: { ...form } })
    toast.success('Cobrança criada com sucesso!')
    router.push('/erp/financeiro/servicos-financeiros/cobrancas-email')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/servicos-financeiros/cobrancas-email')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de cobrança não é suportada pela API.')
    router.replace('/erp/financeiro/servicos-financeiros/cobrancas-email/novo')
  }
  opcoesSacado.value = await carregarOpcoesDe('/servicos-financeiros/sacados', ['nome', 'documento'])
})
</script>

<template>
  <div>
    <PageToolbar title="Nova cobrança por e-mail" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.sacadoId" label="Sacado" :options="opcoesSacado" />
          <TextField v-model="form.nome" label="Nome" maxlength="120" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <TextField v-model="form.periodo" label="Período" maxlength="30" />
          <TextField v-model="form.servicos" label="Serviços" maxlength="200" />
          <TextField v-model="form.conta" label="Conta" maxlength="60" />
          <TextField v-model="form.linkExterno" label="Link externo" maxlength="200" />
          <TextField v-model="form.emails" label="E-mails" placeholder="separados por ;" maxlength="300" :error="erros.emails" />
          <TextField v-model="form.observacao" label="Observação" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
