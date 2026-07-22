<script setup lang="ts">
/**
 * BaixaPagamentoDialog — baixa de pagamento (recebimento) de uma conta a receber.
 *
 * Porta o comportamento de `components/financeiro/BaixaPagamentoDialog.vue` do legado:
 * lista as formas de recebimento já lançadas, permite incluir/editar/excluir e
 * processa o pagamento (PUT) atualizando a situação do título.
 *
 * Contrato:
 *   props: modelValue (v-model), itemId?: number (carrega via API), item?: ContasAReceber (já carregado)
 *   emits: 'update:modelValue', saved: [] (após processar com sucesso)
 */
import { ref, computed, watch } from 'vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import BaixaPagamentoFormaDialog from './BaixaPagamentoFormaDialog.vue'
import { situacaoInfo, type ContasAReceber, type ContasAReceberItem } from './types'

const props = defineProps<{
  modelValue: boolean
  itemId?: number
  item?: ContasAReceber
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  saved: []
}>()

const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()
const { carregarOpcoes } = useEnum()

const carregando = ref(false)
const processando = ref(false)
const data = ref<ContasAReceber | null>(null)
const opcoesTipoPagamento = ref<SelectOption[]>([])

const showFormaDialog = ref(false)
const formaItem = ref<ContasAReceberItem | undefined>()
const formaIndex = ref<number | null>(null)

async function carregarTipoPagamento() {
  if (opcoesTipoPagamento.value.length) return
  opcoesTipoPagamento.value = await carregarOpcoes('financeiros-enums/tipo-pagamento')
}

async function carregarConta() {
  if (props.item) {
    data.value = { ...props.item }
    return
  }
  if (!props.itemId) {
    data.value = null
    return
  }
  carregando.value = true
  try {
    const resposta = await useApi(`/financeiro/contas-receber/${props.itemId}`)
    data.value = extrairDados<ContasAReceber>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
    data.value = null
  } finally {
    carregando.value = false
  }
}

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) {
      void carregarTipoPagamento()
      void carregarConta()
    }
  }
)

const valorTituloLiquido = computed(() => {
  if (!data.value) return 0
  return (
    data.value.valorTitulo -
    (data.value.valorTotalDesconto ?? 0) +
    (data.value.valorTotalMulta ?? 0) +
    (data.value.valorTotalJuros ?? 0) +
    (data.value.valorTotalAcrescimo ?? 0)
  )
})

const valorTotalRecebido = computed(() => {
  return data.value?.contasAReceberItens?.reduce((acc, i) => acc + (i.valorPago ?? 0), 0) ?? 0
})

const diferenca = computed(() => valorTituloLiquido.value - valorTotalRecebido.value)

const situacao = computed(() => situacaoInfo(data.value?.situacao))

function abrirFormaDialog(item?: ContasAReceberItem, index?: number) {
  const valorAReceber = data.value
    ? data.value.valorTitulo -
      (data.value.valorInicialDesconto ?? 0) +
      (data.value.valorInicialJuros ?? 0) +
      (data.value.valorInicialMulta ?? 0) +
      (data.value.valorInicialAcrescimo ?? 0)
    : 0

  formaItem.value = item
    ? { ...item }
    : {
        dataRecebimento: new Date().toISOString().slice(0, 10),
        valorDesconto: 0,
        valorJuros: 0,
        valorMulta: 0,
        valorAcrescimo: 0,
        valorParcela: valorAReceber,
        valorPago: valorAReceber
      }
  formaIndex.value = index ?? null
  showFormaDialog.value = true
}

function salvarForma(item: ContasAReceberItem) {
  if (!data.value) return
  if (!data.value.contasAReceberItens) data.value.contasAReceberItens = []
  if (formaIndex.value !== null) {
    data.value.contasAReceberItens[formaIndex.value] = item
  } else {
    data.value.contasAReceberItens.push(item)
  }
}

function excluirForma(index: number) {
  if (!data.value) return
  data.value.contasAReceberItens.splice(index, 1)
}

