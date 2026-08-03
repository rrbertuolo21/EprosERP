<script setup lang="ts">
/**
 * Formulário de Natureza Financeira (novo/edição) — Financeiro / Cadastros auxiliares.
 *
 * Porta o comportamento de `financeiro/natureza-financeira/[id].vue` do legado:
 * descrição, tipo de configuração (Recebimento/Pagamento) e o mapeamento de cada
 * forma de pagamento/recebimento para um item do plano de contas financeiro ativo
 * da empresa, filtrado por tipo de detalhamento (Crédito/Débito) compatível com o
 * tipo de configuração da natureza (Recebimento => Crédito, Pagamento => Débito),
 * exceto o campo "Troco" que usa o detalhamento oposto — igual ao legado.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useTenant } from '~/composables/useTenant'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const TIPO_NATUREZA_OPCOES: SelectOption[] = [
  { label: 'Recebimento', value: 1 },
  { label: 'Pagamento', value: 2 }
]

// ETipoDetalhamento: Crédito = 0, Débito = 1
const TIPO_DETALHAMENTO_POR_NATUREZA: Record<number, number> = { 1: 0, 2: 1 }

interface PlanoItem {
  id: string
  codigo: string
  descricao: string
  tipoDetalhamento: number
}

interface Plano {
  id: string
  descricao: string
  itens: PlanoItem[]
}

/** Campos de mapeamento forma-de-pagamento -> item do plano de contas (iguais ao legado). */
const CAMPOS_MAPEAMENTO: Array<{ key: keyof NaturezaForm; label: string; trocoInvertido?: boolean }> = [
  { key: 'itemPlanoDeContasFinanceiroDinheiroId', label: 'Dinheiro' },
  { key: 'itemPlanoDeContasFinanceiroCartaoChequeId', label: 'Cartão Cheque' },
  { key: 'itemPlanoDeContasFinanceiroCartaoCreditoId', label: 'Cartão Crédito' },
  { key: 'itemPlanoDeContasFinanceiroCartaoDebitoId', label: 'Cartão Débito' },
  { key: 'itemPlanoDeContasFinanceiroCartaoDaLojaId', label: 'Cartão da Loja' },
  { key: 'itemPlanoDeContasFinanceiroValeAlimentacaoId', label: 'Vale Alimentação' },
  { key: 'itemPlanoDeContasFinanceiroValeRefeicaoId', label: 'Vale Refeição' },
  { key: 'itemPlanoDeContasFinanceiroValePresenteId', label: 'Vale Presente' },
  { key: 'itemPlanoDeContasFinanceiroValeCombustivelId', label: 'Vale Combustível' },
  { key: 'itemPlanoDeContasFinanceiroDuplicataMercantilId', label: 'Duplicata Mercantil' },
  { key: 'itemPlanoDeContasFinanceiroBoletoBancarioId', label: 'Boleto Bancário' },
  { key: 'itemPlanoDeContasFinanceiroDepositoBancarioId', label: 'Depósito Bancário' },
  { key: 'itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId', label: 'PIX Dinâmico' },
  { key: 'itemPlanoDeContasFinanceiroTransferenciaBancariaId', label: 'Transferência Bancária' },
  { key: 'itemPlanoDeContasFinanceiroProgramaDeFidelidadeId', label: 'Programa de Fidelidade' },
  { key: 'itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId', label: 'PIX Estático' },
  { key: 'itemPlanoDeContasFinanceiroCreditoEmLojaId', label: 'Crédito em Loja' },
  { key: 'itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId', label: 'Pagamento Eletrônico Não Informado' },
  { key: 'itemPlanoDeContasFinanceiroOutrosId', label: 'Outros Pagamentos' },
  { key: 'itemPlanoDeContasFinanceiroDescontoId', label: 'Desconto' },
  { key: 'itemPlanoDeContasFinanceiroAcrescimoId', label: 'Acréscimo' },
  { key: 'itemPlanoDeContasFinanceiroJurosId', label: 'Juros' },
  { key: 'itemPlanoDeContasFinanceiroMultaId', label: 'Multa' },
  { key: 'itemPlanoDeContasFinanceiroTrocoId', label: 'Troco', trocoInvertido: true }
]

