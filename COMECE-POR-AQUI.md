# Comece por aqui — EprosERP (time de desenvolvimento)

> Você recebeu **a pasta do projeto EprosERP** — tudo o que precisa está aqui dentro (você não precisa
> de mais nada da "fábrica"). Este é o **roteiro de leitura**: leia nesta ordem e em ~30 min você
> entende o projeto, sobe o sistema e começa a produzir com o processo.

## Leia nesta ordem

1. **[COMECE-AQUI.md](COMECE-AQUI.md)** — o quickstart: pré-requisitos, **subir o sistema em 1 comando**,
   credenciais de teste, configurar o Cursor, e o **processo em 5 passos**. Comece por ele e já deixe o
   sistema rodando.
2. **[CLAUDE.md](CLAUDE.md)** — o "cérebro" do projeto (o Cursor/Claude carrega sozinho, mas **você
   leia**): o produto em uma frase, a estrutura, e as **disciplinas inegociáveis** (o gate de 3 camadas,
   "verde não é prova", negócio vem da skill, módulos sobem desabilitados).
3. **[CONVENCAO_CODIGO.md](CONVENCAO_CODIGO.md)** — as convenções de código (Modo Porte × Consolidação,
   nomenclatura, multi-tenancy, CQRS). É o que mantém o código coeso.
4. **`_fabrica/`** — o processo da fábrica, enxuto, dentro do projeto:
   - `_fabrica/cursor/CONFIGURAR-CURSOR.md` — **ligue o Cursor** (as regras `.mdc` fazem o Cursor seguir
     as convenções do EprosERP automaticamente). Faça isso no primeiro dia.
   - `_fabrica/processo/` — como a fábrica trabalha (PIPELINE, MODELO, RETROSPECTIVA). Leia quando quiser
     entender o "porquê" do fluxo.
   - `_fabrica/agentes/` — os workers (Dev, QA, Arquiteto, Segurança…). Consulte quando for **delegar**
     uma tarefa a um agente.
   - `_fabrica/skills/` — 4 skills-chave: fan-out paralelo, armadilhas de porte, armadilhas de Docker,
     integração de gateway de pagamento. Leia **quando o tema aparecer**.
5. **[HISTORICO-DESENVOLVIMENTO-IA.md](HISTORICO-DESENVOLVIMENTO-IA.md)** — o diário: o que já foi
   construído (blocos 0–12), decisões e números. Leia para **retomar de onde paramos**.
6. **[CONSOLIDACAO-GAPS.md](CONSOLIDACAO-GAPS.md)** — o que ainda falta (o roadmap). É de onde saem as
   próximas tarefas.

## Quando ler o quê (atalho)

| Momento | Leia |
|---|---|
| **Primeiro dia** | 1 (subir o sistema) → 2 (disciplinas) → `_fabrica/cursor/CONFIGURAR-CURSOR.md` |
| **Antes de codar** | 3 (convenções) + o agente/skill do tema em `_fabrica/` |
| **Vai fazer muita tela/módulo** | `_fabrica/skills/fan-out-paralelo.md` |
| **Mexer em pagamento** | `_fabrica/skills/integracao-gateway-pagamento.md` |
| **Travou no build/Docker** | `_fabrica/skills/docker-deploy-armadilhas.md` |
| **Retomar o projeto** | 5 (HISTORICO) + `PROMPT_REINICIO.md` |
| **O que fazer agora** | 6 (CONSOLIDACAO-GAPS) |

## As 3 regras que você nunca esquece
1. **O "verde" do agente é entrada, não prova** — re-execute o build/test e valide no **ambiente vivo**.
2. **Negócio vem SEMPRE da skill de negócio** — nunca invente regra fiscal/tributária; fiscal fica
   travado até validação de contador.
3. **Commit/push só quando combinado**; nunca direto na `main`.

> Se algo aqui apontar para um arquivo que você não tem, avise o Rafael — o pacote do projeto deve ser
> **autossuficiente**. Bom trabalho. 🚀
