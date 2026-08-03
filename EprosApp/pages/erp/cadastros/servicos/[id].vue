<script setup lang="ts">
/**
 * Formulário de Serviço (criar/editar) — cadastros/servicos/[id].vue.
 * Porta o comportamento de `cadastros/servicos/[id].vue` do legado (3 seções:
 * Dados Básicos, Impostos e Retenções, Outros), sem markup Vuetify.
 *
 * Rota `novo` cria; qualquer outro valor numérico edita o registro existente.
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface UnidadeMedida {
  id: number
  descricao: string
}

interface CodigoServicoSefaz {
  id: number
  codigo: string
  descricao: string
}

interface ServicoForm {
  id?: number
  codigo: string
  descricao: string
  codigoNbs: string
  cnae: number | null
  codigoServicoSefazId: number | null
  unidadeMedidaId: number | null
  valor: number
  servicoAtivo: boolean
  indicadorIss: boolean
  indicadorIncentivo: boolean
  calcularRetencao: boolean
  aliquotaIss: number | null
  aliquotaIssRetido: number | null
  aliquotaPis: number | null
  aliquotaCofins: number | null
  aliquotaInss: number | null
  aliquotaIrrfRetido: number | null
  cstPisCofins: number | null
  anexoSimplesNacional: number | null
  informacaoAdicional: string | null
  cClassTrib: string | null
  cstIbsCbs: string | null
}

const route = useRoute()
const toast = useToast()

const idParam = computed(() => {
  const p = route.params.id
  return Array.isArray(p) ? p[0] : p
})
const isEdit = computed(() => !!idParam.value && idParam.value !== 'novo')
const tituloPagina = computed(() => (isEdit.value ? 'Editar Serviço' : 'Novo Serviço'))

const abaAtiva = ref<'dadosBasicos' | 'impostos' | 'outros'>('dadosBasicos')

const form = ref<ServicoForm>({
  codigo: '',
  descricao: '',
  codigoNbs: '',
  cnae: null,
  codigoServicoSefazId: null,
  unidadeMedidaId: null,
  valor: 0,
  servicoAtivo: true,
  indicadorIss: false,
  indicadorIncentivo: false,
  calcularRetencao: false,
  aliquotaIss: null,
  aliquotaIssRetido: null,
  aliquotaPis: null,
  aliquotaCofins: null,
  aliquotaInss: null,
  aliquotaIrrfRetido: null,
  cstPisCofins: 0,
  anexoSimplesNacional: 0,
  informacaoAdicional: null,
  cClassTrib: null,
  cstIbsCbs: null
})

const erros = ref<Record<string, string>>({})

const carregando = ref(false)
const salvando = ref(false)

const unidades = ref<UnidadeMedida[]>([])
const carregandoUnidades = ref(false)
const opcoesUnidades = computed<SelectOption[]>(() =>
  unidades.value.map((u) => ({ label: u.descricao, value: u.id }))
)

const codigosSefaz = ref<CodigoServicoSefaz[]>([])
const carregandoCodigosSefaz = ref(false)
const opcoesCodigosSefaz = computed<SelectOption[]>(() =>
  codigosSefaz.value.map((c) => ({ label: `${c.codigo} - ${c.descricao}`, value: c.id }))
)

async function carregarUnidades() {
  carregandoUnidades.value = true
  try {
    const resposta = await useApi('/unidades-de-medidas-comercial', { query: { tamanhoPagina: 200 } })
    unidades.value = extrairDados<UnidadeMedida[]>(resposta) ?? []
  } catch (e) {
    console.error('[servicos/[id]] erro ao carregar unidades', e)
  } finally {
    carregandoUnidades.value = false
  }
}

async function carregarCodigosSefaz() {
  carregandoCodigosSefaz.value = true
  try {
    const resposta = await useApi('/codigos-servicos-sefaz', { query: { tamanhoPagina: 200 } })
    codigosSefaz.value = extrairDados<CodigoServicoSefaz[]>(resposta) ?? []
  } catch (e) {
    console.error('[servicos/[id]] erro ao carregar códigos SEFAZ', e)
  } finally {
    carregandoCodigosSefaz.value = false
  }
}

async function carregarServico() {
  if (!isEdit.value || !idParam.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/servicos/{id}`, { params: { id: idParam.value } })
    const dados = extrairDados<ServicoForm>(resposta)
    if (dados) {
      form.value = { ...form.value, ...dados }
    }
  } catch (e) {
    toast.error('Não foi possível carregar o serviço')
    console.error('[servicos/[id]] erro ao carregar serviço', e)
    await navigateTo('/erp/cadastros/servicos')
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  const novosErros: Record<string, string> = {}
  if (!form.value.codigo?.trim()) novosErros.codigo = 'Código é obrigatório'
  if (!form.value.descricao?.trim()) novosErros.descricao = 'Descrição é obrigatória'
  if (!form.value.codigoNbs?.trim()) novosErros.codigoNbs = 'Código NBS é obrigatório'
  if (form.value.cnae == null) novosErros.cnae = 'CNAE é obrigatório'
  if (form.value.unidadeMedidaId == null) novosErros.unidadeMedidaId = 'Unidade de medida é obrigatória'
  if (form.value.codigoServicoSefazId == null) novosErros.codigoServicoSefazId = 'Código de serviço SEFAZ é obrigatório'
  erros.value = novosErros
  return Object.keys(novosErros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação')
    return
  }

  salvando.value = true
  try {
    if (isEdit.value && idParam.value) {
      await useApi(`/servicos/{id}`, {
        method: 'PUT',
        params: { id: idParam.value },
        body: { ...form.value, id: Number(idParam.value) }
      })
      toast.success('Serviço atualizado com sucesso!')
    } else {
      await useApi('/servicos', { method: 'POST', body: form.value })
      toast.success('Serviço criado com sucesso!')
    }
    await navigateTo('/erp/cadastros/servicos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  navigateTo('/erp/cadastros/servicos')
}

onMounted(async () => {
  await Promise.all([carregarUnidades(), carregarCodigosSefaz()])
  if (isEdit.value) {
    await carregarServico()
  }
})
</script>

<template>
  <div>
    <PageToolbar :title="tituloPagina" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div class="tabs">
        <button type="button" class="tab" :class="{ active: abaAtiva === 'dadosBasicos' }" @click="abaAtiva = 'dadosBasicos'">
          Dados Básicos
        </button>
        <button type="button" class="tab" :class="{ active: abaAtiva === 'impostos' }" @click="abaAtiva = 'impostos'">
          Impostos e Retenções
        </button>
        <button type="button" class="tab" :class="{ active: abaAtiva === 'outros' }" @click="abaAtiva = 'outros'">
          Outros
        </button>
      </div>

      <div v-if="abaAtiva === 'dadosBasicos'" class="form-grid">
        <TextField
          v-model="form.codigo"
          label="Código"
          required
          :error="erros.codigo"
          class="col-4"
        />
        <TextField
          v-model="form.descricao"
          label="Descrição"
          required
          :error="erros.descricao"
          class="col-8"
        />
        <TextField
          v-model="form.codigoNbs"
          label="Código NBS"
          required
          :error="erros.codigoNbs"
          class="col-4"
        />
        <TextField
          v-model.number="form.cnae"
          type="number"
          label="CNAE"
          required
          :error="erros.cnae"
          class="col-4"
        />
        <MoneyInput v-model="form.valor" label="Valor" class="col-4" />

        <SelectField
          v-model="form.unidadeMedidaId"
          label="Unidade de Medida"
          required
          :options="opcoesUnidades"
          :disabled="carregandoUnidades"
          :error="erros.unidadeMedidaId"
          class="col-6"
        />
        <SelectField
          v-model="form.codigoServicoSefazId"
          label="Código Serviço SEFAZ"
          required
          :options="opcoesCodigosSefaz"
          :disabled="carregandoCodigosSefaz"
          :error="erros.codigoServicoSefazId"
          class="col-6"
        />

        <div class="field col-12">
          <label class="field-label">Informação Adicional</label>
          <textarea v-model="form.informacaoAdicional" class="input textarea" rows="3"></textarea>
        </div>

        <label class="filter-checkbox col-4">
          <input v-model="form.servicoAtivo" type="checkbox" />
          <span>{{ form.servicoAtivo ? 'Ativo' : 'Inativo' }}</span>
        </label>
      </div>

      <div v-else-if="abaAtiva === 'impostos'" class="form-grid">
        <PercentInput v-model="form.aliquotaIss" label="Alíquota ISS (%)" class="col-4" />
        <PercentInput v-model="form.aliquotaIssRetido" label="Alíquota ISS Retido (%)" class="col-4" />
        <PercentInput v-model="form.aliquotaPis" label="Alíquota PIS (%)" class="col-4" />
        <PercentInput v-model="form.aliquotaCofins" label="Alíquota COFINS (%)" class="col-4" />
        <PercentInput v-model="form.aliquotaInss" label="Alíquota INSS (%)" class="col-4" />
        <PercentInput v-model="form.aliquotaIrrfRetido" label="Alíquota IRRF Retido (%)" class="col-4" />
        <TextField v-model.number="form.cstPisCofins" type="number" label="CST PIS/COFINS" class="col-4" />
        <TextField v-model="form.cstIbsCbs" label="CST IBS/CBS" class="col-4" />
        <TextField v-model="form.cClassTrib" label="C. Class Trib" class="col-4" />
      </div>

      <div v-else class="form-grid">
        <label class="filter-checkbox col-4">
          <input v-model="form.indicadorIss" type="checkbox" />
          <span>{{ form.indicadorIss ? 'Indicador ISS' : 'Sem Indicador ISS' }}</span>
        </label>
        <label class="filter-checkbox col-4">
          <input v-model="form.indicadorIncentivo" type="checkbox" />
          <span>{{ form.indicadorIncentivo ? 'Indicador Incentivo' : 'Sem Indicador Incentivo' }}</span>
        </label>
        <label class="filter-checkbox col-4">
          <input v-model="form.calcularRetencao" type="checkbox" />
          <span>{{ form.calcularRetencao ? 'Calcular Retenção' : 'Não Calcular Retenção' }}</span>
        </label>
        <TextField v-model.number="form.anexoSimplesNacional" type="number" label="Anexo Simples Nacional" class="col-4" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-panel {
  padding: 16px;
}
.tabs {
  display: flex;
  gap: 4px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  margin-bottom: 16px;
}
.tab {
  background: transparent;
  border: none;
  color: var(--text-secondary);
  padding: 10px 16px;
  cursor: pointer;
  font-size: 14px;
  border-bottom: 2px solid transparent;
}
.tab.active {
  color: var(--primary);
  border-bottom-color: var(--primary);
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 12px 16px;
}
.col-4 { grid-column: span 4; }
.col-6 { grid-column: span 6; }
.col-8 { grid-column: span 8; }
.col-12 { grid-column: span 12; }
.textarea {
  width: 100%;
  resize: vertical;
  font-family: inherit;
}
@media (max-width: 900px) {
  .col-4, .col-6, .col-8 { grid-column: span 12; }
}
</style>
