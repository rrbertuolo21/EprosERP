---
name: arquitetura-orientada-eventos
categoria: arquitetura
tags: [eda, evento, event-driven, broker, mensageria, pub-sub, publish-subscribe, point-to-point, request-reply, event-streaming, fire-and-forget, notificacao-de-evento, event-notification, event-carried-state-transfer, claim-check, event-sourcing, cqrs, materialized-view, dlq, dead-letter-queue, fifo, ordering, particao, saga, orquestracao, coreografia, compensacao, outbox, transactional-outbox, cdc, change-data-capture, log-tailing, wal, debezium, kafka-connect, webhook, idempotencia, consumidor-idempotente, at-least-once, at-most-once, exactly-once, ack, semantica-de-entrega, schema-registry, schema-evolution, backward-compatibility, forward-compatibility, versionamento-de-topico, avro, protobuf, cloudevents, asyncapi, eventstorming, push, pull, prefetch, batch, roteamento-inteligente, content-based-routing, retencao, replay, offset, visibilidade, agendamento, event-mesh, broker-orientado-fila, broker-orientado-log, broker-orientado-assinatura, kafka, rabbitmq, eventbridge, sqs, kinesis, solace, serverless, streaming, ipaas, api-gateway, consistencia-eventual]
nivel: avancado
aplica-se-a: [qualquer-linguagem, microsservicos, mensageria, streaming, integracao-de-servicos, backend, sistemas-distribuidos, serverless, event-driven]
fontes:
  - "Arquitetura Orientada a Eventos: soluções escaláveis e em tempo real com EDA — Casa do Código/Alura, 2023 (caps. 1-13)"
status: em-construcao
revisao: semestral
---

# Arquitetura Orientada a Eventos (EDA)

> **Conhecimento agnóstico** — vale para qualquer broker (Kafka/RabbitMQ/EventBridge/SQS/Solace…),
> qualquer nuvem e qualquer linguagem. Qual broker concreto, qual formato de payload, quais eventos
> existem e como o outbox é implementado *neste* projeto fica no overlay — ver "Como um projeto
> aterra isto". Esta skill é irmã de [[../sistemas-distribuidos]] (mensageria, idempotência, saga,
> outbox, exactly-once vistos do ângulo da teoria distribuída); aqui o ângulo é **de desenho de
> solução EDA e escolha de broker**.

## Quando usar

Ative quando a tarefa envolver: **desenhar comunicação por eventos entre serviços; escolher entre
request-response e assíncrono; escolher o tipo de broker (fila × log × assinatura) por caso de uso;
decidir quanto do dado vai no evento (notificação × event-carried state transfer × claim check);
aplicar event sourcing e/ou CQRS; garantir entrega (at-most/at-least/exactly-once) e tornar o
consumidor idempotente; ordenar eventos (FIFO/partição); coordenar transação entre serviços (saga
coreografado × orquestrado, compensação); publicar evento sem dual-write (transactional outbox / CDC /
log tailing); tratar erro/veneno com DLQ e retry; expor/consumir webhooks; versionar schema de evento
sem quebrar consumidores (schema registry, backward/forward, versionar tópico); configurar capacidades
do broker (push/pull, batch/prefetch, roteamento inteligente, ack, retenção, replay, visibilidade,
agendamento); descobrir e documentar eventos (EventStorming, AsyncAPI, CloudEvents); ou integrar EDA
com microsserviços/serverless/streaming/iPaaS/API Gateway.**

## Princípios

**Evento é um fato consumado e imutável — pense "X ocorreu", não "faça Y".** Um evento representa uma
mudança de estado que já aconteceu e não pode ser desfeita. Comando cria acoplamento (o chamador
precisa conhecer o endpoint/API do outro e esperar); evento desacopla (o produtor **não conhece** os
consumidores). Essa inversão é a mudança de paradigma da EDA — e a fonte de todos os seus benefícios e
custos.

**EDA existe para mitigar a complexidade do distribuído, não para adicioná-la.** O ganho é
desacoplamento mínimo (só se conhece o evento) → escalabilidade, extensibilidade (adicionar consumidor
sem tocar em quem existe), disponibilidade (o negócio segue mesmo com parte inoperante — o broker retém
o evento) e tempo real. O custo é real: EDA é **mais difícil de entender e testar** que request-response
(fluxo assíncrono, coreografia implícita, compensação, rastreabilidade, governança). Adote por
requisito, não por moda.

