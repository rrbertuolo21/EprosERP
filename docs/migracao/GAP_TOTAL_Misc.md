# GAP TOTAL — Dimensão "Relatórios & Misc"

Auditoria de migração Epros (legado) → EprosERP (novo). Objetivo: **nada ficar para trás** antes de aposentar o legado.
Data: 2026-07-04. Método: grep + leitura direta dos dois códigos. Sem especulação.

- Legado: `Epros/epros_erp-main/src`
- Novo: `EprosERP/src`

## Cobertura estimada da dimensão: ~55%

Os pontos de MAIOR risco (subsistema inteiro ausente): **RealTime/SignalR (PDV em tempo real)** e os dois **background pollers de DFe** (`DfeWorkService`, `TimerService`). Sem eles a emissão assíncrona de NFC-e não é finalizada e a importação assíncrona de XML não roda.

---

## 1. GAPS CRÍTICOS (bloqueiam aposentar o legado)

### 1.1 Projeto RealTime / SignalR — AUSENTE POR COMPLETO
Legado: `Epros/epros_erp-main/src/Epros.Erp.RealTime.API` (projeto inteiro).
- `Hubs/VendaHub.cs` — hub SignalR de venda PDV com ~20 métodos invocáveis: `IniciarVenda`, `ReconectarVenda`, `DefinirDocumentoDfe`, `DefinirEmitente`, `DefinirDestinatario`, `AdicionarProduto(s)`, `AtualizarProduto`, `RemoverProduto`, `DefinirValoresVenda`, `DefinirValoresEmbutir`, `DefinirFrete`, `DefinirIndicadorPresencaIntermediador`, `DefinirEntrega`, `DefinirIntermediador`, `DefinirValoresVenda`, `FinalizarVenda`, `CancelarVenda`, `ObterVendaCompletaParaNfe`.
- `Services/VendaSignalRService.cs` — máquina de estado de venda em memória (`ConcurrentDictionary` de sessões + mapa connectionId→sessionId), reconexão por sessionId.
- `Services/VendaLimpezaService.cs` — `BackgroundService` que limpa sessões antigas a cada 1h (`LimparSessoesAntigas`).
- `Services/VendaSignalRWrapper.cs` — wrapper do agregado Venda para sessão.
- `AsyncApi/` — geração de contrato AsyncAPI + viewer (`AsyncApiDocumentFactory`, `AsyncApiDocumentSerializer`, `AsyncApiViewerPage.html`, `HubContractGenerator`).
- `Attributes/` — `SignalRContractAttribute`, `SignalREventAttribute`, `SignalRInvokeAttribute` (metadados p/ gerar o AsyncAPI).
- `Controllers/VendaController.cs`, `Dtos/Vendas/VendaSignalRDto.cs`, `Interfaces/IVendaSignalRService.cs`.

Prova de ausência no novo: `grep -rli "signalr|ihubcontext|hub"` em `EprosERP/src` → **0 resultados**; nenhum `.csproj` referencia `Microsoft.AspNetCore.SignalR`; nenhum `*RealTime*`/`*SignalR*` no filesystem.

> Impacto: o PDV em tempo real (montagem de venda incremental com reconexão) não existe no novo. Se o front consome esse hub, quebra 100%.

### 1.2 DfeWorkService (finalização assíncrona de NFC-e) — AUSENTE
Legado: `Epros/epros_erp-main/src/Epros.ERP.API/ServicesWork/DfeWorkService.cs` (`IHostedService`, Timer a cada 2 min).
Função: varre `Vendas` com `Nfce.StatusInterno == Recebido`, baixa o XML pela API DFe externa (`BaixaXmlPorDocumentoLocalizadorExternoIdHelper.Baixar`), finaliza a venda (`VendaFinalizarService.FinalizarNfAutorizadaAsync`), gera contas a receber (`ContasAReceberService.GerarContasAReceber`) e trata timeout/rejeição mantendo `Recebido` para retry.

Prova de ausência: `grep -rli "IHostedService|BackgroundService"` no novo só acha os `*OutboxProcessorJob` (Quartz) por módulo. Esses jobs **apenas despacham eventos de domínio do outbox** (ex.: `VendasOutboxProcessorJob` filtra `EventType == "VendaFaturada"/"VendaCancelada"`). `grep "FinalizarNfAutorizada|BaixaXmlPorDocumento|FinalizarVendaAutorizada"` no novo → **0 resultados**.

> Impacto: vendas NFC-e emitidas de forma assíncrona ficam presas em "Recebido" para sempre; contas a receber não são geradas.

