<script setup lang="ts">
/**
 * Listagem de Ordens de Manutenção — Manutenção / Ordens.
 * Fonte: GET /manutencao/ordens. Ações por linha (sem GET/{id}):
 *  - Concluir: POST /manutencao/ordens/{id}/concluir
 *  - Adicionar peça: POST /manutencao/ordens/{id}/pecas
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'

definePageMeta({ layout: 'default' })

interface OrdemManutencao {
  id: string
  tipo?: string | null
  descricaoProblema?: string | null
  status?: string | null
  responsavel?: string | null
  dataAbertura?: string | null
  custoMaoObra?: number | null
  custoPecas?: number | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<OrdemManutencao>('/manutencao/ordens', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<OrdemManutencao>[] = [
  { key: 'tipo', label: 'Tipo', sortable: true, width: '130px' },
  { key: 'descricaoProblema', label: 'Problema', sortable: false },
  { key: 'responsavel', label: 'Responsável', sortable: true },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'dataAbertura', label: 'Abertura', sortable: true, width: '140px' }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/ordens/novo')
}

// ---- Concluir OM ----
const concluirVisivel = ref(false)
const concluindo = ref(false)
const ordemAtual = ref<OrdemManutencao | null>(null)
const formConcluir = reactive<{ descricaoServicoExecutado: string; custoMaoObra: number }>({
  descricaoServicoExecutado: '',
  custoMaoObra: 0
})

function abrirConcluir(item: OrdemManutencao) {
  ordemAtual.value = item
  formConcluir.descricaoServicoExecutado = ''
  formConcluir.custoMaoObra = 0
  concluirVisivel.value = true
}

async function confirmarConcluir() {
  if (!ordemAtual.value) return
  concluindo.value = true
  try {
    await useApi(`/manutencao/ordens/${ordemAtual.value.id}/concluir`, { method: 'POST', body: formConcluir })
    toast.success('Ordem concluída com sucesso.')
    concluirVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    concluindo.value = false
  }
}

// ---- Adicionar peça ----
const pecaVisivel = ref(false)
const adicionandoPeca = ref(false)
const formPeca = reactive<{ produtoId: string; quantidade: number }>({ produtoId: '', quantidade: 1 })

function abrirPeca(item: OrdemManutencao) {
  ordemAtual.value = item
  formPeca.produtoId = ''
  formPeca.quantidade = 1
  pecaVisivel.value = true
}

async function confirmarPeca() {
  if (!ordemAtual.value) return
  if (!formPeca.produtoId) {
    toast.error('Informe o ID do produto.')
    return
  }
  adicionandoPeca.value = true
  try {
    await useApi(`/manutencao/ordens/${ordemAtual.value.id}/pecas`, { method: 'POST', body: formPeca })
    toast.success('Peça adicionada à ordem.')
    pecaVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    adicionandoPeca.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Ordens de manutenção" subtitle="Abertura, peças e conclusão de ordens de manutenção" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova ordem</button>
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
      empty-text="Nenhuma ordem de manutenção encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataAbertura="{ value }">
        <span>{{ formatarData(value) }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge" :class="value === 'Concluida' ? 'badge-success' : value === 'Cancelada' ? 'badge-danger' : 'badge-info'">{{ value }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Adicionar peça" @click.stop="abrirPeca(row)">Peça</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Concluir" @click.stop="abrirConcluir(row)">Concluir</button>
      </template>
    </DataTable>

    <AppDialog v-model="concluirVisivel" title="Concluir ordem" width="480px" persistent>
      <div class="dialog-form">
        <TextField v-model="formConcluir.descricaoServicoExecutado" label="Serviço executado" placeholder="Descreva o serviço realizado" />
        <MoneyInput v-model="formConcluir.custoMaoObra" label="Custo de mão de obra" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="concluindo" @click="concluirVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="concluindo" @click="confirmarConcluir">
          <span v-if="concluindo" class="spinner"></span>
          <span v-else>Concluir</span>
        </button>
      </template>
    </AppDialog>

    <AppDialog v-model="pecaVisivel" title="Adicionar peça" width="480px" persistent>
      <div class="dialog-form">
        <!-- TODO: produtoId sem endpoint de produtos no módulo — entrada por texto até integração. -->
        <TextField v-model="formPeca.produtoId" label="Produto (ID)" placeholder="UUID" required />
        <QuantityInput v-model="formPeca.quantidade" label="Quantidade" :min="0" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="adicionandoPeca" @click="pecaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="adicionandoPeca" @click="confirmarPeca">
          <span v-if="adicionandoPeca" class="spinner"></span>
          <span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-form { display: flex; flex-direction: column; gap: 16px; }
</style>
