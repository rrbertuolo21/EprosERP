<script setup lang="ts">
/**
 * Nova competência de folha — RH / Folha.
 * Fonte: POST /rh/folha/competencias. Criação apenas.
 * empresaId não tem endpoint de listagem no digest — UUID manual.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface CompetenciaForm {
  empresaId: string
  competencia: string | null
  tipo: string | null
  periodoInicio: string | null
  periodoFim: string | null
  descricao: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<CompetenciaForm>({
  empresaId: '',
  competencia: null,
  tipo: null,
  periodoInicio: null,
  periodoFim: null,
  descricao: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa (UUID) é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/folha/competencias', { method: 'POST', body: form })
    toast.success('Competência criada com sucesso!')
    router.push('/erp/rh/folha/competencias')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/folha/competencias')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova competência">
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
          <!-- TODO: sem endpoint de listagem para Empresa no digest — UUID manual. -->
          <TextField v-model="form.empresaId" label="Empresa (UUID)" required :error="erros.empresaId" placeholder="UUID" />
          <TextField v-model="form.competencia" label="Competência" maxlength="20" placeholder="MM/AAAA" />
          <TextField v-model="form.tipo" label="Tipo" maxlength="40" />
          <DateTimeField v-model="form.periodoInicio" label="Período início" />
          <DateTimeField v-model="form.periodoFim" label="Período fim" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
