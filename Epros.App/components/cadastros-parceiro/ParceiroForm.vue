<script setup lang="ts">
/**
 * ParceiroForm — formulário completo de parceiro (cliente/fornecedor/transportadora/...).
 *
 * Porta o comportamento de `ParceiroForm.vue` + `useParceiroForm`/`useParceiroFormEnderecos`
 * do legado (Vuetify) para o design system novo (glass-panel/form-grid), com abas:
 * Dados Gerais, Dados Bancários, Contatos, Motorista (condicional), Transportadora (condicional).
 *
 * Uso:
 *   <ParceiroForm :id="id" ref="formRef" />
 *   const ok = await formRef.value.salvar()
 */
import { computed, onMounted } from 'vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import { useParceiroForm } from './useParceiroForm'
import { OPCOES_TIPO_ENDERECO, OPCOES_TIPO_INDICADOR_IE, OPCOES_TIPO_PESSOA } from './parceiroDisplay'

const props = defineProps<{
  id: string
}>()

const {
  ehNovo,
  carregando,
  salvando,
  abaAtiva,
  pessoa,
  ufs,
  municipios,
  classificacoes,
  temClassificacaoObrigatoria,
  alternarClassificacao,
  aoMudarTipoPessoa,
  atualizarIndicadorIe,
  carregarUfs,
  carregarMunicipios,
  consultarCep,
  enderecoModalAberto,
  enderecoEditandoIndex,
  enderecoDraft,
  novoEndereco,
  editarEndereco,
  fecharModalEndereco,
  removerEndereco,
  confirmarEndereco,
  contatoModalAberto,
  contatoEditandoIndex,
  contatoDraft,
  contatosOrdenados,
  novoContato,
  editarContato,
  fecharModalContato,
  removerContato,
  definirContatoPrincipal,
  confirmarContato,
  veiculoModalAberto,
  veiculoModalOrigem,
  veiculoEditandoIndex,
  veiculoDraft,
  novoVeiculo,
  editarVeiculo,
  fecharModalVeiculo,
  removerVeiculo,
  confirmarVeiculo,
  carregar,
  salvar,
  maskCpfCnpj
} = useParceiroForm(props.id)

const opcoesUf = computed(() => ufs.value.map((u) => ({ label: String(u.descricao), value: String(u.id) })))
const opcoesMunicipio = computed(() => municipios.value.map((m) => ({ label: m.descricao, value: String(m.id) })))
const opcoesVinculoMotorista = [
  { label: 'Terceiro', value: 'Terceiro' },
  { label: 'Agregado', value: 'Agregado' },
  { label: 'Funcionário', value: 'Funcionario' }
]
const opcoesTipoVeiculo = [
  { label: 'Caminhão', value: 'Caminhao' },
  { label: 'Carreta', value: 'Carreta' },
  { label: 'Van', value: 'Van' },
  { label: 'Outro', value: 'Outro' }
]

const abas = computed(() => {
  const base: { key: string; label: string }[] = [
    { key: 'dados_gerais', label: 'Dados Gerais' },
    { key: 'bancarios', label: 'Dados Bancários' },
    { key: 'contatos', label: 'Contatos' }
  ]
  if (pessoa.ehMotorista) base.push({ key: 'motorista', label: 'Motorista' })
  if (pessoa.ehTransportadora) base.push({ key: 'transportadora', label: 'Transportadora' })
  return base
})

function aoMudarUfEndereco(uf: string | number | null) {
  enderecoDraft.value.uf = uf ? String(uf) : null
  enderecoDraft.value.municipioId = null
  if (uf) carregarMunicipios(String(uf))
}

function aoDigitarCep() {
  const cep = enderecoDraft.value.cep ?? ''
  if (cep.replace(/\D/g, '').length === 8) consultarCep(cep)
}

onMounted(async () => {
  await carregarUfs()
  await carregar()
})

defineExpose({ salvar, carregando: computed(() => carregando.value || salvando.value) })
</script>

