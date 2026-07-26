<script setup lang="ts">
/**
 * Novo período de ponto — RH / Ponto.
 * Fonte: POST /rh/ponto/periodos. Criação apenas.
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

interface PeriodoForm {
  empresaId: string
  competencia: string | null
  dataInicio: string | null
  dataFim: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const form = reactive<PeriodoForm>({
  empresaId: '',
  competencia: null,
  dataInicio: null,
  dataFim: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId) erros.empresaId = 'Empresa (UUID) é obrigatória.'
  if (!form.dataInicio) erros.dataInicio = 'Data início é obrigatória.'
  if (!form.dataFim) erros.dataFim = 'Data fim é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/ponto/periodos', { method: 'POST', body: form })
    toast.success('Período criado com sucesso!')
    router.push('/erp/rh/ponto/periodos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/ponto/periodos')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo período">
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
          <DateTimeField v-model="form.dataInicio" label="Data início" required :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataFim" label="Data fim" required :error="erros.dataFim" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
