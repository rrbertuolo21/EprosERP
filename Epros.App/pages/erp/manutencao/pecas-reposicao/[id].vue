<script setup lang="ts">
/**
 * Registro de Peças de Reposição (novo/detalhe) — Manutenção / Peças de Reposição.
 * - novo: POST /manutencao/pecas-reposicao
 * - edição: GET /manutencao/pecas-reposicao/{id} + itens (POST /{id}/itens)
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { tipoSaidaItemOpcoes } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface RegistroForm {
  codigo: string
  descricao: string
  responsavelId: string
  ordemServicoId: string | null
  planoPreventivoId: string | null
}

interface ItemPeca {
  id?: string
  produtoId?: string | null
  sequencia?: number | null
  quantidade?: number | null
  valorUnitario?: number | null
  valorTotal?: number | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})
const itens = ref<ItemPeca[]>([])

const form = reactive<RegistroForm>({
  codigo: '',
  descricao: '',
  responsavelId: '',
  ordemServicoId: null,
  planoPreventivoId: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/manutencao/pecas-reposicao/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta)
    if (dados) {
      Object.assign(form, {
        codigo: (dados.codigo as string) ?? '',
        descricao: (dados.descricao as string) ?? '',
        responsavelId: (dados.responsavelId as string) ?? '',
        ordemServicoId: (dados.ordemServicoId as string) ?? null,
        planoPreventivoId: (dados.planoPreventivoId as string) ?? null
      })
      itens.value = (dados.itens as ItemPeca[]) ?? []
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    const resposta = await useApi('/manutencao/pecas-reposicao', { method: 'POST', body: form })
    const criado = extrairDados<{ id?: string }>(resposta)
    toast.success('Registro salvo com sucesso!')
    if (criado?.id) router.push(`/erp/manutencao/pecas-reposicao/${criado.id}`)
    else router.push('/erp/manutencao/pecas-reposicao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/pecas-reposicao')
}

// ---- Adicionar item ----
const itemVisivel = ref(false)
const salvandoItem = ref(false)
const formItem = reactive({
  produtoId: '' as string,
  sequencia: 1 as number,
  quantidade: 1 as number,
  ordemServicoId: '' as string,
  gradeId: '' as string,
  valorUnitario: 0 as number,
  taxaDesconto: 0 as number,
  tipoSaida: 0 as number
})

function abrirItem() {
  formItem.produtoId = ''
  formItem.sequencia = itens.value.length + 1
  formItem.quantidade = 1
  formItem.ordemServicoId = ''
  formItem.gradeId = ''
  formItem.valorUnitario = 0
  formItem.taxaDesconto = 0
  formItem.tipoSaida = 0
  itemVisivel.value = true
}

async function salvarItem() {
  if (!formItem.produtoId) {
    toast.error('Informe o ID do produto.')
    return
  }
  salvandoItem.value = true
  try {
    await useApi(`/manutencao/pecas-reposicao/${idParam}/itens`, {
      method: 'POST',
      body: {
        registroId: idParam,
        produtoId: formItem.produtoId,
        sequencia: formItem.sequencia,
        quantidade: formItem.quantidade,
        ordemServicoId: formItem.ordemServicoId || null,
        gradeId: formItem.gradeId || null,
        valorUnitario: formItem.valorUnitario,
        taxaDesconto: formItem.taxaDesconto,
        tipoSaida: formItem.tipoSaida
      }
    })
    toast.success('Item adicionado.')
    itemVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoItem.value = false
  }
}

const colunasItens = [
  { key: 'sequencia', label: 'Seq.' },
  { key: 'produtoId', label: 'Produto' },
  { key: 'quantidade', label: 'Qtd.' },
  { key: 'valorUnitario', label: 'Vlr. unit.' },
  { key: 'valorTotal', label: 'Total' }
]

onMounted(async () => {
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? `Registro ${form.codigo}` : 'Novo registro de peças'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" :disabled="isEdit" />
          <!-- TODO: uuids sem endpoint de listagem no módulo — texto até integração. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID" :error="erros.responsavelId" :disabled="isEdit" />
          <TextField v-model="form.ordemServicoId" label="Ordem de serviço (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.planoPreventivoId" label="Plano preventivo (ID)" placeholder="UUID" :disabled="isEdit" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head">
        <h3>Itens do registro</h3>
        <button type="button" class="btn btn-secondary btn-sm" @click="abrirItem">+ Adicionar item</button>
      </div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th v-for="c in colunasItens" :key="c.key">{{ c.label }}</th></tr></thead>
          <tbody>
            <tr v-if="itens.length === 0"><td :colspan="colunasItens.length"><div class="table-empty">Nenhum item.</div></td></tr>
            <tr v-for="(it, i) in itens" :key="it.id ?? i">
              <td>{{ it.sequencia ?? '-' }}</td>
              <td>{{ it.produtoId ?? '-' }}</td>
              <td>{{ it.quantidade ?? 0 }}</td>
              <td>{{ it.valorUnitario ?? 0 }}</td>
              <td>{{ it.valorTotal ?? 0 }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AppDialog v-model="itemVisivel" title="Adicionar item" width="560px" persistent>
      <div class="dialog-grid">
        <TextField v-model="formItem.produtoId" label="Produto (ID)" placeholder="UUID" required />
        <QuantityInput v-model="formItem.quantidade" label="Quantidade" :min="0" />
        <MoneyInput v-model="formItem.valorUnitario" label="Valor unitário" />
        <PercentInput v-model="formItem.taxaDesconto" label="Desconto" />
        <SelectField v-model="formItem.tipoSaida" label="Tipo de saída" :options="tipoSaidaItemOpcoes" :clearable="false" />
        <TextField v-model="formItem.ordemServicoId" label="Ordem de serviço (ID)" placeholder="UUID" />
        <TextField v-model="formItem.gradeId" label="Grade (ID)" placeholder="UUID" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoItem" @click="itemVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoItem" @click="salvarItem">
          <span v-if="salvandoItem" class="spinner"></span>
          <span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.mt-3 { margin-top: 16px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 14px; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; margin: 0; }
</style>
