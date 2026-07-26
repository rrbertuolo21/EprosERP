<script setup lang="ts">
/**
 * Nova marcação de ponto — RH / Ponto.
 * Fonte: POST /rh/ponto/marcacoes. Criação apenas.
 * colaboradorId usa o select de colaboradores; relogioId é UUID manual (sem endpoint).
 * horaMarcacao é TimeSpan (HH:mm:ss) — texto.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface MarcacaoForm {
  colaboradorId: string
  relogioId: string
  nsr: number | null
  dataMarcacao: string | null
  horaMarcacao: string
  tipoMarcacao: string | null
  tipoRegistro: string | null
  parEntradaSaida: string | null
  justificativa: string | null
  origem: string | null
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<MarcacaoForm>({
  colaboradorId: '',
  relogioId: '',
  nsr: null,
  dataMarcacao: null,
  horaMarcacao: '',
  tipoMarcacao: null,
  tipoRegistro: null,
  parEntradaSaida: null,
  justificativa: null,
  origem: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  if (!form.relogioId) erros.relogioId = 'Relógio (UUID) é obrigatório.'
  if (!form.horaMarcacao) erros.horaMarcacao = 'Hora da marcação é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/ponto/marcacoes', {
      method: 'POST',
      body: { ...form, nsr: form.nsr != null ? Number(form.nsr) : null }
    })
    toast.success('Marcação criada com sucesso!')
    router.push('/erp/rh/ponto/marcacoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/ponto/marcacoes')
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova marcação">
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
          <SelectField v-model="form.colaboradorId" label="Colaborador" required :options="colaboradores" :error="erros.colaboradorId" />
          <!-- TODO: sem endpoint de listagem para Relógio no digest — UUID manual. -->
          <TextField v-model="form.relogioId" label="Relógio (UUID)" required :error="erros.relogioId" placeholder="UUID" />
          <TextField v-model="form.nsr" label="NSR" type="number" />
          <DateTimeField v-model="form.dataMarcacao" label="Data da marcação" />
          <TextField v-model="form.horaMarcacao" label="Hora da marcação" required :error="erros.horaMarcacao" placeholder="HH:mm:ss" />
          <TextField v-model="form.tipoMarcacao" label="Tipo de marcação" maxlength="40" />
          <TextField v-model="form.tipoRegistro" label="Tipo de registro" maxlength="40" />
          <TextField v-model="form.parEntradaSaida" label="Par entrada/saída" maxlength="40" />
          <TextField v-model="form.origem" label="Origem" maxlength="40" />
          <TextField v-model="form.justificativa" label="Justificativa" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
