# MC 1 Cadastros Base — Pessoa e Organizacao V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Cadastros Base |
| Submodulo | Pessoa e Organizacao |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Pessoa raiz multipapel | Completo | Pessoa com subtipos, flags de papel, grupo, dados bancarios, PIX, SUFRAMA, observacoes e auditoria. | Confirmar papeis vendedor, comprador e contador no mesmo agregado final. | Validar desenho com cadastros, vendas, compras e fiscal. | P0 | Cadastros |
| Pessoa fisica | Parcial | CPF, nome, sobrenome, RG, orgao emissor, genero e nascimento. | Politica de campos obrigatorios alem de CPF/nome. | Fechar obrigatoriedade por pais e por tipo de operacao. | P1 | Cadastros |
| Pessoa juridica | Parcial | CNPJ, razao, fantasia, IE, IM, CNAE. | Divergencia de tamanho de razao/fantasia: 60 no nucleo de pessoa e 250 em empresa. | Decidir limite final por entidade. | P0 | Cadastros/Fiscal |
| Pessoa estrangeira | Parcial | Nome e identificacao estrangeira. | Limite 20 versus 30 e validacao por jurisdicao. | Definir identificador internacional e regra por pais. | P1 | Cadastros/Fiscal |
| Documento e unicidade | Parcial | CPF/CNPJ validado e unico por tenant. | Excecao do consumidor final padrao precisa regra formal. | Criar politica de excecao controlada. | P0 | Cadastros/Vendas |
| Papeis | Parcial | Cliente, fornecedor, transportadora, motorista, prestador, funcionario, produtor rural e extensoes comerciais. | Campos de vendedor/comprador/contador precisam validacao de propriedade final. | Confirmar responsabilidade entre Cadastros, RH, Compras e Vendas. | P0 | Cadastros |
| Cliente | Parcial | Consumidor final, tipo contribuinte, limite, grupo, desconto, frete, saldo, fidelidade e vendedor. | Dominio final de indicador de preco, forma desconto e tipo frete nao informado. | Definir enums comerciais. | P1 | Vendas/Cadastros |
| Fornecedor | Parcial | Comprador, prazo, retencao, faturamento, entrega, pagamento e grupo. | Campos booleanos/textuais precisam normalizacao. | Transformar Sim/Nao textual em booleano/enum. | P1 | Compras/Cadastros |
| Motorista e veiculo | Parcial | CNH, categoria, vencimento, RNTRC, placa e veiculo. | Politica de renovacao/alerta de CNH nao detalhada. | Criar alerta de vencimento e bloqueio opcional. | P1 | Logistica |
| Transportadora | Parcial | CIOT, RNTRC, ANTT, placa e veiculo. | Relacao motorista x transportadora precisa escopo final. | Definir se motorista pode trocar de transportadora por vigencia. | P1 | Logistica |
| Enderecos | Parcial | Pais, municipio, UF, CEP, logradouro, numero, complemento, bairro, referencia e entrega. | Endereco internacional exige subdivisao e formatos por pais. | Integrar com Geografia internacional. | P1 | Cadastros |
| Contatos | Parcial | Nome, telefone, e-mail, tipo e principal. | Limite de e-mail diverge 150/250; vigencia e funcao nao implementadas. | Definir limite final e vigencia. | P1 | Cadastros |
| Empresa emitente | Parcial | Razao, fantasia, CNPJ, endereco, contato, fiscal, imagens, SMTP, app, PIX e parametros. | Fronteira entre empresa emitente e parametros operacionais deve ser validada. | Definir entidade dona de cada parametro. | P0 | Cadastros/Plataforma |
| Certificado digital | Parcial | Upload, senha, serial, titular, validade e CNPJ. | Politica de armazenamento, rotacao e multi-certificado ativo nao detalhada. | Definir cofre e regra de certificado vigente. | P0 | Fiscal/Plataforma |
| Tenant e autorizacao | Incompleto | Material identifica necessidade de tenant em todas as consultas. | Algumas consultas de empresa e listas apareciam sem filtro/autenticacao no material. | Tornar filtro tenant e autenticacao obrigatorios. | P0 | Plataforma |
| Exclusao protegida | Parcial | Bloqueio por compras, vendas, transacoes, cliente padrao e papel correto. | Lista completa de vinculos por papel precisa contrato com modulos. | Criar matriz de vinculos por papel. | P0 | Cadastros/Integração |
| Importacao | Parcial | Importacao transacional com layout, colunas fiscais, validacao e relatorio. | Layout final e politica lote tudo-ou-nada nao definidos. | Publicar layout oficial e politica de rollback. | P0 | Cadastros |
| Deduplicacao e merge | Incompleto | Gap internacional detalha regras e entidades. | Nao implementado no material base. | Construir matching, merge e de-para. | P1 | Cadastros/MDM |
| Privacidade | Incompleto | Gap exige base legal, consentimento, DSR, exportacao e anonimizacao. | Nao implementado como operacao completa. | Integrar com compliance e auditoria. | P0 | Plataforma/Compliance |
| Bloqueio/KYC | Incompleto | Gap exige bloqueio por risco, certidao, sancao e compliance. | Sem modelo final de certidoes e politicas por transacao. | Criar bloqueio prospectivo e certidoes. | P1 | Compliance |
| Identificador fiscal internacional | Incompleto | Gap exige VAT, EIN, TaxID, ISO 6523 e validacao por pais. | Sem catalogo de jurisdicao. | Definir entidade e validadores por pais. | P2 | Fiscal/Cadastros |
| Hierarquia de parceiro | Incompleto | Gap exige matriz/filial, grupo economico e relacionamentos. | Vigencia e impactos financeiros nao detalhados. | Construir relacionamento com vigencia. | P2 | Cadastros |
| Score de qualidade | Incompleto | Gap exige indicador de qualidade cadastral. | Formula nao definida. | Definir pesos e painéis de saneamento. | P2 | MDM |
| Anexos e documentos | Incompleto | Material aponta documentos, notas, certidoes e contratos. | Integração GED nao detalhada. | Criar vinculo versionado com GED. | P2 | Plataforma |
| Testes automatizados | Parcial | Cenarios de documento, duplicidade, contato, importacao, tenant, certificado e privacidade. | Suite completa nao comprovada. | Criar testes por REG/CA. | P0 | QA |

