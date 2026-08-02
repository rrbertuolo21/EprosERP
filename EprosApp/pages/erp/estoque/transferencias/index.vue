<script setup lang="ts">
/**
 * Transferência entre Locais (erp/estoque/transferencias).
 * `POST /estoque/transferencias` (CriarTransferenciaEstoqueCommand): cabeçalho (origem, destino,
 * data, frete) + N itens (produto, quantidade, valor unitário, lote, validade). Valida origem ≠
 * destino (MVM-020) e saldo (MVM-021) no backend.
 *
 * Observação: a API expõe a criação (POST); não há GET de listagem de transferências neste
 * controller — o efeito aparece no Saldo de Estoque e no Movimento Manual. Tela de lançamento.
 */
import { computed, reactive, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const toast = useToast()
const salvando = ref(false)

interface TransferenciaItem {
  produtoId: string
  quantidade: number | null
  valorUnitario: number | null
  lote: string
  dataValidade: string | null
}

const form = reactive({
  empresaId: '',
  localOrigemId: '',
  localDestinoId: '',
  dataTransferencia: new Date().toISOString().slice(0, 10),
  valorFrete: null as number | null,
  observacao: '',
  itens: [{ produtoId: '', quantidade: null, valorUnitario: null, lote: '', dataValidade: null }] as TransferenciaItem[]
})

const erros = reactive<Record<string, string>>({})

function addLinha() {
  form.itens.push({ produtoId: '', quantidade: null, valorUnitario: null, lote: '', dataValidade: null })
}
function removerLinha(i: number) {
  form.itens.splice(i, 1)
  if (form.itens.length === 0) addLinha()
}

const totalItens = computed(() => form.itens.filter((i) => i.produtoId.trim() && (i.quantidade ?? 0) > 0).length)

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.empresaId.trim()) erros.empresaId = 'Empresa é obrigatória.'
  if (!form.localOrigemId.trim()) erros.localOrigemId = 'Local de origem é obrigatório.'
  if (!form.localDestinoId.trim()) erros.localDestinoId = 'Local de destino é obrigatório.'
  if (form.localOrigemId.trim() && form.localOrigemId.trim() === form.localDestinoId.trim())
    erros.localDestinoId = 'Origem e destino devem ser diferentes.'
  if (totalItens.value === 0) erros.itens = 'Informe ao menos um item com produto e quantidade.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/estoque/transferencias', {
      method: 'POST',
      body: {
        empresaId: form.empresaId.trim(),
        localOrigemId: form.localOrigemId.trim(),
        localDestinoId: form.localDestinoId.trim(),
        dataTransferencia: form.dataTransferencia,
        valorFrete: form.valorFrete,
        observacao: form.observacao.trim() || null,
        itens: form.itens
          .filter((i) => i.produtoId.trim() && (i.quantidade ?? 0) > 0)
          .map((i) => ({
            produtoId: i.produtoId.trim(),
            quantidade: Number(i.quantidade ?? 0),
            valorUnitario: i.valorUnitario != null ? Number(i.valorUnitario) : null,
            lote: i.lote.trim() || null,
            dataValidade: i.dataValidade || null
          }))
      }
    })
    toast.success('Transferência criada com sucesso!')
    form.observacao = ''
    form.valorFrete = null
    form.itens = [{ produtoId: '', quantidade: null, valorUnitario: null, lote: '', dataValidade: null }]
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}
</script>

<template>
  <div>
    <PageToolbar title="Transferência entre Locais" subtitle="Movimenta saldo de um local para outro" :loading="salvando" />

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.empresaId" label="Empresa (ID)" required :error="erros.empresaId" />
        <TextField v-model="form.localOrigemId" label="Local de origem (ID)" required :error="erros.localOrigemId" />
        <TextField v-model="form.localDestinoId" label="Local de destino (ID)" required :error="erros.localDestinoId" />
        <DateTimeField v-model="form.dataTransferencia" label="Data da transferência" required />
        <MoneyInput v-model="form.valorFrete" label="Valor do frete" />
      </div>
      <TextField v-model="form.observacao" label="Observação" />

      <h3 class="secao-titulo">Itens transferidos</h3>
      <p v-if="erros.itens" class="erro-inline">{{ erros.itens }}</p>
      <div class="tabela-itens">
        <div class="linha-item cabecalho">
          <span>Produto (ID)</span><span>Quantidade</span><span>Valor unitário</span><span>Lote</span><span>Validade</span><span></span>
        </div>
        <div v-for="(item, i) in form.itens" :key="i" class="linha-item">
          <TextField v-model="item.produtoId" placeholder="GUID do produto" />
          <QuantityInput v-model="item.quantidade" :decimais="4" />
          <MoneyInput v-model="item.valorUnitario" />
          <TextField v-model="item.lote" placeholder="Opcional" />
          <DateTimeField v-model="item.dataValidade" />
          <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Remover" @click="removerLinha(i)">✕</button>
        </div>
      </div>
      <div class="itens-rodape">
        <button type="button" class="btn btn-secondary btn-sm" @click="addLinha">+ Adicionar item</button>
        <span class="total">{{ totalItens }} item(ns) válido(s)</span>
      </div>

      <div class="acoes-form">
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar transferência</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; margin-bottom: 16px; }
.secao-titulo { font-size: 14px; margin: 18px 0 10px; }
.tabela-itens { display: flex; flex-direction: column; gap: 8px; }
.linha-item { display: grid; grid-template-columns: 2fr 1fr 1.2fr 1fr 1.3fr 40px; gap: 10px; align-items: end; }
.linha-item.cabecalho { font-size: 12px; color: var(--text-muted); align-items: center; }
.itens-rodape { display: flex; justify-content: space-between; align-items: center; margin-top: 12px; }
.total { font-size: 13px; color: var(--text-secondary); }
.acoes-form { display: flex; justify-content: flex-end; margin-top: 20px; }
.erro-inline { color: var(--danger, #dc3545); font-size: 13px; margin: 4px 0; }
</style>