**EDA é assíncrona e eventualmente consistente — não a use onde precisa de consistência forte.** Se o
dado precisa estar correto para todos os leitores imediatamente após a escrita (saldo logo após saque),
EDA é a ferramenta errada para *aquela* parte. Use request-response/consistência forte onde o negócio
exige, e EDA no resto. Quase toda solução real mistura os dois.

**O valor de um evento decai com o tempo.** Um "momento de negócio" é uma oportunidade transitória —
processar tarde é perder valor (ou vidas, no sensor de acidente). Isso justifica processamento em tempo
real (data in motion) em vez de batch D+1 (data at rest).

**A garantia de entrega é uma escolha de negócio, com três semânticas.** *At-most-once*: tolera perda,
nunca duplica, mais barato (não persiste) — ex.: e-mail. *At-least-once*: não tolera perda, **pode
duplicar**, é o padrão sensato — ex.: notificar hospital. *Exactly-once*: sem perda nem duplicação, o
mais difícil, poucos brokers suportam (exige id único do produtor + descarte no broker + controle no
consumidor). **Na prática, exactly-once "real" = at-least-once + consumidor idempotente.** Assuma
at-least-once e projete o consumidor para deduplicar.

**Consumidor idempotente é obrigatório, não opcional.** Processar o mesmo evento N vezes deve ter o
mesmo efeito de processar 1 vez. Guarde o **id do evento** (o broker garante único) numa tabela de "já
processados" e ignore duplicatas (com janela de tempo, se fizer sentido). Sem isso, at-least-once
cobra duas vezes, envia dois e-mails, movimenta estoque em dobro.

**Decidir quanto do dado vai no evento é a decisão de design mais frequente.** *Notificação de evento*
(mínimo, só o id) é leve, mas força o consumidor a uma **chamada remota extra** de volta ao produtor —
reintroduz o acoplamento que a EDA veio eliminar. *Event-carried state transfer* (dado completo/relevante
no corpo) remove a chamada, mas cresce o payload, expõe dados sensíveis (LGPD) e amarra o schema.
*Claim check* (id + dado num storage externo) é para payload grande (imagem/vídeo que estoura o limite do
broker). Regra prática: modele o **mínimo que os consumidores conhecidos precisam**, prevendo o uso.

**O tipo de broker decorre dos requisitos — fila, log ou assinatura.** *Orientado a fila* (RabbitMQ, SQS,
Service Bus): remove após ack, foco em point-to-point/request-reply/pub-sub com FIFO/DLQ/roteamento —
baixa latência, trabalho desacoplado. *Orientado a log* (Kafka, Kinesis, Event Hubs): **não remove**,
append-only particionado, replay de qualquer offset, retenção longa, ordem por partição — streaming de
alto volume, event sourcing, histórico. *Orientado a assinatura* (EventBridge, SNS, Event Grid): push por
regra de filtro, webhooks, integração nativa cloud/serverless — EDA cloud-native num provedor. Escolher o
broker é a etapa inicial; depois casar cada capacidade (deploy, schema, ack, retenção…) com os requisitos.

**Publicar evento e gravar no banco são dois passos que precisam ser um só.** Gravar no BD e **depois**
publicar no broker é um **dual-write não atômico**: se a publicação falha, o mundo diverge e ninguém sabe
da mudança. É a causa nº 1 de falha em projetos de EDA. Resolva com **transactional outbox** (gravar o
evento na mesma transação do dado, um relay publica depois) ou **CDC/log tailing** (Debezium/Kafka Connect
lendo o WAL/binlog — de preferência a tabela outbox). No mínimo, tenha um **processo explícito para o caso
de erro** de publicação.