## 3. Pendencias criticas P0

1. Garantir autenticacao, autorizacao e filtro tenant em 100% das rotas de pessoa, empresa, certificado, grupos, autocomplete e importacao.
2. Fechar limites divergentes de razao social, nome fantasia, e-mail e identificacao estrangeira.
3. Definir formalmente a excecao de consumidor final padrao para unicidade de documento.
4. Validar papeis finais do agregado: vendedor, comprador, contador e produtor rural.
5. Definir matriz de bloqueio de exclusao por papel e por modulo consumidor.
6. Definir armazenamento seguro de senha SMTP, token PIX, certificado e senha de certificado.
7. Publicar layout oficial de importacao e politica de rollback.
8. Implantar privacidade operacional: base legal, consentimento, exportacao, anonimizacao e log de acesso.
9. Definir regra de certificado vigente, validacao de CNPJ e alerta de vencimento.
10. Cobrir criterios de aceite com testes automatizados.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| O Epros tera pessoa padrao de consumidor final por tenant? | Define excecao de documento e bloqueio de exclusao. |
| Vendedor, comprador e contador ficam como papeis de Pessoa ou em modulos proprios com referencia a Pessoa? | Define modelo final e obrigatoriedade. |
| Razao social/nome fantasia de PessoaJuridica usam 60 ou 250 caracteres? | Define constraints e telas. |
| E-mail de contato usa limite 150 ou 250? | Define dicionario e validacao. |
| Identificacao estrangeira usa 20 ou 30 caracteres? | Define modelo internacional. |
| Importacao deve rejeitar o lote inteiro ou apenas linhas invalidas? | Define transacao e experiencia operacional. |
| Merge de duplicatas pode ser revertido por quantos dias? | Define auditoria, de-para e governanca. |
| Alteracao de CNPJ sempre exige workflow ou apenas quando houver movimento? | Define fluxo de aprovacao. |
| Certificado pode ter mais de um ativo por empresa? | Define fiscal e parametros de emissao. |
| Bloqueio de parceiro deve impedir todas as transacoes ou apenas alertar por tipo de risco? | Define KYC e compliance. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Modelo Pessoa multipapel | Pessoa, subtipos, papeis, contatos, enderecos, veiculos e grupos. | P0 |
| Modelo Empresa emitente | Empresa, grupo, contatos, certificado e parametros fiscais. | P0 |
| Segurança tenant | Filtro tenant global, autorizacao por rota e testes de vazamento. | P0 |
| Cofre de segredos | SMTP, PIX, certificado e senha de certificado protegidos. | P0 |
| Exclusao protegida | Matriz de vinculos por papel e soft delete. | P0 |
| Importacao | Layout oficial, validacao, relatorio e transacao. | P0 |
| Privacidade | Consentimento, solicitacao, exportacao, anonimizacao e log. | P0 |
| Testes | Suite por regra critica e criterio de aceite. | P0 |
| Deduplicacao | Regras, candidatos, score, merge e de-para. | P1 |
| KYC/Bloqueio | Motivos, certidoes, politicas e bloqueio prospectivo. | P1 |
| Internacionalizacao | Identificadores fiscais e endereco internacional. | P2 |
| Hierarquia | Relacionamento de parceiros e grupos economicos. | P2 |
| Qualidade cadastral | Score e painel de saneamento. | P2 |
| Anexos | Integracao GED para documentos por pessoa. | P2 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-PEM-001 | Todas as entidades da EF possuem dicionario com campo, tipo, tamanho/dominio, obrigatoriedade, relacao e regra. |
| MC-PEM-002 | Campos sem tamanho conhecido estao marcados como Nao informado no material. |
| MC-PEM-003 | Todas as rotas sensiveis exigem autenticacao, autorizacao e tenant. |
| MC-PEM-004 | Todos os dados sensiveis possuem regra de protecao. |
| MC-PEM-005 | Todas as lacunas de padrao internacional estao priorizadas. |
| MC-PEM-006 | A EF contem informacao suficiente para validacao humana e implantacao. |
