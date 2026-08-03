<script setup lang="ts">
/**
 * Inspeções de Lote pendentes (QLD) — Qualidade / Inspeções.
 *
 * Fonte: GET /qualidade/inspecoes/pendentes (lista simples de lotes pendentes) +
 * POST /qualidade/inspecoes/registrar (registra Aprovado/Reprovado). Não há CRUD
 * completo: a criação de lotes vem de outro fluxo (Compras/Estoque). Esta tela é
 * de leitura + a ação "Registrar resultado" por linha.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface InspecaoLote {
  id: string
  sku?: string | null
  nomeProduto?: string | null
  quantidadeLote?: number | null
  status?: string | null
  criadoEm?: string | null
}

const toast = useToast()

const lista = useApiList<InspecaoLote>('/qualidade/inspecoes/pendentes', {
  tamanhoPaginaInicial: 50
})

const colunas: DataTableColumn<InspecaoLote>[] = [
  { key: 'sku', label: 'SKU', sortable: false, width: '160px' },
  { key: 'nomeProduto', label: 'Produto', sortable: false },
  { key: 'quantidadeLote', label: 'Qtd. lote', sortable: false, align: 'right', width: '120px' },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

// ---- Dialog de registro de resultado ----
const dialogVisivel = ref(false)
const salvando = ref(false)
const loteAtual = ref<InspecaoLote | null>(null)

const opcoesStatus: SelectOption[] = [
  { value: 'Aprovado', label: 'Aprovado' },
  { value: 'Reprovado', label: 'Reprovado' }
]

interface RegistroForm {
  status: string | null
  responsavel: string | null
  observacoes: string | null
}

const form = reactive<RegistroForm>({ status: null, responsavel: null, observacoes: null })
const erros = reactive<Record<string, string>>({})

function abrirRegistro(lote: InspecaoLote) {
  loteAtual.value = lote
  form.status = null
  form.responsavel = null
  form.observacoes = null
  for (const k of Object.keys(erros)) delete erros[k]
  dialogVisivel.value = true
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.status) erros.status = 'Selecione o resultado.'
  if (!form.responsavel) erros.responsavel = 'O responsável é obrigatório.'
  return Object.keys(erros).length === 0
}

async function confirmarRegistro() {
  if (!loteAtual.value || !validar()) return
  salvando.value = true
  try {
    await useApi('/qualidade/inspecoes/registrar', {
      method: 'POST',
      body: {
        inspecaoLoteId: loteAtual.value.id,
        status: form.status,
        responsavel: form.responsavel,
        observacoes: form.observacoes
      }
    })
    toast.success('Resultado da inspeção registrado com sucesso!')
    dialogVisivel.value = false
    loteAtual.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Inspeções pendentes" subtitle="Lotes aguardando inspeção de qualidade" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma inspeção pendente."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirRegistro(row)">Registrar resultado</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogVisivel" title="Registrar resultado da inspeção" persistent>
      <div class="dialog-form">
        <p v-if="loteAtual" class="dialog-info">
          Lote: <strong>{{ loteAtual.sku }}</strong> — {{ loteAtual.nomeProduto }}
        </p>
        <SelectField v-model="form.status" label="Resultado" required :options="opcoesStatus" :clearable="false" :error="erros.status" />
        <TextField v-model="form.responsavel" label="Responsável" required maxlength="150" :error="erros.responsavel" />
        <div class="field">
          <label class="field-label">Observações</label>
          <textarea v-model="form.observacoes" class="input textarea" rows="3"></textarea>
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmarRegistro">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-form { display: flex; flex-direction: column; gap: 14px; }
.dialog-info { color: var(--text-secondary); font-size: 13.5px; margin: 0; }
.textarea { min-height: 72px; resize: vertical; font-family: inherit; }
</style>
