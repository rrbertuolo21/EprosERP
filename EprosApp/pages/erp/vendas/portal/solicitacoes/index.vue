<script setup lang="ts">
/**
 * Portal do Cliente — Solicitações.
 * Contrato real: base `/vendas/portal`.
 *   GET solicitacoes · POST solicitacoes/{id}/responder (ResponderPortalSolicitacaoCommand).
 * Lista + resposta. Apresentação — sem regra nova.
 */
import { onMounted, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatDataHora } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Solicitacao {
  id: string
  assunto?: string | null
  status?: number | string | null
  abertaEm?: string | null
  respondidaEm?: string | null
}

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const lista = useApiList<Solicitacao>('/vendas/portal/solicitacoes', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Solicitacao>[] = [
  { key: 'assunto', label: 'Assunto' },
  { key: 'status', label: 'Status' },
  { key: 'abertaEm', label: 'Aberta em', formatter: formatDataHora },
  { key: 'respondidaEm', label: 'Respondida em', formatter: formatDataHora }
]

async function responder(s: Solicitacao) {
  const ok = await confirmRef.value!.open('Responder solicitação', `Marcar a solicitação "${s.assunto ?? ''}" como respondida?`)
  if (!ok) return
  try {
    await useApi('/vendas/portal/solicitacoes/{id}/responder', {
      method: 'POST',
      params: { id: s.id },
      body: { solicitacaoId: s.id }
    })
    toast.success('Solicitação respondida.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Portal do cliente — solicitações" subtitle="Atendimento das solicitações do portal" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma solicitação encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="responder(row)">Responder</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
