<script setup lang="ts">
/**
 * Garantias (RMA) — Coberturas aplicadas.
 * Contrato real: base `/vendas/garantias`.
 *   GET coberturas?vendaId=&produtoId=&clienteId=&numeroSerieLote=
 *   POST coberturas/{id}/registrar-uso (RegistrarUsoGarantiaCoberturaCommand: { id, usoAtual })
 * Consulta (lista não paginada) + registro de uso. Apresentação — sem regra nova.
 */
import { onMounted, reactive, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface Cobertura {
  id: string
  numeroSerieLote?: string | null
  situacao?: string | null
  dataOrigem?: string | null
  dataVencimento?: string | null
  usoOrigem?: number | null
  usoVencimento?: number | null
  unidadeUso?: number | null
}

const toast = useToast()
const lista = useApiList<Cobertura>('/vendas/garantias/coberturas', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Cobertura>[] = [
  { key: 'numeroSerieLote', label: 'Série / lote' },
  { key: 'situacao', label: 'Situação' },
  { key: 'dataOrigem', label: 'Origem', formatter: formatData },
  { key: 'dataVencimento', label: 'Vencimento', formatter: formatData },
  { key: 'usoVencimento', label: 'Uso limite', align: 'right' }
]

// Registrar uso
const dlg = ref(false)
const salvando = ref(false)
const alvo = ref<Cobertura | null>(null)
const form = reactive({ usoAtual: 0 as number | null })

function abrirUso(c: Cobertura) {
  alvo.value = c
  form.usoAtual = 0
  dlg.value = true
}
async function confirmar() {
  if (!alvo.value || form.usoAtual == null) return
  salvando.value = true
  try {
    await useApi('/vendas/garantias/coberturas/{id}/registrar-uso', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { id: alvo.value.id, usoAtual: form.usoAtual }
    })
    toast.success('Uso registrado.')
    dlg.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Garantias — coberturas" subtitle="Coberturas aplicadas e leitura de uso" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma cobertura encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirUso(row)">Registrar uso</button>
      </template>
    </DataTable>

    <AppDialog v-model="dlg" title="Registrar uso da cobertura" width="420px" persistent>
      <div class="form-grid">
        <QuantityInput v-model="form.usoAtual" label="Uso atual (km/horas)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dlg = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmar">
          <span v-if="salvando" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