**Transação entre serviços é saga, não 2PC — e compensação, não rollback.** Uma operação que cruza vários
serviços vira uma sequência de transações locais (cada uma ACID no seu BD), cada passo com sua
**compensação** que desfaz o efeito. *Coreografado* (serviços reagem a eventos, sem maestro): desacoplado,
encaixa em EDA, mas fluxo implícito e difícil de rastrear/testar — para processos simples. *Orquestrado*
(um controlador dita passos e compensações): visão clara, testável, mas ponto de falha e mais complexo —
para processos complexos. O **fluxo de compensação direciona o desenho** da coreografia (não paralelize
cegamente o caminho feliz se compensar for caro).

**Ordem é garantida por contexto, dentro de uma partição/grupo — não globalmente.** FIFO estrito custa
throughput e latência; só é preciso *dentro de um contexto* (ex.: eventos do mesmo produto). Roteie por
**chave de agrupamento** (Kafka `key`→partição; SQS `group_id`; Service Bus `session_id`). Fora do
contexto, a ordem é irrelevante — e "best-effort ordering" é o normal em brokers distribuídos.

**Schema é um contrato — versione-o como uma API.** Sem schema, o contrato é implícito e frágil. Um
**schema registry** guarda versões e o (de)serializa (a serialização É a validação); só o **id** do
schema viaja no payload. Toda evolução deve passar por **verificação de compatibilidade**: *backward*
(consumidor novo lê produtor antigo — permite remover campo / adicionar opcional com default), *forward*
(consumidor antigo lê produtor novo — permite adicionar campo / remover opcional com default), *full* (os
dois). Mudança compatível → **um tópico, várias versões** (sem redeploy do consumidor). Mudança que
**quebra contrato** → **um tópico por versão** (`-v1`, `-v2`) e versionar as apps, aposentando a antiga
após migrar.

**O broker é o coração — trate segurança e governança como requisito desde o primeiro projeto.** Ele deve
autenticar/autorizar (ACL por tópico/fila, OPA declarativo), criptografar em trânsito (TLS/mTLS) e em
repouso, e ser auditável+monitorado. A melhor defesa de dado sensível é **não colocá-lo no schema**.
Governança (catálogo de eventos, developer portal, AsyncAPI) parece dispensável no início e vira
inviável quando a coreografia cresce para centenas de eventos.

## Na prática

```txt
❌ Produtor publica evento mínimo e o consumidor faz chamada REST de volta buscar o resto do dado
✅ Event-carried state transfer: o evento carrega o que os consumidores conhecidos precisam

❌ Gravar no banco e DEPOIS publicar no broker ("salvei, agora emito o evento")
✅ Transactional outbox: evento na MESMA transação do dado; relay/CDC publica a partir da outbox

❌ Consumidor at-least-once sem dedupe → cobra 2×, movimenta estoque em dobro
✅ Guardar o id do evento numa tabela de processados; reprocessar vira no-op (idempotência)

❌ Prometer "exactly-once delivery" e confiar no broker
✅ At-least-once + consumidor idempotente (exactly-once é processing, não delivery)

❌ 2PC/transação distribuída entre microsserviços
✅ Saga: transações locais + eventos + compensações idempotentes

❌ Evento de erro genérico → todo consumidor abre o corpo p/ ver se lhe interessa (processa à toa)
✅ Eventos de erro específicos (PagamentoNegado) como gatilhos direcionados da compensação

❌ FIFO global no tópico inteiro (mata throughput) para garantir ordem de um produto
✅ Ordenar por chave de contexto (partição/group_id) — só onde a ordem importa

❌ Mudar o schema adicionando campo obrigatório e quebrar consumidores em runtime
✅ Passar pela verificação de compatibilidade (backward/forward); quebra de contrato → versionar tópico

❌ Escolher Kafka porque "é o padrão" para um caso de fila de tarefas com baixa latência
✅ Escolher o tipo de broker pelo requisito: log (replay/streaming) × fila (task/latência) × assinatura (cloud-native)

❌ Vários consumidores recebem tudo e descartam o que não é do seu tipo (desperdício)
✅ Roteamento inteligente: 1 tópico + regra de filtro por consumidor (content-based)

❌ Payload de imagem/vídeo direto no evento (estoura o limite do broker, alta latência/custo)
✅ Claim check: gravar o binário num blob storage e mandar só a URL/id no evento

❌ Ack automático em processamento longo → falha depois do ack = evento perdido
✅ Ack manual após processar com sucesso (aceitando reprocesso → idempotência cobre)
```

