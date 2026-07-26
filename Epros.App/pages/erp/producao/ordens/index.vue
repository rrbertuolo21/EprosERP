<script setup lang="ts">
/**
 * Listagem de Ordens de Produção (simples) — Produção / Ordens.
 * GET lista + POST criar + ações iniciar/apontar/encerrar (por linha). Sem GET/{id}/PUT/DELETE.
 * Fonte: ProducaoController (api/v1/producao/ordens) + ProducaoQueryHandlers.
 * As ações de workflow (iniciar/apontar/encerrar) são botões/diálogos por linha, pois não
 * existe endpoint de detalhe.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import { rotularOrdemSimples, classeBadgeStatus, formatarData, formatarQuantidade } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Ordem {
  id: string
  codigo?: string | null
  produtoAcabadoSku?: string | null
  quantidadePlanejada?: number | null
  quantidadeProduzida?: number | null
  quantidadeRefugada?: number | null
  status?: string | null
  dataAbertura?: string | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Ordem, Record<string, never>>('/producao/ordens', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Ordem>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'produtoAcabadoSku', label: 'SKU Produto' },
  { key: 'quantidadePlanejada', label: 'Planejada', align: 'right', width: '120px' },
  { key: 'quantidadeProduzida', label: 'Produzida', align: 'right', width: '120px' },
  { key: 'quantidadeRefugada', label: 'Refugada', align: 'right', width: '120px' },
  { key: 'status', label: 'Status', align: 'center', width: '130px' },
  { key: 'dataAbertura', label: 'Abertura', width: '150px' }
]

const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const acaoEmAndamento = ref(false)

// Diálogo de apontamento
const apontarVisivel = ref(false)
const ordemAlvo = ref<Ordem | null>(null)
const apontamento = ref({ quantidadeApontada: 0 as number, quantidadeRefugada: 0 as number, operador: '' as string })

function novo() { router.push('/erp/producao/ordens/novo') }

async function iniciar(item: Ordem) {
  const ok = await confirmRef.value?.open('Iniciar ordem?', `Iniciar a produção da ordem ${item.codigo ?? ''}?`, { textoConfirmar: 'Iniciar' })
  if (!ok) return
  await chamar(`/producao/ordens/${item.id}/iniciar`)
}

async function encerrar(item: Ordem) {
  const ok = await confirmRef.value?.open('Encerrar ordem?', `Encerrar a ordem ${item.codigo ?? ''}?`, { textoConfirmar: 'Encerrar', danger: true })
  if (!ok) return
  await chamar(`/producao/ordens/${item.id}/encerrar`)
}

function abrirApontamento(item: Ordem) {
  ordemAlvo.value = item
  apontamento.value = { quantidadeApontada: 0, quantidadeRefugada: 0, operador: '' }
  apontarVisivel.value = true
}

async function confirmarApontamento() {
  if (!ordemAlvo.value) return
  acaoEmAndamento.value = true
  try {
    await useApi(`/producao/ordens/${ordemAlvo.value.id}/apontar`, {
      method: 'POST',
      body: {
        quantidadeApontada: apontamento.value.quantidadeApontada,
        quantidadeRefugada: apontamento.value.quantidadeRefugada,
        operador: apontamento.value.operador || null
      }
    })
    toast.success('Apontamento registrado.')
    apontarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

async function chamar(rota: string) {
  acaoEmAndamento.value = true
  try {
    await useApi(rota, { method: 'POST' })
    toast.success('Ação executada com sucesso.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    acaoEmAndamento.value = false
  }
}

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Ordens de Produção" subtitle="Ordens de produção simples e apontamentos" :loading="lista.carregando.value">
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
      empty-text="Nenhuma ordem de produção encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-quantidadePlanejada="{ value }">{{ formatarQuantidade(value as number) }}</template>
      <template #cell-quantidadeProduzida="{ value }">{{ formatarQuantidade(value as number) }}</template>
      <template #cell-quantidadeRefugada="{ value }">{{ formatarQuantidade(value as number) }}</template>
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotularOrdemSimples(value as string))">{{ rotularOrdemSimples(value as string) }}</span>
      </template>
      <template #cell-dataAbertura="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="acaoEmAndamento" @click.stop="iniciar(row)">Iniciar</button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="acaoEmAndamento" @click.stop="abrirApontamento(row)">Apontar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="acaoEmAndamento" @click.stop="encerrar(row)">Encerrar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />

    <AppDialog v-model="apontarVisivel" title="Registrar apontamento" width="480px">
      <div class="form-grid">
        <QuantityInput v-model="apontamento.quantidadeApontada" label="Quantidade apontada" />
        <QuantityInput v-model="apontamento.quantidadeRefugada" label="Quantidade refugada" />
        <TextField v-model="apontamento.operador" label="Operador" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="apontarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="acaoEmAndamento" @click="confirmarApontamento">Registrar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
</style>
