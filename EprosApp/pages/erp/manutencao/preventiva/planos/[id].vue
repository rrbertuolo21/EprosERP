<script setup lang="ts">
/**
 * Plano Preventivo (novo/detalhe) — Manutenção / Preventiva / Planos.
 * - novo: POST /manutencao/preventiva/planos
 * - edição: GET /{id} + ativar (POST /{id}/ativar)
 *   + periodicidades (POST /{id}/periodicidades) + kit de peças (POST /{id}/kit-pecas)
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
import { tipoPeriodicidadeOpcoes, rotuloStatusRegistro, numeroOuNulo } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface PlanoForm {
  codigo: string
  descricao: string
  responsavelId: string
  alvoTipo: string | null
  alvoId: string | null
  observacao: string | null
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
const periodicidades = ref<Record<string, unknown>[]>([])
const kitPecas = ref<Record<string, unknown>[]>([])

const alvoTipoOpcoes = [
  { value: 'Equipamento', label: 'Equipamento' },
  { value: 'Local', label: 'Local' },
  { value: 'Conjunto', label: 'Conjunto' }
]

const form = reactive<PlanoForm>({
  codigo: '',
  descricao: '',
  responsavelId: '',
  alvoTipo: null,
  alvoId: null,
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
    const resposta = await useApi(`/manutencao/preventiva/planos/${idParam}`)
    const dados = extrairDados<Record<string, unknown>>(resposta)
    if (dados) {
      Object.assign(form, {
        codigo: (dados.codigo as string) ?? '',
        descricao: (dados.descricao as string) ?? '',
        responsavelId: (dados.responsavelId as string) ?? '',
        alvoTipo: (dados.alvoTipo as string) ?? null,
        alvoId: (dados.alvoId as string) ?? null,
        observacao: (dados.observacao as string) ?? null
      })
      status.value = (dados.status as number) ?? null
      periodicidades.value = (dados.periodicidades as Record<string, unknown>[]) ?? []
      kitPecas.value = (dados.kitPecas as Record<string, unknown>[]) ?? []
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
    const resposta = await useApi('/manutencao/preventiva/planos', { method: 'POST', body: form })
    const criado = extrairDados<{ id?: string }>(resposta)
    toast.success('Plano salvo com sucesso!')
    if (criado?.id) router.push(`/erp/manutencao/preventiva/planos/${criado.id}`)
    else router.push('/erp/manutencao/preventiva/planos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/manutencao/preventiva/planos')
}

async function ativar() {
  executandoAcao.value = true
  try {
    await useApi(`/manutencao/preventiva/planos/${idParam}/ativar`, { method: 'POST', body: {} })
    toast.success('Plano ativado.')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    executandoAcao.value = false
  }
}

// ---- Periodicidade ----
const periodVisivel = ref(false)
const salvandoPeriod = ref(false)
const formPeriod = reactive({
  tipoPeriodicidade: 0, dataInicio: null as string | null, intervalo: null as number | null,
  unidadeIntervalo: '', contadorTipo: '', contadorBase: null as number | null,
  contadorProximo: null as number | null, tolerancia: '', proximaExecucao: null as string | null
})
function abrirPeriod() {
  Object.assign(formPeriod, { tipoPeriodicidade: 0, dataInicio: null, intervalo: null, unidadeIntervalo: '', contadorTipo: '', contadorBase: null, contadorProximo: null, tolerancia: '', proximaExecucao: null })
  periodVisivel.value = true
}
async function salvarPeriod() {
  salvandoPeriod.value = true
  try {
    await useApi(`/manutencao/preventiva/planos/${idParam}/periodicidades`, {
      method: 'POST',
      body: {
        planoId: idParam, tipoPeriodicidade: formPeriod.tipoPeriodicidade, dataInicio: formPeriod.dataInicio,
        intervalo: numeroOuNulo(formPeriod.intervalo), unidadeIntervalo: formPeriod.unidadeIntervalo || null,
        contadorTipo: formPeriod.contadorTipo || null, contadorBase: numeroOuNulo(formPeriod.contadorBase),
        contadorProximo: numeroOuNulo(formPeriod.contadorProximo), tolerancia: formPeriod.tolerancia || null,
        proximaExecucao: formPeriod.proximaExecucao
      }
    })
    toast.success('Periodicidade adicionada.')
    periodVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoPeriod.value = false
  }
}

// ---- Kit de peças ----
const kitVisivel = ref(false)
const salvandoKit = ref(false)
const formKit = reactive({ pecaId: '', quantidade: 1, unidade: '', obrigatoria: false, observacao: '' })
function abrirKit() {
  Object.assign(formKit, { pecaId: '', quantidade: 1, unidade: '', obrigatoria: false, observacao: '' })
  kitVisivel.value = true
}
async function salvarKit() {
  if (!formKit.pecaId) {
    toast.error('Informe a peça.')
    return
  }
  salvandoKit.value = true
  try {
    await useApi(`/manutencao/preventiva/planos/${idParam}/kit-pecas`, {
      method: 'POST',
      body: {
        planoId: idParam, pecaId: formKit.pecaId, quantidade: formKit.quantidade,
        unidade: formKit.unidade || null, obrigatoria: formKit.obrigatoria, observacao: formKit.observacao || null
      }
    })
    toast.success('Peça adicionada ao kit.')
    kitVisivel.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoKit.value = false
  }
}

onMounted(async () => {
  await carregar()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? `Plano ${form.codigo}` : 'Novo plano preventivo'" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Voltar</button>
        <button v-if="!isEdit" type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
        <button v-else type="button" class="btn btn-primary" :disabled="executandoAcao" @click="ativar">Ativar</button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <div v-if="isEdit" class="workflow-bar">
        <span class="badge badge-info">{{ rotuloStatusRegistro(status) }}</span>
      </div>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" maxlength="30" :disabled="isEdit" />
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" :error="erros.descricao" :disabled="isEdit" />
          <SelectField v-model="form.alvoTipo" label="Tipo de alvo" :options="alvoTipoOpcoes" :disabled="isEdit" />
          <!-- TODO: alvoId/responsavelId sem endpoint de listagem — texto até integração. -->
          <TextField v-model="form.alvoId" label="Alvo (ID)" placeholder="UUID" :disabled="isEdit" />
          <TextField v-model="form.responsavelId" label="Responsável (ID)" required placeholder="UUID" :error="erros.responsavelId" :disabled="isEdit" />
          <TextField v-model="form.observacao" label="Observação" maxlength="500" :disabled="isEdit" />
        </div>
      </form>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Periodicidades</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirPeriod">+ Adicionar</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Tipo</th><th>Intervalo</th><th>Unidade</th><th>Próxima execução</th></tr></thead>
          <tbody>
            <tr v-if="periodicidades.length === 0"><td colspan="4"><div class="table-empty">Nenhuma periodicidade.</div></td></tr>
            <tr v-for="(p, i) in periodicidades" :key="i">
              <td>{{ p.tipoPeriodicidade }}</td><td>{{ p.intervalo }}</td><td>{{ p.unidadeIntervalo }}</td><td>{{ p.proximaExecucao }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="isEdit" class="glass-panel form-panel mt-3">
      <div class="section-head"><h3>Kit de peças</h3><button type="button" class="btn btn-secondary btn-sm" @click="abrirKit">+ Adicionar</button></div>
      <div class="table-wrap">
        <table class="admin-table">
          <thead><tr><th>Peça</th><th>Quantidade</th><th>Unidade</th><th>Obrigatória</th></tr></thead>
          <tbody>
            <tr v-if="kitPecas.length === 0"><td colspan="4"><div class="table-empty">Nenhuma peça no kit.</div></td></tr>
            <tr v-for="(k, i) in kitPecas" :key="i">
              <td>{{ k.pecaId }}</td><td>{{ k.quantidade }}</td><td>{{ k.unidade }}</td><td>{{ k.obrigatoria ? 'Sim' : 'Não' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <AppDialog v-model="periodVisivel" title="Periodicidade" width="620px" persistent>
      <div class="dialog-grid">
        <SelectField v-model="formPeriod.tipoPeriodicidade" label="Tipo de periodicidade" :options="tipoPeriodicidadeOpcoes" :clearable="false" />
        <DateTimeField v-model="formPeriod.dataInicio" label="Data início" />
        <TextField v-model="formPeriod.intervalo" label="Intervalo" type="number" />
        <TextField v-model="formPeriod.unidadeIntervalo" label="Unidade do intervalo" />
        <TextField v-model="formPeriod.contadorTipo" label="Tipo de contador" />
        <TextField v-model="formPeriod.contadorBase" label="Contador base" type="number" />
        <TextField v-model="formPeriod.contadorProximo" label="Contador próximo" type="number" />
        <TextField v-model="formPeriod.tolerancia" label="Tolerância" />
        <DateTimeField v-model="formPeriod.proximaExecucao" label="Próxima execução" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoPeriod" @click="periodVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoPeriod" @click="salvarPeriod">Adicionar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="kitVisivel" title="Peça do kit" width="560px" persistent>
      <div class="dialog-grid">
        <!-- TODO: pecaId sem endpoint de listagem no módulo — texto até integração. -->
        <TextField v-model="formKit.pecaId" label="Peça (ID)" placeholder="UUID" required />
        <QuantityInput v-model="formKit.quantidade" label="Quantidade" :min="0" />
        <TextField v-model="formKit.unidade" label="Unidade" />
        <TextField v-model="formKit.observacao" label="Observação" />
        <label class="field toggle-row">
          <span class="field-label">Obrigatória</span>
          <input v-model="formKit.obrigatoria" type="checkbox" />
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoKit" @click="kitVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoKit" @click="salvarKit">Adicionar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.mt-3 { margin-top: 16px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 14px; }
.workflow-bar { display: flex; align-items: center; gap: 12px; margin-bottom: 18px; }
.section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.section-head h3 { font-size: 15px; margin: 0; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
