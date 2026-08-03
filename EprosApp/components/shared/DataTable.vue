<script setup lang="ts" generic="T extends object">
/**
 * DataTable — tabela de listagem paginada server-side (substitui VDataTableServer).
 *
 * Contrato (genérico em T = tipo da linha):
 *   props:
 *     items: T[]
 *     columns: DataTableColumn<T>[]
 *     total: number                 (total de registros no servidor)
 *     page: number                  (v-model:page — página atual, 1-based)
 *     pageSize: number              (v-model:pageSize — itens por página)
 *     loading?: boolean
 *     rowKey?: keyof T | string      (chave única da linha; default 'id')
 *     sort?: { key: string; order: 'asc' | 'desc' } | null   (v-model:sort)
 *     emptyText?: string
 *   emits:
 *     'update:page': [value: number]
 *     'update:pageSize': [value: number]
 *     'update:sort': [value: { key: string; order: 'asc' | 'desc' } | null]
 *     'row-click': [row: T]
 *   slots:
 *     cell-<key>  -> conteúdo customizado da célula. Props do slot: { row: T, value: unknown }
 *     actions     -> coluna de ações por linha. Props do slot: { row: T }
 *     empty       -> estado vazio customizado
 *
 * Emparelha com useApiList: ligue :items/:total/:loading e v-model:page/pageSize/sort,
 * e no @update:* chame `lista.buscar(...)`.
 *
 * DataTableColumn<T>:
 *   { key: string; label: string; sortable?: boolean; align?: 'left'|'right'|'center'; width?: string; formatter?: (value, row) => string }
 */
import { computed } from 'vue'

export interface DataTableColumn<Row> {
  key: string
  label: string
  sortable?: boolean
  align?: 'left' | 'right' | 'center'
  width?: string
  formatter?: (value: unknown, row: Row) => string
}

type SortState = { key: string; order: 'asc' | 'desc' } | null

const props = withDefaults(
  defineProps<{
    items: T[]
    columns: DataTableColumn<T>[]
    total: number
    page: number
    pageSize: number
    loading?: boolean
    rowKey?: string
    sort?: SortState
    emptyText?: string
  }>(),
  { loading: false, rowKey: 'id', sort: null, emptyText: 'Nenhum registro encontrado.' }
)

const emit = defineEmits<{
  'update:page': [value: number]
  'update:pageSize': [value: number]
  'update:sort': [value: SortState]
  'row-click': [row: T]
}>()

const opcoesTamanho = [10, 20, 50, 100]

const totalPaginas = computed(() => (props.pageSize > 0 ? Math.max(1, Math.ceil(props.total / props.pageSize)) : 1))
const inicio = computed(() => (props.total === 0 ? 0 : (props.page - 1) * props.pageSize + 1))
const fim = computed(() => Math.min(props.page * props.pageSize, props.total))

function valorCelula(row: T, col: DataTableColumn<T>): string {
  const bruto = (row as Record<string, unknown>)[col.key]
  if (col.formatter) return col.formatter(bruto, row)
  return bruto == null ? '' : String(bruto)
}

function chaveLinha(row: T, index: number): string | number {
  const k = (row as Record<string, unknown>)[props.rowKey]
  return (k as string | number) ?? index
}

function alternarOrdenacao(col: DataTableColumn<T>) {
  if (!col.sortable) return
  const atual = props.sort
  let proximo: SortState
  if (!atual || atual.key !== col.key) proximo = { key: col.key, order: 'asc' }
  else if (atual.order === 'asc') proximo = { key: col.key, order: 'desc' }
  else proximo = null
  emit('update:sort', proximo)
}

function irPara(p: number) {
  if (p < 1 || p > totalPaginas.value || p === props.page) return
  emit('update:page', p)
}

function mudarTamanho(ev: Event) {
  const v = Number((ev.target as HTMLSelectElement).value)
  emit('update:pageSize', v)
  emit('update:page', 1)
}
</script>

<template>
  <div class="data-table glass-panel">
    <div class="table-wrap">
      <table class="admin-table">
        <thead>
          <tr>
            <th
              v-for="col in columns"
              :key="col.key"
              :class="{ sortable: col.sortable, 'td-right': col.align === 'right', 'td-center': col.align === 'center' }"
              :style="col.width ? { width: col.width } : undefined"
              @click="alternarOrdenacao(col)"
            >
              {{ col.label }}
              <span v-if="col.sortable && sort?.key === col.key" class="sort-arrow">
                {{ sort?.order === 'asc' ? '▲' : '▼' }}
              </span>
            </th>
            <th v-if="$slots.actions" class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td :colspan="columns.length + ($slots.actions ? 1 : 0)" class="table-loading">
              <span class="spinner"></span> Carregando...
            </td>
          </tr>
          <tr v-else-if="items.length === 0">
            <td :colspan="columns.length + ($slots.actions ? 1 : 0)">
              <slot name="empty">
                <div class="table-empty">{{ emptyText }}</div>
              </slot>
            </td>
          </tr>
          <tr
            v-for="(row, index) in items"
            v-else
            :key="chaveLinha(row, index)"
            @click="emit('row-click', row)"
          >
            <td
              v-for="col in columns"
              :key="col.key"
              :class="{ 'td-right': col.align === 'right', 'td-center': col.align === 'center' }"
            >
              <slot :name="`cell-${col.key}`" :row="row" :value="(row as Record<string, unknown>)[col.key]">
                {{ valorCelula(row, col) }}
              </slot>
            </td>
            <td v-if="$slots.actions" class="td-actions" @click.stop>
              <slot name="actions" :row="row" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <span class="pagination-info">
        {{ inicio }}–{{ fim }} de {{ total }}
      </span>
      <div class="pager">
        <label class="page-size">
          Por página:
          <select class="select select-inline" :value="pageSize" @change="mudarTamanho">
            <option v-for="n in opcoesTamanho" :key="n" :value="n">{{ n }}</option>
          </select>
        </label>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="page <= 1" @click="irPara(page - 1)">‹</button>
        <span class="page-atual">{{ page }} / {{ totalPaginas }}</span>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="page >= totalPaginas" @click="irPara(page + 1)">›</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.data-table { padding: 8px 12px 4px; }
.td-center { text-align: center; }
.page-size { display: inline-flex; align-items: center; gap: 6px; }
.select-inline { width: auto; padding: 4px 26px 4px 8px; font-size: 12px; }
.page-atual { min-width: 60px; text-align: center; }
</style>
