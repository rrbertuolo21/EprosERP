# Prompt para Reiniciar a Sessão Pós-Formatação

Copie e cole o texto abaixo integralmente na primeira mensagem do chat com o assistente IA na nova sessão pós-formatação:

```text
Olá! Acabei de restaurar meu ambiente local após a formatação física da máquina. 

Por favor, siga estritamente as diretrizes abaixo para retomarmos o desenvolvimento exatamente de onde paramos:

1. Leia o arquivo principal de transição na raiz do projeto:
   -> MEMORIA_SESSAO_TRANSICAO.md
2. Leia o plano de arquitetura aprovado para a tarefa atual na raiz do projeto:
   -> PLANO_REG_030_COFRE.md
3. Leia o checklist técnico contendo o status detalhado da tarefa na raiz do projeto:
   -> TAREFAS_REG_030_COFRE.md

Restrições e Informações Importantes do Ambiente:
- Nosso monólito está compilando com sucesso e temos 209 testes passando.
- A infraestrutura local dockerizada (Postgres, Vault, Valkey, Keycloak, etc.) já está em execução na minha máquina física.
- Mantenha toda a comunicação técnica e codificação estritamente em PORTUGUÊS.
- A nossa tarefa prioritária atual é a REG-030 (implementação do cofre e criptografia de segredos das chaves dos gateways).

Por favor, faça a leitura dos 3 arquivos informados e, com base neles, crie o arquivo local "task.md" na pasta de artifacts correspondente da IA e inicie a codificação da REG-030 a partir do passo 1 ("Contratos Compartilhados (Epros.Shared)").
```
