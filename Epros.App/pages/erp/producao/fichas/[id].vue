<script setup lang="ts">
/**
 * Ficha de Produção — criação (novo) e edição de configuração (id existente).
 *
 * 'novo' → POST /producao/fichas (dados da venda/pessoa + configuração inicial).
 * id existente → GET /producao/fichas/{id}; edição REAL da configuração via
 * PUT /producao/fichas/{id}/configuracao; ações iniciar/concluir.
 * (venda/pessoa/item e datas de entrada/saída não são editáveis após a criação — não
 * fazem parte do comando de configuração.)
 *
 * Campos com palpite: vendaId / itemVendaId / pessoaId são uuid sem endpoint de listagem
 * próprio no módulo → TextField (uuid). Logomarca é enum (SelectField).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { opcoesLogomarca, rotuloSituacaoFicha, rotuloLogomarca, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface FichaForm {
  vendaId: string
  itemVendaId: string
  pessoaId: string
  logomarca: number | null
  lateraisPorta: number | string | null
  apoioCabeca: number | string | null
  entrada: string | null
  saida: string | null
  transportadora: string | null
  anoModelo: string | null
  corCouro: string | null
  costura: string | null
  tipoAcento: string | null
  tipoEncosto: string | null
  abd: string | null
  abt: string | null
  observacao: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const acaoEmAndamento = ref(false)
const registro = ref<Record<string, unknown> | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const form = reactive<FichaForm>({
  vendaId: '',
  itemVendaId: '',
  pessoaId: '',
  logomarca: 1,
  lateraisPorta: 0,
  apoioCabeca: 0,
  entrada: null,
  saida: null,
  transportadora: null,
  anoModelo: null,
  corCouro: null,
  costura: null,
  tipoAcento: null,
  tipoEncosto: null,
  abd: null,
  abt: null,
  observacao: null
})
const erros = reactive<Record<string, string>>({})

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!isEdit.value) {
    if (!form.vendaId) erros.vendaId = 'Venda é obrigatória.'
    if (!form.itemVendaId) erros.itemVendaId = 'Item da venda é obrigatório.'
    if (!form.pessoaId) erros.pessoaId = 'Pessoa é obrigatória.'
  }
  return Object.keys(erros).length === 0
}

async function carregar() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/producao/fichas/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta) ?? null
    registro.value = dados
    if (dados) {
      form.logomarca = (dados.logomarca as number) ?? 1
      form.lateraisPorta = (dados.lateraisPorta as number) ?? 0
      form.apoioCabeca = (dados.apoioCabeca as number) ?? 0
      form.transportadora = (dados.transportadora as string) ?? null
      form.anoModelo = (dados.anoModelo as string) ?? null
      form.corCouro = (dados.corCouro as string) ?? null
      form.costura = (dados.costura as string) ?? null
      form.tipoAcento = (dados.tipoAcento as string) ?? null
      form.tipoEncosto = (dados.tipoEncosto as string) ?? null
      form.abd = (dados.abd as string) ?? null
      form.abt = (dados.abt as string) ?? null
      form.observacao = (dados.observacao as string) ?? null
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    if (isEdit.value) {
      await useApi(`/producao/fichas/${idParam}/configuracao`, {
        method: 'PUT',
        body: {
          id: idParam,
          logomarca: form.logomarca,
          lateraisPorta: Number(form.lateraisPorta) || 0,
          apoioCabeca: Number(form.apoioCabeca) || 0,
          transportadora: form.transportadora,
          anoModelo: form.anoModelo,
          corCouro: form.corCouro,
          costura: form.costura,
          tipoAcento: form.tipoAcento,
          tipoEncosto: form.tipoEncosto,
          abd: form.abd,
          abt: form.abt,
          observacao: form.observacao
        }
      })
      toast.success('Configuração salva com sucesso!')
      await carregar()
    } else {
      await useApi('/producao/fichas', {
        method: 'POST',
        body: {
          ...form,
          lateraisPorta: Number(form.lateraisPorta) || 0,
          apoioCabeca: Number(form.apoioCabeca) || 0
        }
      })
      toast.success('Ficha criada com sucesso!')
      router.push('/erp/producao/fichas')
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function executarAcao(chave: 'iniciar' | 'concluir') {
  const rotulo = chave === 'iniciar' ? 'Iniciar produção' : 'Concluir ficha'
  const ok = await confirmRef.value?.open(`${rotulo}?`, 'Confirmar a ação para esta ficha?', { textoConfirmar: rotulo })
  if (!ok) return
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/fichas/${idParam}/${chave}`, { method: 'POST' })
    toast.success('Ação executada com sucesso.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

function cancelar() { router.push('/erp/producao/fichas') }
const situacaoAtual = computed(() => registro.value?.situacao as number | string | undefined)
onMounted(carregar)
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Ficha de Produção' : 'Nova ficha'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <template v-if="isEdit">
          <button type="button" class="btn btn-sm btn-secondary" :disabled="acaoEmAndamento" @click="executarAcao('iniciar')">Iniciar produção</button>
          <button type="button" class="btn btn-sm btn-secondary" :disabled="acaoEmAndamento" @click="executarAcao('concluir')">Concluir</button>
        </template>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>{{ isEdit ? 'Salvar configuração' : 'Salvar' }}</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <!-- Cabeçalho somente-leitura (edição) -->
      <div v-if="isEdit && registro" class="detail-grid header-block">
        <div class="detail-item"><span class="detail-label">Situação</span><span class="badge" :class="classeBadgeStatus(rotuloSituacaoFicha(situacaoAtual))">{{ rotuloSituacaoFicha(situacaoAtual) }}</span></div>
        <div class="detail-item"><span class="detail-label">Venda (ID)</span><span>{{ registro.vendaId || '—' }}</span></div>
        <div class="detail-item"><span class="detail-label">Pessoa (ID)</span><span>{{ registro.pessoaId || '—' }}</span></div>
        <div class="detail-item"><span class="detail-label">Entrada</span><span>{{ formatarData(registro.entrada as string) }}</span></div>
        <div class="detail-item"><span class="detail-label">Saída</span><span>{{ formatarData(registro.saida as string) }}</span></div>
        <div class="detail-item"><span class="detail-label">Logomarca atual</span><span>{{ rotuloLogomarca(registro.logomarca as number) }}</span></div>
      </div>

      <form class="vertical-form" @submit.prevent="salvar">
        <!-- Dados da venda/pessoa: só na criação -->
        <template v-if="!isEdit">
          <h4 class="section-title">Dados da ordem</h4>
          <div class="form-grid">
            <TextField v-model="form.vendaId" label="Venda (ID)" required :error="erros.vendaId" hint="UUID da venda" />
            <TextField v-model="form.itemVendaId" label="Item da venda (ID)" required :error="erros.itemVendaId" hint="UUID do item" />
            <TextField v-model="form.pessoaId" label="Pessoa (ID)" required :error="erros.pessoaId" hint="UUID da pessoa" />
            <DateTimeField v-model="form.entrada" label="Entrada" mode="datetime" />
            <DateTimeField v-model="form.saida" label="Saída" mode="datetime" />
          </div>
        </template>

        <h4 class="section-title">Configuração</h4>
        <div class="form-grid">
          <SelectField v-model="form.logomarca" label="Logomarca" :options="opcoesLogomarca" :clearable="false" />
          <TextField v-model="form.lateraisPorta" label="Laterais de porta" type="number" />
          <TextField v-model="form.apoioCabeca" label="Apoio de cabeça" type="number" />
          <TextField v-model="form.transportadora" label="Transportadora" />
          <TextField v-model="form.anoModelo" label="Ano/Modelo" />
          <TextField v-model="form.corCouro" label="Cor do couro" />
          <TextField v-model="form.costura" label="Costura" />
          <TextField v-model="form.tipoAcento" label="Tipo de acento" />
          <TextField v-model="form.tipoEncosto" label="Tipo de encosto" />
          <TextField v-model="form.abd" label="ABD" />
          <TextField v-model="form.abt" label="ABT" />
          <TextField v-model="form.observacao" label="Observação" />
        </div>
      </form>
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 16px; }
.detail-item { display: flex; flex-direction: column; gap: 4px; }
.detail-label { font-size: 12px; color: var(--text-secondary); font-weight: 600; }
.header-block { padding-bottom: 16px; margin-bottom: 8px; border-bottom: 1px solid var(--border-color); }
.section-title { font-size: 13.5px; font-weight: 600; margin: 18px 0 4px; color: var(--text-primary); }
</style>
