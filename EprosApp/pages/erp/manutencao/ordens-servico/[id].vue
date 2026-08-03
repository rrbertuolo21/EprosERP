<script setup lang="ts">
/**
 * Ordem de Serviço (nova/edição) — Manutenção / Ordens de Serviço.
 * - novo: POST /manutencao/ordens-servico
 * - edição: GET /manutencao/ordens-servico/{id} + itens (POST /{id}/itens) + status (POST /{id}/status)
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
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import {
  perfilOrdemOpcoes,
  tipoPessoaOpcoes,
  tipoItemOsOpcoes,
  tipoSaidaItemOpcoes,
  statusOrdemServicoOpcoes
} from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface OrdemServicoForm {
  perfilOrdem: number
  tipoPessoa: number
  pessoaId: string
  data: string | null
  garantia: boolean
  tipoAtendimentoId: string | null
  tipoEquipamentoId: string | null
  marcaId: string | null
  colaboradorId: string | null
  numero: string | null
}

interface ItemOs {
  id?: string
  produtoId?: string | null
  complemento?: string | null
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
const itens = ref<ItemOs[]>([])
const statusAtual = ref<number | null>(null)

const form = reactive<OrdemServicoForm>({
  perfilOrdem: 0,
  tipoPessoa: 1,
  pessoaId: '',
  data: null,
  garantia: false,
  tipoAtendimentoId: null,
  tipoEquipamentoId: null,
  marcaId: null,
  colaboradorId: null,
  numero: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.pessoaId) erros.pessoaId = 'Pessoa é obrigatória.'
  if (!form.data) erros.data = 'Data é obrigatória.'
  if (form.perfilOrdem === 1 && !form.colaboradorId) erros.colaboradorId = 'No perfil Campo o colaborador é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/manutencao/ordens-servico/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta)
    if (dados) {
      Object.assign(form, {
        perfilOrdem: (dados.perfilOrdem as number) ?? 0,
        tipoPessoa: (dados.tipoPessoa as number) ?? 1,
        pessoaId: (dados.pessoaId as string) ?? '',
        data: (dados.data as string) ?? null,
        garantia: (dados.garantia as boolean) ?? false,
        numero: (dados.numero as string) ?? null
      })
      statusAtual.value = (dados.statusCodigo as number) ?? null
      itens.value = (dados.itens as ItemOs[]) ?? []
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
    const resposta = await useApi('/manutencao/ordens-servico', { method: 'POST', body: form })
    const criado = extrairDados<{ id?: string }>(resposta)
    toast.success('OS aberta com sucesso!')
    if (criado?.id) router.push(`/erp/manutencao/ordens-servico/${criado.id}`)
    else router.push('/erp/manutencao/ordens-servico')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/ordens-servico')
}

// ---- Adicionar item ----
const itemVisivel = ref(false)
const salvandoItem = ref(false)
const formItem = reactive({
  produtoId: '' as string,
  tipo: 0 as number,
  complemento: '' as string,
  quantidade: 1 as number,
  valorUnitario: 0 as number,
  taxaDesconto: 0 as number,
  tipoSaida: 0 as number,
  gradeId: '' as string
})

function abrirItem() {
  formItem.produtoId = ''
  formItem.tipo = 0
  formItem.complemento = ''
  formItem.quantidade = 1
  formItem.valorUnitario = 0
  formItem.taxaDesconto = 0
  formItem.tipoSaida = 0
  formItem.gradeId = ''
  itemVisivel.value = true
}

async function salvarItem() {
  salvandoItem.value = true
  try {
    await useApi(`/manutencao/ordens-servico/${idParam}/itens`, {
      method: 'POST',
      body: {
        ordemServicoId: idParam,
        produtoId: formItem.produtoId || null,
        tipo: formItem.tipo,
        complemento: formItem.complemento || null,
        quantidade: formItem.quantidade,
        valorUnitario: formItem.valorUnitario,
        taxaDesconto: formItem.taxaDesconto,
        tipoSaida: formItem.tipoSaida,
        gradeId: formItem.gradeId || null
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

// ---- Mudar status ----
const statusVisivel = ref(false)
const salvandoStatus = ref(false)
const novoStatus = ref<number | null>(null)

function abrirStatus() {
  novoStatus.value = statusAtual.value
  statusVisivel.value = true
}

async function salvarStatus() {
  if (novoStatus.value == null) {
    toast.error('Selecione o novo status.')
    return
  }
  salvandoStatus.value = true
  try {
    await useApi(`/manutencao/ordens-servico/${idParam}/status`, {
      method: 'POST',
      body: { ordemServicoId: idParam, novoStatus: novoStatus.value }
    })
    toast.success('Status atualizado.')
    statusVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoStatus.value = false
  }
}

const colunasItens = [
  { key: 'produtoId', label: 'Produto' },
  { key: 'complemento', label: 'Complemento' },
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
    <PageToolbar :title="isEdit ? `Ordem de serviço ${form.numero ?? ''}` : 'Nova ordem de serviço'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
        <button v-else type="button" class="btn btn-primary" @click="abrirStatus">Alterar status</button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField v-model="form.perfilOrdem" label="Perfil da ordem" required :options="perfilOrdemOpcoes" :clearable="false" :disabled="isEdit" />
          <SelectField v-model="form.tipoPessoa" label="Tipo de pessoa" required :options="tipoPessoaOpcoes" :clearable="false" :disabled="isEdit" />
          <!-- TODO: pessoaId/colaboradorId/*Id sem endpoint de listagem no módulo — texto até integração. -->
          <TextField v-model="form.pessoaId" label="Pessoa (ID)" required placeholder="UUID" :error="erros.pessoaId" :disabled="isEdit" />
          <DateTimeField v-model="form.data" label="Data" mode="datetime" required :error="erros.data" :disabled="isEdit" />
          <TextField v-model="form.numero" label="Número" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.colaboradorId" label="Colaborador (ID)" placeholder="UUID" :error="erros.colaboradorId" :disabled="isEdit" />
          <TextField v-model="form.tipoAtendimentoId" label="Tipo de atendimento (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.tipoEquipamentoId" label="Tipo de equipamento (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.marcaId" label="Marca (ID)" placeholder="UUID" :disabled="isEdit" />
          <label class="field toggle-row">
            <span class="field-label">Garantia</span>
            <input v-model="form.garantia" type="checkbox" :disabled="isEdit" />
          </label>
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head">
        <h3>Itens da OS</h3>
        <button type="button" class="btn btn-secondary btn-sm" @click="abrirItem">+ Adicionar item</button>
      </div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead>
            <tr><th v-for="c in colunasItens" :key="c.key">{{ c.label }}</th></tr>
          </thead>
          <tbody>
            <tr v-if="itens.length === 0"><td :colspan="colunasItens.length"><div class="table-empty">Nenhum item.</div></td></tr>
            <tr v-for="(it, i) in itens" :key="it.id ?? i">
              <td>{{ it.produtoId ?? '-' }}</td>
              <td>{{ it.complemento ?? '-' }}</td>
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
        <TextField v-model="formItem.produtoId" label="Produto (ID)" placeholder="UUID" />
        <SelectField v-model="formItem.tipo" label="Tipo" :options="tipoItemOsOpcoes" :clearable="false" />
        <TextField v-model="formItem.complemento" label="Complemento" />
        <SelectField v-model="formItem.tipoSaida" label="Tipo de saída" :options="tipoSaidaItemOpcoes" :clearable="false" />
        <QuantityInput v-model="formItem.quantidade" label="Quantidade" :min="0" />
        <MoneyInput v-model="formItem.valorUnitario" label="Valor unitário" />
        <PercentInput v-model="formItem.taxaDesconto" label="Desconto" />
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

    <AppDialog v-model="statusVisivel" title="Alterar status da OS" width="440px" persistent>
      <SelectField v-model="novoStatus" label="Novo status" :options="statusOrdemServicoOpcoes" :clearable="false" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoStatus" @click="statusVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoStatus" @click="salvarStatus">
          <span v-if="salvandoStatus" class="spinner"></span>
          <span v-else>Salvar</span>
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
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; margin: 0; }
</style>
