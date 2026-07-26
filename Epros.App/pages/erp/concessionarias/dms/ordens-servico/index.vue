<script setup lang="ts">
/**
 * DMS / Ordens de serviço — lista + criação (GET lista + POST criar) e ação "Fechar"
 * (POST /dms/ordens-servico/{id}/fechar). Rota base: /dms/ordens-servico.
 */
import { ref } from 'vue'
import RecursoLista, { type CampoForm } from '~/components/concessionarias-shared/RecursoLista.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { type DataTableColumn } from '~/components/shared/DataTable.vue'
import { formatMoeda } from '~/components/concessionarias-shared/formatadores'
import { useApi } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'

definePageMeta({ layout: 'default' })

interface OrdemServicoDms {
  id: string
  numeroOs?: string | null
  veiculoChassi?: string | null
  valorPecas?: number
  valorMaoDeObra?: number
  reclamacaoGarantia?: boolean
}

const toast = useToast()
const listaRef = ref<InstanceType<typeof RecursoLista> | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog> | null>(null)

const colunas: DataTableColumn<OrdemServicoDms>[] = [
  { key: 'numeroOs', label: 'Nº OS' },
  { key: 'veiculoChassi', label: 'Chassi' },
  { key: 'valorPecas', label: 'Peças', align: 'right', formatter: formatMoeda },
  { key: 'valorMaoDeObra', label: 'Mão de obra', align: 'right', formatter: formatMoeda },
  { key: 'reclamacaoGarantia', label: 'Garantia', align: 'center' }
]

const campos: CampoForm[] = [
  { key: 'numeroOs', label: 'Número da OS', tipo: 'text' },
  { key: 'veiculoChassi', label: 'Chassi do veículo', tipo: 'text' },
  { key: 'descricaoInconveniente', label: 'Descrição do inconveniente', tipo: 'textarea' },
  { key: 'valorPecas', label: 'Valor de peças', tipo: 'money', obrigatorio: true },
  { key: 'valorMaoDeObra', label: 'Valor de mão de obra', tipo: 'money', obrigatorio: true },
  { key: 'reclamacaoGarantia', label: 'Reclamação de garantia', tipo: 'boolean' }
]

async function fechar(row: OrdemServicoDms) {
  const ok = await confirmRef.value?.open(
    'Fechar ordem de serviço',
    `Deseja fechar a OS ${row.numeroOs ?? row.id}? Esta ação encerra a ordem.`,
    { textoConfirmar: 'Fechar OS' }
  )
  if (!ok) return
  try {
    await useApi('/dms/ordens-servico/{id}/fechar', { method: 'POST', params: { id: row.id } })
    toast.success('Ordem de serviço fechada com sucesso.')
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
      title="Ordens de serviço (DMS)"
      subtitle="DMS — ordens de serviço e fechamento"
      path="/dms/ordens-servico"
      :columns="colunas"
      :campos="campos"
      rotulo-novo="+ Nova OS"
      tem-acoes
    >
      <template #cell-reclamacaoGarantia="{ value }">
        <span class="badge" :class="value ? 'badge-warning' : 'badge-neutral'">
          {{ value ? 'Sim' : 'Não' }}
        </span>
      </template>
      <template #acoes-linha="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Fechar OS" @click.stop="fechar(row as OrdemServicoDms)">
          Fechar
        </button>
      </template>
    </RecursoLista>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
