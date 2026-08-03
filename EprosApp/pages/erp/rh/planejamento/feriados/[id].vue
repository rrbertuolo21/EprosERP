<script setup lang="ts">
/**
 * Novo feriado — RH / Planejamento.
 * Fonte: POST /rh/planejamento/feriados. Criação apenas.
 * tipoFeriadoId, criadoPorId e ownerId não têm endpoint de listagem no digest — UUID manual.
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

interface FeriadoForm {
  nome: string | null
  dataInicio: string | null
  dataFim: string | null
  tipoFeriadoId: string | null
  descricao: string | null
  remunerado: boolean
  criadoPorId: string | null
  ownerId: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<FeriadoForm>({
  nome: null,
  dataInicio: null,
  dataFim: null,
  tipoFeriadoId: null,
  descricao: null,
  remunerado: false,
  criadoPorId: null,
  ownerId: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
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
    await useApi('/rh/planejamento/feriados', { method: 'POST', body: form })
    toast.success('Feriado criado com sucesso!')
    router.push('/erp/rh/planejamento/feriados')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/planejamento/feriados')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo feriado">
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
          <TextField v-model="form.nome" label="Nome" required maxlength="120" :error="erros.nome" />
          <DateTimeField v-model="form.dataInicio" label="Data início" />
          <DateTimeField v-model="form.dataFim" label="Data fim" />
          <!-- TODO: sem endpoint de listagem para Tipo de feriado no digest — UUID manual. -->
          <TextField v-model="form.tipoFeriadoId" label="Tipo de feriado (UUID)" placeholder="UUID" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.remunerado ? 'Remunerado' : 'Não remunerado' }}</span>
            <input v-model="form.remunerado" type="checkbox" />
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
