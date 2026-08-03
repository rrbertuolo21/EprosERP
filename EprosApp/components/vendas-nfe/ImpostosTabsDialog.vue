<script setup lang="ts">
/**
 * ImpostosTabsDialog — detalhamento de impostos de um item da NF-e (componente P0).
 *
 * Porta o comportamento de `components/nfe/ImpostosTabsDialog.vue` do legado: abas de
 * detalhamento tributário por item (Totais, IBS/CBS, ICMS, IPI, ST, PIS/COFINS, ICMS
 * Interestadual/DIFAL, FCP, Outros), consulta de valor aproximado de tributos (IBPT) por
 * NCM/UF e cópia dos dados de imposto para a área de transferência (apoio ao suporte).
 *
 * Porte de comportamento, não markup Vuetify: os campos e fórmulas exibidas são fiéis ao
 * legado; a UI usa os componentes compartilhados do design novo (`AppDialog`, `TextField`,
 * `MoneyInput`, `PercentInput`) e tabs simples em vez de `VTabs`/`VTabsWindow`.
 *
 * A aba "Importação" (slot `tab-entrada` no legado, usada em Devolução/Retorno de compra)
 * é preservada como slot nomeado `tab-entrada`.
 */
import { computed, ref, watch } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import { useEnum, type SelectOption } from '~/composables/useEnum'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { criarItemNfeDadosFiscaisVazio, type ItemNfeDadosFiscais } from './useImpostosItem'

type CstPisCofinsModo = 'entrada' | 'saida'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    produto: ItemNfeDadosFiscais
    /** UF do emitente, para a consulta de valor aproximado IBPT. */
    ufEmitente?: string | null
    cstPisCofinsModo?: CstPisCofinsModo
  }>(),
  { ufEmitente: null, cstPisCofinsModo: 'saida' }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'update:produto': [value: ItemNfeDadosFiscais]
}>()

const { formatarMoeda } = useHelper()
const toast = useToast()
const { carregarOpcoes } = useEnum()

type Aba =
  | 'TOTAIS'
  | 'IBS_CBS'
  | 'ICMS'
  | 'IPI'
  | 'ST'
  | 'PIS_COFINS'
  | 'ICMS_INTER'
  | 'FCP'
  | 'OUTROS'
  | 'ENTRADA'

const abaAtiva = ref<Aba>('TOTAIS')
const cstPisCofinsOpcoes = ref<SelectOption[]>([])
const valorIbpt = ref<number | null>(null)
const aliquotaIbpt = ref<number | null>(null)
const carregandoIbpt = ref(false)

const produtoLocal = computed<ItemNfeDadosFiscais>({
  get: () => props.produto ?? criarItemNfeDadosFiscaisVazio(),
  set: (valor) => emit('update:produto', valor)
})

function atualizarCampoImposto<K extends keyof ItemNfeDadosFiscais['imposto']>(
  campo: K,
  valor: ItemNfeDadosFiscais['imposto'][K]
): void {
  emit('update:produto', {
    ...produtoLocal.value,
    imposto: { ...produtoLocal.value.imposto, [campo]: valor }
  })
}

function atualizarCampoProduto<K extends keyof ItemNfeDadosFiscais>(
  campo: K,
  valor: ItemNfeDadosFiscais[K]
): void {
  emit('update:produto', { ...produtoLocal.value, [campo]: valor })
}

/** CST PIS/COFINS único para entrada (mesmo código replicado em cstPis e cstCofins — regra do legado). */
const cstPisCofinsEntrada = computed<string | null>({
  get: () => produtoLocal.value.imposto.cstPis || null,
  set: (valor) => {
    const s = valor != null ? String(valor) : ''
    emit('update:produto', {
      ...produtoLocal.value,
      imposto: { ...produtoLocal.value.imposto, cstPis: s, cstCofins: s }
    })
  }
})

async function copiarImpostosParaClipboard(): Promise<void> {
  try {
    await navigator.clipboard.writeText(JSON.stringify(produtoLocal.value.imposto, null, 2))
    toast.success('Impostos copiados para a área de transferência')
  } catch (e) {
    console.error('[ImpostosTabsDialog] falha ao copiar impostos', e)
    toast.error('Erro ao copiar para a área de transferência')
  }
}

