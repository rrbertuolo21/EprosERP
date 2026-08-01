# Log de Gaps e Lacunas — Módulo Aplicativo (EprosERP)

Este documento consolida e unifica todas as lacunas funcionais, regras de negócio não especificadas e pendências técnicas encontradas durante o desenvolvimento das quatro camadas de cada submódulo do **Módulo Aplicativo**.

A estratégia acordada é: **implementar as funcionalidades principais de todo o módulo Aplicativo e, após a conclusão do módulo, analisar e tratar todas as lacunas centralizadas aqui de uma só vez.**

---

## Índice de Submódulos

1. [Submódulo: Operação Super Admin (APP-TEN-010)](#1-submódulo-operação-super-admin-app-ten-010) - *Camadas 1 a 4 Concluídas*
2. [Submódulo: Usuários e Papéis (APP-TEN-003)](#4-submódulo-usuários-e-papéis-app-ten-003) - *Camadas 1 a 4 Concluídas*
3. [Submódulo: Assinaturas e Planos (APP-TEN-004)](#2-submódulo-assinaturas-e-planos-app-ten-004) - *Próximo da Fila*
4. [Submódulo: Configuração Geral da Plataforma](#3-submódulo-configuração-geral-da-plataforma) - *Pendente*

---

## 1. Submódulo: Operação Super Admin (APP-TEN-010)

| ID Regra | Prioridade | Item / Lacuna Encontrada | Origem / Detalhe na Especificação | Ação / Resolução Proposta | Status |
|---|---|---|---|---|---|
| **REG-009** | P0 | Modelo seguro de configuração global | A Matriz de Completude (MC) indica que a governança de auditoria e cacheamento para escrita de configurações globais/Siser não está detalhada. | Implementar cache em dois níveis (L1 memory cache + L2 Redis) com invalidação por Pub/Sub ao alterar configurações. | ✅ Concluído |
| **REG-030** | P0 | Cofre/segredo de chaves de gateways | Armazenamento seguro das credenciais dos provedores de pagamento (Stripe, Asaas, etc.). Modelo de criptografia e rotação de chaves indefinido. | Integrar com cofre de chaves (HashiCorp Vault/KeyVault) e adotar criptografia de envelope (AES-256-GCM) nas chaves do tenant. | ✅ Concluído |
| **REG-007**<br>**REG-008** | P0 | Aprovação offline de assinaturas | Ambiguidade no recálculo e precedência de datas de início/fim de vigência de planos no caso de ativações manuais offlines pela Siser. | Definir a fórmula oficial de recálculo de datas e regras de vigência retroativa ou futura. | ✅ Concluído |
| **REG-003** | P0 | Catálogo de indicadores do dashboard | Métricas financeiras e operacionais (MRR, Churn, receita total) listadas no painel da Siser sem a especificação de suas fórmulas contábeis exatas. | Definir o catálogo oficial de indicadores e a integração com a base histórica de faturas de todos os tenants. | ✅ Concluído |
| **10.7** | P0 | Metadados completos de Tenant | Campos adicionais e propriedades administrativas de controle da entidade Tenant/Business não detalhados na EF inicial. | Consolidar a entidade Tenant de forma integrada com os submódulos de Onboarding e Limites de Planos. | ✅ Concluído |
| **REG-015**<br>**REG-019** | P0 | Governança de Instalador e Atualizador | Permissões de superusuário necessárias para rodar atualizações, rotinas de rollback automático e regras de reexecução do instalador abertas. | Desenhar a governança de CI/CD para upgrades de banco de dados e bloqueio de reexecução acidental do instalador. | ✅ Concluído |
| **REG-011** | P1 | Lista de entidades protegidas no Modo Demo | Lista de tabelas e registros a serem bloqueados contra mutações (CUD) para demonstração do sistema não está finalizada. | Criar um interceptor de DbContext do EF Core que bloqueia mutações se o tenant ativo for demo (`IsDemo = true`). | ✅ Concluído |
| **REG-026** | P0 | Aprovação dupla e Rollback em execuções em massa | Por segurança, a execução de scripts em massa exige o fluxo de Maker-Checker completo com teste prévio em sandbox e rollback se falhar. | Implementar pipeline behavior no MediatR para reter comandos de risco na fila de aprovação e criar endpoint de simulação (dry-run). | ✅ Concluído |
| **REG-025** | P1 | Privacidade da Newsletter | Termos de consentimento da LGPD, regras de opt-out fácil e tempo limite de retenção de e-mails de inscritos indefinidos. | Definir políticas de privacidade e mecanismos automáticos para descadastro simplificado do assinante. | ✅ Concluído |
| **REG-020** | P1 | Catálogo de notificações e retries do Comunicador | Canais (E-mail, SMS, WhatsApp), templates de e-mail oficiais e política de retentativas (retries) para envios com falha. | Criar microserviço/serviço integrado de mensageria com fila de retentativas no RabbitMQ/outbox. | ✅ Concluído |

---

## 2. Submódulo: Assinaturas e Planos (APP-TEN-004)
| **REG-035** | P0 | Integração e Conciliação Real com Gateways de Pagamento | O fluxo de pagamento por PIX e faturas é 100% simulado na base. Falta especificação da infraestrutura de webhooks para receber notificações assíncronas dos gateways (Asaas, Efí, Mercado Pago) e automatizar a baixa e ativação de planos. | Implementar módulo de webhooks protegido por assinaturas criptográficas (HMAC) e fila de conciliação assíncrona para registrar o pagamento e alterar o status da fatura de forma confiável. | ✅ Concluído |
| **REG-036** | P1 | Motor de Reajuste Anual Automático por Índices | Embora a entidade `HistoricoReajuste` exista, não há regras especificadas para reajuste anual de contratos usando índices oficiais (IGP-M, IPCA) e aviso prévio ao cliente antes do envio da fatura reajustada. | Desenvolver rotina batch em background (Quartz Job) executada mensalmente que identifica assinaturas com aniversário de contratação de 12 meses e aplica o índice de reajuste parametrizado. | ✅ Concluído |
| **REG-037** | P0 | Regras de Split de Pagamento e Apuração de Comissão | Faltam regras explícitas para cálculo de comissões de `Vendedores` e `Revendas` vinculados a clientes, regras de split (divisão) automático de recebíveis no gateway de pagamento e relatório contábil de apuração. | Configurar split de pagamento diretamente no gateway de faturamento do ERP para reter o percentual da revenda de forma nativa na liquidação da fatura. | ✅ Concluído |
| **REG-038** | P1 | Motor de Cupons e Descontos Promocionais | Falta especificação de regras e validade para cupons promocionais (valor fixo, percentual, descontos por N meses recorrentes ou trial estendido) no checkout de contratação do plano. | Criar entidades `Cupom` e `CupomCliente` com regras de uso único, limite de resgates e data de validade, aplicando o desconto no valor gerado para a primeira ou N faturas. | ✅ Concluído |
| **REG-039** | P0 | Régua de Cobrança e Alerta Pré-Bloqueio | O ERP possui o middleware que bloqueia o tenant inadimplente após 15 dias, mas não existe um fluxo automatizado de régua de cobrança para avisar o cliente por e-mail ou WhatsApp (ex: D-3, D-1, D+1, D+5, D+10, D+15 bloqueado). | Desenvolver serviço de Régua de Cobrança acionado por cron job que dispara alertas interativos com botão de PIX fácil para regularização rápida. | ✅ Concluído |

---

## 3. Submódulo: Configuração Geral da Plataforma

*As lacunas deste submódulo serão registradas aqui durante a sua implementação correspondente nas próximas etapas.*

---

## 4. Submódulo: Usuários e Papéis (APP-TEN-003)

| ID Regra | Prioridade | Item / Lacuna Encontrada | Origem / Detalhe na Especificação | Ação / Resolução Proposta | Status |
|---|---|---|---|---|---|
| **REG-031** | P0 | Acoplamento Transacional Multi-Schema | A gravação de credenciais globais (`Usuario` no schema `aplicativo`) e a criação/sincronização do perfil (`PerfilUsuario` no schema `plataforma`/`GestaoClientes`) ocorrem na mesma transação e dependência física direta de DbContexts. Em cenários distribuídos reais, isso gerará acoplamento rígido. | Adotar padrão Outbox na criação de Usuários para enviar eventos de sincronização via Mensageria (consistência eventual). | ✅ Concluído |
| **REG-032** | P1 | Rotação de ApiKey e Limites de Requisições | Falta detalhamento sobre expiração, rotação obrigatória periódica de `ApiKey` e as regras de rate limit associadas a cada chave de API. | Definir políticas de validade de API Keys e implementar middleware de rate limit dinâmico por ApiKey no gateway de entrada. | ✅ Concluído |
| **REG-033** | P0 | Alerta de Impersonação Ativa | Falta de notificação de segurança ao inquilino/usuários donos quando a impersonação for iniciada pela Siser. | Implementar envio automático de e-mail de alerta de segurança e notificação push ao administrador do tenant alvo quando uma impersonação for iniciada. | ✅ Concluído |
| **REG-034** | P1 | Expiração e Invalidação Física de Sessões | Não há detalhamento sobre políticas de time-to-live (TTL) para sessões de impersonação ativas (e.g. timeout forçado após 2 horas) e a limpeza automática das sessões encerradas/antigas do banco. | Implementar job em background para expiração automática das sessões pendentes e expurgar registros encerrados com mais de 90 dias (LGPD). | ✅ Concluído |
