<script setup lang="ts">
/**
 * Formulário de Locação — Imobiliária / Locações.
 *
 * Novo: POST /imobiliaria/locacoes (CriarLocacaoCommand). Campos do corpo:
 *   imovelId, periodoInicial (obrig.), periodoFinal (obrig.), valor (obrig. > 0),
 *   vencimento (1..31), locatarioIds[] (>= 1), fiadorIds[] (opcional).
 *
 * A API não expõe detalhe editável nem PUT: esta tela só cria.
 * Ao abrir com um id existente, redireciona para a listagem.
 */
import { ref, computed, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import SelectField from '~/components/shared/fields/SelectField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

interface ParteInput {
  pessoaId: string | null
}
interface LocacaoForm {
  imovelId: string | null
  periodoInicial: string | null
  periodoFinal: string | null
  valor: number | null
  vencimento: number | null
  locatarios: ParteInput[]
  fiadores: ParteInput[]
}

interface Pessoa {
  id: string
  nome?: string | null
  razaoSocial?: string | null
}
interface Imovel {
  id: string
  descricao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const idParam = route.params.id as string
const isEdit = computed(() => idParam !== 'novo')

const salvando = ref(false)
const pessoas = ref<Pessoa[]>([])
const imoveis = ref<Imovel[]>([])

const form = reactive<LocacaoForm>({
  imovelId: null,
  periodoInicial: null,
  periodoFinal: null,
  valor: 0,
  vencimento: null,
  locatarios: [{ pessoaId: null }],
  fiadores: []
})

const erros = reactive<Record<string, string>>({})

const opcoesPessoas = computed<SelectOption[]>(() =>
  pessoas.value.map((p) => ({ label: p.razaoSocial ?? p.nome ?? p.id, value: p.id }))
)
const opcoesImoveis = computed<SelectOption[]>(() =>
  imoveis.value.map((i) => ({ label: i.descricao ?? i.id, value: i.id }))
)
const opcoesVencimento: SelectOption[] = Array.from({ length: 31 }, (_, i) => ({ label: String(i + 1), value: i + 1 }))

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.periodoInicial) erros.periodoInicial = 'O início do período é obrigatório.'
  if (!form.periodoFinal) erros.periodoFinal = 'O fim do período é obrigatório.'
  if (form.periodoInicial && form.periodoFinal && form.periodoFinal < form.periodoInicial) {
    erros.periodoFinal = 'O fim deve ser igual ou posterior ao início.'
  }
  if (!form.valor || form.valor <= 0) erros.valor = 'O valor da locação deve ser positivo.'
  if (form.vencimento == null || form.vencimento < 1 || form.vencimento > 31) {
    erros.vencimento = 'O vencimento deve estar entre 1 e 31.'
  }
  if (form.locatarios.filter((l) => l.pessoaId).length === 0) {
    erros.locatarios = 'A locação exige ao menos um locatário.'
  }
  return Object.keys(erros).length === 0
}

function adicionarLocatario() {
  form.locatarios.push({ pessoaId: null })
}
function removerLocatario(i: number) {
  form.locatarios.splice(i, 1)
}
function adicionarFiador() {
  form.fiadores.push({ pessoaId: null })
}
function removerFiador(i: number) {
  form.fiadores.splice(i, 1)
}

async function carregarPessoas() {
  try {
    const resposta = await useApi('/cadastros/pessoas', { query: { tamanhoPagina: 500 } })
    pessoas.value = extrairDados<Pessoa[]>(resposta) ?? []
  } catch (e) {
    console.error('[locacoes/[id]] pessoas', e)
  }
}

async function carregarImoveis() {
  try {
    const resposta = await useApi('/imobiliaria/imoveis')
    imoveis.value = extrairDados<Imovel[]>(resposta) ?? []
  } catch (e) {
    console.error('[locacoes/[id]] imoveis', e)
  }
}

