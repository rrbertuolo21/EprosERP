<script setup lang="ts">
/**
 * Riscos de projeto — PROJETOS / Gestão de Riscos.
 * Lista por projeto: GET /projetos/riscos/projeto/{projetoId}.
 * Estágios de risco têm GET/POST próprios (/projetos/riscos/estagios).
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarProjetosOpcoes, rotuloStatusWorkflow, rotuloPrioridadeRisco } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Risco {
  id: string; titulo?: string | null; prioridade?: number | null; status?: number | null
  estagioId?: string | null; probabilidade?: number | null; impacto?: number | null
}

const router = useRouter()
const toast = useToast()
const projetos = ref<SelectOption[]>([])
const projetoId = ref<string | null>(null)
const itens = ref<Risco[]>([])
const carregando = ref(false)

const colunas: DataTableColumn<Risco>[] = [
  { key: 'titulo', label: 'Título' },
  { key: 'prioridade', label: 'Prioridade', align: 'center', width: '120px' },
  { key: 'status', label: 'Status', align: 'center', width: '130px' },
  { key: 'probabilidade', label: 'Probab.', align: 'right', width: '100px' },
  { key: 'impacto', label: 'Impacto', align: 'right', width: '100px' }
]

async function listar() {
  if (!projetoId.value) { itens.value = []; return }
  carregando.value = true
  try {
    const r = await useApi(`/projetos/riscos/projeto/${projetoId.value}`)
    const d = extrairDados<Risco[]>(r)
    itens.value = Array.isArray(d) ? d : []
  } catch (e) { toast.error(obterMensagemErro(e)); itens.value = [] } finally { carregando.value = false }
}

function novo() {
  router.push({ path: '/erp/projetos/riscos/novo', query: projetoId.value ? { projetoId: projetoId.value } : {} })
}
function abrir(item: Risco) { router.push(`/erp/projetos/riscos/${item.id}`) }

/* estágio de risco */
const estagioDialog = ref(false)
const estagioForm = reactive({ nome: '', cor: '#6366f1', completo: false, ordem: 0, criadorId: '' })
const acaoSalvando = ref(false)
async function criarEstagio() {
  acaoSalvando.value = true
  try {
    await useApi('/projetos/riscos/estagios', {
      method: 'POST',
      body: { ...estagioForm, criadorId: estagioForm.criadorId || null }
    })
    toast.success('Estágio de risco criado.'); estagioDialog.value = false
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(async () => { projetos.value = await carregarProjetosOpcoes() })
</script>

<template>
  <div>
    <PageToolbar title="Riscos" subtitle="Gestão de riscos e issues de projeto" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="estagioDialog = true">Novo estágio</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo risco</button>
      </template>
    </PageToolbar>

    <div class="glass-panel filtro-projeto">
      <SelectField v-model="projetoId" label="Projeto" :options="projetos" placeholder="Selecione um projeto..." @change="listar" />
    </div>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="itens.length"
      :page="1"
      :page-size="itens.length || 1"
      :loading="carregando"
      empty-text="Selecione um projeto para listar os riscos."
      @row-click="abrir"
    >
      <template #cell-prioridade="{ value }"><span class="badge badge-pendente">{{ rotuloPrioridadeRisco(value) }}</span></template>
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #cell-probabilidade="{ value }">{{ value ?? '—' }}</template>
      <template #cell-impacto="{ value }">{{ value ?? '—' }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>

    <AppDialog v-model="estagioDialog" title="Novo estágio de risco" width="480px">
      <div class="form-grid">
        <TextField v-model="estagioForm.nome" label="Nome" required />
        <TextField v-model="estagioForm.cor" label="Cor" placeholder="#6366f1" />
        <QuantityInput v-model="estagioForm.ordem" label="Ordem" :decimais="0" />
        <TextField v-model="estagioForm.criadorId" label="Criador (ID)" hint="UUID (opcional)" />
        <label class="field toggle-row">
          <span class="field-label">Estágio de conclusão</span>
          <input v-model="estagioForm.completo" type="checkbox" />
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="estagioDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="criarEstagio">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.filtro-projeto { padding: 16px 20px; margin-bottom: 12px; max-width: 420px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle-row input { width: 18px; height: 18px; accent-color: var(--primary); }
</style>
