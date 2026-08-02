<script setup lang="ts">
/**
 * ClienteCard — seleção do destinatário/consumidor da NF-e simplificada.
 *
 * Porta o comportamento do `components/pos/cliente.vue` do legado, sem Vuetify:
 * busca a pessoa por termo em `cadastros/pessoas`, permite escolher da lista e
 * informar/validar o documento do consumidor (CPF/CNPJ). Para a NF-e (modelo 55)
 * o destinatário é obrigatório.
 *
 * Contrato:
 *   props:
 *     destinatario: DestinatarioVenda   (v-model:destinatario)
 *   emits:
 *     'update:destinatario': [valor]
 */
import { ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import type { DestinatarioVenda } from './tipos'

const props = defineProps<{
  destinatario: DestinatarioVenda
}>()

const emit = defineEmits<{
  'update:destinatario': [valor: DestinatarioVenda]
}>()

const { maskCpfCnpj, somenteDigitos } = useMask()
const { validarCpfCnpj } = useDocumento()
const toast = useToast()

/** Pessoa retornada pela busca em `cadastros/pessoas`. */
interface PessoaBusca {
  id: number
  razaoSocial?: string
  nomeFantasia?: string
  cpf?: string
  cnpj?: string
}

const termo = ref('')
const resultados = ref<PessoaBusca[]>([])
const buscando = ref(false)
const erroBusca = ref<string | null>(null)

const documentoMascarado = ref(props.destinatario.documentoConsumidor)
const erroDocumento = ref('')

function atualizar(parcial: Partial<DestinatarioVenda>) {
  emit('update:destinatario', { ...props.destinatario, ...parcial })
}

async function buscarPessoa() {
  const q = termo.value.trim()
  if (q.length < 2) {
    resultados.value = []
    return
  }
  buscando.value = true
  erroBusca.value = null
  try {
    const resposta = await useApi('/cadastros/pessoas', {
      query: { termo: q, pagina: 1, tamanhoPagina: 20 }
    })
    resultados.value = extrairDados<PessoaBusca[]>(resposta) ?? []
  } catch (e) {
    erroBusca.value = 'Erro ao buscar clientes.'
    console.error('[ClienteCard.buscarPessoa]', e)
  } finally {
    buscando.value = false
  }
}

function selecionar(pessoa: PessoaBusca) {
  const doc = pessoa.cnpj || pessoa.cpf || ''
  documentoMascarado.value = doc ? maskCpfCnpj(doc) : ''
  erroDocumento.value = ''
  atualizar({
    pessoaId: pessoa.id,
    descricao: pessoa.razaoSocial || pessoa.nomeFantasia || `Cliente #${pessoa.id}`,
    documentoConsumidor: somenteDigitos(doc),
    enviarDestinatarioNaNfe: true
  })
  resultados.value = []
  termo.value = ''
}

function aoDigitarDocumento(valor: string) {
  documentoMascarado.value = maskCpfCnpj(valor)
  const digitos = somenteDigitos(valor)
  erroDocumento.value =
    digitos.length > 0 && !validarCpfCnpj(digitos) ? 'Documento inválido' : ''
  atualizar({ documentoConsumidor: digitos })
}

function limpar() {
  documentoMascarado.value = ''
  erroDocumento.value = ''
  termo.value = ''
  resultados.value = []
  atualizar({
    pessoaId: 0,
    descricao: '',
    documentoConsumidor: '',
    enviarDestinatarioNaNfe: true
  })
}

defineExpose({ limpar })
</script>

<template>
  <div class="cliente-card glass-panel">
    <div class="cc-header">
      <span class="cc-title">Cliente / Destinatário</span>
      <button
        v-if="destinatario.pessoaId"
        type="button"
        class="btn btn-ghost btn-sm"
        @click="limpar"
      >
        Limpar
      </button>
    </div>

    <div v-if="destinatario.pessoaId" class="cc-selecionado">
      <div class="cc-nome">{{ destinatario.descricao }}</div>
      <div class="cc-doc">{{ documentoMascarado || 'Sem documento' }}</div>
    </div>

    <template v-else>
      <div class="cc-busca">
        <TextField
          v-model="termo"
          label="Buscar cliente"
          placeholder="Nome, razão social, CPF ou CNPJ"
          @blur="buscarPessoa"
        />
        <button type="button" class="btn btn-secondary btn-sm" :disabled="buscando" @click="buscarPessoa">
          {{ buscando ? 'Buscando...' : 'Buscar' }}
        </button>
      </div>

      <p v-if="erroBusca" class="field-error">{{ erroBusca }}</p>

      <ul v-if="resultados.length" class="cc-resultados">
        <li v-for="p in resultados" :key="p.id" class="cc-resultado" @click="selecionar(p)">
          <span class="cc-resultado-nome">{{ p.razaoSocial || p.nomeFantasia || `Cliente #${p.id}` }}</span>
          <span class="cc-resultado-doc">{{ maskCpfCnpj(p.cnpj || p.cpf || '') }}</span>
        </li>
      </ul>
    </template>

    <div class="cc-doc-manual">
      <TextField
        :model-value="documentoMascarado"
        label="Documento do consumidor"
        placeholder="CPF ou CNPJ"
        :error="erroDocumento"
        hint="Obrigatório para NF-e (modelo 55)"
        @update:model-value="aoDigitarDocumento"
      />
    </div>
  </div>
</template>

<style scoped>
.cliente-card { padding: 14px; display: flex; flex-direction: column; gap: 12px; }
.cc-header { display: flex; align-items: center; justify-content: space-between; }
.cc-title { font-weight: 600; font-size: 14px; }
.cc-busca { display: flex; align-items: flex-end; gap: 8px; }
.cc-busca .field { flex: 1; }
.cc-resultados {
  list-style: none;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}
.cc-resultado {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 12px;
  cursor: pointer;
  font-size: 13px;
  border-bottom: 1px solid var(--border-color);
}
.cc-resultado:last-child { border-bottom: none; }
.cc-resultado:hover { background: rgba(255, 255, 255, 0.05); }
.cc-resultado-doc { color: var(--text-muted); }
.cc-selecionado {
  padding: 10px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
}
.cc-nome { font-weight: 600; font-size: 14px; }
.cc-doc { font-size: 12px; color: var(--text-muted); margin-top: 2px; }
</style>
