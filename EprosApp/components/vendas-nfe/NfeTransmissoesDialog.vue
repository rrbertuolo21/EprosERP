<script setup lang="ts">
/**
 * NfeTransmissoesDialog — lista das últimas vendas fiscais para abrir/editar
 * (porta ListaTransmissaoNfeDialog do legado).
 *
 * Consulta `vendas-fiscal` (listagem paginada) com filtro simples por situação. Emite
 * `abrir` com o id da venda para a página navegar até a edição.
 */
import { ref, watch } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import AppDialog from '~/components/shared/AppDialog.vue'

interface VendaResumo {
  id: string
  numero?: string | null
  serie?: string | null
  situacao?: string | null
  destinatarioNome?: string | null
  dataEmissao?: string | null
  valorNotaFiscal?: number | null
}

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  abrir: [id: string]
}>()

const { formatarData, formatarMoeda } = useHelper()

const itens = ref<VendaResumo[]>([])
const carregando = ref(false)
const erro = ref<string | null>(null)
const filtroSituacao = ref('')

watch(
  () => props.modelValue,
  (aberto) => {
    if (aberto) buscar()
  }
)

async function buscar() {
  carregando.value = true
  erro.value = null
  try {
    const query: Record<string, unknown> = { pagina: 1, tamanhoPagina: 20 }
    if (filtroSituacao.value) query.status = filtroSituacao.value
    const resp = await useApi('/vendas-fiscal', { query })
    itens.value = extrairLista<VendaResumo>(resp) ?? []
  } catch (e) {
    erro.value = obterMensagemErro(e)
    itens.value = []
  } finally {
    carregando.value = false
  }
}

function classeSituacao(s?: string | null): string {
  const v = (s ?? '').toLowerCase()
  if (v.includes('autoriz')) return 'chip chip-success'
  if (v.includes('cancel')) return 'chip chip-danger'
  if (v.includes('rejeit')) return 'chip chip-warning'
  return 'chip'
}

function abrir(id: string) {
  emit('abrir', id)
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Vendas / Transmissões"
    width="880px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="tr-filtros">
      <select v-model="filtroSituacao" class="select" @change="buscar">
        <option value="">Todas as situações</option>
        <option value="Rascunho">Rascunho</option>
        <option value="Autorizada">Autorizada</option>
        <option value="Cancelada">Cancelada</option>
        <option value="Rejeitada">Rejeitada</option>
      </select>
      <button type="button" class="btn btn-secondary btn-sm" :disabled="carregando" @click="buscar">Atualizar</button>
    </div>

    <span v-if="erro" class="field-error">{{ erro }}</span>

    <div class="table-wrap tr-tabela">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Número</th>
            <th>Série</th>
            <th>Destinatário</th>
            <th>Emissão</th>
            <th class="td-right">Valor</th>
            <th>Situação</th>
            <th class="td-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="carregando">
            <td colspan="7" class="table-loading"><span class="spinner"></span> Carregando...</td>
          </tr>
          <tr v-else-if="itens.length === 0">
            <td colspan="7" class="table-empty">Nenhuma venda encontrada.</td>
          </tr>
          <tr v-for="v in itens" v-else :key="v.id">
            <td>{{ v.numero ?? '—' }}</td>
            <td>{{ v.serie ?? '—' }}</td>
            <td>{{ v.destinatarioNome ?? '—' }}</td>
            <td>{{ v.dataEmissao ? formatarData(v.dataEmissao) : '—' }}</td>
            <td class="td-right">{{ formatarMoeda(v.valorNotaFiscal ?? 0) }}</td>
            <td><span :class="classeSituacao(v.situacao)">{{ v.situacao ?? 'Rascunho' }}</span></td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" @click="abrir(v.id)">Abrir</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="emit('update:modelValue', false)">Fechar</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.tr-filtros { display: flex; gap: 10px; align-items: center; margin-bottom: 14px; }
.tr-filtros .select { max-width: 240px; }
.tr-tabela { max-height: 420px; overflow-y: auto; }
</style>
