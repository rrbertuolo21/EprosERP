<script setup lang="ts">
/**
 * Formulário de Taxa de Câmbio — Câmbio/Risco.
 * A API expõe apenas POST /cambio-risco/taxas (sem edição). Esta tela cria uma nova taxa.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import { OPCOES_ORIGEM_TAXA, carregarOpcoesDe } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface TaxaForm {
  moedaId: string | null
  dataTaxa: string | null
  taxaCompra: number | null
  taxaVenda: number | null
  origemTaxa: number | null
  observacao: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const isEdit = computed(() => (route.params.id as string) !== 'novo')
const salvando = ref(false)
const opcoesMoeda = ref<SelectOption[]>([])

const form = reactive<TaxaForm>({
  moedaId: null,
  dataTaxa: null,
  taxaCompra: null,
  taxaVenda: null,
  origemTaxa: 0,
  observacao: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.moedaId) erros.moedaId = 'Moeda é obrigatória.'
  if (!form.dataTaxa) erros.dataTaxa = 'Data da taxa é obrigatória.'
  if (form.taxaCompra == null) erros.taxaCompra = 'Taxa de compra é obrigatória.'
  if (form.taxaVenda == null) erros.taxaVenda = 'Taxa de venda é obrigatória.'
  if (form.origemTaxa == null) erros.origemTaxa = 'Origem é obrigatória.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/cambio-risco/taxas', {
      method: 'POST',
      body: {
        moedaId: form.moedaId,
        dataTaxa: form.dataTaxa,
        taxaCompra: form.taxaCompra,
        taxaVenda: form.taxaVenda,
        origemTaxa: form.origemTaxa,
        observacao: form.observacao
      }
    })
    toast.success('Taxa registrada com sucesso!')
    router.push('/erp/financeiro/cambio-risco/taxas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/cambio-risco/taxas')
}

onMounted(async () => {
  if (isEdit.value) {
    toast.error('Edição de taxa não é suportada pela API. Registre uma nova taxa.')
    router.replace('/erp/financeiro/cambio-risco/taxas/novo')
  }
  opcoesMoeda.value = await carregarOpcoesDe('/cambio-risco/moedas', ['nome', 'codigoIso'])
})
</script>

<template>
  <div>
    <PageToolbar title="Nova taxa de câmbio" :loading="salvando">
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
          <SelectField v-model="form.moedaId" label="Moeda" required :options="opcoesMoeda" :error="erros.moedaId" />
          <DateTimeField v-model="form.dataTaxa" label="Data da taxa" mode="datetime" required :error="erros.dataTaxa" />
          <TextField
            :model-value="form.taxaCompra"
            label="Taxa de compra"
            type="number"
            :error="erros.taxaCompra"
            @update:model-value="(v) => (form.taxaCompra = v === '' ? null : Number(v))"
          />
          <TextField
            :model-value="form.taxaVenda"
            label="Taxa de venda"
            type="number"
            :error="erros.taxaVenda"
            @update:model-value="(v) => (form.taxaVenda = v === '' ? null : Number(v))"
          />
          <SelectField v-model="form.origemTaxa" label="Origem" required :options="OPCOES_ORIGEM_TAXA" :clearable="false" :error="erros.origemTaxa" />
          <TextField v-model="form.observacao" label="Observação" maxlength="200" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
