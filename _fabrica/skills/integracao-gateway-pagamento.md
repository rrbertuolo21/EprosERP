---
name: integracao-gateway-pagamento
categoria: backend-web
tags: [pagamento, gateway, pix, cartao, boleto, webhook, idempotencia, conciliacao, multi-tenant, cofre-de-segredos, hmac, fail-closed, mercadopago, checkout]
nivel: avancado
aplica-se-a: [backend, api, saas, multi-tenant, qualquer-linguagem]
fontes:
  - "Implementação real EprosERP — Modules/Epros.Modules.GestaoClientes (IPaymentGateway, MercadoPagoGateway, ConfiguracaoGatewayPagamento) — destilada para o núcleo agnóstico"
  - "Documentação de provedores (Mercado Pago Payments API v1: X-Idempotency-Key, webhooks/notification_url, point_of_interaction/transaction_data)"
status: v1-semente
revisao: semestral
---

# Integração de Gateway de Pagamento

> **Conhecimento agnóstico** — vale para qualquer provedor (Mercado Pago, Stripe, PagSeguro,
> Asaas, Cielo…) e qualquer linguagem. O provedor concreto, o schema do banco, o cofre de
> segredos e a stack HTTP são o *aterramento* de cada projeto — ver "Como um projeto aterra isto".

## Quando usar

Ative quando a tarefa envolver: **cobrar um cliente dentro do produto (PIX, cartão, boleto),
integrar um provedor de pagamento, gerar cobrança e QR Code/copia-e-cola, receber e validar
webhook de pagamento, dar baixa/cancelar/estornar fatura, conciliar status com o provedor,
guardar credencial de gateway (access token / webhook secret), ou configurar cobrança por tenant
num SaaS**. Dinheiro está em jogo: erro aqui é cobrança duplicada, baixa indevida ou credencial
vazada — trate como código sensível (par com o Security Agent).

## Princípios

**O provedor é a fonte da verdade do dinheiro, não o seu banco.** Nunca "simule" um pagamento
mudando o status da fatura na mão. A cobrança nasce OUTBOUND no provedor (você recebe um
`PaymentId`), e a baixa vem INBOUND do provedor (webhook) ou de uma consulta ao provedor
(conciliação). O seu registro é um espelho; a autoridade é sempre a resposta assinada do gateway.

**Abstraia o provedor atrás de uma porta.** O domínio fala com `IPaymentGateway`, não com
"MercadoPago". Trocar de provedor ou suportar dois ao mesmo tempo não pode tocar regra de
negócio. Adaptador concreto por provedor; contrato único.

**Idempotência não é opcional.** Toda criação de cobrança carrega uma chave de idempotência
(`X-Idempotency-Key` ou equivalente). Sem ela, um retry de rede vira segunda cobrança do mesmo
cliente. Webhooks também chegam repetidos: processar o mesmo evento duas vezes não pode dar
baixa/estorno em dobro.

**Segredo é do cofre, cifrado, e nunca sai em texto.** Access token e webhook secret não moram
em `appsettings`/`.env` global nem em texto no banco: ficam cifrados (ciphertext do cofre) e só
são descriptografados no instante da chamada. No GET da configuração, o token volta **mascarado**
(`••••1234`); o webhook secret só reporta "existe/não existe".

**Fail-closed no webhook.** Sem assinatura válida, o webhook é rejeitado — nunca "processa mesmo
assim por precaução". Endpoint público + dar baixa em fatura = alvo. Assinatura ausente/ruim →
`401`, ponto.

**Máquina de estados completa, não só "pago".** O provedor emite aprovado, pendente, em análise,
rejeitado, cancelado, estornado, expirado, chargeback. Tratar só a baixa deixa faturas presas e
estornos invisíveis. Mapeie o estado do provedor → estado da sua fatura de forma explícita.

## Receitas

### 1. Abstração `IPaymentGateway` + adaptador concreto

Uma porta no domínio/aplicação; um adaptador por provedor na infraestrutura. O contrato mínimo:

