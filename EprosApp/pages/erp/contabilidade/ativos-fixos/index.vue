<script setup lang="ts">
/**
 * Listagem de Ativos Fixos / Imobilizado — Contabilidade.
 *
 * Contrato: GET /ativos-fixos (paginado, filtros status/grupoBemId).
 */
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useAtivoFixo, type AtivoFixo, type GrupoBem, STATUS_ATIVO_FIXO } from '~/composables/useAtivoFixo'
import { obterMensagemErro } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const router = useRouter()
const toast = useToast()
const { formatarMoeda, formatarData } = useHelper()
const api = useAtivoFixo()

const itens = ref<AtivoFixo[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const grupos = ref<GrupoBem[]>([])
const filtroStatus = ref<number | null>(null)
const filtroGrupo = ref<string | null>(null)

const colunas: DataTableColumn<AtivoFixo>[] = [
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'grupoNome', label: 'Grupo', sortable: false, width: '160px' },
  { key: 'dataAquisicao', label: 'Aquisição', sortable: false, width: '120px', align: 'center' },
  { key: 'valorCompra', label: 'Valor de compra', sortable: false, width: '160px', align: 'right' },
  { key: 'valorContabil', label: 'Valor contábil', sortable: false, width: '160px', align: 'right' },
  { key: 'status', label: 'Situação', sortable: false, width: '150px', align: 'center' }
]

const opcoesStatus: SelectOption[] = Object.entries(STATUS_ATIVO_FIXO).map(([v, label]) => ({ label, value: Number(v) }))
const opcoesGrupo = computed<SelectOption[]>(() => grupos.value.map((g) => ({ label: `${g.codigo ? g.codigo + ' — ' : ''}${g.nome}`, value: g.id })))

async function buscar() {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroStatus.value !== null) query.status = filtroStatus.value
    if (filtroGrupo.value) query.grupoBemId = filtroGrupo.value
    const r = await api.listar(query)
    itens.value = r.itens
    total.value = r.total
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []; total.value = 0
  } finally {
    carregando.value = false
  }
}

function novo() { router.push('/erp/contabilidade/ativos-fixos/novo') }
function editar(item: AtivoFixo) { router.push(`/erp/contabilidade/ativos-fixos/${item.id}`) }

onMounted(async () => {
  try { grupos.value = await api.listarGrupos() } catch { /* grupos são opcionais no filtro */ }
  await buscar()
})
</script>

<template>
  <div>
    <PageToolbar
      title="Ativos Fixos / Imobilizado"
      subtitle="Bens do imobilizado, depreciação e movimentações patrimoniais"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo ativo</button>
      </template>
    </PageToolbar>

    <div class="filtros">
      <SelectField v-model="filtroStatus" :options="opcoesStatus" label="Situação" placeholder="Todas" @update:model-value="pagina = 1; buscar()" />
      <SelectField v-model="filtroGrupo" :options="opcoesGrupo" label="Grupo de bens" placeholder="Todos" @update:model-value="pagina = 1; buscar()" />
    </div>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhum ativo fixo encontrado."
      @update:page="pagina = $event; buscar()"
      @update:page-size="tamanhoPagina = $event; pagina = 1; buscar()"
      @row-click="editar"
    >
      <template #cell-dataAquisicao="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-valorCompra="{ value }">{{ formatarMoeda(Number(value ?? 0)) }}</template>
      <template #cell-valorContabil="{ value }">{{ value != null ? formatarMoeda(Number(value)) : '—' }}</template>
      <template #cell-status="{ value }">
        <span class="badge" :class="Number(value) === 0 ? 'badge-success' : Number(value) === 1 ? 'badge-secondary' : 'badge-warning'">
          {{ STATUS_ATIVO_FIXO[Number(value)] ?? value }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="editar(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.filtros {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 1rem;
  max-width: 640px;
}
.filtros > * { flex: 1 1 240px; }
</style>
