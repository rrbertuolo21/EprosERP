<script setup lang="ts">
/**
 * Formulário de Emissão de Carbono (novo) — ESG / Emissões.
 *
 * A API expõe só `POST /esg/emissoes` (sem GET por id nem PUT), então o formulário é
 * create-only. Todos os campos do BODY viram campo com validação de obrigatórios.
 */
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { escopoGhgOptions } from '~/components/esg-comum/statusEsg'

definePageMeta({ layout: 'default' })

interface EmissaoForm {
  fonteEmissao: string | null
  escopo: number | null
  categoriaGhg: string | null
  quantidadeConsumo: number | null
  unidadeMedida: string | null
  fatorEmissao: number | null
  dataTransacao: string | null
}

const router = useRouter()
const toast = useToast()

const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<EmissaoForm>({
  fonteEmissao: null,
  escopo: null,
  categoriaGhg: null,
  quantidadeConsumo: null,
  unidadeMedida: null,
  fatorEmissao: null,
  dataTransacao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.fonteEmissao) erros.fonteEmissao = 'Fonte de emissão é obrigatória.'
  if (form.escopo == null) erros.escopo = 'Escopo é obrigatório.'
  if (form.quantidadeConsumo == null) erros.quantidadeConsumo = 'Consumo é obrigatório.'
  if (form.fatorEmissao == null) erros.fatorEmissao = 'Fator de emissão é obrigatório.'
  if (!form.dataTransacao) erros.dataTransacao = 'Data da transação é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/esg/emissoes', { method: 'POST', body: form })
    toast.success('Emissão registrada com sucesso!')
    router.push('/erp/esg/emissoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/esg/emissoes')
}
</script>

<template>
  <div>
    <PageToolbar title="Nova emissão de carbono">
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
          <TextField v-model="form.fonteEmissao" label="Fonte de emissão" required :error="erros.fonteEmissao" placeholder="Ex.: Combustão estacionária" />
          <SelectField v-model="form.escopo" label="Escopo" required :options="escopoGhgOptions" :error="erros.escopo" />
          <TextField v-model="form.categoriaGhg" label="Categoria GHG" placeholder="Ex.: CombustaoEstacionaria" />
          <QuantityInput v-model="form.quantidadeConsumo" label="Quantidade consumida" required :error="erros.quantidadeConsumo" />
          <TextField v-model="form.unidadeMedida" label="Unidade de medida" placeholder="Ex.: Litro, KWh, Km" />
          <QuantityInput v-model="form.fatorEmissao" label="Fator de emissão (kg CO₂e/unidade)" :decimais="6" required :error="erros.fatorEmissao" />
          <DateTimeField v-model="form.dataTransacao" label="Data da transação" mode="datetime" required :error="erros.dataTransacao" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
</style>
