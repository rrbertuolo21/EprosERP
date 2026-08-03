<script setup lang="ts">
/**
 * Cadastro de Unidades de Medida.
 * Porta o comportamento de `cadastros/produto/unidade.vue` do legado (que usa apenas
 * `unidades-de-medidas-comercial`). O MAPA_FRONTEND desta fatia também lista
 * `unidades-de-medidas-tributaveis`, então a tela oferece uma aba para alternar entre
 * "Comercial" e "Tributável" — mesmo formulário, endpoint diferente.
 *
 * Endpoints: unidades-de-medidas-comercial, unidades-de-medidas-tributaveis
 */
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useRules } from '~/composables/useRules'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface Unidade extends Record<string, unknown> {
  id: number
  unidadeMedida: string
  descricao: string
  fator?: number
  sequenciaTenantId?: number
}

interface UnidadeForm {
  id?: number
  unidadeMedida: string
  descricao: string
  fator: number
}

const toast = useToast()
const rules = useRules()

type TipoUnidade = 'comercial' | 'tributavel'
const abaAtiva = ref<TipoUnidade>('comercial')

const endpoint = computed(() =>
  abaAtiva.value === 'comercial' ? '/unidades-de-medidas-comercial' : '/unidades-de-medidas-tributaveis'
)

// useApiList fixa a rota na criação; para alternar de aba mantemos duas instâncias
// independentes (uma por endpoint) e escolhemos a ativa via `listaAtiva`.
const listaComercial = useApiList<Unidade>('/unidades-de-medidas-comercial', { tamanhoPaginaInicial: 20 })
const listaTributavel = useApiList<Unidade>('/unidades-de-medidas-tributaveis', { tamanhoPaginaInicial: 20 })

const listaAtiva = computed(() => (abaAtiva.value === 'comercial' ? listaComercial : listaTributavel))

const colunas: DataTableColumn<Unidade>[] = [
  { key: 'sequenciaTenantId', label: '#', width: '70px' },
  { key: 'unidadeMedida', label: 'Sigla', width: '100px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'fator', label: 'Fator', align: 'right', width: '100px' }
]

const dialogAberto = ref(false)
const salvando = ref(false)
const erros = reactive<{ unidadeMedida: string; descricao: string; fator: string }>({
  unidadeMedida: '',
  descricao: '',
  fator: ''
})
const form = reactive<UnidadeForm>({ unidadeMedida: '', descricao: '', fator: 1 })
const editando = ref(false)

function limparErros() {
  erros.unidadeMedida = ''
  erros.descricao = ''
  erros.fator = ''
}

function abrirNovo() {
  editando.value = false
  form.id = undefined
  form.unidadeMedida = ''
  form.descricao = ''
  form.fator = 1
  limparErros()
  dialogAberto.value = true
}

function abrirEdicao(item: Unidade) {
  editando.value = true
  form.id = item.id
  form.unidadeMedida = item.unidadeMedida
  form.descricao = item.descricao
  form.fator = item.fator ?? 1
  limparErros()
  dialogAberto.value = true
}

function validar(): boolean {
  limparErros()
  let ok = true
  if (!form.unidadeMedida.trim()) {
    erros.unidadeMedida = 'Informe a sigla'
    ok = false
  } else if (form.unidadeMedida.length > 6) {
    erros.unidadeMedida = 'Máximo de 6 caracteres'
    ok = false
  }
  if (!form.descricao.trim()) {
    erros.descricao = 'Informe a descrição'
    ok = false
  }
  if (form.fator == null || Number.isNaN(form.fator)) {
    erros.fator = 'Informe o fator'
    ok = false
  } else {
    const erroFator = rules.fatorMaiorQueZero(form.fator)
    if (erroFator !== true) {
      erros.fator = erroFator
      ok = false
    }
  }
  return ok
}

async function salvar() {
  if (!validar()) return
  salvando.value = true
  try {
    const path = endpoint.value
    if (editando.value && form.id) {
      await useApi.put(`${path}/{id}`, { ...form }, { params: { id: form.id } })
    } else {
      await useApi.post(path, {
        unidadeMedida: form.unidadeMedida,
        descricao: form.descricao,
        fator: form.fator
      })
    }
    toast.success(`Unidade ${editando.value ? 'editada' : 'cadastrada'} com sucesso!`)
    dialogAberto.value = false
    await listaAtiva.value.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const itemParaExcluir = ref<Unidade | null>(null)
const excluindo = ref(false)

function solicitarExclusao(item: Unidade) {
  itemParaExcluir.value = item
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi.delete(`${endpoint.value}/{id}`, { params: { id: itemParaExcluir.value.id } })
    toast.success('Registro excluído!')
    itemParaExcluir.value = null
    await listaAtiva.value.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

function trocarAba(tipo: TipoUnidade) {
  abaAtiva.value = tipo
}

watch(abaAtiva, () => {
  if (listaAtiva.value.itens.value.length === 0) listaAtiva.value.buscar()
})

onMounted(() => listaComercial.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Cadastro de Unidades" subtitle="Unidades de medida comerciais e tributáveis" :loading="listaAtiva.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Adicionar</button>
      </template>
    </PageToolbar>

    <div class="tabs">
      <button
        type="button"
        class="tab-btn"
        :class="{ active: abaAtiva === 'comercial' }"
        @click="trocarAba('comercial')"
      >
        Comercial
      </button>
      <button
        type="button"
        class="tab-btn"
        :class="{ active: abaAtiva === 'tributavel' }"
        @click="trocarAba('tributavel')"
      >
        Tributável
      </button>
    </div>

    <DataTable
      :items="listaAtiva.itens.value"
      :columns="colunas"
      :total="listaAtiva.total.value"
      :page="listaAtiva.pagina.value"
      :page-size="listaAtiva.tamanhoPagina.value"
      :loading="listaAtiva.carregando.value"
      :sort="listaAtiva.ordenacao.value"
      empty-text="Nenhuma unidade cadastrada."
      @update:page="listaAtiva.irParaPagina($event)"
      @update:page-size="listaAtiva.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="listaAtiva.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click="abrirEdicao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="solicitarExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogAberto" :title="editando ? 'Editar Unidade' : 'Nova Unidade'" width="480px">
      <form class="form-grid" @submit.prevent="salvar">
        <TextField v-model="form.unidadeMedida" label="Sigla" required maxlength="6" :error="erros.unidadeMedida" />
        <TextField v-model="form.descricao" label="Descrição" required :error="erros.descricao" />
        <TextField v-model.number="form.fator" label="Fator" type="number" required :error="erros.fator" />
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
      :model-value="!!itemParaExcluir"
      :item-label="itemParaExcluir?.unidadeMedida"
      :loading="excluindo"
      @update:model-value="itemParaExcluir = null"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

<style scoped>
.form-grid { display: flex; flex-direction: column; gap: 12px; }
.tabs { display: flex; gap: 8px; margin: 0 12px 12px; }
.tab-btn {
  padding: 8px 16px;
  font-size: 13px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-secondary);
  cursor: pointer;
}
.tab-btn.active {
  background: var(--primary);
  color: #fff;
  border-color: var(--primary);
}
</style>
