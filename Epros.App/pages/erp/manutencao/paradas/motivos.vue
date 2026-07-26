<script setup lang="ts">
/**
 * Cadastro de Motivo de Parada — Manutenção / Paradas / Motivos.
 * POST /manutencao/paradas/motivos. A API não expõe listagem de motivos, portanto esta tela
 * cobre apenas o cadastro de um novo motivo.
 */
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface MotivoForm {
  codigo: string
  descricao: string
  motivoPaiId: string | null
  tipoParadaAplicavel: string | null
  exigeObservacao: boolean
  exigeAnexo: boolean
}

const router = useRouter()
const toast = useToast()
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<MotivoForm>({
  codigo: '',
  descricao: '',
  motivoPaiId: null,
  tipoParadaAplicavel: null,
  exigeObservacao: false,
  exigeAnexo: false
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/paradas/motivos', { method: 'POST', body: form })
    toast.success('Motivo cadastrado com sucesso!')
    router.push('/erp/manutencao/paradas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/paradas')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo motivo de parada" subtitle="Cadastro de motivos aplicáveis às paradas">
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
          <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" />
          <TextField v-model="form.tipoParadaAplicavel" label="Tipo de parada aplicável" maxlength="60" />
          <!-- TODO: motivoPaiId sem endpoint de listagem — texto até integração. -->
          <TextField v-model="form.motivoPaiId" label="Motivo pai (ID)" placeholder="UUID (opcional)" />
          <label class="field toggle-row">
            <span class="field-label">Exige observação</span>
            <input v-model="form.exigeObservacao" type="checkbox" />
          </label>
          <label class="field toggle-row">
            <span class="field-label">Exige anexo</span>
            <input v-model="form.exigeAnexo" type="checkbox" />
          </label>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
