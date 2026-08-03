<script setup lang="ts">
/**
 * Formulário de Colaborador (novo) — RH / Colaboradores.
 *
 * Fonte: POST /rh/colaboradores. A API não expõe GET/{id} nem PUT, então a tela é de
 * criação apenas (rota `.../novo`).
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface ColaboradorForm {
  nome: string | null
  cpf: string
  email: string | null
  cargo: string | null
  departamento: string | null
  salarioBase: number | null
  dataAdmissao: string | null
}

const router = useRouter()
const toast = useToast()
const { maskCPF } = useMask()
const { validarCPF, validarEmail } = useDocumento()

const salvando = ref(false)
const form = reactive<ColaboradorForm>({
  nome: null,
  cpf: '',
  email: null,
  cargo: null,
  departamento: null,
  salarioBase: null,
  dataAdmissao: null
})

const erros = reactive<Record<string, string>>({})
function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  if (!form.cpf) erros.cpf = 'CPF é obrigatório.'
  else if (!validarCPF(form.cpf)) erros.cpf = 'CPF inválido.'
  if (form.email && !validarEmail(form.email)) erros.email = 'E-mail inválido.'
  if (!form.dataAdmissao) erros.dataAdmissao = 'Data de admissão é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/colaboradores', { method: 'POST', body: form })
    toast.success('Colaborador criado com sucesso!')
    router.push('/erp/rh/colaboradores')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/rh/colaboradores')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo colaborador">
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
          <TextField v-model="form.nome" label="Nome" required maxlength="150" :error="erros.nome" />
          <TextField
            v-model="form.cpf"
            label="CPF"
            required
            :error="erros.cpf"
            @update:model-value="(v) => (form.cpf = maskCPF(v as string))"
          />
          <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" :error="erros.email" />
          <TextField v-model="form.cargo" label="Cargo" maxlength="100" />
          <TextField v-model="form.departamento" label="Departamento" maxlength="100" />
          <MoneyInput v-model="form.salarioBase" label="Salário base" />
          <DateTimeField v-model="form.dataAdmissao" label="Data de admissão" required :error="erros.dataAdmissao" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
