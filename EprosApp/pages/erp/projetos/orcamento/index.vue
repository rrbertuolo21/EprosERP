<script setup lang="ts">
/**
 * Orçamentos de projeto — PROJETOS / Planejamento e Orçamento.
 * O contrato lista por projeto (GET /projetos/orcamento/projeto/{projetoId}); por isso
 * a tela exige selecionar um projeto antes de listar. Criação leva o projeto selecionado.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { rotuloStatusWorkflow, fmtMoeda, carregarProjetosOpcoes, BILLING_TYPE_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Orcamento {
  id: string; projetoId?: string | null; budget?: number | null; billingType?: number | null
  billingRate?: number | null; estimatedHours?: number | null; costsEstimate?: number | null
  status?: number | null; versao?: number | null
}

const router = useRouter()
const toast = useToast()
const projetos = ref<SelectOption[]>([])
const projetoId = ref<string | null>(null)
const itens = ref<Orcamento[]>([])
const carregando = ref(false)

function rotuloBilling(v: unknown): string {
  const f = BILLING_TYPE_OPCOES.find((o) => String(o.value) === String(v))
  return f ? f.label : '—'
}

const colunas: DataTableColumn<Orcamento>[] = [
  { key: 'budget', label: 'Orçamento', align: 'right', width: '160px' },
  { key: 'billingType', label: 'Cobrança', width: '120px' },
  { key: 'billingRate', label: 'Taxa', align: 'right', width: '120px' },
  { key: 'estimatedHours', label: 'Horas est.', align: 'right', width: '120px' },
  { key: 'status', label: 'Status', align: 'center', width: '130px' }
]

async function listar() {
  if (!projetoId.value) { itens.value = []; return }
  carregando.value = true
  try {
    const r = await useApi(`/projetos/orcamento/projeto/${projetoId.value}`)
    const d = extrairLista<Orcamento>(r)
    itens.value = Array.isArray(d) ? d : []
  } catch (e) { toast.error(obterMensagemErro(e)); itens.value = [] } finally { carregando.value = false }
}

function novo() {
  router.push({ path: '/erp/projetos/orcamento/novo', query: projetoId.value ? { projetoId: projetoId.value } : {} })
}
function abrir(item: Orcamento) { router.push(`/erp/projetos/orcamento/${item.id}`) }

onMounted(async () => { projetos.value = await carregarProjetosOpcoes() })
</script>

<template>
  <div>
    <PageToolbar title="Orçamentos" subtitle="Planejamento e orçamento por projeto" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo orçamento</button>
      </template>
    </PageToolbar>

    <div class="glass-panel filtro-projeto">
      <SelectField
        v-model="projetoId"
        label="Projeto"
        :options="projetos"
        placeholder="Selecione um projeto..."
        @change="listar"
      />
    </div>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="itens.length"
      :page="1"
      :page-size="itens.length || 1"
      :loading="carregando"
      empty-text="Selecione um projeto para listar os orçamentos."
      @row-click="abrir"
    >
      <template #cell-budget="{ value }">{{ fmtMoeda(value) }}</template>
      <template #cell-billingType="{ value }">{{ rotuloBilling(value) }}</template>
      <template #cell-billingRate="{ value }">{{ value == null ? '—' : fmtMoeda(value) }}</template>
      <template #cell-estimatedHours="{ value }">{{ value == null ? '—' : Number(value).toLocaleString('pt-BR') }}</template>
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.filtro-projeto { padding: 16px 20px; margin-bottom: 12px; max-width: 420px; }
</style>
