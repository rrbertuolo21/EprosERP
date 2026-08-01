# Plano de Implementação — Criptografia de Envelope e Cofre de Segredos para Configurações Globais (REG-030)

Este documento descreve o plano técnico detalhado para implementar o **Cofre e Criptografia de Segredos (REG-030)** no submódulo **Operação Super Admin (APP-TEN-010)**.

O objetivo é impedir o armazenamento de chaves de API, senhas ou tokens sensíveis (como credenciais dos gateways Stripe, Asaas, etc.) em texto claro na tabela `plataforma.configuracoes_globais` (entidade `ConfiguracaoGlobal`).

Adotaremos a **Criptografia de Envelope (Envelope Encryption)** integrada ao **HashiCorp Vault (Transit Secret Engine)** local, com mecanismo resiliente de fallback local (AES-256-GCM) para ambientes locais de teste e desenvolvimento que rodem offline.

---

## 1. COMPILAMENTO DE COMPORTAMENTO REQUERIDO

### A. Integração com Vault Transit Engine:
1. Utilizaremos o HashiCorp Vault rodando no Docker Compose (porta 8200, token root `epros-dev-token`).
2. Implementaremos a inicialização automática do Vault Transit Engine programaticamente no bootstrap. O monólito verificará se a rota de trânsito está montada (`/v1/sys/mounts/transit`) e, se não estiver, a habilitará e criará a chave mestre de envelope `epros-kek`. Isso garante que o desenvolvedor não precise executar rotinas manuais no CLI do Vault após ligar os contêineres.

### B. Mecanismo de Fallback Offline:
1. Se o Vault estiver inacessível durante a inicialização da API, o sistema ativará silenciosamente o modo de **Criptografia Local**.
2. O modo local utiliza uma chave KEK (Key Encryption Key) simétrica configurada via `appsettings.json` ou variável de ambiente, aplicando AES-256-GCM para emular o comportamento do Vault.
3. Os ciphertexts gerados serão auto-descritivos para fins de auditoria visual no banco de dados:
   - Criptografados pelo Vault: `"vault:v1:<ciphertext>"`
   - Criptografados Localmente: `"local:v1:<ciphertext>"`

---

## 2. COMPONENTES E ARQUIVOS A ALTERAR

### A. Camada Compartilhada (`Epros.Shared`)

#### [NEW] [ISegredoCofreService.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/Shared/Epros.Shared/Application/Contracts/ISegredoCofreService.cs)
- Definir a interface de trânsito criptográfico:
  ```csharp
  public interface ISegredoCofreService
  {
      Task<string> CriptografarAsync(string valor);
      Task<string> DescriptografarAsync(string ciphertext);
  }
  ```

---

### B. Camada de Infraestrutura (`Epros.Infrastructure`)

#### [NEW] [VaultEncryptionService.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/Infrastructure/Epros.Infrastructure/Services/VaultEncryptionService.cs)
- Implementar o serviço de criptografia integrando `HttpClient` com o HashiCorp Vault.
- Gerenciar o ciclo de vida:
  - Tenta autenticar/conectar no Vault e habilitar o Transit Engine se estiver inativo.
  - Tenta criar a chave `epros-kek` se estiver ausente.
  - Implementar criptografia via `/v1/transit/encrypt/epros-kek`.
  - Implementar descriptografia via `/v1/transit/decrypt/epros-kek`.
- Implementar fallback local via `AesGcm` utilizando uma KEK local de 32 bytes configurada se o Vault estiver offline.

---

### C. Camada do Módulo de Gestão de Clientes (`Epros.Modules.GestaoClientes`)

#### [MODIFY] [DefinirConfiguracaoGlobalCommandHandler.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/Modules/Epros.Modules.GestaoClientes/Application/Handlers/DefinirConfiguracaoGlobalCommandHandler.cs)
- Injetar `ISegredoCofreService`.
- Se o comando de configuração global indicar que a chave é um segredo (`EhSegredo == true` ou `request.EhSegredo`), criptografar o valor usando `_cofreService.CriptografarAsync` antes de passá-lo para a entidade/DbContext.

#### [MODIFY] [ObterConfiguracaoGlobalQueryHandler.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/Modules/Epros.Modules.GestaoClientes/Application/Handlers/ObterConfiguracaoGlobalQueryHandler.cs)
- Injetar `ISegredoCofreService`.
- Ao recuperar a configuração, se ela for um segredo (`EhSegredo` da entidade for `true`), descriptografar o valor utilizando `_cofreService.DescriptografarAsync` antes de retornar os dados na resposta da Query.

---

### D. Camada de API Gateway (`Epros.API`)

#### [MODIFY] [Program.cs](file:///Users/rafael/Documents/Codigos/EprosERP/src/API/Epros.API/Program.cs)
- Configurar o `HttpClient` do Vault na inicialização.
- Registrar `ISegredoCofreService` na DI como Singleton (ou Transient dependendo do registro do HttpClient).

---

### E. Camada de Testes (`Epros.Tests`)

#### [NEW] [CriptografiaSegredosTests.cs](file:///Users/rafael/Documents/Codigos/EprosERP/tests/Epros.Tests/CriptografiaSegredosTests.cs)
- Criar testes unitários e de integração validando:
  - Criptografia local com AES-256-GCM (validação do fallback local).
  - Fluxo integrado dos Handlers (`Definir` e `Obter` de configurações) com criptografia ativa para segredos e armazenamento em texto claro para configurações comuns.
  - Descriptografia bem-sucedida ao consultar.

---

## 3. PLANO DE VERIFICAÇÃO

### Testes Automatizados
- Executar os novos testes:
  `dotnet test --filter CriptografiaSegredosTests`
- Executar toda a suíte de testes do monólito:
  `dotnet test`
- Garantir que todos os testes passem com 100% de sucesso.