### 1.3 TimerService (importação assíncrona de XML) — AUSENTE
Legado: `Epros/epros_erp-main/src/Epros.ERP.API/Services/TimerService.cs` (`IHostedService`, Timer a cada 30s).
Função: `IImportacaoXmlService.SalvarEntities()` + `SalvarPdf()` — persiste em background as entidades derivadas do XML importado e gera os PDFs.

Prova de ausência: `grep "TimerService|Timer("` no novo → **0**. O novo tem `ImportarCompraXmlCommandHandler` (síncrono, no request), sem o loop de background que processa a fila. Não há equivalente de `SalvarPdf` agendado.

---

## 2. GAPS DE MÉDIA GRAVIDADE

### 2.1 Relatório de Vendas em XLSX — AUSENTE
Legado:
- `Epros/epros_erp-main/src/Epros.ERP.API/Controllers/V1/Reports/VendaReportsController.cs` → rota `GET api/v1/relatorios/vendas/simplificado01` (por período + status).
- `Epros/epros_erp-main/src/Epros.ERP.API/Reports/Vendas/VendaRelSimplificado.cs` → gera planilha XLSX via **NPOI** (colunas: Data, Número, Destinatário, Valor, Modelo, Status, Chave).

Prova de ausência no novo: `grep -rli "NPOI|XSSFWorkbook|IWorkbook"` → **0**; `grep "relatorios/vendas|simplificado01|VendaRelSimplificado|VendaReports"` → **0**. Módulo Vendas do novo não tem pasta/arquivo `*Report*`/`*Relatorio*`. (Os `RelatorioESG` existentes são de outro domínio.)

> Único controller/report sob `Reports/` no legado. É a "dimensão relatórios" propriamente dita e está 0% portada.

### 2.2 Envio real de e-mail (SMTP) — SÓ MOCK no novo
Legado: `Epros/epros_erp-main/src/Epros.ERP.API/Services/EmailService.cs` — `SmtpClient`/`MailMessage` reais; métodos `EnviarEmailAsync`, `EnviarNovaSenha` (e-mail de recuperação de senha).
Novo: `EprosERP/src/Infrastructure/Epros.Infrastructure/Services/MockNotificacaoService.cs` — `INotificacaoService.EnviarEmailAsync` **só faz Console.WriteLine/log**; não há `SmtpClient` real (`grep "SmtpClient|MailMessage"` no novo → só um handler de certificado, nada de envio de e-mail transacional).

> Fluxo de recuperação de senha existe (`AccountController.GerarNovaSenha`, `AuthController.RecuperarSenha`) mas o e-mail **não sai** — precisa de implementação SMTP real antes do corte.

### 2.3 Integração MercadoPago — webhook parcial, sem chamada real à API MP
Legado: referências em `EmpresaController`, `EmpresaDto`, `EmpresaMapping` (dados de conta MP no cadastro).
Novo: `ProcessarWebhookPagamentoCommandHandler` valida assinatura (X-Signature) e trata `payment.created/updated`, `PagamentoGlobal`. Porém `grep "api.mercadopago|MercadoPago.Client|mercadopago.com|access_token"` no código do novo → **0 chamadas HTTP reais ao MercadoPago** (não busca o status do pagamento na API MP; assume o payload). Validar se o legado faz chamada de consulta ao MP e portar se necessário.

---

## 3. VERIFICADO — JÁ COBERTO (não é gap)

