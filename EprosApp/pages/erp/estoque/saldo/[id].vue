<script setup lang="ts">
/**
 * Detalhe do Saldo de Estoque (erp/estoque/saldo/[id]).
 * `GET /estoque-produtos/{id}` (ObterEstoqueProdutoPorIdQuery). Consulta somente leitura:
 * a parametrização de mínimo/máximo/custeio é feita em Análise/Planejamento (APE-012).
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums } from '~/composables/useEstoqueEnums'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface SaldoEstoqueDetalhe {
  id: string
  empresaId: string
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  valorSaldo: number
  valorCustoMedio: number
  tipoCusteioEstoque: number
}

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarMoeda, formatarNumero } = useHelper()
const { tipoCusteio } = useEstoqueEnums()

const idParam = computed(() => route.params.id as string)
const carregando = ref(false)
const registro = ref<SaldoEstoqueDetalhe | null>(null)

const disponivel = computed(() =>
  registro.value ? (registro.value.quantidadeSaldoEstoque ?? 0) - (registro.value.quantidadeEstoqueReservado ?? 0) : 0
)

async function carregar() {
  carregando.value = true
  try {
    const resp = await useApi('/estoque-produtos/{id}', { params: { id: idParam.value } })
    registro.value = extrairDados<SaldoEstoqueDetalhe>(resp) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function voltar() {
  router.push('/erp/estoque/saldo')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <PageToolbar title="Detalhe do saldo" subtitle="Posição de estoque do produto" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button type="button" class="btn btn-primary" @click="navigateTo('/erp/estoque/analise')">Parametrizar (Análise)</button>
      </template>
    </PageToolbar>

    <div v-if="registro" class="glass-panel form-panel">
      <div class="cards-grid">
        <div class="card-metrica">
          <span class="metrica-label">Saldo em estoque</span>
          <span class="metrica-valor">{{ formatarNumero(registro.quantidadeSaldoEstoque, 0, 4) }}</span>
        </div>
        <div class="card-metrica">
          <span class="metrica-label">Reservado</span>
          <span class="metrica-valor">{{ formatarNumero(registro.quantidadeEstoqueReservado, 0, 4) }}</span>
        </div>
        <div class="card-metrica destaque">
          <span class="metrica-label">Disponível</span>
          <span class="metrica-valor">{{ formatarNumero(disponivel, 0, 4) }}</span>
        </div>
        <div class="card-metrica">
          <span class="metrica-label">Custo médio</span>
          <span class="metrica-valor">{{ formatarMoeda(registro.valorCustoMedio) }}</span>
        </div>
        <div class="card-metrica">
          <span class="metrica-label">Valor do saldo</span>
          <span class="metrica-valor">{{ formatarMoeda(registro.valorSaldo) }}</span>
        </div>
      </div>

      <div class="dados-grid">
        <div><span class="dado-label">Produto (ID)</span><span class="dado-valor">{{ registro.produtoId }}</span></div>
        <div><span class="dado-label">Empresa (ID)</span><span class="dado-valor">{{ registro.empresaId }}</span></div>
        <div><span class="dado-label">Estoque mínimo</span><span class="dado-valor">{{ formatarNumero(registro.quantidadeEstoqueMinimo, 0, 4) }}</span></div>
        <div><span class="dado-label">Estoque máximo</span><span class="dado-valor">{{ formatarNumero(registro.quantidadeEstoqueMaximo, 0, 4) }}</span></div>
        <div><span class="dado-label">Tipo de custeio</span><span class="dado-valor">{{ tipoCusteio.label(registro.tipoCusteioEstoque) }}</span></div>
      </div>
    </div>

    <div v-else-if="!carregando" class="glass-panel form-panel">Saldo não encontrado.</div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; margin-bottom: 20px; }
.card-metrica { display: flex; flex-direction: column; gap: 6px; padding: 14px 16px; border-radius: 10px; border: 1px solid var(--border-color); background: rgba(255, 255, 255, 0.03); }
.card-metrica.destaque { border-color: rgba(34, 197, 94, 0.4); }
.metrica-label { font-size: 12px; color: var(--text-muted); }
.metrica-valor { font-size: 20px; font-weight: 700; }
.dados-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 14px; }
.dado-label { display: block; font-size: 12px; color: var(--text-muted); }
.dado-valor { font-size: 14px; }
</style>
