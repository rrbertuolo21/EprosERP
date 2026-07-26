<script setup lang="ts">
/**
 * Nova rubrica de folha — RH / Folha.
 * Fonte: POST /rh/folha/rubricas. Criação apenas.
 * empresaId não tem endpoint de listagem no digest — UUID manual.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface RubricaForm {
  empresaId: string
  codigo: string | null
  nome: string | null
  descricao: string | null
  tipo: string | null
  unidade: string | null
  baseCalculo: string | null
  taxa: number | null
  ativo: boolean
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<RubricaForm>({
  empresaId: '',
  codigo: null,
  nome: null,
  descricao: null,
  tipo: null,
  unidade: null,
  baseCalculo: null,
  taxa: null,
  ativo: true
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa (UUID) é obrigatória.'
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/folha/rubricas', { method: 'POST', body: form })
    toast.success('Rubrica criada com sucesso!')
    router.push('/erp/rh/folha/rubricas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/folha/rubricas')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova rubrica">
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
          <TextField v-model="form.codigo" label="Código" maxlength="20" />
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <TextField v-model="form.tipo" label="Tipo" maxlength="40" />
          <TextField v-model="form.unidade" label="Unidade" maxlength="20" />
          <TextField v-model="form.baseCalculo" label="Base de cálculo" maxlength="60" />
          <PercentInput v-model="form.taxa" label="Taxa" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.ativo ? 'Ativo' : 'Inativo' }}</span>
            <input v-model="form.ativo" type="checkbox" />
          </label>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; justify-content: flex-start; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
