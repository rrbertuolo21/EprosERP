<script setup lang="ts">
/**
 * Formulário de CFOP (novo/edição) — Fiscal / CFOP.
 *
 * Porta o comportamento de `fiscal/cfop/[id].vue` do legado: código, correlação,
 * descrição, natureza da operação, grupo de indicadores e incidência do Simples Nacional.
 * A validação de CFOP de devolução espelha a regra do backend (`Cfop.ValidarCfopDevolucao`):
 * o primeiro dígito do CFOP de devolução deve corresponder ao primeiro dígito do CFOP principal
 * (1↔5, 2↔6, 3↔7).
 * Endpoint: `cfops`. Se a rota veio de "Importar da tabela padrão" (`?origemCodigo=`),
 * pré-carrega os dados a partir de `cfop-padrao/obter-por-cfop/{codigo}`.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface CfopForm {
  id?: string
  cfopCodigo: number | null
  cfopCorrelacao: string
  cfopDevolucao: string | null
  descricao: string
  naturezaOperacao: string
  integraFaturamento: boolean
  indicadorNfe: boolean
  indicadorNfce: boolean
  indicadorMei: boolean
  indicadorTransporte: boolean
  indicadorRemessa: boolean
  indicadorTransferencia: boolean
  indicadorRetorno: boolean
  indicadorUsoConsumo: boolean
  indicadorUsoSemOperacao: boolean
  indicadorCombustivel: boolean
  indicadorDevolucao: boolean
  indicadorSt: boolean
  indicadorAnulacao: boolean
  indicadorComunicacao: boolean
  indicadorCiap: boolean
  incidenciaSimples: number
}

// Espelha EIncidenciaSimples (Epros.Shared.Domain.Enums) — sem endpoint de enum dedicado.
const opcoesIncidenciaSimples: SelectOption[] = [
  { label: '1 - Revenda de Produtos Industrializados', value: 1 },
  { label: '2 - Revenda de Mercadorias', value: 2 },
  { label: '3 - Locação de Bens Móveis', value: 3 },
  { label: '4 - Serviço de Transporte de Cargas', value: 4 },
  { label: '5 - Serviço de Comunicação', value: 5 },
  { label: '6 - Prestação de Serviços', value: 6 },
  { label: '7 - Exterior', value: 7 },
  { label: '9 - Não se Aplica', value: 9 }
]

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const cfopId = computed(() => (isEdit.value ? idParam : undefined))

const carregando = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const form = reactive<CfopForm>({
  cfopCodigo: null,
  cfopCorrelacao: '',
  cfopDevolucao: null,
  descricao: '',
  naturezaOperacao: '',
  integraFaturamento: false,
  indicadorNfe: false,
  indicadorNfce: false,
  indicadorMei: false,
  indicadorTransporte: false,
  indicadorRemessa: false,
  indicadorTransferencia: false,
  indicadorRetorno: false,
  indicadorUsoConsumo: false,
  indicadorUsoSemOperacao: false,
  indicadorCombustivel: false,
  indicadorDevolucao: false,
  indicadorSt: false,
  indicadorAnulacao: false,
  indicadorComunicacao: false,
  indicadorCiap: false,
  incidenciaSimples: 9
})

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

/** Regra de correlação de CFOP de devolução, espelhando `Cfop.ValidarCfopDevolucao` no backend. */
function validarCfopDevolucao(): boolean {
  if (!form.cfopDevolucao) return true
  const inicial = String(form.cfopCodigo ?? '')[0]
  const mapa: Record<string, string> = { '1': '5', '2': '6', '3': '7', '5': '1', '6': '2', '7': '3' }
  const esperado = mapa[inicial]
  if (esperado && !form.cfopDevolucao.startsWith(esperado)) {
    erros.cfopDevolucao = `CFOP de devolução deve iniciar com ${esperado}.`
    return false
  }
  return true
}

function validar(): boolean {
  limparErros()
  if (!form.cfopCodigo || form.cfopCodigo < 1000 || form.cfopCodigo > 9999) {
    erros.cfopCodigo = 'O CFOP deve possuir exatamente 4 dígitos.'
  }
  if (!form.descricao?.trim()) erros.descricao = 'A descrição é obrigatória.'
  if (!form.naturezaOperacao?.trim()) erros.naturezaOperacao = 'A natureza de operação é obrigatória.'
  if (!form.cfopCorrelacao || form.cfopCorrelacao.length !== 4) {
    erros.cfopCorrelacao = 'O CFOP de correlação deve possuir exatamente 4 dígitos.'
  }
  const devolucaoOk = validarCfopDevolucao()
  return Object.keys(erros).length === 0 && devolucaoOk
}

