<script setup lang="ts">
/**
 * Formulário de Centro de Custo (novo/edição) — Contabilidade Gerencial.
 *
 * Contrato:
 *   GET  /contabilidade-gerencial/centros-custo/{id}
 *   POST /contabilidade-gerencial/centros-custo            (codigo, descricao, paiId?)
 *   PUT  /contabilidade-gerencial/centros-custo/{id}
 *   POST /contabilidade-gerencial/centros-custo/{id}/estado  (define ativo/inativo)
 *   GET  /contabilidade-gerencial/centros-custo/{id}/consulta (resumo de alocações)
 */
import { computed, reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { estadoCentroCustoLabel } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface CentroCustoForm {
  id?: string
  codigo: string
  descricao: string
  paiId: string | null
  estado: number
}

interface CentroOpcao { id: string; codigo?: string | null; descricao?: string | null }
interface ConsultaGerencial { centroCustoId: string; totalRateado: number; quantidadeAlocacoes: number }

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda } = useHelper()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const carregando = ref(false)
const salvando = ref(false)
const alterandoEstado = ref(false)
const centros = ref<CentroOpcao[]>([])
const consulta = ref<ConsultaGerencial | null>(null)

const form = reactive<CentroCustoForm>({
  id: isEdit.value ? idParam : undefined,
  codigo: '',
  descricao: '',
  paiId: null,
  estado: 0
})

const erros = reactive<Record<string, string>>({})

const opcoesPai = computed<SelectOption[]>(() =>
  centros.value
    .filter((c) => c.id !== form.id)
    .map((c) => ({ label: `${c.codigo ?? ''} — ${c.descricao ?? ''}`.trim(), value: c.id }))
)

function limparErros() {
  for (const k of Object.keys(erros)) delete erros[k]
}

function validar(): boolean {
  limparErros()
  if (!form.codigo?.trim()) erros.codigo = 'Código é obrigatório.'
  if (!form.descricao?.trim()) erros.descricao = 'Descrição é obrigatória.'
  return Object.keys(erros).length === 0
}

async function carregarCentros() {
  try {
    const resposta = await useApi('/contabilidade-gerencial/centros-custo', { query: { tamanhoPagina: 100 } })
    const dados = extrairDados<{ itens?: CentroOpcao[] } | CentroOpcao[]>(resposta)
    centros.value = Array.isArray(dados) ? dados : dados?.itens ?? []
  } catch (e) {
    console.error('[contabilidade/centros-custo/[id]] centros', e)
  }
}

async function carregarCentro() {
  if (!isEdit.value) return
  carregando.value = true
  try {
    const resposta = await useApi(`/contabilidade-gerencial/centros-custo/{id}`, { params: { id: idParam } })
    const dados = extrairDados<Partial<CentroCustoForm>>(resposta)
    if (dados) Object.assign(form, dados)
    await carregarConsulta()
  } catch (e) {
    toast.error(obterMensagemErro(e))
    await router.push('/erp/contabilidade/centros-custo')
  } finally {
    carregando.value = false
  }
}

async function carregarConsulta() {
  if (!isEdit.value) return
  try {
    const resposta = await useApi(`/contabilidade-gerencial/centros-custo/{id}/consulta`, { params: { id: idParam } })
    consulta.value = extrairDados<ConsultaGerencial>(resposta) ?? null
  } catch (e) {
    console.error('[contabilidade/centros-custo/[id]] consulta', e)
  }
}

async function alternarEstado() {
  alterandoEstado.value = true
  const novoEstado = form.estado === 0 ? 1 : 0
  try {
    await useApi(`/contabilidade-gerencial/centros-custo/{id}/estado`, {
      method: 'POST',
      params: { id: idParam },
      body: { id: idParam, estado: novoEstado }
    })
    form.estado = novoEstado
    toast.success(`Centro de custo ${novoEstado === 0 ? 'ativado' : 'inativado'}.`)
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    alterandoEstado.value = false
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
      await useApi(`/contabilidade-gerencial/centros-custo/{id}`, {
        method: 'PUT',
        params: { id: idParam },
        body: { id: idParam, codigo: form.codigo, descricao: form.descricao, paiId: form.paiId }
      })
      toast.success('Centro de custo atualizado com sucesso!')
    } else {
      await useApi('/contabilidade-gerencial/centros-custo', {
        method: 'POST',
        body: { codigo: form.codigo, descricao: form.descricao, paiId: form.paiId }
      })
      toast.success('Centro de custo criado com sucesso!')
    }
    await router.push('/erp/contabilidade/centros-custo')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/contabilidade/centros-custo')
}

onMounted(async () => {
  await carregarCentros()
  await carregarCentro()
})
</script>

<template>
  <div>
    <PageToolbar :title="isEdit ? 'Editar centro de custo' : 'Novo centro de custo'" :loading="carregando || salvando">
      <template #actions>
        <button
          v-if="isEdit"
          type="button"
          class="btn btn-secondary"
          :disabled="alterandoEstado"
          @click="alternarEstado"
        >
          <span v-if="alterandoEstado" class="spinner"></span>
          <span v-else>{{ form.estado === 0 ? 'Inativar' : 'Ativar' }}</span>
        </button>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div v-if="isEdit" class="glass-panel resumo-panel">
      <div class="resumo-item">
        <span class="resumo-label">Estado</span>
        <span class="badge" :class="form.estado === 0 ? 'badge-success' : 'badge-secondary'">{{ estadoCentroCustoLabel(form.estado) }}</span>
      </div>
      <div class="resumo-item">
        <span class="resumo-label">Total Rateado</span>
        <span class="resumo-valor">{{ formatarMoeda(consulta?.totalRateado ?? 0) }}</span>
      </div>
      <div class="resumo-item">
        <span class="resumo-label">Alocações</span>
        <span class="resumo-valor">{{ consulta?.quantidadeAlocacoes ?? 0 }}</span>
      </div>
    </div>

    <div class="glass-panel form-panel">
      <div class="form-grid">
        <TextField v-model="form.codigo" label="Código" required maxlength="30" :error="erros.codigo" />
        <TextField v-model="form.descricao" label="Descrição" required maxlength="150" :error="erros.descricao" />
        <SelectField v-model="form.paiId" label="Centro de Custo Pai" :options="opcoesPai" placeholder="Nenhum (raiz)" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.resumo-panel { display: flex; gap: 24px; padding: 14px 18px; margin-bottom: 16px; }
.resumo-item { display: flex; flex-direction: column; gap: 4px; }
.resumo-label { font-size: 11px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.4px; }
.resumo-valor { font-size: 16px; font-weight: 700; }
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}
</style>
