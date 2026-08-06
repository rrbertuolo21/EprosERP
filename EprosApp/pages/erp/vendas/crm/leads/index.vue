<script setup lang="ts">
/**
 * CRM Comercial — Leads (funil de captação).
 * Contrato real: base `/vendas/crm`.
 *   GET  leads?status=&localizar=&incluirArquivados=&pagina=&tamanhoPagina=
 *   POST leads                         (CriarCrmLeadCommand)
 *   POST leads/{id}/arquivar
 *   POST leads/{id}/converter          (ConverterCrmLeadCommand)
 * `CriadoPorUsuarioId` é injetado com o usuário logado. Apresentação — sem regra nova.
 */
import { onMounted, reactive, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useAuth } from '~/composables/useAuth'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS = [
  { value: 0, label: 'Novo' },
  { value: 1, label: 'Em qualificação' },
  { value: 2, label: 'Convertido' },
  { value: 3, label: 'Arquivado' }
]
function statusLabel(v: unknown) {
  return STATUS.find((s) => s.value === Number(v))?.label ?? ''
}

interface Lead {
  id: string
  nome?: string | null
  email?: string | null
  telefone?: string | null
  assunto?: string | null
  valorEstimado?: number | null
  status?: number | null
}
interface LeadFiltros {
  localizar?: string
  status?: number | null
  incluirArquivados?: boolean
}

const toast = useToast()
const { getUserId } = useAuth()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Lead, LeadFiltros>('/vendas/crm/leads', {
  filtrosIniciais: { localizar: '', status: null, incluirArquivados: false },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Lead>[] = [
  { key: 'nome', label: 'Nome' },
  { key: 'email', label: 'E-mail' },
  { key: 'telefone', label: 'Telefone' },
  { key: 'assunto', label: 'Assunto' },
  { key: 'valorEstimado', label: 'Valor estimado', align: 'right', formatter: formatMoeda },
  { key: 'status', label: 'Status', formatter: statusLabel }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Nome, e-mail...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: STATUS }
]

// ---- Criar lead
const dlgNovo = ref(false)
const salvando = ref(false)
const form = reactive({ nome: '', email: '', telefone: '', assunto: '', valorEstimado: 0 as number | null, notas: '' })
const erros = reactive<Record<string, string>>({})

function abrirNovo() {
  Object.assign(form, { nome: '', email: '', telefone: '', assunto: '', valorEstimado: 0, notas: '' })
  for (const k of Object.keys(erros)) delete erros[k]
  dlgNovo.value = true
}

async function salvar() {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.nome) erros.nome = 'Nome é obrigatório.'
  if (Object.keys(erros).length) return
  salvando.value = true
  try {
    await useApi('/vendas/crm/leads', {
      method: 'POST',
      body: {
        nome: form.nome,
        email: form.email || null,
        telefone: form.telefone || null,
        assunto: form.assunto || null,
        valorEstimado: form.valorEstimado ?? null,
        notas: form.notas || null,
        status: 0,
        criadoPorUsuarioId: getUserId()
      }
    })
    toast.success('Lead criado.')
    dlgNovo.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

async function arquivar(lead: Lead) {
  const ok = await confirmRef.value!.open('Arquivar lead', `Arquivar o lead "${lead.nome ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/crm/leads/{id}/arquivar', { method: 'POST', params: { id: lead.id } })
    toast.success('Lead arquivado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// ---- Converter lead
const dlgConverter = ref(false)
const convertendo = ref(false)
const leadConverter = ref<Lead | null>(null)
const conv = reactive({ criarOportunidade: true, nomeOportunidade: '', valorOportunidade: 0 as number | null })

function abrirConverter(lead: Lead) {
  leadConverter.value = lead
  conv.criarOportunidade = true
  conv.nomeOportunidade = lead.nome ?? ''
  conv.valorOportunidade = lead.valorEstimado ?? 0
  dlgConverter.value = true
}

async function confirmarConverter() {
  if (!leadConverter.value) return
  convertendo.value = true
  try {
    await useApi('/vendas/crm/leads/{id}/converter', {
      method: 'POST',
      params: { id: leadConverter.value.id },
      body: {
        leadId: leadConverter.value.id,
        criarOportunidade: conv.criarOportunidade,
        nomeOportunidade: conv.criarOportunidade ? conv.nomeOportunidade || null : null,
        valorOportunidade: conv.criarOportunidade ? conv.valorOportunidade ?? null : null
      }
    })
    toast.success('Lead convertido.')
    dlgConverter.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    convertendo.value = false
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Leads" subtitle="CRM Comercial — funil de captação" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Novo lead</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum lead encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirConverter(row)">Converter</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="arquivar(row)">Arquivar</button>
      </template>
    </DataTable>

    <AppDialog v-model="dlgNovo" title="Novo lead" width="640px" persistent>
      <div class="form-grid">
        <TextField v-model="form.nome" label="Nome" required :error="erros.nome" />
        <TextField v-model="form.email" label="E-mail" type="email" />
        <TextField v-model="form.telefone" label="Telefone" />
        <TextField v-model="form.assunto" label="Assunto" />
        <MoneyInput v-model="form.valorEstimado" label="Valor estimado" />
        <TextField v-model="form.notas" label="Notas" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dlgNovo = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="dlgConverter" title="Converter lead" width="520px" persistent>
      <div class="form-grid">
        <label class="field toggle-row">
          <span class="field-label">Criar oportunidade</span>
          <input v-model="conv.criarOportunidade" type="checkbox" />
        </label>
        <TextField v-if="conv.criarOportunidade" v-model="conv.nomeOportunidade" label="Nome da oportunidade" />
        <MoneyInput v-if="conv.criarOportunidade" v-model="conv.valorOportunidade" label="Valor da oportunidade" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="convertendo" @click="dlgConverter = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="convertendo" @click="confirmarConverter">
          <span v-if="convertendo" class="spinner"></span><span v-else>Converter</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
