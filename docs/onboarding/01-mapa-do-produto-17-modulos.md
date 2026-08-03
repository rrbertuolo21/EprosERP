---
title: "17 módulos, 132 submódulos: o mapa do Epros ERP"
confluence_id: "191954953"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/191954953/17+m+dulos+132+subm+dulos+o+mapa+do+Epros+ERP"
last_updated: ""
---

> [!NOTE]
> **O que você vai aprender:** como o Epros está organizado em módulos e submódulos e como cada identificador vira pasta no código.

Antes de abrir o Cursor, você precisa saber em qual **módulo** e **submódulo** está trabalhando.

O Epros é um **mapa de negócio** com 17 módulos e 132 submódulos. Cada feature que você implementar pertence a um ponto desse mapa. Este artigo é o catálogo canônico — use-o em Jira, specs e code review.

## **Como ler o mapa**

Todo pedaço do ERP tem três nomes:


| Conceito                       | O que é                                     | Exemplo        |
| ------------------------------ | ------------------------------------------- | -------------- |
| **Nome de negócio**            | Como o PO e o time falam                    | Contas a Pagar |
| **Identificador do módulo**    | Assembly em `src/Modules/`                  | `Financeiro` → `Epros.Modules.Financeiro` |
| **Identificador do submódulo** | Unidade de planejamento (mapa / Jira)       | `ContasAPagar` |


No repositório, o **identificador de módulo** vira assembly `Epros.Modules.<Nome>`. Submódulos no catálogo são unidades de planejamento (Jira/mapa); o layout interno do assembly é Clean Arch (`Domain` / `Application` / `Infrastructure`), não pasta por submódulo.

`Backend:  src/Modules/Epros.Modules.Financeiro/…`  
`Frontend: EprosApp/pages/erp/financeiro/…` (domínio de UX — ver [estrutura-pastas-front.md](estrutura-pastas-front.md))

> [!IMPORTANT]
> O que está no catálogo de negócio guia Jira e priorização. No código, siga `CONVENCAO_CODIGO.md` e o layout real dos Modules.

> [!TIP]
> Em Jira e conversas técnicas, use **módulo + submódulo** (`Financeiro` / `ContasAPagar`). O nome de negócio entra na User Story; os identificadores entram no código.

---



## **Visão dos 17 módulos**


| Nome de negócio          | Identificador do módulo | Submódulos | O que cobre                                        |
| ------------------------ | ----------------------- | ---------- | -------------------------------------------------- |
| Aplicativo               | `Aplicativo`            | 11         | Multi-tenancy, identidade, planos, permissões SaaS |
| Cadastros Base           | `CadastrosBase`         | 3          | Pessoa, organização, parâmetros operacionais       |
| Financeiro               | `Financeiro`            | 12         | Ledger, contas a pagar/receber, tesouraria         |
| Vendas                   | `Vendas`                | 11         | CRM, pedidos, PDV, e-commerce                      |
| Estoque                  | `Estoque`               | 13         | Produtos, WMS, logística, compras                  |
| Compras                  | `Compras`               | 2          | Gestão de compras, faturamento internacional       |
| Produção                 | `Producao`              | 8          | MRP, BOM, execução de manufatura                   |
| Recursos Humanos         | `RecursosHumanos`       | 9          | Folha, ponto, talentos, treinamento                |
| Manutenção               | `Manutencao`            | 7          | Preventiva, preditiva, ordens de serviço           |
| Qualidade                | `Qualidade`             | 7          | Inspeção, NCR, rastreabilidade, recall             |
| Projetos                 | `Projetos`              | 8          | Planejamento, custos, portfólio                    |
| Governança               | `Governanca`            | 6          | Riscos, auditoria, compliance, políticas           |
| ESG                      | `Esg`                   | 6          | Ambiental, social, relatórios ESG                  |
| Concessionárias          | `Concessionarias`       | 8          | Vendas de veículos, CRM, frota, peças              |
| Imobiliária              | `Imoveis`               | 1          | Gestão imobiliária e contratos                     |
| Relatórios               | `Relatorios`            | 2          | BI, relatórios gerenciais                          |
| Plataforma Compartilhada | `Plataforma`            | 18         | Fiscal, GED, workflow, integrações, IoT            |


