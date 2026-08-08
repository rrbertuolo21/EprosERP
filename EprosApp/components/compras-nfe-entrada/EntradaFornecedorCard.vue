<script setup lang="ts">
/**
 * EntradaFornecedorCard — seleção/edição do fornecedor da NF-e de entrada.
 *
 * Porta o `NfeDestinatarioCard` do legado, mas rotulado como "Fornecedor" (config context 'compras').
 * Oferece busca de parceiro (endpoint `cadastros/pessoas`) e também permite preencher CNPJ/nome
 * manualmente, já que o backend `compras/lancar` recebe o fornecedor por CNPJ + nome.
 *
 * IO de busca pelo cliente compartilhado `useApi`. Emite `solicitar-troca` quando há itens na nota
 * e o usuário tenta trocar o fornecedor (a página confirma antes de limpar os itens).
 */
import { ref } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { useMask } from '~/composables/useMask'
import TextField from '~/components/shared/fields/TextField.vue'
import type { EntradaForm } from './tipos'

const props = defineProps<{
  modelValue: EntradaForm
  /** Bloqueia a troca (há itens na nota). A UI mostra aviso e emite solicitar-troca. */
  bloqueado?: boolean
  quantidadeItens?: number
  readonly?: boolean
  erro?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: EntradaForm]
  'solicitar-troca': []
  'novo-fornecedor': []
}>()

const { maskCpfCnpj } = useMask()

interface PessoaResumo {
  id: number
  nome?: string
  razaoSocial?: string
  nomeFantasia?: string
  documento?: string
  cnpj?: string
  cpf?: string
  inscricaoEstadual?: string
  enderecoFormatado?: string
}

const termo = ref('')
const buscando = ref(false)
const resultados = ref<PessoaResumo[]>([])
const mostrarResultados = ref(false)

async function buscar() {
  const q = termo.value.trim()
  if (q.length < 2) {
    resultados.value = []
    return
  }
  buscando.value = true
  try {
    const resp = await useApi('/cadastros/pessoas', {
      query: { localizar: q, pagina: 1, tamanhoPagina: 20 }
    })
    resultados.value = extrairLista<PessoaResumo>(resp) ?? []
    mostrarResultados.value = true
  } catch (e) {
    console.error('[EntradaFornecedorCard] falha ao buscar fornecedor', e)
    resultados.value = []
  } finally {
    buscando.value = false
  }
}

function selecionar(p: PessoaResumo) {
  props.modelValue.fornecedor = {
    pessoaId: p.id,
    nome: p.razaoSocial ?? p.nome ?? p.nomeFantasia ?? '',
    documento: p.documento ?? p.cnpj ?? p.cpf ?? '',
    inscricaoEstadual: p.inscricaoEstadual ?? '',
    enderecoFormatado: p.enderecoFormatado ?? ''
  }
  emit('update:modelValue', props.modelValue)
  mostrarResultados.value = false
  termo.value = ''
  resultados.value = []
}

function limpar() {
  if (props.bloqueado) {
    emit('solicitar-troca')
    return
  }
  props.modelValue.fornecedor = {
    pessoaId: null,
    nome: '',
    documento: '',
    inscricaoEstadual: '',
    enderecoFormatado: ''
  }
  emit('update:modelValue', props.modelValue)
}

