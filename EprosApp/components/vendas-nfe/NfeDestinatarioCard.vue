<script setup lang="ts">
/**
 * NfeDestinatarioCard — seleção do destinatário e configuração de atendimento
 * (porta NfeDestinatarioCard do legado, sem Vuetify).
 *
 * Busca o destinatário por nome/documento em `cadastros/pessoas` (autocomplete próprio),
 * exibe o endereço formatado e permite escolher o tipo de atendimento. Quando já há itens na
 * nota, a troca de destinatário fica bloqueada e emite `solicitar-troca` para a página confirmar.
 */
import { ref, computed, watch } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { useMask } from '~/composables/useMask'
import SelectField from '~/components/shared/fields/SelectField.vue'
import { TIPOS_ATENDIMENTO, type NfeForm } from './nfeTypes'

interface PessoaResumo {
  id: number
  nome?: string
  razaoSocial?: string
  nomeFantasia?: string
  documento?: string
  cpfCnpj?: string
  enderecoFormatado?: string
}

const props = defineProps<{
  modelValue: NfeForm
  /** Bloqueia a troca de destinatário (quando já há itens). */
  bloqueado?: boolean
  quantidadeItens?: number
  erro?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfeForm]
  'solicitar-troca': []
  'novo-parceiro': []
}>()

const { maskCpfCnpj } = useMask()

const busca = ref('')
const resultados = ref<PessoaResumo[]>([])
const carregandoBusca = ref(false)
const abertoDropdown = ref(false)
let debounce: ReturnType<typeof setTimeout> | undefined

const destinatario = computed(() => props.modelValue.destinatario)

const opcoesAtendimento = TIPOS_ATENDIMENTO

const nomeExibicao = (p: PessoaResumo): string =>
  p.razaoSocial || p.nome || p.nomeFantasia || `#${p.id}`

const mensagemBloqueio = computed(() => {
  const qtd = props.quantidadeItens ?? 0
  const label = qtd === 1 ? '1 item da nota' : `${qtd} itens da nota`
  return `Para trocar o cliente, ${label} será removido. Clique para alterar.`
})

watch(busca, (texto) => {
  if (props.bloqueado) return
  if (debounce) clearTimeout(debounce)
  if (!texto || texto.length < 2) {
    resultados.value = []
    abertoDropdown.value = false
    return
  }
  debounce = setTimeout(() => buscarPessoas(texto), 350)
})

async function buscarPessoas(texto: string) {
  carregandoBusca.value = true
  try {
    const resp = await useApi('/cadastros/pessoas', { query: { busca: texto, tamanhoPagina: 20 } })
    resultados.value = extrairLista<PessoaResumo>(resp) ?? []
    abertoDropdown.value = resultados.value.length > 0
  } catch (e) {
    console.error('[NfeDestinatarioCard] falha na busca de pessoas', e)
    resultados.value = []
  } finally {
    carregandoBusca.value = false
  }
}

function selecionar(p: PessoaResumo) {
  if (props.bloqueado) {
    emit('solicitar-troca')
    return
  }
  const novo: NfeForm = {
    ...props.modelValue,
    destinatario: {
      ...props.modelValue.destinatario,
      pessoaId: p.id,
      nome: nomeExibicao(p),
      documento: p.documento ?? p.cpfCnpj ?? '',
      enderecoFormatado: p.enderecoFormatado ?? ''
    }
  }
  emit('update:modelValue', novo)
  busca.value = ''
  resultados.value = []
  abertoDropdown.value = false
}

function limpar() {
  if (props.bloqueado) {
    emit('solicitar-troca')
    return
  }
  emit('update:modelValue', {
    ...props.modelValue,
    destinatario: {
      pessoaId: null,
      nome: '',
      documento: '',
      enderecoEntregaId: null,
      enderecoCobrancaId: null,
      enderecoFormatado: ''
    }
  })
}

