<script setup lang="ts">
/**
 * Treinamentos — RH / Treinamento.
 * Fonte: GET/POST /rh/treinamento/treinamentos + POST /rh/treinamento/feedbacks
 * e POST /rh/treinamento/tarefas/{id}/concluir.
 * A conclusão de tarefa exige o id da tarefa; como não há listagem de tarefas no digest,
 * é oferecida via diálogo pedindo o UUID da tarefa (ver relatório).
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Treinamento {
  id: string
  titulo?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  local?: string | null
  capacidadeMaxima?: number | null
}
interface Filtros { busca?: string }

const router = useRouter()
const toast = useToast()
const lista = useApiList<Treinamento, Filtros>('/rh/treinamento/treinamentos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Treinamento>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'dataInicio', label: 'Início', sortable: true, align: 'center' },
  { key: 'dataFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'local', label: 'Local', sortable: false },
  { key: 'capacidadeMaxima', label: 'Capacidade', sortable: false, align: 'right', width: '120px' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Título...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/treinamento/treinamentos/novo')
}

// --- Registrar feedback ---
const fbVisivel = ref(false)
const salvandoFb = ref(false)
const fb = ref<{
  tarefaId: string
  usuarioAlvoId: string
  nota: number | null
  comentarios: string | null
  criadoPorUsuarioId: string
  donoFuncionalId: string
}>({ tarefaId: '', usuarioAlvoId: '', nota: null, comentarios: null, criadoPorUsuarioId: '', donoFuncionalId: '' })

function abrirFeedback() {
  fb.value = { tarefaId: '', usuarioAlvoId: '', nota: null, comentarios: null, criadoPorUsuarioId: '', donoFuncionalId: '' }
  fbVisivel.value = true
}

async function salvarFeedback() {
  if (!fb.value.tarefaId || !fb.value.usuarioAlvoId || fb.value.nota == null) {
    toast.error('Informe tarefa, usuário-alvo e nota.')
    return
  }
  salvandoFb.value = true
  try {
    await useApi('/rh/treinamento/feedbacks', {
      method: 'POST',
      body: { ...fb.value, nota: Number(fb.value.nota) }
    })
    toast.success('Feedback registrado com sucesso.')
    fbVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFb.value = false
  }
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Treinamentos" subtitle="Ações de treinamento e desenvolvimento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirFeedback">Registrar feedback</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo treinamento</button>
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
      row-key="id"
      empty-text="Nenhum treinamento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
    </DataTable>

    <AppDialog v-model="fbVisivel" title="Registrar feedback de treinamento" width="480px" persistent>
      <div class="dlg-form">
        <!-- TODO: sem endpoint de listagem de tarefas/usuários no digest — UUID manual. -->
        <TextField v-model="fb.tarefaId" label="Tarefa (UUID)" required placeholder="UUID" />
        <TextField v-model="fb.usuarioAlvoId" label="Usuário-alvo (UUID)" required placeholder="UUID" />
        <TextField v-model="fb.nota" label="Nota" type="number" placeholder="0 a 10" />
        <TextField v-model="fb.criadoPorUsuarioId" label="Criado por (UUID)" placeholder="UUID" />
        <TextField v-model="fb.donoFuncionalId" label="Dono funcional (UUID)" placeholder="UUID" />
        <TextField v-model="fb.comentarios" label="Comentários" maxlength="300" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvandoFb" @click="fbVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoFb" @click="salvarFeedback">
          <span v-if="salvandoFb" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dlg-form { display: grid; gap: 14px; }
</style>
