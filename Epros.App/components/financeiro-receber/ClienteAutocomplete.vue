<script setup lang="ts">
/**
 * ClienteAutocomplete — busca e seleção de cliente (pessoa) para o título a receber.
 *
 * Porta o comportamento do `VAutocomplete` de cliente do legado (`useContasAReceberFetchId`):
 * busca por termo em `cadastros/pessoas` com debounce simples e permite selecionar.
 *
 * Contrato:
 *   props: modelValue: number | null (v-model — pessoaId), nomeSelecionado?: string
 *   emits: 'update:modelValue', 'update:nomeSelecionado'
 */
import { ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import TextField from '~/components/shared/fields/TextField.vue'
import type { PessoaBusca } from './types'

const props = defineProps<{
  modelValue: number | null
  nomeSelecionado?: string | null
  error?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: number | null]
  'update:nomeSelecionado': [value: string]
}>()

const termo = ref('')
const resultados = ref<PessoaBusca[]>([])
const buscando = ref(false)
let debounceTimer: ReturnType<typeof setTimeout> | undefined

function nomePessoa(p: PessoaBusca): string {
  return p.nome || p.razaoSocial || p.nomeFantasia || `Cliente #${p.id}`
}

async function buscar() {
  const q = termo.value.trim()
  if (q.length < 2) {
    resultados.value = []
    return
  }
  buscando.value = true
  try {
    const resposta = await useApi('/cadastros/pessoas', { query: { termo: q, pagina: 1, tamanhoPagina: 20 } })
    resultados.value = extrairDados<PessoaBusca[]>(resposta) ?? []
  } catch (e) {
    console.error('[ClienteAutocomplete.buscar]', e)
    resultados.value = []
  } finally {
    buscando.value = false
  }
}

function aoDigitar(v: string) {
  termo.value = v
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(buscar, 350)
}

function selecionar(p: PessoaBusca) {
  emit('update:modelValue', p.id)
  emit('update:nomeSelecionado', nomePessoa(p))
  resultados.value = []
  termo.value = ''
}

function limpar() {
  emit('update:modelValue', null)
  emit('update:nomeSelecionado', '')
  termo.value = ''
  resultados.value = []
}
</script>

<template>
  <div class="cliente-autocomplete">
    <div v-if="modelValue" class="cliente-selecionado">
      <span>{{ nomeSelecionado || `Cliente #${modelValue}` }}</span>
      <button type="button" class="btn btn-ghost btn-sm" @click="limpar">Trocar</button>
    </div>
    <template v-else>
      <TextField
        :model-value="termo"
        label="Cliente"
        placeholder="Digite para pesquisar cliente..."
        required
        :error="error"
        @update:model-value="aoDigitar"
      />
      <p v-if="buscando" class="cliente-status">Buscando...</p>
      <ul v-else-if="resultados.length" class="cliente-resultados">
        <li v-for="p in resultados" :key="p.id" @click="selecionar(p)">
          {{ nomePessoa(p) }}
        </li>
      </ul>
      <p v-else-if="termo.length >= 2" class="cliente-status">Nenhum cliente encontrado</p>
    </template>
  </div>
</template>

<style scoped>
.cliente-autocomplete { position: relative; }
.cliente-selecionado {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  font-size: 14px;
}
.cliente-status { font-size: 12px; color: var(--text-muted); margin-top: 4px; }
.cliente-resultados {
  list-style: none;
  margin-top: 4px;
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}
.cliente-resultados li {
  padding: 8px 12px;
  font-size: 13px;
  cursor: pointer;
  border-bottom: 1px solid var(--border-color);
}
.cliente-resultados li:last-child { border-bottom: none; }
.cliente-resultados li:hover { background: rgba(255, 255, 255, 0.05); }
</style>
