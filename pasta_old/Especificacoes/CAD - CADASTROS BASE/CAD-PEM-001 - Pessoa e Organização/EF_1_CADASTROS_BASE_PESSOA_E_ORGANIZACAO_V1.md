# EF 1 Cadastros Base — Pessoa e Organizacao V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Cadastros Base |
| Submodulo | Pessoa e Organizacao |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo Pessoa e Organizacao mantem o cadastro mestre de pessoas, parceiros comerciais e empresas emitentes do Epros. Ele permite que uma mesma pessoa acumule varios papeis operacionais sem duplicacao cadastral: cliente, fornecedor, transportadora, motorista, prestador de servico, funcionario, produtor rural, vendedor, comprador e contador.

O submodulo tambem governa enderecos, contatos, documentos, dados bancarios, chave PIX, grupos comerciais, limite de credito, condicoes comerciais, dados fiscais basicos, certificados digitais, empresas emitentes, matriz/filiais, historico de estado, bloqueios, importacao, deduplicacao, privacidade, auditoria e eventos para os modulos consumidores.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Pessoa unica multipapel | Manter cadastro unico com subtipos fisica, juridica e estrangeira. |
| Papeis operacionais | Ativar extensoes por papel, sem criar cadastros paralelos. |
| Cliente | Manter consumidor final, tipo contribuinte, limite de credito, grupo, vendedor, saldos e regras comerciais. |
| Fornecedor | Manter comprador responsavel, prazo de pagamento, prazo medio de entrega, retencao, faturamento e dados de compra. |
| Motorista | Manter vinculo, categoria CNH, emissao, vencimento, RNTRC e veiculos. |
| Transportadora | Manter CIOT, RNTRC, ANTT, veiculo, placa e relacao com motoristas. |
| Prestador de servico | Manter CEI e vinculos de servico. |
| Funcionario | Manter cargo, comissao, CLT, admissao e salario basico quando identificado no cadastro mestre. |
| Vendedor e comprador | Manter dados de vendedor/comprador para vendas, compras, metas e regras de desconto. |
| Contador | Manter CRC e vinculo fiscal/contabil. |
| Enderecos | Manter enderecos fiscal, principal, entrega e obra, normalizados por geografia quando aplicavel. |
| Contatos | Manter telefones, e-mails, contato principal, tipo e detalhes. |
| Dados bancarios | Manter titular, agencia, conta, chave PIX e dados financeiros cadastrais. |
| Empresa emitente | Manter empresas, matriz/filiais, grupos, endereco, contato, certificados e parametros fiscais basicos. |
| Certificado digital | Cadastrar, validar, guardar metadados e controlar validade de certificado A1. |
| Grupos | Manter grupo de pessoa, grupo de cliente, grupo de fornecedor e grupo de empresa. |
| Importacao | Importar pessoas em massa com validacao linha a linha e transacao unica por lote. |
| Deduplicacao | Identificar candidatos a duplicidade, impedir duplicidade critica e controlar merge. |
| Privacidade | Registrar base legal, consentimento, solicitacoes do titular e exportacao/anonimizacao conforme retencao legal. |
| Bloqueio/KYC | Bloquear parceiros por compliance, certidao, sancao, litigio ou risco. |
| Eventos | Publicar eventos de criacao, atualizacao, inativacao, bloqueio, merge e anonimização apos commit. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Calculo de folha, ponto e beneficios | Pertence a RH. |
| Emissao e transmissao fiscal | Pertence a faturamento fiscal eletronico. |
| Regras tributarias completas | Pertencem ao modulo fiscal/tributario. |
| Movimento financeiro e conciliacao | Pertencem ao financeiro e tesouraria. |
| Produto, estoque e servico operacional | Pertencem a estoque, vendas, compras e servicos. |
| Movimento de caixa/PDV | Pertence a vendas/PDV e financeiro. |

## 4. Dependencias e consumidores

### 4.1 Dependencias

| Dependencia | Uso |
|---|---|
| Geografia e Localizacao | Pais, subdivisao, municipio, CEP/codigo postal e validacao territorial. |
| Parametros Operacionais | Tenant, moeda padrao, fuso, parametros fiscais e grupos auxiliares. |
| Assinatura e Limites | Limite de clientes, fornecedores, usuarios e capacidades contratadas. |
| Permissoes de Menu | Controle de acesso por rota, tela, papel e acao. |
| Workflow | Aprovacao de alteracoes sensiveis, bloqueio e merge. |
| Compliance e Privacidade | Base legal, consentimento, solicitacoes de titular e retencao. |
| Gestao Eletronica de Documentos | Documentos, certidoes, contratos, anexos e evidencias por pessoa. |

### 4.2 Consumidores

| Consumidor | Dados consumidos |
|---|---|
| Vendas, CRM e PDV | Cliente, grupo, limite, contato, endereco, consumidor final, contribuinte e autocomplete. |
| Compras | Fornecedor, comprador, prazo, retencao, endereco, contato e bloqueios. |
| Financeiro | Pessoa em titulos, conta bancaria, saldos, limite, historico e dados de cobranca. |
| Estoque e Logistica | Transportadora, motorista, veiculo, RNTRC, endereco e fornecedor. |
| RH | Funcionario, cargo, admissao, salario basico e vinculo com usuario. |
| Fiscal | Emitente, destinatario, transportadora, certificados, IE, IM, CNAE, SUFRAMA e parametros de documento fiscal. |
| Relatorios | Status, saldos, grupos, historico, auditoria, mapas e qualidade cadastral. |
| Plataforma | Eventos, auditoria, permissao, tenant, importacao e privacidade. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-PEM-001 | Toda pessoa pertence a um tenant e nenhuma consulta pode retornar dados de outro tenant. |
| REG-PEM-002 | O tenant e resolvido pelo contexto autenticado e nao pode ser aceito por parametro externo editavel. |
| REG-PEM-003 | A pessoa e o agregado raiz; subtipos e papeis sao extensoes do mesmo cadastro. |
| REG-PEM-004 | Uma pessoa pode acumular papeis, exceto combinacoes expressamente proibidas. |
| REG-PEM-005 | CPF e CNPJ devem ser validados por digito verificador. |
| REG-PEM-006 | Documento nacional deve ser unico por tenant, salvo pessoa padrao de consumidor final quando configurada. |
| REG-PEM-007 | Pessoa estrangeira deve possuir identificacao estrangeira ou identificador fiscal internacional quando aplicavel. |
| REG-PEM-008 | A exclusao padrao e logica e deve respeitar vinculos transacionais. |
| REG-PEM-009 | Alteracao sensivel deve ser auditada e pode exigir aprovacao. |
| REG-PEM-010 | Dados sensiveis e segredos devem ser protegidos por cofre, criptografia ou mascaramento conforme natureza do dado. |
| REG-PEM-011 | Campos sem informacao de tamanho no material ficam marcados como Nao informado no material no dicionario. |
| REG-PEM-012 | Bugs identificados no material sao saneados: a EF registra a regra correta e a MC registra a decisao em aberto quando houver. |

## 6. Regras funcionais detalhadas

### 6.1 Pessoa e subtipos

| Codigo | Regra |
|---|---|
| REG-PEM-013 | Toda pessoa deve possuir `Id`, `TenantId`, `TipoPessoa`, `TipoIndicadorIe`, status, pelo menos um papel e auditoria de criacao. |
| REG-PEM-014 | `TipoPessoa` aceita PessoaFisica, PessoaJuridica ou PessoaEstrangeira. |
| REG-PEM-015 | Para PessoaFisica, deve existir exatamente uma extensao de pessoa fisica. |
| REG-PEM-016 | Para PessoaJuridica, deve existir exatamente uma extensao de pessoa juridica. |
| REG-PEM-017 | Para PessoaEstrangeira, deve existir exatamente uma extensao de pessoa estrangeira. |
| REG-PEM-018 | PessoaFisica exige CPF valido e nome. |
| REG-PEM-019 | PessoaJuridica exige CNPJ valido e razao social. |
| REG-PEM-020 | PessoaEstrangeira exige nome e identificacao estrangeira quando o pais nao utilizar documento nacional. |
| REG-PEM-021 | `PessoaGrupoId`, quando informado, deve referenciar grupo existente no tenant. |
| REG-PEM-022 | `InscricaoSuframa` tem limite numerico de 999999999 quando usada como numero. |
| REG-PEM-023 | `TipoIndicadorIe` aceita ContribuinteICMS, Isento ou NaoContribuinte. |
| REG-PEM-024 | Inscricao estadual e obrigatoria quando a pessoa for contribuinte e a jurisdicao exigir. |
| REG-PEM-025 | A pessoa deve manter `EhInativo` e status operacional para filtros de Ativo, Inativo, Bloqueado e Todos. |
| REG-PEM-026 | O campo observacoes deve aceitar texto livre limitado e nao pode guardar credenciais. |
| REG-PEM-027 | Nascimento/fundacao deve ser armazenado quando informado e usado para pessoa fisica, pessoa juridica e cobranca. |

### 6.2 Papeis

