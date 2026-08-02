<script setup lang="ts">
/**
 * Planejamento de Demanda — Previsões.
 * Contrato real: base `/vendas/demanda`.
 *   GET/POST previsoes · POST previsoes/{id}/aprovar · POST previsoes/{id}/publicar.
 * Lista + criação via RecursoLista; aprovar/publicar pelo slot de ações. Apresentação — sem regra nova.
 */
import { ref } from 'vue'
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { formatData } from '~/components/concessionarias-shared/formatadores'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Previsao {
  id: string
  codigo?: string | null
  nome?: string | null
  periodoInicio?: string | null
  periodoFim?: string | null
  status?: number | string | null
}

const toast = useToast()
const listaRef = ref<InstanceType<typeof RecursoLista>>()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const colunas: DataTableColumn<Previsao>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'nome', label: 'Previsão' },
  { key: 'periodoInicio', label: 'Início', formatter: formatData },
  { key: 'periodoFim', label: 'Fim', formatter: formatData },
  { key: 'status', label: 'Status' }
]

const campos: CampoForm[] = [
  { key: 'codigo', label: 'Código', tipo: 'text' },
  { key: 'nome', label: 'Nome', tipo: 'text', obrigatorio: true },
  { key: 'periodoInicio', label: 'Período — início', tipo: 'date', obrigatorio: true },
  { key: 'periodoFim', label: 'Período — fim', tipo: 'date', obrigatorio: true },
  { key: 'observacoes', label: 'Observações', tipo: 'textarea' }
]

async function aprovar(p: Previsao) {
  const ok = await confirmRef.value!.open('Aprovar previsão', `Aprovar a previsão "${p.nome ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/demanda/previsoes/{id}/aprovar', { method: 'POST', params: { id: p.id }, body: { previsaoId: p.id } })
    toast.success('Previsão aprovada.')
    await listaRef.value?.recarregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
async function publicar(p: Previsao) {
  const ok = await confirmRef.value!.open('Publicar previsão', `Publicar a previsão "${p.nome ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/demanda/previsoes/{id}/publicar', { method: 'POST', params: { id: p.id }, body: { previsaoId: p.id } })
    toast.success('Previsão publicada.')
    await listaRef.value?.recarregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}
</script>

<template>
  <div>
    <RecursoLista
      ref="listaRef"
      title="Planejamento de demanda"
      subtitle="Previsões de venda"
      path="/vendas/demanda/previsoes"
      :columns="colunas"
      :campos="campos"
      rotulo-novo="+ Nova previsão"
      :com-busca="false"
      :tem-acoes="true"
    >
      <template #acoes-linha="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="aprovar(row)">Aprovar</button>
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="publicar(row)">Publicar</button>
      </template>
    </RecursoLista>
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
