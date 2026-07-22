# EPROS ERP — Memória de Sessão & Guia de Transição para Reinício Pós-Formatação
> Última atualização: 15 de Junho de 2026 · Monólito Modular em ASP.NET Core 8 / C# · 209 Testes Passando

Este documento consolida o estado atual do projeto **EprosERP**, as entregas concluídas, o backlog funcional e o guia prático passo a passo para restaurar o ambiente local e reiniciar o desenvolvimento exatamente de onde paramos após a formatação de sua máquina física.

---

## 1. ESTADO ATUAL DA SOLUÇÃO (CHECKPOINT DO SISTEMA)

- **Compilação**: 100% limpa (0 erros, 0 warnings).
- **Testes**: 209 testes de unidade e de integração passando com sucesso absoluto (`dotnet test`).
- **Arquitetura**: Monólito modular estruturado com isolamento de schemas PostgreSQL por módulo, comunicação assíncrona entre módulos via Outbox Pattern (Quartz.NET), isolamento físico de inquilinos via Row-Level Security (RLS) nativa automática no banco PostgreSQL e pipeline de middlewares transversais de segurança (Exceções, Tenant, Entitlements, Máscara e Auditoria).
- **Infraestrutura Local (Docker Compose)**: Configurada via `docker-compose.yml` na raiz, incluindo:
  - **PostgreSQL 16**: Banco de dados relacional com RLS e isolamento modular.
  - **Keycloak 24 (OIDC)**: Provedor de identidade com importação automática do Realm `epros-tenant` no boot do contêiner.
  - **HashiCorp Vault 1.16**: Cofre para segredos dinâmicos (token root: `epros-dev-token`).
  - **MinIO**: Storage compatível com S3 para armazenar XMLs, PDFs, etc.
  - **Valkey 7**: Cache híbrido de alta performance L1/L2 com Pub/Sub.

---

## 2. CONTEÚDO JÁ EXECUTADO (ENTREGAS RECENTES)

### A. REG-026: Maker-Checker e Sandbox com Rollback em Execuções em Massa (Concluído)
- **Objetivo**: Evitar que comandos de reajuste em lote e scripts de alto risco alterem o banco de dados sem autorização dupla e simulação dry-run prévia.
- **Implementação**:
  - Criada a interface `IComandoRisco` e o interceptor MediatR `MakerCheckerPipelineBehavior`.
  - Comandos sem `Aprovado = true` são interceptados, gravados como rascunho (`Draft`) na tabela `aplicativo.execucoes_massa_global` contendo o payload serializado, abortando a execução.
  - O `SimularExecucaoMassaGlobalCommand/Handler` executa o comando original em sandbox (`Simular = true`) abrindo uma transação física no banco, gerando logs comparativos detalhados e forçando um `Rollback` obrigatório no final (mantendo dados intactos).
  - O `AtivarExecucaoMassaGlobalCommandHandler` implementa a barreira Maker-Checker: bloqueia a aprovação se o aprovador (`Checker`) for o mesmo usuário que criou a execução (`Maker`).
  - Aprovado por usuário diferente, a execução definitiva ocorre via reflexão com `Aprovado = true` e `Simular = false` no banco de produção.

### B. REG-009: Cache em 2 Níveis (L1/L2) com Invalidação Pub/Sub para Configurações Globais (Concluído)
- **Objetivo**: Otimizar leituras de configurações do sistema (ex: SMTP, trial, chaves de gateways) usando cache híbrido.
- **Implementação**:
  - Adicionado pacote `StackExchange.Redis` no projeto `Epros.Infrastructure`.
  - Criada a interface `IConfiguracaoGlobalCache` e a implementação `ConfiguracaoGlobalCache`.
  - **L1 (Memory Cache local)**: Expiração de 30 minutos.
  - **L2 (Redis/Valkey distribuído)**: Expiração de 1 hora.
  - **Resiliência a Quedas (Fallback)**: Conexão resiliente encapsulada em try-catch. Se o Valkey/Redis estiver indisponível (ex: executando testes offline), o cache emite um warning no log e entra em fallback silencioso operando apenas em L1.
  - **Pub/Sub Reativo**: Subscreve ao canal `"configuracaoglobal:invalida"`. Quando uma configuração é definida via `DefinirConfiguracaoGlobalCommand`, ela invalida o L2 e publica um sinal Pub/Sub para que todas as instâncias limpem seu L1 correspondente em tempo real.
  - A query `ObterConfiguracaoGlobalQueryHandler` reconstrói a entidade de domínio `ConfiguracaoGlobal` do cache e repara o ID privado via reflexão na classe base `EntidadeSaaSBase` (para preservar o `Id` do banco), garantindo compatibilidade.

