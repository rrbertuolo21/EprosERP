<script setup lang="ts">
/**
 * Listagem de Imóveis — Imobiliária / Imóveis.
 *
 * Fonte: GET /imobiliaria/imoveis (ListarImoveisQuery devolve a lista completa do tenant,
 * sem paginação/filtro server-side). Ações: visualizar (GET /{id}) e excluir (DELETE /{id}).
 * A API não expõe atualização (PUT) — não há edição.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface Imovel {
  id: string
  descricao?: string | null
  cep?: string | null
  logradouro?: string | null
  numero?: string | null
  complemento?: string | null
  bairro?: string | null
  municipioId?: string | null
  status?: number | string | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Imovel>('/imobiliaria/imoveis', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Imovel>[] = [
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'logradouro', label: 'Endereço', sortable: false },
  { key: 'bairro', label: 'Bairro', sortable: true },
  { key: 'cep', label: 'CEP', sortable: false, width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

const STATUS_IMOVEL: Record<string, string> = {
  '0': 'Em cadastro',
  '1': 'Disponível',
  '2': 'Locado',
  '3': 'Inativo',
  EmCadastro: 'Em cadastro',
  Disponivel: 'Disponível',
  Locado: 'Locado',
  Inativo: 'Inativo'
}

function rotularStatus(v: unknown): string {
  if (v === null || v === undefined) return '—'
  return STATUS_IMOVEL[String(v)] ?? String(v)
}

function enderecoResumo(row: Imovel): string {
  const partes = [row.logradouro, row.numero].filter(Boolean)
  return partes.length ? partes.join(', ') : '—'
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Imovel | null>(null)

function novoImovel() {
  router.push('/erp/imobiliaria/imoveis/novo')
}

function verImovel(item: Imovel) {
  router.push(`/erp/imobiliaria/imoveis/${item.id}`)
}

function pedirExclusao(item: Imovel) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/imobiliaria/imoveis/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Imóvel excluído com sucesso.')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Imóveis" subtitle="Cadastro de imóveis, proprietários, vistorias e custos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoImovel">+ Novo imóvel</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum imóvel encontrado. Adicione um novo imóvel para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="verImovel"
    >
      <template #cell-logradouro="{ row }">
        <span>{{ enderecoResumo(row) }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge">{{ rotularStatus(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Visualizar" @click.stop="verImovel(row)">Visualizar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