```txt
interface IPaymentGateway {
  GerarCobrancaPix(fatura, config, pagador) -> { PaymentId, QrCode, QrCodeBase64, TicketUrl, DataExpiracao, Status }
  ConsultarPagamento(paymentId, config)     -> { PaymentId, Status, ValorTransacao, ValorTarifa, DataAprovacao }
  TestarConexao(config)                     -> ok | falha   // GET leve p/ validar credencial
}
```

Regras do adaptador:
- **`IHttpClientFactory` / client nomeado**, nunca `new HttpClient()` por chamada (esgota sockets).
- **BaseAddress + timeout** configurados; toda exceção de rede/timeout vira resultado tipado de
  falha, não estoura para o chamador.
- **Idempotência** no POST de criação: `X-Idempotency-Key = {faturaId}:{guid}` (chave estável por
  tentativa lógica).
- **Autenticação** montada por request (`Authorization: Bearer <token descriptografado>`); o token
  vem do cofre, na hora, e nunca é logado.
- **Parsing defensivo** do JSON do provedor: campos podem faltar; leia com `TryGetProperty` e
  devolva erro claro se o `PaymentId` não veio.
- Retorne um resultado uniforme (`Ok(dto)` / `Falha(mensagem)`), não `null` nem exceção crua.

```txt
❌ new HttpClient(); status "pago" gravado na mão; token em appsettings; retry sem chave
✅ IHttpClientFactory + X-Idempotency-Key + token do cofre + PaymentId persistido do provedor
```

### 2. Entidade de configuração do gateway (credencial no banco, cifrada)

Uma entidade de configuração — não uma credencial global hard-coded. Campos:

| Campo | Papel |
|---|---|
| `Provedor` | enum do provedor (MercadoPago, Stripe…) |
| `Ambiente` | `Sandbox` \| `Producao` |
| `AccessToken` | **cifrado no cofre** (ciphertext) |
| `PublicKey` | pública (não sigilosa) |
| `WebhookSecret` | **cifrado no cofre** (ciphertext) |
| `Moeda` | ISO 4217, ex.: `BRL` |
| `NotificationUrl` | URL do seu webhook, enviada ao provedor |
| `TenantAlvo` | **`null` = config global** / preenchido = override daquele tenant |
| `Ativo` | liga/desliga sem apagar |

Detalhes que evitam bug:
- **A cifragem é do handler, não da entidade.** O handler recebe o segredo em texto, chama
  `cofre.Criptografar(...)` e passa o *ciphertext* ao construtor. A entidade só guarda ciphertext.
- **Editar sem reenviar o segredo:** no update, se `accessToken`/`webhookSecret` vierem vazios,
  **preserve** o valor atual (não sobrescreva com vazio). Só recifra quando um novo segredo é enviado.
- **Entidade global (multi-tenant):** marque-a como "global" para escapar do filtro automático por
  tenant — a config global (`TenantAlvo == null`) convive com overrides por tenant no mesmo conjunto.
- **Nunca exponha o token no GET:** o DTO de leitura devolve `AccessTokenMascarado` (`••••1234`) e
  um booleano `PossuiWebhookSecret` — jamais o valor.

### 3. Fluxo de cobrança (OUTBOUND) — gerar o PIX de verdade

Botão "Gerar Pix" na fatura → comando → `GerarCobrancaPix`. O que persistir do retorno do provedor:
`PaymentId`, `QrCode` (copia-e-cola), `QrCodeBase64` (imagem do QR), `TicketUrl`, `DataExpiracao`,
`Status` inicial. O front mostra o QR e o botão copiar. Corpo típico do POST (PIX): valor,
`payment_method_id = "pix"`, `external_reference = faturaId` (o elo para reconciliar o webhook
depois), expiração (ex.: +30 min), dados do pagador (email; CPF/CNPJ desmascarado), e a
`notification_url`. **Nunca** marque a fatura como paga aqui — ela continua "aguardando pagamento"
até o webhook/conciliação confirmar.

### 4. Webhook (INBOUND) com assinatura validada e fail-closed

Endpoint público que o provedor chama quando o status muda. Ordem obrigatória:

