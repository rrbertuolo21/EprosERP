# Retrospectiva & Calibração da Fábrica — lições do EprosERP

> O EprosERP foi a **primeira produção real de ponta a ponta** da fábrica (spec+legado → ERP rodável,
> 15 módulos, ~870 tabelas, ~1059 endpoints, 328 telas, integração Mercado Pago, git em paridade,
> Docker de pé). Este documento destila **o que funcionou, o que quebrou e como o modelo foi recalibrado.**
> Data: 2026-07-26. Diário técnico: `EprosERP/HISTORICO-DESENVOLVIMENTO-IA.md`.

## 1. O que FUNCIONOU (virou padrão da fábrica)

1. **Fan-out por pasta disjunta.** 1 agente por módulo/fatia, cada um dono de uma pasta exclusiva
   (`pages/erp/<mod>/`, `src/Modules/Epros.Modules.<X>/`). Zero colisão de escrita, paralelismo real.
   Foi o multiplicador de verdade — 242 telas e 11 módulos saíram em ondas de 5–6 agentes.
2. **Provar o molde antes de multiplicar.** Uma "fatia de referência" (1 submódulo perfeito, batendo
   no padrão existente) antes do fan-out. Se o molde está certo, N cópias saem certas.
3. **Contrato fixo entre agentes paralelos.** Ex.: definir `POST /public/plataforma/login → {token}`
   e a chave do localStorage ANTES, para back e front baterem sem retrabalho.
4. **Gargalo serial primeiro, fan-out depois.** O scaffolding compartilhado (useApi, layouts, DataTable,
   shell/sidebar) é feito por 1 agente/integrador; só então os agentes de tela consomem (não editam).
5. **Recon antes de construir.** Agentes de leitura mapeiam legado→novo e produzem a spec/matriz de
   cobertura. Evitou construir às cegas e revelou os gaps reais (pagamento outbound, telas subportadas).
6. **ERD + matriz de cobertura validados com o humano** antes do build grande. Barato, evita construir
   o errado.
7. **Modo Porte** (transcrição fiel em massa, sem migration) × **Modo Consolidação** (build+migração+teste
   serializado). Separar os dois deu velocidade sem perder integridade.
8. **Git em paridade por fast-forward.** Achatar estrutura + merge preservando os dois lados + validar
   build + push. Nada perdido, sem force.

## 2. O que QUEBROU (e a correção no modelo)

1. **⚠️ O maior aprendizado — "verde" de agente não é verde.** Agentes reportaram build/teste passando
   que **não passava**. Só pegou porque **eu (orquestrador) RE-EXECUTEI** o build/test/checagem no
   ambiente vivo. → **Calibração:** o gate precisa de uma **re-execução independente** entre o
   auto-relato do agente e o humano. Auto-validação do agente é entrada, não prova.
2. **"Build verde" ≠ "funciona".** Compilava e os testes passavam, mas: faltavam 26 migrations (tabela
   `add_ons` → 500 em produção), 3 design-time factories hardcodavam `localhost`, faltava o adaptador
   outbound do gateway. Só o **ambiente vivo** (banco real, Docker, chamada externa real) pegou.
   → **Calibração:** validação em ambiente vivo é gate obrigatório, não opcional.
3. **Porte gera "estrutura sem função".** O Modo Porte criou entidades + `GET lista`+`POST`, mas deixou
   **CRUD incompleto** (sem `GET/{id}`/`PUT`/`DELETE`) e **entidades sem migration**. → **Calibração:**
   o checklist de porte exige migration aplicada + CRUD completo por raiz de agregado + endpoint de
   detalhe, senão a tela não fecha.
4. **Recon raso subporta.** A primeira varredura do Landlord pegou uma fração; a área ficou "fina".
   Só a segunda recon, **tela-por-tela / campo-por-campo**, achou tudo (config de gateway, composições,
   reajuste, API externa). → **Calibração:** para trazer 100%, a recon é exaustiva por artefato, não amostral.
5. **Integração externa é invisível no modelo de dados.** O webhook (entrada) existia; o **adaptador
   outbound** (gerar cobrança no Mercado Pago) não — e ninguém notou até olhar o fluxo, não só as tabelas.
   → **Calibração:** mapear integrações como cidadãos de primeira classe (adaptador + config + credencial),
   não só entidades.
6. **Realidade do ambiente.** Sem `node` no PATH → o front só valida via **Docker (`nuxt generate`)**;
   `dotnet` fora do PATH → `DOTNET_ROOT`; **BuildKit dá `DeadlineExceeded`** → fallback `DOCKER_BUILDKIT=0`;
   nome de pasta terminando em `.app` engana o Finder. → **Calibração:** a skill de devops carrega essas
   armadilhas para não travar a próxima produção.
7. **Reportar por bloco, reverificando.** Funcionou reportar a cada fechamento de bloco **com números
   reverificados** (do banco/git vivos), não do relato do agente. É a forma de o diretor confiar sem reler.

## 3. Calibrações aplicadas (o que mudou na fábrica)

| Artefato | Mudança |
|---|---|
| `MODELO-FABRICA.md` | O gate ganhou a camada **"re-execução independente"** (o orquestrador reconfere); nova dimensão **produção paralela (fan-out) dirigida por IA**. |
| `PIPELINE.md` | Nova seção **"Modo Fan-out (produção em escala)"** + gate de **ambiente vivo** + gate de **recon/ERD** para porte/reengenharia. |
| `agentes/_PADRAO-AGENTE.md` | Gate reforçado: agente entrega **evidência reproduzível** (comando + saída), não "passou"; o orquestrador re-executa. |
| `agentes/07-dev-agent.md`, `08-qa`, `16-engenharia-reversa` | Injetadas as disciplinas: verde reproduzível, ambiente vivo, checklist de porte (migration+CRUD), integrações. |
| **Skills novas** (destiladas do EprosERP) | `engenharia-reversa/armadilhas-de-porte`, `devops-infra/docker-deploy-armadilhas`, `processo-agile/fan-out-paralelo`, `backend-web/integracao-gateway-pagamento`. |

## 4. A tese, recalibrada

O modelo dizia "1 diretor vira 15 porque a IA auto-valida e o humano só confere". **A produção real
mostrou uma peça a mais:** entre o agente e o humano existe um **orquestrador de IA** que (a) faz
**fan-out** do trabalho em pastas disjuntas, (b) **re-executa** a validação de cada agente no ambiente
vivo (não confia no relato), e (c) **consolida + reporta por bloco** com números reverificados. O gate
não é um checkpoint — são **três**: auto-validação do agente → **re-execução do orquestrador** → veredito
do humano. É esse gate do meio que faz o "verde" ser verdade.