function definirAtendimento(valor: number | null) {
  emit('update:modelValue', { ...props.modelValue, tipoAtendimento: (valor as number) ?? 0 })
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Destinatário</h2>
      <button type="button" class="btn btn-secondary btn-sm" @click="emit('novo-parceiro')">
        + Novo cliente
      </button>
    </header>

    <!-- Destinatário já selecionado -->
    <div v-if="destinatario.pessoaId" class="dest-selecionado">
      <div class="dest-info">
        <span class="dest-nome">{{ destinatario.nome }}</span>
        <span v-if="destinatario.documento" class="dest-doc">{{ maskCpfCnpj(destinatario.documento) }}</span>
      </div>
      <button
        type="button"
        class="btn btn-ghost btn-sm"
        :title="bloqueado ? mensagemBloqueio : 'Remover destinatário'"
        @click="limpar"
      >
        <span v-if="bloqueado">🔒 Trocar</span>
        <span v-else>Remover</span>
      </button>
    </div>

    <!-- Campo de busca -->
    <div v-else class="dest-busca">
      <label class="field-label">Cliente<span class="required">*</span></label>
      <div class="dest-input-wrap">
        <input
          v-model="busca"
          class="input"
          :class="{ 'is-invalid': !!erro }"
          placeholder="Digite o nome, razão social ou documento do destinatário"
          @focus="abertoDropdown = resultados.length > 0"
        />
        <span v-if="carregandoBusca" class="dest-spinner spinner"></span>
        <ul v-if="abertoDropdown" class="dest-dropdown glass-panel">
          <li v-for="p in resultados" :key="p.id" class="dest-opcao" @click="selecionar(p)">
            <span class="dest-opcao-nome">{{ nomeExibicao(p) }}</span>
            <span v-if="p.documento || p.cpfCnpj" class="dest-opcao-doc">
              {{ maskCpfCnpj(p.documento ?? p.cpfCnpj ?? '') }}
            </span>
          </li>
        </ul>
      </div>
      <span v-if="erro" class="field-error">{{ erro }}</span>
    </div>

    <!-- Endereço + atendimento (só com destinatário) -->
    <template v-if="destinatario.pessoaId">
      <p v-if="destinatario.enderecoFormatado" class="dest-endereco">
        {{ destinatario.enderecoFormatado }}
      </p>
      <div class="form-grid dest-grid">
        <SelectField
          :model-value="modelValue.tipoAtendimento"
          label="Tipo de Atendimento"
          :options="opcoesAtendimento"
          :clearable="false"
          @update:model-value="definirAtendimento($event as number | null)"
        />
      </div>
    </template>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }

.dest-selecionado {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 14px; border: 1px solid var(--border-color); border-radius: 8px;
  background: rgba(255, 255, 255, 0.02);
}
.dest-info { display: flex; flex-direction: column; gap: 2px; }
.dest-nome { font-weight: 600; font-size: 14px; }
.dest-doc { font-size: 12px; color: var(--text-muted); }

.dest-busca { position: relative; }
.dest-input-wrap { position: relative; }
.dest-spinner { position: absolute; right: 12px; top: 12px; }
.dest-dropdown {
  position: absolute; z-index: 30; top: calc(100% + 4px); left: 0; right: 0;
  list-style: none; max-height: 260px; overflow-y: auto; padding: 6px; margin: 0;
}
.dest-opcao {
  display: flex; flex-direction: column; gap: 2px; padding: 8px 10px;
  border-radius: 6px; cursor: pointer;
}
.dest-opcao:hover { background: rgba(255, 255, 255, 0.05); }
.dest-opcao-nome { font-size: 13.5px; font-weight: 500; }
.dest-opcao-doc { font-size: 11.5px; color: var(--text-muted); }

.dest-endereco {
  margin: 12px 0; padding: 10px 12px; border-radius: 8px;
  background: var(--primary-glow, rgba(59, 130, 246, 0.08));
  font-size: 12.5px; color: var(--text-secondary); white-space: pre-wrap;
}
.dest-grid { grid-template-columns: repeat(2, 1fr); gap: 14px; }
@media (max-width: 900px) { .dest-grid { grid-template-columns: 1fr; } }
</style>