1. **Validar a assinatura ANTES de ler o corpo como verdade** — HMAC-SHA256 sobre o payload (ou o
   esquema do provedor: MP usa `x-signature` + `x-request-id` + `data.id`) com o `WebhookSecret`
   descriptografado do cofre. Comparação em tempo constante. Assinatura inválida/ausente → `401`,
   **fail-closed** (não processa).
2. **Idempotência do evento:** guarde os IDs de evento já processados; reentrega do mesmo evento é
   no-op.
3. **Buscar a config e consultar o provedor** pelo `PaymentId` (não confie só no corpo do webhook —
   o corpo diz "algo mudou"; a fonte da verdade é a consulta autenticada).
4. **Aplicar a máquina de estados** (receita 4b) e persistir de forma transacional.
5. Responder `200` rápido; trabalho pesado vai para fila/async se necessário (o provedor faz retry
   se você demorar/errar).

**4b — Máquina de estados (não só baixa):**

| Status do provedor | Ação na fatura |
|---|---|
| `approved` / pago | dar **baixa** (paga, registrar valor líquido = valor − tarifa, data de aprovação) |
| `pending` / `in_process` | **em análise** — aguarda, não baixa |
| `rejected` | marcar **rejeitado**, manter em aberto p/ nova tentativa |
| `cancelled` | **cancelar** a cobrança |
| `refunded` / `charged_back` | **estorno/chargeback** — reverter a baixa, sinalizar financeiro |
| expirou (sem pagamento até `DataExpiracao`) | marcar **expirada**, permitir gerar nova |

Estorno e chargeback tocam regra financeira: sinalize para validação humana e registre trilha.

### 5. Conciliação (fallback do webhook)

Webhook não é garantido: cai, é bloqueado por firewall, chega fora de ordem. Tenha um **job de
polling** que periodicamente pega faturas em aberto/em análise com `PaymentId` e chama
`ConsultarPagamento` no provedor, aplicando a mesma máquina de estados da receita 4b. É a rede de
segurança: se o webhook falhou, a conciliação fecha a fatura. Idempotência aqui também — webhook e
job podem processar o mesmo pagamento; o resultado tem de ser o mesmo.

### 6. Resolução da configuração (tenant → global)

