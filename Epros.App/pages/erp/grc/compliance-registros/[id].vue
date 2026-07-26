<script setup lang="ts">
/**
 * Formulário de Registro Regulatório (novo) — GRC / Compliance Regulatório.
 * Fonte: POST /api/v1/grc/compliance/registros. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface RegistroForm {
  codigo: string | null
  descricao: string | null
  norma: string | null
  responsavelId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const form = reactive<RegistroForm>({ codigo: null, descricao: null, norma: null, responsavelId: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável (responsavelId) é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/compliance/registros', { method: 'POST', body: form })
    toast.success('Registro cadastrado com sucesso!')
    router.push('/erp/grc/compliance-registros')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/compliance-registros')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos registros.')
    router.replace('/erp/grc/compliance-registros')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo registro regulatório">
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
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" maxlength="50" />
          <TextField v-model="form.norma" label="Norma" maxlength="150" />
          <!-- responsavelId é UUID; sem endpoint de listagem de usuários no módulo GRC. -->
          <TextField v-model="form.responsavelId" label="Responsável (responsavelId)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <div class="span-2">
            <TextField v-model="form.descricao" label="Descrição" maxlength="1000" />
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
