<script setup lang="ts">
/**
 * Não Conformidades ativas (legado QLD) — Qualidade / Não Conformidades.
 *
 * Fonte: GET /qualidade/nao-conformidades/ativas (lista simples de NCs abertas,
 * geradas a partir de inspeções reprovadas) + POST /qualidade/nao-conformidades/tratar
 * (registra causa raiz + plano de ação). Sem CRUD completo — leitura + ação "Tratar".
 *
 * Obs.: o endpoint POST /qualidade/ncr/tratar espelha esta mesma ação (mesmo command
 * TratarNaoConformidade); mantemos o tratamento aqui, na entidade dona.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface NaoConformidade {
  id: string
  sku?: string | null
  titulo?: string | null
  descricao?: string | null
  status?: string | null
  criadoEm?: string | null
}

const toast = useToast()

const lista = useApiList<NaoConformidade>('/qualidade/nao-conformidades/ativas', {
  tamanhoPaginaInicial: 50
})

const colunas: DataTableColumn<NaoConformidade>[] = [
  { key: 'sku', label: 'SKU', sortable: false, width: '160px' },
  { key: 'titulo', label: 'Título', sortable: false },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

// ---- Dialog de tratamento ----
const dialogVisivel = ref(false)
const salvando = ref(false)
const ncAtual = ref<NaoConformidade | null>(null)

interface TratarForm {
  causaRaiz: string | null
  planoAcao: string | null
  resolvidoPor: string | null
}

const form = reactive<TratarForm>({ causaRaiz: null, planoAcao: null, resolvidoPor: null })
const erros = reactive<Record<string, string>>({})

function abrirTratamento(nc: NaoConformidade) {
  ncAtual.value = nc
  form.causaRaiz = null
  form.planoAcao = null
  form.resolvidoPor = null
  for (const k of Object.keys(erros)) delete erros[k]
  dialogVisivel.value = true
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.causaRaiz) erros.causaRaiz = 'A causa raiz é obrigatória.'
  if (!form.planoAcao) erros.planoAcao = 'O plano de ação é obrigatório.'
  if (!form.resolvidoPor) erros.resolvidoPor = 'O responsável pela resolução é obrigatório.'
  return Object.keys(erros).length === 0
}

async function confirmarTratamento() {
  if (!ncAtual.value || !validar()) return
  salvando.value = true
  try {
    await useApi('/qualidade/nao-conformidades/tratar', {
      method: 'POST',
      body: {
        naoConformidadeId: ncAtual.value.id,
        causaRaiz: form.causaRaiz,
        planoAcao: form.planoAcao,
        resolvidoPor: form.resolvidoPor
      }
    })
    toast.success('Não conformidade tratada com sucesso!')
    dialogVisivel.value = false
    ncAtual.value = null
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
    <PageToolbar title="Não Conformidades ativas" subtitle="NCs abertas aguardando tratamento (causa raiz + plano de ação)" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma não conformidade ativa."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrirTratamento(row)">Tratar</button>
      </template>
    </DataTable>

    <AppDialog v-model="dialogVisivel" title="Tratar não conformidade" width="640px" persistent>
      <div class="dialog-form">
        <p v-if="ncAtual" class="dialog-info">
          <strong>{{ ncAtual.sku }}</strong> — {{ ncAtual.titulo }}
        </p>
        <div class="field">
          <label class="field-label">Causa raiz<span class="required">*</span></label>
          <textarea v-model="form.causaRaiz" class="input textarea" rows="3" :class="{ 'is-invalid': !!erros.causaRaiz }"></textarea>
          <span v-if="erros.causaRaiz" class="field-error">{{ erros.causaRaiz }}</span>
        </div>
        <div class="field">
          <label class="field-label">Plano de ação<span class="required">*</span></label>
          <textarea v-model="form.planoAcao" class="input textarea" rows="3" :class="{ 'is-invalid': !!erros.planoAcao }"></textarea>
          <span v-if="erros.planoAcao" class="field-error">{{ erros.planoAcao }}</span>
        </div>
        <TextField v-model="form.resolvidoPor" label="Resolvido por" required maxlength="150" :error="erros.resolvidoPor" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="dialogVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmarTratamento">
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
