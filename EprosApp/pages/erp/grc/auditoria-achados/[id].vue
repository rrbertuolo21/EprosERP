<script setup lang="ts">
/**
 * Formulário de Achado de Auditoria (novo) — GRC / Controles Internos e Auditoria.
 * Fonte: POST /api/v1/grc/auditoria/achados. Apenas criação.
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

interface AchadoForm {
  testeControleId: string | null
  titulo: string | null
  descricao: string | null
  severidade: string | null
  prazoRemediacao: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const severidades: SelectOption[] = ['Baixa', 'Media', 'Alta', 'Critica'].map((s) => ({ label: s, value: s }))

const form = reactive<AchadoForm>({ testeControleId: null, titulo: null, descricao: null, severidade: 'Media', prazoRemediacao: null })
const erros = reactive<Record<string, string>>({})
const salvando = ref(false)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
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
    await useApi('/grc/auditoria/achados', { method: 'POST', body: form })
    toast.success('Achado registrado com sucesso!')
    router.push('/erp/grc/auditoria-achados')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/grc/auditoria-achados')
}

onMounted(() => {
  if ((route.params.id as string) !== 'novo') {
    toast.error('Edição não disponível neste módulo. Apenas cadastro de novos achados.')
    router.replace('/erp/grc/auditoria-achados')
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Novo achado de auditoria">
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
          <SelectField v-model="form.severidade" label="Severidade" :options="severidades" />
          <DateTimeField v-model="form.prazoRemediacao" label="Prazo de remediação" mode="datetime" />
          <!-- testeControleId é UUID opcional; sem endpoint de listagem de testes de controle. -->
          <TextField v-model="form.testeControleId" label="Teste de controle (testeControleId)" hint="UUID do teste (opcional)" />
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