interface NaturezaForm {
  id: string | null
  empresaId: string
  descricao: string
  tipoConfiguracaoNatureza: number | null
  itemPlanoDeContasFinanceiroDinheiroId: string | null
  itemPlanoDeContasFinanceiroCartaoChequeId: string | null
  itemPlanoDeContasFinanceiroCartaoCreditoId: string | null
  itemPlanoDeContasFinanceiroCartaoDebitoId: string | null
  itemPlanoDeContasFinanceiroCartaoDaLojaId: string | null
  itemPlanoDeContasFinanceiroValeAlimentacaoId: string | null
  itemPlanoDeContasFinanceiroValeRefeicaoId: string | null
  itemPlanoDeContasFinanceiroValePresenteId: string | null
  itemPlanoDeContasFinanceiroValeCombustivelId: string | null
  itemPlanoDeContasFinanceiroDuplicataMercantilId: string | null
  itemPlanoDeContasFinanceiroBoletoBancarioId: string | null
  itemPlanoDeContasFinanceiroDepositoBancarioId: string | null
  itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId: string | null
  itemPlanoDeContasFinanceiroTransferenciaBancariaId: string | null
  itemPlanoDeContasFinanceiroProgramaDeFidelidadeId: string | null
  itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId: string | null
  itemPlanoDeContasFinanceiroCreditoEmLojaId: string | null
  itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId: string | null
  itemPlanoDeContasFinanceiroOutrosId: string | null
  itemPlanoDeContasFinanceiroDescontoId: string | null
  itemPlanoDeContasFinanceiroAcrescimoId: string | null
  itemPlanoDeContasFinanceiroJurosId: string | null
  itemPlanoDeContasFinanceiroMultaId: string | null
  itemPlanoDeContasFinanceiroTrocoId: string | null
}

