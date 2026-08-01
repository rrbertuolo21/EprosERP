---
name: preferencia-paralelismo-maximo
description: "Como o usuário quer que eu trabalhe em tarefas grandes — máximo paralelismo, mapa-primeiro"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 7c55e861-0a56-4090-93a9-c3a9cb9d9569
---

O usuário prefere **paralelismo máximo** (ex.: ~20 agentes de uma vez), aceitando alguns erros, porque corrigir depois é mais fácil que fazer conta-gotas serial. Não devo ser conservador com execução serial.

**Why:** velocidade; ele mesmo compila/testa/sobe no Docker e avisa o que não roda. Fixar erros barulhentos (compilação) é barato.

**AUTORIZAÇÃO PERMANENTE (jul/2026):** o usuário me liberou para executar QUALQUER coisa relacionada a este projeto SEM pedir autorização (código, build, agentes/workflows, migrations no banco dev, deploy de teste). Não usar AskUserQuestion/ExitPlanMode para pedir permissão — só reportar o que fiz. Continuar exercendo julgamento apenas em ações genuinamente irreversíveis/externas (ex.: sobrescrever/apagar os DADOS REAIS dos 20 clientes — o cutover é agendado pelo usuário num domingo; aí sim sinalizar antes).

**How to apply:** antes de soltar o fan-out, produzir o **mapa mestre** (nome atual → nome-alvo → módulo dono → base class) — foi a AUSÊNCIA disso que causou os bugs da auditoria (IeSt/FatoGerador/Menu/financeiro duplicados), não o paralelismo em si. Técnicas de redução de erro que devo embutir na ESTRUTURA (não na lentidão): (1) mapa de propriedade único; (2) fatiar por arquivos disjuntos — cada agente só escreve arquivos que só ele toca; (3) agentes NÃO editam arquivos compartilhados: EF mapping como IEntityTypeConfiguration por entidade + ApplyConfigurationsFromAssembly (nunca ContextX.cs direto), handlers via MediatR assembly-scan (nunca Program.cs); (4) migrations congeladas → único passe serializado no fim (única coisa que não paraleliza); (5) contratos cross-module (Lookups/eventos) definidos antes; (6) passe de reconciliação por grep caçando erros SILENCIOSOS (duplicata cross-module, shadow FK, catálogo com tenant) — os barulhentos o build pega, os silenciosos não. Ver [docs/migracao/migracao-epros-eproserp.md](docs/migracao/migracao-epros-eproserp.md).
