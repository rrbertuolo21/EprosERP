<script setup lang="ts">
/**
 * Listagem de Lançamentos Contábeis — Contabilidade Geral / Lançamentos.
 *
 * Contrato:
 *   GET  /contabilidade-geral/lancamentos?periodoContabilId=&pagina=&tamanhoPagina=
 *   POST /contabilidade-geral/lancamentos/{id}/confirmar
 *   POST /contabilidade-geral/lancamentos/{id}/estornar
 *   POST /contabilidade-geral/lancamentos/{id}/cancelar
 * Não há GET/{id} nem PUT/DELETE: a edição não existe — o lançamento nasce e muda de
 * estado por ações. Criação em `lancamentos/novo.vue`.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import type { SelectOption } from '~/composables/useEnum'
import { estadoLancamentoLabel, estadoLancamentoClasse } from '~/components/contabilidade-contas/enums'

definePageMeta({ layout: 'default' })

interface LancamentoContabil {
  id: string
  numeroLancamento?: string | null
  data: string
  estado: number
  historico?: string | null
  periodoContabilId?: string | null
}

interface LancamentoFiltros {
  periodoContabilId?: string | null
}

interface PeriodoOpcao {
  id: string
  anoFiscal: number
}

const router = useRouter()
const toast = useToast()
const { formatarData } = useHelper()

const periodos = ref<PeriodoOpcao[]>([])
const opcoesPeriodo = ref<SelectOption[]>([])

const lista = useApiList<LancamentoContabil, LancamentoFiltros>('/contabilidade-geral/lancamentos', {
  filtrosIniciais: { periodoContabilId: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<LancamentoContabil>[] = [
  { key: 'numeroLancamento', label: 'Número', sortable: false, width: '150px' },
  { key: 'data', label: 'Data', sortable: false, width: '120px' },
  { key: 'historico', label: 'Histórico', sortable: false },
  { key: 'estado', label: 'Estado', sortable: false, align: 'center', width: '130px' }
]

const camposFiltro = ref<FilterField[]>([
  { key: 'periodoContabilId', label: 'Período contábil', type: 'select', options: [], grow: true }
])

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

async function carregarPeriodos() {
  try {
    const resposta = await useApi('/contabilidade-geral/periodos')
    periodos.value = extrairDados<PeriodoOpcao[]>(resposta) ?? []
    opcoesPeriodo.value = periodos.value.map((p) => ({ label: `Ano ${p.anoFiscal}`, value: p.id }))
    camposFiltro.value = [
      { key: 'periodoContabilId', label: 'Período contábil', type: 'select', options: opcoesPeriodo.value, grow: true }
    ]
  } catch (e) {
    console.error('[contabilidade/lancamentos] periodos', e)
  }
}

function novoLancamento() {
  router.push('/erp/contabilidade/lancamentos/novo')
}

async function executarAcao(item: LancamentoContabil, acao: 'confirmar' | 'estornar' | 'cancelar') {
  const textos: Record<typeof acao, { titulo: string; msg: string; danger: boolean }> = {
    confirmar: { titulo: 'Confirmar lançamento', msg: 'Deseja confirmar este lançamento contábil? Ele passará a compor os saldos.', danger: false },
    estornar: { titulo: 'Estornar lançamento', msg: 'Deseja estornar este lançamento confirmado? Um contra-lançamento será gerado.', danger: true },
    cancelar: { titulo: 'Cancelar lançamento', msg: 'Deseja cancelar este lançamento? Esta ação não pode ser desfeita.', danger: true }
  }
  const t = textos[acao]
  const ok = await confirmRef.value!.open(t.titulo, t.msg, { danger: t.danger, textoConfirmar: t.titulo.split(' ')[0] })
  if (!ok) return
  try {
    await useApi(`/contabilidade-geral/lancamentos/{id}/${acao}`, { method: 'POST', params: { id: item.id } })
    toast.success('Ação executada com sucesso.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(async () => {
  await carregarPeriodos()
  await lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Lançamentos Contábeis"
      subtitle="Lançamentos da contabilidade geral (partidas dobradas por período)"
      :loading="lista.carregando.value"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoLancamento">+ Novo lançamento</button>
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
      empty-text="Nenhum lançamento contábil encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-data="{ value }">
        <span>{{ value ? formatarData(String(value)) : '—' }}</span>
      </template>
      <template #cell-estado="{ value }">
        <span class="badge" :class="`badge-${estadoLancamentoClasse(Number(value))}`">
          {{ estadoLancamentoLabel(Number(value)) }}
        </span>
      </template>
      <template #actions="{ row }">
        <button
          v-if="row.estado === 0"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Confirmar"
          @click.stop="executarAcao(row, 'confirmar')"
        >Confirmar</button>
        <button
          v-if="row.estado === 1"
          type="button"
          class="btn btn-ghost btn-sm"
          title="Estornar"
          @click.stop="executarAcao(row, 'estornar')"
        >Estornar</button>
        <button
          v-if="row.estado === 0"
          type="button"
          class="btn btn-ghost btn-sm btn-danger-action"
          title="Cancelar"
          @click.stop="executarAcao(row, 'cancelar')"
        >Cancelar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
