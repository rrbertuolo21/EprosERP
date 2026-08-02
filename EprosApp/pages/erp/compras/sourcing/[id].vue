<script setup lang="ts">
/**
 * Mapa comparativo de cotação (erp/compras/sourcing/[id]).
 *
 * Camada de apresentação sobre `CotacoesCompraController`:
 *   - GET {id}/mapa-comparativo → matriz produto × fornecedor (preço/prazo/condição), destacando
 *     o melhor preço por produto;
 *   - POST {id}/selecionar-vencedor → escolhe o fornecedor vencedor.
 *
 * Endpoints: cotacoes-compra/{id}/mapa-comparativo (GET), cotacoes-compra/{id}/selecionar-vencedor (POST).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface Proposta {
  cotacaoFornecedorId: string
  fornecedorId: string | null
  prazoEntrega: number | null
  condicoesPagamento: string | null
  quantidade: number | null
  valorUnitario: number | null
  valorTotal: number | null
  melhorPreco: boolean
}
interface LinhaMapa {
  produtoId: string
  melhorPrecoFornecedorId: string | null
  melhorValorUnitario: number | null
  propostas: Proposta[]
}
interface FornecedorMapa {
  id: string
  fornecedorId: string | null
  prazoEntrega: number | null
  condicoesPagamento: string | null
  subtotal: number | null
  desconto: number | null
  total: number | null
}
interface MapaComparativo {
  id: string
  descricao: string | null
  situacao: string | null
  fornecedorVencedorId: string | null
  decididaEm: string | null
  fornecedores: FornecedorMapa[]
  linhas: LinhaMapa[]
}

const route = useRoute()
const toast = useToast()
const { formatarMoeda } = useHelper()

const cotacaoId = computed(() => String(route.params.id))
const carregando = ref(false)
const selecionando = ref(false)
const mapa = ref<MapaComparativo | null>(null)
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

async function carregar() {
  carregando.value = true
  try {
    const resposta = await useApi('/cotacoes-compra/{id}/mapa-comparativo', { params: { id: cotacaoId.value } })
    mapa.value = extrairDados<MapaComparativo>(resposta) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

/** Rótulo curto do fornecedor (o mapa não traz nome — usa fragmento do id). */
function rotuloFornecedor(f: FornecedorMapa): string {
  const id = f.fornecedorId ?? f.id
  return `Forn. ${String(id).slice(0, 8)}`
}

/** Proposta de um fornecedor para a linha (produto), casando por cotacaoFornecedorId. */
function propostaDe(linha: LinhaMapa, forn: FornecedorMapa): Proposta | undefined {
  return linha.propostas.find((p) => p.cotacaoFornecedorId === forn.id)
}

const vencedorId = computed(() => mapa.value?.fornecedorVencedorId ?? null)
const decidida = computed(() => vencedorId.value != null)

async function selecionar(forn: FornecedorMapa) {
  if (decidida.value) return
  const ok = await confirmRef.value?.open(
    'Selecionar vencedor',
    `Confirmar ${rotuloFornecedor(forn)} como fornecedor vencedor desta cotação? Esta decisão encerra o comparativo.`,
    { textoConfirmar: 'Selecionar', textoCancelar: 'Voltar' }
  )
  if (!ok) return
  selecionando.value = true
  try {
    await useApi('/cotacoes-compra/{id}/selecionar-vencedor', {
      method: 'POST',
      params: { id: cotacaoId.value },
      body: { fornecedorId: forn.fornecedorId }
    })
    toast.success('Fornecedor vencedor selecionado')
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    selecionando.value = false
  }
}

function ehVencedor(forn: FornecedorMapa): boolean {
  return vencedorId.value != null && forn.fornecedorId === vencedorId.value
}

