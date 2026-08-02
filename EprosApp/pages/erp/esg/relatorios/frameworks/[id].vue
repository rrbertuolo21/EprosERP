<script setup lang="ts">
/**
 * Formulário de Framework de Relatório (novo) — ESG / Relatórios / Frameworks.
 * A API expõe só `POST /esg/relatorios/frameworks` (create-only).
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

interface FrameworkForm {
  codigo: string | null
  versao: string | null
  descricao: string | null
  inicioVigencia: string | null
  fimVigencia: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<FrameworkForm>({
  codigo: null,
  versao: null,
  descricao: null,
  inicioVigencia: null,
  fimVigencia: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/relatorios/frameworks', { method: 'POST', body: form })
    toast.success('Framework criado com sucesso!')
    router.push('/erp/esg/relatorios/frameworks')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/relatorios/frameworks')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo framework de relatório">
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
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" placeholder="Ex.: GRI, SASB, TCFD" />
          <TextField v-model="form.versao" label="Versão" />
          <TextField v-model="form.descricao" label="Descrição" />
          <DateTimeField v-model="form.inicioVigencia" label="Início da vigência" />
          <DateTimeField v-model="form.fimVigencia" label="Fim da vigência" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
