<script setup lang="ts">
/**
 * Novo Lançamento Contábil — Contabilidade Geral / Lançamentos.
 *
 * Contrato: POST /contabilidade-geral/lancamentos
 * Body: periodoContabilId?, numeroLancamento?, data, historico?,
 *   linhas[] = { contaContabilId, debito, credito, historico? }.
 * Só criação (não há GET/{id} nem PUT). O lançamento nasce em Rascunho e é
 * confirmado/estornado/cancelado na listagem.
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface LinhaLancamento {
  contaContabilId: string | null
  debito: number
  credito: number
  historico: string
}

interface LancamentoForm {
  periodoContabilId: string | null
  numeroLancamento: string
  data: string
  historico: string
  linhas: LinhaLancamento[]
}

interface ContaOpcao { id: string; codigoConta?: string | null; nomeConta?: string | null }
interface PeriodoOpcao { id: string; anoFiscal: number }

const router = useRouter()
const toast = useToast()
const { formatarMoeda } = useHelper()

const salvando = ref(false)
const contas = ref<ContaOpcao[]>([])
const periodos = ref<PeriodoOpcao[]>([])

const opcoesConta = computed<SelectOption[]>(() =>
  contas.value.map((c) => ({ label: `${c.codigoConta ?? ''} — ${c.nomeConta ?? ''}`.trim(), value: c.id }))
)
const opcoesPeriodo = computed<SelectOption[]>(() =>
  periodos.value.map((p) => ({ label: `Ano ${p.anoFiscal}`, value: p.id }))
)

function novaLinha(): LinhaLancamento {
  return { contaContabilId: null, debito: 0, credito: 0, historico: '' }
}

const form = reactive<LancamentoForm>({
  periodoContabilId: null,
  numeroLancamento: '',
  data: new Date().toISOString().slice(0, 10),
  historico: '',
  linhas: [novaLinha(), novaLinha()]
})

const erros = reactive<Record<string, string>>({})

const totalDebito = computed(() => form.linhas.reduce((s, l) => s + (l.debito || 0), 0))
const totalCredito = computed(() => form.linhas.reduce((s, l) => s + (l.credito || 0), 0))
const balanceado = computed(() => Math.abs(totalDebito.value - totalCredito.value) < 0.005 && totalDebito.value > 0)

function adicionarLinha() {
  form.linhas.push(novaLinha())
}
function removerLinha(i: number) {
  if (form.linhas.length > 1) form.linhas.splice(i, 1)
}

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.data) erros.data = 'Data é obrigatória.'
  if (!form.linhas.length) erros.linhas = 'Informe ao menos uma linha.'
  if (form.linhas.some((l) => !l.contaContabilId)) erros.linhas = 'Toda linha precisa de uma conta contábil.'
  if (!balanceado.value) erros.balanco = 'A soma dos débitos deve ser igual à soma dos créditos e maior que zero.'
  return Object.keys(erros).length === 0
}

async function carregarDados() {
  try {
    const [rConta, rPer] = await Promise.all([
      useApi('/contabilidade-geral/contas', { query: { tamanhoPagina: 100 } }),
      useApi('/contabilidade-geral/periodos')
    ])
    const dConta = extrairDados<{ itens?: ContaOpcao[] } | ContaOpcao[]>(rConta)
    contas.value = Array.isArray(dConta) ? dConta : dConta?.itens ?? []
    periodos.value = extrairLista<PeriodoOpcao>(rPer) ?? []
  } catch (e) {
    console.error('[contabilidade/lancamentos/novo] carregarDados', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi('/contabilidade-geral/lancamentos', {
      method: 'POST',
      body: {
        periodoContabilId: form.periodoContabilId,
        numeroLancamento: form.numeroLancamento || null,
        data: form.data,
        historico: form.historico || null,
        linhas: form.linhas.map((l) => ({
          contaContabilId: l.contaContabilId,
          debito: l.debito || 0,
          credito: l.credito || 0,
          historico: l.historico || null
        }))
      }
    })
    toast.success('Lançamento contábil criado com sucesso!')
    await router.push('/erp/contabilidade/lancamentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/contabilidade/lancamentos')
}

onMounted(carregarDados)
</script>

<template>
  <div>
    <PageToolbar title="Novo lançamento contábil" :loading="salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <SelectField v-model="form.periodoContabilId" label="Período Contábil" :options="opcoesPeriodo" />
        <TextField v-model="form.numeroLancamento" label="Número do Lançamento" maxlength="30" />
        <DateTimeField v-model="form.data" label="Data" required :error="erros.data" />
        <TextField v-model="form.historico" label="Histórico" maxlength="200" />
      </div>
    </div>

    <div class="glass-panel form-panel">
      <div class="linhas-header">
        <h3 class="secao-titulo">Partidas (linhas)</h3>
        <button type="button" class="btn btn-secondary btn-sm" @click="adicionarLinha">+ Adicionar linha</button>
      </div>
      <p v-if="erros.linhas" class="field-error">{{ erros.linhas }}</p>

      <div v-for="(linha, i) in form.linhas" :key="i" class="linha-grid">
        <SelectField v-model="linha.contaContabilId" label="Conta Contábil" :options="opcoesConta" class="linha-conta" />
        <MoneyInput v-model="linha.debito" label="Débito" />
        <MoneyInput v-model="linha.credito" label="Crédito" />
        <TextField v-model="linha.historico" label="Histórico da linha" />
        <button
          type="button"
          class="btn btn-ghost btn-sm btn-danger-action linha-remover"
          :disabled="form.linhas.length <= 1"
          title="Remover linha"
          @click="removerLinha(i)"
        >Remover</button>
      </div>

      <div class="totais">
        <div class="total-item">
          <span class="total-label">Total Débito</span>
          <span class="total-valor">{{ formatarMoeda(totalDebito) }}</span>
        </div>
        <div class="total-item">
          <span class="total-label">Total Crédito</span>
          <span class="total-valor">{{ formatarMoeda(totalCredito) }}</span>
        </div>
        <div class="total-item">
          <span class="total-label">Situação</span>
          <span class="badge" :class="balanceado ? 'badge-success' : 'badge-warning'">
            {{ balanceado ? 'Balanceado' : 'Não balanceado' }}
          </span>
        </div>
      </div>
      <p v-if="erros.balanco" class="field-error">{{ erros.balanco }}</p>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-bottom: 16px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
.linhas-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}
.secao-titulo { font-size: 14px; color: var(--text-primary); margin: 0; }
.linha-grid {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 2fr auto;
  gap: 12px;
  align-items: end;
  padding: 8px 0;
  border-bottom: 1px solid var(--border-color);
}
.linha-remover { margin-bottom: 4px; }
.totais {
  display: flex;
  gap: 32px;
  margin-top: 16px;
}
.total-item { display: flex; flex-direction: column; gap: 4px; }
.total-label { font-size: 11px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.4px; }
.total-valor { font-size: 15px; font-weight: 700; }
@media (max-width: 900px) {
  .linha-grid { grid-template-columns: 1fr; }
}
</style>
