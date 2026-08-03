<script setup lang="ts">
/**
 * Formulário de Plano de Auditoria (novo) — GRC / Controles Internos e Auditoria.
 * Fonte: POST /api/v1/grc/auditoria/planos. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface PlanoForm {
  codigo: string | null
  titulo: string | null
  descricao: string | null
  ciclo: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const ciclos: SelectOption[] = ['Anual', 'Semestral', 'Trimestral', 'Mensal'].map((c) => ({ label: c, value: c }))

const form = reactive<PlanoForm>({ codigo: null, titulo: null, descricao: null, ciclo: 'Anual' })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/auditoria/planos', { method: 'POST', body: form })
    toast.success('Plano de auditoria cadastrado com sucesso!')
    router.push('/erp/grc/auditoria-planos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/auditoria-planos')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos planos.')
    router.replace('/erp/grc/auditoria-planos')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo plano de auditoria">
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
          <TextField v-model="form.titulo" label="Título" required :error="erros.titulo" maxlength="200" />
          <SelectField v-model="form.ciclo" label="Ciclo" :options="ciclos" />
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