| Codigo | Regra |
|---|---|
| REG-PEM-028 | Pelo menos um papel deve estar marcado para gravar uma pessoa. |
| REG-PEM-029 | Cada papel marcado exige seu objeto de papel correspondente. |
| REG-PEM-030 | Cliente e fornecedor podem coexistir na mesma pessoa. |
| REG-PEM-031 | Motorista e transportadora sao mutuamente exclusivos para a mesma pessoa. |
| REG-PEM-032 | Motorista nao pode ser pessoa juridica. |
| REG-PEM-033 | O papel cliente deve manter consumidor final e tipo contribuinte. |
| REG-PEM-034 | O papel cliente pode manter limite de credito, grupo de cliente, vendedor responsavel, desconto e indicador de preco. |
| REG-PEM-035 | O papel cliente pode manter saldos agregados para exibicao, mas saldos contabeis sao calculados pelo financeiro. |
| REG-PEM-036 | O papel fornecedor pode manter comprador responsavel, prazo medio de entrega, prazo de pagamento, retencao, faturamento e conta remetente. |
| REG-PEM-037 | Fornecedor com comprador obrigatorio deve exigir comprador valido. |
| REG-PEM-038 | Cliente com vendedor obrigatorio deve exigir vendedor valido. |
| REG-PEM-039 | O papel vendedor deve manter senha de aplicativo com tamanho maximo 6 quando usado, e-mail, meta, gestor e regras de desconto. |
| REG-PEM-040 | O papel funcionario deve manter cargo, percentual de comissao, CLT, data de admissao e salario quando informado. |
| REG-PEM-041 | Percentual de comissao nao pode exceder 100. |
| REG-PEM-042 | O papel motorista deve manter categoria CNH, data de emissao, data de vencimento, RNTRC e vinculo Proprio/Terceiros. |
| REG-PEM-043 | Motorista com CNH obrigatoria deve informar CNH antes de ativar o papel. |
| REG-PEM-044 | Motorista vinculado a transportadora deve apontar transportadora ativa. |
| REG-PEM-045 | O papel transportadora deve manter CIOT, RNTRC, ANTT, placa e veiculo quando informado. |
| REG-PEM-046 | O papel prestador de servico deve manter CEI quando aplicavel. |
| REG-PEM-047 | O papel contador deve manter CRC quando aplicavel. |
| REG-PEM-048 | Papeis removidos devem ficar historizados quando houver movimento, e nao apagados fisicamente. |

### 6.3 Enderecos

| Codigo | Regra |
|---|---|
| REG-PEM-049 | Pessoa pode ter multiplos enderecos. |
| REG-PEM-050 | Deve haver no maximo um endereco Principal por pessoa. |
| REG-PEM-051 | Quando politica do tenant exigir endereco principal, deve haver exatamente um endereco Principal. |
| REG-PEM-052 | Endereco nacional deve referenciar pais, municipio e UF validos. |
| REG-PEM-053 | Municipio deve pertencer a UF informada. |
| REG-PEM-054 | CEP deve ser valido e obrigatorio para pessoa nacional, salvo configuracao especifica. |
| REG-PEM-055 | CEP pode ser nulo para pessoa estrangeira. |
| REG-PEM-056 | Endereco internacional deve permitir subdivisao, codigo postal internacional e linhas livres de endereco. |
| REG-PEM-057 | Endereco de entrega pode herdar endereco de cobranca/fiscal quando `MesmoEnderecoCobranca` estiver ativo. |
| REG-PEM-058 | Endereco pode ter tipo Principal, Entrega, Obra, Cobranca ou Fiscal conforme uso. |
| REG-PEM-059 | Endereco pode manter referencia/ponto de referencia e coordenadas quando usado em mapa. |
| REG-PEM-060 | Enderecos excluidos em edicao devem ser tratados como remocao logica quando houver historico. |

### 6.4 Contatos

| Codigo | Regra |
|---|---|
| REG-PEM-061 | Pessoa pode ter multiplos contatos. |
| REG-PEM-062 | Deve haver exatamente um contato principal quando a pessoa tiver contatos obrigatorios. |
| REG-PEM-063 | Nao pode haver mais de um contato principal para o mesmo tipo funcional. |
| REG-PEM-064 | Contato principal deve ser resolvido por `EhPrincipal` e tipo, nao pela primeira linha encontrada. |
| REG-PEM-065 | Telefone exige tipo e numero quando usado. |
| REG-PEM-066 | E-mail exige tipo e endereco valido quando usado. |
| REG-PEM-067 | E-mail principal deve ser identificado de forma estruturada. |
| REG-PEM-068 | Contatos podem possuir vigencia, funcao/cargo e detalhe. |
| REG-PEM-069 | Autocomplete de cliente deve retornar somente clientes ativos e permitidos ao usuario. |
| REG-PEM-070 | Quando houver restricao de contatos por usuario, o autocomplete deve filtrar apenas pessoas autorizadas. |

### 6.5 Dados bancarios, PIX e comerciais

| Codigo | Regra |
|---|---|
| REG-PEM-071 | Titular da conta bancaria deve ter tamanho maximo 150. |
| REG-PEM-072 | Agencia deve ter tamanho maximo 20. |
| REG-PEM-073 | Numero de conta deve ter tamanho maximo 20. |
| REG-PEM-074 | Chave PIX deve ter tamanho maximo 32 quando informada no cadastro base. |
| REG-PEM-075 | Tipo PIX aceita ChaveAleatoria, CpfCnpj, Email ou Telefone. |
| REG-PEM-076 | Limite de credito deve aceitar valor decimal e pode ser nulo para sem limite definido. |
| REG-PEM-077 | Desconto comercial deve ter forma e percentual/valor conforme configuracao. |
| REG-PEM-078 | Prazo de pagamento de fornecedor pode ser definido por numero e tipo de periodo: dias ou meses. |
| REG-PEM-079 | Saldo inicial de parceiro, quando informado, deve gerar evento financeiro ou lancamento controlado pelo financeiro. |
| REG-PEM-080 | Saldo inicial nao deve ser alterado sem recalcular pagamentos ja vinculados. |
| REG-PEM-081 | Dados bancarios e documentos devem ser mascarados nas telas quando o perfil nao possuir permissao de visualizacao sensivel. |

### 6.6 Grupos e classificacoes

| Codigo | Regra |
|---|---|
| REG-PEM-082 | Grupo de pessoa deve possuir descricao unica por tenant. |
| REG-PEM-083 | Grupo de cliente pode possuir percentual de desconto. |
| REG-PEM-084 | Grupo de fornecedor pode classificar compras e analises. |
| REG-PEM-085 | Grupo de empresa agrupa matriz e filiais. |
| REG-PEM-086 | Grupo de empresa nao pode ser removido se possuir empresas vinculadas. |
| REG-PEM-087 | Grupos usados em transacoes devem ser inativados em vez de excluidos. |

### 6.7 Empresa emitente

| Codigo | Regra |
|---|---|
| REG-PEM-088 | Empresa emitente pertence ao tenant e pode estar vinculada a grupo de empresa. |
| REG-PEM-089 | Empresa deve manter razao social, nome fantasia, CNPJ, regime tributario, regime de apuracao e endereco. |
| REG-PEM-090 | Razao social, nome fantasia, CNPJ e bairro sao obrigatorios quando a empresa for emitente ativa. |
| REG-PEM-091 | Empresa pode manter IE, IM, SUFRAMA, CNAE, CSC, IDCSC e parametros fiscais. |
| REG-PEM-092 | Empresa pode manter logotipo, imagem de impressao, imagens de publicidade de PDV e links de publicidade. |
| REG-PEM-093 | Upload de imagem deve validar conteudo, MIME e extensao antes de gravar. |
| REG-PEM-094 | Se nenhuma imagem nova for enviada em edicao, a imagem existente deve ser preservada. |
| REG-PEM-095 | Empresa pode manter SMTP operacional, mas senha e tokens devem ser armazenados em cofre. |
| REG-PEM-096 | Alteracao de senha SMTP vazia em edicao deve preservar senha anterior quando o usuario nao solicitar troca. |
| REG-PEM-097 | Teste de e-mail deve validar host, porta, SSL, credenciais e timeout. |
| REG-PEM-098 | Empresa pode manter link de aplicativo de vendas e token PIX, protegidos como segredo. |
| REG-PEM-099 | Empresa pode manter parametros SAT/MFe, modelo de impressao e parametros de documento fiscal. |
| REG-PEM-100 | Empresa nao pode ser consultada, listada ou detalhada sem autenticacao e filtro tenant. |
| REG-PEM-101 | Empresa nao deve ser excluida se houver movimentos fiscais, financeiros, estoque ou vendas vinculados. |

### 6.8 Certificado digital

| Codigo | Regra |
|---|---|
| REG-PEM-102 | Certificado digital deve pertencer a empresa e ao tenant. |
| REG-PEM-103 | Upload de certificado exige arquivo e senha. |
| REG-PEM-104 | Certificado deve ser validado antes de gravar. |
| REG-PEM-105 | Certificado deve registrar serial, titular, informacao, CNPJ, validade inicial e validade final. |
| REG-PEM-106 | CNPJ do certificado deve ser conferido contra a empresa ou aceito por regra formal de matriz/filial. |
| REG-PEM-107 | Certificado vencido ou a vencer deve gerar alerta operacional. |
| REG-PEM-108 | Excluir certificado deve respeitar vinculo com parametros fiscais e historico de uso. |

