<script setup lang="ts">
/**
 * Nova vaga — RH / Recrutamento.
 * Fonte: POST /rh/recrutamento/vagas. Criação apenas.
 * filialId/tipoVagaId/localVagaId/criadoPorUsuarioId/donoFuncionalId são UUID manual
 * (sem endpoint de listagem no digest).
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface VagaForm {
  titulo: string | null
  posicoes: number | null
  prioridade: number | null
  descricao: string | null
  habilidades: string | null
  tipoCandidatura: string | null
  urlCandidatura: string | null
  filialId: string
  tipoVagaId: string
  localVagaId: string
  criadoPorUsuarioId: string
  donoFuncionalId: string
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<VagaForm>({
  titulo: null,
  posicoes: 1,
  prioridade: 0,
  descricao: null,
  habilidades: null,
  tipoCandidatura: null,
  urlCandidatura: null,
  filialId: '',
  tipoVagaId: '',
  localVagaId: '',
  criadoPorUsuarioId: '',
  donoFuncionalId: ''
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (!form.filialId) erros.filialId = 'Filial (UUID) é obrigatória.'
  if (!form.tipoVagaId) erros.tipoVagaId = 'Tipo de vaga (UUID) é obrigatório.'
  if (!form.localVagaId) erros.localVagaId = 'Local da vaga (UUID) é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/recrutamento/vagas', {
      method: 'POST',
      body: {
        ...form,
        posicoes: Number(form.posicoes ?? 0),
        prioridade: Number(form.prioridade ?? 0)
      }
    })
    toast.success('Vaga criada com sucesso!')
    router.push('/erp/rh/recrutamento/vagas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/recrutamento/vagas')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova vaga">
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
          <TextField v-model="form.titulo" label="Título" required maxlength="150" :error="erros.titulo" />
          <TextField v-model="form.posicoes" label="Posições" type="number" />
          <TextField v-model="form.prioridade" label="Prioridade" type="number" />
          <TextField v-model="form.tipoCandidatura" label="Tipo de candidatura" maxlength="40" />
          <TextField v-model="form.urlCandidatura" label="URL de candidatura" maxlength="250" />
          <!-- TODO: sem endpoint de listagem para Filial/Tipo de vaga/Local/Usuário no digest — UUID manual. -->
          <TextField v-model="form.filialId" label="Filial (UUID)" required :error="erros.filialId" placeholder="UUID" />
          <TextField v-model="form.tipoVagaId" label="Tipo de vaga (UUID)" required :error="erros.tipoVagaId" placeholder="UUID" />
          <TextField v-model="form.localVagaId" label="Local da vaga (UUID)" required :error="erros.localVagaId" placeholder="UUID" />
          <TextField v-model="form.criadoPorUsuarioId" label="Criado por (UUID)" placeholder="UUID" />
          <TextField v-model="form.donoFuncionalId" label="Dono funcional (UUID)" placeholder="UUID" />
          <TextField v-model="form.habilidades" label="Habilidades" maxlength="300" />
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