function editarManual<K extends keyof EntradaForm['fornecedor']>(chave: K, valor: EntradaForm['fornecedor'][K]) {
  props.modelValue.fornecedor[chave] = valor
  emit('update:modelValue', props.modelValue)
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Fornecedor</h2>
      <div class="header-actions">
        <button
          v-if="!readonly"
          type="button"
          class="btn btn-ghost btn-sm"
          @click="emit('novo-fornecedor')"
        >
          + Novo fornecedor
        </button>
      </div>
    </header>

    <span v-if="erro" class="field-error erro-topo">{{ erro }}</span>

    <!-- Fornecedor selecionado -->
    <div v-if="modelValue.fornecedor.pessoaId || modelValue.fornecedor.nome" class="forn-selecionado">
      <div class="forn-info">
        <strong class="forn-nome">{{ modelValue.fornecedor.nome || 'Fornecedor sem nome' }}</strong>
        <span v-if="modelValue.fornecedor.documento" class="forn-doc">
          {{ maskCpfCnpj(modelValue.fornecedor.documento) }}
        </span>
        <span v-if="modelValue.fornecedor.enderecoFormatado" class="forn-end">
          {{ modelValue.fornecedor.enderecoFormatado }}
        </span>
      </div>
      <button v-if="!readonly" type="button" class="btn btn-ghost btn-sm" @click="limpar">Trocar</button>
    </div>

    <template v-else-if="!readonly">
      <!-- Busca de parceiro -->
      <div class="forn-busca">
        <div class="busca-input">
          <TextField
            v-model="termo"
            label="Buscar fornecedor cadastrado"
            placeholder="Nome, razão social ou CNPJ..."
            @update:model-value="mostrarResultados = false"
          />
          <button type="button" class="btn btn-secondary btn-sm" :disabled="buscando" @click="buscar">
            <span v-if="buscando" class="spinner"></span>
            <span v-else>Buscar</span>
          </button>
        </div>

        <ul v-if="mostrarResultados && resultados.length" class="busca-resultados">
          <li v-for="p in resultados" :key="p.id" @click="selecionar(p)">
            <span class="res-nome">{{ p.razaoSocial ?? p.nome ?? p.nomeFantasia }}</span>
            <span class="res-doc">{{ maskCpfCnpj(p.documento ?? p.cnpj ?? p.cpf ?? '') }}</span>
          </li>
        </ul>
        <p v-else-if="mostrarResultados && !resultados.length" class="busca-vazio">
          Nenhum fornecedor encontrado.
        </p>
      </div>

      <!-- Entrada manual (fornecedor não cadastrado) -->
      <div class="forn-manual">
        <p class="forn-manual-label">Ou informe o fornecedor manualmente:</p>
        <div class="form-grid">
          <TextField
            :model-value="modelValue.fornecedor.documento"
            label="CNPJ do fornecedor"
            placeholder="CNPJ"
            @update:model-value="editarManual('documento', $event)"
          />
          <TextField
            class="col-span-2"
            :model-value="modelValue.fornecedor.nome"
            label="Nome / Razão social"
            placeholder="Nome do fornecedor"
            @update:model-value="editarManual('nome', $event)"
          />
          <TextField
            :model-value="modelValue.fornecedor.inscricaoEstadual"
            label="Inscrição estadual"
            placeholder="IE"
            @update:model-value="editarManual('inscricaoEstadual', $event)"
          />
        </div>
      </div>
    </template>

    <p v-else class="forn-vazio">Nenhum fornecedor informado.</p>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.erro-topo { display: block; margin-bottom: 8px; }
.forn-selecionado { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px 14px; background: rgba(255,255,255,0.03); border-radius: 8px; }
.forn-info { display: flex; flex-direction: column; gap: 2px; }
.forn-nome { font-size: 14px; }
.forn-doc { font-size: 12.5px; color: var(--text-secondary); font-family: monospace; }
.forn-end { font-size: 12px; color: var(--text-muted); }
.forn-busca { position: relative; margin-bottom: 16px; }
.busca-input { display: flex; align-items: flex-end; gap: 10px; }
.busca-input :deep(.field) { flex: 1; }
.busca-resultados { list-style: none; margin-top: 6px; border: 1px solid var(--border-color); border-radius: 8px; overflow: hidden; max-height: 260px; overflow-y: auto; }
.busca-resultados li { display: flex; justify-content: space-between; gap: 12px; padding: 9px 12px; cursor: pointer; font-size: 13px; }
.busca-resultados li:hover { background: rgba(255,255,255,0.05); }
.res-doc { color: var(--text-muted); font-family: monospace; font-size: 12px; }
.busca-vazio { font-size: 12.5px; color: var(--text-muted); margin-top: 6px; }
.forn-manual-label { font-size: 12px; color: var(--text-muted); margin-bottom: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px 14px; }
.col-span-2 { grid-column: span 2; }
.forn-vazio { font-size: 13px; color: var(--text-muted); }
@media (max-width: 900px) {
  .form-grid { grid-template-columns: repeat(2, 1fr); }
  .col-span-2 { grid-column: span 2; }
}
</style>