### 6.9 Veiculos

| Codigo | Regra |
|---|---|
| REG-PEM-109 | Veiculo deve pertencer a pessoa motorista ou transportadora. |
| REG-PEM-110 | Pais do veiculo deve ser informado e valido. |
| REG-PEM-111 | Tipo de veiculo aceita Veiculo ou Reboque. |
| REG-PEM-112 | UF do veiculo deve ser valida quando o pais exigir UF. |
| REG-PEM-113 | Placa deve aceitar padrao Mercosul ou padrao nacional antigo. |
| REG-PEM-114 | Placa deve ter tamanho maximo 8. |
| REG-PEM-115 | RNTRC do veiculo deve ter tamanho maximo 14. |

### 6.10 Status, estados e bloqueios

| Codigo | Regra |
|---|---|
| REG-PEM-116 | Pessoa pode iniciar em Rascunho. |
| REG-PEM-117 | Pessoa pode ir de Rascunho para EmValidacao quando politica exigir aprovacao. |
| REG-PEM-118 | Pessoa pode ir de Rascunho diretamente para Ativo quando o perfil e a politica permitirem. |
| REG-PEM-119 | Pessoa em EmValidacao pode ser aprovada para Ativo ou rejeitada para Rascunho. |
| REG-PEM-120 | Pessoa Ativa pode ser Inativada por gestor. |
| REG-PEM-121 | Pessoa Ativa pode ser Bloqueada por compliance. |
| REG-PEM-122 | Pessoa Inativa pode ser Reativada por gestor. |
| REG-PEM-123 | Bloqueio de parceiro e prospectivo e nao altera movimentos existentes. |
| REG-PEM-124 | Transacao com parceiro bloqueado deve bloquear ou alertar conforme politica do tenant e tipo de bloqueio. |
| REG-PEM-125 | Bloqueio deve manter motivo, data, usuario e eventual data de desbloqueio. |

### 6.11 Exclusao e integridade

| Codigo | Regra |
|---|---|
| REG-PEM-126 | Exclusao de pessoa deve verificar compras, vendas, titulos, estoque, fiscal, RH, projetos e demais vinculos. |
| REG-PEM-127 | Exclusao de papel deve verificar movimentos daquele proprio papel. |
| REG-PEM-128 | Cliente padrao de PDV nao pode ser excluido. |
| REG-PEM-129 | Pessoa com transacao deve ser inativada em vez de excluida fisicamente. |
| REG-PEM-130 | Remocao de contatos, enderecos e papeis deve preservar historico quando houver uso. |
| REG-PEM-131 | A remocao nao pode consultar papel diferente daquele que esta sendo removido. |
| REG-PEM-132 | Grupos, certificados e empresas em uso devem ser bloqueados para exclusao. |

### 6.12 Importacao e API externa

| Codigo | Regra |
|---|---|
| REG-PEM-133 | Importacao de pessoas deve exigir permissao de criacao de pessoa e assinatura/capacidade ativa quando aplicavel. |
| REG-PEM-134 | Importacao deve usar layout publicado e validado contra o numero esperado de colunas. |
| REG-PEM-135 | Importacao deve validar tipo, documento, e-mail, cidade/municipio, consumidor final, contribuinte, endereco, CEP e campos comerciais. |
| REG-PEM-136 | Importacao deve ser transacional por lote ou por bloco controlado, com rollback quando a politica for tudo-ou-nada. |
| REG-PEM-137 | Importacao deve apresentar relatorio de linhas aceitas, rejeitadas e motivos. |
| REG-PEM-138 | API externa de criacao/consulta de cliente deve exigir autenticacao forte e escopo tenant. |
| REG-PEM-139 | API externa deve buscar cliente existente por identificador confiavel antes de criar novo cadastro. |
| REG-PEM-140 | Codigo de referencia de cliente/fornecedor deve ser unico por tenant quando informado. |
| REG-PEM-141 | Codigo de referencia pode ser gerado automaticamente com sequencia por tenant. |

### 6.13 Deduplicacao, merge e qualidade

| Codigo | Regra |
|---|---|
| REG-PEM-142 | O Epros deve calcular candidatos a duplicidade por documento, nome, data, e-mail, telefone e endereco. |
| REG-PEM-143 | Regra de deduplicacao deve permitir estrategia Exata, Fuzzy ou Fonetica. |
| REG-PEM-144 | Score acima do limiar de bloqueio impede gravacao ate resolucao. |
| REG-PEM-145 | Score acima do limiar de alerta permite continuidade com justificativa. |
| REG-PEM-146 | Merge deve consolidar historico na pessoa sobrevivente e manter de-para para auditoria. |
| REG-PEM-147 | Merge deve ser auditavel e reversivel por periodo de carencia definido. |
| REG-PEM-148 | Pessoa deve ter score de qualidade calculado por completude, validade e consistencia. |
| REG-PEM-149 | Enriquecimento por base oficial deve registrar fonte, data, status e dados atualizados. |

### 6.14 Privacidade e dados pessoais

| Codigo | Regra |
|---|---|
| REG-PEM-150 | Dados pessoais devem ter base legal registrada por finalidade quando aplicavel. |
| REG-PEM-151 | Consentimento deve registrar finalidade, base legal, data, canal e eventual revogacao. |
| REG-PEM-152 | Solicitacao do titular deve controlar tipo, status, prazo legal e conclusao. |
| REG-PEM-153 | Exportacao de dados pessoais deve agregar todos os dados associados a PessoaId. |
| REG-PEM-154 | Anonimizacao deve respeitar retencao legal fiscal, trabalhista e financeira. |
| REG-PEM-155 | Acesso a dados sensiveis deve gerar log de acesso. |
| REG-PEM-156 | Campos de senha, token, certificado e segredo nao podem ser exibidos em claro. |

### 6.15 Auditoria e eventos

| Codigo | Regra |
|---|---|
| REG-PEM-157 | Pessoa e empresa devem registrar usuario/data de criacao e usuario/data de alteracao. |
| REG-PEM-158 | Transicao de estado deve registrar estado anterior, estado novo, usuario, data/hora, IP e motivo. |
| REG-PEM-159 | Alteracao de documento, status, bloqueio, limite, certificado, dados bancarios e contato principal deve ser auditada. |
| REG-PEM-160 | Eventos de dominio devem ser publicados somente apos commit transacional. |
| REG-PEM-161 | Eventos minimos: pessoa.criada, pessoa.atualizada, pessoa.inativada, pessoa.bloqueada, pessoa.mesclada, pessoa.anonimizada, empresa.atualizada e certificado.vencendo. |

## 7. Enumerações de dominio

| Enumeracao | Valores |
|---|---|
| ETipoPessoa | PessoaFisica; PessoaJuridica; PessoaEstrangeira |
| EStatusPessoa | Rascunho; EmValidacao; Ativo; Inativo; Bloqueado; Todos |
| ETipoIndicadorIe | ContribuinteICMS; Isento; NaoContribuinte |
| ETipoContribuinte | NaoInformado; SimplesNacional; RPA; MEI |
| ETipoGenero | NaoUtiliza; Feminino; Masculino; Outros |
| ETipoPix | ChaveAleatoria; CpfCnpj; Email; Telefone |
| ETipoEndereco | Principal; Entrega; Obra; Cobranca; Fiscal |
| ETipoContatoTelefonico | NaoUtiliza; Residencial; Comercial; Recado; Emergencia; Outros |
| ETipoContatoEmail | NaoUtiliza; EnvioNFe; EnvioNfse; Contador; Financeiro; Comercial |
| ETipoCargo | Operador; Vendedor; Supervisor; Gerente |
| ETipoCategoriaCnh | NaoUtiliza; A; B; C; D; E; AB; AC; AD; AE |
| ETipoVeiculo | Veiculo; Reboque |
| ETipoVinculoMotorista | Proprio; Terceiros |
| ETipoRelacaoParceiro | MatrizFilial; GrupoEconomico; Representante; Controladora; ContatoDaConta |
| EStatusDuplicata | Pendente; Confirmada; Descartada; Mesclada |
| EEstrategiaMatch | Exata; Fuzzy; Fonetica |
| EFinalidadeDados | Marketing; Cobranca; Contrato; ObrigacaoLegal; Suporte; Fiscal |
| EBaseLegalPrivacidade | Consentimento; Contrato; ObrigacaoLegal; LegítimoInteresse; ExercicioRegularDireitos |
| ETipoSolicitacaoTitular | Acesso; Correcao; Exclusao; Portabilidade; Anonimizacao |
| EStatusSolicitacaoTitular | Aberta; EmAndamento; Concluida; Negada |
| ETipoIdFiscal | CPF; CNPJ; VAT; EIN; TaxID; ISO6523; Outro |
| ETipoPeriodoPagamento | Dias; Meses |
| EMotivoBloqueioParceiro | Litigio; CertidaoVencida; Sancao; Compliance; Credito; Manual |