Ao cobrar, resolva a config na ordem: **override do tenant** (`TenantAlvo == tenantId` e `Ativo`),
senão **config global** (`TenantAlvo == null` e `Ativo`). Nenhuma das duas → **erro claro e
acionável**: "Gateway de pagamento não configurado" (não `NullReferenceException`, não "erro
interno"). O mesmo caminho de resolução vale para gerar cobrança, consultar e conciliar.

### 7. Segurança (checklist mínimo, sempre)

- Segredos **cifrados no cofre**, descriptografados só no instante da chamada.
- Token **nunca logado, nunca no GET** (mascarado), nunca em URL/query string.
- Webhook **fail-closed** com assinatura validada em tempo constante.
- `Sandbox` isolado de `Producao` — credenciais e URLs distintas por ambiente.
- Trilha de auditoria em baixa/cancel/estorno (quem/quando/qual PaymentId).
- Falha do cofre ao ler o token → erro tratado ("não foi possível ler o access token"), nunca
  seguir com token vazio.

## Armadilhas comuns

- **Marcar como pago sem confirmação do provedor.** O clássico. A baixa só existe com evidência
  do gateway (webhook validado ou consulta).
- **POST de cobrança sem chave de idempotência.** Um timeout + retry = duas cobranças. Sempre a chave.
- **Webhook que confia no corpo sem validar assinatura.** Qualquer um posta `{"status":"approved"}`
  no seu endpoint e "paga" a fatura. Fail-closed, sempre.
- **Tratar só `approved`.** Faturas ficam presas em "em análise"; estornos somem; expirados nunca
  liberam nova cobrança.
- **Token em `appsettings`/`.env` ou em texto no banco.** Vira credencial vazada em log, dump ou tela.
- **Sobrescrever o segredo com vazio no update.** Editar a moeda apaga o access token. Preserve
  quando não reenviado.
- **`new HttpClient()` por chamada.** Esgota portas sob carga. `IHttpClientFactory`.
- **Confiar só no webhook.** Sem conciliação, um webhook perdido = fatura paga eternamente em aberto.
- **Não distinguir tenant de global.** Cobra com a credencial errada ou não acha nenhuma.

## Checklist de integração de pagamento

**Contrato e adaptador**
- [ ] `IPaymentGateway` com `GerarCobrancaPix` / `ConsultarPagamento` / `TestarConexao`.
- [ ] Adaptador concreto por provedor, na infraestrutura, sem vazar tipos do provedor para o domínio.
- [ ] `IHttpClientFactory` (client nomeado), BaseAddress + timeout, exceções viram falha tipada.
- [ ] `X-Idempotency-Key` (ou equivalente) em toda criação de cobrança.

**Configuração**
- [ ] Entidade de config: provedor, ambiente, tokens cifrados, moeda, notificationUrl, `TenantAlvo`, `Ativo`.
- [ ] Cifragem no handler; entidade guarda só ciphertext.
- [ ] Update preserva segredo quando não reenviado.
- [ ] GET mascara o token (`••••1234`) e só reporta existência do webhook secret.
- [ ] Config marcada como global para conviver com overrides por tenant.

**Cobrança (outbound)**
- [ ] Botão "Gerar Pix" → persiste `PaymentId`, QR, copia-e-cola, expiração.
- [ ] Fatura NÃO é marcada paga na geração.
- [ ] `external_reference = faturaId` para reconciliar depois.

**Webhook (inbound)**
- [ ] Assinatura validada (HMAC/`x-signature`) em tempo constante, ANTES de agir.
- [ ] Fail-closed: assinatura inválida/ausente → `401`, sem processar.
- [ ] Idempotência de evento (reentrega = no-op).
- [ ] Consulta o provedor pelo `PaymentId` (não confia só no corpo).
- [ ] Máquina de estados completa: baixa, pendente/análise, rejeitado, cancel, estorno/chargeback, expiração.
- [ ] Estorno/chargeback sinalizados para validação humana + trilha.

**Conciliação**
- [ ] Job de polling consulta status de faturas em aberto/análise (fallback do webhook).
- [ ] Mesma máquina de estados; idempotente com o webhook.

**Resolução e segurança**
- [ ] Resolução tenant → global; erro claro "gateway não configurado".
- [ ] Segredos no cofre; token nunca logado/exposto/em URL.
- [ ] Sandbox isolado de Produção.
- [ ] Falha do cofre é tratada, nunca segue com token vazio.

## Como um projeto aterra isto

O *seam* que cada projeto preenche:
- **Provedor concreto e o mapeamento de status.** Cada provedor tem seu JSON, seus nomes de status
  e seu esquema de assinatura de webhook — o adaptador e a máquina de estados são específicos.
- **O cofre de segredos.** O projeto escolhe o serviço (`ISegredoCofreService`, KMS, Vault, Data
  Protection) que cifra/decifra os tokens.
- **O modelo de fatura e seus estados.** "Baixa", "estorno", "expirada" são estados do domínio do
  projeto; esta skill diz *quando* transitar, não *como* a fatura é modelada.
- **A stack HTTP e o multi-tenant.** `IHttpClientFactory` (ou equivalente da linguagem) e o
  mecanismo de "entidade global" que faz a config escapar do filtro por tenant.
- **Regra financeira e fiscal** (tarifa, valor líquido, tratamento contábil de estorno) vem de
  `Negocio-acumulado/financeiro` via o Especialista de Negócio — não invente aqui.

O overlay do EprosERP materializa este padrão em `Modules/Epros.Modules.GestaoClientes`
(`IPaymentGateway`, `MercadoPagoGateway`, `ConfiguracaoGatewayPagamento`) — referência de
implementação real desta skill.

## Fontes

- Implementação real EprosERP — `Epros.Modules.GestaoClientes` (IPaymentGateway, MercadoPagoGateway,
  ConfiguracaoGatewayPagamento, handlers de máscara/cifragem, resolução tenant→global).
- Documentação de provedores de pagamento (padrões de idempotência, webhooks assinados, PIX/QR).

> Rascunhos de extração acumulam em `EXTRACOES.md` nesta mesma pasta até amadurecerem para cá.
