<script setup lang="ts">
/**
 * Monitoramento Preditivo (novo/detalhe) — Manutenção / Preditiva / Monitoramentos.
 * - novo: POST /manutencao/preditiva/monitoramentos
 * - edição: GET /{id} + fluxo (submeter/aprovar/suspender/encerrar)
 *   + pontos de medição (POST /{id}/pontos-medicao)
 *   + por ponto: leituras (POST /pontos-medicao/{pontoId}/leituras) e regras (.../regras)
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
import type { SelectOption } from '~/composables/useEnum'
import {
  tipoRegraMonitoramentoOpcoes,
  rotuloStatusRegistro,
  carregarEquipamentoOpcoes,
  numeroOuNulo
} from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface MonitoramentoForm {
  codigo: string
  descricao: string
  responsavelId: string
  equipamentoId: string | null
  observacao: string | null
}

interface Ponto {
  id?: string
  codigoPonto?: string | null
  variavel?: string | null
  unidade?: string | null
  periodicidade?: string | null
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
const pontos = ref<Ponto[]>([])
const equipamentoOpcoes = ref<SelectOption[]>([])

const form = reactive<MonitoramentoForm>({
  codigo: '',
  descricao: '',
  responsavelId: '',
  equipamentoId: null,
  observacao: null
})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.descricao) erros.descricao = 'Descrição é obrigatória.'
  if (!form.responsavelId) erros.responsavelId = 'Responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/manutencao/preditiva/monitoramentos/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta)
    if (dados) {
      Object.assign(form, {
        codigo: (dados.codigo as string) ?? '',
        descricao: (dados.descricao as string) ?? '',
        responsavelId: (dados.responsavelId as string) ?? '',
        equipamentoId: (dados.equipamentoId as string) ?? null,
        observacao: (dados.observacao as string) ?? null
      })
      status.value = (dados.status as number) ?? null
      pontos.value = (dados.pontosMedicao as Ponto[]) ?? []
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
    const resposta = await useApi('/manutencao/preditiva/monitoramentos', { method: 'POST', body: form })
    const criado = extrairDados<{ id?: string }>(resposta)
    toast.success('Monitoramento salvo com sucesso!')
    if (criado?.id) router.push(`/erp/manutencao/preditiva/monitoramentos/${criado.id}`)
    else router.push('/erp/manutencao/preditiva/monitoramentos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/preditiva/monitoramentos')
}

async function executarAcao(acao: string, body?: Record<string, unknown>) {
  executandoAcao.value = true
  try {
    await useApi(`/manutencao/preditiva/monitoramentos/${idParam}/${acao}`, { method: 'POST', body: body ?? {} })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    executandoAcao.value = false
  }
}

const encerrarVisivel = ref(false)
const motivoEncerrar = ref('')
async function confirmarEncerrar() {
  await executarAcao('encerrar', { monitoramentoId: idParam, motivo: motivoEncerrar.value || null })
  encerrarVisivel.value = false
}

// ---- Ponto de medição ----
const pontoVisivel = ref(false)
const salvandoPonto = ref(false)
const formPonto = reactive({
  equipamentoId: '', codigoPonto: '', variavel: '', unidade: '', localTecnico: '', periodicidade: ''
})
function abrirPonto() {
  Object.assign(formPonto, { equipamentoId: form.equipamentoId ?? '', codigoPonto: '', variavel: '', unidade: '', localTecnico: '', periodicidade: '' })
  pontoVisivel.value = true
}
async function salvarPonto() {
  if (!formPonto.equipamentoId) {
    toast.error('Informe o equipamento do ponto.')
    return
  }
  salvandoPonto.value = true
  try {
    await useApi(`/manutencao/preditiva/monitoramentos/${idParam}/pontos-medicao`, {
      method: 'POST',
      body: {
        monitoramentoId: idParam, equipamentoId: formPonto.equipamentoId,
        codigoPonto: formPonto.codigoPonto || null, variavel: formPonto.variavel || null,
        unidade: formPonto.unidade || null, localTecnico: formPonto.localTecnico || null,
        periodicidade: formPonto.periodicidade || null
      }
    })
    toast.success('Ponto de medição adicionado.')
    pontoVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoPonto.value = false
  }
}

// ---- Leitura (por ponto) ----
const leituraVisivel = ref(false)
const salvandoLeitura = ref(false)
const pontoSelecionado = ref<Ponto | null>(null)
const formLeitura = reactive({
  dataHoraMedicao: null as string | null, valor: 0, unidade: '', qualidadeDado: null as number | null, origem: ''
})
function abrirLeitura(p: Ponto) {
  pontoSelecionado.value = p
  Object.assign(formLeitura, { dataHoraMedicao: null, valor: 0, unidade: p.unidade ?? '', qualidadeDado: null, origem: '' })
  leituraVisivel.value = true
}
async function salvarLeitura() {
  if (!pontoSelecionado.value?.id) return
  if (!formLeitura.dataHoraMedicao) {
    toast.error('Informe a data/hora da medição.')
    return
  }
  salvandoLeitura.value = true
  try {
    await useApi(`/manutencao/preditiva/pontos-medicao/${pontoSelecionado.value.id}/leituras`, {
      method: 'POST',
      body: {
        pontoMedicaoId: pontoSelecionado.value.id, dataHoraMedicao: formLeitura.dataHoraMedicao,
        valor: numeroOuNulo(formLeitura.valor) ?? 0, unidade: formLeitura.unidade || null,
        qualidadeDado: numeroOuNulo(formLeitura.qualidadeDado), origem: formLeitura.origem || null
      }
    })
    toast.success('Leitura registrada.')
    leituraVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoLeitura.value = false
  }
}

// ---- Regra (por ponto) ----
const regraVisivel = ref(false)
const salvandoRegra = ref(false)
const formRegra = reactive({
  tipoRegra: 0, operador: '', limiteMinimo: null as number | null, limiteMaximo: null as number | null,
  janelaAvaliacao: '', severidade: '', acaoEsperada: '', vigenciaInicio: null as string | null, vigenciaFim: null as string | null
})
function abrirRegra(p: Ponto) {
  pontoSelecionado.value = p
  Object.assign(formRegra, { tipoRegra: 0, operador: '', limiteMinimo: null, limiteMaximo: null, janelaAvaliacao: '', severidade: '', acaoEsperada: '', vigenciaInicio: null, vigenciaFim: null })
  regraVisivel.value = true
}
async function salvarRegra() {
  if (!pontoSelecionado.value?.id) return
  salvandoRegra.value = true
  try {
    await useApi(`/manutencao/preditiva/pontos-medicao/${pontoSelecionado.value.id}/regras`, {
      method: 'POST',
      body: {
        pontoMedicaoId: pontoSelecionado.value.id, tipoRegra: formRegra.tipoRegra,
        operador: formRegra.operador || null, limiteMinimo: numeroOuNulo(formRegra.limiteMinimo), limiteMaximo: numeroOuNulo(formRegra.limiteMaximo),
        janelaAvaliacao: formRegra.janelaAvaliacao || null, severidade: formRegra.severidade || null,
        acaoEsperada: formRegra.acaoEsperada || null, vigenciaInicio: formRegra.vigenciaInicio, vigenciaFim: formRegra.vigenciaFim
      }
    })
    toast.success('Regra adicionada.')
    regraVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoRegra.value = false
  }
}

const usarSelectEquip = computed(() => equipamentoOpcoes.value.length > 0)

onMounted(async () => {
  equipamentoOpcoes.value = await carregarEquipamentoOpcoes()
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? `Monitoramento ${form.codigo}` : 'Novo monitoramento preditivo'" :loading="carregando">
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
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="executarAcao('aprovar')">Aprovar</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="executarAcao('suspender')">Suspender</button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="executandoAcao" @click="encerrarVisivel = true">Encerrar</button>
        </div>
      </div>

      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" :disabled="isEdit" />
          <SelectField v-if="usarSelectEquip" v-model="form.equipamentoId" label="Equipamento" :options="equipamentoOpcoes" :disabled="isEdit" />
          <TextField v-else v-model="form.equipamentoId" label="Equipamento (ID)" placeholder="UUID" :disabled="isEdit" />
          <!-- TODO: responsavelId sem endpoint de listagem — texto até integração. -->
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID" :error="erros.responsavelId" :disabled="isEdit" />
          <TextField v-model="form.observacao" label="Observação" maxlength="500" :disabled="isEdit" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Pontos de medição</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirPonto">+ Adicionar ponto</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Código</th><th>Variável</th><th>Unidade</th><th>Periodicidade</th><th class="td-actions">Ações</th></tr></thead>
          <tbody>
            <tr v-if="pontos.length === 0"><td colspan="5"><div class="table-empty">Nenhum ponto de medição.</div></td></tr>
            <tr v-for="(p, i) in pontos" :key="p.id ?? i">
              <td>{{ p.codigoPonto }}</td><td>{{ p.variavel }}</td><td>{{ p.unidade }}</td><td>{{ p.periodicidade }}</td>
              <td class="td-actions">
                <button type="button" class="btn btn-ghost btn-sm" :disabled="!p.id" @click="abrirLeitura(p)">Leitura</button>
                <button type="button" class="btn btn-ghost btn-sm" :disabled="!p.id" @click="abrirRegra(p)">Regra</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AppDialog v-model="encerrarVisivel" title="Encerrar monitoramento" width="440px" persistent>
      <TextField v-model="motivoEncerrar" label="Motivo" placeholder="Descreva o motivo" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="executandoAcao" @click="encerrarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="executandoAcao" @click="confirmarEncerrar">Encerrar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="pontoVisivel" title="Ponto de medição" width="560px" persistent>
      <div class="dialog-grid">
        <SelectField v-if="usarSelectEquip" v-model="formPonto.equipamentoId" label="Equipamento" :options="equipamentoOpcoes" />
        <TextField v-else v-model="formPonto.equipamentoId" label="Equipamento (ID)" placeholder="UUID" />
        <TextField v-model="formPonto.codigoPonto" label="Código do ponto" />
        <TextField v-model="formPonto.variavel" label="Variável" />
        <TextField v-model="formPonto.unidade" label="Unidade" />
        <TextField v-model="formPonto.localTecnico" label="Local técnico" />
        <TextField v-model="formPonto.periodicidade" label="Periodicidade" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoPonto" @click="pontoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoPonto" @click="salvarPonto">Adicionar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="leituraVisivel" title="Registrar leitura" width="560px" persistent>
      <div class="dialog-grid">
        <DateTimeField v-model="formLeitura.dataHoraMedicao" label="Data/hora medição" mode="datetime" required />
        <TextField v-model="formLeitura.valor" label="Valor" type="number" />
        <TextField v-model="formLeitura.unidade" label="Unidade" />
        <TextField v-model="formLeitura.qualidadeDado" label="Qualidade do dado" type="number" />
        <TextField v-model="formLeitura.origem" label="Origem" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoLeitura" @click="leituraVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoLeitura" @click="salvarLeitura">Registrar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="regraVisivel" title="Regra do ponto" width="620px" persistent>
      <div class="dialog-grid">
        <SelectField v-model="formRegra.tipoRegra" label="Tipo de regra" :options="tipoRegraMonitoramentoOpcoes" :clearable="false" />
        <TextField v-model="formRegra.operador" label="Operador" />
        <TextField v-model="formRegra.limiteMinimo" label="Limite mínimo" type="number" />
        <TextField v-model="formRegra.limiteMaximo" label="Limite máximo" type="number" />
        <TextField v-model="formRegra.janelaAvaliacao" label="Janela de avaliação" />
        <TextField v-model="formRegra.severidade" label="Severidade" />
        <TextField v-model="formRegra.acaoEsperada" label="Ação esperada" />
        <DateTimeField v-model="formRegra.vigenciaInicio" label="Vigência início" mode="datetime" />
        <DateTimeField v-model="formRegra.vigenciaFim" label="Vigência fim" mode="datetime" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoRegra" @click="regraVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoRegra" @click="salvarRegra">Adicionar</button>
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