---

## 3. O QUE FALTA EXECUTAR (BACKLOG DE GAPS DO MÓDULO APLICATIVO)

O progresso é rastreado no arquivo `GAPS_MODULO_APLICATIVO.md` na raiz do projeto. O backlog pendente de implementação é:

1. **REG-030** (Cofre/segredo de chaves de gateways) ➡️ ✅ Concluído
2. **REG-032** (Rotação de ApiKey e Limites de Requisições) ➡️ ✅ Concluído
3. **REG-007 / REG-008** (Aprovação offline de assinaturas) ➡️ ✅ Concluído
4. **REG-003** (Catálogo de indicadores do dashboard) ➡️ ✅ Concluído
5. **10.7** (Metadados completos de Tenant) ➡️ ✅ Concluído
6. **REG-015 / REG-019** (Governança de Instalador e Atualizador) ➡️ ✅ Concluído
7. **REG-025** (Privacidade da Newsletter) ➡️ ✅ Concluído
8. **REG-020** (Catálogo de notificações e retries do Comunicador) ➡️ ✅ Concluído

---

## 4. POR ONDE REINICIAR (PASSO A PASSO PÓS-FORMATAÇÃO)

O plano de implementação para o gap **REG-030 (Cofre/segredo de chaves de gateways)** já foi **aprovado pelo usuário** e é a primeira tarefa a ser executada no reinício dos trabalhos.

### Passo 1: Configurar a Máquina Local
1. Instalar o **Docker Desktop** (ou Docker Engine).
2. Instalar o **.NET 8.0 SDK**.
3. Garantir que as ferramentas do `dotnet` e do `docker` estejam no `PATH` do sistema.

### Passo 2: Inicializar a Infraestrutura
1. Abra um terminal na raiz do projeto e suba os contêineres:
   ```bash
   docker compose up -d
   ```
2. Aguarde de 10 a 15 segundos para que todos os contêineres (especialmente PostgreSQL e Keycloak) inicializem completamente e passem nos testes de saúde.

### Passo 3: Validar e Aplicar Migrations automaticamente
1. Execute a API Gateway para aplicar todas as migrations pendentes de todos os 5 DbContexts em ambiente de Desenvolvimento:
   ```bash
   dotnet run --project src/API/Epros.API/Epros.API.csproj
   ```
2. O console mostrará as mensagens de aplicação de migrations para `ContextGestaoClientes`, `ContextAplicativo`, `ContextEstoque`, `ContextFiscal`, `ContextFinanceiro` e `ContextVendas`.

### Passo 4: Executar a Suíte de Testes
1. Certifique-se de que o ambiente está 100% íntegro executando toda a suíte de testes:
   ```bash
   dotnet test
   ```
2. Todos os 209 testes atuais devem passar com sucesso.

### Passo 5: Iniciar o Gap REG-030
1. Abra a pasta do projeto em sua IDE preferida.
2. Localize os arquivos de plano que deixamos salvos na raiz do repositório:
   - **`PLANO_REG_030_COFRE.md`**: Detalhamento técnico da solução.
   - **`TAREFAS_REG_030_COFRE.md`**: Lista de tarefas e checklist passo a passo para a execução.
3. Inicie o desenvolvimento conforme o planejado.

---

## 5. RECOMENDAÇÃO DE INICIALIZAÇÃO DA ASSISTENTE IA (ANTIGRAVITY)
Ao iniciar uma nova sessão com a IA após a formatação de sua máquina, forneça a seguinte mensagem de contexto:
> *"Olá! Acabei de restaurar meu ambiente pós-formatação. Por favor, leia os arquivos `MEMORIA_SESSAO_TRANSICAO.md`, `PLANO_REG_030_COFRE.md` e `TAREFAS_REG_030_COFRE.md` na raiz do projeto. O plano para a REG-030 já foi aprovado e a infraestrutura local dockerizada está ativa. Vamos retomar o desenvolvimento a partir do passo 1 do checklist em `TAREFAS_REG_030_COFRE.md`."*
