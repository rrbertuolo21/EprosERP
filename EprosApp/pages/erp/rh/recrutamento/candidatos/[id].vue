<script setup lang="ts">
/**
 * Novo candidato — RH / Recrutamento.
 * Fonte: POST /rh/recrutamento/candidatos. Criação apenas.
 * vagaId usa o select de vagas; fonteCandidatoId/criadoPorUsuarioId/donoFuncionalId são
 * UUID manual (sem endpoint de listagem no digest).
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useDocumento } from '~/composables/useDocumento'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface CandidatoForm {
  primeiroNome: string | null
  sobrenome: string | null
  email: string | null
  anosExperiencia: number | null
  vagaId: string
  fonteCandidatoId: string
  criadoPorUsuarioId: string
  donoFuncionalId: string
}

const router = useRouter()
const toast = useToast()
const { validarEmail } = useDocumento()
const { vagas, carregarVagas } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<CandidatoForm>({
  primeiroNome: null,
  sobrenome: null,
  email: null,
  anosExperiencia: null,
  vagaId: '',
  fonteCandidatoId: '',
  criadoPorUsuarioId: '',
  donoFuncionalId: ''
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.primeiroNome) erros.primeiroNome = 'Nome é obrigatório.'
  if (form.email && !validarEmail(form.email)) erros.email = 'E-mail inválido.'
  if (!form.vagaId) erros.vagaId = 'Vaga é obrigatória.'
  if (!form.fonteCandidatoId) erros.fonteCandidatoId = 'Fonte (UUID) é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/recrutamento/candidatos', {
      method: 'POST',
      body: { ...form, anosExperiencia: form.anosExperiencia != null ? Number(form.anosExperiencia) : 0 }
    })
    toast.success('Candidato criado com sucesso!')
    router.push('/erp/rh/recrutamento/candidatos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/recrutamento/candidatos')
}
onMounted(() => {
  void carregarVagas()
})
</script>

<template>
  <div>
    <PageToolbar title="Novo candidato">
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
          <TextField v-model="form.primeiroNome" label="Primeiro nome" required maxlength="80" :error="erros.primeiroNome" />
          <TextField v-model="form.sobrenome" label="Sobrenome" maxlength="80" />
          <TextField v-model="form.email" label="E-mail" type="email" maxlength="150" :error="erros.email" />
          <QuantityInput v-model="form.anosExperiencia" label="Anos de experiência" :decimais="1" />
          <SelectField v-model="form.vagaId" label="Vaga" required :options="vagas" :error="erros.vagaId" />
          <!-- TODO: sem endpoint de listagem para Fonte/Usuário no digest — UUID manual. -->
          <TextField v-model="form.fonteCandidatoId" label="Fonte do candidato (UUID)" required :error="erros.fonteCandidatoId" placeholder="UUID" />
          <TextField v-model="form.criadoPorUsuarioId" label="Criado por (UUID)" placeholder="UUID" />
          <TextField v-model="form.donoFuncionalId" label="Dono funcional (UUID)" placeholder="UUID" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
