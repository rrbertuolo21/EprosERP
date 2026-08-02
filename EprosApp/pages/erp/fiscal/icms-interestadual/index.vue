<script setup lang="ts">
/**
 * Tabela Interestadual de ICMS — Fiscal / ICMS Interestadual.
 *
 * Porta o comportamento de `fiscal/icms-interestadual/index.vue` do legado: matriz UF origem
 * (linhas) x UF destino (colunas) com a alíquota interestadual de ICMS, mais dois seletores
 * de destaque (origem/destino) que realçam a linha/coluna correspondente na matriz — útil para
 * localizar rapidamente uma combinação específica. A tela é somente leitura (a alimentação da
 * tabela é feita via importação/seed no backend).
 *
 * Endpoints: `fiscal/icms-aliquotas-interestaduais` (matriz de alíquotas) e
 * `fiscal/fcp-aliquotas-uf` (percentual de Fundo de Combate à Pobreza por UF, exibido em coluna
 * auxiliar ao final da tabela).
 */
import { ref, reactive, computed, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface IcmsAliquotaInterestadual {
  id: string
  ufOrigem: string
  ufDestino: string
  valorAliquota: number
}

interface FcpAliquotaUf {
  id: string
  uf: string
  valorPercentual: number
}

const ufsBrasil = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]
const opcoesUf: SelectOption[] = ufsBrasil.map((uf) => ({ label: uf, value: uf }))

const toast = useToast()
const carregando = ref(false)

interface LinhaMatriz {
  ufOrigem: string
  [ufDestino: string]: string | number
}

const matriz = ref<LinhaMatriz[]>(ufsBrasil.map((uf) => ({ ufOrigem: uf })))
const fcpPorUf = ref<Record<string, number>>({})

const filtro = reactive({
  ufOrigem: '' as string | null,
  ufDestino: '' as string | null
})

async function carregar() {
  carregando.value = true
  try {
    const [respostaIcms, respostaFcp] = await Promise.all([
      useApi('/fiscal/icms-aliquotas-interestaduais', { query: { tamanhoPagina: 1000 } }),
      useApi('/fiscal/fcp-aliquotas-uf', { query: { tamanhoPagina: 100 } })
    ])

    const aliquotas = extrairDados<IcmsAliquotaInterestadual[]>(respostaIcms) ?? []
    const novaMatriz: LinhaMatriz[] = ufsBrasil.map((uf) => ({ ufOrigem: uf }))
    for (const item of aliquotas) {
      const linha = novaMatriz.find((l) => l.ufOrigem === item.ufOrigem)
      if (linha) linha[item.ufDestino] = item.valorAliquota
    }
    matriz.value = novaMatriz

    const fcps = extrairDados<FcpAliquotaUf[]>(respostaFcp) ?? []
    const mapaFcp: Record<string, number> = {}
    for (const item of fcps) mapaFcp[item.uf] = item.valorPercentual
    fcpPorUf.value = mapaFcp
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function valorCelula(ufOrigem: string, ufDestino: string): string {
  const linha = matriz.value.find((l) => l.ufOrigem === ufOrigem)
  const v = linha?.[ufDestino]
  return v == null || v === '' ? '—' : `${v}%`
}

/** Realce de linha/coluna conforme os seletores de UF origem/destino escolhidos, espelhando o legado. */
function classeCelula(ufOrigem: string, ufDestino: string): string {
  const isOrigem = !!filtro.ufOrigem && ufOrigem === filtro.ufOrigem
  const isDestino = !!filtro.ufDestino && ufDestino === filtro.ufDestino
  if (!isOrigem && !isDestino) return ''

  const indexOrigem = filtro.ufOrigem ? ufsBrasil.indexOf(filtro.ufOrigem) : -1
  const indexDestino = filtro.ufDestino ? ufsBrasil.indexOf(filtro.ufDestino) : -1
  const indexItemOrigem = ufsBrasil.indexOf(ufOrigem)
  const indexItemDestino = ufsBrasil.indexOf(ufDestino)

  const destacaOrigem = isOrigem && indexItemDestino <= indexDestino
  const destacaDestino = isDestino && indexItemOrigem <= indexOrigem

  if (destacaOrigem && destacaDestino) return 'destaque-ambos'
  if (destacaOrigem) return 'destaque-origem'
  if (destacaDestino) return 'destaque-destino'
  return ''
}

const aliquotaSelecionada = computed(() => {
  if (!filtro.ufOrigem || !filtro.ufDestino) return null
  const linha = matriz.value.find((l) => l.ufOrigem === filtro.ufOrigem)
  const v = linha?.[filtro.ufDestino]
  return v == null || v === '' ? null : v
})

onMounted(() => {
  void carregar()
})
</script>

<template>
  <div>
    <PageToolbar title="Tabela Interestadual de ICMS" subtitle="Alíquotas de ICMS por UF de origem e destino" :loading="carregando">
      <template #actions>
        <SelectField v-model="filtro.ufOrigem" label="UF Origem" :options="opcoesUf" />
        <SelectField v-model="filtro.ufDestino" label="UF Destino" :options="opcoesUf" />
      </template>
    </PageToolbar>

    <p v-if="aliquotaSelecionada !== null" class="hint-text">
      Alíquota {{ filtro.ufOrigem }} → {{ filtro.ufDestino }}: <strong>{{ aliquotaSelecionada }}%</strong>
    </p>

    <div class="glass-panel table-panel">
      <div class="table-scroll">
        <table class="admin-table icms-table">
          <thead>
            <tr>
              <th class="sticky-col">Origem \ Destino</th>
              <th v-for="uf in ufsBrasil" :key="uf" class="td-center">{{ uf }}</th>
              <th class="td-center">% FCP</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="carregando">
              <td :colspan="ufsBrasil.length + 2" class="table-loading">
                <span class="spinner"></span> Carregando...
              </td>
            </tr>
            <tr v-for="linha in matriz" v-else :key="linha.ufOrigem">
              <td class="sticky-col td-strong">{{ linha.ufOrigem }}</td>
              <td
                v-for="ufDestino in ufsBrasil"
                :key="`${linha.ufOrigem}_${ufDestino}`"
                class="td-center"
                :class="classeCelula(linha.ufOrigem, ufDestino)"
              >
                {{ valorCelula(linha.ufOrigem, ufDestino) }}
              </td>
              <td class="td-center">
                {{ fcpPorUf[linha.ufOrigem] != null ? `${fcpPorUf[linha.ufOrigem]}%` : '—' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.table-panel { padding: 8px 12px; margin-top: 8px; }
.table-scroll { overflow-x: auto; max-height: 70vh; overflow-y: auto; }
.icms-table { min-width: 900px; }
.icms-table th, .icms-table td { white-space: nowrap; }
.sticky-col {
  position: sticky;
  left: 0;
  background: var(--bg-color, #14141c);
  z-index: 1;
}
.td-strong { font-weight: 600; }
.hint-text { color: var(--text-secondary); font-size: 13px; margin: 4px 0 12px; }
.destaque-origem { background: rgba(99, 102, 241, 0.18); }
.destaque-destino { background: rgba(16, 185, 129, 0.18); }
.destaque-ambos { background: rgba(168, 85, 247, 0.22); }
</style>
