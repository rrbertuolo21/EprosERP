<script setup lang="ts">
/**
 * Cadastro/edição de Produto (cadastros/produtos/[id]).
 * Porta o comportamento de `ProdutoItemForm.vue` (legado): abas Dados, Combustível e
 * Adicionais, com busca de NCM/CEST por texto, vínculo de balança e persistência via
 * `estoque-produtos`. `id === 'novo'` cria um produto; qualquer outro valor numérico edita.
 *
 * Endpoints consumidos: estoque-produtos, produtos-especificos, fiscal/ncms, cests,
 * adicionais, balancas.
 */
import { computed, onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ProdutoAbaDados from '~/components/cadastros-produto/ProdutoAbaDados.vue'
import ProdutoAbaCombustivel from '~/components/cadastros-produto/ProdutoAbaCombustivel.vue'
import ProdutoAbaAdicionais from '~/components/cadastros-produto/ProdutoAbaAdicionais.vue'
import { criarProdutoFormInicial, type ProdutoForm } from '~/components/cadastros-produto/produtoTypes'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

const route = useRoute()
const idParam = computed(() => route.params.id as string)
const ehNovo = computed(() => idParam.value === 'novo')
const produtoId = computed(() => (ehNovo.value ? null : Number(idParam.value)))

const toast = useToast()

const abaAtiva = ref<'dados' | 'combustivel' | 'adicionais'>('dados')
const carregando = ref(false)
const salvando = ref(false)
const erros = ref<Record<string, string>>({})

const produto = ref<ProdutoForm>(criarProdutoFormInicial())

// --- Listas de apoio ---
const marcasOpcoes = ref<SelectOption[]>([])
const categoriasOpcoes = ref<SelectOption[]>([])
const unidadesOpcoes = ref<SelectOption[]>([])
const balancasOpcoes = ref<SelectOption[]>([])
const ufsOpcoes = ref<SelectOption[]>([])
const origensCombustivelOpcoes = ref<SelectOption[]>([])
const adicionaisDisponiveis = ref<{ id: number; descricao: string }[]>([])

const ncmOpcoes = ref<SelectOption[]>([])
const cestOpcoes = ref<SelectOption[]>([])
const buscandoNcm = ref(false)
const buscandoCest = ref(false)

async function carregarListasApoio() {
  try {
    const respMarcas = await useApi('/marcas-produtos', { query: { tamanhoPagina: 200 } })
    const marcas = extrairDados<Array<{ id: number; descricao: string }>>(respMarcas) ?? []
    marcasOpcoes.value = marcas.map((m) => ({ label: m.descricao, value: m.id }))
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar marcas', e)
  }

  try {
    const respCategorias = await useApi('/categorias-produtos', { query: { tamanhoPagina: 200 } })
    const categorias = extrairDados<Array<{ id: number; descricao: string }>>(respCategorias) ?? []
    categoriasOpcoes.value = categorias.map((c) => ({ label: c.descricao, value: c.id }))
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar categorias', e)
  }

  try {
    const respUnidades = await useApi('/unidades-de-medidas-comercial', { query: { tamanhoPagina: 200 } })
    const unidades = extrairDados<Array<{ id: number; descricao: string; unidadeMedida?: string }>>(respUnidades) ?? []
    unidadesOpcoes.value = unidades.map((u) => ({
      label: `${(u.unidadeMedida ?? '').toUpperCase()} - ${u.descricao.toUpperCase()}`,
      value: u.id
    }))
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar unidades', e)
  }

  try {
    const respBalancas = await useApi('/balancas', { query: { tamanhoPagina: 200 } })
    const balancas = extrairDados<Array<{ id: number; nome: string }>>(respBalancas) ?? []
    balancasOpcoes.value = balancas.map((b) => ({ label: b.nome, value: b.id }))
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar balanças', e)
  }

  try {
    const respAdicionais = await useApi('/adicionais', { query: { tamanhoPagina: 200 } })
    adicionaisDisponiveis.value = extrairDados<Array<{ id: number; descricao: string }>>(respAdicionais) ?? []
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar adicionais', e)
  }

  try {
    const respUfs = await useApi('/cadastros/geografia/ufs')
    const ufs = extrairDados<Array<{ sigla?: string; descricao?: string; id?: number | string }>>(respUfs) ?? []
    ufsOpcoes.value = ufs.map((u) => ({
      label: u.sigla ?? u.descricao ?? String(u.id ?? ''),
      value: u.sigla ?? String(u.id ?? '')
    }))
  } catch (e) {
    console.error('[produtos:[id]] falha ao carregar UFs (pode não existir rota exposta)', e)
  }
}

async function buscarNcm(texto: string) {
  produto.value = { ...produto.value, ncmDescricao: texto }
  if (!texto || texto.length < 2) {
    ncmOpcoes.value = []
    return
  }
  buscandoNcm.value = true
  try {
    const resp = await useApi('/fiscal/ncms', { query: { tamanhoPagina: 100, busca: texto } })
    const itens = extrairDados<Array<{ id: number; descricao: string; codigoNcm?: string }>>(resp) ?? []
    ncmOpcoes.value = itens.map((n) => ({
      label: n.codigoNcm ? `${n.codigoNcm} - ${n.descricao}` : n.descricao,
      value: n.id
    }))
  } catch (e) {
    console.error('[produtos:[id]] falha na busca de NCM', e)
    ncmOpcoes.value = []
  } finally {
    buscandoNcm.value = false
  }
}

let debounceCest: ReturnType<typeof setTimeout> | undefined
async function buscarCest(texto: string) {
  if (debounceCest) clearTimeout(debounceCest)
  if (!texto || texto.length < 2) {
    cestOpcoes.value = []
    return
  }
  debounceCest = setTimeout(async () => {
    buscandoCest.value = true
    try {
      const resp = await useApi('/cests', { query: { tamanhoPagina: 100, busca: texto } })
      const itens = extrairDados<Array<{ id: number; descricao: string; codigo?: string }>>(resp) ?? []
      cestOpcoes.value = itens.map((c) => ({
        label: c.codigo ? `${c.codigo} - ${c.descricao}` : c.descricao,
        value: c.id
      }))
    } catch (e) {
      console.error('[produtos:[id]] falha na busca de CEST', e)
      cestOpcoes.value = []
    } finally {
      buscandoCest.value = false
    }
  }, 400)
}

function mapRespostaParaForm(dados: Record<string, unknown>): ProdutoForm {
  const base = criarProdutoFormInicial()
  const especifico = dados.produtoEspecifico as Record<string, unknown> | undefined
  return {
    ...base,
    ...dados,
    id: dados.id as number | undefined,
    codigo: (dados.codigo as string) ?? '',
    descricao: (dados.descricao as string) ?? '',
    ean: (dados.ean as string) ?? '',
    ativo: (dados.ativo as boolean) ?? true,
    marcaProdutoId: (dados.marcaProdutoId as number) ?? null,
    categoriaId: (dados.categoriaId as number) ?? null,
    unidadeMedidaComercialId: (dados.unidadeMedidaComercialId as number) ?? null,
    valorCompra: (dados.valorCompra as number) ?? 0,
    valorVenda: (dados.valorVenda as number) ?? 0,
    valorVendaPrazo: (dados.valorVendaPrazo as number) ?? 0,
    pesoLiquido: (dados.pesoLiquido as number) ?? 0,
    pesoBruto: (dados.pesoBruto as number) ?? 0,
    ncmId: (dados.ncmId as number) ?? null,
    ncmDescricao: (dados.ncmDescricao as string) ?? '',
    cestId: (dados.cestId as number) ?? null,
    codigoAnpId: (dados.codigoAnpId as number) ?? null,
    utilizaBalanca: (dados.utilizaBalanca as boolean) ?? false,
    codigoProdutoBalanca: (dados.codigoProdutoBalanca as string) ?? '',
    balancaId: (dados.balancaId as number) ?? null,
    imagem: (dados.imagem as string) ?? '',
    adicionaisProduto:
      (dados.adicionaisProduto as Array<{ id?: number; adicionaisId: number; produtoId?: number; descricao?: string }>)?.map((a) => ({
        ...a,
        descricao: a.descricao ?? adicionaisDisponiveis.value.find((d) => d.id === a.adicionaisId)?.descricao ?? ''
      })) ?? [],
    produtoEspecifico: especifico
      ? {
          ufConsumo: (especifico.ufConsumo as string) ?? '',
          valorPartida: (especifico.valorPartida as number) ?? 0,
          valorPercentualGlpDerivadoPetroleo: (especifico.valorPercentualGlpDerivadoPetroleo as number) ?? 0,
          valorPercentualGasNaturalImportado: (especifico.valorPercentualGasNaturalImportado as number) ?? 0,
          valorPercentualGasNaturalNacional: (especifico.valorPercentualGasNaturalNacional as number) ?? 0,
          origens: (especifico.origens as ProdutoForm['produtoEspecifico']['origens']) ?? []
        }
      : base.produtoEspecifico
  }
}

async function carregarProduto() {
  if (!produtoId.value) return
  carregando.value = true
  try {
    const resp = await useApi(`/estoque-produtos/{id}`, { params: { id: produtoId.value } })
    const dados = extrairDados<Record<string, unknown>>(resp)
    if (!dados) {
      toast.error('Produto não encontrado')
      return
    }
    produto.value = mapRespostaParaForm(dados)
    if (produto.value.ncmId && produto.value.ncmDescricao) {
      ncmOpcoes.value = [{ label: produto.value.ncmDescricao, value: produto.value.ncmId }]
    }
    if (produto.value.cestId) {
      cestOpcoes.value = [{ label: 'CEST atual', value: produto.value.cestId }]
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function validar(): boolean {
  const novosErros: Record<string, string> = {}
  if (!produto.value.descricao.trim()) novosErros.descricao = 'Descrição é obrigatória'
  if (!produto.value.unidadeMedidaComercialId) novosErros.unidadeMedidaComercialId = 'Unidade comercial é obrigatória'
  if (!produto.value.ncmId) novosErros.ncmId = 'NCM é obrigatório'
  if (produto.value.valorCompra == null) novosErros.valorCompra = 'Valor de compra é obrigatório'
  if (produto.value.utilizaBalanca) {
    if (!produto.value.codigoProdutoBalanca.trim()) novosErros.codigoProdutoBalanca = 'Código do produto na balança é obrigatório'
    if (!produto.value.balancaId) novosErros.balancaId = 'Balança é obrigatória'
  }
  erros.value = novosErros
  return Object.keys(novosErros).length === 0
}

async function salvar() {
  if (!validar()) {
    toast.error('Preencha os campos obrigatórios')
    return
  }

  salvando.value = true
  try {
    const payload = {
      ...produto.value,
      codigo: produto.value.codigo.trim() || null,
      ean: produto.value.ean.trim() || 'SEM GTIN'
    }

    if (produto.value.id) {
      await useApi(`/estoque-produtos/{id}`, { method: 'PUT', params: { id: produto.value.id }, body: payload })
    } else {
      await useApi('/estoque-produtos', { method: 'POST', body: payload })
    }

    toast.success('Produto salvo com sucesso')
    navigateTo('/erp/cadastros/produtos')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  navigateTo('/erp/cadastros/produtos')
}

onMounted(async () => {
  await carregarListasApoio()
  if (!ehNovo.value) {
    await carregarProduto()
  }
})
</script>

<template>
  <div>
    <PageToolbar :title="ehNovo ? 'Novo Produto' : 'Editar Produto'" subtitle="Cadastro de produto" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel produto-form-card">
      <nav class="form-tabs">
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'dados' }" @click="abaAtiva = 'dados'">Dados</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'combustivel' }" @click="abaAtiva = 'combustivel'">Combustível</button>
        <button type="button" class="tab-link" :class="{ active: abaAtiva === 'adicionais' }" @click="abaAtiva = 'adicionais'">Adicionais</button>
      </nav>

      <div class="produto-form-body">
        <ProdutoAbaDados
          v-show="abaAtiva === 'dados'"
          v-model="produto"
          :marcas-opcoes="marcasOpcoes"
          :categorias-opcoes="categoriasOpcoes"
          :unidades-opcoes="unidadesOpcoes"
          :ncm-opcoes="ncmOpcoes"
          :cest-opcoes="cestOpcoes"
          :balancas-opcoes="balancasOpcoes"
          :buscando-ncm="buscandoNcm"
          :buscando-cest="buscandoCest"
          :erros="erros"
          @buscar-ncm="buscarNcm"
          @buscar-cest="buscarCest"
        />

        <ProdutoAbaCombustivel
          v-if="abaAtiva === 'combustivel'"
          v-model="produto.produtoEspecifico"
          :ufs-opcoes="ufsOpcoes"
          :origens-combustivel-opcoes="origensCombustivelOpcoes"
        />

        <ProdutoAbaAdicionais
          v-if="abaAtiva === 'adicionais'"
          v-model="produto.adicionaisProduto"
          :disponiveis="adicionaisDisponiveis"
          :produto-descricao="produto.descricao"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.produto-form-card { padding: 0; overflow: hidden; }
.form-tabs {
  display: flex;
  gap: 8px;
  padding: 16px 20px 0;
  border-bottom: 1px solid var(--border-color);
}
.tab-link {
  background: none;
  border: none;
  color: var(--text-secondary);
  padding: 10px 16px;
  font-size: 13.5px;
  font-weight: 600;
  cursor: pointer;
  border-radius: 6px 6px 0 0;
  transition: all 0.2s ease;
}
.tab-link:hover,
.tab-link.active {
  background: rgba(255, 255, 255, 0.03);
  color: var(--text-primary);
}
.tab-link.active {
  box-shadow: 0 -2px 0 var(--primary) inset;
}
.produto-form-body { padding: 20px; }
</style>
