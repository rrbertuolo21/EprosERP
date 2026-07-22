<script setup lang="ts">
/**
 * Formulário de Plano de Contas Financeiro (novo/edição) — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/plano-de-contas/[id].vue` do legado: descrição,
 * máscara, configurações de natureza para pagamento/recebimento e tabela de itens do plano.
 *
 * Diferença de contrato com o legado: no backend novo os itens do plano NÃO são enviados
 * embutidos no POST/PUT do plano — são uma sub-entidade com CRUD próprio em
 * `/plano-de-contas-financeiro-itens` (Criar/Atualizar/Deletar individuais). O plano
 * precisa existir (ser salvo) antes de permitir adicionar itens.
 */
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const TIPO_DETALHAMENTO_OPCOES: SelectOption[] = [
  { label: 'Crédito', value: 0 },
  { label: 'Débito', value: 1 }
]

interface NaturezaFinanceira {
  id: string
  descricao: string
  tipoConfiguracaoNatureza: number
}

interface PlanoItem {
  id: string
  codigo: string
  descricao: string
  tipoDetalhamento: number
  movimentaCaixa: boolean
}

interface PlanoForm {
  id: string | null
  descricao: string
  mascara: string
  configuracaoCodigoNaturezaFinanceiraPagamentoId: string | null
  configuracaoCodigoNaturezaFinanceiraRecebimentoId: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const planoId = computed(() => (isEdit.value ? idParam : null))

const carregando = ref(false)
const salvando = ref(false)
const naturezas = ref<NaturezaFinanceira[]>([])
const itensPlano = ref<PlanoItem[]>([])
const carregandoItens = ref(false)

const form = reactive<PlanoForm>({
  id: isEdit.value ? idParam : null,
  descricao: '',
  mascara: '',
  configuracaoCodigoNaturezaFinanceiraPagamentoId: null,
  configuracaoCodigoNaturezaFinanceiraRecebimentoId: null
})

const erros = reactive<Record<string, string>>({})

const opcoesPagamento = computed<SelectOption[]>(() =>
  naturezas.value.filter((n) => n.tipoConfiguracaoNatureza === 2).map((n) => ({ label: n.descricao, value: n.id }))
)
const opcoesRecebimento = computed<SelectOption[]>(() =>
  naturezas.value.filter((n) => n.tipoConfiguracaoNatureza === 1).map((n) => ({ label: n.descricao, value: n.id }))
)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (!form.mascara) erros.mascara = 'Máscara é obrigatória.'
  return Object.keys(erros).length === 0
}

/** Restringe a digitação da máscara aos caracteres 9 e . (mesma regra do legado). */
function restringirMascara(ev: KeyboardEvent) {
  if (!['9', '.'].includes(ev.key)) ev.preventDefault()
}

async function carregarNaturezas() {
  try {
    const resposta = await useApi<{ dados?: { itens: NaturezaFinanceira[] } }>(
      '/configuracao-codigo-naturezas-financeiras',
      { query: { tamanhoPagina: 200 } }
    )
    const dados = extrairDados<{ itens: NaturezaFinanceira[] }>(resposta)
    naturezas.value = dados?.itens ?? []
  } catch (e) {
    console.error('[plano-de-contas/[id]] naturezas', e)
    naturezas.value = []
  }
}

async function carregarItens() {
  if (!planoId.value) {
    itensPlano.value = []
    return
  }
  carregandoItens.value = true
  try {
    const resposta = await useApi<{ dados?: { itens: PlanoItem[] } }>('/plano-de-contas-financeiro-itens', {
      query: { planoDeContasId: planoId.value, tamanhoPagina: 500 }
    })
    const dados = extrairDados<{ itens: PlanoItem[] }>(resposta)
    itensPlano.value = dados?.itens ?? []
  } catch (e) {
    console.error('[plano-de-contas/[id]] itens', e)
    itensPlano.value = []
  } finally {
    carregandoItens.value = false
  }
}

