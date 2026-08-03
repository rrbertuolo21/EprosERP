<script setup lang="ts">
/**
 * Formulário de Risco Corporativo (novo) — GRC / Gestão de Riscos.
 *
 * Fonte: POST /api/v1/grc/riscos.
 * O backend não expõe GET/{id} nem PUT, portanto o formulário é apenas de criação.
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

interface RiscoForm {
  titulo: string | null
  descricao: string | null
  categoria: string | null
  probabilidade: number | null
  impacto: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const categorias: SelectOption[] = ['Operacional', 'Financeiro', 'Legal', 'Seguranca'].map((c) => ({ label: c, value: c }))
const escala: SelectOption[] = [1, 2, 3, 4, 5].map((n) => ({ label: String(n), value: n }))

const form = reactive<RiscoForm>({
  titulo: null,
  descricao: null,
  categoria: 'Operacional',
  probabilidade: null,
  impacto: null
})

const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.titulo) erros.titulo = 'Título é obrigatório.'
  if (form.probabilidade == null) erros.probabilidade = 'Probabilidade é obrigatória.'
  if (form.impacto == null) erros.impacto = 'Impacto é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/riscos', { method: 'POST', body: form })
    toast.success('Risco cadastrado com sucesso!')
    router.push('/erp/grc/riscos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/riscos')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos riscos.')
    router.replace('/erp/grc/riscos')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo risco">
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
          <TextField v-model="form.titulo" label="Título" required :error="erros.titulo" maxlength="200" />
          <SelectField v-model="form.categoria" label="Categoria" :options="categorias" />
          <SelectField v-model="form.probabilidade" label="Probabilidade (1 a 5)" required :options="escala" :error="erros.probabilidade" />
          <SelectField v-model="form.impacto" label="Impacto (1 a 5)" required :options="escala" :error="erros.impacto" />
          <div class="span-2">
            <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
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