## 8. Fluxos funcionais

### 8.1 Cadastro de pessoa

1. Operador inicia nova pessoa e escolhe tipo: fisica, juridica ou estrangeira.
2. O Epros adapta campos do subtipo e exige documento correspondente.
3. Operador marca um ou mais papeis.
4. O Epros exige os objetos dos papeis marcados.
5. Operador informa enderecos e contatos.
6. O Epros valida documento, duplicidade, grupo, municipio, UF, CEP, contato principal, endereco principal e papeis.
7. O Epros calcula score de duplicidade e qualidade.
8. Se nao houver bloqueio, o Epros grava pessoa, subtipos, papeis, contatos, enderecos e historico em transacao.
9. Apos commit, publica evento de pessoa criada ou atualizada.

### 8.2 Alteracao sensivel

1. Operador altera documento, limite, dados bancarios, certificado, status ou bloqueio.
2. O Epros verifica politica de aprovacao do tenant.
3. Havendo politica, abre workflow com dados antes/depois.
4. Aprovador aprova ou rejeita.
5. Aprovado: grava nova versao e audita. Rejeitado: mantem versao anterior.

### 8.3 Importacao de pessoas

1. Usuario com permissao baixa o layout vigente.
2. Usuario envia arquivo.
3. O Epros valida cabecalho, quantidade de colunas, formato e dominio linha a linha.
4. O Epros valida duplicidade, tenant, documento, e-mail, municipio, fiscal, endereco e papeis.
5. O Epros executa transacao conforme politica do lote.
6. O Epros gera relatorio de importacao e eventos para linhas aceitas.

### 8.4 Merge de duplicatas

1. O Epros calcula candidatos por regras configuradas.
2. Analista revisa candidato.
3. Analista escolhe pessoa sobrevivente e campos finais.
4. O Epros transfere vinculos, preserva de-para, audita e marca pessoa incorporada como mesclada.
5. A reversao fica disponivel pelo periodo de carencia definido.

### 8.5 Certificado digital

1. Usuario seleciona empresa.
2. Usuario envia arquivo e senha.
3. O Epros valida arquivo, senha, serial, titular, CNPJ e validade.
4. O Epros grava metadados e guarda segredo protegido.
5. O Epros alerta vencimentos e bloqueia uso de certificado vencido.

## 9. Telas e experiencia operacional

| ID | Tela | Funcao |
|---|---|---|
| TEL-PEM-001 | Lista de pessoas | Busca por nome, documento, papel, UF, IE, status, grupo, bloqueio e qualidade. |
| TEL-PEM-002 | Cadastro de pessoa | Formulario multipapel com abas de dados, papeis, endereco, contato, fiscal, financeiro, documentos, historico e privacidade. |
| TEL-PEM-003 | Grupos de pessoa | CRUD de grupos, descontos, classificacoes e inativacao. |
| TEL-PEM-004 | Lista de empresas | Empresas emitentes, matriz/filiais, status, certificado e parametros. |
| TEL-PEM-005 | Cadastro de empresa | Dados cadastrais, endereco, contato, imagens, SMTP, fiscal, certificado e parametros. |
| TEL-PEM-006 | Certificados | Lista, upload, validacao, validade e alertas. |
| TEL-PEM-007 | Importacao de pessoas | Upload, validacao, preview, erros por linha e relatorio. |
| TEL-PEM-008 | Duplicatas | Candidatos, score, comparacao e merge. |
| TEL-PEM-009 | Dados pessoais | Consentimentos, bases legais e solicitacoes do titular. |
| TEL-PEM-010 | Mapa de contatos | Pessoas ativas com coordenadas ou posicao. |
| TEL-PEM-011 | Extrato do parceiro | Visao de saldos e movimentos consumida do financeiro. |
| TEL-PEM-012 | Documentos e anexos | Contratos, certidoes, comprovantes e documentos por pessoa. |

## 10. APIs funcionais

**Base:** `/api/v1/cadastros`

| Metodo | Rota | Funcao |
|---|---|---|
| GET | `/pessoas` | Lista pessoas com filtros e paginacao. |
| GET | `/pessoas/{id}` | Consulta pessoa completa por id. |
| GET | `/pessoas/localizar-cliente` | Autocomplete de clientes ativos e permitidos. |
| POST | `/pessoas` | Cria pessoa. |
| PUT | `/pessoas/{id}` | Atualiza pessoa. |
| DELETE | `/pessoas/{id}` | Inativa/exclui logicamente conforme vinculos. |
| POST | `/pessoas/importar` | Importa pessoas por layout vigente. |
| GET | `/pessoas/importacoes/{id}` | Consulta resultado de importacao. |
| GET | `/pessoas/duplicatas` | Lista candidatos a duplicidade. |
| POST | `/pessoas/{id}/mesclar` | Executa merge. |
| POST | `/pessoas/duplicatas/{id}/descartar` | Descarta candidato a duplicidade. |
| GET | `/pessoas/{id}/dados-pessoais` | Exporta dados pessoais agregados. |
| POST | `/pessoas/{id}/solicitacoes-titular` | Abre solicitacao do titular. |
| POST | `/pessoas/{id}/anonimizar` | Executa anonimização conforme retencao. |
| GET | `/pessoa-grupos` | Lista grupos de pessoa. |
| POST | `/pessoa-grupos` | Cria grupo. |
| PUT | `/pessoa-grupos/{id}` | Atualiza grupo. |
| DELETE | `/pessoa-grupos/{id}` | Inativa/exclui grupo conforme uso. |
| GET | `/pessoas-enums/{enum}` | Retorna dominios enumerados. |
| GET | `/empresas` | Lista empresas do tenant. |
| GET | `/empresas/{id}` | Consulta empresa. |
| GET | `/empresas/obter-por-cnpj/{cnpj}` | Consulta empresa por CNPJ dentro do tenant. |
| POST | `/empresas` | Cria empresa. |
| PUT | `/empresas/{id}` | Atualiza empresa. |
| PUT | `/empresas/{id}/alterar-logo` | Atualiza logotipo. |
| DELETE | `/empresas/{id}` | Inativa/exclui empresa conforme vinculos. |
| GET | `/empresas/{id}/certificados` | Lista certificados da empresa. |
| POST | `/empresas/{id}/certificados` | Envia e valida certificado. |
| DELETE | `/empresas/{id}/certificados/{certificadoId}` | Remove certificado conforme regras. |
| POST | `/empresas/{id}/testar-email` | Testa configuracao SMTP da empresa. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Visao geral

| Entidade | Papel | Cardinalidade principal |
|---|---|---|
| pessoa | Agregado raiz de pessoa/parceiro | Tenant 1:N Pessoa |
| pessoa_fisica | Subtipo fisico | Pessoa 1:0..1 |
| pessoa_juridica | Subtipo juridico | Pessoa 1:0..1 |
| pessoa_estrangeira | Subtipo estrangeiro | Pessoa 1:0..1 |
| pessoa_grupo | Agrupador comercial/cadastral | Tenant 1:N Grupo |
| pessoa_cliente | Extensao de cliente | Pessoa 1:0..1 |
| pessoa_fornecedor | Extensao de fornecedor | Pessoa 1:0..1 |
| pessoa_funcionario | Extensao de funcionario | Pessoa 1:0..1 |
| pessoa_vendedor | Extensao de vendedor | Pessoa 1:0..1 |
| pessoa_comprador | Extensao de comprador | Pessoa 1:0..1 |
| pessoa_motorista | Extensao de motorista | Pessoa 1:0..1 |
| pessoa_transportadora | Extensao de transportadora | Pessoa 1:0..1 |
| pessoa_prestador_servico | Extensao de prestador | Pessoa 1:0..1 |
| pessoa_contador | Extensao de contador | Pessoa 1:0..1 |
| pessoa_endereco | Endereco da pessoa | Pessoa 1:N |
| pessoa_contato | Contato da pessoa | Pessoa 1:N |
| pessoa_veiculo | Veiculo motorista/transportadora | Pessoa 1:N |
| empresa | Empresa emitente/matriz/filial | Tenant 1:N |
| empresa_grupo | Grupo de empresas | Tenant 1:N |
| empresa_contato | Contato da empresa | Empresa 1:N |
| empresa_certificado | Certificado digital | Empresa 1:N |
| empresa_parametros_fiscais | Parametros fiscais de documento | Empresa 1:1 |
| identificador_fiscal | Identificador por pais/jurisdicao | Pessoa 1:N |
| relacionamento_parceiro | Hierarquia/relacao entre pessoas | Pessoa N:N |
| regra_deduplicacao | Configuracao de matching | Tenant 1:N |
| candidato_duplicata | Candidato a duplicidade | Pessoa N:N |
| consentimento_titular | Consentimento/base legal | Pessoa 1:N |
| solicitacao_titular | Solicitacao de titular | Pessoa 1:N |
| pessoa_historico_estado | Historico de status | Pessoa 1:N |
| pessoa_log_auditoria | Auditoria de alteracao/acesso | Pessoa/Empresa 1:N |
| pessoa_importacao_lote | Controle de importacao | Tenant 1:N |
| pessoa_importacao_linha | Linhas de importacao | Lote 1:N |

