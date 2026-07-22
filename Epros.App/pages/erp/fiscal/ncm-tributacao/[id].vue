<script setup lang="ts">
/**
 * Formulário de NCM Tributação (novo/edição) — Fiscal / NCM Tributação.
 *
 * Porta o comportamento de `fiscal/ncm-tributacao/[id].vue` do legado, organizado em abas:
 * Geral (CFOPs por tipo de nota, código de benefício fiscal, textos), ICMS (origem, CSOSN/CST
 * dentro e fora do estado, reduções de base), PIS/COFINS (CST de entrada/saída, alíquotas),
 * IPI (CST entrada/saída, alíquota, enquadramento), IBS/CBS (CST + cClassTrib da Reforma
 * Tributária para NF-e e NFC-e), NCMs da Regra (vínculo de códigos NCM), Substituição
 * Tributária (linhas por UF) e Fundo de Combate à Pobreza (linhas por UF).
 *
 * A criação (`CriarNcmTributacaoCommand`) grava os campos "Geral/ICMS/PIS-COFINS/IPI/IBS-CBS"
 * de uma vez; ST e Fundo de Combate à Pobreza são sub-recursos adicionados via endpoints
 * próprios após a regra existir (`/sts`, `/fundos-combate-pobreza`) — por isso essas duas
 * abas só ficam habilitadas em modo edição, como no legado (a entidade precisa existir
 * primeiro no backend).
 *
 * Endpoints: `fiscal/ncm-tributacoes` (+ subrotas sts/fundos-combate-pobreza/configuracoes/ibs-cbs),
 * `fiscal/ncms` (busca de NCM), `cfops`, `fiscal/cests`, `fiscal/csts-ibs-cbs`.
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'
import {
  opcoesOrigemMercadoria,
  opcoesCsosn,
  opcoesCstIcms,
  opcoesCstPisCofins,
  opcoesCstIpi,
  opcoesTipoReducao,
  opcoesDestinoReducao,
  opcoesCodigoValorFiscalIcms,
  opcoesTipoCalculoSt,
  ufsBrasil,
  TIPO_REDUCAO_NAO_UTILIZA,
  CODIGO_VALOR_FISCAL_TRIBUTADO
} from './_enums'

definePageMeta({ layout: 'default' })

type Aba = 'geral' | 'icms' | 'pis-cofins' | 'ipi' | 'ibs-cbs' | 'ncms' | 'st' | 'fcp'

interface NcmOpcao {
  id: string
  codigoNcm: string
  descricao: string
}

interface CfopOpcao {
  id: string
  cfopCodigo: number
  descricao: string
  indicadorNfe: boolean
  indicadorNfce: boolean
}

interface CestOpcao {
  id: string
  codigoCest: string
  descricao: string
}

interface CstIbsCbsOpcao {
  id: string
  cst: string
  descricao: string
}

interface NcmTributacaoForm {
  id?: string
  tributarioGrupoId: string | null
  codigoBeneficioFiscalId: string | null
  codRegra: number | null
  descricao: string
  cfopNotaConsumidor: number | null
  cfopNotaFiscal: number | null
  cfopNotaFiscalInterestadual: number | null
  origem: number | null
  csosnNotaConsumidor: number | null
  cstIcmsNotaConsumidor: number | null
  csosnNotaFiscal: number | null
  cstIcmsNotaFiscalInterna: number | null
  cstIcmsNotaFiscalInterstadual: number | null
  codigoValorFiscalIcmsInterna: number | null
  valorAliquotaIcmsInterna: number | null
  tipoReducaoIcmsInterna: number | null
  valorPercentualReducacaoBcIcmsInterna: number | null
  destinoReducaoIcmsInterna: number | null
  codigoValorFiscalcmsInterstadual: number | null
  valorAliquotaIcmsInterstadual: number | null
  tipoReducaoIcmsInterstadual: number | null
  valorPercentualReducacaoBcIcmsInterstadual: number | null
  destinoReducaoIcmsInterstadual: number | null
  cstPis: number | null
  cstCofins: number | null
  valorUnitFixoPis: number | null
  valorUnitFixoCofins: number | null
  valorAliquotaPis: number | null
  valorAliquotaCofins: number | null
  cstPisCofinsEntrada: number | null
  cstIpiSaida: number | null
  cstIpiEntrada: number | null
  valorAliquotaIpi: number | null
  valorPercentualReducacaoBcIpi: number | null
  tipoReducaoIpi: number | null
  destinoReducaoIpi: number | null
  ipiEmbutido: boolean
  enquadramentoIpi: string | null
  codigoBeneficioFiscalIcms: string | null
  motivoDesoneracaoIcms: number
  informacoesComplementares: string | null
  informacoesAdicionaisAoFisco: string | null
  cstIbsCbsNfe: string | null
  cClassTribNfe: string | null
  cstIbsCbsNfce: string | null
  cClassTribNfce: string | null
}

interface NcmConfiguracao {
  id?: string
  ncmId: string
  ncm?: NcmOpcao
}

interface NcmTributacaoSt {
  id?: string
  uf: string
  tipoCalculo: number
  valorAliquotaIcmsSt: number
  valorMva: number
  valorReducaoBcIcmsSt: number
  tipoReducaoIcmsSt: number
  valorUnitarioSt: number
  valorPercentualFcpSt: number
}

interface NcmTributacaoFcp {
  id?: string
  uf: string
  valorPercentual: number
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')
const tributacaoId = computed(() => (isEdit.value ? idParam : undefined))

const abaAtiva = ref<Aba>('geral')
const carregando = ref(false)
const salvando = ref(false)
const erros = reactive<Record<string, string>>({})

const cfops = ref<CfopOpcao[]>([])
const cests = ref<CestOpcao[]>([])
const cstsIbsCbs = ref<CstIbsCbsOpcao[]>([])

const form = reactive<NcmTributacaoForm>({
  tributarioGrupoId: null,
  codigoBeneficioFiscalId: null,
  codRegra: null,
  descricao: '',
  cfopNotaConsumidor: null,
  cfopNotaFiscal: null,
  cfopNotaFiscalInterestadual: null,
  origem: 0,
  csosnNotaConsumidor: null,
  cstIcmsNotaConsumidor: null,
  csosnNotaFiscal: null,
  cstIcmsNotaFiscalInterna: null,
  cstIcmsNotaFiscalInterstadual: null,
  codigoValorFiscalIcmsInterna: CODIGO_VALOR_FISCAL_TRIBUTADO,
  valorAliquotaIcmsInterna: 0,
  tipoReducaoIcmsInterna: TIPO_REDUCAO_NAO_UTILIZA,
  valorPercentualReducacaoBcIcmsInterna: 0,
  destinoReducaoIcmsInterna: -1,
  codigoValorFiscalcmsInterstadual: CODIGO_VALOR_FISCAL_TRIBUTADO,
  valorAliquotaIcmsInterstadual: 0,
  tipoReducaoIcmsInterstadual: TIPO_REDUCAO_NAO_UTILIZA,
  valorPercentualReducacaoBcIcmsInterstadual: 0,
  destinoReducaoIcmsInterstadual: -1,
  cstPis: null,
  cstCofins: null,
  valorUnitFixoPis: 0,
  valorUnitFixoCofins: 0,
  valorAliquotaPis: 0,
  valorAliquotaCofins: 0,
  cstPisCofinsEntrada: null,
  cstIpiSaida: null,
  cstIpiEntrada: null,
  valorAliquotaIpi: 0,
  valorPercentualReducacaoBcIpi: 0,
  tipoReducaoIpi: TIPO_REDUCAO_NAO_UTILIZA,
  destinoReducaoIpi: -1,
  ipiEmbutido: false,
  enquadramentoIpi: null,
  codigoBeneficioFiscalIcms: null,
  motivoDesoneracaoIcms: 0,
  informacoesComplementares: null,
  informacoesAdicionaisAoFisco: null,
  cstIbsCbsNfe: null,
  cClassTribNfe: null,
  cstIbsCbsNfce: null,
  cClassTribNfce: null
})

function formatarCfop(c: CfopOpcao): SelectOption {
  return { label: `${c.cfopCodigo} - ${c.descricao}`, value: c.cfopCodigo }
}

const opcoesCfopNfce = computed(() => cfops.value.filter((c) => c.indicadorNfce).map(formatarCfop))
const opcoesCfopNfeSaidaInterna = computed(() =>
  cfops.value.filter((c) => c.indicadorNfe && String(c.cfopCodigo).startsWith('5')).map(formatarCfop)
)
const opcoesCfopNfeSaidaInterestadual = computed(() =>
  cfops.value.filter((c) => c.indicadorNfe && String(c.cfopCodigo).startsWith('6')).map(formatarCfop)
)

const opcoesCstIbsCbs = computed<SelectOption[]>(() =>
  cstsIbsCbs.value.map((c) => ({ label: `${c.cst} - ${c.descricao}`, value: c.cst }))
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codRegra || form.codRegra <= 0) erros.codRegra = 'O código da regra deve ser maior que zero.'
  if (form.descricao && form.descricao.length > 200) erros.descricao = 'A descrição deve ter no máximo 200 caracteres.'
  if (!form.tributarioGrupoId) erros.tributarioGrupoId = 'O grupo tributário é obrigatório.'
  if (form.origem === null) erros.origem = 'A origem é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregarCfops() {
  try {
    const resposta = await useApi('/cfops', { query: { tamanhoPagina: 500 } })
    cfops.value = extrairDados<CfopOpcao[]>(resposta) ?? []
  } catch (e) {
    console.error('[ncm-tributacao/[id]] cfops', e)
  }
}

async function carregarCests() {
  try {
    const resposta = await useApi('/fiscal/cests', { query: { tamanhoPagina: 500 } })
    cests.value = extrairDados<CestOpcao[]>(resposta) ?? []
  } catch (e) {
    console.error('[ncm-tributacao/[id]] cests', e)
  }
}

async function carregarCstsIbsCbs() {
  try {
    const resposta = await useApi('/fiscal/csts-ibs-cbs', { query: { tamanhoPagina: 200 } })
    cstsIbsCbs.value = extrairDados<CstIbsCbsOpcao[]>(resposta) ?? []
  } catch (e) {
    console.error('[ncm-tributacao/[id]] csts-ibs-cbs', e)
  }
}

async function carregarTributacao() {
  if (!isEdit.value || !tributacaoId.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}`)
    const dados = extrairDados<Partial<NcmTributacaoForm> & {
      ncmConfiguracoes?: NcmConfiguracao[]
      ncmTributacaoSts?: NcmTributacaoSt[]
      ncmTributacaoFundoCombatePobrezas?: NcmTributacaoFcp[]
    }>(resposta)
    if (dados) {
      Object.assign(form, dados)
      ncmConfiguracoes.value = dados.ncmConfiguracoes ?? []
      linhasSt.value = dados.ncmTributacaoSts ?? []
      linhasFcp.value = dados.ncmTributacaoFundoCombatePobrezas ?? []
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
      await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}`, { method: 'PUT', body: { id: tributacaoId.value, ...form } })
      toast.success('Regra de tributação salva com sucesso!')
    } else {
      const resposta = await useApi<{ dados?: { id?: string } }>('/fiscal/ncm-tributacoes', { method: 'POST', body: form })
      const novoId = resposta?.dados?.id
      toast.success('Regra de tributação criada com sucesso!')
      if (novoId) {
        router.push(`/erp/fiscal/ncm-tributacao/${novoId}`)
        return
      }
    }
    router.push('/erp/fiscal/ncm-tributacao')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/fiscal/ncm-tributacao')
}

// ---- Aba "NCMs da Regra" ----
const ncmConfiguracoes = ref<NcmConfiguracao[]>([])
const ncmBusca = ref('')
const ncmResultados = ref<NcmOpcao[]>([])
const ncmSelecionado = ref<NcmOpcao | null>(null)
const buscandoNcm = ref(false)
let ncmDebounceTimer: ReturnType<typeof setTimeout> | undefined

async function executarBuscaNcm(valor: string) {
  if (!valor || valor.trim().length < 2) {
    ncmResultados.value = []
    return
  }
  buscandoNcm.value = true
  try {
    const resposta = await useApi('/fiscal/ncms', { query: { localizar: valor.trim(), tamanhoPagina: 50 } })
    ncmResultados.value = extrairDados<NcmOpcao[]>(resposta) ?? []
  } catch (e) {
    console.error('[ncm-tributacao/[id]] busca ncm', e)
  } finally {
    buscandoNcm.value = false
  }
}

/** Debounce simples (500ms) para não disparar uma busca a cada tecla digitada. */
function buscarNcms(valor: string) {
  if (ncmDebounceTimer) clearTimeout(ncmDebounceTimer)
  ncmDebounceTimer = setTimeout(() => void executarBuscaNcm(valor), 500)
}

