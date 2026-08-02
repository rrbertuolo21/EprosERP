<script setup lang="ts">
/**
 * Revisão de Confiabilidade (nova/detalhe) — Manutenção / Confiabilidade / Revisões.
 * - novo: POST /manutencao/confiabilidade/revisoes
 * - edição: GET /{id} + fluxo (submeter/aprovar/rejeitar/suspender/retomar/encerrar)
 *   + coleções (modos-falha, indicadores, recomendações).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import type { SelectOption } from '~/composables/useEnum'
import {
  tipoIndicadorConfiabilidadeOpcoes,
  calculadoPorOpcoes,
  estrategiaManutencaoOpcoes,
  rotuloStatusRegistro,
  carregarEquipamentoOpcoes,
  numeroOuNulo
} from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface RevisaoForm {
  codigo: string
  descricao: string
  responsavelId: string
  ativoId: string | null
  funcaoOperacional: string | null
  estadoConservacao: string | null
  criticidadeOperacional: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const executandoAcao = ref(false)
const erros = reactive<Record<string, string>>({})
const status = ref<number | null>(null)
const modosFalha = ref<Record<string, unknown>[]>([])
const indicadores = ref<Record<string, unknown>[]>([])
const recomendacoes = ref<Record<string, unknown>[]>([])
const equipamentoOpcoes = ref<SelectOption[]>([])

const form = reactive<RevisaoForm>({
  codigo: '',
  descricao: '',
  responsavelId: '',
  ativoId: null,
  funcaoOperacional: null,
  estadoConservacao: null,
  criticidadeOperacional: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/manutencao/confiabilidade/revisoes/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta)
    if (dados) {
      Object.assign(form, {
        codigo: (dados.codigo as string) ?? '',
        descricao: (dados.descricao as string) ?? '',
        responsavelId: (dados.responsavelId as string) ?? '',
        ativoId: (dados.ativoId as string) ?? null,
        funcaoOperacional: (dados.funcaoOperacional as string) ?? null,
        estadoConservacao: (dados.estadoConservacao as string) ?? null,
        criticidadeOperacional: (dados.criticidadeOperacional as string) ?? null
      })
      status.value = (dados.status as number) ?? null
      modosFalha.value = (dados.modosFalha as Record<string, unknown>[]) ?? []
      indicadores.value = (dados.indicadores as Record<string, unknown>[]) ?? []
      recomendacoes.value = (dados.recomendacoes as Record<string, unknown>[]) ?? []
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
    const resposta = await useApi('/manutencao/confiabilidade/revisoes', { method: 'POST', body: form })
    const criado = extrairDados<{ id?: string }>(resposta)
    toast.success('Revisão salva com sucesso!')
    if (criado?.id) router.push(`/erp/manutencao/confiabilidade/revisoes/${criado.id}`)
    else router.push('/erp/manutencao/confiabilidade/revisoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/confiabilidade/revisoes')
}

// ---- Ações de fluxo ----
async function executarAcao(acao: string, body?: Record<string, unknown>) {
  executandoAcao.value = true
  try {
    await useApi(`/manutencao/confiabilidade/revisoes/${idParam}/${acao}`, { method: 'POST', body: body ?? {} })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    executandoAcao.value = false
  }
}

// Diálogos de ação que exigem entrada
const motivoVisivel = ref(false)
const acaoMotivo = ref<'rejeitar' | 'suspender' | 'encerrar'>('rejeitar')
const motivo = ref('')
function abrirMotivo(acao: 'rejeitar' | 'suspender' | 'encerrar') {
  acaoMotivo.value = acao
  motivo.value = ''
  motivoVisivel.value = true
}
async function confirmarMotivo() {
  await executarAcao(acaoMotivo.value, { revisaoId: idParam, motivo: motivo.value || null })
  motivoVisivel.value = false
}

const aprovarVisivel = ref(false)
const aprovadorId = ref('')
function abrirAprovar() {
  aprovadorId.value = ''
  aprovarVisivel.value = true
}
async function confirmarAprovar() {
  if (!aprovadorId.value) {
    toast.error('Informe o aprovador.')
    return
  }
  await executarAcao('aprovar', { revisaoId: idParam, aprovadorId: aprovadorId.value })
  aprovarVisivel.value = false
}

// ---- Modo de falha ----
const modoVisivel = ref(false)
const salvandoModo = ref(false)
const formModo = reactive({
  sequencia: 1, componente: '', modoFalha: '', efeitoFalha: '', causaFalha: '', controleAtual: '',
  severidade: null as number | null, ocorrencia: null as number | null, deteccao: null as number | null,
  quantidade: null as number | null, observacao: ''
})
function abrirModo() {
  Object.assign(formModo, { sequencia: modosFalha.value.length + 1, componente: '', modoFalha: '', efeitoFalha: '', causaFalha: '', controleAtual: '', severidade: null, ocorrencia: null, deteccao: null, quantidade: null, observacao: '' })
  modoVisivel.value = true
}
async function salvarModo() {
  salvandoModo.value = true
  try {
    await useApi(`/manutencao/confiabilidade/revisoes/${idParam}/modos-falha`, {
      method: 'POST',
      body: {
        revisaoId: idParam, sequencia: numeroOuNulo(formModo.sequencia) ?? 1, componente: formModo.componente || null,
        modoFalha: formModo.modoFalha || null, efeitoFalha: formModo.efeitoFalha || null,
        causaFalha: formModo.causaFalha || null, controleAtual: formModo.controleAtual || null,
        severidade: numeroOuNulo(formModo.severidade), ocorrencia: numeroOuNulo(formModo.ocorrencia), deteccao: numeroOuNulo(formModo.deteccao),
        quantidade: numeroOuNulo(formModo.quantidade), observacao: formModo.observacao || null
      }
    })
    toast.success('Modo de falha adicionado.')
    modoVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoModo.value = false
  }
}

// ---- Indicador ----
const indicadorVisivel = ref(false)
const salvandoIndicador = ref(false)
const formIndicador = reactive({
  tipoIndicador: 0, periodoInicio: null as string | null, periodoFim: null as string | null,
  valor: 0, unidade: '', formulaAplicada: '', origemDados: '', calculadoPor: 0
})
function abrirIndicador() {
  Object.assign(formIndicador, { tipoIndicador: 0, periodoInicio: null, periodoFim: null, valor: 0, unidade: '', formulaAplicada: '', origemDados: '', calculadoPor: 0 })
  indicadorVisivel.value = true
}
async function salvarIndicador() {
  salvandoIndicador.value = true
  try {
    await useApi(`/manutencao/confiabilidade/revisoes/${idParam}/indicadores`, {
      method: 'POST',
      body: {
        revisaoId: idParam, tipoIndicador: formIndicador.tipoIndicador,
        periodoInicio: formIndicador.periodoInicio, periodoFim: formIndicador.periodoFim,
        valor: numeroOuNulo(formIndicador.valor) ?? 0, unidade: formIndicador.unidade || null,
        formulaAplicada: formIndicador.formulaAplicada || null, origemDados: formIndicador.origemDados || null,
        calculadoPor: formIndicador.calculadoPor
      }
    })
    toast.success('Indicador adicionado.')
    indicadorVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoIndicador.value = false
  }
}

// ---- Recomendação ----
const recomendacaoVisivel = ref(false)
const salvandoRec = ref(false)
const formRec = reactive({
  estrategia: 0, justificativa: '', rpnReferencia: null as number | null, mtbfReferencia: null as number | null,
  mttrReferencia: null as number | null, disponibilidadeReferencia: null as number | null, responsavelId: ''
})
function abrirRecomendacao() {
  Object.assign(formRec, { estrategia: 0, justificativa: '', rpnReferencia: null, mtbfReferencia: null, mttrReferencia: null, disponibilidadeReferencia: null, responsavelId: form.responsavelId })
  recomendacaoVisivel.value = true
}
async function salvarRecomendacao() {
  if (!formRec.responsavelId) {
    toast.error('Informe o responsável.')
    return
  }
  salvandoRec.value = true
  try {
    await useApi(`/manutencao/confiabilidade/revisoes/${idParam}/recomendacoes`, {
      method: 'POST',
      body: {
        revisaoId: idParam, estrategia: formRec.estrategia, justificativa: formRec.justificativa || null,
        rpnReferencia: numeroOuNulo(formRec.rpnReferencia), mtbfReferencia: numeroOuNulo(formRec.mtbfReferencia),
        mttrReferencia: numeroOuNulo(formRec.mttrReferencia), disponibilidadeReferencia: numeroOuNulo(formRec.disponibilidadeReferencia),
        responsavelId: formRec.responsavelId
      }
    })
    toast.success('Recomendação adicionada.')
    recomendacaoVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoRec.value = false
  }
}

const usarSelectAtivo = computed(() => equipamentoOpcoes.value.length > 0)

onMounted(async () => {
  equipamentoOpcoes.value = await carregarEquipamentoOpcoes()
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? `Revisão ${form.codigo}` : 'Nova revisão de confiabilidade'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit" class="workflow-bar">
        <span class="badge badge-info">{{ rotuloStatusRegistro(status) }}</span>
        <div class="workflow-actions">
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="executarAcao('submeter')">Submeter</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="abrirAprovar">Aprovar</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="abrirMotivo('rejeitar')">Rejeitar</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="abrirMotivo('suspender')">Suspender</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="executarAcao('retomar')">Retomar</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="abrirMotivo('encerrar')">Encerrar</button>
        </div>
      </div>

      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" :disabled="isEdit" />
          <SelectField v-if="usarSelectAtivo" v-model="form.ativoId" label="Ativo/Equipamento" :options="equipamentoOpcoes" :disabled="isEdit" />
          <TextField v-else v-model="form.ativoId" label="Ativo (ID)" placeholder="UUID" :disabled="isEdit" />
          <!-- TODO: responsavelId sem endpoint de listagem — texto até integração. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID" :error="erros.responsavelId" :disabled="isEdit" />
          <TextField v-model="form.funcaoOperacional" label="Função operacional" maxlength="120" :disabled="isEdit" />
          <TextField v-model="form.estadoConservacao" label="Estado de conservação" maxlength="60" :disabled="isEdit" />
          <TextField v-model="form.criticidadeOperacional" label="Criticidade operacional" maxlength="60" :disabled="isEdit" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Modos de falha (FMEA)</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirModo">+ Adicionar</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Seq.</th><th>Componente</th><th>Modo de falha</th><th>Sev.</th><th>Ocor.</th><th>Det.</th></tr></thead>
          <tbody>
            <tr v-if="modosFalha.length === 0"><td colspan="6"><div class="table-empty">Nenhum modo de falha.</div></td></tr>
            <tr v-for="(m, i) in modosFalha" :key="i">
              <td>{{ m.sequencia }}</td><td>{{ m.componente }}</td><td>{{ m.modoFalha }}</td>
              <td>{{ m.severidade }}</td><td>{{ m.ocorrencia }}</td><td>{{ m.deteccao }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Indicadores</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirIndicador">+ Adicionar</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Tipo</th><th>Valor</th><th>Unidade</th></tr></thead>
          <tbody>
            <tr v-if="indicadores.length === 0"><td colspan="3"><div class="table-empty">Nenhum indicador.</div></td></tr>
            <tr v-for="(ind, i) in indicadores" :key="i"><td>{{ ind.tipoIndicador }}</td><td>{{ ind.valor }}</td><td>{{ ind.unidade }}</td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Recomendações</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirRecomendacao">+ Adicionar</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Estratégia</th><th>Justificativa</th></tr></thead>
          <tbody>
            <tr v-if="recomendacoes.length === 0"><td colspan="2"><div class="table-empty">Nenhuma recomendação.</div></td></tr>
            <tr v-for="(r, i) in recomendacoes" :key="i"><td>{{ r.estrategia }}</td><td>{{ r.justificativa }}</td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Diálogos -->
    <AppDialog v-model="motivoVisivel" :title="`Confirmar ${acaoMotivo}`" width="440px" persistent>
      <TextField v-model="motivo" label="Motivo" placeholder="Descreva o motivo" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="executandoAcao" @click="motivoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="executandoAcao" @click="confirmarMotivo">Confirmar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="aprovarVisivel" title="Aprovar revisão" width="440px" persistent>
      <TextField v-model="aprovadorId" label="Aprovador (ID)" placeholder="UUID" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="executandoAcao" @click="aprovarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="executandoAcao" @click="confirmarAprovar">Aprovar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="modoVisivel" title="Modo de falha" width="620px" persistent>
      <div class="dialog-grid">
        <TextField v-model="formModo.sequencia" label="Sequência" type="number" />
        <TextField v-model="formModo.componente" label="Componente" />
        <TextField v-model="formModo.modoFalha" label="Modo de falha" />
        <TextField v-model="formModo.efeitoFalha" label="Efeito da falha" />
        <TextField v-model="formModo.causaFalha" label="Causa da falha" />
        <TextField v-model="formModo.controleAtual" label="Controle atual" />
        <TextField v-model="formModo.severidade" label="Severidade (1-10)" type="number" />
        <TextField v-model="formModo.ocorrencia" label="Ocorrência (1-10)" type="number" />
        <TextField v-model="formModo.deteccao" label="Detecção (1-10)" type="number" />
        <QuantityInput v-model="formModo.quantidade" label="Quantidade" :min="0" />
        <TextField v-model="formModo.observacao" label="Observação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoModo" @click="modoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoModo" @click="salvarModo">Adicionar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="indicadorVisivel" title="Indicador" width="620px" persistent>
      <div class="dialog-grid">
        <SelectField v-model="formIndicador.tipoIndicador" label="Tipo de indicador" :options="tipoIndicadorConfiabilidadeOpcoes" :clearable="false" />
        <SelectField v-model="formIndicador.calculadoPor" label="Calculado por" :options="calculadoPorOpcoes" :clearable="false" />
        <DateTimeField v-model="formIndicador.periodoInicio" label="Período início" mode="datetime" />
        <DateTimeField v-model="formIndicador.periodoFim" label="Período fim" mode="datetime" />
        <TextField v-model="formIndicador.valor" label="Valor" type="number" />
        <TextField v-model="formIndicador.unidade" label="Unidade" />
        <TextField v-model="formIndicador.formulaAplicada" label="Fórmula aplicada" />
        <TextField v-model="formIndicador.origemDados" label="Origem dos dados" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoIndicador" @click="indicadorVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoIndicador" @click="salvarIndicador">Adicionar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="recomendacaoVisivel" title="Recomendação" width="620px" persistent>
      <div class="dialog-grid">
        <SelectField v-model="formRec.estrategia" label="Estratégia" :options="estrategiaManutencaoOpcoes" :clearable="false" />
        <TextField v-model="formRec.responsavelId" label="Responsável (ID)" placeholder="UUID" required />
        <TextField v-model="formRec.justificativa" label="Justificativa" />
        <TextField v-model="formRec.rpnReferencia" label="RPN referência" type="number" />
        <TextField v-model="formRec.mtbfReferencia" label="MTBF referência" type="number" />
        <TextField v-model="formRec.mttrReferencia" label="MTTR referência" type="number" />
        <TextField v-model="formRec.disponibilidadeReferencia" label="Disponibilidade referência" type="number" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoRec" @click="recomendacaoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoRec" @click="salvarRecomendacao">Adicionar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.mt-3 { margin-top: 16px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 14px; }
.workflow-bar { display: flex; align-items: center; justify-content: space-between; gap: 12px; margin-bottom: 18px; flex-wrap: wrap; }
.workflow-actions { display: flex; gap: 8px; flex-wrap: wrap; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; margin: 0; }
</style>