function voltar() {
  navigateTo('/erp/compras/sourcing')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar
      :title="mapa?.descricao ? `Mapa Comparativo · ${mapa.descricao}` : 'Mapa Comparativo'"
      subtitle="Sourcing"
      :loading="carregando"
    >
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
      </template>
    </PageToolbar>

    <div v-if="decidida" class="aviso-decidida glass-panel">
      Cotação <strong>decidida</strong>. Fornecedor vencedor selecionado.
    </div>

    <section v-if="mapa && mapa.fornecedores.length" class="glass-panel bloco tabela-wrap">
      <table class="mapa">
        <thead>
          <tr>
            <th class="col-produto">Produto</th>
            <th v-for="f in mapa.fornecedores" :key="f.id" class="col-forn" :class="{ vencedor: ehVencedor(f) }">
              <div class="forn-head">
                <span class="forn-nome">{{ rotuloFornecedor(f) }}</span>
                <span class="forn-meta">
                  <template v-if="f.prazoEntrega != null">{{ f.prazoEntrega }} dia(s)</template>
                  <template v-if="f.condicoesPagamento"> · {{ f.condicoesPagamento }}</template>
                </span>
                <button
                  type="button"
                  class="btn btn-sm"
                  :class="ehVencedor(f) ? 'btn-primary' : 'btn-secondary'"
                  :disabled="decidida || selecionando"
                  @click="selecionar(f)"
                >
                  {{ ehVencedor(f) ? 'Vencedor' : 'Selecionar' }}
                </button>
              </div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="linha in mapa.linhas" :key="linha.produtoId">
            <td class="col-produto">
              <span class="produto-id">Produto {{ String(linha.produtoId).slice(0, 8) }}</span>
            </td>
            <td
              v-for="f in mapa.fornecedores"
              :key="f.id"
              class="cel-proposta"
              :class="{ melhor: propostaDe(linha, f)?.melhorPreco }"
            >
              <template v-if="propostaDe(linha, f)">
                <span class="preco">{{ formatarMoeda(propostaDe(linha, f)?.valorUnitario ?? null) }}</span>
                <span class="detalhe">
                  qtd {{ propostaDe(linha, f)?.quantidade ?? '-' }} ·
                  total {{ formatarMoeda(propostaDe(linha, f)?.valorTotal ?? null) }}
                </span>
              </template>
              <span v-else class="sem-proposta">—</span>
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td class="col-produto">Total</td>
            <td v-for="f in mapa.fornecedores" :key="f.id" class="cel-total">
              {{ formatarMoeda(f.total ?? null) }}
            </td>
          </tr>
        </tfoot>
      </table>
    </section>

    <div v-else-if="!carregando" class="glass-panel bloco vazio">
      Esta cotação ainda não possui fornecedores/itens para comparar.
    </div>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.bloco { margin-bottom: 16px; }
.tabela-wrap { padding: 0; overflow-x: auto; }
.aviso-decidida {
  padding: 12px 16px;
  margin-bottom: 16px;
  border-left: 3px solid var(--success);
  color: var(--text-secondary);
  font-size: 14px;
}
.mapa {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
  min-width: 640px;
}
.mapa th,
.mapa td {
  padding: 12px 14px;
  border-bottom: 1px solid var(--border-color, rgba(255, 255, 255, 0.08));
  text-align: left;
  vertical-align: top;
}
.col-produto { min-width: 160px; font-weight: 600; }
.col-forn.vencedor { background: rgba(16, 185, 129, 0.08); }
.forn-head { display: flex; flex-direction: column; gap: 6px; align-items: flex-start; }
.forn-nome { font-weight: 700; }
.forn-meta { font-size: 11px; color: var(--text-secondary); }
.produto-id { font-variant-numeric: tabular-nums; }
.cel-proposta { display: table-cell; }
.cel-proposta .preco { display: block; font-weight: 600; font-variant-numeric: tabular-nums; }
.cel-proposta .detalhe { display: block; font-size: 11px; color: var(--text-secondary); }
.cel-proposta.melhor {
  background: rgba(16, 185, 129, 0.12);
  border-radius: 6px;
}
.cel-proposta.melhor .preco { color: var(--success); }
.sem-proposta { color: var(--text-secondary); }
.cel-total { font-weight: 700; font-variant-numeric: tabular-nums; }
.vazio { padding: 24px; text-align: center; color: var(--text-secondary); }
</style>