<template>
  <div class="parceiro-form">
    <!-- Tipo de pessoa + classificações -->
    <div class="glass-panel bloco">
      <div class="linha-tipo-pessoa">
        <span class="field-label">Tipo de Pessoa</span>
        <div class="grupo-botoes">
          <button
            v-for="opt in OPCOES_TIPO_PESSOA"
            :key="opt.value"
            type="button"
            class="btn btn-sm"
            :class="pessoa.tipoPessoa === opt.value ? 'btn-primary' : 'btn-secondary'"
            @click="aoMudarTipoPessoa(opt.value as typeof pessoa.tipoPessoa)"
          >
            {{ opt.label }}
          </button>
        </div>
      </div>

      <div class="classificacoes" :class="{ 'com-erro': !temClassificacaoObrigatoria }">
        <label v-for="c in classificacoes" :key="c.field" class="chip chip-classificacao" :class="{ ativo: !!pessoa[c.field] }">
          <input
            type="checkbox"
            :checked="!!pessoa[c.field]"
            @change="alternarClassificacao(c.field, ($event.target as HTMLInputElement).checked)"
          />
          {{ c.label }}
        </label>
      </div>
      <p v-if="!temClassificacaoObrigatoria" class="field-error">
        Selecione ao menos uma classificação: Cliente, Fornecedor, Transportadora, Prestador de Serviço, Motorista ou Funcionário.
      </p>
    </div>

    <!-- Abas -->
    <div class="glass-panel bloco">
      <div class="tabs">
        <button
          v-for="a in abas"
          :key="a.key"
          type="button"
          class="tab-btn"
          :class="{ ativo: abaAtiva === a.key }"
          @click="abaAtiva = a.key as typeof abaAtiva"
        >
          {{ a.label }}
        </button>
      </div>

      <!-- Aba: Dados Gerais -->
      <div v-if="abaAtiva === 'dados_gerais'" class="tab-conteudo">
        <div v-if="!pessoa.tipoPessoa" class="vazio">Selecione o Tipo de Pessoa para continuar.</div>
        <template v-else>
          <div class="form-grid">
            <template v-if="pessoa.tipoPessoa === 'PessoaFisica' && pessoa.pessoaFisica">
              <TextField
                v-model="pessoa.pessoaFisica.cpf"
                class="col-3"
                label="CPF"
                required
                placeholder="000.000.000-00"
              />
              <TextField v-model="pessoa.pessoaFisica.nome" class="col-5" label="Nome" required />
              <TextField v-model="pessoa.pessoaFisica.sobrenome" class="col-4" label="Sobrenome" required />
              <TextField v-model="pessoa.pessoaFisica.rgNumero" class="col-3" label="RG" />
              <TextField v-model="pessoa.pessoaFisica.rgOrgaoEmissor" class="col-3" label="Órgão Emissor" />
              <SelectField
                v-model="pessoa.tipoIndicadorIe"
                class="col-3"
                label="Indicador IE"
                required
                :options="OPCOES_TIPO_INDICADOR_IE"
              />
              <TextField v-model="pessoa.inscricaoSuframa" class="col-3" label="Suframa" />
            </template>

            <template v-if="pessoa.tipoPessoa === 'PessoaJuridica' && pessoa.pessoaJuridica">
              <TextField
                v-model="pessoa.pessoaJuridica.cnpj"
                class="col-3"
                label="CNPJ"
                required
                placeholder="00.000.000/0000-00"
                @blur="atualizarIndicadorIe"
              />
              <TextField v-model="pessoa.pessoaJuridica.razaoSocial" class="col-5" label="Razão Social" required :maxlength="60" />
              <TextField v-model="pessoa.pessoaJuridica.nomeFantasia" class="col-4" label="Nome Fantasia" />
              <SelectField v-model="pessoa.tipoIndicadorIe" class="col-3" label="Indicador IE" required :options="OPCOES_TIPO_INDICADOR_IE" />
              <TextField
                v-model="pessoa.pessoaJuridica.inscricaoEstadual"
                class="col-3"
                label="Inscrição Estadual"
                @blur="atualizarIndicadorIe"
              />
              <TextField v-model="pessoa.pessoaJuridica.inscricaoMunicipal" class="col-3" label="Inscrição Municipal" />
              <TextField v-model="pessoa.inscricaoSuframa" class="col-3" label="Suframa" />
              <TextField v-model="pessoa.pessoaJuridica.cnae" class="col-3" label="CNAE" placeholder="00.00-0/00" />
            </template>

            <template v-if="pessoa.tipoPessoa === 'PessoaEstrangeira' && pessoa.pessoaEstrangeiro">
              <TextField v-model="pessoa.pessoaEstrangeiro.nome" class="col-12" label="Nome" required />
              <TextField v-model="pessoa.pessoaEstrangeiro.identificacaoEstrangeiro" class="col-6" label="Identificação Estrangeiro" />
              <SelectField v-model="pessoa.tipoIndicadorIe" class="col-6" label="Indicador IE" required :options="OPCOES_TIPO_INDICADOR_IE" />
            </template>

            <SelectField
              v-if="pessoa.ehCliente && pessoa.pessoaCliente"
              v-model="pessoa.pessoaCliente.tipoContribuinte"
              class="col-4"
              label="Contribuinte"
              :options="[{ label: 'Contribuinte', value: 'Contribuinte' }, { label: 'Isento', value: 'Isento' }, { label: 'Não Informado', value: 'NaoInformado' }]"
            />
            <TextField
              v-if="pessoa.ehPrestadorServico && pessoa.pessoaPrestadorServico"
              v-model="pessoa.pessoaPrestadorServico.cei"
              class="col-3"
              label="CEI"
            />
            <SelectField
              v-if="pessoa.ehFuncionario && pessoa.pessoaFuncionario"
              v-model="pessoa.pessoaFuncionario.tipoCargo"
              class="col-3"
              label="Tipo Cargo"
              required
              :options="[{ label: 'Vendedor', value: 'Vendedor' }, { label: 'Administrativo', value: 'Administrativo' }, { label: 'Motorista', value: 'Motorista' }]"
            />
            <PercentInput
              v-if="pessoa.ehFuncionario && pessoa.pessoaFuncionario"
              v-model="pessoa.pessoaFuncionario.valorPercentualComissao"
              class="col-2"
              label="Comissão"
            />
            <label v-if="pessoa.ehCliente && pessoa.pessoaCliente" class="col-3 field campo-switch">
              <span class="field-label">Consumidor Final</span>
              <input v-model="pessoa.pessoaCliente.ehConsumidorFinal" type="checkbox" class="switch" />
            </label>
          </div>

          <!-- Endereços -->
          <div class="secao-header">
            <h3>Endereços</h3>
            <button type="button" class="btn btn-primary btn-sm" @click="novoEndereco">+ Adicionar Endereço</button>
          </div>
          <div class="table-wrap">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Tipo</th>
                  <th>UF</th>
                  <th>Município</th>
                  <th>CEP</th>
                  <th>Logradouro</th>
                  <th>Bairro</th>
                  <th>Número</th>
                  <th class="td-actions">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="pessoa.enderecos.length === 0">
                  <td colspan="8" class="table-empty">Nenhum endereço cadastrado.</td>
                </tr>
                <tr v-for="(end, index) in pessoa.enderecos" :key="index">
                  <td>{{ end.tipoEndereco }}</td>
                  <td>{{ end.paisId === 1058 ? end.uf : 'EX' }}</td>
                  <td>{{ end.paisId === 1058 ? (end.municipioNome ?? '') : 'EXTERIOR' }}</td>
                  <td>{{ end.cep }}</td>
                  <td>{{ end.logradouro }}</td>
                  <td>{{ end.bairro }}</td>
                  <td>{{ end.numero }}</td>
                  <td class="td-actions">
                    <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click="editarEndereco(index)">Editar</button>
                    <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click="removerEndereco(index)">Excluir</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </div>

      <!-- Aba: Dados Bancários -->
      <div v-else-if="abaAtiva === 'bancarios'" class="tab-conteudo">
        <div class="form-grid">
          <TextField v-model="pessoa.titularContaBancaria" class="col-5" label="Titular da Conta" />
          <TextField v-model="pessoa.agenciaContaBancaria" class="col-2" label="Agência" />
          <TextField v-model="pessoa.numeroContaBancaria" class="col-2" label="Conta" />
          <SelectField
            v-model="pessoa.tipoPix"
            class="col-3"
            label="Tipo PIX"
            :options="[{ label: 'CPF/CNPJ', value: 'CpfCnpj' }, { label: 'E-mail', value: 'Email' }, { label: 'Telefone', value: 'Telefone' }, { label: 'Aleatória', value: 'Aleatoria' }]"
          />
          <TextField v-model="pessoa.chavePix" class="col-4" label="Chave PIX" />
        </div>
      </div>

      <!-- Aba: Contatos -->
      <div v-else-if="abaAtiva === 'contatos'" class="tab-conteudo">
        <div class="secao-header">
          <span />
          <button type="button" class="btn btn-primary btn-sm" @click="novoContato">+ Adicionar Contato</button>
        </div>
        <div class="table-wrap">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th>Telefone</th>
                <th class="td-center">Principal</th>
                <th class="td-actions">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="contatosOrdenados.length === 0">
                <td colspan="5" class="table-empty">Nenhum contato cadastrado.</td>
              </tr>
              <tr v-for="(c, index) in contatosOrdenados" :key="index">
                <td>{{ c.nome }}</td>
                <td>{{ c.email }}</td>
                <td>{{ c.numeroTelefone }}</td>
                <td class="td-center">
                  <button type="button" class="btn btn-ghost btn-sm" @click="definirContatoPrincipal(pessoa.contatos.indexOf(c))">
                    {{ c.ehPrincipal ? '★' : '☆' }}
                  </button>
                </td>
                <td class="td-actions">
                  <button type="button" class="btn btn-ghost btn-sm" @click="editarContato(pessoa.contatos.indexOf(c))">Editar</button>
                  <button type="button" class="btn btn-ghost btn-sm" @click="removerContato(pessoa.contatos.indexOf(c))">Excluir</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Aba: Motorista -->
      <div v-else-if="abaAtiva === 'motorista' && pessoa.pessoaMotorista" class="tab-conteudo">
        <div class="form-grid">
          <SelectField
            v-model="pessoa.pessoaMotorista.tipoVinculoMotorista"
            class="col-6"
            label="Tipo de vínculo"
            required
            :options="opcoesVinculoMotorista"
          />
          <div class="col-6 acao-veiculo">
            <button type="button" class="btn btn-primary btn-sm" @click="novoVeiculo('motorista')">+ Adicionar Veículo</button>
          </div>
        </div>
        <div class="table-wrap">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Placa</th>
                <th>UF</th>
                <th>RNTRC</th>
                <th class="td-actions">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="(pessoa.pessoaMotorista.veiculos ?? []).length === 0">
                <td colspan="4" class="table-empty">Nenhum veículo cadastrado.</td>
              </tr>
              <tr v-for="(v, index) in pessoa.pessoaMotorista.veiculos" :key="index">
                <td>{{ v.placa }}</td>
                <td>{{ v.uf }}</td>
                <td>{{ v.rntrc }}</td>
                <td class="td-actions">
                  <button type="button" class="btn btn-ghost btn-sm" @click="editarVeiculo('motorista', index)">Editar</button>
                  <button type="button" class="btn btn-ghost btn-sm" @click="removerVeiculo('motorista', index)">Excluir</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Aba: Transportadora -->
      <div v-else-if="abaAtiva === 'transportadora' && pessoa.pessoaTransportadora" class="tab-conteudo">
        <div class="secao-header">
          <h3>Veículos</h3>
          <button type="button" class="btn btn-primary btn-sm" @click="novoVeiculo('transportadora')">+ Adicionar Veículo</button>
        </div>
        <div class="table-wrap">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Placa</th>
                <th>UF</th>
                <th>RNTRC</th>
                <th class="td-actions">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="(pessoa.pessoaTransportadora.veiculos ?? []).length === 0">
                <td colspan="4" class="table-empty">Nenhum veículo cadastrado.</td>
              </tr>
              <tr v-for="(v, index) in pessoa.pessoaTransportadora.veiculos" :key="index">
                <td>{{ v.placa }}</td>
                <td>{{ v.uf }}</td>
                <td>{{ v.rntrc }}</td>
                <td class="td-actions">
                  <button type="button" class="btn btn-ghost btn-sm" @click="editarVeiculo('transportadora', index)">Editar</button>
                  <button type="button" class="btn btn-ghost btn-sm" @click="removerVeiculo('transportadora', index)">Excluir</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="carregando" class="overlay-carregando">
      <span class="spinner"></span>
    </div>

    <!-- Modal: Endereço -->
    <AppDialog :model-value="enderecoModalAberto" :title="enderecoEditandoIndex === null ? 'Novo Endereço' : 'Editar Endereço'" width="820px" persistent @update:model-value="fecharModalEndereco">
      <div class="form-grid">
        <TextField :model-value="String(enderecoDraft.paisId)" class="col-3" label="País (código IBGE)" disabled hint="1058 = Brasil" />
        <TextField
          v-if="enderecoDraft.paisId === 1058"
          v-model="enderecoDraft.cep"
          class="col-3"
          label="CEP"
          required
          placeholder="00000-000"
          @blur="aoDigitarCep"
        />
        <SelectField
          v-if="enderecoDraft.paisId === 1058"
          :model-value="enderecoDraft.uf"
          class="col-2"
          label="UF"
          required
          :options="opcoesUf"
          @update:model-value="aoMudarUfEndereco"
        />
        <SelectField
          v-if="enderecoDraft.paisId === 1058"
          v-model="enderecoDraft.municipioId"
          class="col-4"
          label="Município"
          required
          :options="opcoesMunicipio"
        />
        <TextField v-model="enderecoDraft.bairro" class="col-6" label="Bairro" required />
        <TextField v-model="enderecoDraft.logradouro" class="col-10" label="Logradouro" required />
        <TextField v-model="enderecoDraft.numero" class="col-2" label="Número" required />
        <TextField v-model="enderecoDraft.complemento" class="col-4" label="Complemento" />
        <TextField v-model="enderecoDraft.referencia" class="col-4" label="Referência" />
        <SelectField v-model="enderecoDraft.tipoEndereco" class="col-4" label="Tipo Endereço" required :options="OPCOES_TIPO_ENDERECO" />
        <template v-if="enderecoDraft.tipoEndereco === 'Entrega'">
          <TextField v-model="enderecoDraft.nomeDoRecebedor" class="col-8" label="Nome do Recebedor" required />
          <TextField v-model="enderecoDraft.documentoDoRecebedor" class="col-4" label="Documento do Recebedor" required />
        </template>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fecharModalEndereco">Cancelar</button>
        <button type="button" class="btn btn-primary" @click="confirmarEndereco">
          {{ enderecoEditandoIndex === null ? 'Adicionar' : 'Atualizar' }}
        </button>
      </template>
    </AppDialog>

    <!-- Modal: Contato -->
    <AppDialog :model-value="contatoModalAberto" :title="contatoEditandoIndex === null ? 'Novo contato' : 'Editar contato'" width="560px" persistent @update:model-value="fecharModalContato">
      <div class="form-grid">
        <TextField v-model="contatoDraft.nome" class="col-12" label="Nome" required />
        <TextField v-model="contatoDraft.email" class="col-7" label="E-mail" type="email" />
        <SelectField
          v-model="contatoDraft.tipoContatoEmail"
          class="col-5"
          label="Tipo Contato E-mail"
          :options="[{ label: 'Comercial', value: 'Comercial' }, { label: 'Financeiro', value: 'Financeiro' }, { label: 'Pessoal', value: 'Pessoal' }]"
        />
        <TextField v-model="contatoDraft.numeroTelefone" class="col-5" label="Telefone" />
        <SelectField
          v-model="contatoDraft.tipoContatoTelefonico"
          class="col-7"
          label="Tipo Contato Telefônico"
          :options="[{ label: 'Celular', value: 'Celular' }, { label: 'Fixo', value: 'Fixo' }, { label: 'WhatsApp', value: 'WhatsApp' }]"
        />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fecharModalContato">Cancelar</button>
        <button type="button" class="btn btn-primary" @click="confirmarContato">
          {{ contatoEditandoIndex === null ? 'Adicionar' : 'Atualizar' }}
        </button>
      </template>
    </AppDialog>

    <!-- Modal: Veículo -->
    <AppDialog :model-value="veiculoModalAberto" :title="veiculoEditandoIndex === null ? 'Novo veículo' : 'Editar veículo'" width="560px" persistent @update:model-value="fecharModalVeiculo">
      <div class="form-grid">
        <TextField :model-value="String(veiculoDraft.paisId ?? '')" class="col-6" label="País (código IBGE)" disabled hint="1058 = Brasil" />
        <SelectField v-model="veiculoDraft.tipoVeiculo" class="col-6" label="Tipo do veículo" required :options="opcoesTipoVeiculo" />
        <SelectField v-model="veiculoDraft.uf" class="col-4" label="UF" required :options="opcoesUf" />
        <TextField v-model="veiculoDraft.placa" class="col-4" label="Placa" required placeholder="AAA-0A00" />
        <TextField v-model="veiculoDraft.rntrc" class="col-4" label="RNTRC" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fecharModalVeiculo">Cancelar</button>
        <button type="button" class="btn btn-primary" @click="confirmarVeiculo">Salvar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.parceiro-form { display: flex; flex-direction: column; gap: 16px; position: relative; }
