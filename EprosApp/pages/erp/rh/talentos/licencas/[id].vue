<script setup lang="ts">
/**
 * Nova licença — RH / Talentos.
 * Fonte: POST /rh/talentos/licencas. Criação apenas.
 * colaboradorId usa o select; tipoLicencaId/ownerId/criadoPorId são UUID manual.
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

interface LicencaForm {
  colaboradorId: string
  tipoLicencaId: string | null
  dataInicio: string | null
  dataFim: string | null
  totalDias: number | null
  motivo: string | null
  anexo: string | null
  ownerId: string | null
  criadoPorId: string | null
}

const router = useRouter()
const toast = useToast()
const { colaboradores, carregarColaboradores } = useOpcoesRh()

const salvando = ref(false)
const form = reactive<LicencaForm>({
  colaboradorId: '',
  tipoLicencaId: null,
  dataInicio: null,
  dataFim: null,
  totalDias: null,
  motivo: null,
  anexo: null,
  ownerId: null,
  criadoPorId: null
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
    await useApi('/rh/talentos/licencas', {
      method: 'POST',
      body: { ...form, totalDias: form.totalDias != null ? Number(form.totalDias) : null }
    })
    toast.success('Licença criada com sucesso!')
    router.push('/erp/rh/talentos/licencas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
function cancelar() {
  router.push('/erp/rh/talentos/licencas')
}
onMounted(() => {
  void carregarColaboradores()
})
</script>

<template>
  <div>
    <PageToolbar title="Nova licença">
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
          <!-- TODO: sem endpoint de listagem para Tipo de licença no digest — UUID manual. -->
          <TextField v-model="form.tipoLicencaId" label="Tipo de licença (UUID)" placeholder="UUID" />
          <DateTimeField v-model="form.dataInicio" label="Data início" />
          <DateTimeField v-model="form.dataFim" label="Data fim" />
          <TextField v-model="form.totalDias" label="Total de dias" type="number" />
          <TextField v-model="form.motivo" label="Motivo" maxlength="200" />
          <TextField v-model="form.anexo" label="Anexo" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
