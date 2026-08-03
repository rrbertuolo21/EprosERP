<script setup lang="ts">
/**
 * Formulário de Fator de Emissão GEE (novo) — ESG / GHG / Fatores.
 * A API expõe só `POST /esg/ghg/fatores` (create-only).
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface FatorForm {
  codigo: string | null
  versao: string | null
  fonteReferencia: string | null
  valor: number | null
  unidadeEntrada: string | null
  unidadeSaida: string | null
  inicioVigencia: string | null
  fimVigencia: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<FatorForm>({
  codigo: null,
  versao: null,
  fonteReferencia: null,
  valor: null,
  unidadeEntrada: null,
  unidadeSaida: null,
  inicioVigencia: null,
  fimVigencia: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.codigo) erros.codigo = 'Código é obrigatório.'
  if (form.valor == null) erros.valor = 'Valor é obrigatório.'
  if (!form.inicioVigencia) erros.inicioVigencia = 'Início da vigência é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/ghg/fatores', { method: 'POST', body: form })
    toast.success('Fator de emissão registrado com sucesso!')
    router.push('/erp/esg/ghg/fatores')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/ghg/fatores')
}
</script>

<template>
  <div>
    <PageToolbar title="Novo fator de emissão GEE">
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
          <TextField v-model="form.codigo" label="Código" required :error="erros.codigo" />
          <TextField v-model="form.versao" label="Versão" placeholder="Ex.: 2024.1" />
          <TextField v-model="form.fonteReferencia" label="Fonte de referência" placeholder="Ex.: GHG Protocol, MCTI" />
          <QuantityInput v-model="form.valor" label="Valor do fator" :decimais="6" required :error="erros.valor" />
          <TextField v-model="form.unidadeEntrada" label="Unidade de entrada" placeholder="Ex.: Litro" />
          <TextField v-model="form.unidadeSaida" label="Unidade de saída" placeholder="Ex.: kg CO₂e" />
          <DateTimeField v-model="form.inicioVigencia" label="Início da vigência" required :error="erros.inicioVigencia" />
          <DateTimeField v-model="form.fimVigencia" label="Fim da vigência" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
