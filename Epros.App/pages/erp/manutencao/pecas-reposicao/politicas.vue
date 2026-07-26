<script setup lang="ts">
/**
 * Política de Estoque de Peça — Manutenção / Peças de Reposição / Políticas.
 * POST /manutencao/pecas-reposicao/politicas. Sem endpoint de listagem — apenas cadastro.
 */
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import { criticidadeOpcoes, numeroOuNulo } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface PoliticaForm {
  produtoId: string
  estoqueMinimo: number | null
  estoqueMaximo: number | null
  pontoPedido: number | null
  leadTimeDias: number | null
  criticidade: string | null
  gradeId: string | null
  localEstoqueId: string | null
}

const router = useRouter()
const toast = useToast()
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<PoliticaForm>({
  produtoId: '',
  estoqueMinimo: null,
  estoqueMaximo: null,
  pontoPedido: null,
  leadTimeDias: null,
  criticidade: null,
  gradeId: null,
  localEstoqueId: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.produtoId) erros.produtoId = 'Produto é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/manutencao/pecas-reposicao/politicas', {
      method: 'POST',
      body: { ...form, leadTimeDias: numeroOuNulo(form.leadTimeDias) }
    })
    toast.success('Política salva com sucesso!')
    router.push('/erp/manutencao/pecas-reposicao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/pecas-reposicao')
}
</script>

<template>
  <div>
    <PageToolbar title="Política de estoque de peça" subtitle="Parâmetros de estoque por produto">
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
          <TextField v-model="form.produtoId" label="Produto (ID)" required placeholder="UUID" :error="erros.produtoId" />
          <QuantityInput v-model="form.estoqueMinimo" label="Estoque mínimo" :min="0" />
          <QuantityInput v-model="form.estoqueMaximo" label="Estoque máximo" :min="0" />
          <QuantityInput v-model="form.pontoPedido" label="Ponto de pedido" :min="0" />
          <TextField v-model="form.leadTimeDias" label="Lead time (dias)" type="number" />
          <SelectField v-model="form.criticidade" label="Criticidade" :options="criticidadeOpcoes" />
          <TextField v-model="form.gradeId" label="Grade (ID)" placeholder="UUID" />
          <TextField v-model="form.localEstoqueId" label="Local de estoque (ID)" placeholder="UUID" />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
