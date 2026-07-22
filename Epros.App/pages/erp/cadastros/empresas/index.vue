<script setup lang="ts">
/**
 * Listagem de Empresas (multiempresa) — Cadastros > Empresas.
 *
 * Porta o comportamento de `cadastros/empresa/index.vue` do legado:
 * tabela server-side com Razão Social, CNPJ, Inscrição Estadual, UF e ações de editar/excluir.
 */
import { onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default', middleware: 'auth' })

interface EmpresaEndereco {
  estado?: string
  uf?: string
}

interface EmpresaListItem {
  id: string
  razaoSocial: string
  nomeFantasia?: string
  cnpj?: string
  inscricaoEstadual?: string
  endereco?: EmpresaEndereco
  certificadoDigitalDataValidade?: string | null
}

const toast = useToast()
const { maskCpfCnpj } = useMask()
const { formatarDataHora } = useHelper()

const lista = useApiList<EmpresaListItem, { busca?: string }>('/cadastros/empresas', {
  filtrosIniciais: { busca: '' }
})

const colunas: DataTableColumn<EmpresaListItem>[] = [
  { key: 'razaoSocial', label: 'Razão Social', sortable: true },
  { key: 'nomeFantasia', label: 'Nome Fantasia', sortable: true },
  { key: 'cnpj', label: 'CNPJ', align: 'center', formatter: (v) => maskCpfCnpj(String(v ?? '')) },
  { key: 'inscricaoEstadual', label: 'Inscrição Estadual' },
  {
    key: 'uf',
    label: 'UF',
    align: 'center',
    formatter: (_v, row) => row.endereco?.estado ?? row.endereco?.uf ?? ''
  },
  {
    key: 'certificadoDigitalDataValidade',
    label: 'Validade Certificado',
    align: 'center',
    formatter: (v) => (v ? formatarDataHora(v as string) : '')
  }
]

const filtroCampos: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Razão social, CNPJ...', grow: true }
]

const excluindoId = ref<string | null>(null)
const confirmDeleteAberto = ref(false)
const empresaParaExcluir = ref<EmpresaListItem | null>(null)

function pedirExclusao(item: EmpresaListItem) {
  empresaParaExcluir.value = item
  confirmDeleteAberto.value = true
}

async function confirmarExclusao() {
  const item = empresaParaExcluir.value
  if (!item) return
  excluindoId.value = item.id
  try {
    await useApi.delete(`/cadastros/empresas/{id}`, { params: { id: item.id } })
    toast.success('Empresa excluída com sucesso.')
    confirmDeleteAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindoId.value = null
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Cadastro de Empresas" subtitle="Empresas do grupo (multiempresa) e parâmetros de DF-e" :loading="lista.carregando.value">
      <template #actions>
        <NuxtLink to="/erp/cadastros/empresas/novo" class="btn btn-primary">
          + Nova empresa
        </NuxtLink>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="filtroCampos"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma empresa cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <NuxtLink :to="`/erp/cadastros/empresas/${row.id}`" class="btn btn-ghost btn-sm">Editar</NuxtLink>
        <button type="button" class="btn btn-ghost btn-sm" @click="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="confirmDeleteAberto"
      :item-label="empresaParaExcluir?.razaoSocial"
      :loading="!!excluindoId"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