**Modelo mental do fluxo:** produtor → (header + payload em JSON/Avro/ProtoBuf, schema versionado) →
**broker** (roteamento, persistência, garantia de entrega, DLQ) → consumidor (push ou pull, ack
auto/manual, idempotente). O broker desacopla no tempo (retém p/ consumidor offline) e no espaço
(produtor não conhece consumidor).

## Receitas (tarefas canônicas — o agente adapta, não reinventa)

### 1. Escolher o tipo de broker pelo caso de uso (fila × log × assinatura)

Antes de escolher fornecedor, escolha o **tipo** pela característica dominante:

```txt
Preciso de replay / retenção longa / event sourcing / streaming de alto volume / ordem por partição?
   → LOG-oriented (Kafka, Kinesis, Event Hubs, MSK, Confluent)

Preciso de task queue / point-to-point / request-reply / baixa latência / FIFO+DLQ / remover após ack?
   → QUEUE-oriented (RabbitMQ, SQS, Amazon MQ, ActiveMQ, Azure Service Bus, Solace, Google Pub/Sub)

Estou 100% cloud-native/serverless num provedor, quero webhooks/push por filtro, at-least-once, sem FIFO?
   → SUBSCRIPTION-oriented (Amazon EventBridge, SNS, Google Eventarc, Azure Event Grid)
```

Depois cruze cada requisito não funcional (latência × throughput × replay × ordering × exactly-once ×
cloud-native × deploy) com as capacidades. Exemplos do livro: e-commerce transacional → RabbitMQ (fila,
baixa latência, at-least-once, sem histórico); notificar parceiros → EventBridge (assinatura, webhook) +
SQS; rastrear frota em tempo real → Kafka (log, streaming, Kafka Streams para agregação, replay). Uma
empresa pode ter **vários brokers** para iniciativas distintas.

### 2. Escolher o padrão de comunicação de mudança de estado (quanto do dado vai no evento)

```txt
Consumidores precisam só saber QUE mudou? (e conseguem viver sem o dado)
   → Notificação de evento (payload = só o id). Leve. Cuidado: chamada extra reintroduz acoplamento.

Consumidores precisam do DADO para agir, sem voltar ao produtor?
   → Event-carried state transfer (payload = dado relevante). Sem chamada extra.
     Cuidado: payload maior, LGPD (não coloque dado sensível), schema vira contrato.

Payload é grande (imagem/áudio/vídeo/documento) e estoura o limite do broker?
   → Claim check (grava no blob storage/S3, evento carrega só a URL/id).

Preciso do HISTÓRICO completo (auditoria, rastreio, reverter)?
   → Event Sourcing (guardar a sequência append-only; estado = replay; snapshots p/ performance).
     Só funciona com event-carried state transfer. Consulta é cara → combine com CQRS.
```

Modele o **mínimo que os consumidores conhecidos precisam**, prevendo o uso — híbrido de notificação +
event-carried. Os padrões coexistem na mesma solução.

### 3. Transactional Outbox + consumidor idempotente (publicar sem dual-write, exactly-once *processing*)