async function salvar() {
  if (!validar()) {
    toast.error('Formulário possui erros de validação.')
    return
  }
  salvando.value = true
  try {
    const payload = {
      imovelId: form.imovelId,
      periodoInicial: form.periodoInicial,
      periodoFinal: form.periodoFinal,
      valor: form.valor ?? 0,
      vencimento: form.vencimento,
      locatarioIds: form.locatarios.filter((l) => l.pessoaId).map((l) => l.pessoaId),
      fiadorIds: form.fiadores.filter((f) => f.pessoaId).map((f) => f.pessoaId)
    }
    await useApi('/imobiliaria/locacoes', { method: 'POST', body: payload })
    toast.success('Locação formalizada com sucesso!')
    router.push('/erp/imobiliaria/locacoes')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.push('/erp/imobiliaria/locacoes')
}

onMounted(async () => {
  if (isEdit.value) {
    // Sem detalhe editável nem PUT: edição não é suportada pela API.
    toast.error('A API não permite editar locações. Redirecionando para a lista.')
    router.replace('/erp/imobiliaria/locacoes')
    return
  }
  await Promise.all([carregarPessoas(), carregarImoveis()])
})
</script>

<template>
  <div>
    <PageToolbar title="Nova locação">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel form-panel">
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <SelectField
            v-model="form.imovelId"
            label="Imóvel"
            :options="opcoesImoveis"
            placeholder="Selecione (opcional)..."
          />
          <DateTimeField v-model="form.periodoInicial" label="Início do período" required :error="erros.periodoInicial" />
          <DateTimeField v-model="form.periodoFinal" label="Fim do período" required :error="erros.periodoFinal" />
          <MoneyInput v-model="form.valor" label="Valor" required :error="erros.valor" :min="0" />
          <SelectField
            v-model="form.vencimento"
            label="Vencimento (dia do mês)"
            required
            :options="opcoesVencimento"
            :error="erros.vencimento"
          />
        </div>

        <section class="repeater-section">
          <h4 class="section-title">Locatários</h4>
          <p v-if="erros.locatarios" class="field-error mb-2">{{ erros.locatarios }}</p>
          <div v-for="(loc, i) in form.locatarios" :key="`loc-${i}`" class="repeater-row">
            <SelectField v-model="loc.pessoaId" label="Locatário (pessoa)" :options="opcoesPessoas" class="repeater-grow" />
            <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerLocatario(i)">Remover</button>
          </div>
          <button type="button" class="btn btn-secondary btn-sm mt-2" @click="adicionarLocatario">+ Adicionar locatário</button>
        </section>

        <section class="repeater-section">
          <h4 class="section-title">Fiadores</h4>
          <div v-for="(fia, i) in form.fiadores" :key="`fia-${i}`" class="repeater-row">
            <SelectField v-model="fia.pessoaId" label="Fiador (pessoa)" :options="opcoesPessoas" class="repeater-grow" />
            <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click="removerFiador(i)">Remover</button>
          </div>
          <p v-if="!form.fiadores.length" class="text-muted">Nenhum fiador adicionado.</p>
          <button type="button" class="btn btn-secondary btn-sm mt-2" @click="adicionarFiador">+ Adicionar fiador</button>
        </section>
      </form>
    </div>
  </div>
</template>

<style scoped>
.form-panel { padding: 20px; margin-top: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.repeater-section { margin-top: 24px; padding-top: 16px; border-top: 1px solid var(--border-color); }
.section-title { margin: 0 0 12px; font-size: 14px; font-weight: 700; color: var(--text-primary); }
.repeater-row { display: flex; align-items: flex-end; gap: 12px; margin-bottom: 12px; }
.repeater-grow { flex: 1; }
.text-muted { color: var(--text-secondary); font-size: 13px; }
.mt-2 { margin-top: 10px; }
.mb-2 { margin-bottom: 10px; }
</style>
