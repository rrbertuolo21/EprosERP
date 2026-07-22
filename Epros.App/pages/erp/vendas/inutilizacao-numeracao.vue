<script setup lang="ts">
/**
 * Inutilização de Numeração (fatia 11) — `erp/vendas/inutilizacao-numeracao`.
 *
 * Porta a tela legada `vendas/inutilizacao-numeracao.vue` para o design novo: formulário de
 * inutilização de faixa (modelo + série + nº inicial/final + justificativa ≥ 15 caracteres) e
 * listagem das inutilizações já realizadas para a empresa ativa.
 *
 * Estado/IO em `useInutilizacao`; esta página é orquestração de UI.
 * Endpoint: `inutilizacao-dfe/inutilizar` (envio real). Listagem ainda sem rota no backend.
 */
import { onMounted, computed } from 'vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useInutilizacao, JUSTIFICATIVA_MIN } from '~/components/vendas-transmissoes/useInutilizacao'
import { useHelper } from '~/composables/useHelper'
import { MODELOS_FISCAIS, type InutilizacaoItem } from '~/components/vendas-transmissoes/transmissoesTypes'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const { formatarDataHora } = useHelper()

const {
  body,
  erros,
  itens,
  enviando,
  carregandoLista,
  carregarLista,
  inutilizar
} = useInutilizacao()

const opcoesModelo = computed(() => MODELOS_FISCAIS.map((m) => ({ label: m.label, value: m.value })))

const colunas: DataTableColumn<InutilizacaoItem>[] = [
  { key: 'ano', label: 'Ano', align: 'center' },
  { key: 'serie', label: 'Série', align: 'center' },
  { key: 'nrNfInicial', label: 'Nº inicial', align: 'right' },
  { key: 'nrNfFinal', label: 'Nº final', align: 'right' },
  { key: 'justificativa', label: 'Justificativa' },
  { key: 'protocolo', label: 'Protocolo' },
  { key: 'dataHora', label: 'Data/hora' }
]

async function onInutilizar() {
  await inutilizar()
}

// Recarrega a lista ao trocar o modelo do documento (série/numeração são por modelo).
async function onTrocarModelo() {
  await carregarLista()
}

onMounted(async () => {
  await carregarLista()
})
</script>

<template>
  <div class="inutilizacao-page">
    <PageToolbar title="Inutilização de Numeração" subtitle="Vendas · SEFAZ" :loading="enviando" />

    <section class="glass-panel card">
      <header class="card-header">
        <h2 class="card-title">Inutilizar faixa de numeração</h2>
      </header>

      <div class="grid">
        <SelectField
          v-model="body.modeloDocumento"
          label="Modelo do documento"
          :options="opcoesModelo"
          :clearable="false"
          required
          :error="erros.modeloDocumento"
          @change="onTrocarModelo"
        />
        <TextField
          v-model="body.serie"
          label="Série"
          required
          :error="erros.serie"
        />
        <TextField
          v-model="body.nrNfInicial"
          label="Número inicial"
          type="number"
          required
          :error="erros.nrNfInicial"
        />
        <TextField
          v-model="body.nrNfFinal"
          label="Número final"
          type="number"
          required
          :error="erros.nrNfFinal"
        />
      </div>

      <TextField
        v-model="body.justificativa"
        :label="`Justificativa (mínimo ${JUSTIFICATIVA_MIN} caracteres)`"
        :maxlength="255"
        required
        :error="erros.justificativa"
        :hint="`${(body.justificativa || '').length} caracteres`"
      />

      <div class="acoes">
        <button
          type="button"
          class="btn btn-primary"
          :disabled="enviando"
          @click="onInutilizar"
        >
          <span v-if="enviando" class="spinner"></span>
          <span v-else>Inutilizar numeração</span>
        </button>
      </div>
    </section>

    <section class="glass-panel card">
      <header class="card-header">
        <h2 class="card-title">Inutilizações realizadas</h2>
      </header>

      <DataTable
        :items="itens"
        :columns="colunas"
        :total="itens.length"
        :page="1"
        :page-size="itens.length || 1"
        :loading="carregandoLista"
        row-key="id"
        empty-text="Nenhuma inutilização registrada."
      >
        <template #cell-justificativa="{ row }">
          <span class="justificativa">{{ (row as InutilizacaoItem).justificativa ?? '—' }}</span>
        </template>
        <template #cell-protocolo="{ row }">
          {{ (row as InutilizacaoItem).protocolo ?? '—' }}
        </template>
        <template #cell-dataHora="{ row }">
          {{ (row as InutilizacaoItem).dataHora ? formatarDataHora((row as InutilizacaoItem).dataHora) : '—' }}
        </template>
      </DataTable>
    </section>
  </div>
</template>

<style scoped>
.inutilizacao-page { padding-bottom: 40px; }
.card { padding: 16px 18px; margin-bottom: 16px; }
.card-header { margin-bottom: 12px; }
.card-title { font-size: 15px; font-weight: 700; }
.grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  margin-bottom: 12px;
}
.acoes { margin-top: 16px; display: flex; justify-content: flex-end; }
.justificativa { font-size: 12px; }
@media (max-width: 900px) {
  .grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 520px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