function selecionarNcmResultado(item: NcmOpcao) {
  ncmSelecionado.value = item
  ncmBusca.value = `${item.codigoNcm} - ${item.descricao}`
  ncmResultados.value = []
}

async function adicionarNcm() {
  if (!ncmSelecionado.value) {
    toast.error('Selecione um NCM na busca.')
    return
  }
  if (ncmConfiguracoes.value.some((c) => c.ncmId === ncmSelecionado.value?.id)) {
    toast.error('NCM já está na lista.')
    return
  }
  if (isEdit.value && tributacaoId.value) {
    try {
      await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/configuracoes`, {
        method: 'POST',
        body: { ncmTributacaoId: tributacaoId.value, ncmId: ncmSelecionado.value.id }
      })
      toast.success('NCM adicionado à regra.')
      await carregarTributacao()
    } catch (e) {
      toast.error(obterMensagemErro(e))
    }
  } else {
    ncmConfiguracoes.value.push({ ncmId: ncmSelecionado.value.id, ncm: ncmSelecionado.value })
  }
  ncmSelecionado.value = null
  ncmBusca.value = ''
}

async function removerNcm(item: NcmConfiguracao, index: number) {
  if (isEdit.value && tributacaoId.value && item.id) {
    try {
      await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/configuracoes/${item.id}`, { method: 'DELETE' })
      toast.success('NCM removido da regra.')
      await carregarTributacao()
      return
    } catch (e) {
      toast.error(obterMensagemErro(e))
      return
    }
  }
  ncmConfiguracoes.value.splice(index, 1)
}