### 11.2 Constraints e indices minimos

| Entidade | Constraint/indice |
|---|---|
| pessoa | Indice por TenantId, Status, TipoPessoa, NomeBusca, DocumentoBusca. |
| pessoa_fisica | Unico por TenantId + CPF, exceto politica de consumidor final padrao. |
| pessoa_juridica | Unico por TenantId + CNPJ. |
| pessoa_estrangeira | Unico por TenantId + PaisId + IdentificacaoEstrangeiro quando informado. |
| identificador_fiscal | Unico por TenantId + PaisId + Tipo + Valor. |
| pessoa_grupo | Unico por TenantId + Descricao. |
| pessoa_cliente | Unico por PessoaId; indice por GrupoClienteId e VendedorId. |
| pessoa_fornecedor | Unico por PessoaId; indice por GrupoFornecedorId e CompradorId. |
| pessoa_contato | Indice por PessoaId, TipoContatoEmail, TipoContatoTelefonico, EhPrincipal. |
| pessoa_endereco | Indice por PessoaId, TipoEndereco, MunicipioId, Cep. |
| pessoa_veiculo | Indice por TenantId + Placa; indice por RNTRC. |
| empresa | Unico por TenantId + CNPJ; indice por GrupoEmpresaId. |
| empresa_certificado | Indice por EmpresaId, Serial, ValidadeFinal. |
| candidato_duplicata | Indice por TenantId, Status, PessoaAId, PessoaBId e Score. |
| consentimento_titular | Indice por PessoaId, Finalidade, BaseLegal e DataRevogacao. |
| pessoa_log_auditoria | Indice por TenantId, Entidade, EntidadeId, DataEvento. |

## 12. Dicionario de dados implantavel

### 12.1 pessoa

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK | Gerado pelo Epros. |
| TenantId | texto | 200 | Sim | Indice | Isolamento tenant. |
| CodigoReferencia | texto | Nao informado no material | Nao | Unico opcional por tenant | Gerado automaticamente quando configurado. |
| TipoPessoa | enum | ETipoPessoa | Sim |  | REG-PEM-014. |
| TipoIndicadorIe | enum | ETipoIndicadorIe | Sim |  | REG-PEM-023. |
| PessoaGrupoId | uuid | uuid | Nao | FK pessoa_grupo | Deve existir no tenant. |
| InscricaoSuframa | numero/texto | limite 999999999 quando numerico | Nao |  | REG-PEM-022. |
| TitularContaBancaria | texto | 150 | Nao |  | Dados sensiveis. |
| AgenciaContaBancaria | texto | 20 | Nao |  | Dados sensiveis. |
| NumeroContaBancaria | texto | 20 | Nao |  | Dados sensiveis. |
| TipoPix | enum | ETipoPix | Nao |  | REG-PEM-075. |
| ChavePix | texto | 32 | Nao |  | Mascarar quando necessario. |
| Observacoes | texto | 300 | Nao |  | Sem credenciais. |
| EhCliente | booleano | true/false | Sim |  | Exige pessoa_cliente se true. |
| EhFornecedor | booleano | true/false | Sim |  | Exige pessoa_fornecedor se true. |
| EhTransportadora | booleano | true/false | Sim |  | Mutuamente exclusivo com motorista. |
| EhMotorista | booleano | true/false | Sim |  | Nao permitido para pessoa juridica. |
| EhPrestadorServico | booleano | true/false | Sim |  | Exige pessoa_prestador_servico se true. |
| EhFuncionario | booleano | true/false | Sim |  | Exige pessoa_funcionario se true. |
| EhProdutorRural | booleano | true/false | Sim |  | Papel identificado no material. |
| EhVendedor | booleano | true/false | Nao |  | Campo consolidado de papeis comerciais. |
| EhComprador | booleano | true/false | Nao |  | Campo consolidado de papeis comerciais. |
| EhContador | booleano | true/false | Nao |  | Campo consolidado de papeis fiscais. |
| EhInativo | booleano | true/false | Sim |  | Mantido para compatibilidade funcional de status. |
| StatusPessoa | enum | EStatusPessoa | Sim |  | Rascunho/EmValidacao/Ativo/Inativo/Bloqueado. |
| Bloqueado | booleano | true/false | Nao |  | Gap internacional incorporado. |
| MotivoBloqueio | enum/texto | EMotivoBloqueioParceiro | Nao |  | Obrigatorio quando bloqueado. |
| DataBloqueio | data/hora | ISO 8601 | Nao |  | Auditoria de bloqueio. |
| DataDesbloqueio | data/hora | ISO 8601 | Nao |  | Auditoria de desbloqueio. |
| ScoreQualidade | decimal | 0 a 100 | Nao |  | Calculado. |
| PessoaMescladaEmId | uuid | uuid | Nao | FK pessoa | Preenchido em merge. |
| UsuarioCriacaoId | uuid | uuid | Sim | FK usuario | Auditoria. |
| DataCriacao | data/hora | ISO 8601 | Sim |  | Auditoria. |
| UsuarioAlteracaoId | uuid | uuid | Nao | FK usuario | Auditoria. |
| DataAlteracao | data/hora | ISO 8601 | Nao |  | Auditoria. |

### 12.2 pessoa_fisica

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| TenantId | texto | 200 | Sim | Indice | Isolamento. |
| Cpf | texto validado | CPF | Sim | Unico por tenant | Digito verificador. |
| Nome | texto | 60 | Sim |  | REG-PEM-018. |
| Sobrenome | texto | 100 | Nao |  |  |
| RgNumero | texto | 14 | Nao |  |  |
| RgOrgaoEmissor | texto | 10 | Nao |  |  |
| TipoGenero | enum | ETipoGenero | Nao |  |  |
| DataNascimento | data | ISO 8601 | Nao |  | Preservado de fontes de cobranca/cadastro. |

### 12.3 pessoa_juridica

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| TenantId | texto | 200 | Sim | Indice | Isolamento. |
| Cnpj | texto validado | CNPJ | Sim | Unico por tenant | Digito verificador. |
| RazaoSocial | texto | 60 no material central; 250 em empresa | Sim |  | Usar limite final conforme decisao de MC. |
| NomeFantasia | texto | 60 no material central; 250 em empresa | Nao |  |  |
| InscricaoEstadual | texto | 14 | Condicional |  | Conforme indicador IE. |
| InscricaoMunicipal | texto | 15 | Nao |  |  |
| Cnae | texto | 7 | Nao |  |  |
| DataFundacao | data | ISO 8601 | Nao |  | Preservado de nascimento/fundacao. |

### 12.4 pessoa_estrangeira

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| TenantId | texto | 200 | Sim | Indice | Isolamento. |
| Nome | texto | 60 | Sim |  |  |
| IdentificacaoEstrangeiro | texto | 20 no material central; 30 em dados fiscais | Sim | Unico por tenant/pais quando aplicavel | Limite final pendente na MC. |
| PaisId | uuid/long | Conforme geografia | Nao | FK pais | Necessario para validacao internacional. |

### 12.5 pessoa_cliente

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| TenantId | texto | 200 | Sim | Indice | Isolamento. |
| EhConsumidorFinal | booleano | true/false | Sim |  | Default true em material complementar. |
| TipoContribuinte | enum | ETipoContribuinte | Sim |  |  |
| LimiteCredito | decimal | 22,4 / Nao informado no material | Nao |  | Sem limite quando nulo. |
| VendedorId | uuid | uuid | Condicional | FK pessoa_vendedor | Obrigatorio quando politica exigir. |
| GrupoClienteId | uuid | uuid | Nao | FK pessoa_grupo | Desconto comercial. |
| Desde | data | ISO 8601 | Nao |  | Data de relacionamento. |
| DataCadastro | data/hora | ISO 8601 | Nao |  |  |
| Observacao | texto | Nao informado no material | Nao |  | Observacao comercial. |
| ContaTomador | texto | Nao informado no material | Nao |  | Campo comercial/financeiro. |
| GeraFinanceiro | booleano/texto | Nao informado no material | Nao |  | Definir dominio final na MC. |
| IndicadorPreco | texto/enum | Nao informado no material | Nao |  | Definir dominio final na MC. |
| PorcentoDesconto | decimal | Nao informado no material | Nao |  | Percentual desconto. |
| FormaDesconto | texto/enum | Nao informado no material | Nao |  | Forma do desconto. |
| TipoFrete | texto/enum | Nao informado no material | Nao |  | Padrao comercial. |
| TotalPontosFidelidade | inteiro | Nao informado no material | Nao |  | Quando fidelidade estiver ativa. |
| PontosUsados | inteiro | Nao informado no material | Nao |  | Quando fidelidade estiver ativa. |
| PontosExpirados | inteiro | Nao informado no material | Nao |  | Quando fidelidade estiver ativa. |
| UltimaVenda | data/hora | ISO 8601 | Nao |  | Calculado/atualizado por vendas. |

