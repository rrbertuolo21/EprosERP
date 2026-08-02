<script setup lang="ts">
/**
 * Inventário Físico — criação e ciclo de vida (erp/estoque/inventarios/[id]).
 *
 * `id === 'novo'`  → formulário de criação (POST /estoque-inventarios): cabeçalho + N itens.
 * `id === guid`    → detalhe + fluxo (InventarioQueries): iniciar contagem, registrar contagem por
 *                    item (até 3 contagens + fechamento), enviar à conferência, aprovar (calcula
 *                    acurácia), aplicar ajuste por diferença e cancelar.
 *
 * Endpoints (InventariosController):
 *   POST /estoque-inventarios                         criar
 *   POST /estoque-inventarios/{id}/iniciar-contagem   Rascunho → EmContagem
 *   POST /estoque-inventarios/{id}/itens/{itemId}/contagem   registra contagens
 *   POST /estoque-inventarios/{id}/conferencia        EmContagem → EmConferencia
 *   POST /estoque-inventarios/{id}/aprovar            EmConferencia → Aprovado
 *   POST /estoque-inventarios/{id}/aplicar-ajuste     Aprovado → Ajustado
 *   DELETE /estoque-inventarios/{id}                  cancela (body { id, motivo })
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { formatarData, formatarDataHora, formatarNumero, formatarPorcentagem } = useHelper()
const { tipoInventario, situacaoInventario } = useEstoqueEnums()

const idParam = computed(() => route.params.id as string)
const ehNovo = computed(() => idParam.value === 'novo')

// --------- Situações ---------
const S = { Rascunho: 0, EmContagem: 1, EmConferencia: 2, Aprovado: 3, Ajustado: 4, Cancelado: 5 }

// --------- Detalhe ---------
interface InventarioItem {
  id: string
  produtoId: string
  localId: string | null
  lote: string | null
  quantidadeSistema: number | null
  contagem01: number | null
  contagem02: number | null
  contagem03: number | null
  quantidadeContada: number | null
  divergencia: number | null
  fechadoContagem: boolean
}
interface InventarioDetalhe {
  id: string
  empresaId: string
  dataContagem: string | null
  tipoInventario: number
  situacao: number
  acuracidade: number | null
  estoqueAtualizado: boolean
  observacao: string | null
  criadoEm: string
  itens: InventarioItem[]
}

const carregando = ref(false)
const processando = ref(false)
const detalhe = ref<InventarioDetalhe | null>(null)

async function carregar() {
  if (ehNovo.value) return
  carregando.value = true
  try {
    const resp = await useApi('/estoque-inventarios/{id}', { params: { id: idParam.value } })
    detalhe.value = extrairDados<InventarioDetalhe>(resp) ?? null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

const situacao = computed(() => detalhe.value?.situacao ?? S.Rascunho)
const podeIniciar = computed(() => situacao.value === S.Rascunho)
const podeContar = computed(() => situacao.value === S.EmContagem)
const podeConferir = computed(() => situacao.value === S.EmContagem)
const podeAprovar = computed(() => situacao.value === S.EmConferencia)
const podeAjustar = computed(() => situacao.value === S.Aprovado)
const podeCancelar = computed(() => [S.Rascunho, S.EmContagem, S.EmConferencia].includes(situacao.value))

const colunasItens: DataTableColumn<InventarioItem>[] = [
  { key: 'produtoId', label: 'Produto (ID)' },
  { key: 'lote', label: 'Lote', formatter: (v) => (v as string) || '-' },
  { key: 'quantidadeSistema', label: 'Sistema', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeContada', label: 'Contado', align: 'right', formatter: (v) => (v == null ? '-' : formatarNumero(v as number, 0, 4)) },
  { key: 'divergencia', label: 'Divergência', align: 'right' },
  { key: 'fechadoContagem', label: 'Fechado', align: 'center', width: '90px' }
]

// --------- Registrar contagem (dialog) ---------
const contagemDialog = ref(false)
const itemSelecionado = ref<InventarioItem | null>(null)
const contagem = reactive({ contagem01: null as number | null, contagem02: null as number | null, contagem03: null as number | null, quantidadeContada: null as number | null, fechar: false })

function abrirContagem(item: InventarioItem) {
  itemSelecionado.value = item
  contagem.contagem01 = item.contagem01
  contagem.contagem02 = item.contagem02
  contagem.contagem03 = item.contagem03
  contagem.quantidadeContada = item.quantidadeContada
  contagem.fechar = item.fechadoContagem
  contagemDialog.value = true
}

async function salvarContagem() {
  if (!detalhe.value || !itemSelecionado.value) return
  processando.value = true
  try {
    await useApi('/estoque-inventarios/{id}/itens/{itemId}/contagem', {
      method: 'POST',
      params: { id: detalhe.value.id, itemId: itemSelecionado.value.id },
      body: {
        inventarioId: detalhe.value.id,
        itemId: itemSelecionado.value.id,
        contagem01: contagem.contagem01,
        contagem02: contagem.contagem02,
        contagem03: contagem.contagem03,
        quantidadeContada: contagem.quantidadeContada,
        fechar: contagem.fechar
      }
    })
    toast.success('Contagem registrada.')
    contagemDialog.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Transições ---------
async function transicao(rota: string, msg: string) {
  if (!detalhe.value) return
  processando.value = true
  try {
    await useApi(`/estoque-inventarios/{id}/${rota}`, { method: 'POST', params: { id: detalhe.value.id } })
    toast.success(msg)
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Cancelar (dialog) ---------
const cancelarDialog = ref(false)
const motivoCancelamento = ref('')
async function confirmarCancelamento() {
  if (!detalhe.value) return
  if (!motivoCancelamento.value.trim()) {
    toast.error('Informe o motivo do cancelamento.')
    return
  }
  processando.value = true
  try {
    await useApi('/estoque-inventarios/{id}', {
      method: 'DELETE',
      params: { id: detalhe.value.id },
      body: { id: detalhe.value.id, motivo: motivoCancelamento.value.trim() }
    })
    toast.success('Inventário cancelado.')
    cancelarDialog.value = false
    await carregar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

// --------- Criação (novo) ---------
interface NovoItem { produtoId: string; localId: string; lote: string; quantidadeSistema: number | null }
const novo = reactive({
  empresaId: '',
  dataContagem: new Date().toISOString().slice(0, 10),
  tipoInventario: 0 as number,
  observacao: '',
  itens: [{ produtoId: '', localId: '', lote: '', quantidadeSistema: null }] as NovoItem[]
})
const errosNovo = reactive<Record<string, string>>({})
function addLinha() { novo.itens.push({ produtoId: '', localId: '', lote: '', quantidadeSistema: null }) }
function removerLinha(i: number) { novo.itens.splice(i, 1); if (novo.itens.length === 0) addLinha() }

function validarNovo(): boolean {
  for (const k of Object.keys(errosNovo)) delete errosNovo[k]
  if (!novo.empresaId.trim()) errosNovo.empresaId = 'Empresa é obrigatória.'
  if (novo.itens.filter((i) => i.produtoId.trim()).length === 0) errosNovo.itens = 'Informe ao menos um produto.'
  return Object.keys(errosNovo).length === 0
}

async function criar() {
  if (!validarNovo()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  processando.value = true
  try {
    const resp = await useApi('/estoque-inventarios', {
      method: 'POST',
      body: {
        empresaId: novo.empresaId.trim(),
        dataContagem: novo.dataContagem || null,
        tipoInventario: novo.tipoInventario,
        observacao: novo.observacao.trim() || null,
        itens: novo.itens
          .filter((i) => i.produtoId.trim())
          .map((i) => ({
            produtoId: i.produtoId.trim(),
            localId: i.localId.trim() || null,
            lote: i.lote.trim() || null,
            quantidadeSistema: i.quantidadeSistema
          }))
      }
    })
    const criado = extrairDados<{ id?: string }>(resp)
    toast.success('Inventário criado com sucesso!')
    if (criado?.id) await router.push(`/erp/estoque/inventarios/${criado.id}`)
    else await router.push('/erp/estoque/inventarios')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = false
  }
}

function voltar() {
  router.push('/erp/estoque/inventarios')
}

onMounted(() => void carregar())
</script>

<template>
  <div>
    <!-- ============ NOVO ============ -->
    <template v-if="ehNovo">
      <PageToolbar title="Novo inventário" subtitle="Cabeçalho + itens a contar" :loading="processando">
        <template #actions>
          <button type="button" class="btn btn-secondary" :disabled="processando" @click="voltar">Cancelar</button>
          <button type="button" class="btn btn-primary" :disabled="processando" @click="criar">
            <span v-if="processando" class="spinner"></span><span v-else>Criar inventário</span>
          </button>
        </template>
      </PageToolbar>

      <div class="glass-panel form-panel">
        <div class="form-grid">
          <TextField v-model="novo.empresaId" label="Empresa (ID)" required :error="errosNovo.empresaId" />
          <DateTimeField v-model="novo.dataContagem" label="Data da contagem" />
          <SelectField v-model="novo.tipoInventario" label="Tipo de inventário" :options="tipoInventario.opcoes" :clearable="false" />
        </div>
        <TextField v-model="novo.observacao" label="Observação" />

        <h3 class="secao-titulo">Itens</h3>
        <p v-if="errosNovo.itens" class="erro-inline">{{ errosNovo.itens }}</p>
        <div class="tabela-itens">
          <div class="linha-item cabecalho">
            <span>Produto (ID)</span><span>Local (ID)</span><span>Lote</span><span>Qtd. sistema</span><span></span>
          </div>
          <div v-for="(item, i) in novo.itens" :key="i" class="linha-item">
            <TextField v-model="item.produtoId" placeholder="GUID do produto" />
            <TextField v-model="item.localId" placeholder="Opcional" />
            <TextField v-model="item.lote" placeholder="Opcional" />
            <QuantityInput v-model="item.quantidadeSistema" :decimais="4" hint="Vazio = foto do kardex" />
            <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerLinha(i)">✕</button>
          </div>
        </div>
        <button type="button" class="btn btn-secondary btn-sm add-btn" @click="addLinha">+ Adicionar item</button>
      </div>
    </template>

    <!-- ============ DETALHE ============ -->
    <template v-else>
      <PageToolbar title="Inventário" :subtitle="detalhe ? tipoInventario.label(detalhe.tipoInventario) : ''" :loading="carregando || processando">
        <template #actions>
          <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
          <button v-if="podeIniciar" type="button" class="btn btn-primary" :disabled="processando" @click="transicao('iniciar-contagem', 'Contagem iniciada.')">Iniciar contagem</button>
          <button v-if="podeConferir" type="button" class="btn btn-primary" :disabled="processando" @click="transicao('conferencia', 'Enviado à conferência.')">Enviar à conferência</button>
          <button v-if="podeAprovar" type="button" class="btn btn-primary" :disabled="processando" @click="transicao('aprovar', 'Inventário aprovado.')">Aprovar</button>
          <button v-if="podeAjustar" type="button" class="btn btn-primary" :disabled="processando" @click="transicao('aplicar-ajuste', 'Ajuste aplicado ao estoque.')">Aplicar ajuste</button>
          <button v-if="podeCancelar" type="button" class="btn btn-danger" :disabled="processando" @click="cancelarDialog = true">Cancelar inventário</button>
        </template>
      </PageToolbar>

      <div v-if="detalhe" class="glass-panel form-panel">
        <div class="cabecalho-info">
          <div><span class="dado-label">Situação</span><span class="badge" :class="classeBadge(situacaoInventario.cor(detalhe.situacao))">{{ situacaoInventario.label(detalhe.situacao) }}</span></div>
          <div><span class="dado-label">Criado em</span><span class="dado-valor">{{ formatarDataHora(detalhe.criadoEm) }}</span></div>
          <div><span class="dado-label">Data contagem</span><span class="dado-valor">{{ formatarData(detalhe.dataContagem) || '-' }}</span></div>
          <div><span class="dado-label">Acurácia</span><span class="dado-valor">{{ detalhe.acuracidade == null ? '-' : formatarPorcentagem(detalhe.acuracidade, 2) }}</span></div>
          <div><span class="dado-label">Estoque ajustado</span><span class="dado-valor">{{ detalhe.estoqueAtualizado ? 'Sim' : 'Não' }}</span></div>
        </div>
        <p v-if="detalhe.observacao" class="observacao">{{ detalhe.observacao }}</p>
      </div>

      <DataTable
        v-if="detalhe"
        :items="detalhe.itens"
        :columns="colunasItens"
        :total="detalhe.itens.length"
        :page="1"
        :page-size="detalhe.itens.length || 1"
        row-key="id"
        empty-text="Sem itens"
      >
        <template #cell-fechadoContagem="{ value }">
          <span class="badge" :class="value ? 'badge-success' : 'badge-muted'">{{ value ? 'Sim' : 'Não' }}</span>
        </template>
        <template #cell-divergencia="{ value }">
          <span v-if="value == null">-</span>
          <span v-else :class="{ 'txt-danger': (value as number) !== 0 }">{{ formatarNumero(value as number, 0, 4) }}</span>
        </template>
        <template #actions="{ row }">
          <button v-if="podeContar" type="button" class="btn btn-ghost btn-sm" @click.stop="abrirContagem(row)">Contar</button>
        </template>
      </DataTable>
    </template>

    <!-- Dialog de contagem -->
    <AppDialog v-model="contagemDialog" title="Registrar contagem do item" width="520px">
      <div v-if="itemSelecionado" class="dialog-body">
        <p class="dialog-sub">Produto: <strong>{{ itemSelecionado.produtoId }}</strong> · Sistema: {{ formatarNumero(itemSelecionado.quantidadeSistema, 0, 4) }}</p>
        <div class="form-grid">
          <QuantityInput v-model="contagem.contagem01" label="Contagem 1" :decimais="4" />
          <QuantityInput v-model="contagem.contagem02" label="Contagem 2" :decimais="4" />
          <QuantityInput v-model="contagem.contagem03" label="Contagem 3" :decimais="4" />
          <QuantityInput v-model="contagem.quantidadeContada" label="Quantidade final contada" :decimais="4" />
        </div>
        <label class="check-fechar"><input v-model="contagem.fechar" type="checkbox" /> Fechar contagem deste item</label>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="contagemDialog = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="processando" @click="salvarContagem">
          <span v-if="processando" class="spinner"></span><span v-else>Salvar contagem</span>
        </button>
      </template>
    </AppDialog>

    <!-- Dialog de cancelamento -->
    <AppDialog v-model="cancelarDialog" title="Cancelar inventário" width="440px" persistent>
      <TextField v-model="motivoCancelamento" label="Motivo do cancelamento" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="processando" @click="cancelarDialog = false">Voltar</button>
        <button type="button" class="btn btn-danger" :disabled="processando" @click="confirmarCancelamento">
          <span v-if="processando" class="spinner"></span><span v-else>Confirmar cancelamento</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-bottom: 12px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 12px; }
.secao-titulo { font-size: 14px; margin: 18px 0 10px; }
.tabela-itens { display: flex; flex-direction: column; gap: 8px; }
.linha-item { display: grid; grid-template-columns: 2fr 1.5fr 1fr 1.2fr 40px; gap: 10px; align-items: end; }
.linha-item.cabecalho { font-size: 12px; color: var(--text-muted); align-items: center; }
.add-btn { margin-top: 12px; }
.cabecalho-info { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 16px; }
.dado-label { display: block; font-size: 12px; color: var(--text-muted); margin-bottom: 4px; }
.dado-valor { font-size: 14px; }
.observacao { margin-top: 14px; font-size: 13px; color: var(--text-secondary); }
.erro-inline { color: var(--danger, #dc3545); font-size: 13px; margin: 4px 0; }
.dialog-sub { font-size: 13px; color: var(--text-secondary); margin-bottom: 12px; }
.check-fechar { display: flex; align-items: center; gap: 8px; font-size: 13px; margin-top: 12px; }
.txt-danger { color: var(--danger, #dc3545); font-weight: 600; }
</style>
