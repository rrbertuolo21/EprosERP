<script setup lang="ts">
/**
 * Código de Benefício Fiscal — Fiscal / Código de Benefício Fiscal.
 *
 * Porta o comportamento de `fiscal/codigo-beneficio-fiscal.vue` do legado: listagem paginada
 * server-side com CRUD em modal (código, descrição, UF, CSTs e CSOSNs vinculados).
 * Endpoint: `codigos-beneficios-fiscais`.
 */
import { reactive, ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface CstItem { id?: string; cst: number }
interface CsosnItem { id?: string; csosn: number }

interface CodigoBeneficioFiscal {
  id: string
  codigo: string
  descricao: string
  uf: string
  csts?: CstItem[]
  csosns?: CsosnItem[]
}

interface CodigoBeneficioFiscalFiltros {
  localizar?: string
}

const ufsBrasil = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]
const opcoesUf: SelectOption[] = ufsBrasil.map((uf) => ({ label: uf, value: uf }))

const toast = useToast()

const lista = useApiList<CodigoBeneficioFiscal, CodigoBeneficioFiscalFiltros>('/codigos-beneficios-fiscais', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<CodigoBeneficioFiscal>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'uf', label: 'UF', sortable: true, width: '80px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

interface BeneficioForm {
  id?: string
  codigo: string
  descricao: string
  uf: string
  csts: number[]
  csosns: number[]
}

function formVazio(): BeneficioForm {
  return { id: undefined, codigo: '', descricao: '', uf: '', csts: [], csosns: [] }
}

const dialogAberto = ref(false)
const salvando = ref(false)
const form = reactive<BeneficioForm>(formVazio())
const originalCsts = ref<CstItem[]>([])
const originalCsosns = ref<CsosnItem[]>([])

// Entradas livres de código CST/CSOSN (não há endpoint de enum dedicado nesta fatia;
// os valores são digitados como números separados por vírgula, espelhando o campo multi-select do legado).
const cstsTexto = ref('')
const csosnsTexto = ref('')

function abrirNovo() {
  Object.assign(form, formVazio())
  originalCsts.value = []
  originalCsosns.value = []
  cstsTexto.value = ''
  csosnsTexto.value = ''
  dialogAberto.value = true
}

function abrirEdicao(item: CodigoBeneficioFiscal) {
  form.id = item.id
  form.codigo = item.codigo ?? ''
  form.descricao = item.descricao ?? ''
  form.uf = item.uf ?? ''
  form.csts = item.csts?.map((c) => c.cst) ?? []
  form.csosns = item.csosns?.map((c) => c.csosn) ?? []
  originalCsts.value = item.csts ?? []
  originalCsosns.value = item.csosns ?? []
  cstsTexto.value = form.csts.join(', ')
  csosnsTexto.value = form.csosns.join(', ')
  dialogAberto.value = true
}

function parseNumeros(texto: string): number[] {
  return texto
    .split(',')
    .map((v) => Number(v.trim()))
    .filter((n) => !isNaN(n) && n > 0)
}

async function salvar() {
  if (!form.codigo || !form.descricao) {
    toast.error('Informe código e descrição.')
    return
  }
  form.csts = parseNumeros(cstsTexto.value)
  form.csosns = parseNumeros(csosnsTexto.value)

  salvando.value = true
  try {
    const csts = form.csts.map((cst) => {
      const existente = originalCsts.value.find((o) => o.cst === cst)
      return existente ? { id: existente.id, cst } : { cst }
    })
    const csosns = form.csosns.map((csosn) => {
      const existente = originalCsosns.value.find((o) => o.csosn === csosn)
      return existente ? { id: existente.id, csosn } : { csosn }
    })

    const isEdit = !!form.id
    const body = {
      ...(isEdit ? { id: form.id } : {}),
      codigo: form.codigo,
      descricao: form.descricao,
      uf: form.uf,
      csts,
      csosns
    }

    if (isEdit) {
      await useApi(`/codigos-beneficios-fiscais/${form.id}`, { method: 'PUT', body })
    } else {
      await useApi('/codigos-beneficios-fiscais', { method: 'POST', body })
    }
    toast.success('Código de Benefício Fiscal salvo com sucesso!')
    dialogAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<CodigoBeneficioFiscal | null>(null)

function pedirExclusao(item: CodigoBeneficioFiscal) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/codigos-beneficios-fiscais/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Código de Benefício Fiscal excluído com sucesso!')
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
    <PageToolbar title="Código de Benefício Fiscal" subtitle="Códigos de benefício fiscal do ICMS por UF" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Adicionar</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
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
      empty-text="Nenhum código de benefício fiscal cadastrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrirEdicao"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="form.id ? 'Editando Código de Benefício Fiscal' : 'Novo Código de Benefício Fiscal'" width="640px">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid form-grid-1">
          <TextField v-model="form.descricao" label="Descrição" required maxlength="200" />
        </div>
        <div class="form-grid">
          <TextField v-model="form.codigo" label="Código" required maxlength="10" />
          <SelectField v-model="form.uf" label="UF" :options="opcoesUf" />
        </div>
        <div class="form-grid form-grid-1">
          <TextField v-model="cstsTexto" label="CSTs vinculados" placeholder="Ex.: 00, 10, 20 (códigos separados por vírgula)" hint="Informe os códigos CST do ICMS separados por vírgula." />
          <TextField v-model="csosnsTexto" label="CSOSNs vinculados" placeholder="Ex.: 101, 102 (códigos separados por vírgula)" hint="Informe os códigos CSOSN separados por vírgula." />
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogAberto = false">Fechar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
  margin-bottom: 12px;
}
.form-grid-1 { grid-template-columns: 1fr; }
</style>
