<script setup lang="ts">
/**
 * Formulário de Devolução (Economia Circular) — ESG / ECO / Devoluções.
 * A API expõe só `POST /esg/eco/devolucoes` (create-only).
 * O BODY tem `itens (array<obj>)`, cujo formato não é detalhado pelo digest — os itens
 * não são editados aqui (ver relatório: lacuna) e o cabeçalho é enviado sem `itens`.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface DevolucaoForm {
  tipo: string | null
  numeroNf: string | null
  chaveNfEntrada: string | null
  contactId: string | null
  naturezaId: string | null
  valorIntegral: number | null
  valorDevolvido: number | null
  valorFrete: number | null
  valorDesconto: number | null
  motivo: string | null
  observacao: string | null
  estado: string | null
  devolucaoParcial: boolean
  businessId: string | null
  locationId: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)

const form = reactive<DevolucaoForm>({
  tipo: null,
  numeroNf: null,
  chaveNfEntrada: null,
  contactId: null,
  naturezaId: null,
  valorIntegral: null,
  valorDevolvido: null,
  valorFrete: null,
  valorDesconto: null,
  motivo: null,
  observacao: null,
  estado: null,
  devolucaoParcial: false,
  businessId: null,
  locationId: null
})

async function salvar() {
  salvando.value = true
  try {
    await useApi('/esg/eco/devolucoes', { method: 'POST', body: form })
    toast.success('Devolução importada com sucesso!')
    router.push('/erp/esg/eco/devolucoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/eco/devolucoes')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova devolução">
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
          <TextField v-model="form.tipo" label="Tipo" />
          <TextField v-model="form.numeroNf" label="Número da NF" />
          <TextField v-model="form.chaveNfEntrada" label="Chave da NF de entrada" maxlength="44" />
          <TextField v-model="form.estado" label="Estado" />
          <MoneyInput v-model="form.valorIntegral" label="Valor integral" />
          <MoneyInput v-model="form.valorDevolvido" label="Valor devolvido" />
          <MoneyInput v-model="form.valorFrete" label="Valor do frete" />
          <MoneyInput v-model="form.valorDesconto" label="Valor do desconto" />
          <TextField v-model="form.motivo" label="Motivo" />
          <TextField v-model="form.observacao" label="Observação" />
          <!-- TODO: contactId / naturezaId / businessId / locationId são uuid; sem endpoint de listagem, texto. -->
          <TextField v-model="form.contactId" label="Contato (UUID)" />
          <TextField v-model="form.naturezaId" label="Natureza (UUID)" />
          <TextField v-model="form.businessId" label="Empresa (UUID)" />
          <TextField v-model="form.locationId" label="Local (UUID)" />
          <label class="field toggle-row">
            <span class="field-label">{{ form.devolucaoParcial ? 'Devolução parcial' : 'Devolução total' }}</span>
            <input v-model="form.devolucaoParcial" type="checkbox" />
          </label>
        </div>
        <p class="hint-itens">Os itens da devolução (array) não são detalhados pela API e não são editados nesta tela.</p>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; justify-content: flex-start; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
.hint-itens { margin-top: 16px; font-size: 12.5px; color: var(--text-secondary); }
</style>
