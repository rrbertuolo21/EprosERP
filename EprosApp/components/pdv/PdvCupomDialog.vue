<script setup lang="ts">
/**
 * PdvCupomDialog — exibe o cupom (DANFE NFC-e) após a emissão, com opção de imprimir/baixar
 * e cancelar a nota. Substitui `components/pos/NfceDialog.vue` do legado usando o DanfeViewer
 * compartilhado. O PDF é carregado como Blob pela página e passado em `src`.
 */
import { ref } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DanfeViewer from '~/components/shared/DanfeViewer.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import type { DocumentoEmitido } from './tipos'

defineProps<{
  modelValue: boolean
  documento: DocumentoEmitido
  pdf: Blob | string | null
  carregandoPdf?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  imprimir: []
  baixar: []
  cancelar: [motivo: string]
  fechar: []
}>()

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

async function solicitarCancelamento() {
  const ok = await confirmRef.value?.open(
    'Cancelar NFC-e',
    'Confirma o cancelamento desta NFC-e junto à SEFAZ? Esta ação é irreversível.',
    { danger: true, textoConfirmar: 'Cancelar NFC-e' }
  )
  if (ok) emit('cancelar', 'Cancelamento solicitado pelo operador de caixa')
}

function fechar() {
  emit('update:modelValue', false)
  emit('fechar')
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="NFC-e emitida"
    width="720px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="cupom-info">
      <span v-if="documento.numero" class="chip chip-success">Nº {{ documento.numero }}</span>
      <span v-if="documento.chave" class="cupom-chave">Chave: {{ documento.chave }}</span>
    </div>

    <div v-if="carregandoPdf" class="cupom-carregando">
      <span class="spinner"></span> Gerando cupom...
    </div>
    <DanfeViewer
      v-else
      :src="pdf"
      title="Cupom NFC-e"
      height="60vh"
      @print="emit('imprimir')"
      @download="emit('baixar')"
    />

    <template #footer>
      <button type="button" class="btn btn-danger" @click="solicitarCancelamento">Cancelar NFC-e</button>
      <button type="button" class="btn btn-primary" @click="fechar">Nova venda</button>
    </template>

    <ConfirmDialog ref="confirmRef" />
  </AppDialog>
</template>

<style scoped>
.cupom-info { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 10px; }
.cupom-chave { font-size: 12px; color: var(--text-secondary); word-break: break-all; }
.cupom-carregando { display: flex; align-items: center; gap: 8px; justify-content: center; padding: 48px 0; color: var(--text-secondary); font-size: 13px; }
</style>