### 12.6 pessoa_fornecedor

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| TenantId | texto | 200 | Sim | Indice | Isolamento. |
| CompradorId | uuid | uuid | Condicional | FK pessoa_comprador | Obrigatorio quando politica exigir. |
| GrupoFornecedorId | uuid | uuid | Nao | FK pessoa_grupo | Classificacao fornecedor. |
| OptanteSimplesNacional | booleano/texto | Nao informado no material | Nao |  | Dominio final pendente. |
| Localizacao | texto | Nao informado no material | Nao |  |  |
| SofreRetencao | booleano/texto | Nao informado no material | Nao |  |  |
| ChequeNominalA | texto | Nao informado no material | Nao |  |  |
| Observacao | texto | Nao informado no material | Nao |  |  |
| ContaRemetente | texto | Nao informado no material | Nao |  |  |
| PrazoMedioEntrega | inteiro | dias | Nao |  |  |
| GeraFaturamento | booleano/texto | Nao informado no material | Nao |  | Dominio final pendente. |
| NumDiasPrimeiroVencimento | inteiro | dias | Nao |  |  |
| NumDiasIntervalo | inteiro | dias | Nao |  |  |
| QuantidadeParcelas | inteiro | Nao informado no material | Nao |  |  |
| PayTermNumber | inteiro | Nao informado no material | Nao |  | Prazo pagamento. |
| PayTermType | enum | Dias/Meses | Nao |  | Prazo pagamento. |
| UltimaCompra | data/hora | ISO 8601 | Nao |  | Calculado/atualizado por compras. |

### 12.7 demais papeis

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| pessoa_funcionario | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_funcionario | TipoCargo | enum | ETipoCargo | Sim |  |  |
| pessoa_funcionario | ValorPercentualComissao | decimal | 5,2; max 100 | Nao |  |  |
| pessoa_funcionario | Clt | texto | 15 | Nao |  |  |
| pessoa_funcionario | DataAdmissao | data | ISO 8601 | Nao |  |  |
| pessoa_funcionario | Salario | decimal | Nao informado no material | Nao |  | Campo cadastral; folha calcula remuneracao. |
| pessoa_vendedor | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_vendedor | SenhaAPP | texto protegido | 6 | Condicional |  | Guardar como segredo/hash, nunca texto claro. |
| pessoa_vendedor | Email | e-mail | Nao informado no material | Nao |  |  |
| pessoa_vendedor | Meta | decimal | Nao informado no material | Nao |  | Meta comercial. |
| pessoa_vendedor | Gestor | booleano | true/false | Nao |  |  |
| pessoa_vendedor | FormaDesconto | texto/enum | Nao informado no material | Nao |  |  |
| pessoa_vendedor | TipoDesconto | texto/enum | Nao informado no material | Nao |  |  |
| pessoa_comprador | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_motorista | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_motorista | TipoVinculoMotorista | enum | Proprio/Terceiros | Sim |  |  |
| pessoa_motorista | TipoCategoriaCnh | enum | ETipoCategoriaCnh | Condicional |  |  |
| pessoa_motorista | Cnh | texto | 15 | Condicional |  | Obrigatoria quando politica exigir. |
| pessoa_motorista | DataEmissaoCnh | data | ISO 8601 | Nao |  |  |
| pessoa_motorista | DataVencimentoCnh | data | ISO 8601 | Nao |  |  |
| pessoa_motorista | Rntrc | texto | 14 | Nao |  |  |
| pessoa_motorista | TransportadoraId | uuid | uuid | Condicional | FK pessoa_transportadora | Quando motorista vinculado. |
| pessoa_transportadora | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_transportadora | Ciot | texto | 16 | Nao |  |  |
| pessoa_transportadora | Rntrc | texto | 14 | Nao |  |  |
| pessoa_transportadora | Antt | texto | Nao informado no material | Nao |  |  |
| pessoa_transportadora | PlacaVeiculo | texto | 15 | Nao |  |  |
| pessoa_transportadora | Veiculo | texto | 50 | Nao |  |  |
| pessoa_prestador_servico | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_prestador_servico | Cei | texto | 12 | Nao |  |  |
| pessoa_contador | PessoaId | uuid | uuid | Sim | PK/FK pessoa | 1:1. |
| pessoa_contador | Crc | texto | 15 | Nao |  |  |

### 12.8 pessoa_endereco

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| TenantId | texto | 200 | Sim | Indice |  |
| PessoaId | uuid | uuid | Sim | FK pessoa |  |
| TipoEndereco | enum | ETipoEndereco | Sim |  | Principal/Entrega/Obra/Cobranca/Fiscal. |
| PaisId | uuid/long | Conforme geografia | Sim | FK pais | > 0. |
| MunicipioId | uuid/long | Codigo geografico | Condicional | FK municipio | Obrigatorio para endereco nacional. |
| SubdivisaoId | uuid | uuid | Nao | FK subdivisao | Endereco internacional. |
| Uf | texto | 2 | Condicional |  | UF valida quando pais exigir. |
| Cep | texto validado | 8 BR; internacional por pais | Condicional |  | Nulo somente para estrangeiro quando permitido. |
| CodigoPostalInternacional | texto | Nao informado no material | Nao |  | Para paises sem CEP BR. |
| Logradouro | texto | 60 | Sim |  |  |
| Numero | texto | 60 | Nao |  |  |
| Complemento | texto | 60 | Nao |  |  |
| Bairro | texto | 60 | Sim |  |  |
| Referencia | texto | 250 | Nao |  |  |
| LinhaEndereco1 | texto | Nao informado no material | Nao |  | Endereco internacional. |
| LinhaEndereco2 | texto | Nao informado no material | Nao |  | Endereco internacional. |
| Principal | booleano | true/false | Nao |  | Derivado do tipo/flag. |
| Entrega | booleano | true/false | Nao |  | Endereco entrega. |
| MesmoEnderecoCobranca | booleano | true/false | Nao |  | Heranca de endereco. |
| Latitude | texto/decimal | Nao informado no material | Nao |  | Mapa. |
| Longitude | texto/decimal | Nao informado no material | Nao |  | Mapa. |
| Excluido | booleano | true/false | Nao |  | Soft delete operacional. |

### 12.9 pessoa_contato

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| TenantId | texto | 200 | Sim | Indice |  |
| PessoaId | uuid | uuid | Sim | FK pessoa |  |
| Nome | texto | 60 | Nao |  | Nome do contato. |
| NomeContato | texto | Nao informado no material | Nao |  | Campo equivalente em materiais complementares. |
| TipoContatoTelefonico | enum | ETipoContatoTelefonico | Nao |  |  |
| NumeroTelefone | texto | 14 | Nao |  |  |
| TipoContatoEmail | enum | ETipoContatoEmail | Nao |  |  |
| Email | e-mail | 150 no material central; 250 em material complementar | Nao |  | Limite final pendente na MC. |
| Contato | texto | Nao informado no material | Nao |  | Telefone/e-mail generico quando necessario. |
| Detalhe | texto | Nao informado no material | Nao |  | Observacao do contato. |
| Funcao | texto | Nao informado no material | Nao |  | Gap de vigencia/função. |
| EhPrincipal | booleano | true/false | Sim |  | Unico por tipo funcional. |
| VigenciaInicio | data | ISO 8601 | Nao |  | Gap internacional. |
| VigenciaFim | data | ISO 8601 | Nao |  | Gap internacional. |
| Excluido | booleano | true/false | Nao |  | Soft delete operacional. |

### 12.10 pessoa_veiculo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| TenantId | texto | 200 | Sim | Indice |  |
| PessoaId | uuid | uuid | Sim | FK pessoa | Motorista ou transportadora. |
| PaisId | uuid/long | Conforme geografia | Sim | FK pais | > 0. |
| TipoVeiculo | enum | Veiculo/Reboque | Sim |  |  |
| Uf | texto | 2 | Sim |  | Quando pais exigir. |
| Placa | texto | 8 | Sim | Indice | Padrao Mercosul ou antigo. |
| Rntrc | texto | 14 | Nao |  |  |

### 12.11 pessoa_grupo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| TenantId | texto | 200 | Sim | Indice unico com Descricao |  |
| Descricao | texto | 100 | Sim | Unico por tenant |  |
| TipoGrupo | enum/texto | Pessoa/Cliente/Fornecedor | Nao |  | Consolidado de grupos. |
| PercentualDesconto | decimal | 5,2 | Nao |  | Para grupo de cliente. |
| Ativo | booleano | true/false | Sim |  | Inativar quando em uso. |

