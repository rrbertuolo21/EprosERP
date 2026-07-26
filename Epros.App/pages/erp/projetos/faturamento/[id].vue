<script setup lang="ts">
/**
 * Faturamento de projeto — criação e detalhe com ações e itens.
 * POST /projetos/faturamento · GET /projetos/faturamento/{id}.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import {
  rotuloStatusWorkflow, fmtMoeda,
  MODALIDADE_FATURAMENTO_OPCOES, TIPO_ITEM_FATURAMENTO_OPCOES
} from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface ItemFat {
  id: string; sequencia?: number; quantidade?: number | null; tipoItem?: number | null
  valorUnitario?: number | null; valorTotal?: number | null; observacao?: string | null
}
interface Faturamento {
  id: string; codigo?: string | null; descricao?: string | null; status?: number | null
  projetoId?: string | null; responsavelId?: string | null; clienteId?: string | null
  modalidadeFaturamento?: number | null; moeda?: string | null; valorTotal?: number | null
  dataVencimento?: string | null; motivoRejeicao?: string | null; versao?: number | null; itens?: ItemFat[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const item = ref<Faturamento | null>(null)

const form = reactive({
  codigo: '', descricao: '', projetoId: '', responsavelId: '', clienteId: '',
  modalidadeFaturamento: null as number | null, moeda: 'BRL', dataVencimento: null as string | null
})
const erros = reactive<Record<string, string>>({})
function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.projetoId) erros.projetoId = 'Projeto é obrigatório.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const r = await useApi(`/projetos/faturamento/${idParam}`)
    item.value = extrairDados<Faturamento>(r) ?? null
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { carregando.value = false }
}
async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/projetos/faturamento', {
      method: 'POST',
      body: { ...form, clienteId: form.clienteId || null }
    })
    toast.success('Faturamento criado com sucesso!')
    router.push('/erp/projetos/faturamento')
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}
function voltar() { router.push('/erp/projetos/faturamento') }

const acaoSalvando = ref(false)
async function acaoSimples(acao: 'submeter' | 'aprovar') {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/faturamento/${idParam}/${acao}`, { method: 'POST' })
    toast.success('Ação executada.'); await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

const rejDialog = ref(false)
const motivo = ref('')
async function rejeitar() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/faturamento/${idParam}/rejeitar`, { method: 'POST', body: { motivo: motivo.value } })
    toast.success('Faturamento rejeitado.'); rejDialog.value = false; motivo.value = ''; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

const itemDialog = ref(false)
const itemForm = reactive({
  sequencia: 1, quantidade: 0, observacao: '', tipoItem: null as number | null,
  valorUnitario: 0, valorTotal: 0, origemTipo: '', origemId: ''
})
async function adicionarItem() {
  acaoSalvando.value = true
  try {
    await useApi(`/projetos/faturamento/${idParam}/itens`, {
      method: 'POST',
      body: {
        sequencia: itemForm.sequencia, quantidade: itemForm.quantidade, observacao: itemForm.observacao || null,
        tipoItem: itemForm.tipoItem, valorUnitario: itemForm.valorUnitario, valorTotal: itemForm.valorTotal,
        origemTipo: itemForm.origemTipo || null, origemId: itemForm.origemId || null
      }
    })
    toast.success('Item adicionado.'); itemDialog.value = false; await carregar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

function rotuloTipoItem(v: unknown): string {
  const f = TIPO_ITEM_FATURAMENTO_OPCOES.find((o) => String(o.value) === String(v))
  return f ? f.label : '—'
}
onMounted(carregar)
</script>

<template>
  <div>
    <template v-if="!isEdit">
      <PageToolbar title="Novo faturamento">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
          </button>
        </template>
      </PageToolbar>
      <div class="glass-panel form-panel">
        <div class="form-grid">
          <TextField v-model="form.projetoId" label="Projeto (ID)" required :error="erros.projetoId" hint="UUID do projeto" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required :error="erros.responsavelId" hint="UUID do responsável" />
          <TextField v-model="form.clienteId" label="Cliente (ID)" hint="UUID do cliente (opcional)" />
          <TextField v-model="form.codigo" label="Código" maxlength="40" />
          <SelectField v-model="form.modalidadeFaturamento" label="Modalidade" :options="MODALIDADE_FATURAMENTO_OPCOES" />
          <TextField v-model="form.moeda" label="Moeda" maxlength="3" />
          <DateTimeField v-model="form.dataVencimento" label="Data de vencimento" mode="datetime" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="500" />
        </div>
      </div>
    </template>

    <template v-else>
      <PageToolbar :title="item?.codigo || 'Faturamento'" :subtitle="rotuloStatusWorkflow(item?.status)" :loading="carregando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        </template>
      </PageToolbar>

      <div v-if="item" class="glass-panel form-panel">
        <div class="detail-grid">
          <div><span class="dl">Descrição</span><span class="dv">{{ item.descricao || '—' }}</span></div>
          <div><span class="dl">Projeto (ID)</span><span class="dv">{{ item.projetoId || '—' }}</span></div>
          <div><span class="dl">Valor total</span><span class="dv">{{ fmtMoeda(item.valorTotal) }}</span></div>
          <div><span class="dl">Moeda</span><span class="dv">{{ item.moeda || '—' }}</span></div>
          <div v-if="item.motivoRejeicao"><span class="dl">Motivo rejeição</span><span class="dv">{{ item.motivoRejeicao }}</span></div>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head"><h3>Ações</h3></div>
        <div class="btn-row">
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('submeter')">Submeter</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="acaoSimples('aprovar')">Aprovar</button>
          <button type="button" class="btn btn-ghost btn-sm" @click="rejDialog = true">Rejeitar</button>
        </div>
      </div>

      <div class="glass-panel form-panel mt-2">
        <div class="section-head">
          <h3>Itens de faturamento</h3>
          <button type="button" class="btn btn-primary btn-sm" @click="itemDialog = true">+ Item</button>
        </div>
        <table class="admin-table">
          <thead><tr><th>Seq.</th><th>Tipo</th><th class="td-right">Qtd.</th><th class="td-right">Vlr. unit.</th><th class="td-right">Vlr. total</th></tr></thead>
          <tbody>
            <tr v-if="!item?.itens?.length"><td colspan="5"><div class="table-empty">Nenhum item.</div></td></tr>
            <tr v-for="it in item?.itens ?? []" :key="it.id">
              <td>{{ it.sequencia }}</td>
              <td>{{ rotuloTipoItem(it.tipoItem) }}</td>
              <td class="td-right">{{ Number(it.quantidade ?? 0).toLocaleString('pt-BR') }}</td>
              <td class="td-right">{{ fmtMoeda(it.valorUnitario) }}</td>
              <td class="td-right">{{ fmtMoeda(it.valorTotal) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <AppDialog v-model="rejDialog" title="Rejeitar faturamento" width="480px">
      <TextField v-model="motivo" label="Motivo" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="rejDialog = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="acaoSalvando" @click="rejeitar">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Rejeitar</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="itemDialog" title="Adicionar item de faturamento" width="640px">
      <div class="form-grid">
        <QuantityInput v-model="itemForm.sequencia" label="Sequência" :decimais="0" />
        <SelectField v-model="itemForm.tipoItem" label="Tipo de item" :options="TIPO_ITEM_FATURAMENTO_OPCOES" />
        <QuantityInput v-model="itemForm.quantidade" label="Quantidade" :decimais="2" />
        <MoneyInput v-model="itemForm.valorUnitario" label="Valor unitário" />
        <MoneyInput v-model="itemForm.valorTotal" label="Valor total" />
        <TextField v-model="itemForm.origemTipo" label="Origem (tipo)" />
        <TextField v-model="itemForm.origemId" label="Origem (ID)" hint="UUID (opcional)" />
        <TextField v-model="itemForm.observacao" label="Observação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="itemDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="adicionarItem">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.detail-grid .dl { display: block; font-size: 12px; color: var(--text-secondary); }
.detail-grid .dv { display: block; font-size: 14px; color: var(--text-primary); font-weight: 600; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; }
.btn-row { display: flex; gap: 8px; flex-wrap: wrap; }
.mt-2 { margin-top: 16px; }
</style>