> [!TIP]
> O módulo **Aplicativo** é a fundação — sem tenant, identidade e planos, nenhum outro módulo funciona. Por isso `EntidadeSaaSBase` e os middlewares de tenant vivem no Shared.

---



## **Catálogo completo por módulo**



### Aplicativo — Aplicativo · 11 submódulos · infraestrutura SaaS


| Nome de negócio              | Identificador do submódulo |
| ---------------------------- | -------------------------- |
| Identidade e Contexto Tenant | `IdentidadeContextoTenant` |
| Onboarding e Empresa         | `OnboardingEmpresa`        |
| Usuários e Papéis            | `UsuariosPapeis`           |
| Assinatura e Planos          | `AssinaturaPlanos`         |
| Limites de Plano             | `LimitesPlano`             |
| Pedidos e Cobrança SaaS      | `PedidosCobrancaSaas`      |
| Permissões de Menu           | `PermissoesMenu`           |
| Isolamento de Dados          | `IsolamentoDados`          |
| Operação Super Admin         | `OperacaoSuperAdmin`       |
| Dashboard e Layout           | `DashboardLayout`          |
| Catálogos Globais SaaS       | `CatalogosGlobaisSaas`     |




### Cadastros Base — CadastrosBase · 3 submódulos · entidades fundamentais


| Nome de negócio         | Identificador do submódulo |
| ----------------------- | -------------------------- |
| Pessoa e Organização    | `PessoaOrganizacao`        |
| Geografia e Localização | `GeografiaLocalizacao`     |
| Parâmetros Operacionais | `ParametrosOperacionais`   |


### Financeiro — Financeiro · 12 submódulos · hub financeiro central


| Nome de negócio                 | Identificador do submódulo   |
| ------------------------------- | ---------------------------- |
| Contabilidade Geral             | `ContabilidadeGeral`         |
| Contas a Pagar                  | `ContasAPagar`               |
| Contas a Receber                | `ContasAReceber`             |
| Serviços Financeiros            | `ServicosFinanceiros`        |
| Tesouraria e Gestão de Liquidez | `Tesouraria`                 |
| Planejamento e Orçamento        | `PlanejamentoOrcamento`      |
| Ativos Fixos                    | `AtivosFIxos`                |
| Contabilidade Gerencial         | `ContabilidadeGerencial`     |
| Consolidação e Relatórios       | `ConsolidacaoRelatorios`     |
| Câmbio e Risco de Mercado       | `CambioRiscoMercado`         |
| Gestão de Contratos Financeiros | `GestaoContratosFinanceiros` |
| Subsídios e Fundos              | `SubsidiosFundos`            |


### Vendas — Vendas · 11 submódulos


| Nome de negócio                     | Identificador do submódulo          |
| ----------------------------------- | ----------------------------------- |
| CRM                                 | `Crm`                               |
| Gestão de Pedidos                   | `GestaoPedidos`                     |
| Ponto de Venda PDV                  | `Pdv`                               |
| Comércio Eletrônico                 | `ComercioEletronico`                |
| Logística de Saída                  | `LogisticaSaida`                    |
| Gestão de Contratos de Venda        | `GestaoContratosVenda`              |
| Portal do Cliente                   | `PortalCliente`                     |
| Gestão de Serviços                  | `EntregaServico`                    |
| Planejamento de Demanda             | `AnaliseVendas`                     |
| Garantias                           | `Garantias`                         |
| Faturamento Comercial Internacional | `FaturamentoComercialInternacional` |


### Estoque — Estoque · 13 submódulos