### 12.12 empresa

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| TenantId | texto | 200 | Sim | Indice |  |
| EmpresaGrupoId | uuid | uuid | Nao | FK empresa_grupo | Matriz/filiais. |
| RazaoSocial | texto | 250 | Sim |  | Empresa emitente. |
| NomeFantasia | texto | 250 | Sim/Condicional |  | Obrigatorio em material complementar. |
| CNPJ | texto validado | 20 | Sim | Unico por tenant | Documento da empresa. |
| RegimeTributario | enum | Nao informado no material | Sim |  |  |
| RegimeApuracao | enum | Nao informado no material | Sim |  |  |
| InscricaoEstadual | texto | 20 | Nao |  |  |
| InscricaoMunicipal | texto | 20 | Nao |  |  |
| InscricaoSuframa | texto | 20 | Nao |  |  |
| Cnae | texto | 7 / Nao informado no material | Nao |  |  |
| PessoaGrupoId | uuid | uuid | Nao | FK pessoa_grupo |  |
| ProdutoGrupoId | uuid | uuid | Nao | FK produto_grupo | Consumido por estoque/produtos. |
| PlanoContasFinanceiroId | uuid | uuid | Nao | FK plano_contas |  |
| TributarioGrupoId | uuid | uuid | Nao | FK grupo tributario | Regras fora do submodulo. |
| NcmTributacaoId | uuid | uuid | Nao | FK fiscal |  |
| CertificadoDigitalId | uuid | uuid | Nao | FK empresa_certificado | Certificado ativo. |
| EmpresaParametrosFiscaisId | uuid | uuid | Nao | FK parametros fiscais |  |
| Logradouro | texto | Nao informado no material | Sim |  | Endereco empresa. |
| Numero | texto | Nao informado no material | Nao |  |  |
| Complemento | texto | Nao informado no material | Nao |  |  |
| Bairro | texto | Nao informado no material | Sim |  | Obrigatorio em material complementar. |
| CEP | texto | Nao informado no material | Nao |  |  |
| UF | texto | 2 | Condicional |  |  |
| MunicipioId | uuid/long | Conforme geografia | Condicional | FK municipio |  |
| Email | e-mail | Nao informado no material | Nao |  |  |
| Telefone | texto | Nao informado no material | Nao |  |  |
| Celular | texto | Nao informado no material | Nao |  |  |
| Logo | texto/arquivo | 500 | Nao |  | Caminho/identificador protegido. |
| ImagemImpressao | arquivo | Nao informado no material | Nao |  | Impressao. |
| ImagemPublicidade1 | arquivo/url | Nao informado no material | Nao |  | PDV. |
| ImagemPublicidade2 | arquivo/url | Nao informado no material | Nao |  | PDV. |
| LinkPublicidade01 | texto/url | Nao informado no material | Nao |  |  |
| LinkPublicidade02 | texto/url | Nao informado no material | Nao |  |  |
| LoginEmail | texto | Nao informado no material | Nao |  | Configuracao SMTP. |
| SenhaEmailSegredoId | uuid | uuid | Nao | FK cofre | Nunca texto claro. |
| HostEmail | texto | Nao informado no material | Nao |  |  |
| PortaEmail | inteiro | Nao informado no material | Nao |  |  |
| SslEmail | booleano | true/false | Nao |  |  |
| TimeoutEmail | inteiro | Nao informado no material | Nao |  |  |
| LinkWebApiAppVendas | texto/url | 500 | Nao |  | Integracao app vendas. |
| TokenPixSegredoId | uuid | uuid | Nao | FK cofre | Substitui armazenamento em claro. |
| ModeloImpressaoVenda | enum | Nao informado no material | Nao |  |  |
| Ativo | booleano | true/false | Sim |  |  |

### 12.13 empresa_grupo, certificado e parametros fiscais

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| empresa_grupo | Id | uuid | uuid | Sim | PK |  |
| empresa_grupo | TenantId | texto | 200 | Sim | Indice |  |
| empresa_grupo | Nome | texto | 250 | Sim | Unico por tenant |  |
| empresa_grupo | Ativo | booleano | true/false | Sim |  |  |
| empresa_certificado | Id | uuid | uuid | Sim | PK |  |
| empresa_certificado | TenantId | texto | 200 | Sim | Indice |  |
| empresa_certificado | EmpresaId | uuid | uuid | Sim | FK empresa |  |
| empresa_certificado | CertificadoSegredoId | uuid | uuid | Sim | FK cofre | Arquivo protegido. |
| empresa_certificado | SenhaSegredoId | uuid | uuid | Sim | FK cofre | Senha protegida. |
| empresa_certificado | Serial | texto | Nao informado no material | Sim | Indice | Exigido apos validacao. |
| empresa_certificado | Titular | texto | Nao informado no material | Nao |  |  |
| empresa_certificado | Informacao | texto | Nao informado no material | Nao |  |  |
| empresa_certificado | CNPJ | texto | Nao informado no material | Nao |  | Extraido/validado. |
| empresa_certificado | ValidadeInicial | data | ISO 8601 | Nao |  |  |
| empresa_certificado | ValidadeFinal | data | ISO 8601 | Nao | Indice | Alertas de vencimento. |
| empresa_parametros_fiscais | Id | uuid | uuid | Sim | PK |  |
| empresa_parametros_fiscais | EmpresaId | uuid | uuid | Sim | FK empresa | 1:1. |
| empresa_parametros_fiscais | CSC | texto | Nao informado no material | Nao |  | Segredo quando aplicavel. |
| empresa_parametros_fiscais | IDCSC | texto | Nao informado no material | Nao |  |  |
| empresa_parametros_fiscais | VersaoXML | texto | Nao informado no material | Nao |  |  |
| empresa_parametros_fiscais | CodigoAtivacao | texto | Nao informado no material | Nao |  | Segredo quando aplicavel. |
| empresa_parametros_fiscais | PastaInput | texto | Nao informado no material | Nao |  | Parametro operacional. |
| empresa_parametros_fiscais | PastaOutput | texto | Nao informado no material | Nao |  | Parametro operacional. |
| empresa_parametros_fiscais | SignAC | texto | Nao informado no material | Nao |  | Segredo quando aplicavel. |
| empresa_parametros_fiscais | ModeloSAT | enum/texto | Nao informado no material | Nao |  |  |
| empresa_parametros_fiscais | TipoAmbienteNfe | enum | Producao/Homologacao | Nao |  |  |
| empresa_parametros_fiscais | TipoAmbienteNfce | enum | Producao/Homologacao | Nao |  |  |
| empresa_parametros_fiscais | DestacarIcmsSt | booleano | true/false | Nao |  |  |

### 12.14 identificadores, relacionamento e qualidade

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| identificador_fiscal | Id | uuid | uuid | Sim | PK |  |
| identificador_fiscal | TenantId | texto | 200 | Sim | Indice |  |
| identificador_fiscal | PessoaId | uuid | uuid | Sim | FK pessoa |  |
| identificador_fiscal | PaisId | uuid/long | Conforme geografia | Sim | FK pais |  |
| identificador_fiscal | Tipo | enum | ETipoIdFiscal | Sim |  |  |
| identificador_fiscal | Valor | texto | Nao informado no material | Sim | Unico por tenant/pais/tipo |  |
| identificador_fiscal | Validado | booleano | true/false | Sim |  | Validacao por jurisdicao. |
| relacionamento_parceiro | Id | uuid | uuid | Sim | PK |  |
| relacionamento_parceiro | TenantId | texto | 200 | Sim | Indice |  |
| relacionamento_parceiro | PessoaOrigemId | uuid | uuid | Sim | FK pessoa |  |
| relacionamento_parceiro | PessoaDestinoId | uuid | uuid | Sim | FK pessoa |  |
| relacionamento_parceiro | TipoRelacao | enum | ETipoRelacaoParceiro | Sim |  |  |
| relacionamento_parceiro | VigenciaInicio | data | ISO 8601 | Nao |  |  |
| relacionamento_parceiro | VigenciaFim | data | ISO 8601 | Nao |  |  |
| regra_deduplicacao | Id | uuid | uuid | Sim | PK |  |
| regra_deduplicacao | TenantId | texto | 200 | Sim | Indice |  |
| regra_deduplicacao | Campo | texto | Nao informado no material | Sim |  | Campo comparado. |
| regra_deduplicacao | Estrategia | enum | EEstrategiaMatch | Sim |  | Exata/Fuzzy/Fonetica. |
| regra_deduplicacao | Peso | decimal | Nao informado no material | Sim |  | Score. |
| regra_deduplicacao | LimiarBloqueio | decimal | Nao informado no material | Sim |  |  |
| regra_deduplicacao | LimiarAlerta | decimal | Nao informado no material | Sim |  |  |
| candidato_duplicata | Id | uuid | uuid | Sim | PK |  |
| candidato_duplicata | TenantId | texto | 200 | Sim | Indice |  |
| candidato_duplicata | PessoaAId | uuid | uuid | Sim | FK pessoa |  |
| candidato_duplicata | PessoaBId | uuid | uuid | Sim | FK pessoa |  |
| candidato_duplicata | Score | decimal | Nao informado no material | Sim |  | Similaridade. |
| candidato_duplicata | Status | enum | EStatusDuplicata | Sim |  |  |

