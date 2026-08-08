<script setup lang="ts">
/**
 * Tarefas — PROJETOS / Planejamento e Rastreamento.
 * Lista por projeto: GET /projetos/rastreamento/tarefas/projeto/{projetoId}.
 * Também expõe criação de estágios e dependências (endpoints só-POST) via diálogos.
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { fmtData, carregarProjetosOpcoes, TIPO_DEPENDENCIA_OPCOES } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Tarefa {
  id: string; titulo?: string | null; estado?: number | null; prioridade?: string | null
  dataInicio?: string | null; dataTermino?: string | null; percentualConcluido?: number | null
  indicadorMarco?: boolean; ordem?: number | null
}

const router = useRouter()
const toast = useToast()
const projetos = ref<SelectOption[]>([])
const projetoId = ref<string | null>(null)
const itens = ref<Tarefa[]>([])
const carregando = ref(false)

const ESTADO_TAREFA: Record<number, string> = {
  0: 'Planejada', 1: 'Em execução', 2: 'Bloqueada', 3: 'Concluída', 4: 'Adiada', 5: 'Cancelada', 6: 'Arquivada'
}
function rotuloEstado(v: unknown): string { return ESTADO_TAREFA[Number(v)] ?? '—' }

const colunas: DataTableColumn<Tarefa>[] = [
  { key: 'titulo', label: 'Título' },
  { key: 'estado', label: 'Estado', align: 'center', width: '130px' },
  { key: 'prioridade', label: 'Prioridade', width: '120px' },
  { key: 'dataInicio', label: 'Início', width: '110px' },
  { key: 'dataTermino', label: 'Término', width: '110px' },
  { key: 'percentualConcluido', label: 'Conclusão', align: 'right', width: '110px' }
]

async function listar() {
  if (!projetoId.value) { itens.value = []; return }
  carregando.value = true
  try {
    const r = await useApi(`/projetos/rastreamento/tarefas/projeto/${projetoId.value}`)
    const d = extrairLista<Tarefa>(r)
    itens.value = Array.isArray(d) ? d : []
  } catch (e) { toast.error(obterMensagemErro(e)); itens.value = [] } finally { carregando.value = false }
}

function novo() {
  router.push({ path: '/erp/projetos/tarefas/novo', query: projetoId.value ? { projetoId: projetoId.value } : {} })
}
function abrir(item: Tarefa) { router.push(`/erp/projetos/tarefas/${item.id}`) }

/* estágio */
const estagioDialog = ref(false)
const estagioForm = reactive({ nome: '', cor: '#6366f1', indicadorConclusao: false, ordem: 0 })
const acaoSalvando = ref(false)
async function criarEstagio() {
  acaoSalvando.value = true
  try {
    await useApi('/projetos/rastreamento/estagios', { method: 'POST', body: { ...estagioForm } })
    toast.success('Estágio criado.'); estagioDialog.value = false
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

/* dependência */
const depDialog = ref(false)
const depForm = reactive({ tarefaDependenteId: '', tarefaPredecessoraId: '', tipoDependencia: 0 as number | null, observacao: '' })
async function criarDependencia() {
  acaoSalvando.value = true
  try {
    await useApi('/projetos/rastreamento/dependencias', { method: 'POST', body: { ...depForm } })
    toast.success('Dependência criada.'); depDialog.value = false
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { acaoSalvando.value = false }
}

onMounted(async () => { projetos.value = await carregarProjetosOpcoes() })
</script>

<template>
  <div>
    <PageToolbar title="Tarefas" subtitle="Planejamento e rastreamento de tarefas" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="estagioDialog = true">Novo estágio</button>
        <button type="button" class="btn btn-ghost" @click="depDialog = true">Nova dependência</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova tarefa</button>
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
      empty-text="Selecione um projeto para listar as tarefas."
      @row-click="abrir"
    >
      <template #cell-estado="{ value }"><span class="badge badge-cancelada">{{ rotuloEstado(value) }}</span></template>
      <template #cell-dataInicio="{ value }">{{ fmtData(value) }}</template>
      <template #cell-dataTermino="{ value }">{{ fmtData(value) }}</template>
      <template #cell-percentualConcluido="{ value }">{{ Number(value ?? 0).toFixed(0) }}%</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>

    <!-- Estágio -->
    <AppDialog v-model="estagioDialog" title="Novo estágio" width="480px">
      <div class="form-grid">
        <TextField v-model="estagioForm.nome" label="Nome" required />
        <TextField v-model="estagioForm.cor" label="Cor" placeholder="#6366f1" />
        <QuantityInput v-model="estagioForm.ordem" label="Ordem" :decimais="0" />
        <label class="field toggle-row">
          <span class="field-label">Indica conclusão</span>
          <input v-model="estagioForm.indicadorConclusao" type="checkbox" />
        </label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="estagioDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="criarEstagio">
          <span v-if="acaoSalvando" class="spinner"></span><span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dependência -->
    <AppDialog v-model="depDialog" title="Nova dependência" width="520px">
      <div class="form-grid">
        <TextField v-model="depForm.tarefaDependenteId" label="Tarefa dependente (ID)" required hint="UUID" />
        <TextField v-model="depForm.tarefaPredecessoraId" label="Tarefa predecessora (ID)" required hint="UUID" />
        <SelectField v-model="depForm.tipoDependencia" label="Tipo de dependência" :options="TIPO_DEPENDENCIA_OPCOES" :clearable="false" />
        <TextField v-model="depForm.observacao" label="Observação" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="depDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoSalvando" @click="criarDependencia">
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