/** Busca o valor aproximado de tributos (Lei da Transparência / IBPT) para o NCM do item. */
async function buscarValorIbpt(): Promise<void> {
  if (!produtoLocal.value.ncm) {
    toast.warning('NCM não informado no produto')
    return
  }
  if (!props.ufEmitente) {
    toast.warning('UF do emitente não encontrada')
    return
  }

  const valorBase = produtoLocal.value.totalItem || 0
  const origem = produtoLocal.value.imposto.origem || '0'
  const ncm = produtoLocal.value.ncm

  carregandoIbpt.value = true
  valorIbpt.value = null
  aliquotaIbpt.value = null
  try {
    const resposta = await useApi<{ aliquota: number; valorImposto: number }>(
      `/ibpt-dfe/calcular-valor-aproximado/${ncm}/${props.ufEmitente}/${valorBase}/${origem}`
    )
    const dados = extrairDados<{ aliquota: number; valorImposto: number }>(resposta)
    if (dados) {
      valorIbpt.value = dados.valorImposto
      aliquotaIbpt.value = dados.aliquota
    } else {
      toast.error('Erro ao buscar valor IBPT')
    }
  } catch (e) {
    console.error('[ImpostosTabsDialog] falha ao buscar IBPT', e)
    toast.error('Erro ao buscar valor aproximado IBPT')
  } finally {
    carregandoIbpt.value = false
  }
}

watch(
  () => props.cstPisCofinsModo,
  async (modo) => {
    const uri = modo === 'entrada' ? 'fiscais-enums/cst-pis-cofins-entrada' : 'fiscais-enums/cst-pis-cofins-saida'
    cstPisCofinsOpcoes.value = await carregarOpcoes(uri)
  },
  { immediate: true }
)

