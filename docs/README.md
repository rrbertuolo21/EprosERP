# Documentação — EprosERP

> **Novo no projeto?** → [ROTEIRO-ONBOARDING.md](ROTEIRO-ONBOARDING.md) (ordem de leitura)
> **Teste / validação?** → [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) (Docker + seed)
> **Desenvolver (hot reload)?** → [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md)
> Canônicos na raiz: [CLAUDE.md](../CLAUDE.md) · [CONVENCAO_CODIGO.md](../CONVENCAO_CODIGO.md)
> Fábrica: [fabrica/](fabrica/)

## Clone do repositório

```bash
# SSH (recomendado se você já usa chave no GitHub)
git clone --recurse-submodules git@github.com:rrbertuolo21/EprosERP.git
cd EprosERP
```

Se você clonou **sem** `--recurse-submodules` (o mobile fica em `Epros.Mobile/`):

```bash
git submodule update --init --recursive
```

Em seguida: [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) (teste) ou [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md) (dev).

## Árvore


| Pasta / arquivo                                | Papel                                                |
| ---------------------------------------------- | ---------------------------------------------------- |
| [ROTEIRO-ONBOARDING.md](ROTEIRO-ONBOARDING.md) | Índice: o que ler e em que ordem                     |
| [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md)     | Stack Docker de teste/validação (1 comando)          |
| [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md)     | Ambiente de desenvolvimento com hot reload           |
| [fabrica/](fabrica/)                           | Agentes, processo, skills, rules `.mdc`, guias, Jira |
| [onboarding/](onboarding/)                     | Trilha humana longa (produto, stack, tutoriais)      |
| [orquestracao/](orquestracao/)                 | Mapa mestre / plano por submódulo                    |
| [migracao/](migracao/)                         | DePara, GAPs, logs de completude, molde de porte     |
| [historico/](historico/)                       | Planos e memórias arquivados                         |
| [ops/](ops/)                                   | Instalação local expandida, deploy                   |
| [processos/](processos/)                       | Endpoints por ambiente e nomenclatura                |




## Primeiro dia

1. [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) — Docker + seed (smoke) **ou** [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md) se for codar
2. [fabrica/cursor/CONFIGURAR-CURSOR.md](fabrica/cursor/CONFIGURAR-CURSOR.md) — ligar as rules
3. [onboarding/](onboarding/) — trilha longa (opcional, ~1h45)
4. [fabrica/processo/PIPELINE.md](fabrica/processo/PIPELINE.md) — esteira de fases



## Retomada / backlog


| Preciso de…        | Onde                                                                  |
| ------------------ | --------------------------------------------------------------------- |
| Diário da fábrica  | [HISTORICO-DESENVOLVIMENTO-IA.md](../HISTORICO-DESENVOLVIMENTO-IA.md) |
| Gaps vivos         | [CONSOLIDACAO-GAPS.md](../CONSOLIDACAO-GAPS.md)                       |
| Índice IA          | [MEMORY.md](../MEMORY.md)                                             |
| Prompt nova sessão | [PROMPT_REINICIO.md](../PROMPT_REINICIO.md)                           |



Front real: [onboarding/estrutura-pastas-front.md](onboarding/estrutura-pastas-front.md).
Memória de reorganização: [MEMORIA_SESSAO_REORGANIZACAO.md](../MEMORIA_SESSAO_REORGANIZACAO.md).
