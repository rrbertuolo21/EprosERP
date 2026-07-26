<script setup lang="ts" generic="T extends object">
/**
 * RecursoLista — componente local do módulo Concessionárias.
 *
 * Compõe os componentes compartilhados (PageToolbar + FilterBar + DataTable + AppDialog +
 * campos de `shared/fields`) para entregar, de forma declarativa, a tela padrão dos
 * agregados avançados: LISTA de leitura (GET paginado) + CRIAÇÃO via diálogo (POST).
 *
 * A maioria dos endpoints do módulo expõe apenas `GET lista` + `POST criar` — não há
 * `GET /{id}` nem `PUT/DELETE`. Por isso não há edição/exclusão inline: criar abre um
 * diálogo; ações específicas (fechar OS, julgar garantia) entram pelo slot `#acoes-linha`.
 *
 * Toda IO passa por `useApi`/`useApiList`. Sem URLs hardcoded, sem `$fetch`.
 */
import { ref, reactive, watch } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import type { SelectOption } from '~/composables/useEnum'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { carregarOpcoesFk } from './fkOptions'

/** Tipos de campo suportados pelo formulário de criação declarativo. */
export type TipoCampo =
  | 'text'
  | 'textarea'
  | 'int'
  | 'money'
  | 'quantity'
  | 'percent'
  | 'date'
  | 'datetime'
  | 'select'
  | 'fk'
  | 'boolean'

export interface CampoForm {
  key: string
  label: string
  tipo: TipoCampo
  obrigatorio?: boolean
  /** Opções estáticas (tipo 'select'). */
  opcoes?: SelectOption[]
  /** Rota de listagem para carregar opções (tipo 'fk'). SEM /api/v1. */
  fkPath?: string
  /** Texto de ajuda abaixo do campo. */
  hint?: string
}

interface FiltroBusca {
  busca?: string
}

const props = withDefaults(
  defineProps<{
    title: string
    subtitle?: string
    /** Rota base do recurso (GET lista + POST criar). SEM /api/v1. */
    path: string
    columns: DataTableColumn<T>[]
    /** Campos do corpo do POST. Vazio => somente leitura (sem botão criar). */
    campos?: CampoForm[]
    /** Rótulo do botão de criação. */
    rotuloNovo?: string
    /** Habilita a coluna de ações (usa o slot `#acoes-linha`). */
    temAcoes?: boolean
    /** Habilita o filtro de busca textual. */
    comBusca?: boolean
  }>(),
  { campos: () => [], rotuloNovo: '+ Novo', temAcoes: false, comBusca: true }
)

const emit = defineEmits<{ recarregar: [] }>()

const toast = useToast()