| Nome de negócio                        | Identificador do submódulo        |
| -------------------------------------- | --------------------------------- |
| Produtos                               | `Produtos`                        |
| Sourcing e Compras                     | `SourcingCompras`                 |
| Gestão de Armazém WMS                  | `GestaoEstoque`                   |
| Movimentação Manual e Ajustes          | `MovimentacaoAjustes`             |
| Análise e Planejamento de Estoque      | `AnalisesPlanejamentoEstoque`     |
| Inventário Físico e Contagem Cíclica   | `InventarioContagemCiclica`       |
| Rastreabilidade de Lote e Serialização | `RastreabilidadeLoteSerializacao` |
| Logística de Entrada                   | `LogisticaEntrada`                |
| Transporte e Frete TMS                 | `TransporteFreteTms`              |
| Comércio Exterior                      | `ComercioExterior`                |
| Gestão de Contratos de Compra          | `GestaoContratosCompra`           |
| Portal do Fornecedor                   | `PortalFornecedor`                |
| Subcontratação                         | `Subcontratacao`                  |


### Compras — Compras · 2 submódulos


| Nome de negócio                  | Identificador do submódulo       |
| -------------------------------- | -------------------------------- |
| Gestão de Compras                | `GestaoCompras`                  |
| Faturamento Compra Internacional | `FaturamentoCompraInternacional` |


### Produção — Producao · 8 submódulos


| Nome de negócio                  | Identificador do submódulo |
| -------------------------------- | -------------------------- |
| Planejamento de Produção         | `PlanejamentoProducao`     |
| MRP — Planejamento Integrado IBP | `MrpPlanejamentoIntegrado` |
| Estrutura de Produto BOM         | `EstruturaProdutoBom`      |
| Execução de Manufatura MES       | `ExecucaoManufaturaMes`    |
| Escalonamento e Programação      | `EscalonamentoProgramacao` |
| Estimativa                       | `Estimativa`               |
| Gestão de Ordens de Serviço      | `GestaoOrdemServico`       |
| Custos de Produção               | `CustosProducao`           |


### Recursos Humanos — RecursosHumanos · 9 submódulos


| Nome de negócio                 | Identificador do submódulo    |
| ------------------------------- | ----------------------------- |
| Folha de Pagamento e Benefícios | `FolhaPagamentoBeneficios`    |
| Ponto e Jornada                 | `PontoJornada`                |
| Gestão da Força de Trabalho     | `GestaoForcaTrabalho`         |
| Recrutamento                    | `Recrutamento`                |
| Desenvolvimento de Funcionários | `DesenvolvimentoFuncionarios` |
| Gestão de Talentos              | `GestaoTalentos`              |
| Treinamento e Certificações LMS | `TreinamentoCertificacoesLms` |
| Planejamento de RH              | `PlanejamentoRh`              |
| Saúde e Segurança Ocupacional   | `SaudeSegurancaOcupacional`   |


### Manutenção — Manutencao · 7 submódulos


| Nome de negócio                        | Identificador do submódulo       |
| -------------------------------------- | -------------------------------- |
| Manutenção Preventiva                  | `ManutencaoPreventivaPreventiva` |
| Manutenção Preditiva                   | `ManutencaoPreditiva`            |
| Gestão de Trabalho                     | `OrdemServico`                   |
| Gestão de Peças de Reposição           | `PecasReposicao`                 |
| Gestão de Paradas                      | `ParadasProgramadas`             |
| Indução e Configuração de Equipamentos | `GestaoAtivos`                   |
| Confiabilidade e Revisão               | `ConfiabilidadeRevisao`          |


### Qualidade — Qualidade · 7 submódulos


| Nome de negócio                 | Identificador do submódulo |
| ------------------------------- | -------------------------- |
| Planos de Inspeção e Amostragem | `ControleQualidade`        |
| Análise de Aceitação e Rejeição | `AceitacaoRejeicao`        |
| Não Conformidades NCR           | `NaoConformidade`          |
| Rastreabilidade e Recall        | `RastreabilidadeRecall`    |
| Qualidade de Fornecedor         | `AuditoriaQualidade`       |
| Gestão de Atributos             | `GestaoAtributos`          |
| Administração da Qualidade      | `DocumentacaoQualidade`    |


### Projetos — Projetos · 8 submódulos


