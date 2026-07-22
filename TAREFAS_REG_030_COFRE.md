# Checklist de Tarefas — Criptografia de Envelope e Cofre de Segredos (REG-030)

Utilize esta lista de tarefas como guia técnico e controle de progresso para a implementação da REG-030 após o restabelecimento do seu ambiente local.

---

- [x] **1. Contratos Compartilhados (`Epros.Shared`)**
  - [x] Criar a interface `ISegredoCofreService.cs` em `src/Shared/Epros.Shared/Application/Contracts/`
  - [x] Definir assinaturas `Task<string> CriptografarAsync(string valor)` e `Task<string> DescriptografarAsync(string ciphertext)`

- [x] **2. Serviços de Infraestrutura (`Epros.Infrastructure`)**
  - [x] Criar a classe `VaultEncryptionService.cs` em `src/Infrastructure/Epros.Infrastructure/Services/`
  - [x] Implementar a conexão/inicialização do Vault Transit Engine (habilitação automática de transit engine e criação da chave KEK `epros-kek`)
  - [x] Implementar as operações de criptografia e descriptografia chamando a API Rest do HashiCorp Vault local
  - [x] Implementar o algoritmo AES-256-GCM (`AesGcm` nativo do .NET 8) para criptografia local resiliente
  - [x] Adicionar lógica de fallback silencioso (caso o Vault esteja inacessível) para operar no modo de criptografia local
  - [x] Formatar os ciphertexts de saída com prefixos auto-descritivos (`"vault:v1:"` e `"local:v1:"`)

- [x] **3. Integração na Camada de Negócio (`Epros.Modules.GestaoClientes`)**
  - [x] Injetar `ISegredoCofreService` no `DefinirConfiguracaoGlobalCommandHandler.cs`
  - [x] Criptografar o valor no handler de gravação de configurações se `EhSegredo == true`
  - [x] Injetar `ISegredoCofreService` no `ObterConfiguracaoGlobalQueryHandler.cs`
  - [x] Descriptografar o valor de forma transparente no handler de leitura de configurações se a entidade possuir `EhSegredo == true`

- [x] **4. Registro de Dependências (`Epros.API`)**
  - [x] Configurar e registrar o serviço `ISegredoCofreService` no container de injeção de dependência em `src/API/Epros.API/Program.cs`
  - [x] Configurar chave mestre local simétrica de fallback nas configurações de desenvolvimento local (se aplicável)

- [x] **5. Testes e Homologação (`Epros.Tests`)**
  - [x] Criar classe de testes `CriptografiaSegredosTests.cs` em `tests/Epros.Tests/`
  - [x] Validar unitariamente a criptografia e descriptografia local via AES-256-GCM
  - [x] Validar a resiliência/fallback de serviço offline do Vault
  - [x] Validar o fluxo de ponta a ponta: salvar configuração marcada como segredo (verificar se está cifrada no banco físico mockado) e recuperar o valor limpo descriptografado
  - [x] Executar toda a suíte de testes globais com `dotnet test` e garantir 100% de sucesso