### 12.15 privacidade, auditoria e importacao

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| consentimento_titular | Id | uuid | uuid | Sim | PK |  |
| consentimento_titular | PessoaId | uuid | uuid | Sim | FK pessoa |  |
| consentimento_titular | Finalidade | enum | EFinalidadeDados | Sim |  |  |
| consentimento_titular | BaseLegal | enum | EBaseLegalPrivacidade | Sim |  |  |
| consentimento_titular | DataConsentimento | data/hora | ISO 8601 | Nao |  |  |
| consentimento_titular | DataRevogacao | data/hora | ISO 8601 | Nao |  |  |
| consentimento_titular | Canal | texto | Nao informado no material | Nao |  |  |
| solicitacao_titular | Id | uuid | uuid | Sim | PK |  |
| solicitacao_titular | PessoaId | uuid | uuid | Sim | FK pessoa |  |
| solicitacao_titular | Tipo | enum | ETipoSolicitacaoTitular | Sim |  |  |
| solicitacao_titular | Status | enum | EStatusSolicitacaoTitular | Sim |  |  |
| solicitacao_titular | Prazo | data | ISO 8601 | Sim |  | Prazo legal. |
| pessoa_historico_estado | Id | uuid | uuid | Sim | PK |  |
| pessoa_historico_estado | PessoaId | uuid | uuid | Sim | FK pessoa |  |
| pessoa_historico_estado | EstadoAnterior | enum | EStatusPessoa | Nao |  |  |
| pessoa_historico_estado | EstadoNovo | enum | EStatusPessoa | Sim |  |  |
| pessoa_historico_estado | Motivo | texto | Nao informado no material | Nao |  |  |
| pessoa_historico_estado | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| pessoa_historico_estado | DataEvento | data/hora | ISO 8601 | Sim |  |  |
| pessoa_historico_estado | Ip | texto | Nao informado no material | Nao |  |  |
| pessoa_log_auditoria | Id | uuid | uuid | Sim | PK |  |
| pessoa_log_auditoria | TenantId | texto | 200 | Sim | Indice |  |
| pessoa_log_auditoria | Entidade | texto | Nao informado no material | Sim |  | Pessoa/Empresa/Certificado. |
| pessoa_log_auditoria | EntidadeId | uuid | uuid | Sim | Indice |  |
| pessoa_log_auditoria | Campo | texto | Nao informado no material | Nao |  |  |
| pessoa_log_auditoria | ValorAnterior | texto/json | Nao informado no material | Nao |  | Mascarar sensiveis. |
| pessoa_log_auditoria | ValorNovo | texto/json | Nao informado no material | Nao |  | Mascarar sensiveis. |
| pessoa_log_auditoria | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| pessoa_log_auditoria | DataEvento | data/hora | ISO 8601 | Sim |  |  |
| pessoa_log_auditoria | TipoEvento | enum/texto | Nao informado no material | Sim |  | Alteracao/acesso/merge/importacao. |
| pessoa_importacao_lote | Id | uuid | uuid | Sim | PK |  |
| pessoa_importacao_lote | TenantId | texto | 200 | Sim | Indice |  |
| pessoa_importacao_lote | NomeArquivo | texto | Nao informado no material | Sim |  |  |
| pessoa_importacao_lote | LayoutVersao | texto | Nao informado no material | Sim |  |  |
| pessoa_importacao_lote | Status | enum/texto | Nao informado no material | Sim |  |  |
| pessoa_importacao_lote | TotalLinhas | inteiro | Nao informado no material | Sim |  |  |
| pessoa_importacao_lote | LinhasAceitas | inteiro | Nao informado no material | Sim |  |  |
| pessoa_importacao_lote | LinhasRejeitadas | inteiro | Nao informado no material | Sim |  |  |
| pessoa_importacao_linha | Id | uuid | uuid | Sim | PK |  |
| pessoa_importacao_linha | LoteId | uuid | uuid | Sim | FK lote |  |
| pessoa_importacao_linha | NumeroLinha | inteiro | Nao informado no material | Sim |  |  |
| pessoa_importacao_linha | DadosOriginais | json | Nao informado no material | Sim |  |  |
| pessoa_importacao_linha | Status | enum/texto | Nao informado no material | Sim |  |  |
| pessoa_importacao_linha | MensagemErro | texto | Nao informado no material | Nao |  |  |
| pessoa_importacao_linha | PessoaIdGerada | uuid | uuid | Nao | FK pessoa |  |

## 13. Relatorios e consultas

| ID | Nome | Conteudo |
|---|---|---|
| REL-PEM-001 | Clientes inativos com titulos | Pessoas inativas com saldo ou titulos em aberto. |
| REL-PEM-002 | Fornecedores com pendencias | Fornecedores com saldo, vencimentos, bloqueios ou certidoes vencidas. |
| REL-PEM-003 | Auditoria de alteracoes | Alteracoes por periodo, usuario, entidade e campo. |
| REL-PEM-004 | Qualidade cadastral | Pessoas com score baixo, campos incompletos e duplicidades. |
| REL-PEM-005 | Certificados a vencer | Empresas e certificados por data de vencimento. |
| REL-PEM-006 | Consentimentos e solicitacoes | Bases legais, consentimentos revogados e solicitacoes abertas. |
| REL-PEM-007 | Mapa de parceiros | Pessoas ativas com coordenadas, cidade, UF e contato. |
| REL-PEM-008 | Extrato do parceiro | Visao consolidada de compras, vendas, saldos e pagamentos consumida do financeiro. |

## 14. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-PEM-001 | Pessoa nao e gravada sem pelo menos um papel. |
| CA-PEM-002 | Pessoa fisica nao e gravada com CPF invalido. |
| CA-PEM-003 | Pessoa juridica nao e gravada com CNPJ invalido. |
| CA-PEM-004 | CPF/CNPJ duplicado no tenant e bloqueado. |
| CA-PEM-005 | Pessoa estrangeira exige identificacao estrangeira quando aplicavel. |
| CA-PEM-006 | Motorista e transportadora nao podem coexistir na mesma pessoa. |
| CA-PEM-007 | Motorista pessoa juridica e bloqueado. |
| CA-PEM-008 | Papel marcado sem objeto correspondente e bloqueado. |
| CA-PEM-009 | Cliente com vendedor obrigatorio exige vendedor. |
| CA-PEM-010 | Fornecedor com comprador obrigatorio exige comprador. |
| CA-PEM-011 | Percentual de comissao acima de 100 e bloqueado. |
| CA-PEM-012 | Endereco nacional sem municipio valido e bloqueado. |
| CA-PEM-013 | Municipio fora da UF e bloqueado. |
| CA-PEM-014 | CEP nulo para pessoa nacional e bloqueado. |
| CA-PEM-015 | Mais de um endereco principal e bloqueado. |
| CA-PEM-016 | Sem contato principal quando obrigatorio e bloqueado. |
| CA-PEM-017 | Dois contatos principais do mesmo tipo sao bloqueados. |
| CA-PEM-018 | Placa fora do padrao aceito e bloqueada. |
| CA-PEM-019 | Grupo duplicado no tenant e bloqueado. |
| CA-PEM-020 | Exclusao de pessoa com movimentos gera inativacao/bloqueio, nao exclusao fisica. |
| CA-PEM-021 | Exclusao de papel verifica movimento do proprio papel. |
| CA-PEM-022 | Cliente padrao de PDV nao pode ser excluido. |
| CA-PEM-023 | Empresa sem filtro tenant nao e retornada. |
| CA-PEM-024 | Certificado sem arquivo/senha/serial valido nao e gravado. |
| CA-PEM-025 | Certificado vencido gera alerta. |
| CA-PEM-026 | Segredos de SMTP, PIX e certificado nao aparecem em claro. |
| CA-PEM-027 | Importacao com layout invalido rejeita lote ou linhas conforme politica. |
| CA-PEM-028 | Importacao gera relatorio de linhas aceitas e rejeitadas. |
| CA-PEM-029 | Autocomplete retorna somente clientes ativos permitidos. |
| CA-PEM-030 | Alteracao sensivel gera historico e, quando configurado, workflow. |
| CA-PEM-031 | Merge preserva de-para e audita consolidacao. |
| CA-PEM-032 | Exportacao de dados pessoais agrega os dados da pessoa. |
| CA-PEM-033 | Anonimizacao respeita retencao legal. |
| CA-PEM-034 | Todos os endpoints exigem autenticacao, autorizacao e tenant. |
| CA-PEM-035 | Eventos de dominio sao publicados apos commit. |

## 15. Notas de rodape

[^1]: As entidades `regra_deduplicacao`, `candidato_duplicata`, `consentimento_titular`, `solicitacao_titular`, `identificador_fiscal`, `relacionamento_parceiro`, `pessoa_historico_estado`, `pessoa_log_auditoria`, `pessoa_importacao_lote` e `pessoa_importacao_linha` foram estruturadas a partir das lacunas internacionais e necessidades de governanca registradas no material, para transformar os gaps em desenho funcional validavel.
[^2]: Campos de cofre de segredos foram definidos para substituir armazenamento direto de senha, token e certificado, conforme saneamento de seguranca identificado no material.
[^3]: Limites divergentes entre materiais foram preservados no dicionario com observacao de decisao na MC, sem inventar limite final quando o material nao resolve a divergencia.

