<script setup lang="ts">
/**
 * Desligamentos — RH / Desenvolvimento.
 * Fonte: POST /rh/desenvolvimento/desligamentos. A API expõe apenas criação (sem GET/PUT/DELETE),
 * então esta é uma tela de registro de desligamento. colaboradorId usa o select de colaboradores.
 */
import { reactive, ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface DesligamentoForm {
  colaboradorId: string
  tipoDesligamentoId: string | null
  dataAviso: string | null
  dataDesligamento: string | null
  motivo: string | null
  descricao: string | null
  documento: string | null
}

const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<DesligamentoForm>({
  colaboradorId: '',
  tipoDesligamentoId: null,
  dataAviso: null,
  dataDesligamento: null,
  motivo: null,
  descricao: null,
  documento: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  return Object.keys(erros).length === 0
}

function limpar() {
  Object.assign(form, {
    colaboradorId: '',
    tipoDesligamentoId: null,
    dataAviso: null,
    dataDesligamento: null,
    motivo: null,
    descricao: null,
    documento: null
  })
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/desenvolvimento/desligamentos', { method: 'POST', body: form })
    toast.success('Desligamento registrado com sucesso!')
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
    <PageToolbar title="Desligamentos" subtitle="Registro de desligamento de colaborador">
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
          <!-- TODO: sem endpoint de listagem para Tipo de desligamento no digest — UUID manual. -->
          <TextField v-model="form.tipoDesligamentoId" label="Tipo de desligamento (UUID)" placeholder="UUID" />
          <DateTimeField v-model="form.dataAviso" label="Data do aviso" />
          <DateTimeField v-model="form.dataDesligamento" label="Data do desligamento" />
          <TextField v-model="form.motivo" label="Motivo" maxlength="200" />
          <TextField v-model="form.documento" label="Documento" maxlength="150" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
