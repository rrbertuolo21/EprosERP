# Modelos de IA no Cursor — referência para o time Epros

> **Para quem é:** devs backend e frontend que usam `/dev` e demais agentes no Cursor.
> **Última revisão:** julho/2026 · Fonte de preços: [Cursor — Models & Pricing](https://cursor.com/docs/models)
>
> **Regra do time:** a **skill vence a memória do modelo** — o modelo certo ajuda, mas não substitui as skills (S02, S03, S06…).

---

## 1. Como ler esta tabela

| Coluna | Significado |
|---|---|
| **Consumo** | Quanto o modelo drena do seu plano por requisição (relativo). Agentes consomem mais que Tab/autocomplete. |
| **Capacidade** | Complexidade máxima de tarefa em que o modelo costuma entregar bem no Epros. |
| **Pool** | De onde sai o crédito no plano pago. |

### Escala de consumo (relativa)

| Nível | Símbolo | Perfil |
|---|---|---|
| Muito baixo | ⭐ | First-party com limites generosos; ideal para o dia a dia |
| Baixo | ⭐⭐ | Barato por token; bom custo × desempenho |
| Médio | ⭐⭐⭐ | Frontier padrão; uso consciente em tarefas médias |
| Alto | ⭐⭐⭐⭐ | Frontier premium; reservar para tarefas difíceis |
| Muito alto | ⭐⭐⭐⭐⭐ | Máxima capacidade; drena créditos rápido |

### Escala de complexidade da tarefa (Epros)

| Nível | Exemplos |
|---|---|
| **Baixa** | 1–2 arquivos, sem migration/evento, padrão já existente no módulo |
| **Média** | Feature CQRS ou tela padrão, 3–6 arquivos, testes de handler/componente |
| **Alta** | Migration + domínio + handler, multi-tenant sensível, integração entre módulos |
| **Crítica** | Fiscal/DFe, sync offline, jobs Quartz, auth/LGPD, refactor amplo, arquitetura |

### Dois pools de uso (planos individuais e Teams)

| Pool | Modelos | Comportamento |
|---|---|---|
| **First-party** | Auto, Composer 2.5, Grok 4.5 | Limites **muito mais generosos** no plano pago |
| **API** | Claude, GPT, Gemini (seleção manual) | Debita do crédito API ($20 Pro, $70 Pro+, $400 Ultra) ao preço do provedor |

> **Dica de custo:** use **Auto** ou **Composer 2.5** para rotina; selecione frontier manualmente só quando a tarefa exigir.

---

## 2. Catálogo de modelos disponíveis

Modelos visíveis no seletor do Cursor (jul/2026) usados no fluxo Epros. Preços por **milhão de tokens de saída** — principal driver de custo em modo Agent.

| Modelo (seletor Cursor) | Provedor | Pool | Output ($/M tok) | Consumo | Capacidade | Melhor para |
|---|---|---|---|---|---|---|
| **Auto** | Cursor | First-party | ~$6 (tarifa fixa) | ⭐ | Média | Rotina diária; Cursor escolhe o modelo |
| **Composer 2.5** / **Composer 2.5 Fast** | Cursor | First-party | $2,50 | ⭐ | Média | Agent interativo, refactors moderados, maior parte do `/dev` |
| **Grok 4.5** | Cursor + SpaceXAI | First-party | $6 | ⭐⭐ | Alta | Tarefas longas de código; contexto amplo |
| **Grok 4.5 Fast** | Cursor + SpaceXAI | First-party | $18 | ⭐⭐⭐ | Alta | Mesmo perfil, mais velocidade (2× custo) |
| **Gemini 3.5 Flash** | Google | API | $9 | ⭐⭐ | Baixa–Média | Edits rápidos, perguntas pontuais, baixo custo |
| **Gemini 3.1 Pro** | Google | API | $12 | ⭐⭐⭐ | Alta–Crítica | Raciocínio longo, análise fiscal/arquitetura |
| **GPT-5.6 Luna** | OpenAI | API | $6 | ⭐⭐ | Baixa–Média | Variante econômica GPT-5.6; tarefas simples |
| **GPT-5.6 Terra** | OpenAI | API | $15 | ⭐⭐⭐ | Alta | Multi-arquivo, agente com raciocínio; bom meio-termo |
| **GPT-5.6 Sol** | OpenAI | API | $30 | ⭐⭐⭐⭐ | Crítica | Tarefas agentic longas; exige Max Mode |
| **Claude Sonnet 5** | Anthropic | API | $15* | ⭐⭐⭐ | Alta | Padrão do time para features médias/altas |
| **Claude Sonnet 5 Thinking** | Anthropic | API | ~$30† | ⭐⭐⭐⭐ | Alta–Crítica | Raciocínio explícito; migrations, tenancy, integrações |
| **Claude Opus 4.8** | Anthropic | API | $25 | ⭐⭐⭐⭐ | Crítica | Decisões difíceis, code review profundo |
| **Claude Opus 4.8 Thinking** | Anthropic | API | ~$50† | ⭐⭐⭐⭐⭐ | Crítica | Máxima capacidade; fiscal, segurança, refactor amplo |
| **Claude Fable 5** | Anthropic | API | $50 | ⭐⭐⭐⭐⭐ | Crítica | Frontier experimental; ~2× Opus; requer Max Mode |

\* Promo Sonnet 5 até ago/2026: $10/M output.  
† Variantes *Thinking* multiplicam consumo (mais tokens de raciocínio interno).

### Modos especiais

| Modo | Efeito no consumo | Quando usar |
|---|---|---|
| **Plan mode** | Similar ao modelo escolhido, mas em etapas | Etapa 1 do `/dev` — planejar antes de codar |
| **Max Mode** | Contexto até 1M tokens; **consome mais rápido** | Codebase grande, refactor cross-módulo |
| **Premium** (seletor) | Cursor escolhe o frontier mais capaz | Quando não sabe qual modelo; custo variável |

---

## 3. Matriz de escolha — complexidade da tarefa × modelo

Guia usado na **Etapa 1 (Planejamento)** do command `/dev` (backend e frontend).

| Complexidade da tarefa | Recomendado | Alternativa econômica | Alternativa máxima |
|---|---|---|---|
| **Baixa** | Composer 2.5 Fast · Gemini 3.5 Flash | (mesmo) | Claude Sonnet 5 |
| **Média** | Claude Sonnet 5 · GPT-5.6 Terra | Composer 2.5 Fast | Claude Opus 4.8 |
| **Alta** | Claude Sonnet 5 Thinking · GPT-5.6 Terra | Sonnet 5 (sem thinking) | Claude Opus 4.8 Thinking |
| **Crítica** | Claude Opus 4.8 Thinking · Gemini 3.1 Pro | Sonnet 5 Thinking | Opus 4.8 (manter) |

### Por fase / agente (sugestão rápida)

| Momento | Modelo sugerido | Por quê |
|---|---|---|
| Tab / autocomplete | Auto (padrão) | Incluso, sem escolha manual |
| `/dev` — planejamento (Etapa 1) | Composer 2.5 Fast ou Sonnet 5 | Rápido para decompor; Plan mode se preferir |
| `/dev` — implementação baixa/média | Composer 2.5 Fast | Pool first-party generoso |
| `/dev` — implementação alta/crítica | Sonnet 5 Thinking ou Terra | Raciocínio + custo controlado |
| `/architect`, `/planning` | Sonnet 5 Thinking ou Opus 4.8 | Decisões com trade-offs |
| `/code-review` | Sonnet 5 ou Opus 4.8 | Segurança/tenancy → Opus |
| `/qa` | Composer 2.5 Fast ou Gemini 3.5 Flash | Planos de teste, edge cases |
| `/fiscal` | Sonnet 5 Thinking ou Opus 4.8 Thinking | Domínio crítico; não economize aqui |

---

## 4. Política de custo do time

1. **Comece barato.** Se a tarefa é baixa/média, use Composer 2.5 Fast ou Auto.
2. **Suba de degrau só com motivo.** Thinking/Opus quando tenancy, fiscal ou refactor amplo exigirem.
3. **Troque antes de executar.** Na Etapa 1 do `/dev`, o agente recomenda modelo — confirme no seletor (`Cmd/Ctrl + /`).
4. **Max Mode com parcimônia.** Só para contexto que não cabe no padrão.
5. **Monitore o uso.** [Dashboard de uso](https://cursor.com/dashboard/usage) — se passar de 70% do crédito API no meio do mês, ajuste modelo ou avalie Pro+.

---

## 5. Links oficiais

- [Models & Pricing](https://cursor.com/docs/models) — tabela completa e preços por token
- [Available models](https://cursor.com/help/models-and-usage/available-models) — Auto, Premium, regiões
- [Usage and limits](https://cursor.com/docs/account/usage) — pools e limites do plano

> Preços e modelos mudam com o vendor. Em caso de divergência, a documentação oficial prevalece — atualize a data no topo deste arquivo.