| Feature legada | Onde no novo | Nota |
|---|---|---|
| CnpjOnline (consulta CNPJ) | `Modules/GestaoClientes/Application/Queries/ConsultarCnpjOnlineQuery.cs` + `Controllers/CnpjOnlineController.cs` | Porte fiel; HTTP real ReceitaWS/BrasilAPI, desligável por config. Melhor que o legado. |
| IBPT (alíquotas aproximadas) | `Modules/Fiscal/Application/Queries/IbptQueries.cs`, `External/DfeCalculos/Impostos/CalculoIbpt.cs`, `IbptDfeController.cs` | Coberto. Confirmar se o cálculo por NCM/UF está completo. |
| Importação OFX (parse extrato) | `Modules/Financeiro/.../BancoAndContaBancariaAndCartaoHandlers.cs` (`ProcessarExtratoOfxCommand`) | Parseia `<STMTTRN>/<DTPOSTED>/<TRNAMT>` via Regex. **Difere do legado**: novo recebe conteúdo por JSON (`OfxConteudo`), legado recebe `IFormFile` e usa `OFXParser.Parser.GenerateExtract`. Ver §4. |
| Importação de XML (compra/entrada) | `Modules/Estoque/.../ImportarCompraXmlCommandHandler.cs`, `NfeXmlParser.cs`, `ImportacoesXmlController.cs` | Coberto (parte síncrona). Falta o loop de background (§1.3). |
| Balança (produto pesável) | `Controllers/BalancasController.cs`, `Modules/Estoque/.../Balanca*`, `Domain/Entities/Balanca.cs` | Coberto. |
| BaixaDocumento DFe / distribuição | `Controllers/BaixaDocumentoDfeController.cs`, `DfeCalculos/Dtos/V1/BaixaDocumentoContadorDto.cs` | Coberto; DTO do contador (período emitente/destinatário, `IncluiPdfs`) presente. |
| Hash de senha | `Infrastructure/Services/Pbkdf2PasswordHasher.cs` (`IPasswordHasher`) | Substitui `Security.Sha256`. Ver §4 sobre `Security.Encrypt/Decrypt`. |
| Cálculo de reajuste de produto | `Controllers/ProdutosHistoricosReajustesController.cs` + `ETipoOperacaoReajuste` | Coberto (equivalente a `ProdutoHelper.CalcularReajuste`). |
| DANFE (frx FastReport) | `Modules/Fiscal/Infrastructure/Services/DanfeQuestPdfService.cs` (QuestPDF) | **Substituído** por QuestPDF (não porta os `.frx`). `MotorLegadoFiscalService` tem `TODO(DANFE)` — validar paridade visual/legal antes do corte. |

---

## 4. HELPERS/DETALHES A CONFIRMAR (baixa gravidade, mas listar p/ não esquecer)

- **`Security.Encrypt` / `Security.Decrypt`** (AES do legado, `Helpers/Security.cs`): o novo tem `Pbkdf2PasswordHasher` (senha) e `VaultEncryptionService` (cofre). Confirmar que **todo dado cifrado com `Security.Encrypt` no banco legado** (ex.: segredos/tokens) é decifrável/migrado pelo `VaultEncryptionService` — algoritmos podem divergir. Risco de dados legados ilegíveis pós-corte.
- **`OFXParser` (parser SGML completo)**: `Epros/.../External/OFXParser/` (Parser + Entities Bank/BankAccount/Extract/Transaction) trata header SGML, encoding e datas. O novo usa Regex simples em 2 tags. Confirmar que arquivos OFX reais dos clientes (bancos variados) são parseados igual; e que o **upload de arquivo** (IFormFile) existe no novo, não só envio de string.
- **`XmlHelper.ObterTipoNf` / `EmitenteIsValid`**: validação de tipo de NF e do emitente no XML. O novo `NfeXmlParser.Parse` cobre parse, mas confirmar detecção de tipo e validação de emitente (anti-troca de XML).
- **`TextoXmlHelper.LimparTextoXml`**, **`ByteArrayHelper.ToByteArray`**: utilitários triviais; confirmar equivalentes ou inline no novo.
- **`IbptAliquotaHelper.BuscaAliquotasNcmUf`**: chamada HTTP à API DFe (`/ibpts/obter-aliquotas-por-ncm-uf`). Confirmar que o novo `CalcularIbptValorAproximadoQuery` cobre a busca por NCM/UF (não só cálculo de valor aproximado).

---

## 5. RESUMO EXECUTIVO — O QUE FALTA PORTAR

| # | Item | Gravidade | Status |
|---|---|---|---|
| 1 | Projeto SignalR/RealTime (VendaHub + 20 métodos + sessões + AsyncAPI + limpeza) | CRÍTICO | Ausente 100% |
| 2 | `DfeWorkService` (finaliza NFC-e assíncrona + gera contas a receber) | CRÍTICO | Ausente |
| 3 | `TimerService` (importação XML assíncrona: SalvarEntities + SalvarPdf) | CRÍTICO | Ausente |
| 4 | Relatório de Vendas XLSX (`simplificado01` / NPOI) | MÉDIA | Ausente |
| 5 | Envio real de e-mail SMTP (recuperação de senha etc.) | MÉDIA | Só Mock |
| 6 | MercadoPago: chamada real à API de pagamento | MÉDIA | Webhook parcial |
| 7 | `Security.Encrypt/Decrypt` (compat. de dados cifrados legados) | A CONFIRMAR | Divergente |
| 8 | OFX: parser SGML robusto + upload de arquivo | A CONFIRMAR | Simplificado |
| 9 | `XmlHelper.ObterTipoNf/EmitenteIsValid` | A CONFIRMAR | Parcial |
| 10 | `IbptAliquotaHelper.BuscaAliquotasNcmUf` (busca NCM/UF) | A CONFIRMAR | Provável parcial |