function formVazio(empresaId: string): NaturezaForm {
  return {
    id: null,
    empresaId,
    descricao: '',
    tipoConfiguracaoNatureza: null,
    itemPlanoDeContasFinanceiroDinheiroId: null,
    itemPlanoDeContasFinanceiroCartaoChequeId: null,
    itemPlanoDeContasFinanceiroCartaoCreditoId: null,
    itemPlanoDeContasFinanceiroCartaoDebitoId: null,
    itemPlanoDeContasFinanceiroCartaoDaLojaId: null,
    itemPlanoDeContasFinanceiroValeAlimentacaoId: null,
    itemPlanoDeContasFinanceiroValeRefeicaoId: null,
    itemPlanoDeContasFinanceiroValePresenteId: null,
    itemPlanoDeContasFinanceiroValeCombustivelId: null,
    itemPlanoDeContasFinanceiroDuplicataMercantilId: null,
    itemPlanoDeContasFinanceiroBoletoBancarioId: null,
    itemPlanoDeContasFinanceiroDepositoBancarioId: null,
    itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId: null,
    itemPlanoDeContasFinanceiroTransferenciaBancariaId: null,
    itemPlanoDeContasFinanceiroProgramaDeFidelidadeId: null,
    itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId: null,
    itemPlanoDeContasFinanceiroCreditoEmLojaId: null,
    itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId: null,
    itemPlanoDeContasFinanceiroOutrosId: null,
    itemPlanoDeContasFinanceiroDescontoId: null,
    itemPlanoDeContasFinanceiroAcrescimoId: null,
    itemPlanoDeContasFinanceiroJurosId: null,
    itemPlanoDeContasFinanceiroMultaId: null,
    itemPlanoDeContasFinanceiroTrocoId: null
  }
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { empresaId } = useTenant()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const planos = ref<Plano[]>([])

const form = reactive<NaturezaForm>(formVazio(String(empresaId.value || '')))
const erros = reactive<Record<string, string>>({})

/** Itens do plano de contas ativo compatíveis com o tipo de configuração selecionado. */
const opcoesItens = computed<SelectOption[]>(() => {
  if (!form.tipoConfiguracaoNatureza) return []
  const tipoDetalhamento = TIPO_DETALHAMENTO_POR_NATUREZA[form.tipoConfiguracaoNatureza]
  const todosItens = planos.value.flatMap((p) => p.itens)
  return todosItens
    .filter((i) => i.tipoDetalhamento === tipoDetalhamento)
    .map((i) => ({ label: `${i.codigo} - ${i.descricao}`, value: i.id }))
})

/** Itens para o campo "Troco" — usa o detalhamento OPOSTO ao da natureza, igual ao legado. */
const opcoesItensTroco = computed<SelectOption[]>(() => {
  if (!form.tipoConfiguracaoNatureza) return []
  const tipoDetalhamento = TIPO_DETALHAMENTO_POR_NATUREZA[form.tipoConfiguracaoNatureza]
  const todosItens = planos.value.flatMap((p) => p.itens)
  return todosItens
    .filter((i) => i.tipoDetalhamento !== tipoDetalhamento)
    .map((i) => ({ label: `${i.codigo} - ${i.descricao}`, value: i.id }))
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (!form.tipoConfiguracaoNatureza) erros.tipoConfiguracaoNatureza = 'Tipo é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregarPlanos() {
  try {
    const resposta = await useApi<{ dados?: { itens: Array<{ id: string; descricao: string; mascara: string }> } }>(
      '/plano-de-contas-financeiro',
      { query: { tamanhoPagina: 100 } }
    )
    const dados = extrairDados<{ itens: Array<{ id: string; descricao: string }> }>(resposta)
    const listaPlanos = dados?.itens ?? []

    const planosComItens: Plano[] = []
    for (const p of listaPlanos) {
      const respostaItens = await useApi<{ dados?: { itens: PlanoItem[] } }>('/plano-de-contas-financeiro-itens', {
        query: { planoDeContasId: p.id, tamanhoPagina: 500 }
      })
      const dadosItens = extrairDados<{ itens: PlanoItem[] }>(respostaItens)
      planosComItens.push({ id: p.id, descricao: p.descricao, itens: dadosItens?.itens ?? [] })
    }
    planos.value = planosComItens
  } catch (e) {
    console.error('[natureza-financeira/[id]] planos', e)
    planos.value = []
  }
}

async function carregarNatureza() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/configuracao-codigo-naturezas-financeiras/{id}`, { params: { id: idParam } })
    const dados = extrairDados<Partial<NaturezaForm>>(resposta)
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
    const { id, ...payload } = form
    if (isEdit.value) {
      await useApi('/configuracao-codigo-naturezas-financeiras/{id}', {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, ...payload }
      })
    } else {
      await useApi('/configuracao-codigo-naturezas-financeiras', { method: 'POST', body: payload })
    }
    toast.success('Natureza financeira salva com sucesso!')
    router.push('/erp/financeiro/natureza-financeira')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/financeiro/natureza-financeira')
}

onMounted(async () => {
  await carregarPlanos()
  await carregarNatureza()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar natureza financeira' : 'Nova natureza financeira'" :loading="carregando">
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
          <SelectField
            v-model="form.tipoConfiguracaoNatureza"
            label="Tipo Configuração Natureza"
            required
            :options="TIPO_NATUREZA_OPCOES"
            :error="erros.tipoConfiguracaoNatureza"
          />
        </div>

        <hr class="divider" />
        <p class="section-hint">
          Mapeie cada forma de pagamento/recebimento para um item do plano de contas financeiro
          {{ !form.tipoConfiguracaoNatureza ? '(selecione o tipo acima primeiro).' : '.' }}
        </p>

        <div class="form-grid">
          <SelectField
            v-for="campo in CAMPOS_MAPEAMENTO"
            :key="campo.key"
            v-model="(form[campo.key] as string | null)"
            :label="campo.label"
            :options="campo.trocoInvertido ? opcoesItensTroco : opcoesItens"
            :disabled="!form.tipoConfiguracaoNatureza"
          />
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.divider { border: none; border-top: 1px solid var(--border-color); margin: 20px 0 12px; }
.section-hint { color: var(--text-secondary); font-size: 13px; margin: 0 0 16px; }
</style>
