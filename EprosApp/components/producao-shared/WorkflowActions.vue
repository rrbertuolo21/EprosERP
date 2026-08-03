<script setup lang="ts">
/**
 * WorkflowActions — botões de transição de workflow para agregados de produção
 * (EStatusWorkflowProducao: submeter/aprovar/rejeitar/inativar/reativar/encerrar).
 *
 * Componente LOCAL do módulo produção. Deriva as ações do status atual (via helper
 * `acoesWorkflow`), pede confirmação (ConfirmDialog) e — no caso de "rejeitar" — coleta
 * o motivo num diálogo. Emite `acao` com a chave e o motivo opcional; a página dona faz
 * o POST `/{raiz}/{id}/{chave}`.
 *
 * Props:
 *   status: number | string | null   (status atual do agregado)
 *   loading?: boolean                 (desabilita durante a chamada)
 * Emits:
 *   acao: [chave: string, motivo?: string]
 */
import { ref } from 'vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { acoesWorkflow, type AcaoWorkflow } from './producao'

const props = withDefaults(
  defineProps<{
    status?: number | string | null
    loading?: boolean
  }>(),
  { loading: false }
)

const emit = defineEmits<{
  acao: [chave: string, motivo?: string]
}>()

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const motivoVisivel = ref(false)
const motivo = ref('')
const acaoPendente = ref<AcaoWorkflow | null>(null)

async function acionar(acao: AcaoWorkflow) {
  if (acao.pedeMotivo) {
    motivo.value = ''
    acaoPendente.value = acao
    motivoVisivel.value = true
    return
  }
  const ok = await confirmRef.value?.open(
    `${acao.rotulo}?`,
    `Confirmar a ação "${acao.rotulo}" para este registro?`,
    { textoConfirmar: acao.rotulo, danger: acao.danger }
  )
  if (ok) emit('acao', acao.chave)
}

function confirmarMotivo() {
  if (!acaoPendente.value) return
  emit('acao', acaoPendente.value.chave, motivo.value)
  motivoVisivel.value = false
  acaoPendente.value = null
}
</script>

<template>
  <div class="workflow-actions">
    <button
      v-for="acao in acoesWorkflow(status)"
      :key="acao.chave"
      type="button"
      class="btn btn-sm"
      :class="acao.danger ? 'btn-danger' : 'btn-primary'"
      :disabled="loading"
      @click="acionar(acao)"
    >
      {{ acao.rotulo }}
    </button>

    <ConfirmDialog ref="confirmRef" />

    <AppDialog v-model="motivoVisivel" title="Rejeitar registro" width="440px" persistent>
      <TextField v-model="motivo" label="Motivo da rejeição" placeholder="Descreva o motivo..." />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="motivoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="!motivo" @click="confirmarMotivo">Rejeitar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.workflow-actions { display: inline-flex; gap: 8px; flex-wrap: wrap; }
</style>
