<script setup lang="ts">
/**
 * Formulário de Regra SoD (nova) — GRC / Segregação de Funções.
 * Fonte: POST /api/v1/grc/sod/regras. Apenas criação.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface RegraForm {
  codigo: string | null
  funcaoAId: string | null
  funcaoBId: string | null
  criticidade: string | null
  vigenciaInicio: string | null
  vigenciaFim: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const criticidades: SelectOption[] = ['Baixa', 'Media', 'Alta', 'Critica'].map((c) => ({ label: c, value: c }))

const form = reactive<RegraForm>({
  codigo: null,
  funcaoAId: null,
  funcaoBId: null,
  criticidade: 'Alta',
  vigenciaInicio: null,
  vigenciaFim: null
})
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.funcaoAId) erros.funcaoAId = 'Função A é obrigatória.'
  if (!form.funcaoBId) erros.funcaoBId = 'Função B é obrigatória.'
  if (!form.vigenciaInicio) erros.vigenciaInicio = 'Início da vigência é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/grc/sod/regras', { method: 'POST', body: form })
    toast.success('Regra SoD cadastrada com sucesso!')
    router.push('/erp/grc/sod-regras')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/sod-regras')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novas regras.')
    router.replace('/erp/grc/sod-regras')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Nova regra SoD">
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
          <TextField v-model="form.codigo" label="Código" maxlength="50" />
          <SelectField v-model="form.criticidade" label="Criticidade" :options="criticidades" />
          <!-- funcaoAId / funcaoBId são UUID; não há endpoint GET de funções SoD exposto. -->
          <TextField v-model="form.funcaoAId" label="Função A (funcaoAId)" required :error="erros.funcaoAId" hint="UUID da função A" />
          <TextField v-model="form.funcaoBId" label="Função B (funcaoBId)" required :error="erros.funcaoBId" hint="UUID da função B" />
          <DateTimeField v-model="form.vigenciaInicio" label="Vigência início" mode="datetime" required :error="erros.vigenciaInicio" />
          <DateTimeField v-model="form.vigenciaFim" label="Vigência fim" mode="datetime" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
