<script setup lang="ts">
/**
 * Alocações de recurso — PROJETOS / Gestão de Recursos.
 * Lista por projeto: GET /projetos/recursos/alocacoes/projeto/{projetoId}.
 * O contrato só expõe POST para criar (sem GET/{id} nem PUT) — criação via diálogo.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { carregarProjetosOpcoes, rotuloStatusWorkflow, fmtData } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Alocacao {
  id: string; recursoId?: string | null; projetoId?: string | null; tarefaId?: string | null
  papelNoProjeto?: string | null; dataInicio?: string | null; dataFim?: string | null
  cargaPlanejadaHoras?: number | null; status?: number | null
}

const toast = useToast()
const projetos = ref<SelectOption[]>([])
const projetoId = ref<string | null>(null)
const itens = ref<Alocacao[]>([])
const carregando = ref(false)

const colunas: DataTableColumn<Alocacao>[] = [
  { key: 'recursoId', label: 'Recurso (ID)' },
  { key: 'papelNoProjeto', label: 'Papel', width: '160px' },
  { key: 'dataInicio', label: 'Início', width: '120px' },
  { key: 'dataFim', label: 'Fim', width: '120px' },
  { key: 'cargaPlanejadaHoras', label: 'Carga (h)', align: 'right', width: '110px' },
  { key: 'status', label: 'Status', align: 'center', width: '120px' }
]

async function listar() {
  if (!projetoId.value) { itens.value = []; return }
  carregando.value = true
  try {
    const r = await useApi(`/projetos/recursos/alocacoes/projeto/${projetoId.value}`)
    const d = extrairDados<Alocacao[]>(r)
    itens.value = Array.isArray(d) ? d : []
  } catch (e) { toast.error(obterMensagemErro(e)); itens.value = [] } finally { carregando.value = false }
}

/* criar */
const dialog = ref(false)
const salvando = ref(false)
const form = reactive({
  recursoId: '', tarefaId: '', papelNoProjeto: '',
  dataInicio: null as string | null, dataFim: null as string | null, cargaPlanejadaHoras: 0
})
const erros = reactive<Record<string, string>>({})
function abrir() {
  form.recursoId = ''; form.tarefaId = ''; form.papelNoProjeto = ''
  form.dataInicio = null; form.dataFim = null; form.cargaPlanejadaHoras = 0
  for (const k of Object.keys(erros)) delete erros[k]
  dialog.value = true
}
async function salvar() {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!projetoId.value) { toast.error('Selecione um projeto primeiro.'); return }
  if (!form.recursoId) { erros.recursoId = 'Recurso é obrigatório.'; return }
  salvando.value = true
  try {
    await useApi('/projetos/recursos/alocacoes', {
      method: 'POST',
      body: {
        recursoId: form.recursoId, projetoId: projetoId.value, tarefaId: form.tarefaId || null,
        papelNoProjeto: form.papelNoProjeto || null, dataInicio: form.dataInicio, dataFim: form.dataFim,
        cargaPlanejadaHoras: form.cargaPlanejadaHoras
      }
    })
    toast.success('Alocação criada com sucesso!'); dialog.value = false; await listar()
  } catch (e) { toast.error(obterMensagemErro(e)) } finally { salvando.value = false }
}

onMounted(async () => { projetos.value = await carregarProjetosOpcoes() })
</script>

<template>
  <div>
    <PageToolbar title="Alocações de recurso" subtitle="Planejamento de capacidade por projeto" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" :disabled="!projetoId" @click="abrir">+ Nova alocação</button>
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
      empty-text="Selecione um projeto para listar as alocações."
    >
      <template #cell-papelNoProjeto="{ value }">{{ value || '—' }}</template>
      <template #cell-dataInicio="{ value }">{{ fmtData(value) }}</template>
      <template #cell-dataFim="{ value }">{{ fmtData(value) }}</template>
      <template #cell-cargaPlanejadaHoras="{ value }">{{ value == null ? '—' : Number(value).toLocaleString('pt-BR') }}</template>
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
    </DataTable>

    <AppDialog v-model="dialog" title="Nova alocação" width="560px">
      <div class="form-grid">
        <TextField v-model="form.recursoId" label="Recurso (ID)" required :error="erros.recursoId" hint="UUID do recurso" />
        <TextField v-model="form.tarefaId" label="Tarefa (ID)" hint="UUID (opcional)" />
        <TextField v-model="form.papelNoProjeto" label="Papel no projeto" />
        <QuantityInput v-model="form.cargaPlanejadaHoras" label="Carga planejada (h)" :decimais="2" />
        <DateTimeField v-model="form.dataInicio" label="Início" mode="datetime" />
        <DateTimeField v-model="form.dataFim" label="Fim" mode="datetime" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span><span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.filtro-projeto { padding: 16px 20px; margin-bottom: 12px; max-width: 420px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