const lista = useApiList<T, FiltroBusca>(props.path, {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const camposFiltro: FilterField[] = props.comBusca
  ? [{ key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Pesquisar...', grow: true }]
  : []

// ---- Diálogo de criação -------------------------------------------------
const dialogoAberto = ref(false)
const salvando = ref(false)
const form = reactive<Record<string, unknown>>({})
const erros = reactive<Record<string, string>>({})
const opcoesFk = reactive<Record<string, SelectOption[]>>({})

function valorInicial(campo: CampoForm): unknown {
  if (campo.tipo === 'boolean') return false
  if (['money', 'quantity', 'percent'].includes(campo.tipo)) return 0
  return null
}

function resetForm() {
  for (const k of Object.keys(form)) delete form[k]
  for (const k of Object.keys(erros)) delete erros[k]
  for (const campo of props.campos) form[campo.key] = valorInicial(campo)
}

async function carregarFks() {
  for (const campo of props.campos) {
    if (campo.tipo === 'fk' && campo.fkPath && !opcoesFk[campo.key]) {
      opcoesFk[campo.key] = await carregarOpcoesFk(campo.fkPath)
    }
  }
}

async function abrirNovo() {
  resetForm()
  dialogoAberto.value = true
  await carregarFks()
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  for (const campo of props.campos) {
    if (!campo.obrigatorio) continue
    const v = form[campo.key]
    const vazio = v === null || v === undefined || v === '' || (campo.tipo === 'int' && Number.isNaN(v))
    if (vazio) erros[campo.key] = `${campo.label} é obrigatório.`
  }
  return Object.keys(erros).length === 0
}

function montarCorpo(): Record<string, unknown> {
  const corpo: Record<string, unknown> = {}
  for (const campo of props.campos) {
    const v = form[campo.key]
    if (campo.tipo === 'int') corpo[campo.key] = v === null || v === '' ? null : Number(v)
    else corpo[campo.key] = v
  }
  return corpo
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    await useApi(props.path, { method: 'POST', body: montarCorpo() })
    toast.success('Registro criado com sucesso!')
    dialogoAberto.value = false
    await lista.buscar()
    emit('recarregar')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

watch(dialogoAberto, (aberto) => {
  if (!aberto) {
    for (const k of Object.keys(erros)) delete erros[k]
  }
})

// Expõe recarregar para o pai (após ações do slot).
defineExpose({ recarregar: () => lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar :title="title" :subtitle="subtitle" :loading="lista.carregando.value">
      <template #actions>
        <slot name="toolbar-extra" />
        <button v-if="campos.length" type="button" class="btn btn-primary" @click="abrirNovo">
          {{ rotuloNovo }}
        </button>
      </template>
    </PageToolbar>

    <FilterBar
      v-if="comBusca"
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="columns"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum registro encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <!-- Repassa slots de célula customizados definidos pela página. -->
      <template v-for="(_, nome) in $slots" #[nome]="slotProps" :key="nome">
        <slot :name="nome" v-bind="slotProps ?? {}" />
      </template>

      <template v-if="temAcoes" #actions="{ row }">
        <slot name="acoes-linha" :row="row" />
      </template>
    </DataTable>

    <AppDialog v-model="dialogoAberto" :title="rotuloNovo.replace(/^\+\s*/, '')" width="640px" persistent>
      <form class="form-grid" @submit.prevent="salvar">
        <template v-for="campo in campos" :key="campo.key">
          <SelectField
            v-if="campo.tipo === 'select' || campo.tipo === 'fk'"
            :model-value="(form[campo.key] as string | number | null)"
            :label="campo.label"
            :required="campo.obrigatorio"
            :options="campo.tipo === 'fk' ? (opcoesFk[campo.key] ?? []) : (campo.opcoes ?? [])"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
          <MoneyInput
            v-else-if="campo.tipo === 'money'"
            :model-value="(form[campo.key] as number | null)"
            :label="campo.label"
            :required="campo.obrigatorio"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
          <QuantityInput
            v-else-if="campo.tipo === 'quantity'"
            :model-value="(form[campo.key] as number | null)"
            :label="campo.label"
            :required="campo.obrigatorio"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
          <PercentInput
            v-else-if="campo.tipo === 'percent'"
            :model-value="(form[campo.key] as number | null)"
            :label="campo.label"
            :required="campo.obrigatorio"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
          <DateTimeField
            v-else-if="campo.tipo === 'date' || campo.tipo === 'datetime'"
            :model-value="(form[campo.key] as string | null)"
            :label="campo.label"
            :mode="campo.tipo === 'datetime' ? 'datetime' : 'date'"
            :required="campo.obrigatorio"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
          <label v-else-if="campo.tipo === 'boolean'" class="field toggle-row">
            <span class="field-label">{{ campo.label }}</span>
            <input
              type="checkbox"
              :checked="!!form[campo.key]"
              @change="form[campo.key] = ($event.target as HTMLInputElement).checked"
            />
          </label>
          <TextField
            v-else
            :model-value="(form[campo.key] as string | number | null)"
            :label="campo.label"
            :type="campo.tipo === 'int' ? 'number' : 'text'"
            :required="campo.obrigatorio"
            :error="erros[campo.key]"
            :hint="campo.hint"
            @update:model-value="(v) => (form[campo.key] = v)"
          />
        </template>
      </form>

      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dialogoAberto = false">
          Cancelar
        </button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}
.toggle-row {
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: flex-start;
}
.toggle-row input {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
}
</style>
