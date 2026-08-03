<template>
  <div class="pessoas-module">
    <!-- Listagem Principal -->
    <div v-if="!showForm" class="animate-fade">
      <header class="tab-header flex-row-between">
        <div>
          <h2>Gestão de Pessoas & Contatos</h2>
          <p>Gerencie o cadastro de Clientes, Fornecedores, Motoristas, Transportadoras e Funcionários.</p>
        </div>
        <button @click="openCreateForm" class="btn btn-primary">
          👥 Novo Cadastro
        </button>
      </header>

      <!-- Filtros -->
      <section class="filters-bar glass-panel flex-row-between">
        <div class="filters-left">
          <input 
            type="text" 
            v-model="filterName" 
            placeholder="🔍 Buscar por nome, CPF ou CNPJ..." 
            class="search-input" 
            @input="debounceSearch"
          />
          
          <select v-model="filterRole" @change="loadPessoas" class="select-input">
            <option value="">Todos os Papéis</option>
            <option value="ehCliente">Clientes</option>
            <option value="ehFornecedor">Fornecedores</option>
            <option value="ehTransportadora">Transportadoras</option>
            <option value="ehMotorista">Motoristas</option>
            <option value="ehFuncionario">Funcionários</option>
          </select>

          <select v-model="filterStatus" @change="loadPessoas" class="select-input">
            <option value="">Todos os Status</option>
            <option value="Ativo">Ativos</option>
            <option value="Inativo">Inativos</option>
          </select>
        </div>
        <button @click="loadPessoas" class="btn btn-secondary btn-small">
          🔄 Atualizar
        </button>
      </section>

      <!-- Tabela -->
      <div class="table-container glass-panel">
        <table class="workspace-table">
          <thead>
            <tr>
              <th>Nome / Razão Social</th>
              <th>Tipo</th>
              <th>Documento</th>
              <th>Papéis Ativos</th>
              <th>Status</th>
              <th style="text-align: right;">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in pessoas" :key="p.id" class="table-row-hover">
              <td class="bold-text">
                {{ getPessoaName(p) }}
              </td>
              <td>
                <span class="type-badge" :class="p.tipoPessoa === 1 ? 'pf-type' : p.tipoPessoa === 2 ? 'pj-type' : 'pe-type'">
                  {{ p.tipoPessoa === 1 ? 'Física' : p.tipoPessoa === 2 ? 'Jurídica' : 'Estrangeira' }}
                </span>
              </td>
              <td>
                {{ formatDocumento(p) }}
              </td>
              <td>
                <div class="roles-badges">
                  <span v-if="p.ehCliente" class="role-badge client-badge">Cliente</span>
                  <span v-if="p.ehFornecedor" class="role-badge provider-badge">Fornecedor</span>
                  <span v-if="p.ehTransportadora" class="role-badge carrier-badge">Transp</span>
                  <span v-if="p.ehMotorista" class="role-badge driver-badge">Motorista</span>
                  <span v-if="p.ehFuncionario" class="role-badge employee-badge">Func</span>
                  <span v-if="p.ehProdutorRural" class="role-badge farmer-badge">Produtor</span>
                </div>
              </td>
              <td>
                <span :class="['badge', p.status === 1 ? 'badge-paga' : 'badge-cancelada']">
                  {{ p.status === 1 ? 'Ativo' : 'Inativo' }}
                </span>
              </td>
              <td style="text-align: right;">
                <div style="display: flex; gap: 8px; justify-content: flex-end;">
                  <button @click="openEditForm(p.id)" class="btn-api-sub">✏️ Editar</button>
                  <button @click="deletePessoa(p.id, getPessoaName(p))" class="btn-api-sub delete-btn">🗑️ Excluir</button>
                </div>
              </td>
            </tr>
            <tr v-if="pessoas.length === 0 && !loading">
              <td colspan="6" style="text-align: center; color: var(--text-muted); padding: 30px;">
                Nenhuma pessoa cadastrada ou encontrada.
              </td>
            </tr>
            <tr v-if="loading">
              <td colspan="6" style="text-align: center; padding: 30px;">
                <span class="spinner"></span> Carregando...
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Paginação -->
      <div class="pagination flex-row-between" v-if="total > tamanhoPagina">
        <span class="kpi-sub">Total: {{ total }} registros</span>
        <div style="display: flex; gap: 8px;">
          <button @click="prevPage" :disabled="pagina === 1" class="btn btn-secondary btn-small">◀ Anterior</button>
          <span class="page-indicator">Página {{ pagina }}</span>
          <button @click="nextPage" :disabled="pagina * tamanhoPagina >= total" class="btn btn-secondary btn-small">Próxima ▶</button>
        </div>
      </div>
    </div>

    <!-- Form Cadastro/Edição -->
    <div v-else class="animate-fade">
      <header class="tab-header flex-row-between">
        <div>
          <h2>{{ isEditing ? '🔄 Editar Pessoa / Parceiro' : '⚡ Cadastrar Pessoa / Parceiro' }}</h2>
          <p>Preencha os dados cadastrais, papéis operacionais, contatos e endereços.</p>
        </div>
        <button @click="closeForm" class="btn btn-secondary">
          Voltar para Listagem
        </button>
      </header>

      <!-- Abas Internas do Formulário -->
      <div class="form-wrapper glass-panel">
        <div class="form-tabs-nav">
          <button type="button" @click="activeFormTab = 'identificacao'" :class="['form-tab-btn', { 'active': activeFormTab === 'identificacao' }]">👤 Identificação</button>
          <button type="button" @click="activeFormTab = 'papeis'" :class="['form-tab-btn', { 'active': activeFormTab === 'papeis' }]">🎖️ Papéis Ativos</button>
          <button type="button" @click="activeFormTab = 'enderecos'" :class="['form-tab-btn', { 'active': activeFormTab === 'enderecos' }]">📍 Endereços</button>
          <button type="button" @click="activeFormTab = 'contatos'" :class="['form-tab-btn', { 'active': activeFormTab === 'contatos' }]">📞 Contatos</button>
          <button type="button" @click="activeFormTab = 'financeiro'" :class="['form-tab-btn', { 'active': activeFormTab === 'financeiro' }]">💸 Financas & PIX</button>
        </div>

        <form @submit.prevent="saveForm" class="config-form" style="padding: 24px;">
          
          <!-- TAB 1: IDENTIFICAÇÃO -->
          <div v-show="activeFormTab === 'identificacao'" class="animate-fade">
            <div class="form-grid-3" style="margin-bottom: 20px;">
              <div class="form-group">
                <label>Tipo de Pessoa *</label>
                <select v-model.number="form.tipoPessoa" required :disabled="isEditing">
                  <option :value="1">Pessoa Física</option>
                  <option :value="2">Pessoa Jurídica</option>
                  <option :value="3">Estrangeiro / Internacional</option>
                </select>
              </div>
              <div class="form-group">
                <label>Indicador IE *</label>
                <select v-model.number="form.tipoIndicadorIe" required>
                  <option :value="1">Contribuinte ICMS</option>
                  <option :value="2">Isento</option>
                  <option :value="3">Não Contribuinte</option>
                </select>
              </div>
              <div class="form-group">
                <label>Grupo de Pessoa</label>
                <select v-model="form.pessoaGrupoId">
                  <option :value="null">Nenhum Grupo</option>
                  <option v-for="g in pessoaGrupos" :key="g.id" :value="g.id">{{ g.descricao }}</option>
                </select>
              </div>
            </div>

            <!-- Campos: PESSOA FÍSICA -->
            <div v-if="form.tipoPessoa === 1" class="sub-form-section">
              <h4 class="form-section-title">Dados de Pessoa Física</h4>
              <div class="form-grid-3">
                <div class="form-group">
                  <label>CPF *</label>
                  <input type="text" v-model="form.fisicaCpf" required placeholder="Ex: 123.456.789-00" @input="applyCpfMask" />
                </div>
                <div class="form-group">
                  <label>Nome *</label>
                  <input type="text" v-model="form.fisicaNome" required placeholder="Ex: Maria" />
                </div>
                <div class="form-group">
                  <label>Sobrenome</label>
                  <input type="text" v-model="form.fisicaSobrenome" placeholder="Ex: da Silva" />
                </div>
              </div>
              <div class="form-grid-4">
                <div class="form-group">
                  <label>RG</label>
                  <input type="text" v-model="form.rgNumero" placeholder="Número do RG" />
                </div>
                <div class="form-group">
                  <label>Órgão Emissor</label>
                  <input type="text" v-model="form.rgOrgaoEmissor" placeholder="Ex: SSP/SP" />
                </div>
                <div class="form-group">
                  <label>Gênero</label>
                  <select v-model.number="form.tipoGenero">
                    <option :value="1">Não Utiliza</option>
                    <option :value="2">Feminino</option>
                    <option :value="3">Masculino</option>
                    <option :value="4">Outros</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Data de Nascimento</label>
                  <input type="date" v-model="form.dataNascimento" />
                </div>
              </div>
            </div>

            <!-- Campos: PESSOA JURÍDICA -->
            <div v-if="form.tipoPessoa === 2" class="sub-form-section">
              <h4 class="form-section-title">Dados de Pessoa Jurídica</h4>
              <div class="form-grid-3">
                <div class="form-group">
                  <label>CNPJ *</label>
                  <input type="text" v-model="form.juridicaCnpj" required placeholder="Ex: 12.345.678/0001-90" @input="applyCnpjMask" />
                </div>
                <div class="form-group" style="grid-column: span 2;">
                  <label>Razão Social *</label>
                  <input type="text" v-model="form.razaoSocial" required placeholder="Ex: Empresa de Alimentos Ltda" />
                </div>
              </div>
              <div class="form-grid-3">
                <div class="form-group">
                  <label>Nome Fantasia</label>
                  <input type="text" v-model="form.nomeFantasia" placeholder="Ex: Comercial Alimentos" />
                </div>
                <div class="form-group">
                  <label>Inscrição Estadual</label>
                  <input type="text" v-model="form.inscricaoEstadual" placeholder="IE do emitente" />
                </div>
                <div class="form-group">
                  <label>Inscrição Municipal</label>
                  <input type="text" v-model="form.inscricaoMunicipal" placeholder="IM se houver" />
                </div>
              </div>
              <div class="form-grid-2">
                <div class="form-group">
                  <label>CNAE Principal</label>
                  <input type="text" v-model="form.cnae" placeholder="Ex: 4711302" />
                </div>
                <div class="form-group">
                  <label>Inscrição SUFRAMA</label>
                  <input type="number" v-model.number="form.inscricaoSuframa" placeholder="Inscrição de benefícios fiscais" />
                </div>
              </div>
            </div>

            <!-- Campos: PESSOA ESTRANGEIRA -->
            <div v-if="form.tipoPessoa === 3" class="sub-form-section">
              <h4 class="form-section-title">Dados de Pessoa Estrangeira</h4>
              <div class="form-grid-2">
                <div class="form-group">
                  <label>Nome do Estrangeiro / Organização *</label>
                  <input type="text" v-model="form.estrangeiroNome" required placeholder="Ex: John Smith / Global Tech" />
                </div>
                <div class="form-group">
                  <label>Identificação Estrangeira (VAT / Tax ID)</label>
                  <input type="text" v-model="form.identificacaoEstrangeiro" placeholder="Ex: US123456789" />
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 2: PAPÉIS ATIVOS -->
          <div v-show="activeFormTab === 'papeis'" class="animate-fade">
            <h4 class="form-section-title">Quais papéis esta pessoa assume na operação?</h4>
            <div class="roles-checkboxes-grid">
              <label class="checkbox-card glass-panel" :class="{ 'checked': form.ehCliente }">
                <input type="checkbox" v-model="form.ehCliente" />
                <div class="checkbox-card-info">
                  <span class="title">🛒 Cliente</span>
                  <span class="desc">Consome produtos ou contrata serviços.</span>
                </div>
              </label>

              <label class="checkbox-card glass-panel" :class="{ 'checked': form.ehFornecedor }">
                <input type="checkbox" v-model="form.ehFornecedor" />
                <div class="checkbox-card-info">
                  <span class="title">📦 Fornecedor</span>
                  <span class="desc">Fornece insumos ou produtos para revenda.</span>
                </div>
              </label>

              <label class="checkbox-card glass-panel" :class="{ 'checked': form.ehTransportadora }">
                <input type="checkbox" v-model="form.ehTransportadora" />
                <div class="checkbox-card-info">
                  <span class="title">🚛 Transportadora</span>
                  <span class="desc">Realiza movimentações logísticas.</span>
                </div>
              </label>

              <label class="checkbox-card glass-panel" :class="{ 'checked': form.ehMotorista }">
                <input type="checkbox" v-model="form.ehMotorista" />
                <div class="checkbox-card-info">
                  <span class="title">👨‍✈️ Motorista</span>
                  <span class="desc">Condutor físico dos veículos.</span>
                </div>
              </label>

              <label class="checkbox-card glass-panel" :class="{ 'checked': form.ehFuncionario }">
                <input type="checkbox" v-model="form.ehFuncionario" />
                <div class="checkbox-card-info">
                  <span class="title">👔 Funcionário / Vendedor</span>
                  <span class="desc">Trabalha na equipe operacional ou comercial.</span>
                </div>
              </label>
            </div>

            <!-- Dados condicionais: CLIENTE -->
            <div v-if="form.ehCliente" class="sub-form-section animate-scale-in" style="margin-top: 20px;">
              <h4 class="form-section-title">Especificações de Cliente</h4>
              <div class="form-grid-3">
                <div class="form-group" style="flex-direction: row; align-items: center; gap: 8px;">
                  <input type="checkbox" v-model="form.clienteEhConsumidorFinal" style="width: 18px; height: 18px;" />
                  <label style="margin: 0; text-transform: none;">Consumidor Final</label>
                </div>
                <div class="form-group">
                  <label>Regime / Tipo Contribuinte</label>
                  <select v-model.number="form.clienteTipoContribuinte">
                    <option :value="1">Não Informado</option>
                    <option :value="2">Simples Nacional</option>
                    <option :value="3">RPA</option>
                    <option :value="4">MEI</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Limite de Crédito (R$)</label>
                  <input type="number" step="0.01" v-model.number="form.limiteCredito" placeholder="Ex: 5000.00" />
                </div>
              </div>
            </div>

            <!-- Dados condicionais: FUNCIONÁRIO -->
            <div v-if="form.ehFuncionario" class="sub-form-section animate-scale-in" style="margin-top: 20px;">
              <h4 class="form-section-title">Especificações de Funcionário</h4>
              <div class="form-grid-2">
                <div class="form-group">
                  <label>Cargo Operacional</label>
                  <select v-model.number="form.funcionarioTipoCargo">
                    <option :value="1">Operador</option>
                    <option :value="2">Vendedor</option>
                    <option :value="3">Supervisor</option>
                    <option :value="4">Gerente</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Comissão Operacional (%)</label>
                  <input type="number" step="0.01" v-model.number="form.funcionarioComissao" placeholder="Ex: 2.5" />
                </div>
              </div>
            </div>

            <!-- Dados condicionais: MOTORISTA -->
            <div v-if="form.ehMotorista" class="sub-form-section animate-scale-in" style="margin-top: 20px;">
              <h4 class="form-section-title">Especificações de Motorista</h4>
              <div class="form-grid-3">
                <div class="form-group">
                  <label>Tipo de Vínculo</label>
                  <select v-model.number="form.motoristaTipoVinculo">
                    <option :value="1">Próprio</option>
                    <option :value="2">Terceiros</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>Categoria CNH</label>
                  <select v-model.number="form.motoristaCategoriaCnh">
                    <option :value="1">Não Utiliza</option>
                    <option :value="2">A</option>
                    <option :value="3">B</option>
                    <option :value="4">C</option>
                    <option :value="5">D</option>
                    <option :value="6">E</option>
                    <option :value="7">AB</option>
                    <option :value="8">AC</option>
                    <option :value="9">AD</option>
                    <option :value="10">AE</option>
                  </select>
                </div>
                <div class="form-group">
                  <label>RNTRC Motorista</label>
                  <input type="text" v-model="form.motoristaRntrc" placeholder="RNTRC se houver" />
                </div>
              </div>
              <div class="form-grid-2">
                <div class="form-group">
                  <label>Data de Emissão CNH</label>
                  <input type="date" v-model="form.motoristaDataEmissaoCnh" />
                </div>
                <div class="form-group">
                  <label>Data de Vencimento CNH</label>
                  <input type="date" v-model="form.motoristaDataVencimentoCnh" />
                </div>
              </div>
            </div>

            <!-- Dados condicionais: TRANSPORTADORA -->
            <div v-if="form.ehTransportadora" class="sub-form-section animate-scale-in" style="margin-top: 20px;">
              <h4 class="form-section-title">Especificações de Transportadora</h4>
              <div class="form-grid-2">
                <div class="form-group">
                  <label>CIOT</label>
                  <input type="text" v-model="form.transportadoraCiot" placeholder="CIOT logístico" />
                </div>
                <div class="form-group">
                  <label>RNTRC Transportadora</label>
                  <input type="text" v-model="form.transportadoraRntrc" placeholder="RNTRC da Transportadora" />
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 3: ENDEREÇOS -->
          <div v-show="activeFormTab === 'enderecos'" class="animate-fade">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
              <h4 class="form-section-title" style="margin: 0;">📍 Endereços Vinculados</h4>
              <button type="button" @click="addNewEndereco" class="btn btn-secondary btn-small">
                ➕ Adicionar Endereço
              </button>
            </div>

            <div class="addresses-list-container">
              <div v-for="(end, index) in form.enderecos" :key="index" class="address-item-card glass-panel animate-fade">
                <header class="address-card-header">
                  <span class="bold-text">Endereço #{{ index + 1 }}</span>
                  <button type="button" @click="removeEndereco(index)" class="btn btn-logout" style="padding: 2px 8px; font-size: 11px;">
                    Remover
                  </button>
                </header>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label>Tipo de Endereço *</label>
                    <select v-model.number="end.tipoEndereco" required>
                      <option :value="1">Principal</option>
                      <option :value="2">Entrega</option>
                      <option :value="3">Obra</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>País *</label>
                    <select v-model="end.paisId" required>
                      <option v-for="p in paises" :key="p.id" :value="p.id">{{ p.nome }}</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>CEP *</label>
                    <div style="display: flex; gap: 8px;">
                      <input type="text" v-model="end.cep" placeholder="00000-000" required @blur="searchCepOnBlur(index)" />
                    </div>
                  </div>
                </div>

                <div class="form-grid-3">
                  <div class="form-group" style="grid-column: span 2;">
                    <label>Logradouro *</label>
                    <input type="text" v-model="end.logradouro" placeholder="Ex: Av Paulista" required />
                  </div>
                  <div class="form-group">
                    <label>Número</label>
                    <input type="text" v-model="end.numero" placeholder="Ex: 123" />
                  </div>
                </div>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label>Bairro *</label>
                    <input type="text" v-model="end.bairro" placeholder="Ex: Centro" required />
                  </div>
                  <div class="form-group">
                    <label>UF *</label>
                    <select v-model="end.uf" @change="onUfChange(index)" required class="select-input">
                      <option value="">Selecione...</option>
                      <option v-for="uf in ufsList" :key="uf" :value="uf">{{ uf }}</option>
                    </select>
                  </div>
                  <div class="form-group">
                    <label>Município *</label>
                    <select v-model="end.municipioId" required class="select-input">
                      <option value="">Selecione...</option>
                      <option v-for="m in end.municipiosDisponiveis" :key="m.id" :value="m.id">{{ m.nome }}</option>
                    </select>
                  </div>
                </div>

                <div class="form-grid-2">
                  <div class="form-group">
                    <label>Complemento</label>
                    <input type="text" v-model="end.complemento" placeholder="Sala, bloco..." />
                  </div>
                  <div class="form-group">
                    <label>Referência</label>
                    <input type="text" v-model="end.referencia" placeholder="Ex: Próximo ao metrô" />
                  </div>
                </div>
              </div>
              <div v-if="form.enderecos.length === 0" class="text-center text-muted" style="padding: 20px;">
                Nenhum endereço cadastrado. Adicione pelo menos um endereço principal.
              </div>
            </div>
          </div>

          <!-- TAB 4: CONTATOS -->
          <div v-show="activeFormTab === 'contatos'" class="animate-fade">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;">
              <h4 class="form-section-title" style="margin: 0;">📞 Contatos</h4>
              <button type="button" @click="addNewContato" class="btn btn-secondary btn-small">
                ➕ Adicionar Contato
              </button>
            </div>

            <div class="contacts-list-container">
              <div v-for="(c, index) in form.contatos" :key="index" class="address-item-card glass-panel animate-fade" style="padding: 16px;">
                <header class="address-card-header">
                  <span class="bold-text">Contato #{{ index + 1 }}</span>
                  <button type="button" @click="removeContato(index)" class="btn btn-logout" style="padding: 2px 8px; font-size: 11px;">
                    Remover
                  </button>
                </header>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label>Nome do Contato</label>
                    <input type="text" v-model="c.nome" placeholder="Ex: João Compras" />
                  </div>
                  <div class="form-group">
                    <label>E-mail</label>
                    <input type="email" v-model="c.email" placeholder="contato@empresa.com" />
                  </div>
                  <div class="form-group">
                    <label>Tipo E-mail</label>
                    <select v-model.number="c.tipoContatoEmail">
                      <option :value="1">Não Utiliza</option>
                      <option :value="2">Envio NF-e</option>
                      <option :value="3">Envio NFS-e</option>
                      <option :value="4">Contador</option>
                    </select>
                  </div>
                </div>

                <div class="form-grid-3">
                  <div class="form-group">
                    <label>Telefone / Celular</label>
                    <input type="text" v-model="c.numeroTelefone" placeholder="Ex: (11) 99999-9999" />
                  </div>
                  <div class="form-group">
                    <label>Tipo Telefone</label>
                    <select v-model.number="c.tipoContatoTelefonico">
                      <option :value="1">Não Utiliza</option>
                      <option :value="2">Residencial</option>
                      <option :value="3">Comercial</option>
                      <option :value="4">Recado</option>
                      <option :value="5">Emergência</option>
                      <option :value="6">Outros</option>
                    </select>
                  </div>
                  <div class="form-group" style="flex-direction: row; align-items: center; gap: 8px; margin-top: 24px;">
                    <input type="checkbox" v-model="c.ehPrincipal" style="width: 18px; height: 18px;" @change="onContatoPrincipalChange(index)" />
                    <label style="margin: 0; text-transform: none;">Contato Principal</label>
                  </div>
                </div>
              </div>
              <div v-if="form.contatos.length === 0" class="text-center text-muted" style="padding: 20px;">
                Nenhum contato cadastrado.
              </div>
            </div>
          </div>

          <!-- TAB 5: FINANÇAS & PIX -->
          <div v-show="activeFormTab === 'financeiro'" class="animate-fade">
            <h4 class="form-section-title">Dados Bancários</h4>
            <div class="form-grid-3">
              <div class="form-group">
                <label>Titular da Conta</label>
                <input type="text" v-model="form.titularContaBancaria" placeholder="Ex: Titular ME" maxlength="150" />
              </div>
              <div class="form-group">
                <label>Agência</label>
                <input type="text" v-model="form.agenciaContaBancaria" placeholder="Agência" maxlength="20" />
              </div>
              <div class="form-group">
                <label>Número da Conta</label>
                <input type="text" v-model="form.numeroContaBancaria" placeholder="Conta" maxlength="20" />
              </div>
            </div>

            <h4 class="form-section-title" style="margin-top: 24px;">Chave PIX</h4>
            <div class="form-grid-2">
              <div class="form-group">
                <label>Tipo Chave PIX</label>
                <select v-model="form.tipoPix">
                  <option :value="null">Não Utiliza</option>
                  <option :value="1">Chave Aleatória</option>
                  <option :value="2">CPF / CNPJ</option>
                  <option :value="3">E-mail</option>
                  <option :value="4">Telefone</option>
                </select>
              </div>
              <div class="form-group">
                <label>Chave PIX</label>
                <input type="text" v-model="form.chavePix" placeholder="Chave de recebimento PIX" maxlength="32" />
              </div>
            </div>

            <h4 class="form-section-title" style="margin-top: 24px;">Observações Internas</h4>
            <div class="form-group">
              <textarea v-model="form.observacoes" rows="3" placeholder="Insira observações relevantes sobre o parceiro comercial..."></textarea>
            </div>
          </div>

          <!-- Ações de Rodapé do Form -->
          <div class="form-actions" style="margin-top: 30px;">
            <button type="submit" class="btn btn-success" :disabled="saving">
              {{ saving ? 'Salvando...' : '💾 Gravar Cadastro Parceiro' }}
            </button>
            <button type="button" @click="closeForm" class="btn btn-secondary">
              Cancelar
            </button>
          </div>

        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'