// ---- Aba "Substituição Tributária" (só disponível em edição — subrecurso do backend) ----
const linhasSt = ref<NcmTributacaoSt[]>([])
const formSt = reactive<NcmTributacaoSt>({
  uf: '',
  tipoCalculo: 0,
  valorAliquotaIcmsSt: 0,
  valorMva: 0,
  valorReducaoBcIcmsSt: 0,
  tipoReducaoIcmsSt: 0,
  valorUnitarioSt: 0,
  valorPercentualFcpSt: 0
})

async function adicionarSt() {
  if (!tributacaoId.value) return
  if (!formSt.uf) {
    toast.error('Informe a UF.')
    return
  }
  try {
    await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/sts`, {
      method: 'POST',
      body: { ncmTributacaoId: tributacaoId.value, ...formSt }
    })
    toast.success('Substituição tributária adicionada.')
    Object.assign(formSt, { uf: '', tipoCalculo: 0, valorAliquotaIcmsSt: 0, valorMva: 0, valorReducaoBcIcmsSt: 0, tipoReducaoIcmsSt: 0, valorUnitarioSt: 0, valorPercentualFcpSt: 0 })
    await carregarTributacao()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function removerSt(item: NcmTributacaoSt) {
  if (!tributacaoId.value || !item.id) return
  try {
    await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/sts/${item.id}`, { method: 'DELETE' })
    toast.success('Substituição tributária removida.')
    await carregarTributacao()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// ---- Aba "Fundo de Combate à Pobreza" (só disponível em edição — subrecurso do backend) ----
const linhasFcp = ref<NcmTributacaoFcp[]>([])
const formFcp = reactive<NcmTributacaoFcp>({ uf: '', valorPercentual: 0 })

async function adicionarFcp() {
  if (!tributacaoId.value) return
  if (!formFcp.uf || !formFcp.valorPercentual) {
    toast.error('Preencha UF e percentual.')
    return
  }
  try {
    await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/fundos-combate-pobreza`, {
      method: 'POST',
      body: { ncmTributacaoId: tributacaoId.value, ...formFcp }
    })
    toast.success('Fundo de Combate à Pobreza adicionado.')
    Object.assign(formFcp, { uf: '', valorPercentual: 0 })
    await carregarTributacao()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function removerFcp(item: NcmTributacaoFcp) {
  if (!tributacaoId.value || !item.id) return
  try {
    await useApi(`/fiscal/ncm-tributacoes/${tributacaoId.value}/fundos-combate-pobreza/${item.id}`, { method: 'DELETE' })
    toast.success('Fundo de Combate à Pobreza removido.')
    await carregarTributacao()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(async () => {
  await Promise.all([carregarCfops(), carregarCests(), carregarCstsIbsCbs()])
  await carregarTributacao()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar NCM Tributação' : 'Nova NCM Tributação'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <nav class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'geral' }" @click="abaAtiva = 'geral'">Geral</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'icms' }" @click="abaAtiva = 'icms'">ICMS</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'pis-cofins' }" @click="abaAtiva = 'pis-cofins'">PIS/COFINS</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'ipi' }" @click="abaAtiva = 'ipi'">IPI</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'ibs-cbs' }" @click="abaAtiva = 'ibs-cbs'">IBS/CBS</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'ncms' }" @click="abaAtiva = 'ncms'">NCMs da Regra</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'st' }" :disabled="!isEdit" @click="abaAtiva = 'st'">Substituição Tributária</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'fcp' }" :disabled="!isEdit" @click="abaAtiva = 'fcp'">Fundo Combate Pobreza</button>
      </nav>

      <form class="vertical-form mt-4" @submit.prevent="salvar">
        <!-- GERAL -->
        <div v-show="abaAtiva === 'geral'" class="form-tab-content">
          <div class="form-grid">
            <TextField v-model.number="form.codRegra" label="Código Regra" type="number" required :error="erros.codRegra" />
            <TextField v-model="form.descricao" label="Descrição da Tributação" required maxlength="200" :error="erros.descricao" />
            <SelectField v-model.number="form.cfopNotaConsumidor" label="CFOP NFC-e e NF-e Simplificada" :options="opcoesCfopNfce" />
            <SelectField v-model.number="form.cfopNotaFiscal" label="CFOP NF-e Saída (dentro do estado)" :options="opcoesCfopNfeSaidaInterna" />
            <SelectField v-model.number="form.cfopNotaFiscalInterestadual" label="CFOP NF-e Saída (fora do estado)" :options="opcoesCfopNfeSaidaInterestadual" />
            <TextField v-model="form.codigoBeneficioFiscalIcms" label="Código Benefício Fiscal ICMS" maxlength="10" />
          </div>
          <div class="form-grid form-grid-1">
            <TextField v-model="form.informacoesComplementares" label="Informações Complementares" maxlength="5000" />
            <TextField v-model="form.informacoesAdicionaisAoFisco" label="Informações Adicionais ao Fisco" maxlength="2000" />
          </div>
        </div>

        <!-- ICMS -->
        <div v-show="abaAtiva === 'icms'" class="form-tab-content">
          <div class="form-grid">
            <SelectField v-model.number="form.origem" label="Origem" required :options="opcoesOrigemMercadoria" :error="erros.origem" />
            <SelectField v-model.number="form.csosnNotaConsumidor" label="CSOSN NFC-e e NF-e Simplificada" :options="opcoesCsosn" />
            <SelectField v-model.number="form.cstIcmsNotaConsumidor" label="CST NFC-e e NF-e Simplificada" :options="opcoesCstIcms" />
            <SelectField v-model.number="form.csosnNotaFiscal" label="CSOSN NF-e" :options="opcoesCsosn" />
          </div>

          <h3 class="section-title">ICMS — Dentro do Estado</h3>
          <div class="form-grid">
            <SelectField v-model.number="form.cstIcmsNotaFiscalInterna" label="CST NF-e" :options="opcoesCstIcms" />
            <SelectField v-model.number="form.codigoValorFiscalIcmsInterna" label="Código Valor Fiscal" :options="opcoesCodigoValorFiscalIcms" />
            <PercentInput v-if="form.codigoValorFiscalIcmsInterna === 0" v-model="form.valorAliquotaIcmsInterna" label="Alíquota" />
            <SelectField v-model.number="form.tipoReducaoIcmsInterna" label="Tipo de Redução" :options="opcoesTipoReducao" />
            <template v-if="form.tipoReducaoIcmsInterna !== TIPO_REDUCAO_NAO_UTILIZA">
              <PercentInput v-model="form.valorPercentualReducacaoBcIcmsInterna" label="Percentual Redução" />
              <SelectField v-model.number="form.destinoReducaoIcmsInterna" label="Destino Redução" :options="opcoesDestinoReducao" />
            </template>
          </div>

          <h3 class="section-title">ICMS — Fora do Estado</h3>
          <div class="form-grid">
            <SelectField v-model.number="form.cstIcmsNotaFiscalInterstadual" label="CST NF-e Interestadual" :options="opcoesCstIcms" />
            <SelectField v-model.number="form.codigoValorFiscalcmsInterstadual" label="Código Valor Fiscal" :options="opcoesCodigoValorFiscalIcms" />
            <PercentInput v-if="form.codigoValorFiscalcmsInterstadual === 0" v-model="form.valorAliquotaIcmsInterstadual" label="Alíquota" />
            <SelectField v-model.number="form.tipoReducaoIcmsInterstadual" label="Tipo de Redução" :options="opcoesTipoReducao" />
            <template v-if="form.tipoReducaoIcmsInterstadual !== TIPO_REDUCAO_NAO_UTILIZA">
              <PercentInput v-model="form.valorPercentualReducacaoBcIcmsInterstadual" label="Percentual Redução" />
              <SelectField v-model.number="form.destinoReducaoIcmsInterstadual" label="Destino Redução" :options="opcoesDestinoReducao" />
            </template>
          </div>
        </div>

        <!-- PIS / COFINS -->
        <div v-show="abaAtiva === 'pis-cofins'" class="form-tab-content">
          <div class="form-grid">
            <SelectField v-model.number="form.cstPisCofinsEntrada" label="CST PIS/COFINS de Entrada" :options="opcoesCstPisCofins" />
          </div>

          <h3 class="section-title">PIS</h3>
          <div class="form-grid">
            <SelectField v-model.number="form.cstPis" label="CST PIS de Saída" :options="opcoesCstPisCofins" />
            <TextField v-model.number="form.valorUnitFixoPis" label="Valor Unit. Fixo PIS" type="number" />
            <PercentInput v-model="form.valorAliquotaPis" label="Alíquota PIS" />
          </div>

          <h3 class="section-title">COFINS</h3>
          <div class="form-grid">
            <SelectField v-model.number="form.cstCofins" label="CST COFINS de Saída" :options="opcoesCstPisCofins" />
            <TextField v-model.number="form.valorUnitFixoCofins" label="Valor Unit. Fixo COFINS" type="number" />
            <PercentInput v-model="form.valorAliquotaCofins" label="Alíquota COFINS" />
          </div>
        </div>

        <!-- IPI -->
        <div v-show="abaAtiva === 'ipi'" class="form-tab-content">
          <div class="form-grid">
            <SelectField v-model.number="form.cstIpiSaida" label="CST IPI Saída" :options="opcoesCstIpi" />
            <SelectField v-model.number="form.cstIpiEntrada" label="CST IPI Entrada" :options="opcoesCstIpi" />
            <TextField v-model="form.enquadramentoIpi" label="Enquadramento IPI" maxlength="10" />
            <PercentInput v-model="form.valorAliquotaIpi" label="Alíquota IPI" />
            <PercentInput v-model="form.valorPercentualReducacaoBcIpi" label="Percentual Redução IPI" />
            <SelectField v-model.number="form.tipoReducaoIpi" label="Tipo Redução IPI" :options="opcoesTipoReducao" />
            <SelectField v-model.number="form.destinoReducaoIpi" label="Destino Redução IPI" :options="opcoesDestinoReducao" />
            <label class="field-checkbox toggle-row"><input v-model="form.ipiEmbutido" type="checkbox" /> Embutir IPI na Base ICMS</label>
          </div>
        </div>

        <!-- IBS / CBS -->
        <div v-show="abaAtiva === 'ibs-cbs'" class="form-tab-content">
          <p class="hint-text">
            Classificação tributária da Reforma Tributária (IBS/CBS) para NF-e e NFC-e.
            Selecione o CST e, quando aplicável, o código cClassTrib correspondente.
          </p>
          <div class="form-grid">
            <SelectField v-model="form.cstIbsCbsNfe" label="CST IBS/CBS NF-e" :options="opcoesCstIbsCbs" />
            <TextField v-model="form.cClassTribNfe" label="cClassTrib NF-e" maxlength="20" />
            <SelectField v-model="form.cstIbsCbsNfce" label="CST IBS/CBS NFC-e" :options="opcoesCstIbsCbs" />
            <TextField v-model="form.cClassTribNfce" label="cClassTrib NFC-e" maxlength="20" />
          </div>
        </div>

        <!-- NCMs DA REGRA -->
        <div v-show="abaAtiva === 'ncms'" class="form-tab-content">
          <div class="ncm-busca">
            <TextField v-model="ncmBusca" label="NCM" placeholder="Digite o código ou descrição do NCM..." @update:model-value="(v) => buscarNcms(v as string)" />
            <button type="button" class="btn btn-primary btn-sm" @click="adicionarNcm">+ Adicionar</button>
          </div>
          <ul v-if="ncmResultados.length" class="ncm-resultados">
            <li v-for="item in ncmResultados" :key="item.id" @click="selecionarNcmResultado(item)">
              {{ item.codigoNcm }} - {{ item.descricao }}
            </li>
          </ul>
          <span v-if="buscandoNcm" class="hint-text">Buscando...</span>

          <table class="admin-table mt-3">
            <thead>
              <tr><th>Código</th><th>Descrição</th><th></th></tr>
            </thead>
            <tbody>
              <tr v-if="ncmConfiguracoes.length === 0">
                <td colspan="3" class="table-empty">Nenhum NCM vinculado a esta regra.</td>
              </tr>
              <tr v-for="(item, index) in ncmConfiguracoes" :key="item.id ?? index">
                <td>{{ item.ncm?.codigoNcm }}</td>
                <td>{{ item.ncm?.descricao }}</td>
                <td class="td-right">
                  <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerNcm(item, index)">Remover</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- SUBSTITUIÇÃO TRIBUTÁRIA -->
        <div v-show="abaAtiva === 'st'" class="form-tab-content">
          <p v-if="!isEdit" class="hint-text">Salve a regra primeiro para poder adicionar linhas de Substituição Tributária.</p>
          <template v-else>
            <div class="form-grid">
              <SelectField v-model="formSt.uf" label="UF" :options="ufsBrasil" />
              <SelectField v-model.number="formSt.tipoCalculo" label="Tipo Cálculo ST" :options="opcoesTipoCalculoSt" />
              <PercentInput v-model="formSt.valorAliquotaIcmsSt" label="Alíquota ST" />
              <PercentInput v-model="formSt.valorMva" label="% IVA (MVA)" />
              <SelectField v-model.number="formSt.tipoReducaoIcmsSt" label="Tipo de Redução" :options="opcoesTipoReducao" />
              <PercentInput v-model="formSt.valorReducaoBcIcmsSt" label="Percentual Redução ST" />
              <TextField v-model.number="formSt.valorUnitarioSt" label="Valor Unit. Fixado" type="number" />
              <PercentInput v-model="formSt.valorPercentualFcpSt" label="Percentual FCP ST" />
            </div>
            <button type="button" class="btn btn-secondary btn-sm mb-2" @click="adicionarSt">+ Adicionar Substituição Tributária</button>

            <table class="admin-table">
              <thead>
                <tr><th>UF</th><th>Alíquota ST</th><th>Valor Unit. Fixado</th><th>% IVA</th><th>% FCP ST</th><th></th></tr>
              </thead>
              <tbody>
                <tr v-if="linhasSt.length === 0">
                  <td colspan="6" class="table-empty">Nenhuma substituição tributária cadastrada.</td>
                </tr>
                <tr v-for="item in linhasSt" :key="item.id">
                  <td>{{ item.uf }}</td>
                  <td>{{ item.valorAliquotaIcmsSt }}%</td>
                  <td>{{ item.valorUnitarioSt }}</td>
                  <td>{{ item.valorMva }}%</td>
                  <td>{{ item.valorPercentualFcpSt }}%</td>
                  <td class="td-right">
                    <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerSt(item)">Remover</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </template>
        </div>

        <!-- FUNDO DE COMBATE À POBREZA -->
        <div v-show="abaAtiva === 'fcp'" class="form-tab-content">
          <p v-if="!isEdit" class="hint-text">Salve a regra primeiro para poder adicionar linhas de Fundo de Combate à Pobreza.</p>
          <template v-else>
            <div class="form-grid">
              <SelectField v-model="formFcp.uf" label="UF" :options="ufsBrasil" />
              <PercentInput v-model="formFcp.valorPercentual" label="Valor Percentual FCP" />
            </div>
            <button type="button" class="btn btn-secondary btn-sm mb-2" @click="adicionarFcp">+ Adicionar</button>

            <table class="admin-table">
              <thead>
                <tr><th>UF</th><th>Percentual</th><th></th></tr>
              </thead>
              <tbody>
                <tr v-if="linhasFcp.length === 0">
                  <td colspan="3" class="table-empty">Nenhum Fundo de Combate à Pobreza cadastrado.</td>
                </tr>
                <tr v-for="item in linhasFcp" :key="item.id">
                  <td>{{ item.uf }}</td>
                  <td>{{ item.valorPercentual }}%</td>
                  <td class="td-right">
                    <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerFcp(item)">Remover</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </template>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-tabs { display: flex; gap: 8px; flex-wrap: wrap; border-bottom: 1px solid var(--border-color); padding-bottom: 12px; }
.tab-link {
  background: none;
  border: none;
  color: var(--text-secondary);
  padding: 8px 14px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.2s ease;
  white-space: nowrap;
}
.tab-link:hover:not(:disabled), .tab-link.active { background: rgba(255, 255, 255, 0.03); color: var(--text-primary); }
.tab-link.active { box-shadow: 0 0 10px rgba(99, 102, 241, 0.15); border: 1px solid rgba(99, 102, 241, 0.2); }
.tab-link:disabled { opacity: 0.4; cursor: not-allowed; }
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
.field-checkbox { display: flex; align-items: center; gap: 8px; font-size: 13.5px; color: var(--text-secondary); }
.field-checkbox input { width: 16px; height: 16px; accent-color: var(--primary); }
.toggle-row { padding-top: 22px; }
.mt-3 { margin-top: 16px; }
.mt-4 { margin-top: 20px; }
.mb-2 { margin-bottom: 12px; }
.hint-text { color: var(--text-secondary); font-size: 13px; margin-bottom: 12px; }
.ncm-busca { display: flex; gap: 12px; align-items: flex-end; }
.ncm-busca > :first-child { flex: 1; }
.ncm-resultados {
  list-style: none;
  margin: 4px 0 0;
  padding: 0;
  border: 1px solid var(--border-color);
  border-radius: 6px;
  max-height: 180px;
  overflow-y: auto;
}
.ncm-resultados li { padding: 8px 12px; cursor: pointer; font-size: 13px; }
.ncm-resultados li:hover { background: rgba(255, 255, 255, 0.05); }
</style>
