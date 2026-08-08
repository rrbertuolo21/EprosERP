<script setup lang="ts">
/**
 * NfeTransporteCard — modalidade de frete e transportadora (porta NfeTransporteCard do legado).
 *
 * Modalidade de frete via select. Transportadora buscada em `cadastros/pessoas` (autocomplete),
 * opcional. Sem cálculo de volumes/reboques/lacres nesta versão do design novo (mantido enxuto).
 */
import { ref, computed, watch } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { useMask } from '~/composables/useMask'
import SelectField from '~/components/shared/fields/SelectField.vue'
import { MODALIDADES_FRETE, type NfeForm } from './nfeTypes'

interface PessoaResumo {
  id: number
  nome?: string
  razaoSocial?: string
  documento?: string
  cpfCnpj?: string
}

const props = defineProps<{
  modelValue: NfeForm
  readonly?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: NfeForm]
}>()

const { maskCpfCnpj } = useMask()

const busca = ref('')
const resultados = ref<PessoaResumo[]>([])
const carregandoBusca = ref(false)
const abertoDropdown = ref(false)
let debounce: ReturnType<typeof setTimeout> | undefined

const form = computed(() => props.modelValue)
const opcoesFrete = MODALIDADES_FRETE

const nomePessoa = (p: PessoaResumo): string => p.razaoSocial || p.nome || `#${p.id}`

watch(busca, (texto) => {
  if (props.readonly) return
  if (debounce) clearTimeout(debounce)
  if (!texto || texto.length < 2) {
    resultados.value = []
    abertoDropdown.value = false
    return
  }
  debounce = setTimeout(() => buscarTransportadoras(texto), 350)
})

async function buscarTransportadoras(texto: string) {
  carregandoBusca.value = true
  try {
    const resp = await useApi('/cadastros/pessoas', { query: { busca: texto, tamanhoPagina: 20 } })
    resultados.value = extrairLista<PessoaResumo>(resp) ?? []
    abertoDropdown.value = resultados.value.length > 0
  } catch (e) {
    console.error('[NfeTransporteCard] falha na busca de transportadoras', e)
    resultados.value = []
  } finally {
    carregandoBusca.value = false
  }
}

function selecionar(p: PessoaResumo) {
  emit('update:modelValue', {
    ...props.modelValue,
    transportadora: { pessoaId: p.id, nome: nomePessoa(p), documento: p.documento ?? p.cpfCnpj ?? '' }
  })
  busca.value = ''
  resultados.value = []
  abertoDropdown.value = false
}

function limparTransportadora() {
  emit('update:modelValue', {
    ...props.modelValue,
    transportadora: { pessoaId: null, nome: '', documento: '' }
  })
}

function definirFrete(valor: number | null) {
  emit('update:modelValue', { ...props.modelValue, modalidadeFrete: (valor as number) ?? 9 })
}
</script>

<template>
  <section class="glass-panel nfe-card">
    <header class="nfe-card-header">
      <h2 class="nfe-card-title">Transporte</h2>
    </header>

    <div class="form-grid transp-grid">
      <SelectField
        :model-value="form.modalidadeFrete"
        label="Modalidade do Frete"
        :options="opcoesFrete"
        :clearable="false"
        :disabled="readonly"
        @update:model-value="definirFrete($event as number | null)"
      />

      <div class="transp-busca">
        <label class="field-label">Transportadora</label>
        <div v-if="form.transportadora.pessoaId" class="transp-selecionada">
          <div class="transp-info">
            <span class="transp-nome">{{ form.transportadora.nome }}</span>
            <span v-if="form.transportadora.documento" class="transp-doc">
              {{ maskCpfCnpj(form.transportadora.documento) }}
            </span>
          </div>
          <button v-if="!readonly" type="button" class="btn btn-ghost btn-sm" @click="limparTransportadora">
            Remover
          </button>
        </div>
        <div v-else class="transp-input-wrap">
          <input
            v-model="busca"
            class="input"
            :disabled="readonly"
            placeholder="Nome ou documento da transportadora"
          />
          <span v-if="carregandoBusca" class="transp-spinner spinner"></span>
          <ul v-if="abertoDropdown" class="transp-dropdown glass-panel">
            <li v-for="p in resultados" :key="p.id" class="transp-opcao" @click="selecionar(p)">
              <span class="transp-opcao-nome">{{ nomePessoa(p) }}</span>
              <span v-if="p.documento || p.cpfCnpj" class="transp-opcao-doc">
                {{ maskCpfCnpj(p.documento ?? p.cpfCnpj ?? '') }}
              </span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.nfe-card { padding: 18px 20px; margin-bottom: 16px; }
.nfe-card-header { margin-bottom: 14px; }
.nfe-card-title { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); }
.transp-grid { grid-template-columns: 1fr 1fr; gap: 14px; align-items: start; }
@media (max-width: 900px) { .transp-grid { grid-template-columns: 1fr; } }

.transp-busca { position: relative; }
.transp-input-wrap { position: relative; }
.transp-spinner { position: absolute; right: 12px; top: 12px; }
.transp-dropdown {
  position: absolute; z-index: 30; top: calc(100% + 4px); left: 0; right: 0;
  list-style: none; max-height: 220px; overflow-y: auto; padding: 6px; margin: 0;
}
.transp-opcao { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: 6px; cursor: pointer; }
.transp-opcao:hover { background: rgba(255, 255, 255, 0.05); }
.transp-opcao-nome { font-size: 13.5px; font-weight: 500; }
.transp-opcao-doc { font-size: 11.5px; color: var(--text-muted); }

.transp-selecionada {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px; border: 1px solid var(--border-color); border-radius: 8px;
  background: rgba(255, 255, 255, 0.02);
}
.transp-info { display: flex; flex-direction: column; gap: 2px; }
.transp-nome { font-weight: 600; font-size: 13.5px; }
.transp-doc { font-size: 11.5px; color: var(--text-muted); }
</style>