// API Online & Dados
const apiOnline = ref(true)
const loading = ref(false)
const saving = ref(false)
const pessoas = ref([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(10)

// Filtros
const filterName = ref('')
const filterRole = ref('')
const filterStatus = ref('')
let searchDebounceTimer = null

// Form State
const showForm = ref(false)
const isEditing = ref(false)
const activeFormTab = ref('identificacao')
const pessoaGrupos = ref([])
const paises = ref([])

const defaultForm = () => ({
  id: null,
  tipoPessoa: 1,
  tipoIndicadorIe: 3,
  pessoaGrupoId: null,
  inscricaoSuframa: null,
  titularContaBancaria: '',
  agenciaContaBancaria: '',
  numeroContaBancaria: '',
  tipoPix: null,
  chavePix: '',
  observacoes: '',

  // Fisica
  fisicaCpf: '',
  fisicaNome: '',
  fisicaSobrenome: '',
  rgNumero: '',
  rgOrgaoEmissor: '',
  tipoGenero: 1,
  dataNascimento: null,

  // Juridica
  juridicaCnpj: '',
  razaoSocial: '',
  nomeFantasia: '',
  inscricaoEstadual: '',
  inscricaoMunicipal: '',
  cnae: '',

  // Estrangeiro
  estrangeiroNome: '',
  identificacaoEstrangeiro: '',

  // Roles
  ehCliente: false,
  ehFornecedor: false,
  ehTransportadora: false,
  ehMotorista: false,
  ehPrestadorServico: false,
  ehFuncionario: false,
  ehProdutorRural: false,

  // Role details
  clienteEhConsumidorFinal: false,
  clienteTipoContribuinte: 1,
  limiteCredito: null,
  funcionarioTipoCargo: 1,
  funcionarioComissao: null,
  motoristaTipoVinculo: 1,
  motoristaCategoriaCnh: 1,
  motoristaDataEmissaoCnh: null,
  motoristaDataVencimentoCnh: null,
  motoristaRntrc: '',
  transportadoraCiot: '',
  transportadoraRntrc: '',
  prestadorCei: '',

  // Listas
  enderecos: [],
  contatos: [],
  veiculos: []
})

const form = ref(defaultForm())

// UFs de Referência
const ufsList = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 
  'MA', 'MT', 'MS', 'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 
  'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
]

// Lifecycle
onMounted(async () => {
  await loadMetadata()
  await loadPessoas()
})

const loadMetadata = async () => {
  try {
    // Carregar Grupos de Pessoa
    const resGrupos = await useApi('/cadastros/pessoa-grupos')
    pessoaGrupos.value = extrairDados(resGrupos) || []

    // Carregar Países
    const resPaises = await useApi('/cadastros/geografia/paises')
    paises.value = extrairDados(resPaises) || []
  } catch (e) {
    console.error('Falha ao carregar metadados da API.', e)
  }
}

const loadPessoas = async () => {
  loading.value = true
  try {
    // Monta a query como objeto — o useApi/ofetch cuida da serialização e do encode.
    const query = {
      pagina: pagina.value,
      tamanhoPagina: tamanhoPagina.value
    }
    if (filterName.value) query.localizar = filterName.value
    if (filterRole.value) query[filterRole.value] = true
    if (filterStatus.value) query.status = filterStatus.value

    const resposta = await useApi('/cadastros/pessoas', { query })
    const dados = extrairDados(resposta) || {}
    pessoas.value = dados.itens || []
    total.value = dados.total || 0
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    console.error('API C# offline. Falha ao listar pessoas.', e)
  } finally {
    loading.value = false
  }
}

const debounceSearch = () => {
  clearTimeout(searchDebounceTimer)
  searchDebounceTimer = setTimeout(() => {
    pagina.value = 1
    loadPessoas()
  }, 400)
}

// Helpers Formatação
const getPessoaName = (p) => {
  if (p.tipoPessoa === 1 && p.pessoaFisica) return `${p.pessoaFisica.nome} ${p.pessoaFisica.sobrenome || ''}`.trim()
  if (p.tipoPessoa === 2 && p.pessoaJuridica) return p.pessoaJuridica.razaoSocial
  if (p.tipoPessoa === 3 && p.pessoaEstrangeiro) return p.pessoaEstrangeiro.nome
  return 'Sem Nome'
}

const formatDocumento = (p) => {
  if (p.tipoPessoa === 1 && p.pessoaFisica) {
    const v = p.pessoaFisica.cpf?.valor || ''
    if (v.length === 11) return v.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
    return v
  }
  if (p.tipoPessoa === 2 && p.pessoaJuridica) {
    const v = p.pessoaJuridica.cnpj?.valor || ''
    if (v.length === 14) return v.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5')
    return v
  }
  if (p.tipoPessoa === 3 && p.pessoaEstrangeiro) {
    return p.pessoaEstrangeiro.identificacaoEstrangeiro || 'Estrangeiro'
  }
  return 'N/A'
}

// Eventos Paginação
const prevPage = () => {
  if (pagina.value > 1) {
    pagina.value--
    loadPessoas()
  }
}
const nextPage = () => {
  if (pagina.value * tamanhoPagina.value < total.value) {
    pagina.value++
    loadPessoas()
  }
}

// Ações do Form
const openCreateForm = () => {
  form.value = defaultForm()
  
  // Pré-popular país padrão Brasil
  const brasil = paises.value.find(p => p.codigoIsoAlpha2 === 'BR')
  if (brasil) {
    // Inicializa com um endereço padrão Principal
    form.value.enderecos.push({
      tipoEndereco: 1,
      paisId: brasil.id,
      municipioId: '',
      uf: '',
      cep: '',
      logradouro: '',
      numero: '',
      complemento: '',
      bairro: '',
      referencia: '',
      municipiosDisponiveis: []
    })
  }

  isEditing.value = false
  showForm.value = true
  activeFormTab.value = 'identificacao'
}

const openEditForm = async (id) => {
  loading.value = true
  try {
    const resposta = await useApi(`/cadastros/pessoas/${id}`)
    const data = extrairDados(resposta)

    // Adaptar retorno do backend para o formato do form reativo
    const p = {
        id: data.id,
        tipoPessoa: data.tipoPessoa,
        tipoIndicadorIe: data.tipoIndicadorIe,
        pessoaGrupoId: data.pessoaGrupoId,
        inscricaoSuframa: data.inscricaoSuframa,
        titularContaBancaria: data.titularContaBancaria || '',
        agenciaContaBancaria: data.agenciaContaBancaria || '',
        numeroContaBancaria: data.numeroContaBancaria || '',
        tipoPix: data.tipoPix,
        chavePix: data.chavePix || '',
        observacoes: data.observacoes || '',

        // Fisica
        fisicaCpf: data.pessoaFisica?.cpf?.valor || '',
        fisicaNome: data.pessoaFisica?.nome || '',
        fisicaSobrenome: data.pessoaFisica?.sobrenome || '',
        rgNumero: data.pessoaFisica?.rgNumero || '',
        rgOrgaoEmissor: data.pessoaFisica?.rgOrgaoEmissor || '',
        tipoGenero: data.pessoaFisica?.tipoGenero || 1,
        dataNascimento: data.pessoaFisica?.dataNascimento ? data.pessoaFisica.dataNascimento.substring(0, 10) : null,

        // Juridica
        juridicaCnpj: data.pessoaJuridica?.cnpj?.valor || '',
        razaoSocial: data.pessoaJuridica?.razaoSocial || '',
        nomeFantasia: data.pessoaJuridica?.nomeFantasia || '',
        inscricaoEstadual: data.pessoaJuridica?.inscricaoEstadual || '',
        inscricaoMunicipal: data.pessoaJuridica?.inscricaoMunicipal || '',
        cnae: data.pessoaJuridica?.cnae || '',

        // Estrangeiro
        estrangeiroNome: data.pessoaEstrangeiro?.nome || '',
        identificacaoEstrangeiro: data.pessoaEstrangeiro?.identificacaoEstrangeiro || '',

        // Roles flags
        ehCliente: data.ehCliente,
        ehFornecedor: data.ehFornecedor,
        ehTransportadora: data.ehTransportadora,
        ehMotorista: data.ehMotorista,
        ehPrestadorServico: data.ehPrestadorServico,
        ehFuncionario: data.ehFuncionario,
        ehProdutorRural: data.ehProdutorRural,

        // Role details
        clienteEhConsumidorFinal: data.pessoaCliente?.ehConsumidorFinal || false,
        clienteTipoContribuinte: data.pessoaCliente?.tipoContribuinte || 1,
        limiteCredito: data.limiteCredito,
        funcionarioTipoCargo: data.pessoaFuncionario?.tipoCargo || 1,
        funcionarioComissao: data.pessoaFuncionario?.comissao,
        motoristaTipoVinculo: data.pessoaMotorista?.tipoVinculo || 1,
        motoristaCategoriaCnh: data.pessoaMotorista?.categoriaCnh || 1,
        motoristaDataEmissaoCnh: data.pessoaMotorista?.dataEmissaoCnh ? data.pessoaMotorista.dataEmissaoCnh.substring(0, 10) : null,
        motoristaDataVencimentoCnh: data.pessoaMotorista?.dataVencimentoCnh ? data.pessoaMotorista.dataVencimentoCnh.substring(0, 10) : null,
        motoristaRntrc: data.pessoaMotorista?.rntrc || '',
        transportadoraCiot: data.pessoaTransportadora?.ciot || '',
        transportadoraRntrc: data.pessoaTransportadora?.rntrc || '',
        prestadorCei: data.pessoaPrestadorServico?.cei || '',

        enderecos: (data.enderecos || []).map(e => ({
          tipoEndereco: e.tipoEndereco,
          paisId: e.paisId,
          municipioId: e.municipioId,
          uf: e.uf,
          cep: e.cep,
          logradouro: e.logradouro,
          numero: e.numero,
          complemento: e.complemento,
          bairro: e.bairro,
          referencia: e.referencia,
          municipiosDisponiveis: []
        })),
        contatos: (data.contatos || []).map(c => ({
          nome: c.nome,
          tipoContatoTelefonico: c.tipoContatoTelefonico,
          numeroTelefone: c.numeroTelefone,
          tipoContatoEmail: c.tipoContatoEmail,
          email: c.email,
          ehPrincipal: c.ehPrincipal
        }))
      }

      // Carregar municípios para cada endereço de forma assíncrona
      for (let i = 0; i < p.enderecos.length; i++) {
        if (p.enderecos[i].uf) {
          const resMuns = await useApi(`/cadastros/geografia/municipios/obter-por-uf/${p.enderecos[i].uf}`)
          p.enderecos[i].municipiosDisponiveis = extrairDados(resMuns) || []
        }
      }

      form.value = p
      isEditing.value = true
      showForm.value = true
      activeFormTab.value = 'identificacao'
  } catch (e) {
    console.error('Falha ao conectar na API.', e)
    alert('Erro ao carregar registro do parceiro comercial.')
  } finally {
    loading.value = false
  }
}

const closeForm = () => {
  showForm.value = false
  form.value = defaultForm()
}

// Gestão de Endereços
const addNewEndereco = () => {
  const brasil = paises.value.find(p => p.codigoIsoAlpha2 === 'BR')
  form.value.enderecos.push({
    tipoEndereco: form.value.enderecos.length === 0 ? 1 : 2,
    paisId: brasil ? brasil.id : '',
    municipioId: '',
    uf: '',
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    referencia: '',
    municipiosDisponiveis: []
  })
}

const removeEndereco = (index) => {
  form.value.enderecos.splice(index, 1)
}

const onUfChange = async (index) => {
  const end = form.value.enderecos[index]
  end.municipioId = ''
  end.municipiosDisponiveis = []
  if (!end.uf) return
  
  try {
    const res = await useApi(`/cadastros/geografia/municipios/obter-por-uf/${end.uf}`)
    end.municipiosDisponiveis = extrairDados(res) || []
  } catch (e) {
    console.error('Erro ao buscar municípios para UF.', e)
  }
}

const searchCepOnBlur = async (index) => {
  const end = form.value.enderecos[index]
  if (!end.cep) return
  const cleanCep = end.cep.replace(/\D/g, '')
  if (cleanCep.length !== 8) return

  try {
    const resposta = await useApi(`/cadastros/geografia/cep/${cleanCep}`)
    const data = extrairDados(resposta)
    if (data && !data.falhou) {
      end.logradouro = data.logradouro || ''
      end.bairro = data.bairro || ''
      end.uf = data.uf || ''

      // Buscar municípios da UF retornada
      await onUfChange(index)

      // Tentar localizar e associar o MunicipioId correspondente ao código IBGE
      if (data.ibge && end.municipiosDisponiveis.length > 0) {
        const codIbgeNum = parseInt(data.ibge, 10)
        const matchedMun = end.municipiosDisponiveis.find(m => m.codigoIbge === codIbgeNum)
        if (matchedMun) {
          end.municipioId = matchedMun.id
        }
      }
    }
  } catch (e) {
    console.error('Falha na busca de CEP.', e)
  }
}

// Gestão de Contatos
const addNewContato = () => {
  form.value.contatos.push({
    nome: '',
    tipoContatoTelefonico: 1,
    numeroTelefone: '',
    tipoContatoEmail: 1,
    email: '',
    ehPrincipal: form.value.contatos.length === 0
  })
}

const removeContato = (index) => {
  form.value.contatos.splice(index, 1)
}

const onContatoPrincipalChange = (selectedIndex) => {
  form.value.contatos.forEach((c, idx) => {
    if (idx !== selectedIndex) {
      c.ehPrincipal = false
    }
  })
}

// Máscaras
const applyCpfMask = () => {
  let v = form.value.fisicaCpf.replace(/\D/g, '')
  if (v.length > 11) v = v.substring(0, 11)
  if (v.length > 9) {
    v = v.replace(/^(\d{3})(\d{3})(\d{3})/, '$1.$2.$3-')
  } else if (v.length > 6) {
    v = v.replace(/^(\d{3})(\d{3})/, '$1.$2.')
  } else if (v.length > 3) {
    v = v.replace(/^(\d{3})/, '$1.')
  }
  form.value.fisicaCpf = v
}

const applyCnpjMask = () => {
  let v = form.value.juridicaCnpj.replace(/\D/g, '')
  if (v.length > 14) v = v.substring(0, 14)
  if (v.length > 12) {
    v = v.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})/, '$1.$2.$3/$4-')
  } else if (v.length > 8) {
    v = v.replace(/^(\d{2})(\d{3})(\d{3})/, '$1.$2.$3/')
  } else if (v.length > 5) {
    v = v.replace(/^(\d{2})(\d{3})/, '$1.$2.')
  } else if (v.length > 2) {
    v = v.replace(/^(\d{2})/, '$1.')
  }
  form.value.juridicaCnpj = v
}

