<script setup lang="ts">
/**
 * PdvCliente — identificação do consumidor na NFC-e (opcional).
 *
 * Porta o essencial de `components/pos/cliente.vue`: informar CPF/CNPJ do consumidor,
 * alternar "enviar destinatário na nota" e limpar. A busca de parceiro cadastrado é
 * simplificada para digitação direta do documento (o cadastro de parceiros é outra fatia);
 * o documento é mascarado com `useMask` e validado com `useDocumento`.
 */
import { computed, nextTick, onMounted, ref } from 'vue'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import { useToast } from '~/composables/useToast'
import type { DestinatarioPdv } from './tipos'

const destinatario = defineModel<DestinatarioPdv>('destinatario', { required: true })

const { maskCpfCnpj, somenteDigitos } = useMask()
const { validarCpfCnpj } = useDocumento()
const toast = useToast()

const docRef = ref<HTMLInputElement | null>(null)

const documentoMascarado = computed({
  get: () => maskCpfCnpj(destinatario.value.documentoConsumidor),
  set: (v: string) => {
    destinatario.value.documentoConsumidor = somenteDigitos(v)
    destinatario.value.enviarNaNfce = destinatario.value.documentoConsumidor.length > 0
  }
})

function validarDocumento() {
  const doc = destinatario.value.documentoConsumidor
  if (doc && !validarCpfCnpj(doc)) {
    toast.warning('CPF/CNPJ do consumidor inválido.')
  }
}

function limpar() {
  destinatario.value.pessoaId = null
  destinatario.value.documentoConsumidor = ''
  destinatario.value.descricao = ''
  destinatario.value.enviarNaNfce = false
  focar()
}

function focar() {
  nextTick(() => docRef.value?.focus())
}

onMounted(focar)

defineExpose({ limpar, focar, validarDocumento })
</script>

<template>
  <div class="pdv-cliente">
    <div class="field">
      <label class="field-label">CPF/CNPJ do consumidor</label>
      <input
        ref="docRef"
        v-model="documentoMascarado"
        class="input"
        type="text"
        inputmode="numeric"
        placeholder="Informe o CPF ou CNPJ (opcional)"
        @blur="validarDocumento"
      />
      <span class="field-hint">Deixe em branco para consumidor não identificado.</span>
    </div>

    <label class="pdv-switch">
      <input v-model="destinatario.enviarNaNfce" type="checkbox" />
      <span>Enviar CPF/CNPJ na nota (F6)</span>
    </label>

    <div v-if="destinatario.descricao" class="pdv-cliente-nome">
      {{ destinatario.descricao }}
    </div>

    <button type="button" class="btn btn-secondary btn-limpar" @click="limpar">Limpar consumidor (DEL)</button>
  </div>
</template>

<style scoped>
.pdv-cliente { display: flex; flex-direction: column; gap: 14px; }
.pdv-switch { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--text-secondary); cursor: pointer; }
.pdv-switch input { width: 16px; height: 16px; accent-color: var(--primary); }
.pdv-cliente-nome { font-size: 13px; font-weight: 600; color: var(--success); }
.btn-limpar { align-self: flex-start; }
</style>