async function carregarPlano() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/plano-de-contas-financeiro/{id}`, { params: { id: idParam } })
    const dados = extrairDados<Partial<PlanoForm>>(resposta)
    if (dados) Object.assign(form, dados)
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
    const payload = {
      descricao: form.descricao,
      mascara: form.mascara,
      configuracaoCodigoNaturezaFinanceiraPagamentoId: form.configuracaoCodigoNaturezaFinanceiraPagamentoId,
      configuracaoCodigoNaturezaFinanceiraRecebimentoId: form.configuracaoCodigoNaturezaFinanceiraRecebimentoId
    }

    if (isEdit.value) {
      await useApi('/plano-de-contas-financeiro/{id}', { method: 'PUT', params: { id: idParam }, body: { id: idParam, ...payload } })
      toast.success('Plano de contas salvo com sucesso!')
    } else {
      const resposta = await useApi<{ dados?: { id: string } }>('/plano-de-contas-financeiro', { method: 'POST', body: payload })
      const criado = extrairDados<{ id: string }>(resposta)
      toast.success('Plano de contas criado com sucesso! Agora adicione os itens.')
      if (criado?.id) {
        router.replace(`/erp/financeiro/plano-de-contas/${criado.id}`)
        return
      }
    }
    router.push('/erp/financeiro/plano-de-contas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/plano-de-contas')
}

// --- Gestão de itens do plano (sub-CRUD) ---

const itemForm = reactive<{ id: string | null; codigo: string; descricao: string; tipoDetalhamento: number | null; movimentaCaixa: boolean }>({
  id: null,
  codigo: '',
  descricao: '',
  tipoDetalhamento: null,
  movimentaCaixa: false
})
const salvandoItem = ref(false)

const colunasItens: DataTableColumn<PlanoItem>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'tipoDetalhamento', label: 'Detalhamento', sortable: false, width: '140px' },
  { key: 'movimentaCaixa', label: 'Movimenta Caixa', sortable: false, align: 'center', width: '140px' }
]

function limparItemForm() {
  itemForm.id = null
  itemForm.codigo = ''
  itemForm.descricao = ''
  itemForm.tipoDetalhamento = null
  itemForm.movimentaCaixa = false
}

function editarItem(item: PlanoItem) {
  itemForm.id = item.id
  itemForm.codigo = item.codigo
  itemForm.descricao = item.descricao
  itemForm.tipoDetalhamento = item.tipoDetalhamento
  itemForm.movimentaCaixa = item.movimentaCaixa
}

async function salvarItem() {
  if (!planoId.value) {
    toast.error('Salve o plano de contas antes de adicionar itens.')
    return
  }
  if (!itemForm.codigo || !itemForm.descricao || itemForm.tipoDetalhamento == null) {
    toast.error('Preencha código, descrição e detalhamento do item.')
    return
  }

  salvandoItem.value = true
  try {
    if (itemForm.id) {
      await useApi('/plano-de-contas-financeiro-itens/{id}', {
        method: 'PUT',
        params: { id: itemForm.id },
        body: {
          id: itemForm.id,
          codigo: itemForm.codigo,
          descricao: itemForm.descricao,
          tipoDetalhamento: itemForm.tipoDetalhamento,
          movimentaCaixa: itemForm.movimentaCaixa
        }
      })
    } else {
      await useApi('/plano-de-contas-financeiro-itens', {
        method: 'POST',
        body: {
          planoDeContasFinanceiroId: planoId.value,
          codigo: itemForm.codigo,
          descricao: itemForm.descricao,
          tipoDetalhamento: itemForm.tipoDetalhamento,
          movimentaCaixa: itemForm.movimentaCaixa
        }
      })
    }
    toast.success('Item salvo com sucesso!')
    limparItemForm()
    await carregarItens()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoItem.value = false
  }
}

const excluirItemVisivel = ref(false)
const excluindoItem = ref(false)
const itemParaExcluir = ref<PlanoItem | null>(null)

function pedirExclusaoItem(item: PlanoItem) {
  itemParaExcluir.value = item
  excluirItemVisivel.value = true
}

async function confirmarExclusaoItem() {
  if (!itemParaExcluir.value) return
  excluindoItem.value = true
  try {
    await useApi('/plano-de-contas-financeiro-itens/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Item excluído com sucesso.')
    excluirItemVisivel.value = false
    itemParaExcluir.value = null
    await carregarItens()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindoItem.value = false
  }
}

function descricaoDetalhamento(tipo: number): string {
  return TIPO_DETALHAMENTO_OPCOES.find((o) => o.value === tipo)?.label ?? ''
}

watch(planoId, () => {
  void carregarItens()
})

onMounted(async () => {
  await carregarNaturezas()
  await carregarPlano()
  await carregarItens()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar plano de contas' : 'Novo plano de contas'" :loading="carregando">
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
          <TextField v-model="form.descricao" label="Descrição" required maxlength="150" :error="erros.descricao" />
          <TextField
            v-model="form.mascara"
            label="Máscara"
            required
            hint="Exemplo: 9.9.9.99.9999 (Ex. Conta 1.1.1.11.0001)"
            :error="erros.mascara"
            @keypress="restringirMascara"
          />
          <SelectField v-model="form.configuracaoCodigoNaturezaFinanceiraPagamentoId" label="Configuração: Pagamento" :options="opcoesPagamento" />
          <SelectField v-model="form.configuracaoCodigoNaturezaFinanceiraRecebimentoId" label="Configuração: Recebimento" :options="opcoesRecebimento" />
        </div>
      </form>
    </div>

    <div class="glass-panel form-panel">
      <h3 class="section-title">Itens do plano de contas</h3>
      <p v-if="!planoId" class="section-hint">Salve o plano de contas para poder adicionar itens.</p>

      <form v-else class="item-form" @submit.prevent="salvarItem">
        <TextField v-model="itemForm.codigo" label="Código" maxlength="20" />
        <TextField v-model="itemForm.descricao" label="Descrição" maxlength="150" />
        <SelectField v-model="itemForm.tipoDetalhamento" label="Detalhamento" :options="TIPO_DETALHAMENTO_OPCOES" />
        <label class="field toggle-row">
          <span class="field-label">Movimenta Caixa</span>
          <input v-model="itemForm.movimentaCaixa" type="checkbox" />
        </label>
        <button type="button" class="btn btn-primary" :disabled="salvandoItem" @click="salvarItem">
          {{ itemForm.id ? 'Atualizar item' : '+ Adicionar item' }}
        </button>
        <button v-if="itemForm.id" type="button" class="btn btn-secondary" @click="limparItemForm">Cancelar edição</button>
      </form>

      <DataTable
        :items="itensPlano"
        :columns="colunasItens"
        :total="itensPlano.length"
        :page="1"
        :page-size="500"
        :loading="carregandoItens"
        empty-text="Nenhum item cadastrado neste plano de contas."
      >
        <template #cell-tipoDetalhamento="{ value }">
          {{ descricaoDetalhamento(Number(value)) }}
        </template>
        <template #cell-movimentaCaixa="{ value }">
          {{ value ? 'Sim' : 'Não' }}
        </template>
        <template #actions="{ row }">
          <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarItem(row)">Editar</button>
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusaoItem(row)">Excluir</button>
        </template>
      </DataTable>
    </div>

    <DeleteAlert
      v-model="excluirItemVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindoItem"
      @confirm="confirmarExclusaoItem"
    />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.section-title { font-size: 15px; font-weight: 600; margin: 0 0 12px; color: var(--text-primary); }
.section-hint { color: var(--text-secondary); font-size: 13px; }
.item-form {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 12px;
  align-items: end;
  margin-bottom: 16px;
}
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
