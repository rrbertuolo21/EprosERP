<script setup lang="ts">
/**
 * Formulário de Fluxo Circular (novo) — ESG / ECO / Fluxos.
 * A API expõe só `POST /esg/eco/fluxos` (create-only).
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface FluxoForm {
  codigo: string | null
  descricao: string | null
  tipo: string | null
  devolucaoId: string | null
  responsavelId: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<FluxoForm>({
  codigo: null,
  descricao: null,
  tipo: null,
  devolucaoId: null,
  responsavelId: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/eco/fluxos', { method: 'POST', body: form })
    toast.success('Fluxo circular criado com sucesso!')
    router.push('/erp/esg/eco/fluxos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/eco/fluxos')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo fluxo circular">
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
          <TextField v-model="form.codigo" label="Código" />
          <TextField v-model="form.tipo" label="Tipo" placeholder="Ex.: Reciclagem, Reuso" />
          <TextField v-model="form.descricao" label="Descrição" />
          <!-- TODO: responsavelId / devolucaoId são uuid; sem endpoint de listagem, mantidos como texto. -->
          <TextField v-model="form.responsavelId" label="Responsável (UUID)" required :error="erros.responsavelId" />
          <TextField v-model="form.devolucaoId" label="Devolução de origem (UUID)" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
