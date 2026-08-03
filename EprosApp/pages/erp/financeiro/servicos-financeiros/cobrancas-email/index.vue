<script setup lang="ts">
/**
 * Listagem de Cobranças por E-mail — Serviços Financeiros.
 * GET /servicos-financeiros/cobrancas-email, POST. Ação: transicionar (POST /{id}/transicionar).
 */
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Cobranca {
  id: string
  nome?: string | null
  valor?: number | null
  periodo?: string | null
  emails?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarMoeda } = useHelper()

const lista = useApiList<Cobranca>('/servicos-financeiros/cobrancas-email', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Cobranca>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'periodo', label: 'Período', sortable: false },
  { key: 'emails', label: 'E-mails', sortable: false },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/servicos-financeiros/cobrancas-email/novo')
}

// --- Transicionar
const transVisivel = ref(false)
const salvandoTrans = ref(false)
const alvo = ref<Cobranca | null>(null)
const trans = reactive<{ acao: string | null; comprovante: string | null }>({ acao: null, comprovante: null })

function abrirTransicao(item: Cobranca) {
  alvo.value = item
  trans.acao = null
  trans.comprovante = null
  transVisivel.value = true
}

async function confirmarTransicao() {
  if (!alvo.value || !trans.acao) {
    toast.error('Informe a ação de transição.')
    return
  }
  salvandoTrans.value = true
  try {
    await useApi('/servicos-financeiros/cobrancas-email/{id}/transicionar', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { id: alvo.value.id, acao: trans.acao, comprovante: trans.comprovante }
    })
    toast.success('Transição registrada.')
    transVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoTrans.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Cobranças por E-mail" subtitle="Régua de cobrança por e-mail" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova cobrança</button>
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
      empty-text="Nenhuma cobrança registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Transicionar" @click.stop="abrirTransicao(row)">Transicionar</button>
      </template>
    </DataTable>

    <AppDialog v-model="transVisivel" title="Transicionar cobrança" width="440px">
      <div class="form-grid-modal">
        <!-- acao: string livre (máquina de estados definida no backend). -->
        <TextField v-model="trans.acao" label="Ação" placeholder="Ex.: avancar, finalizar, validar" maxlength="40" />
        <TextField v-model="trans.comprovante" label="Comprovante (opcional)" maxlength="200" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="transVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoTrans" @click="confirmarTransicao">
          <span v-if="salvandoTrans" class="spinner"></span>
          <span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
</style>
