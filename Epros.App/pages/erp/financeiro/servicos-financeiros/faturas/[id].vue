<script setup lang="ts">
/**
 * Nova Fatura de Cobrança — Serviços Financeiros.
 * A API expõe apenas POST /servicos-financeiros/faturas (sem edição; baixa pela listagem).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { OPCOES_TIPO_FATURA, carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface FaturaForm {
  sacadoId: string | null
  grupoRecorrenciaId: string | null
  referencia: string | null
  numeroDocumento: string | null
  data: string | null
  dataVencimento: string | null
  valor: number | null
  email: string | null
  tipoFatura: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesSacado = ref<SelectOption[]>([])
const opcoesGrupo = ref<SelectOption[]>([])

const form = reactive<FaturaForm>({
  sacadoId: null,
  grupoRecorrenciaId: null,
  referencia: null,
  numeroDocumento: null,
  data: null,
  dataVencimento: null,
  valor: null,
  email: null,
  tipoFatura: 0
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.sacadoId) erros.sacadoId = 'Sacado é obrigatório.'
  if (!form.data) erros.data = 'Data é obrigatória.'
  if (!form.dataVencimento) erros.dataVencimento = 'Vencimento é obrigatório.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  if (form.tipoFatura == null) erros.tipoFatura = 'Tipo é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/servicos-financeiros/faturas', { method: 'POST', body: { ...form } })
    toast.success('Fatura criada com sucesso!')
    router.push('/erp/financeiro/servicos-financeiros/faturas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/servicos-financeiros/faturas')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de fatura não é suportada pela API.')
    router.replace('/erp/financeiro/servicos-financeiros/faturas/novo')
  }
  const [sacados, grupos] = await Promise.all([
    carregarOpcoesDe('/servicos-financeiros/sacados', ['nome', 'documento']),
    carregarOpcoesDe('/servicos-financeiros/grupos-recorrencia', ['descricao'])
  ])
  opcoesSacado.value = sacados
  opcoesGrupo.value = grupos
})
</script>

<template>
  <div>
    <PageToolbar title="Nova fatura de cobrança" :loading="salvando">
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
          <SelectField v-model="form.sacadoId" label="Sacado" required :options="opcoesSacado" :error="erros.sacadoId" />
          <SelectField v-model="form.grupoRecorrenciaId" label="Grupo de recorrência" :options="opcoesGrupo" />
          <SelectField v-model="form.tipoFatura" label="Tipo de fatura" required :options="OPCOES_TIPO_FATURA" :clearable="false" :error="erros.tipoFatura" />
          <TextField v-model="form.referencia" label="Referência" maxlength="60" />
          <TextField v-model="form.numeroDocumento" label="Número do documento" maxlength="30" />
          <DateTimeField v-model="form.data" label="Data" mode="datetime" required :error="erros.data" />
          <DateTimeField v-model="form.dataVencimento" label="Vencimento" mode="datetime" required :error="erros.dataVencimento" />
          <MoneyInput v-model="form.valor" label="Valor" :error="erros.valor" />
          <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
