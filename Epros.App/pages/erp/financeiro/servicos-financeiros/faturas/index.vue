<script setup lang="ts">
/**
 * Listagem de Faturas de Cobrança — Serviços Financeiros.
 * GET /servicos-financeiros/faturas, POST. Ação: baixar (POST /{id}/baixar).
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
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { OPCOES_TIPO_FATURA } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Fatura {
  id: string
  sacadoNome?: string | null
  referencia?: string | null
  numeroDocumento?: string | null
  dataVencimento?: string | null
  valor?: number | null
  tipoFatura?: number | null
  statusDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Fatura>('/servicos-financeiros/faturas', { tamanhoPaginaInicial: 25 })

function tipoLabel(v: unknown): string {
  return OPCOES_TIPO_FATURA.find((o) => o.value === v)?.label ?? ''
}

const colunas: DataTableColumn<Fatura>[] = [
  { key: 'sacadoNome', label: 'Sacado', sortable: true },
  { key: 'numeroDocumento', label: 'Documento', sortable: false },
  { key: 'dataVencimento', label: 'Vencimento', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'tipoFatura', label: 'Tipo', sortable: false, formatter: (v) => tipoLabel(v) },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/servicos-financeiros/faturas/novo')
}

// --- Baixar
const baixaVisivel = ref(false)
const salvandoBaixa = ref(false)
const alvo = ref<Fatura | null>(null)
const baixa = reactive<{ dataBaixa: string | null; valorRecebido: number | null }>({ dataBaixa: null, valorRecebido: null })

function abrirBaixa(item: Fatura) {
  alvo.value = item
  baixa.dataBaixa = null
  baixa.valorRecebido = item.valor ?? null
  baixaVisivel.value = true
}

async function confirmarBaixa() {
  if (!alvo.value || !baixa.dataBaixa || baixa.valorRecebido == null) {
    toast.error('Informe a data da baixa e o valor recebido.')
    return
  }
  salvandoBaixa.value = true
  try {
    await useApi('/servicos-financeiros/faturas/{id}/baixar', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { id: alvo.value.id, dataBaixa: baixa.dataBaixa, valorRecebido: baixa.valorRecebido }
    })
    toast.success('Fatura baixada.')
    baixaVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoBaixa.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Faturas de Cobrança" subtitle="Faturas de serviços financeiros" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova fatura</button>
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
      empty-text="Nenhuma fatura registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Baixar" @click.stop="abrirBaixa(row)">Baixar</button>
      </template>
    </DataTable>

    <AppDialog v-model="baixaVisivel" title="Baixar fatura" width="420px">
      <div class="form-grid-modal">
        <DateTimeField v-model="baixa.dataBaixa" label="Data da baixa" mode="datetime" />
        <MoneyInput v-model="baixa.valorRecebido" label="Valor recebido" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="baixaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoBaixa" @click="confirmarBaixa">
          <span v-if="salvandoBaixa" class="spinner"></span>
          <span v-else>Baixar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid-modal { display: grid; grid-template-columns: 1fr; gap: 14px; }
</style>
