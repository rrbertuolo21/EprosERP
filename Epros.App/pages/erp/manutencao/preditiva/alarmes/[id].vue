<script setup lang="ts">
/**
 * Novo Alarme Preditivo — Manutenção / Preditiva / Alarmes.
 * POST /manutencao/preditiva/alarmes. Sem GET/{id} nem PUT/DELETE — apenas registro manual.
 */
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface AlarmeForm {
  monitoramentoId: string
  pontoMedicaoId: string
  regraId: string
  leituraId: string | null
  severidade: string | null
  descricao: string | null
}

const router = useRouter()
const toast = useToast()
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<AlarmeForm>({
  monitoramentoId: '',
  pontoMedicaoId: '',
  regraId: '',
  leituraId: null,
  severidade: null,
  descricao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.monitoramentoId) erros.monitoramentoId = 'Monitoramento é obrigatório.'
  if (!form.pontoMedicaoId) erros.pontoMedicaoId = 'Ponto de medição é obrigatório.'
  if (!form.regraId) erros.regraId = 'Regra é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/preditiva/alarmes', { method: 'POST', body: form })
    toast.success('Alarme registrado com sucesso!')
    router.push('/erp/manutencao/preditiva/alarmes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/preditiva/alarmes')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo alarme preditivo">
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
          <!-- TODO: uuids sem endpoint de listagem no módulo — texto até integração. -->
          <TextField v-model="form.monitoramentoId" label="Monitoramento (ID)" required placeholder="UUID" :error="erros.monitoramentoId" />
          <TextField v-model="form.pontoMedicaoId" label="Ponto de medição (ID)" required placeholder="UUID" :error="erros.pontoMedicaoId" />
          <TextField v-model="form.regraId" label="Regra (ID)" required placeholder="UUID" :error="erros.regraId" />
          <TextField v-model="form.leituraId" label="Leitura (ID)" placeholder="UUID (opcional)" />
          <TextField v-model="form.severidade" label="Severidade" maxlength="40" />
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
