<script setup lang="ts">
/**
 * Histórico de Reajustes de Preço de Produtos — ProdutosHistoricosReajustesController.
 *
 * Contrato (rota `api/v1/produtos-historicos-reajustes`):
 *   GET  /produtos-historicos-reajustes?localizar&dataAlteracao&ativo&pagina&tamanhoPagina
 *   POST /produtos-historicos-reajustes   (ProdutoId, Tipo, Fator, ValorFixo, ValorNovo, Motivo)
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi, extrairDados, extrairLista } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()

interface Reajuste {
  id: string
  produtoId?: string | null
  produtoDescricao?: string | null
  tipo?: number | null
  fator?: number | null
  valorFixo?: number | null
  valorNovo?: number | null
  motivo?: string | null
  dataAlteracao?: string | null
}

const TIPO_REAJUSTE: Record<number, string> = { 0: 'Venda', 1: 'Compra' }

const itens = ref<Reajuste[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const localizar = ref('')

const produtos = ref<{ id: string; descricao?: string }[]>([])
const modalAberto = ref(false)
const salvando = ref(false)
const form = reactive({ produtoId: '' as string, tipo: 0 as number, fator: 0, valorFixo: 0, valorNovo: 0, motivo: '' })

const colunas: DataTableColumn<Reajuste>[] = [
  { key: 'produtoDescricao', label: 'Produto', sortable: false },
  { key: 'tipo', label: 'Tipo', sortable: false, width: '110px', align: 'center' },
  { key: 'valorNovo', label: 'Valor novo', sortable: false, width: '150px', align: 'right' },
  { key: 'dataAlteracao', label: 'Data', sortable: false, width: '130px', align: 'center' },
  { key: 'motivo', label: 'Motivo', sortable: false }
]
const opcoesTipo: SelectOption[] = Object.entries(TIPO_REAJUSTE).map(([v, label]) => ({ label, value: Number(v) }))
const opcoesProduto = () => produtos.value.map((p) => ({ label: p.descricao ?? p.id, value: p.id } as SelectOption))

async function buscar() {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value, ativo: true }
    if (localizar.value.trim()) query.localizar = localizar.value.trim()
    const resp = await useApi('/produtos-historicos-reajustes', { query })
    const d = extrairDados<{ itens?: Reajuste[]; total?: number }>(resp)
    itens.value = Array.isArray(d) ? (d as Reajuste[]) : d?.itens ?? []
    total.value = (d && !Array.isArray(d) ? d.total : undefined) ?? itens.value.length
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []; total.value = 0
  } finally {
    carregando.value = false
  }
}

async function abrirModal() {
  modalAberto.value = true
  if (!produtos.value.length) {
    try {
      const resp = await useApi('/produtos', { query: { tamanhoPagina: 200 } })
      produtos.value = extrairLista<{ id: string; descricao?: string }>(resp)
    } catch { /* select fica vazio; usuário pode digitar id */ }
  }
}

async function registrar() {
  if (!form.produtoId) { toast.error('Selecione o produto.'); return }
  salvando.value = true
  try {
    await useApi('/produtos-historicos-reajustes', { method: 'POST', body: { ...form } })
    toast.success('Reajuste registrado.')
    modalAberto.value = false
    Object.assign(form, { produtoId: '', tipo: 0, fator: 0, valorFixo: 0, valorNovo: 0, motivo: '' })
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(buscar)
</script>

<template>
  <div>
    <PageToolbar title="Reajustes de Preço" subtitle="Histórico de reajustes de valor de venda/compra dos produtos" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirModal">+ Registrar reajuste</button>
      </template>
    </PageToolbar>

    <div class="filtros">
      <TextField v-model="localizar" label="Localizar produto" placeholder="Descrição do produto" @keyup.enter="pagina = 1; buscar()" />
      <button type="button" class="btn btn-ghost" @click="pagina = 1; buscar()">Filtrar</button>
    </div>

    <DataTable
      :items="itens" :columns="colunas" :total="total" :page="pagina" :page-size="tamanhoPagina" :loading="carregando"
      empty-text="Nenhum reajuste registrado."
      @update:page="pagina = $event; buscar()"
      @update:page-size="tamanhoPagina = $event; pagina = 1; buscar()"
    >
      <template #cell-produtoDescricao="{ row }">{{ row.produtoDescricao ?? (row.produtoId ? String(row.produtoId).slice(0, 8) : '—') }}</template>
      <template #cell-tipo="{ value }">{{ TIPO_REAJUSTE[Number(value)] ?? '—' }}</template>
      <template #cell-valorNovo="{ value }">{{ value != null ? formatarMoeda(Number(value)) : '—' }}</template>
      <template #cell-dataAlteracao="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-motivo="{ value }">{{ value ?? '—' }}</template>
    </DataTable>

    <div v-if="modalAberto" class="modal-overlay" @click.self="modalAberto = false">
      <div class="modal-card">
        <h3>Registrar reajuste</h3>
        <div class="grid">
          <SelectField v-model="form.produtoId" :options="opcoesProduto()" label="Produto" required placeholder="Selecione…" />
          <SelectField v-model="form.tipo" :options="opcoesTipo" label="Tipo" />
          <MoneyInput v-model="form.valorNovo" label="Valor novo" />
          <MoneyInput v-model="form.valorFixo" label="Valor fixo (opcional)" />
          <TextField v-model.number="form.fator" type="number" label="Fator (%)" />
          <TextField v-model="form.motivo" label="Motivo" />
        </div>
        <div class="modal-actions">
          <button type="button" class="btn btn-ghost" @click="modalAberto = false">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="registrar">{{ salvando ? 'Salvando…' : 'Registrar' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.filtros { display: flex; gap: 1rem; align-items: flex-end; margin-bottom: 1rem; max-width: 520px; }
.filtros > *:first-child { flex: 1; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; align-items: center; justify-content: center; z-index: 50; }
.modal-card { background: var(--surface, #fff); border-radius: 12px; padding: 1.5rem; width: min(640px, 92%); }
.modal-card h3 { margin: 0 0 1rem; }
.grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
@media (max-width: 700px) { .grid { grid-template-columns: 1fr; } }
.modal-actions { display: flex; justify-content: flex-end; gap: .75rem; margin-top: 1.25rem; }
</style>