async function carregarCfop() {
  if (!isEdit.value || !cfopId.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/cfops/${cfopId.value}`)
    const dados = extrairDados<Partial<CfopForm>>(resposta)
    if (dados) Object.assign(form, dados)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

/** Pré-carrega os dados a partir da tabela padrão (fluxo "Importar da tabela padrão"). */
async function carregarDeOrigemPadrao(codigo: string) {
  carregando.value = true
  try {
    const resposta = await useApi(`/cfop-padrao/obter-por-cfop/${codigo}`)
    const dados = extrairDados<Partial<CfopForm>>(resposta)
    if (dados) {
      Object.assign(form, dados)
      form.cfopCodigo = Number(codigo)
    }
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
    if (isEdit.value) {
      await useApi(`/cfops/${cfopId.value}`, { method: 'PUT', body: { id: cfopId.value, ...form } })
    } else {
      await useApi('/cfops', { method: 'POST', body: form })
    }
    toast.success('CFOP salvo com sucesso!')
    router.push('/erp/fiscal/cfop')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/fiscal/cfop')
}

onMounted(async () => {
  const origemCodigo = route.query.origemCodigo as string | undefined
  if (!isEdit.value && origemCodigo) {
    await carregarDeOrigemPadrao(origemCodigo)
  } else {
    await carregarCfop()
  }
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar CFOP' : 'Novo CFOP'" :loading="carregando">
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
          <TextField v-model.number="form.cfopCodigo" label="CFOP" type="number" required :error="erros.cfopCodigo" :disabled="isEdit" />
          <TextField v-model="form.cfopCorrelacao" label="CFOP de Correlação" required maxlength="4" :error="erros.cfopCorrelacao" />
          <TextField v-model="form.cfopDevolucao" label="CFOP de Devolução" maxlength="4" :error="erros.cfopDevolucao" />
        </div>
        <div class="form-grid form-grid-1">
          <TextField v-model="form.descricao" label="Descrição" required maxlength="1000" :error="erros.descricao" />
          <TextField v-model="form.naturezaOperacao" label="Natureza da Operação" required maxlength="1000" :error="erros.naturezaOperacao" />
        </div>

        <h3 class="section-title">Grupo Fiscal</h3>
        <div class="checkbox-grid">
          <label class="field-checkbox"><input v-model="form.indicadorNfe" type="checkbox" /> NF-e</label>
          <label class="field-checkbox"><input v-model="form.indicadorNfce" type="checkbox" /> NFC-e</label>
          <label class="field-checkbox"><input v-model="form.indicadorMei" type="checkbox" /> MEI</label>
          <label class="field-checkbox"><input v-model="form.indicadorTransporte" type="checkbox" /> Transporte</label>
        </div>

        <h3 class="section-title">Indicador CFOP</h3>
        <div class="checkbox-grid">
          <label class="field-checkbox"><input v-model="form.indicadorRemessa" type="checkbox" /> Remessa</label>
          <label class="field-checkbox"><input v-model="form.indicadorTransferencia" type="checkbox" /> Transferência</label>
          <label class="field-checkbox"><input v-model="form.indicadorRetorno" type="checkbox" /> Retorno</label>
          <label class="field-checkbox"><input v-model="form.indicadorUsoConsumo" type="checkbox" /> Uso e Consumo</label>
          <label class="field-checkbox"><input v-model="form.indicadorUsoSemOperacao" type="checkbox" /> Sem Operação</label>
          <label class="field-checkbox"><input v-model="form.indicadorCombustivel" type="checkbox" /> Combustível</label>
          <label class="field-checkbox"><input v-model="form.indicadorDevolucao" type="checkbox" /> Devolução</label>
          <label class="field-checkbox"><input v-model="form.indicadorSt" type="checkbox" /> Substituição Tributária</label>
          <label class="field-checkbox"><input v-model="form.indicadorAnulacao" type="checkbox" /> Anulação</label>
          <label class="field-checkbox"><input v-model="form.indicadorComunicacao" type="checkbox" /> Comunicação</label>
          <label class="field-checkbox"><input v-model="form.indicadorCiap" type="checkbox" /> CIAP</label>
        </div>

        <h3 class="section-title">Cálculo Simples Nacional</h3>
        <div class="form-grid">
          <SelectField v-model.number="form.incidenciaSimples" label="Incidência para o Simples" required :options="opcoesIncidenciaSimples" />
          <label class="field-checkbox toggle-row"><input v-model="form.integraFaturamento" type="checkbox" /> Integra Faturamento</label>
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
  margin-bottom: 12px;
}
.form-grid-1 { grid-template-columns: 1fr; }
.section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin: 20px 0 10px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 6px;
}
.checkbox-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 10px;
}
.field-checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13.5px;
  color: var(--text-secondary);
}
.field-checkbox input { width: 16px; height: 16px; accent-color: var(--primary); }
.toggle-row { align-items: center; padding-top: 22px; }
</style>
