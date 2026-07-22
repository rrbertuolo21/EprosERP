<script setup lang="ts">
/**
 * Cadastro de Balanças (configuração de leitura de código de barras balança).
 * Porta o comportamento de `cadastros/produto/balanca.vue` do legado.
 *
 * Endpoints: balancas (api/v1/balancas), balancas/enum-tipo-valor-balanca (enum de domínio)
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface Balanca extends Record<string, unknown> {
  id: number
  nome: string
  qntDigitoIdentificador: number
  qntDigitoCodigoProduto: number
  qntDigitoValorProduto: number
  qntCasaDecimal: number
  tipoValor: number
}

interface BalancaForm {
  id?: number
  nome: string
  qntDigitoIdentificador: number
  qntDigitoCodigoProduto: number
  qntDigitoValorProduto: number
  qntCasaDecimal: number
  tipoValor: number | null
}

function estadoInicial(): BalancaForm {
  return {
    nome: '',
    qntDigitoIdentificador: 0,
    qntDigitoCodigoProduto: 4,
    qntDigitoValorProduto: 3,
    qntCasaDecimal: 2,
    tipoValor: 1
  }
}

const toast = useToast()
const { carregarOpcoes } = useEnum()

const lista = useApiList<Balanca>('/balancas', { tamanhoPaginaInicial: 20 })

const opcoesTipoValor = ref<SelectOption[]>([])

function formatarTipoValor(valor: number): string {
  const encontrada = opcoesTipoValor.value.find((o) => Number(o.value) === valor)
  if (encontrada) return encontrada.label
  return valor === 1 ? 'Peso (KG)' : 'Preço (R$)'
}

const colunas: DataTableColumn<Balanca>[] = [
  { key: 'id', label: '#', width: '60px' },
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'qntDigitoIdentificador', label: 'Dígito Identificador', align: 'right' },
  { key: 'qntDigitoCodigoProduto', label: 'Casas Código Produto', align: 'right' },
  { key: 'qntDigitoValorProduto', label: 'Casas Valor/Peso', align: 'right' },
  { key: 'qntCasaDecimal', label: 'Casas Decimais', align: 'right' },
  { key: 'tipoValor', label: 'Tipo Código', formatter: (v) => formatarTipoValor(v as number) }
]

const dialogAberto = ref(false)
const salvando = ref(false)
const erros = reactive<{ nome: string }>({ nome: '' })
const form = reactive<BalancaForm>(estadoInicial())
const editando = ref(false)

function abrirNovo() {
  editando.value = false
  Object.assign(form, estadoInicial())
  form.id = undefined
  erros.nome = ''
  dialogAberto.value = true
}

function abrirEdicao(item: Balanca) {
  editando.value = true
  Object.assign(form, item)
  erros.nome = ''
  dialogAberto.value = true
}

function validar(): boolean {
  erros.nome = ''
  if (!form.nome.trim()) {
    erros.nome = 'Informe o nome da balança'
    return false
  }
  return true
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    if (editando.value && form.id) {
      await useApi.put(`/balancas/{id}`, { ...form }, { params: { id: form.id } })
    } else {
      await useApi.post('/balancas', { ...form })
    }
    toast.success('Dados salvos com sucesso')
    dialogAberto.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const itemParaExcluir = ref<Balanca | null>(null)
const excluindo = ref(false)

function solicitarExclusao(item: Balanca) {
  itemParaExcluir.value = item
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi.delete(`/balancas/{id}`, { params: { id: itemParaExcluir.value.id } })
    toast.success('Registro excluído!')
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(async () => {
  opcoesTipoValor.value = await carregarOpcoes('/balancas/enum-tipo-valor-balanca')
  await lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Balança" subtitle="Configuração de balanças (código de barras)" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Adicionar</button>
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
      empty-text="Nenhuma balança cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="solicitarExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="editando ? 'Editar Balança' : 'Cadastrar Balança'" width="560px" persistent>
      <form class="form-grid" @submit.prevent="salvar">
        <TextField v-model="form.nome" label="Nome da Balança" placeholder="Ex: Balança Hortifruti" required :error="erros.nome" />
        <div class="form-row">
          <TextField
            v-model.number="form.qntDigitoIdentificador"
            label="Dígito Identificador (posição inicial)"
            type="number"
            placeholder="Ex: 2"
            required
          />
          <TextField v-model.number="form.qntDigitoCodigoProduto" label="Casas do Código Produto" type="number" required />
        </div>
        <div class="form-row">
          <TextField v-model.number="form.qntDigitoValorProduto" label="Casas do Valor/Peso" type="number" required />
          <TextField v-model.number="form.qntCasaDecimal" label="Casas Decimais" type="number" required />
        </div>
        <SelectField v-model="form.tipoValor" label="Tipo de Código" :options="opcoesTipoValor" :clearable="false" />
      </form>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dialogAberto = false">Fechar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar Balança</span>
        </button>
      </template>
    </AppDialog>

    <DeleteAlert
      :model-value="!!itemParaExcluir"
      :item-label="itemParaExcluir?.nome"
      :loading="excluindo"
      @update:model-value="itemParaExcluir = null"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid { display: flex; flex-direction: column; gap: 12px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
</style>