// Envio do Form
const saveForm = async () => {
  saving.value = true
  try {
    // Normalizar CPFs e CNPJs limpando caracteres não-numéricos
    const payload = {
      tipoPessoa: form.value.tipoPessoa,
      tipoIndicadorIe: form.value.tipoIndicadorIe,
      pessoaGrupoId: form.value.pessoaGrupoId,
      inscricaoSuframa: form.value.inscricaoSuframa,
      titularContaBancaria: form.value.titularContaBancaria,
      agenciaContaBancaria: form.value.agenciaContaBancaria,
      numeroContaBancaria: form.value.numeroContaBancaria,
      tipoPix: form.value.tipoPix,
      chavePix: form.value.chavePix,
      observacoes: form.value.observacoes,

      // Fisica
      fisicaCpf: form.value.fisicaCpf ? form.value.fisicaCpf.replace(/\D/g, '') : null,
      fisicaNome: form.value.fisicaNome,
      fisicaSobrenome: form.value.fisicaSobrenome,
      rgNumero: form.value.rgNumero,
      rgOrgaoEmissor: form.value.rgOrgaoEmissor,
      tipoGenero: form.value.tipoGenero,
      dataNascimento: form.value.dataNascimento || null,

      // Juridica
      juridicaCnpj: form.value.juridicaCnpj ? form.value.juridicaCnpj.replace(/\D/g, '') : null,
      razaoSocial: form.value.razaoSocial,
      nomeFantasia: form.value.nomeFantasia,
      inscricaoEstadual: form.value.inscricaoEstadual,
      inscricaoMunicipal: form.value.inscricaoMunicipal,
      cnae: form.value.cnae,

      // Estrangeiro
      estrangeiroNome: form.value.estrangeiroNome,
      identificacaoEstrangeiro: form.value.identificacaoEstrangeiro,

      // Roles
      ehCliente: form.value.ehCliente,
      ehFornecedor: form.value.ehFornecedor,
      ehTransportadora: form.value.ehTransportadora,
      ehMotorista: form.value.ehMotorista,
      ehPrestadorServico: form.value.ehPrestadorServico,
      ehFuncionario: form.value.ehFuncionario,
      ehProdutorRural: form.value.ehProdutorRural,

      // Role Details
      clienteEhConsumidorFinal: form.value.clienteEhConsumidorFinal,
      clienteTipoContribuinte: form.value.clienteTipoContribuinte,
      limiteCredito: form.value.limiteCredito,
      funcionarioTipoCargo: form.value.funcionarioTipoCargo,
      funcionarioComissao: form.value.funcionarioComissao,
      motoristaTipoVinculo: form.value.motoristaTipoVinculo,
      motoristaCategoriaCnh: form.value.motoristaCategoriaCnh,
      motoristaDataEmissaoCnh: form.value.motoristaDataEmissaoCnh || null,
      motoristaDataVencimentoCnh: form.value.motoristaDataVencimentoCnh || null,
      motoristaRntrc: form.value.motoristaRntrc,
      transportadoraCiot: form.value.transportadoraCiot,
      transportadoraRntrc: form.value.transportadoraRntrc,
      prestadorCei: form.value.prestadorCei,

      // Collections
      enderecos: form.value.enderecos.map(e => ({
        tipoEndereco: e.tipoEndereco,
        paisId: e.paisId,
        municipioId: e.municipioId,
        uf: e.uf,
        cep: e.cep ? e.cep.replace(/\D/g, '') : null,
        logradouro: e.logradouro,
        numero: e.numero,
        complemento: e.complemento,
        bairro: e.bairro,
        referencia: e.referencia
      })),
      contatos: form.value.contatos.map(c => ({
        nome: c.nome,
        tipoContatoTelefonico: c.tipoContatoTelefonico,
        numeroTelefone: c.numeroTelefone ? c.numeroTelefone.replace(/\D/g, '') : null,
        tipoContatoEmail: c.tipoContatoEmail,
        email: c.email,
        ehPrincipal: c.ehPrincipal
      })),
      veiculos: []
    }

    let rota = '/cadastros/pessoas'
    let method = 'POST'

    if (isEditing.value) {
      rota = `/cadastros/pessoas/${form.value.id}`
      method = 'PUT'
      payload.id = form.value.id
    }

    // useApi injeta token/tenant, serializa o corpo em JSON e devolve o envelope CommandResult.
    const resultData = await useApi(rota, { method, body: payload })
    if (resultData && resultData.sucesso) {
      alert(isEditing.value ? 'Parceiro comercial atualizado com sucesso!' : 'Parceiro comercial cadastrado com sucesso!')
      closeForm()
      await loadPessoas()
    } else {
      const errMsg = resultData?.erros ? resultData.erros.join('\n') : resultData?.mensagem
      alert(`Falha ao gravar parceiro:\n${errMsg}`)
    }
  } catch (e) {
    console.error(e)
    const errMsg = e.data?.erros ? e.data.erros.join('\n') : (e.data?.mensagem || e.message)
    alert(`Erro ao enviar formulário para o servidor:\n${errMsg}`)
  } finally {
    saving.value = false
  }
}

