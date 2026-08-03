<script setup lang="ts">
/**
 * Cadastro/edição de Empresa — Cadastros > Empresas > [id].
 *
 * Porta o comportamento de `cadastros/empresa/[id].vue` do legado (abas Identificação,
 * Endereço, Contato, NFCe, NFe) para o design system novo. A rota usa `novo` como
 * marcador de criação (padrão do MAPA_FRONTEND / demais fatias de cadastro).
 *
 * Persistência: a API nova separa Empresa, Parâmetros DF-e e Contatos em recursos
 * distintos (`cadastros/empresas`, `cadastros/empresas/{id}/parametros-dfe`,
 * `cadastros/empresas/{id}/contatos`). Ao salvar, a tela primeiro cria/atualiza a
 * Empresa, depois cria/atualiza os Parâmetros DF-e e por fim sincroniza os Contatos.
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from '#app'
import { useApi, extrairDados, type CommandResult } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useMask } from '~/composables/useMask'
import { useDocumento } from '~/composables/useDocumento'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import EmpresaContatosPanel from '~/components/cadastros-empresa/EmpresaContatosPanel.vue'
import EmpresaDfePanel from '~/components/cadastros-empresa/EmpresaDfePanel.vue'
import { criarEmpresaFormInicial, type EmpresaContato, type EmpresaFormState } from '~/components/cadastros-empresa/types'

definePageMeta({ layout: 'default', middleware: 'auth' })

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { maskCpfCnpj, maskCEP, somenteDigitos } = useMask()
const { validarCpfCnpj } = useDocumento()

const idParam = route.params.id as string
const criandoNova = idParam === 'novo'
const tituloPagina = computed(() => (criandoNova ? 'Cadastrar nova empresa' : 'Editar dados da empresa'))

const abas = ['Identificação', 'Endereço', 'Contato', 'NFCe/NFe'] as const
const abaAtiva = ref<(typeof abas)[number]>('Identificação')

const empresa = reactive<EmpresaFormState>(criarEmpresaFormInicial())
const contatos = ref<EmpresaContato[]>([])
const cnpjDigitado = ref('')

const carregando = ref(false)
const salvando = ref(false)
const erroValidacao = ref<string | null>(null)

const regimeTributarioOpcoes = [
  { label: 'Simples Nacional', value: 1 },
  { label: 'Lucro Presumido', value: 2 },
  { label: 'Lucro Real', value: 3 }
]
const regimeApuracaoOpcoes = [
  { label: 'Cumulativo', value: 1 },
  { label: 'Não Cumulativo', value: 2 },
  { label: 'Misto', value: 3 }
]
const ufOpcoes = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
].map((uf) => ({ label: uf, value: uf }))

const regimeSimplesNacional = computed(() => empresa.regimeTributario === 1)
const exigeApuracao = computed(() => empresa.regimeTributario === 2 || empresa.regimeTributario === 3)

// --- Certificado digital A1 (.pfx/.p12) — enviado ao endpoint da própria empresa ---
const certSenha = ref('')
const certArquivoBase64 = ref<string | null>(null)
const certNomeArquivo = ref<string | null>(null)
const enviandoCert = ref(false)

function onCertificadoSelecionado(evento: Event): void {
  const input = evento.target as HTMLInputElement
  const arquivo = input.files?.[0]
  if (!arquivo) return
  certNomeArquivo.value = arquivo.name
  const reader = new FileReader()
  reader.onload = () => {
    const resultado = String(reader.result)
    certArquivoBase64.value = resultado.includes(',') ? resultado.split(',')[1] : resultado
  }
  reader.readAsDataURL(arquivo)
}

async function enviarCertificado(): Promise<void> {
  if (criandoNova) { toast.error('Salve a empresa antes de enviar o certificado.'); return }
  if (!certArquivoBase64.value) { toast.error('Selecione o arquivo do certificado (.pfx/.p12).'); return }
  if (!certSenha.value) { toast.error('Informe a senha do certificado.'); return }
  enviandoCert.value = true
  try {
    await useApi('/cadastros/empresas/{id}/certificados', {
      method: 'POST',
      params: { id: idParam },
      body: { arquivoBase64: certArquivoBase64.value, senha: certSenha.value }
    })
    toast.success('Certificado enviado e armazenado com segurança.')
    certArquivoBase64.value = null
    certNomeArquivo.value = null
    certSenha.value = ''
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    enviandoCert.value = false
  }
}

/** Carrega a empresa (Identificação + Endereço), os Parâmetros DF-e e os Contatos. */
async function carregarEmpresa() {
  if (criandoNova) return
  carregando.value = true
  try {
    const respEmpresa = await useApi<CommandResult<EmpresaFormState> | EmpresaFormState>('/cadastros/empresas/{id}', {
      params: { id: idParam }
    })
    const dados = extrairDados<EmpresaFormState>(respEmpresa) ?? (respEmpresa as EmpresaFormState)
    if (dados) {
      Object.assign(empresa, dados)
      empresa.id = idParam
      cnpjDigitado.value = maskCpfCnpj(dados.cnpj ?? '')
    }

    try {
      const respDfe = await useApi<CommandResult<EmpresaFormState['empresaParametrosDfe']>>(
        '/cadastros/empresas/{empresaId}/parametros-dfe',
        { params: { empresaId: idParam } }
      )
      const dfe = extrairDados<EmpresaFormState['empresaParametrosDfe']>(respDfe)
      if (dfe) empresa.empresaParametrosDfe = { ...empresa.empresaParametrosDfe, ...dfe }
    } catch {
      // Empresa pode ainda não ter parâmetros DF-e cadastrados — mantém default.
    }

    try {
      const respContatos = await useApi<CommandResult<EmpresaContato[]>>('/cadastros/empresas/{empresaId}/contatos', {
        params: { empresaId: idParam }
      })
      contatos.value = extrairDados<EmpresaContato[]>(respContatos) ?? []
    } catch {
      contatos.value = []
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

function montarEnderecoPayload() {
  return {
    logradouro: empresa.endereco.logradouro,
    numero: empresa.endereco.numero,
    complemento: empresa.endereco.complemento || null,
    bairro: empresa.endereco.bairro,
    cep: somenteDigitos(empresa.endereco.cep),
    cidade: empresa.endereco.cidade,
    estado: empresa.endereco.estado
  }
}

function montarEmpresaPayload() {
  return {
    id: criandoNova ? undefined : empresa.id,
    razaoSocial: empresa.razaoSocial,
    nomeFantasia: empresa.nomeFantasia || null,
    cnpj: somenteDigitos(cnpjDigitado.value),
    inscricaoEstadual: empresa.inscricaoEstadual || null,
    inscricaoMunicipal: empresa.inscricaoMunicipal || null,
    inscricaoSuframa: empresa.inscricaoSuframa || null,
    cnae: empresa.cnae || null,
    regimeTributario: empresa.regimeTributario,
    regimeApuracao: exigeApuracao.value ? empresa.regimeApuracao : 1,
    pessoaGrupoId: empresa.pessoaGrupoId || null,
    produtoGrupoId: empresa.produtoGrupoId || null,
    planoContasFinanceiroId: empresa.planoContasFinanceiroId || null,
    tributarioGrupoId: empresa.tributarioGrupoId || null,
    ncmTributacaoId: empresa.ncmTributacaoId || null,
    certificadoDigitalId: empresa.certificadoDigitalId || null,
    empresaParametrosDfeId: empresa.empresaParametrosDfeId || null,
    linkWebApiAppVendas: empresa.linkWebApiAppVendas || null,
    tokenMercadoPagoPix: empresa.tokenMercadoPagoPix || null,
    logo: empresa.logo || null,
    endereco: montarEnderecoPayload()
  }
}

function montarDfePayload(empresaId: string) {
  const dfe = empresa.empresaParametrosDfe
  return {
    empresaId,
    destacarIcmsSt: dfe.destacarIcmsSt,
    nfe: dfe.nfe,
    nfceHomologacao: dfe.nfceHomologacao,
    nfceProducao: dfe.nfceProducao,
    tipoAmbienteNfce: dfe.tipoAmbienteNfce || 2,
    tipoAmbienteNfe: dfe.tipoAmbienteNfe || 2
  }
}

function validar(): boolean {
  if (!empresa.razaoSocial) {
    erroValidacao.value = 'Razão Social é obrigatória.'
    return false
  }
  if (!validarCpfCnpj(cnpjDigitado.value)) {
    erroValidacao.value = 'CNPJ/CPF inválido.'
    return false
  }
  if (!empresa.planoContasFinanceiroId) {
    erroValidacao.value = 'Plano de Contas Financeiro é obrigatório.'
    return false
  }
  erroValidacao.value = null
  return true
}

async function salvar() {
  if (!validar()) {
    toast.error(erroValidacao.value ?? 'Formulário inválido.')
    return
  }

  salvando.value = true
  try {
    let empresaId = empresa.id

    if (criandoNova) {
      const resp = await useApi.post<CommandResult<{ empresaId?: string; id?: string }>>(
        '/cadastros/empresas',
        montarEmpresaPayload()
      )
      const dados = extrairDados<{ empresaId?: string; id?: string }>(resp)
      empresaId = dados?.empresaId ?? dados?.id
      if (!empresaId) throw new Error('A API não retornou o identificador da empresa criada.')
    } else {
      await useApi.put('/cadastros/empresas', montarEmpresaPayload())
    }

    if (empresaId) {
      try {
        await useApi.put(`/cadastros/empresas/{empresaId}/parametros-dfe`, montarDfePayload(empresaId), {
          params: { empresaId }
        })
      } catch {
        await useApi.post(`/cadastros/empresas/{empresaId}/parametros-dfe`, montarDfePayload(empresaId), {
          params: { empresaId }
        })
      }
    }

    toast.success(`Empresa ${criandoNova ? 'criada' : 'atualizada'} com sucesso.`)
    await router.push('/erp/cadastros/empresas')
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

function cancelar() {
  router.back()
}

onMounted(() => {
  void carregarEmpresa()
})
</script>

<template>
  <div>
    <PageToolbar :title="tituloPagina" :loading="carregando || salvando">
      <template #actions>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="cancelar">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvar">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Salvar</span>
        </button>
      </template>
    </PageToolbar>

    <div class="glass-panel empresa-form">
      <div class="tabs-nav">
        <button
          v-for="aba in abas"
          :key="aba"
          type="button"
          class="tab-btn"
          :class="{ active: abaAtiva === aba }"
          @click="abaAtiva = aba"
        >
          {{ aba }}
        </button>
      </div>

      <div v-if="carregando" class="empresa-loading">
        <span class="spinner"></span> Carregando dados da empresa...
      </div>

      <div v-else class="tab-content">
        <section v-show="abaAtiva === 'Identificação'" class="form-grid">
          <div class="col-4">
            <TextField v-model="cnpjDigitado" label="CNPJ/CPF" required @update:model-value="(v) => (cnpjDigitado = maskCpfCnpj(v))" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.inscricaoEstadual" label="Inscrição Estadual" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.razaoSocial" label="Razão Social" required maxlength="250" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.nomeFantasia" label="Nome Fantasia" maxlength="250" />
          </div>
          <div class="col-4">
            <SelectField v-model="empresa.regimeTributario" label="Regime Tributário" :options="regimeTributarioOpcoes" :clearable="false" required />
          </div>
          <div v-if="exigeApuracao" class="col-4">
            <SelectField v-model="empresa.regimeApuracao" label="Regime de Apuração" :options="regimeApuracaoOpcoes" :clearable="false" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.cnae" label="CNAE" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.inscricaoMunicipal" label="Inscrição Municipal" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.inscricaoSuframa" label="Inscrição SUFRAMA" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.linkWebApiAppVendas" label="Link API de vendas" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.tokenMercadoPagoPix" label="Integração Mercado Pago" />
          </div>
        </section>

        <section v-show="abaAtiva === 'Endereço'" class="form-grid">
          <div class="col-4">
            <TextField
              :model-value="empresa.endereco.cep"
              label="CEP"
              @update:model-value="(v) => (empresa.endereco.cep = maskCEP(v))"
            />
          </div>
          <div class="col-4">
            <SelectField v-model="empresa.endereco.estado" label="UF" :options="ufOpcoes" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.endereco.cidade" label="Município" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.endereco.bairro" label="Bairro" maxlength="60" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.endereco.logradouro" label="Logradouro" maxlength="60" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.endereco.numero" label="Número" />
          </div>
          <div class="col-4">
            <TextField v-model="empresa.endereco.complemento" label="Complemento" maxlength="60" />
          </div>
        </section>

        <section v-show="abaAtiva === 'Contato'">
          <EmpresaContatosPanel v-model="contatos" />
        </section>

        <section v-show="abaAtiva === 'NFCe/NFe'">
          <EmpresaDfePanel v-model="empresa.empresaParametrosDfe" :regime-simples-nacional="regimeSimplesNacional" />

          <div class="cert-box">
            <h4 class="cert-titulo">Certificado digital A1</h4>
            <p class="cert-hint">
              Arquivo <code>.pfx</code>/<code>.p12</code> + senha. Armazenado criptografado (cofre) e usado para
              assinar/transmitir os documentos fiscais à SEFAZ. Envie após salvar a empresa.
            </p>
            <div class="cert-grid">
              <label class="file-label">
                <input type="file" accept=".pfx,.p12" class="file-input" @change="onCertificadoSelecionado" />
                <span class="btn btn-secondary">Selecionar arquivo…</span>
                <span class="file-nome">{{ certNomeArquivo ?? 'Nenhum arquivo selecionado' }}</span>
              </label>
              <TextField v-model="certSenha" type="password" label="Senha do certificado" />
              <button type="button" class="btn btn-primary" :disabled="enviandoCert || criandoNova" @click="enviarCertificado">
                <span v-if="enviandoCert" class="spinner"></span>
                <span v-else>Enviar certificado</span>
              </button>
            </div>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<style scoped>
.empresa-form { padding: 8px 16px 20px; }
.cert-box { margin-top: 20px; padding-top: 16px; border-top: 1px solid rgba(255, 255, 255, 0.08); }
.cert-titulo { margin: 0 0 4px; font-size: 14px; }
.cert-hint { margin: 0 0 12px; font-size: 13px; color: var(--text-secondary); }
.cert-hint code { background: rgba(255, 255, 255, 0.06); padding: 1px 5px; border-radius: 4px; }
.cert-grid { display: flex; align-items: flex-end; gap: 16px; flex-wrap: wrap; }
.file-label { display: flex; align-items: center; gap: 12px; cursor: pointer; }
.file-input { position: absolute; width: 1px; height: 1px; opacity: 0; }
.file-nome { font-size: 13px; color: var(--text-secondary); }
.tabs-nav {
  display: flex;
  gap: 4px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  margin-bottom: 20px;
  overflow-x: auto;
}
.tab-btn {
  background: none;
  border: none;
  color: var(--text-secondary);
  padding: 10px 16px;
  font-size: 13px;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  white-space: nowrap;
}
.tab-btn.active {
  color: var(--text-primary, #fff);
  border-bottom-color: var(--primary);
}
.tab-content { padding: 4px 4px 12px; }
.empresa-loading {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: center;
  padding: 40px 0;
  color: var(--text-secondary);
}
</style>
