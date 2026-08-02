<script setup lang="ts">
/**
 * Lista de Materiais (BOM legado) — Produção / Lista de Materiais.
 *
 * Agregado sem GET-lista: expõe apenas POST /producao/bom (criar) e GET /producao/bom/{sku}
 * (consulta por SKU). Por isso a tela é uma consulta-por-SKU + formulário de criação, sem
 * DataTable de listagem. Fonte: ProducaoController (bom, bom/{sku}) + CriarListaMateriaisCommand.
 *
 * Lacuna: a coleção `itens` do POST não é editável aqui (sem sub-endpoint de manutenção) — ver relatório.
 */
import { ref, reactive } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

const toast = useToast()

// Consulta por SKU
const skuConsulta = ref('')
const consultando = ref(false)
const resultado = ref<Record<string, unknown> | null>(null)
const consultado = ref(false)

// Criação
const salvando = ref(false)
const form = reactive<{ produtoAcabadoSku: string | null; descricao: string | null; versao: string | null }>({
  produtoAcabadoSku: null,
  descricao: null,
  versao: null
})
const erros = reactive<Record<string, string>>({})

async function consultar() {
  if (!skuConsulta.value) { toast.error('Informe um SKU para consultar.'); return }
  consultando.value = true
  consultado.value = false
  try {
    const resposta = await useApi(`/producao/bom/${encodeURIComponent(skuConsulta.value)}`)
    resultado.value = extrairDados<Record<string, unknown>>(resposta) ?? null
    consultado.value = true
  } catch (e) {
    resultado.value = null
    consultado.value = true
    toast.error(obterMensagemErro(e))
  } finally {
    consultando.value = false
  }
}

function validar(): boolean {
  for (const k of Object.keys(erros)) delete erros[k]
  if (!form.produtoAcabadoSku) erros.produtoAcabadoSku = 'SKU do produto acabado é obrigatório.'
  return Object.keys(erros).length === 0
}

async function salvar() {
  if (!validar()) { toast.error('Formulário possui erros de validação.'); return }
  salvando.value = true
  try {
    await useApi('/producao/bom', { method: 'POST', body: form })
    toast.success('Lista de materiais criada com sucesso!')
    form.produtoAcabadoSku = null
    form.descricao = null
    form.versao = null
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

const itens = () => (resultado.value?.itens as unknown[] | undefined) ?? []
</script>

<template>
  <div>
    <PageToolbar title="Lista de Materiais (BOM)" subtitle="Consulta por SKU e cadastro de lista de materiais" />

    <!-- Consulta por SKU -->
    <div class="glass-panel section">
      <h4 class="section-title">Consultar por SKU</h4>
      <div class="consulta-row">
        <TextField v-model="skuConsulta" label="SKU do produto acabado" placeholder="Digite o SKU..." @keyup.enter="consultar" />
        <button type="button" class="btn btn-primary" :disabled="consultando" @click="consultar">
          <span v-if="consultando" class="spinner"></span>
          <span v-else>Consultar</span>
        </button>
      </div>

      <div v-if="consultado && resultado" class="detail-grid mt">
        <div class="detail-item"><span class="detail-label">SKU</span><span>{{ resultado.produtoAcabadoSku || skuConsulta }}</span></div>
        <div class="detail-item"><span class="detail-label">Descrição</span><span>{{ resultado.descricao || '—' }}</span></div>
        <div class="detail-item"><span class="detail-label">Versão</span><span>{{ resultado.versao || '—' }}</span></div>
        <div class="detail-item"><span class="detail-label">Itens</span><span>{{ itens().length }}</span></div>
      </div>
      <p v-else-if="consultado && !resultado" class="empty-detail mt">Nenhuma lista de materiais encontrada para este SKU.</p>
    </div>

    <!-- Criação -->
    <div class="glass-panel section">
      <h4 class="section-title">Nova lista de materiais</h4>
      <form class="vertical-form" @submit.prevent="salvar">
        <div class="form-grid">
          <TextField v-model="form.produtoAcabadoSku" label="SKU do produto acabado" required :error="erros.produtoAcabadoSku" />
          <TextField v-model="form.descricao" label="Descrição" maxlength="200" />
          <TextField v-model="form.versao" label="Versão" maxlength="20" />
        </div>
        <p class="form-note">Os itens (componentes) da lista são gerenciados após a criação (sem endpoint de manutenção de itens exposto).</p>
        <div class="actions">
          <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
            <span v-if="salvando" class="spinner"></span>
            <span v-else>Salvar</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.section { padding: 20px; margin-top: 12px; }
.section-title { font-size: 14px; font-weight: 600; margin: 0 0 14px; color: var(--text-primary); }
.consulta-row { display: flex; gap: 12px; align-items: flex-end; }
.consulta-row .field { flex: 1; max-width: 360px; }
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.detail-item { display: flex; flex-direction: column; gap: 4px; }
.detail-label { font-size: 12px; color: var(--text-secondary); font-weight: 600; }
.form-note { margin-top: 16px; font-size: 12.5px; color: var(--text-secondary); }
.actions { margin-top: 16px; display: flex; justify-content: flex-end; }
.empty-detail { color: var(--text-secondary); }
.mt { margin-top: 16px; }
</style>
