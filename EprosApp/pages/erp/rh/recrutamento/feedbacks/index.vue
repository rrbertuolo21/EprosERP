<script setup lang="ts">
/**
 * Feedbacks de entrevista — RH / Recrutamento.
 * Fonte: POST /rh/recrutamento/feedbacks. A API expõe apenas criação (sem GET/PUT/DELETE) —
 * tela de registro de feedback de entrevista.
 * entrevistaId e usuários são UUID manual (sem endpoint de listagem no digest).
 */
import { reactive, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface FeedbackForm {
  entrevistaId: string
  notaTecnica: number | null
  notaComunicacao: number | null
  notaAderenciaCultural: number | null
  recomendacao: string | null
  pontosFortes: string | null
  pontosFracos: string | null
  comentarios: string | null
  entrevistadoresJson: string | null
  criadoPorUsuarioId: string
  donoFuncionalId: string
}

const toast = useToast()

const salvando = ref(false)
const form = reactive<FeedbackForm>({
  entrevistaId: '',
  notaTecnica: null,
  notaComunicacao: null,
  notaAderenciaCultural: null,
  recomendacao: null,
  pontosFortes: null,
  pontosFracos: null,
  comentarios: null,
  entrevistadoresJson: null,
  criadoPorUsuarioId: '',
  donoFuncionalId: ''
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.entrevistaId) erros.entrevistaId = 'Entrevista (UUID) é obrigatória.'
  return Object.keys(erros).length === 0
}

function limpar() {
  Object.assign(form, {
    entrevistaId: '',
    notaTecnica: null,
    notaComunicacao: null,
    notaAderenciaCultural: null,
    recomendacao: null,
    pontosFortes: null,
    pontosFracos: null,
    comentarios: null,
    entrevistadoresJson: null,
    criadoPorUsuarioId: '',
    donoFuncionalId: ''
  })
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/recrutamento/feedbacks', {
      method: 'POST',
      body: {
        ...form,
        notaTecnica: Number(form.notaTecnica ?? 0),
        notaComunicacao: Number(form.notaComunicacao ?? 0),
        notaAderenciaCultural: Number(form.notaAderenciaCultural ?? 0)
      }
    })
    toast.success('Feedback registrado com sucesso!')
    limpar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Feedbacks de entrevista" subtitle="Avaliação de candidatos em entrevistas">
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
          <!-- TODO: sem endpoint de listagem para Entrevista/Usuário no digest — UUID manual. -->
          <TextField v-model="form.entrevistaId" label="Entrevista (UUID)" required :error="erros.entrevistaId" placeholder="UUID" />
          <QuantityInput v-model="form.notaTecnica" label="Nota técnica" :decimais="1" />
          <QuantityInput v-model="form.notaComunicacao" label="Nota comunicação" :decimais="1" />
          <QuantityInput v-model="form.notaAderenciaCultural" label="Nota aderência cultural" :decimais="1" />
          <TextField v-model="form.recomendacao" label="Recomendação" maxlength="60" />
          <TextField v-model="form.criadoPorUsuarioId" label="Criado por (UUID)" placeholder="UUID" />
          <TextField v-model="form.donoFuncionalId" label="Dono funcional (UUID)" placeholder="UUID" />
          <TextField v-model="form.pontosFortes" label="Pontos fortes" maxlength="300" />
          <TextField v-model="form.pontosFracos" label="Pontos fracos" maxlength="300" />
          <TextField v-model="form.comentarios" label="Comentários" maxlength="300" />
          <TextField v-model="form.entrevistadoresJson" label="Entrevistadores (JSON)" maxlength="500" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