// Excluir Pessoa
const deletePessoa = async (id, name) => {
  if (!confirm(`Deseja realmente excluir a pessoa "${name}"?`)) return
  
  try {
    await useApi(`/cadastros/pessoas/${id}`, { method: 'DELETE' })
    alert('Registro excluído com sucesso!')
    await loadPessoas()
  } catch (e) {
    console.error(e)
    alert('Falha ao excluir registro. Pode haver vínculos operacionais ou erro de conexão.')
  }
}
</script>

<style scoped>
.pessoas-module {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.filters-bar {
  padding: 16px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
}

.filters-left {
  display: flex;
  gap: 12px;
  flex-grow: 1;
  max-width: 800px;
}

.search-input {
  flex-grow: 1;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border-color);
  padding: 10px 14px;
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 14px;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: var(--primary);
}

.select-input {
  background: rgba(15, 15, 20, 0.9);
  border: 1px solid var(--border-color);
  color: var(--text-primary);
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 14px;
  min-width: 160px;
}

.roles-badges {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.role-badge {
  font-size: 10px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.client-badge { background: rgba(99, 102, 241, 0.15); color: #818cf8; border: 1px solid rgba(99, 102, 241, 0.3); }
.provider-badge { background: rgba(236, 72, 153, 0.15); color: #f472b6; border: 1px solid rgba(236, 72, 153, 0.3); }
.carrier-badge { background: rgba(16, 185, 129, 0.15); color: #34d399; border: 1px solid rgba(16, 185, 129, 0.3); }
.driver-badge { background: rgba(245, 158, 11, 0.15); color: #fbbf24; border: 1px solid rgba(245, 158, 11, 0.3); }
.employee-badge { background: rgba(139, 92, 246, 0.15); color: #a78bfa; border: 1px solid rgba(139, 92, 246, 0.3); }
.farmer-badge { background: rgba(14, 165, 233, 0.15); color: #38bdf8; border: 1px solid rgba(14, 165, 233, 0.3); }

.type-badge {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: 4px;
}
.pf-type { background: rgba(255, 255, 255, 0.05); color: var(--text-primary); }
.pj-type { background: rgba(59, 130, 246, 0.15); color: #60a5fa; border: 1px solid rgba(59, 130, 246, 0.3); }
.pe-type { background: rgba(239, 68, 68, 0.15); color: #f87171; border: 1px solid rgba(239, 68, 68, 0.3); }

.delete-btn {
  color: var(--danger);
  border-color: rgba(239, 68, 68, 0.2);
}

.delete-btn:hover {
  background: rgba(239, 68, 68, 0.1) !important;
  border-color: var(--danger) !important;
}

/* Form Styles */
.form-wrapper {
  display: flex;
  flex-direction: column;
}

.form-tabs-nav {
  display: flex;
  border-bottom: 1px solid var(--border-color);
  background: rgba(255,255,255,0.01);
  border-top-left-radius: 16px;
  border-top-right-radius: 16px;
}

.form-tab-btn {
  padding: 16px 24px;
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  border-bottom: 2px solid transparent;
  transition: all 0.2s ease;
}

.form-tab-btn:hover {
  color: var(--text-primary);
  background: rgba(255,255,255,0.02);
}

.form-tab-btn.active {
  color: var(--primary);
  border-bottom-color: var(--primary);
  background: rgba(99, 102, 241, 0.03);
}

.sub-form-section {
  background: rgba(255, 255, 255, 0.01);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  padding: 20px;
  margin-top: 14px;
}

.roles-checkboxes-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
  margin-top: 16px;
}

.checkbox-card {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 16px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.checkbox-card input[type="checkbox"] {
  width: 20px;
  height: 20px;
  margin-top: 2px;
  cursor: pointer;
}

.checkbox-card-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.checkbox-card-info .title {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-primary);
}

.checkbox-card-info .desc {
  font-size: 11px;
  color: var(--text-muted);
}

.checkbox-card.checked {
  border-color: var(--primary);
  background: rgba(99, 102, 241, 0.05);
}

.address-item-card {
  padding: 20px;
  border-radius: 10px;
  border: 1px solid var(--border-color);
  background: rgba(255, 255, 255, 0.02);
  margin-bottom: 16px;
}

.address-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.03);
  padding-bottom: 10px;
  margin-bottom: 16px;
}

.page-indicator {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary);
  display: flex;
  align-items: center;
  padding: 0 10px;
}
</style>