async function processarPagamento() {
  if (!data.value) return
  processando.value = true
  try {
    await useApi(`/financeiro/contas-receber/${data.value.id}`, { method: 'PUT', body: data.value })
    toast.success('Pagamento processado com sucesso!')
    emit('saved')
    emit('update:modelValue', false)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

function fechar() {
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog :model-value="modelValue" title="Baixa de Pagamento" width="900px" @update:model-value="emit('update:modelValue', $event)">
    <div v-if="carregando" class="baixa-loading"><span class="spinner"></span> Carregando...</div>
    <template v-else-if="data">
      <p class="baixa-subtitulo">Conta: {{ data.detalhamento }} - {{ data.documento }}</p>

      <div class="baixa-resumo">
        <div class="resumo-item">
          <span class="resumo-label">Valor Original</span>
          <span class="resumo-valor">{{ formatarMoeda(data.valorTitulo) }}</span>
        </div>
        <div class="resumo-item">
          <span class="resumo-label">Desconto</span>
          <span class="resumo-valor cor-sucesso">{{ formatarMoeda(data.valorTotalDesconto) }}</span>
        </div>
        <div class="resumo-item">
          <span class="resumo-label">Juros/Multa</span>
          <span class="resumo-valor cor-erro">
            {{ formatarMoeda((data.valorTotalMulta ?? 0) + (data.valorTotalJuros ?? 0) + (data.valorTotalAcrescimo ?? 0)) }}
          </span>
        </div>
        <div class="resumo-item">
          <span class="resumo-label">Total a Receber</span>
          <span class="resumo-valor">{{ formatarMoeda(valorTituloLiquido) }}</span>
        </div>
      </div>

      <div class="baixa-formas">
        <div class="formas-header">
          <span class="formas-titulo">Formas de Pagamento</span>
          <button type="button" class="btn btn-primary btn-sm" @click="abrirFormaDialog()">+ Nova Forma de Pagamento</button>
        </div>

        <div v-if="!data.contasAReceberItens?.length" class="formas-vazio">
          Nenhum pagamento registrado ainda. Clique em "Nova Forma de Pagamento" para adicionar.
        </div>

        <div v-else class="formas-lista">
          <div v-for="(item, index) in data.contasAReceberItens" :key="`item-${index}`" class="forma-card">
            <div class="forma-linha">
              <div class="forma-info">
                <div class="forma-tipo">{{ item.formaPagamento ?? 'Forma de recebimento' }}</div>
                <div class="forma-data">
                  {{ formatarData(item.dataRecebimento) }}
                  <span v-if="item.contaBancaria"> • {{ item.contaBancaria }}</span>
                </div>
              </div>
              <div class="forma-valores">
                <div class="forma-valor-pago">{{ formatarMoeda(item.valorPago) }}</div>
                <div v-if="item.valorDesconto > 0" class="forma-detalhe cor-sucesso">Desconto: {{ formatarMoeda(item.valorDesconto) }}</div>
                <div v-if="item.valorMulta + item.valorJuros > 0" class="forma-detalhe cor-erro">
                  Juros/Multa: {{ formatarMoeda(item.valorMulta + item.valorJuros) }}
                </div>
              </div>
            </div>
            <div class="forma-rodape">
              <span>Valor Líquido: {{ formatarMoeda(item.valorParcela) }}</span>
              <div class="forma-acoes">
                <button type="button" class="link-action" @click="abrirFormaDialog(item, index)">Editar</button>
                <button type="button" class="link-action link-danger" @click="excluirForma(index)">Excluir</button>
              </div>
            </div>
          </div>
        </div>

        <div class="baixa-totais">
          <div class="resumo-item">
            <span class="resumo-label">Total a Receber</span>
            <span class="resumo-valor">{{ formatarMoeda(valorTituloLiquido) }}</span>
          </div>
          <div class="resumo-item">
            <span class="resumo-label">Total Recebido</span>
            <span class="resumo-valor cor-sucesso">{{ formatarMoeda(valorTotalRecebido) }}</span>
          </div>
          <div class="resumo-item">
            <span class="resumo-label">Diferença</span>
            <span class="resumo-valor" :class="diferenca > 0 ? 'cor-erro' : 'cor-muted'">{{ formatarMoeda(diferenca) }}</span>
          </div>
          <div class="resumo-item">
            <span class="resumo-label">Status</span>
            <span class="badge" :class="`badge-${situacao.cor === 'error' ? 'danger' : situacao.cor}`">{{ situacao.texto }}</span>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Cancelar</button>
      <button v-if="data" type="button" class="btn btn-primary" :disabled="processando" @click="processarPagamento">
        <span v-if="processando" class="spinner"></span>
        <span v-else>Processar Pagamento</span>
      </button>
    </template>
  </AppDialog>

  <BaixaPagamentoFormaDialog
    v-model="showFormaDialog"
    :item="formaItem"
    :opcoes-tipo-pagamento="opcoesTipoPagamento"
    @save="salvarForma"
  />
</template>

<style scoped>
.baixa-loading { display: flex; align-items: center; gap: 8px; color: var(--text-secondary); padding: 20px 0; }
.baixa-subtitulo { color: var(--text-secondary); font-size: 13px; margin-bottom: 12px; }
.baixa-resumo, .baixa-totais {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
  padding: 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  margin-bottom: 16px;
}
.resumo-item { display: flex; flex-direction: column; gap: 4px; }
.resumo-label { font-size: 12px; color: var(--text-muted); }
.resumo-valor { font-size: 16px; font-weight: 700; }
.cor-sucesso { color: var(--success, #22c55e); }
.cor-erro { color: var(--danger, #ef4444); }
.cor-muted { color: var(--text-muted); }
.baixa-formas { display: flex; flex-direction: column; gap: 10px; }
.formas-header { display: flex; justify-content: space-between; align-items: center; }
.formas-titulo { font-weight: 700; font-size: 15px; }
.formas-vazio {
  text-align: center;
  padding: 24px;
  color: var(--text-muted);
  border: 1px dashed var(--border-color);
  border-radius: 8px;
}
.formas-lista { display: flex; flex-direction: column; gap: 10px; }
.forma-card { border: 1px solid var(--border-color); border-radius: 8px; padding: 12px 14px; background: rgba(255, 255, 255, 0.02); }
.forma-linha { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; }
.forma-tipo { font-weight: 700; font-size: 14px; }
.forma-data { font-size: 12px; color: var(--text-muted); margin-top: 2px; }
.forma-valores { display: flex; flex-direction: column; align-items: flex-end; gap: 2px; }
.forma-valor-pago { font-weight: 700; color: var(--success, #22c55e); }
.forma-detalhe { font-size: 11px; }
.forma-rodape {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid var(--border-color);
  font-size: 13px;
  color: var(--text-secondary);
}
.forma-acoes { display: flex; gap: 12px; }
.link-action { background: none; border: none; cursor: pointer; color: var(--primary); font-size: 13px; padding: 0; }
.link-danger { color: var(--danger, #ef4444); }
</style>
