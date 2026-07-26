<script setup lang="ts">
/**
 * Nova meta — RH / Talentos.
 * Fonte: POST /rh/talentos/metas. Criação apenas.
 * colaboradorId usa o select; tipoMetaId/ownerId/criadoPorId são UUID manual.
 */
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { useOpcoesRh } from '~/components/rh-comum/useOpcoesRh'

definePageMeta({ layout: 'default' })

interface MetaForm {
  colaboradorId: string
  tipoMetaId: string | null
  titulo: string | null
  descricao: string | null
  dataInicio: string | null
  dataFim: string | null
  alvo: number | null
  progresso: number | null
  ownerId: string
  criadoPorId: string | null
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<MetaForm>({
  colaboradorId: '',
  tipoMetaId: null,
  titulo: null,
  descricao: null,
  dataInicio: null,
  dataFim: null,
  alvo: null,
  progresso: 0,
  ownerId: '',
  criadoPorId: null
})

const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.colaboradorId) erros.colaboradorId = 'Colaborador é obrigatório.'
  if (!form.dataInicio) erros.dataInicio = 'Data início é obrigatória.'
  if (!form.dataFim) erros.dataFim = 'Data fim é obrigatória.'
  if (!form.ownerId) erros.ownerId = 'Responsável (UUID) é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/rh/talentos/metas', {
      method: 'POST',
      body: { ...form, progresso: Number(form.progresso ?? 0) }
    })
    toast.success('Meta criada com sucesso!')
    router.push('/erp/rh/talentos/metas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/talentos/metas')
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova meta">
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
          <TextField v-model="form.titulo" label="Título" maxlength="150" />
          <!-- TODO: sem endpoint de listagem para Tipo de meta no digest — UUID manual. -->
          <TextField v-model="form.tipoMetaId" label="Tipo de meta (UUID)" placeholder="UUID" />
          <DateTimeField v-model="form.dataInicio" label="Data início" required :error="erros.dataInicio" />
          <DateTimeField v-model="form.dataFim" label="Data fim" required :error="erros.dataFim" />
          <QuantityInput v-model="form.alvo" label="Alvo" />
          <QuantityInput v-model="form.progresso" label="Progresso" />
          <!-- TODO: sem endpoint de listagem para Usuário no digest — UUID manual. -->
          <TextField v-model="form.ownerId" label="Responsável (UUID)" required :error="erros.ownerId" placeholder="UUID" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="300" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
