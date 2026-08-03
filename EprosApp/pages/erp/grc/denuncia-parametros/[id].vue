<script setup lang="ts">
/**
 * Formulário de Parâmetro do Canal de Denúncias (novo) — GRC.
 * Fonte: POST /api/v1/grc/denuncias/parametros. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface ParametroForm {
  chave: string | null
  valorJson: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const form = reactive<ParametroForm>({ chave: null, valorJson: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.chave) erros.chave = 'Chave é obrigatória.'
  if (form.valorJson) {
    try {
      JSON.parse(form.valorJson)
    } catch {
      erros.valorJson = 'Valor deve ser um JSON válido.'
    }
  }
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/denuncias/parametros', { method: 'POST', body: form })
    toast.success('Parâmetro salvo com sucesso!')
    router.push('/erp/grc/denuncia-parametros')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/denuncia-parametros')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos parâmetros.')
    router.replace('/erp/grc/denuncia-parametros')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo parâmetro">
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
          <TextField v-model="form.chave" label="Chave" required :error="erros.chave" maxlength="150" />
          <div class="span-2">
            <TextField v-model="form.valorJson" label="Valor (JSON)" :error="erros.valorJson" placeholder='{"exemplo": true}' />
          </div>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.span-2 { grid-column: 1 / -1; }
</style>
