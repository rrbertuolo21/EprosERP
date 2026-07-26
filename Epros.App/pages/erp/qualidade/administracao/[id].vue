<script setup lang="ts">
/**
 * Formulário de Administração da Qualidade (QLD-ADM) — criação.
 *
 * Fonte: POST /qualidade/administracao. Sem GET detalhe/PUT → somente criação.
 */
import { ref, computed, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface AdmForm {
  codigo: string | null
  descricao: string | null
  responsavelId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isNovo = computed(() => idParam === 'novo')
const salvando = ref(false)

const form = reactive<AdmForm>({
  codigo: null,
  descricao: null,
  responsavelId: null
})

const erros = reactive<Record<string, string>>({})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigo) erros.codigo = 'O código é obrigatório.'
  if (!form.descricao) erros.descricao = 'A descrição é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'O responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/qualidade/administracao', { method: 'POST', body: form })
    toast.success('Registro criado com sucesso!')
    router.push('/erp/qualidade/administracao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/qualidade/administracao')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo registro de administração">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando || !isNovo" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="!isNovo" class="glass-panel form-panel">
      <p>A edição não está disponível (o backend só expõe criação). Volte para a
        <a href="/erp/qualidade/administracao">listagem</a>.</p>
    </div>

    <div v-else class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <!-- responsavelId é uuid; sem endpoint no digest → input de texto. TODO: SelectField quando houver rota. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID do responsável" :error="erros.responsavelId" />
        </div>
        <div class="form-full">
          <label class="field-label">Descrição<span class="required">*</span></label>
          <textarea v-model="form.descricao" class="input textarea" rows="4" :class="{ 'is-invalid': !!erros.descricao }"></textarea>
          <span v-if="erros.descricao" class="field-error">{{ erros.descricao }}</span>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.form-full { margin-top: 16px; display: flex; flex-direction: column; gap: 6px; }
.textarea { min-height: 96px; resize: vertical; font-family: inherit; }
</style>