function fechar(): void {
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    title="Cálculo de Impostos"
    width="960px"
    @update:model-value="emit('update:modelValue', $event)"
  >
    <template #title>
      <div class="impostos-titulo">
        <span class="text-h5">Cálculo de Impostos</span>
        <button type="button" class="btn-icon" title="Copiar impostos para clipboard" @click="copiarImpostosParaClipboard">
          copiar
        </button>
      </div>
    </template>

    <div class="impostos-tabs">
      <button
        v-for="aba in (['TOTAIS', 'IBS_CBS', 'ICMS', 'IPI', 'ST', 'PIS_COFINS', 'ICMS_INTER', 'FCP', 'OUTROS'] as Aba[])"
        :key="aba"
        type="button"
        class="impostos-tab"
        :class="{ ativo: abaAtiva === aba }"
        @click="abaAtiva = aba"
      >
        {{ aba.replace('_', ' / ') }}
      </button>
      <button
        v-if="!!$slots['tab-entrada']"
        type="button"
        class="impostos-tab"
        :class="{ ativo: abaAtiva === 'ENTRADA' }"
        @click="abaAtiva = 'ENTRADA'"
      >
        Importação
      </button>
    </div>

    <!-- Aba TOTAIS -->
    <div v-if="abaAtiva === 'TOTAIS'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <thead>
          <tr>
            <td></td>
            <th>Base</th>
            <th>Alíquota</th>
            <th>Imposto</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="td-label">ICMS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoIcms" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaIcms" readonly /></td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIcms" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">IPI</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoIpi" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaIpi" readonly /></td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIpi" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">ST</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoStIcms" readonly /></td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaStIcms"
                @update:model-value="atualizarCampoImposto('aliquotaStIcms', $event ?? 0)"
              />
            </td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoRecolherStIcms" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">PIS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoPis" readonly /></td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaPercetualPis"
                @update:model-value="atualizarCampoImposto('aliquotaPercetualPis', $event ?? 0)"
              />
            </td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoPis" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">COFINS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoCofins" readonly /></td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaPercetualCofins"
                @update:model-value="atualizarCampoImposto('aliquotaPercetualCofins', $event ?? 0)"
              />
            </td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoCofins" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">Valor Aprox.</td>
            <td><TextField :model-value="produtoLocal.ncm" label="NCM" readonly /></td>
            <td><PercentInput :model-value="aliquotaIbpt ?? 0" readonly /></td>
            <td>
              <MoneyInput :model-value="valorIbpt ?? 0" readonly />
              <button type="button" class="btn btn-secondary btn-sm" :disabled="carregandoIbpt" @click="buscarValorIbpt">
                {{ carregandoIbpt ? 'Buscando...' : 'Buscar IBPT' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba IBS/CBS -->
    <div v-else-if="abaAtiva === 'IBS_CBS'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <thead>
          <tr><td>Tributo</td><th>Base</th><th>Alíquota</th><th>% Redução</th><th>Imposto</th></tr>
        </thead>
        <tbody>
          <tr>
            <td class="td-label">CBS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.baseDeCalculoIbsCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaCbsReducaoIbsCbs" readonly /></td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoCbsIbsCbs" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">IBS Estadual</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.baseDeCalculoIbsCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaEstadualIbsCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaEstadualReducaoIbsCbs" readonly /></td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoEstadualIbsCbs" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">IBS Municipal</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.baseDeCalculoIbsCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaMunicipalIbsCbs" readonly /></td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaMunicipalReducaoIbsCbs" readonly /></td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoMunicipalIbsCbs" readonly /></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba ICMS -->
    <div v-else-if="abaAtiva === 'ICMS'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <tbody>
          <tr>
            <td class="td-label">Total Produto</td>
            <td><MoneyInput :model-value="produtoLocal.totalItem" readonly /></td>
            <td class="td-icon">+</td>
            <td><MoneyInput :model-value="produtoLocal.ipiEmbutido ? produtoLocal.imposto.valorImpostoDevidoIpi : 0" readonly /></td>
            <td class="td-icon">+</td>
            <td><MoneyInput :model-value="produtoLocal.valorFreteRateado" readonly /></td>
            <td class="td-icon">+</td>
            <td><MoneyInput :model-value="produtoLocal.valorSeguroRateado" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">Base de Cálculo</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoIcms" readonly /></td>
            <td class="td-icon">×</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaIcms"
                @update:model-value="atualizarCampoImposto('aliquotaIcms', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIcms" readonly /></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba IPI -->
    <div v-else-if="abaAtiva === 'IPI'" class="impostos-conteudo form-grid impostos-grid-3">
      <MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoIpi" label="Base Cálculo IPI" readonly />
      <PercentInput
        :model-value="produtoLocal.imposto.aliquotaIpi"
        label="Alíquota IPI (%)"
        @update:model-value="atualizarCampoImposto('aliquotaIpi', $event ?? 0)"
      />
      <MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIpi" label="Valor IPI" readonly />
    </div>

    <!-- Aba ST -->
    <div v-else-if="abaAtiva === 'ST'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <tbody>
          <tr>
            <td class="td-label">Sub Total 1</td>
            <td><MoneyInput :model-value="produtoLocal.totalItem + produtoLocal.imposto.valorImpostoDevidoIpi" readonly /></td>
            <td class="td-icon">+</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaMvaIcms"
                label="% IVA"
                @update:model-value="atualizarCampoImposto('aliquotaMvaIcms', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoStIcms" label="Base de Cálc. ST" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">ICMS-ST</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoStIcms" readonly /></td>
            <td class="td-icon">×</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaStIcms"
                label="Aliq. ST (%)"
                @update:model-value="atualizarCampoImposto('aliquotaStIcms', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoStIcms" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">ICMS Próprio</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoIcms" readonly /></td>
            <td class="td-icon">×</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaIcms"
                label="Aliq. ICMS (%)"
                @update:model-value="atualizarCampoImposto('aliquotaIcms', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIcms" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">Imposto ST</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoStIcms" readonly /></td>
            <td class="td-icon">-</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoIcms" readonly /></td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoRecolherStIcms" readonly /></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba PIS/COFINS -->
    <div v-else-if="abaAtiva === 'PIS_COFINS'" class="impostos-conteudo">
      <SelectField
        v-if="cstPisCofinsModo === 'entrada'"
        :model-value="cstPisCofinsEntrada"
        label="CST PIS COFINS"
        :options="cstPisCofinsOpcoes"
        @update:model-value="cstPisCofinsEntrada = $event as string"
      />
      <SelectField
        v-else
        :model-value="produtoLocal.imposto.cstCofins"
        label="CST PIS COFINS"
        :options="cstPisCofinsOpcoes"
        @update:model-value="atualizarCampoImposto('cstCofins', String($event ?? ''))"
      />

      <table class="impostos-tabela mt-2">
        <tbody>
          <tr>
            <td class="td-label">PIS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoPis" label="Base de Cálculo" readonly /></td>
            <td class="td-icon">×</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaPercetualPis"
                @update:model-value="atualizarCampoImposto('aliquotaPercetualPis', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoPis" label="Vlr Imposto" readonly /></td>
          </tr>
          <tr>
            <td class="td-label">COFINS</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoCofins" label="Base de Cálculo" readonly /></td>
            <td class="td-icon">×</td>
            <td>
              <PercentInput
                :model-value="produtoLocal.imposto.aliquotaPercetualCofins"
                @update:model-value="atualizarCampoImposto('aliquotaPercetualCofins', $event ?? 0)"
              />
            </td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoCofins" label="Vlr Imposto" readonly /></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba ICMS em Operações Interestaduais (DIFAL) -->
    <div v-else-if="abaAtiva === 'ICMS_INTER'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <tbody>
          <tr>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaDifalInternaIcms" label="Aliq. Estado Destino" readonly /></td>
            <td class="td-icon">-</td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaDifalInterestadualIcms" label="Aliq. Interestadual" readonly /></td>
            <td class="td-icon">=</td>
            <td>
              <MoneyInput
                :model-value="produtoLocal.imposto.aliquotaDifalInternaIcms - produtoLocal.imposto.aliquotaDifalInterestadualIcms"
                label="Diferencial de Alíq."
                readonly
              />
            </td>
          </tr>
          <tr>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoDifalIcms" label="Base de Cálculo" readonly /></td>
            <td class="td-icon">×</td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaFcpIcms" label="Percentual FCP" readonly /></td>
            <td class="td-icon">=</td>
            <td>
              <MoneyInput
                :model-value="(produtoLocal.imposto.valorBaseDeCalculoDifalIcms * produtoLocal.imposto.aliquotaFcpIcms) / 100"
                label="Valor ao FCP"
                readonly
              />
            </td>
          </tr>
          <tr>
            <td>
              <MoneyInput
                :model-value="produtoLocal.imposto.valorImpostoDevidoDifalIcms + (produtoLocal.imposto.valorBaseDeCalculoDifalIcms * produtoLocal.imposto.aliquotaFcpIcms) / 100"
                label="ICMS Destino"
                readonly
              />
            </td>
            <td>
              <MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoFcp" label="ICMS Origem" readonly />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba FCP -->
    <div v-else-if="abaAtiva === 'FCP'" class="impostos-conteudo">
      <table class="impostos-tabela">
        <tbody>
          <tr>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoFcpIcms" label="Base de Cálculo ICMS" readonly /></td>
            <td class="td-icon">×</td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaFcpIcms" label="Percentual Fundo Combate" readonly /></td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoFcpIcms" label="Valor FCP ICMS" readonly /></td>
          </tr>
          <tr>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorBaseDeCalculoStFcpIcms" label="Base de Cálculo ST" readonly /></td>
            <td class="td-icon">×</td>
            <td><PercentInput :model-value="produtoLocal.imposto.aliquotaFcpStIcms" label="Percentual Fundo Combate" readonly /></td>
            <td class="td-icon">=</td>
            <td><MoneyInput :model-value="produtoLocal.imposto.valorImpostoDevidoRecolherFcpStIcms" label="Valor FCP ST" readonly /></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Aba OUTROS -->
    <div v-else-if="abaAtiva === 'OUTROS'" class="impostos-conteudo form-grid impostos-grid-2">
      <TextField
        :model-value="produtoLocal.numeroPedidoCompra ?? ''"
        label="Número do Pedido de Compra"
        @update:model-value="atualizarCampoProduto('numeroPedidoCompra', ($event as string) || null)"
      />
      <TextField
        :model-value="produtoLocal.numeroItemPedidoCompra != null ? String(produtoLocal.numeroItemPedidoCompra) : ''"
        label="Item do Pedido de Compra"
        @update:model-value="atualizarCampoProduto('numeroItemPedidoCompra', $event ? Number($event) : null)"
      />
      <TextField
        :model-value="produtoLocal.fichaConteudoImportacao ?? ''"
        label="Ficha Conteúdo Importação"
        @update:model-value="atualizarCampoProduto('fichaConteudoImportacao', ($event as string) || null)"
      />
    </div>

    <!-- Aba Importação (slot) -->
    <div v-else-if="abaAtiva === 'ENTRADA'" class="impostos-conteudo">
      <slot name="tab-entrada" :produto="produtoLocal"></slot>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Fechar</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.impostos-titulo { display: flex; align-items: center; justify-content: space-between; width: 100%; }
.impostos-tabs {
  display: flex; flex-wrap: wrap; gap: 4px; margin-bottom: 14px;
  border-bottom: 1px solid var(--border-color);
}
.impostos-tab {
  padding: 8px 12px; font-size: 12.5px; font-weight: 600; text-transform: uppercase;
  letter-spacing: 0.3px; color: var(--text-muted); background: transparent; border: none;
  border-bottom: 2px solid transparent; cursor: pointer;
}
.impostos-tab.ativo { color: var(--primary); border-bottom-color: var(--primary); }
.impostos-tab:hover { color: var(--text-primary); }

.impostos-conteudo { padding-top: 4px; }
.impostos-grid-3 { grid-template-columns: repeat(3, 1fr); gap: 14px; }
.impostos-grid-2 { grid-template-columns: repeat(2, 1fr); gap: 14px; }
@media (max-width: 720px) {
  .impostos-grid-3, .impostos-grid-2 { grid-template-columns: 1fr; }
}

.impostos-tabela { width: 100%; border-collapse: collapse; }
.impostos-tabela th, .impostos-tabela td { padding: 6px 8px; vertical-align: middle; }
.impostos-tabela thead th { font-size: 11px; text-transform: uppercase; color: var(--text-muted); text-align: left; }
.td-label { font-weight: 700; white-space: nowrap; width: 120px; }
.td-icon { width: 28px; text-align: center; color: var(--text-muted); }
.mt-2 { margin-top: 12px; }

.btn-icon {
  background: transparent; border: 1px solid var(--border-color); border-radius: 6px;
  padding: 4px 10px; font-size: 12px; cursor: pointer; color: var(--text-secondary);
}
.btn-sm { padding: 4px 10px; font-size: 12px; margin-top: 4px; }
</style>