.bloco { padding: 16px 20px; }
.linha-tipo-pessoa { display: flex; align-items: center; gap: 14px; margin-bottom: 12px; flex-wrap: wrap; }
.grupo-botoes { display: flex; gap: 6px; flex-wrap: wrap; }
.classificacoes { display: flex; gap: 8px; flex-wrap: wrap; }
.classificacoes.com-erro { outline: 1px dashed var(--danger); border-radius: 10px; padding: 8px; }
.chip-classificacao { display: inline-flex; align-items: center; gap: 6px; cursor: pointer; user-select: none; }
.chip-classificacao input { accent-color: var(--primary); }
.chip-classificacao.ativo { background: var(--primary-glow); border-color: rgba(99, 102, 241, 0.3); color: var(--primary-hover); }

.tabs { display: flex; gap: 4px; border-bottom: 1px solid rgba(255, 255, 255, 0.08); margin-bottom: 16px; flex-wrap: wrap; }
.tab-btn {
  background: transparent; border: none; color: var(--text-secondary); padding: 10px 16px;
  cursor: pointer; font-size: 13px; font-weight: 600; border-bottom: 2px solid transparent;
}
.tab-btn.ativo { color: var(--primary-hover); border-bottom-color: var(--primary); }
.tab-conteudo { padding-top: 4px; }
.vazio { text-align: center; padding: 40px 0; color: var(--text-muted); }

.secao-header { display: flex; align-items: center; justify-content: space-between; margin: 18px 0 10px; }
.secao-header h3 { font-size: 14px; font-weight: 700; }

.campo-switch { display: flex; flex-direction: column; gap: 6px; justify-content: center; }
.switch { width: 36px; height: 20px; accent-color: var(--primary); }
.acao-veiculo { display: flex; align-items: flex-end; justify-content: flex-end; }

.overlay-carregando {
  position: absolute; inset: 0; display: flex; align-items: center; justify-content: center;
  background: rgba(10, 10, 12, 0.4); border-radius: 16px; backdrop-filter: blur(2px);
}
</style>
