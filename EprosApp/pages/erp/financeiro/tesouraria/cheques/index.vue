<script setup lang="ts">
/**
 * Listagem de Cheques (Tesouraria).
 * GET /tesouraria/cheques, POST. Ação: atualizar situação (POST /{id}/situacao).
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
import SelectField from '~/components/shared/fields/SelectField.vue'
import { OPCOES_SITUACAO_CHEQUE, OPCOES_TIPO_CHEQUE } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Cheque {
  id: string
  tipo?: number | null
  pessoaNome?: string | null
  emissao?: string | null
  vencimento?: string | null
  valor?: number | null
  situacao?: number | null
  situacaoDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Cheque>('/tesouraria/cheques', { tamanhoPaginaInicial: 25 })

function tipoLabel(v: unknown): string {
  return OPCOES_TIPO_CHEQUE.find((o) => o.value === v)?.label ?? ''
}
function situacaoLabel(row: Cheque): string {
  return row.situacaoDescricao ?? OPCOES_SITUACAO_CHEQUE.find((o) => o.value === row.situacao)?.label ?? ''
}

const colunas: DataTableColumn<Cheque>[] = [
  { key: 'tipo', label: 'Tipo', sortable: false, formatter: (v) => tipoLabel(v) },
  { key: 'pessoaNome', label: 'Pessoa', sortable: true },
  { key: 'emissao', label: 'Emissão', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'vencimento', label: 'Vencimento', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'valor', label: 'Valor', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'situacao', label: 'Situação', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/tesouraria/cheques/novo')
}

// --- Situação
const situacaoVisivel = ref(false)
const salvandoSituacao = ref(false)
const alvo = ref<Cheque | null>(null)
const novaSituacao = reactive<{ situacao: number | null }>({ situacao: null })

function abrirSituacao(item: Cheque) {
  alvo.value = item
  novaSituacao.situacao = item.situacao ?? 0
  situacaoVisivel.value = true
}

async function confirmarSituacao() {
  if (!alvo.value || novaSituacao.situacao == null) return
  salvandoSituacao.value = true
  try {
    await useApi('/tesouraria/cheques/{id}/situacao', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { id: alvo.value.id, situacao: novaSituacao.situacao }
    })
    toast.success('Situação atualizada.')
    situacaoVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoSituacao.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Cheques" subtitle="Cheques emitidos e recebidos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo cheque</button>
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
      empty-text="Nenhum cheque registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-situacao="{ row }">
        <span class="badge">{{ situacaoLabel(row) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Alterar situação" @click.stop="abrirSituacao(row)">Situação</button>
      </template>
    </DataTable>

    <AppDialog v-model="situacaoVisivel" title="Atualizar situação do cheque" width="420px">
      <SelectField v-model="novaSituacao.situacao" label="Situação" :options="OPCOES_SITUACAO_CHEQUE" :clearable="false" />
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="situacaoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoSituacao" @click="confirmarSituacao">
          <span v-if="salvandoSituacao" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>
