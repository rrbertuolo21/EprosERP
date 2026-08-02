<script setup lang="ts">
/**
 * Sourcing — Cotações multi-fornecedor (erp/compras/sourcing).
 *
 * Camada de apresentação sobre `CotacoesCompraController` (`/api/v1/cotacoes-compra`):
 *   - listagem paginada server-side (envelope CommandResult `dados = { total, pagina, itens }`);
 *   - abrir o mapa comparativo de uma cotação (rota de detalhe);
 *   - criar cotação (diálogo simples de cabeçalho — itens/fornecedores no backend/detalhe).
 *
 * Endpoints: cotacoes-compra (GET, POST).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface CotacaoListagem {
  id: string
  descricao: string | null
  situacao: string | null
  dataCotacao: string | null
  criadoEm: string | null
}
interface CotacoesDados {
  total: number
  pagina: number
  itens: CotacaoListagem[]
}

const toast = useToast()
const { formatarData } = useHelper()

const itens = ref<CotacaoListagem[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)

const filtrosForm = ref<Record<string, unknown>>({ localizar: '' })
const localizar = ref('')

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Descrição da cotação', grow: true }
]

const colunas: DataTableColumn<CotacaoListagem>[] = [
  { key: 'descricao', label: 'Descrição', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'situacao', label: 'Situação', align: 'center', width: '160px' },
  { key: 'dataCotacao', label: 'Data', width: '120px', formatter: (v) => formatarData(v as string | null) },
  { key: 'criadoEm', label: 'Criada em', width: '120px', formatter: (v) => formatarData(v as string | null) }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const resposta = await useApi('/cotacoes-compra', {
      query: { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    })
    const dados = extrairDados<CotacoesDados>(resposta)
    let lista = dados?.itens ?? []
    // Filtro por descrição é client-side (o endpoint atual não expõe busca textual).
    if (localizar.value.trim()) {
      const t = localizar.value.trim().toLowerCase()
      lista = lista.filter((c) => (c.descricao ?? '').toLowerCase().includes(t))
    }
    itens.value = lista
    total.value = dados?.total ?? lista.length
  } catch (e) {
    itens.value = []
    total.value = 0
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

let debounceTimer: ReturnType<typeof setTimeout> | undefined
function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    localizar.value = (valores.localizar as string) || ''
    void buscar()
  }, 500)
}
function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  localizar.value = (valores.localizar as string) || ''
  void buscar()
}
function aoLimpar() {
  filtrosForm.value = { localizar: '' }
  localizar.value = ''
  void buscar()
}
function irParaPagina(p: number) {
  pagina.value = p
  void buscar()
}
function mudarTamanho(ps: number) {
  tamanhoPagina.value = ps
  pagina.value = 1
  void buscar()
}

function abrirMapa(item: CotacaoListagem) {
  navigateTo(`/erp/compras/sourcing/${item.id}`)
}

// --- Nova cotação (cabeçalho) ---
const novaVisivel = ref(false)
const salvando = ref(false)
const nova = ref({ descricao: '', situacao: 'Aberta', dataCotacao: hojeIso() })

function hojeIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function abrirNova() {
  nova.value = { descricao: '', situacao: 'Aberta', dataCotacao: hojeIso() }
  novaVisivel.value = true
}

async function salvarNova() {
  if (!nova.value.descricao.trim()) {
    toast.error('Informe a descrição da cotação')
    return
  }
  salvando.value = true
  try {
    await useApi('/cotacoes-compra', {
      method: 'POST',
      body: {
        descricao: nova.value.descricao.trim(),
        situacao: nova.value.situacao,
        dataCotacao: nova.value.dataCotacao,
        fornecedores: [],
        itens: []
      }
    })
    toast.success('Cotação criada. Adicione fornecedores e itens para montar o mapa comparativo.')
    novaVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void buscar())
</script>

<template>
  <div>
    <PageToolbar title="Sourcing" subtitle="Cotações multi-fornecedor e mapa comparativo" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNova">+ Nova Cotação</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="carregando"
      @update:model-value="aoMudarFiltros"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhuma cotação encontrada"
      @update:page="irParaPagina"
      @update:page-size="mudarTamanho"
      @row-click="abrirMapa"
    >
      <template #cell-situacao="{ row }">
        <span class="badge st-pendente">{{ row.situacao || '-' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Mapa comparativo" @click.stop="abrirMapa(row)">
          📊
        </button>
      </template>
    </DataTable>

    <AppDialog v-model="novaVisivel" title="Nova Cotação" width="520px" persistent>
      <div class="form-grid">
        <div class="col-12">
          <TextField v-model="nova.descricao" label="Descrição" required placeholder="Ex.: Cotação de matéria-prima Q3" />
        </div>
        <div class="col-6">
          <TextField v-model="nova.situacao" label="Situação" />
        </div>
        <div class="col-6">
          <DateTimeField v-model="nova.dataCotacao" label="Data da cotação" />
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="novaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarNova">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.badge.st-pendente {
  background: rgba(245, 158, 11, 0.1);
  border: 1px solid rgba(245, 158, 11, 0.3);
  color: var(--warning);
}
</style>
