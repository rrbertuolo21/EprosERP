<script setup lang="ts">
/**
 * Timesheets — RH.
 * Fonte: POST /rh/timesheets. A API expõe apenas criação (sem GET/PUT/DELETE) — tela de
 * apontamento de horas. colaboradorId usa o select de colaboradores.
 */
import { reactive, ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface TimesheetForm {
  colaboradorId: string
  data: string | null
  horasTrabalhadas: number | null
  descricaoAtividade: string | null
}

const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<TimesheetForm>({
  colaboradorId: '',
  data: null,
  horasTrabalhadas: null,
  descricaoAtividade: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  if (!form.data) erros.data = 'Data é obrigatória.'
  if (form.horasTrabalhadas == null) erros.horasTrabalhadas = 'Horas trabalhadas é obrigatória.'
  return Object.keys(erros).length === 0
}

function limpar() {
  Object.assign(form, { colaboradorId: '', data: null, horasTrabalhadas: null, descricaoAtividade: null })
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/timesheets', {
      method: 'POST',
      body: { ...form, horasTrabalhadas: Number(form.horasTrabalhadas ?? 0) }
    })
    toast.success('Apontamento registrado com sucesso!')
    limpar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Timesheets" subtitle="Apontamento de horas trabalhadas">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="limpar">Limpar</button>
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
          <DateTimeField v-model="form.data" label="Data" required :error="erros.data" />
          <QuantityInput v-model="form.horasTrabalhadas" label="Horas trabalhadas" :decimais="2" suffix="h" :error="erros.horasTrabalhadas" />
          <TextField v-model="form.descricaoAtividade" label="Descrição da atividade" maxlength="300" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