| Nome de negócio             | Identificador do submódulo |
| --------------------------- | -------------------------- |
| Definição de Projeto        | `GestaoProjetosAgil`       |
| Planejamento e Rastreamento | `PlanejamentoRastreamento` |
| Gestão de Recursos          | `PlanejamentoRecursos`     |
| Planejamento e Orçamento    | `CustosProjeto`            |
| Gestão de Riscos de Projeto | `GestaoRiscosProjeto`      |
| Faturamento de Projeto      | `FaturamentoProjeto`       |
| Portfólio e Priorização     | `PortfolioProjetos`        |
| Encerramento de Projeto     | `EncerramentoProjeto`      |


### Governança — Governanca · 6 submódulos


| Nome de negócio                | Identificador do submódulo |
| ------------------------------ | -------------------------- |
| Gestão de Riscos Corporativos  | `GestaoRiscos`             |
| Controles Internos e Auditoria | `AuditoriaInterna`         |
| Segregação de Funções SoD      | `ControlesInternos`        |
| Compliance Regulatório         | `Compliance`               |
| Gestão de Políticas            | `GestaoPoliticas`          |
| Investigações e Denúncias      | `InvestigacoesDenuncias`   |


### ESG — Esg · 6 submódulos


| Nome de negócio                       | Identificador do submódulo |
| ------------------------------------- | -------------------------- |
| Pegada de Carbono                     | `IndicadoresAmbientais`    |
| Gestão Ambiental EHS                  | `GestaoAmbientalEhs`       |
| Economia Circular                     | `EconomiaCircular`         |
| Transporte Sustentável                | `TransporteSustentavel`    |
| Diversidade e Responsabilidade Social | `ResponsabilidadeSocial`   |
| Relatórios ESG                        | `Governanca`               |


### Concessionárias — Concessionarias · 8 submódulos


| Nome de negócio                    | Identificador do submódulo |
| ---------------------------------- | -------------------------- |
| Vendas                             | `VendasVeiculos`           |
| CRM de Concessionária              | `CrmConcessionaria`        |
| Gestão de Peças de Reposição       | `PecasReposicao`           |
| Gestão de Serviços                 | `GestaoServicos`           |
| Garantias                          | `Garantias`                |
| Manutenção                         | `ManutencaoFrota`          |
| Finanças                           | `FinancasConcessionaria`   |
| Desenvolvimento de Concessionárias | `DesenvolvimentoRede`      |


### Imobiliária — Imoveis · 1 submódulo


| Nome de negócio    | Identificador do submódulo |
| ------------------ | -------------------------- |
| Gestão Imobiliária | `GestaoContratos`          |


### Relatórios — Relatorios · 2 submódulos


| Nome de negócio       | Identificador do submódulo |
| --------------------- | -------------------------- |
| BI OneManager         | `Bi`                       |
| Operacionais Openbook | `RelatoriosGerenciais`     |


### Plataforma Compartilhada — Plataforma · 18 submódulos · serviços transversais


| Nome de negócio                     | Identificador do submódulo    |
| ----------------------------------- | ----------------------------- |
| Faturamento Fiscal Eletrônico       | `FaturamentoFiscalEletronico` |
| Gestão Eletrônica de Documentos GED | `GestaoDocumentosGed`         |
| Assinatura Eletrônica               | `AssinaturaEletronica`        |
| Workflow                            | `Workflow`                    |
| API Gateway e OpenAPI               | `ApiGateway`                  |
| Analytics e Mobilidade              | `AnalyticsMobilidade`         |
| Compliance LGPD SOX IFRS            | `ComplianceLgpd`              |
| IA / ML                             | `IaMl`                        |
| Integrações e Conectores            | `IntegracoesConectores`       |
| SOA e Colaboração                   | `SoaColaboracao`              |
| SDK e Extensões                     | `SdkExtensoes`                |
| Upload e Migração de Dados          | `UploadMigracaoDados`         |
| Interface Assistida Wizards         | `InterfaceAssistidaWizards`   |
| Offline Shell                       | `OfflineShell`                |
| Planejamento In-Memory              | `PlanejamentoInMemory`        |
| Integração IoT                      | `IntegracaoIot`               |
| Impressão Térmica                   | `ImpressaoTermica`            |
| Configuração                        | `Configuracao`                |


---

**Próximo passo →** [Monólito modular: a arquitetura do Epros](02-monolito-modular.md)
