<script setup lang="ts">
/**
 * Relatório de Vendas Simplificado — exporta um Excel (.xlsx) com as vendas do período.
 *
 * Porta o comportamento de `relatorios/vendas/simplificado01.vue` do legado:
 * filtro por data inicial/final (padrão: mês corrente) + status da venda (enum),
 * disparando o download do relatório em Excel via `responseType: 'blob'`.
 *
 * Endpoint de leitura do arquivo: GET /relatorios/vendas/simplificado01
 * Endpoint de enum de status: GET /vendas-enums/venda-status-dfe
 */
import { ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useEnum, type Enum } from '~/composables/useEnum'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'default' })

interface FiltroRelatorio {
  dataVendaInicial: string | null
  dataVendaFinal: string | null
  status: number | null
}

const toast = useToast()
const { carregarEnum, paraOpcoes } = useEnum()
const { paraIsoData } = useHelper()

function primeiroDiaMes(): string {
  const hoje = new Date()
  return paraIsoData(new Date(hoje.getFullYear(), hoje.getMonth(), 1)) ?? ''
}

function ultimoDiaMes(): string {
  const hoje = new Date()
  return paraIsoData(new Date(hoje.getFullYear(), hoje.getMonth() + 1, 0)) ?? ''
}

const filtro = ref<FiltroRelatorio>({
  dataVendaInicial: primeiroDiaMes(),
  dataVendaFinal: ultimoDiaMes(),
  status: null
})

const statusEnum = ref<Enum[]>([])
const carregandoDownload = ref(false)

async function exportarExcel() {
  if (!filtro.value.dataVendaInicial || !filtro.value.dataVendaFinal) {
    toast.warning('Preencha as datas inicial e final.')
    return
  }

  carregandoDownload.value = true
  try {
    const blobResposta = await useApi<Blob>('/relatorios/vendas/simplificado01', {
      method: 'GET',
      query: {
        dataVendaInicial: filtro.value.dataVendaInicial,
        dataVendaFinal: filtro.value.dataVendaFinal,
        status: filtro.value.status
      },
      responseType: 'blob'
    })

    const blob = new Blob([blobResposta], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `vendas_${new Date().toISOString()}.xlsx`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
    toast.success('Relatório exportado com sucesso.')
  } catch (e) {
    console.error('[relatorios/vendas/simplificado01]', e)
    toast.error('Erro ao exportar para Excel.')
  } finally {
    carregandoDownload.value = false
  }
}

onMounted(async () => {
  await carregarEnum('/vendas-enums/venda-status-dfe', statusEnum)
})
</script>

<template>
  <div>
    <PageToolbar title="Relatório de Vendas Simplificado" subtitle="Exportação de vendas por período em Excel" />

    <div class="report-card glass-panel">
      <div class="form-grid">
        <DateTimeField v-model="filtro.dataVendaInicial" label="Data Inicial" required />
        <DateTimeField v-model="filtro.dataVendaFinal" label="Data Final" required />
        <SelectField
          v-model="filtro.status"
          label="Status"
          :options="paraOpcoes(statusEnum)"
          placeholder="Todos"
        />
      </div>

      <button
        type="button"
        class="btn btn-primary btn-export"
        :disabled="carregandoDownload"
        @click="exportarExcel"
      >
        <span v-if="carregandoDownload" class="spinner"></span>
        <span v-else>⬇ Exportar para Excel</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.report-card {
  max-width: 640px;
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 16px;
}
.btn-export {
  width: 100%;
  padding: 12px;
}
</style>
