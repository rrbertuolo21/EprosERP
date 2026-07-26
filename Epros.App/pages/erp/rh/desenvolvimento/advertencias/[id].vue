<script setup lang="ts">
/**
 * Nova advertência — RH / Desenvolvimento.
 * Fonte: POST /rh/desenvolvimento/advertencias. Criação apenas.
 * colaboradorId usa o select de colaboradores; tipoAdvertenciaId e advertidoPor são UUID
 * manual (sem endpoint de listagem no digest).
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface AdvertenciaForm {
  colaboradorId: string
  tipoAdvertenciaId: string | null
  assunto: string | null
  severidade: string | null
  dataAdvertencia: string | null
  descricao: string | null
  documento: string | null
  advertidoPor: string | null
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<AdvertenciaForm>({
  colaboradorId: '',
  tipoAdvertenciaId: null,
  assunto: null,
  severidade: null,
  dataAdvertencia: null,
  descricao: null,
  documento: null,
  advertidoPor: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/desenvolvimento/advertencias', { method: 'POST', body: form })
    toast.success('Advertência criada com sucesso!')
    router.push('/erp/rh/desenvolvimento/advertencias')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/desenvolvimento/advertencias')
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova advertência">
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
          <SelectField v-model="form.colaboradorId" label="Colaborador" required :options="colaboradores" :error="erros.colaboradorId" />
          <!-- TODO: sem endpoint de listagem para Tipo de advertência no digest — UUID manual. -->
          <TextField v-model="form.tipoAdvertenciaId" label="Tipo de advertência (UUID)" placeholder="UUID" />
          <TextField v-model="form.assunto" label="Assunto" maxlength="150" />
          <TextField v-model="form.severidade" label="Severidade" maxlength="40" />
          <DateTimeField v-model="form.dataAdvertencia" label="Data da advertência" />
          <!-- TODO: sem endpoint de listagem para Usuário no digest — UUID manual. -->
          <TextField v-model="form.advertidoPor" label="Advertido por (UUID)" placeholder="UUID" />
          <TextField v-model="form.documento" label="Documento" maxlength="150" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
