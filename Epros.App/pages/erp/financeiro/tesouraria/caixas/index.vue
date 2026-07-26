<script setup lang="ts">
/**
 * Caixas Operacionais (Tesouraria).
 * GET /tesouraria/caixas · POST /caixas/abrir · POST /{id}/fechar.
 * Sem CRUD de edição: abertura e fechamento são operações (diálogos).
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Caixa {
  id: string
  localNome?: string | null
  usuarioNome?: string | null
  valorInicial?: number | null
  statusDescricao?: string | null
  abertura?: string | null
}

const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Caixa>('/tesouraria/caixas', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Caixa>[] = [
  { key: 'localNome', label: 'Local', sortable: false },
  { key: 'usuarioNome', label: 'Usuário', sortable: false },
  { key: 'valorInicial', label: 'Valor Inicial', sortable: false, align: 'right', formatter: (v) => (v != null ? formatarMoeda(v as number) : '') },
  { key: 'abertura', label: 'Abertura', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

// --- Abrir caixa
const abrirVisivel = ref(false)
const salvandoAbrir = ref(false)
const abrirForm = reactive<{ localId: string | null; usuarioId: string | null; valorInicial: number | null }>({
  localId: null,
  usuarioId: null,
  valorInicial: 0
})

async function confirmarAbrir() {
  if (abrirForm.valorInicial == null) {
    toast.error('Informe o valor inicial.')
    return
  }
  salvandoAbrir.value = true
  try {
    await useApi('/tesouraria/caixas/abrir', { method: 'POST', body: { ...abrirForm } })
    toast.success('Caixa aberto.')
    abrirVisivel.value = false
    abrirForm.localId = null
    abrirForm.usuarioId = null
    abrirForm.valorInicial = 0
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAbrir.value = false
  }
}

// --- Fechar caixa
const fecharVisivel = ref(false)
const salvandoFechar = ref(false)
const alvo = ref<Caixa | null>(null)
const fecharForm = reactive<{ valorFechamento: number | null; totalComprovantesCartao: number | null; totalCheques: number | null; observacao: string | null }>({
  valorFechamento: null,
  totalComprovantesCartao: null,
  totalCheques: null,
  observacao: null
})

function abrirFechar(item: Caixa) {
  alvo.value = item
  fecharForm.valorFechamento = null
  fecharForm.totalComprovantesCartao = null
  fecharForm.totalCheques = null
  fecharForm.observacao = null
  fecharVisivel.value = true
}

async function confirmarFechar() {
  if (!alvo.value) return
  salvandoFechar.value = true
  try {
    await useApi('/tesouraria/caixas/{id}/fechar', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { id: alvo.value.id, ...fecharForm }
    })
    toast.success('Caixa fechado.')
    fecharVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFechar.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Caixas Operacionais" subtitle="Abertura e fechamento de caixa" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirVisivel = true">Abrir caixa</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum caixa registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Fechar caixa" @click.stop="abrirFechar(row)">Fechar</button>
      </template>
    </DataTable>

    <AppDialog v-model="abrirVisivel" title="Abrir caixa" width="440px">
      <div class="form-grid-modal">
        <MoneyInput v-model="abrirForm.valorInicial" label="Valor inicial" />
        <!-- TODO: localId e usuarioId são UUID sem endpoint de listagem no digest. -->
        <TextField v-model="abrirForm.localId" label="ID do local (opcional)" hint="UUID" />
        <TextField v-model="abrirForm.usuarioId" label="ID do usuário (opcional)" hint="UUID" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="abrirVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAbrir" @click="confirmarAbrir">
          <span v-if="salvandoAbrir" class="spinner"></span>
          <span v-else>Abrir</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="fecharVisivel" title="Fechar caixa" width="440px">
      <div class="form-grid-modal">
        <MoneyInput v-model="fecharForm.valorFechamento" label="Valor de fechamento" />
        <MoneyInput v-model="fecharForm.totalComprovantesCartao" label="Total comprovantes cartão" />
        <MoneyInput v-model="fecharForm.totalCheques" label="Total cheques" />
        <TextField v-model="fecharForm.observacao" label="Observação" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fecharVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="salvandoFechar" @click="confirmarFechar">
          <span v-if="salvandoFechar" class="spinner"></span>
          <span v-else>Fechar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
</style>
