# Prompt para Reiniciar a Sessão

Copie e cole o texto abaixo na primeira mensagem do chat com o assistente IA:

```text
Olá! Vamos retomar o desenvolvimento do EprosERP.

Leia nesta ordem:
1. CLAUDE.md — disciplinas e estrutura do repo
2. HISTORICO-DESENVOLVIMENTO-IA.md — o que já foi feito (diário)
3. CONSOLIDACAO-GAPS.md — backlog vivo (próximas tarefas)
4. MEMORY.md — índice de memórias e preferências

Ambiente local (se precisar subir):
  docker compose -f docker-compose.local.yml up -d --build && ./scripts/seed-local.sh
Front http://localhost:3000 · API http://localhost:8080/swagger
Admin: admin@epros.local / Admin@12345 · Demo: cliente@demo.local / Cliente@12345

Convenções: CONVENCAO_CODIGO.md · Processo/fábrica: docs/fabrica/
Comunicação e código em português (BR). Commit/push só quando eu pedir.
```
