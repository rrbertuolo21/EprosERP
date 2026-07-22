<script setup lang="ts">
/**
 * Listagem de Parceiros (cliente/fornecedor/transportadora/prestador/motorista/funcionário).
 *
 * Porta o comportamento de `cadastros/parceiro/index.vue` do legado:
 * busca textual + filtros (IE, UF, classificações), tabela server-side com
 * colunas derivadas (nome, CPF/CNPJ, telefone/e-mail/UF/cidade do contato/endereço principal),
 * ações de editar/excluir com confirmação.
 */
import { computed, onMounted, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import type { PessoaListaItem } from '~/components/cadastros-parceiro/types'
import { contatoPrincipal, documentoExibicao, enderecoPrincipal, nomeExibicao } from '~/components/cadastros-parceiro/parceiroDisplay'

definePageMeta({ layout: 'default' })

interface ParceiroFiltros {
  [key: string]: unknown
  localizar?: string
  inscricaoEstadual?: string
  uf?: string
  ehCliente?: boolean
  ehFornecedor?: boolean
  ehTransportadora?: boolean
  ehFuncionario?: boolean
  ehPrestadorServico?: boolean
  ehMotorista?: boolean
}

const toast = useToast()
const { maskCpfCnpj, maskTelefone } = useMask()
const router = useRouter()

const lista = useApiList<PessoaListaItem, ParceiroFiltros>('/cadastros/pessoas', {
  filtrosIniciais: {}
})

const filtrosForm = ref<ParceiroFiltros>({})

const opcoesUf = ref<{ label: string; value: string }[]>(
  ['AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO']
    .map((uf) => ({ label: uf, value: uf }))
)

const filterFields = computed<FilterField[]>(() => [
  { key: 'localizar', label: 'Buscar (nome, CPF, CNPJ...)', type: 'text', grow: true },
  { key: 'inscricaoEstadual', label: 'Inscrição Estadual', type: 'text' },
  { key: 'uf', label: 'UF', type: 'select', options: opcoesUf.value },
  { key: 'ehCliente', label: 'Cliente', type: 'boolean', placeholder: 'Cliente' },
  { key: 'ehFornecedor', label: 'Fornecedor', type: 'boolean', placeholder: 'Fornecedor' },
  { key: 'ehTransportadora', label: 'Transportadora', type: 'boolean', placeholder: 'Transportadora' },
  { key: 'ehFuncionario', label: 'Funcionário', type: 'boolean', placeholder: 'Funcionário' },
  { key: 'ehPrestadorServico', label: 'Prestador de Serviço', type: 'boolean', placeholder: 'Prestador de Serviço' },
  { key: 'ehMotorista', label: 'Motorista', type: 'boolean', placeholder: 'Motorista' }
])

const columns: DataTableColumn<PessoaListaItem>[] = [
  { key: 'sequenciaTenantId', label: '#', width: '70px' },
  { key: 'nome', label: 'Nome' },
  { key: 'documento', label: 'CPF/CNPJ' },
  { key: 'tipo', label: 'Rel. Comercial' },
  { key: 'telefone', label: 'Telefone' },
  { key: 'email', label: 'E-mail' },
  { key: 'uf', label: 'UF', width: '60px' },
  { key: 'cidade', label: 'Cidade' }
]

async function buscar() {
  await lista.buscar()
}

function aoBuscarFiltros(valores: Record<string, unknown>) {
  lista.aplicarFiltros(valores as Partial<ParceiroFiltros>)
}

function aoLimparFiltros() {
  filtrosForm.value = {}
  lista.limpar()
}

function novoParceiro() {
  router.push('/erp/cadastros/parceiros/novo')
}

function editarParceiro(item: PessoaListaItem) {
  router.push(`/erp/cadastros/parceiros/${item.id}`)
}

// --- Exclusão ---
const excluirAberto = ref(false)
const excluirAlvo = ref<PessoaListaItem | null>(null)
const excluindo = ref(false)

function solicitarExclusao(item: PessoaListaItem) {
  excluirAlvo.value = item
  excluirAberto.value = true
}

async function confirmarExclusao() {
  if (!excluirAlvo.value) return
  excluindo.value = true
  try {
    await useApi.delete('/cadastros/pessoas/{id}', { params: { id: excluirAlvo.value.id } })
    toast.success('Parceiro excluído com sucesso.')
    excluirAberto.value = false
    excluirAlvo.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
    console.error('[parceiros/index.confirmarExclusao]', e)
  } finally {
    excluindo.value = false
  }
}

// Pré-filtra por Cliente/Fornecedor quando a rota chega com `?tipo=cliente|fornecedor`
// (usado pelos atalhos de Acesso Rápido, que hoje apontavam ambos para esta mesma tela).
const route = useRoute()

onMounted(async () => {
  const tipo = route.query.tipo
  if (tipo === 'cliente') {
    filtrosForm.value = { ...filtrosForm.value, ehCliente: true }
    lista.aplicarFiltros({ ehCliente: true })
  } else if (tipo === 'fornecedor') {
    filtrosForm.value = { ...filtrosForm.value, ehFornecedor: true }
    lista.aplicarFiltros({ ehFornecedor: true })
  } else {
    await buscar()
  }
})
</script>

<template>
  <div>
    <PageToolbar title="Parceiros" subtitle="Clientes, fornecedores, transportadoras e demais parceiros" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoParceiro">+ Novo</button>
      </template>
    </PageToolbar>

    <FilterBar
      v-model="filtrosForm"
      :fields="filterFields"
      :loading="lista.carregando.value"
      @search="aoBuscarFiltros"
      @clear="aoLimparFiltros"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="columns"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum parceiro encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarParceiro"
    >
      <template #cell-nome="{ row }">
        {{ nomeExibicao(row) }}
      </template>
      <template #cell-documento="{ row }">
        {{ documentoExibicao(row, maskCpfCnpj) }}
      </template>
      <template #cell-tipo="{ row }">
        <span class="chips-tipo">
          <span v-if="row.ehCliente" class="chip chip-success">Cliente</span>
          <span v-if="row.ehFornecedor" class="chip chip-danger">Fornecedor</span>
          <span v-if="row.ehTransportadora" class="chip chip-primary">Transportadora</span>
          <span v-if="row.ehFuncionario" class="chip chip-warning">Funcionário</span>
          <span v-if="row.ehPrestadorServico" class="chip">Prestador de Serviço</span>
          <span v-if="row.ehMotorista" class="chip">Motorista</span>
        </span>
      </template>
      <template #cell-telefone="{ row }">
        {{ contatoPrincipal(row)?.numeroTelefone ? maskTelefone(contatoPrincipal(row)!.numeroTelefone as string) : '' }}
      </template>
      <template #cell-email="{ row }">
        {{ contatoPrincipal(row)?.email }}
      </template>
      <template #cell-uf="{ row }">
        {{ enderecoPrincipal(row)?.uf }}
      </template>
      <template #cell-cidade="{ row }">
        {{ enderecoPrincipal(row)?.municipioNome ?? '' }}
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editarParceiro(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="solicitarExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirAberto"
      :item-label="excluirAlvo ? nomeExibicao(excluirAlvo) : undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.chips-tipo { display: flex; gap: 6px; flex-wrap: wrap; }
</style>