O par que resolve as duas falhas mais comuns de EDA. **Lado do produtor** (nunca "commita e depois
publica"):

```sql
BEGIN;
  INSERT INTO pedido (id, status) VALUES (:id, 'CRIADO');
  INSERT INTO outbox (evt_id, aggregate, tipo, payload, criado_em)
         VALUES (:evt_id, 'pedido', 'PedidoCriado', :json, now());   -- mesma transação, atômico
COMMIT;
-- Relay separado (ou CDC/Debezium lendo a tabela outbox) publica at-least-once e marca como publicado.
```

**Lado do consumidor** (dedupe pelo id do evento, na mesma transação do efeito):

```sql
BEGIN;
  INSERT INTO eventos_processados (evt_id) VALUES (:evt_id)
    ON CONFLICT (evt_id) DO NOTHING;         -- já processado? não faz de novo (checar rowcount)
  -- só aplica o efeito de negócio se a linha acima foi inserida agora
  UPDATE estoque SET qtd = qtd - :n WHERE produto_id = :p;
COMMIT;
-- ack ao broker DEPOIS do commit
```

O relay publica **at-least-once** → o consumidor **precisa** ser idempotente (Receita fecha o ciclo).
Alternativa moderna: apontar CDC direto para a tabela `outbox` (sem relay próprio; sem transformar
schema do BD → schema do evento). Se não implementar outbox/CDC, **defina um processo explícito** para
o caso de falha de publicação (retry após health check no broker).

### 4. Saga entre serviços (coreografada × orquestrada, com compensação)

Transação que cruza serviços = sequência de transações locais, cada passo publicando um evento que
dispara o próximo; em vez de rollback global, cada passo tem uma **compensação**:

```txt
Pedido cria (PENDENTE) ─▶ PedidoCriado
Pagamento cobra ────────▶ PagamentoAprovado   (compensação: estornar)
Estoque reserva ────────▶ EstoqueReservado    (compensação: liberar)
Pedido confirma (PAGO)

Se Estoque falha:  publica ProdutoIndisponivel ▶ Pagamento estorna ▶ Pedido marca CANCELADO
```

**Coreografado** (sem maestro, reage a eventos) → simples/desacoplado, fluxo implícito, difícil rastrear
→ processos simples. **Orquestrado** (controlador dita passos/compensações; pode usar AWS Step Functions /
Azure Durable Functions) → visão clara, testável, ponto de falha → processos complexos. Invariantes: cada
passo e cada compensação **idempotentes**; estado da saga **persistido** (retomar após crash); modele
estados intermediários visíveis (`PENDENTE`); use **eventos de erro específicos** (não genéricos) como
gatilhos de compensação; deixe o **fluxo de compensação direcionar** o quanto paralelizar no caminho feliz.

### 5. Evoluir o schema do evento sem quebrar consumidores

```txt
1. Registre o schema no schema registry (id + versão). Só o id viaja no payload.
2. Toda alteração passa pela verificação de compatibilidade (por subject/grupo):
   - Backward  → remover campo | adicionar campo opcional com default   (consumidor novo lê produtor antigo)
   - Forward   → adicionar campo | remover campo opcional com default    (consumidor antigo lê produtor novo)
   - Full      → interseção das duas | transitivo = checa TODAS as versões anteriores
3. Alteração COMPATÍVEL → um único tópico com várias versões; consumidor não precisa de novo deploy.
4. Alteração que QUEBRA contrato → tópico por versão (pedido-criado-v1/-v2) + versionar as apps;
   aposentar v1 e as apps v1 após a migração dos consumidores.
```

Formatos binários (Avro/ProtoBuf) **exigem** schema; JSON/XML são autodescritivos mas ganham robustez com
JSON Schema/XSD. Prefira JSON para APIs externas; binário para tráfego interno de alto volume.

### 6. Resiliência de consumo: retry → DLQ, visibilidade e ack

```txt
Erro TRANSITÓRIO (rede/infra)      → retry com backoff (produtor reenvia ou consumidor reconsome; self-healing)
Erro PERMANENTE (payload inválido) → não adianta retry → mover para DLQ
Consumidor lento/caiu no meio      → timeout de visibilidade: evento invisível a outros até processar;
                                     estender visibilidade em runtime se precisar de mais tempo
Evento expirou (TTL)               → vai para DLQ
```

Regras: **ack manual** após processar em processamento longo (auto só p/ curto); calibre o **timeout de
visibilidade** (baixo → duplica; alto → atrasa reprocesso em falha); trate a DLQ com **observabilidade dos
erros** e reprocesso (automático de preferência; cada erro deve gerar melhoria na solução, não virar
processo manual eterno). Consumidor idempotente (Receita 3) torna todo retry/reentrega seguro.

## Checklist de pronto (o agente roda antes de finalizar)

- [ ] A escolha **assíncrono (EDA) × síncrono (request-response)** é explícita por parte da solução (forte consistência/resposta imediata → síncrono).
- [ ] O **tipo de broker** (fila/log/assinatura) foi escolhido pelos requisitos, não pela moda; capacidades casadas (deploy, schema, ack, retenção).
- [ ] O **padrão de mudança de estado** foi escolhido (notificação × event-carried × claim check × event sourcing) e não força chamada remota extra desnecessária.
- [ ] Publicação de evento usa **outbox/CDC** — zero dual-write "commita e depois publica"; ou há processo explícito para falha de publicação.
- [ ] Todo consumidor é **idempotente** (dedupe por id do evento na mesma transação do efeito); **ack só após commit**.
- [ ] **Semântica de entrega** escolhida por caso de uso (at-most/at-least/exactly-once); assume-se at-least-once no consumidor.
- [ ] Transação entre serviços é **saga** (não 2PC); cada passo e cada **compensação** são idempotentes; estado da saga **persistido**; eventos de erro **específicos**.
- [ ] **Ordem** garantida por **chave de contexto/partição** onde importa — não FIFO global; ciente de "best-effort ordering".
- [ ] **Schema versionado** em registry; alteração passou por verificação de compatibilidade (backward/forward); quebra de contrato → **tópico por versão** + apps versionadas.
- [ ] Erro tratado: transitório → **retry**; permanente/veneno/expirado → **DLQ** com observabilidade e reprocesso.
- [ ] **Roteamento inteligente** onde há filtragem (evita consumidor receber tudo e descartar); **claim check** para payload grande.
- [ ] **Segurança**: auth (ACL/OPA), TLS/mTLS em trânsito, repouso avaliado, **dados sensíveis fora do schema**; auditoria **monitorada**.
- [ ] **Governança/documentação**: eventos no catálogo; contrato em **AsyncAPI** (+ CloudEvents se interoperabilidade); "The Big Picture" da coreografia.
- [ ] **Observabilidade**: correlation/trace id no header, métricas de lag/acúmulo de fila, visualização da coreografia em runtime.
- [ ] Consistência eventual **assumida e documentada** onde a solução é EDA (leitor pode ver dado stale por um período).

## Armadilhas comuns

- **Dual-write**: gravar no BD e publicar no broker em dois passos — cai no meio e diverge. É a falha nº 1 de EDA. Use outbox/CDC.
- Consumidor **não idempotente** com at-least-once → efeito colateral duplicado (cobra 2×, estoque em dobro).
- Prometer **"exactly-once delivery"** (não existe) em vez de projetar exactly-once *processing* (at-least-once + idempotência).
- **Notificação de evento** que obriga o consumidor a chamada remota de volta ao produtor → reintroduz o acoplamento que a EDA veio eliminar; sobrecarrega e exige o produtor sempre online.
- Colocar **dado sensível** no corpo do evento (event-carried) sem pensar em LGPD/privacidade.
- **Payload grande** (imagem/vídeo) direto no evento → estoura o limite do broker, alta latência e custo. Use claim check.
- Usar **2PC/transação distribuída** entre microsserviços em vez de saga; ou saga **sem compensação idempotente** / **sem persistir estado** → não retoma após crash, compensa em dobro.
- **Evento de erro genérico** → consumidores processam à toa para descobrir se lhes interessa. Use eventos de erro específicos.
- **FIFO global** para garantir ordem de um único contexto → mata throughput e custa caro. Ordene por chave/partição.
- Confiar em **best-effort ordering** como se fosse ordem garantida.
- Mudar schema com campo **obrigatório novo** e quebrar consumidores só em **runtime**. Passe por verificação de compatibilidade; quebra de contrato → versionar tópico.
- Escolher o broker **pela moda** (Kafka para tudo) em vez do requisito; ou ignorar que o broker escolhido **não remove** (log) ou **remove** (fila) após ack.
- **Ack automático** em processamento longo → falha depois do ack = evento perdido silenciosamente.
- **Timeout de visibilidade** mal calibrado: baixo → duplicação clássica; alto → reprocesso lento em falha.
- Consumidores recebem tudo em pub/sub e **descartam** o que não é do seu tipo (desperdício) — falta roteamento inteligente.
- Adotar EDA onde o negócio exige **consistência forte** (saldo logo após saque).
- Tratar **DLQ como processo manual eterno** (não escala, atrasa entrega, expõe dado sensível) em vez de gerar melhoria na solução.
- Ignorar **governança/catálogo/AsyncAPI** no início — a coreografia com centenas de eventos vira impossível de rastrear.
- Não definir o **processo para o caso de erro** de publicação (dual-write) achando que retentativas do broker resolvem tudo.

## Como um projeto aterra isto

Cada projeto define, no seu overlay (ex.: `projetos/<projeto>/skills/…`), **referenciando** esta skill:
- O **broker concreto** (Kafka/RabbitMQ/EventBridge/SQS/Solace…), o **tipo** e o **padrão de deploy**
  (serverless/gerenciado/VM/container/multi-cloud/event mesh) escolhidos por requisito.
- O **formato de payload** (JSON × Avro/ProtoBuf), o **schema registry** usado, os **modos de
  compatibilidade** e a política de versionamento (tópico único × tópico por versão).
- O **catálogo de eventos** reais (nomes no passado, cardinalidade produtor/consumidor) e o contrato em
  **AsyncAPI/CloudEvents**; onde vive o developer portal.
- Como a **idempotência** do consumidor é implementada (tabela de dedupe, chave natural, offset) e a
  **semântica de entrega** por fluxo.
- Se usa **transactional outbox** próprio ou **CDC** (Debezium/Kafka Connect / WAL/binlog), o relay, e o
  **processo para falha de publicação**.
- Como as **sagas** são desenhadas (coreografia × orquestração), onde vivem estado e compensações.
- As políticas de **retry/DLQ/visibilidade/ack/retenção/replay** concretas e os padrões de resiliência.
- A **segurança** concreta (IdP, ACL/OPA, TLS/mTLS, criptografia em repouso) e a política de dado sensível.

**No EprosERP (primeiro overlay real):** o padrão **Transactional Outbox + Domain Events** já vive em
`Epros.Shared` / tabela `Outbox` e está documentado na skill de projeto
`projetos/epros/skills/S08-outbox-domain-events`. Aquele overlay aterra as Receitas 3 e 4 desta skill:
comunicação entre módulos **só por evento** (nunca DbContext cruzado), gravação do evento na `OutboxMessage`
na **mesma transação** da mudança de estado, consumidor idempotente e o fluxo real `VendaFaturada` →
FIN/FISCAL. Ao trabalhar com eventos no Epros, **carregue S08** (código e regras reais) e use esta skill
canônica para o "porquê" e as alternativas (tipo de broker, event-carried × notificação, saga
coreografada × orquestrada, evolução de schema).

## Fontes

- **Arquitetura Orientada a Eventos: soluções escaláveis e em tempo real com EDA** — Casa do Código/Alura,
  2023 (~322 p., 13 caps.). Parte I Fundamentos (evento × comando, componentes, tipos/anatomia/formato/
  protocolo/destino de mensagem, semânticas de entrega + consumidor idempotente; padrões de arquitetura:
  entrega [point-to-point/pub-sub/event streaming/request-reply/fire-and-forget], comunicação de mudança de
  estado [notificação/event-carried/claim check/event sourcing], CQRS, DLQ, FIFO/ordering, SAGA orq×coreo,
  transactional outbox, CDC/log tailing, webhooks, segurança; EventStorming/AsyncAPI/CloudEvents). Parte II
  Broker (papel/deploy/operação/governança/schema registry+compatibilidade, capacidades push-pull/batch/
  prefetch/roteamento/ack/retenção/replay/visibilidade/agendamento, tipos fila/log/assinatura). Parte III
  microsserviços/serverless/streaming/iPaaS+API Management. Parte IV metodologia de execução e escolha de
  broker por caso de uso (RabbitMQ/EventBridge/Kafka). Destilado em `EXTRACOES.md` desta pasta.
- Relacionadas: [[../sistemas-distribuidos]] (a teoria distribuída por trás — idempotência, exactly-once,
  saga, outbox, quórum, ordering, backpressure; caps. de Kleppmann/DDIA); [[../ddd]] (EventStorming,
  bounded context, agregado, linguagem ubíqua); overlay `projetos/epros/skills/S08-outbox-domain-events`
  (aterrissagem real do outbox + domain events no EprosERP).

> Rascunhos de extração acumulam em `EXTRACOES.md` nesta mesma pasta até amadurecerem para cá.
