# 05 — Auditoria de Paridade FISCAL (Epros → EprosERP)

> Auditoria cética, **read-only**. Data: 2026-07-05. Modelo: claude-opus-4-8.
> Legado: `Epros/epros_erp-main` (projeto dedicado `src/Epros.ERP.Dfe.API`, motores `Hercules.NET.NFe.NFCe`, `OpenAC.Net.NFSe`, `FastReport`/`NFe.Danfe.OpenFast`).
> Novo: `EprosERP/src/Modules/Epros.Modules.Fiscal` + motor legado copiado em `src/External/Epros.ERP.DfeCalculos` + controllers em `src/API/Epros.API`.
>
> **Convenção de status**: `PRESENTE E REAL` = transmite/gera de verdade contra SEFAZ/prefeitura ou produz artefato real; `PARCIAL` = orquestração/cálculo/persistência prontos mas transmissão é *fallback honesto* (nunca fabrica chave/protocolo) — falta trocar o adapter; `AUSENTE` = não existe caminho.
> **Código vs Ambiente**: "Ambiente" = certificado A1, homologação SEFAZ, webservice municipal, templates FastReport — coisas que **só o usuário/infra fornece**. "Código" = trabalho de programação que ainda falta neste repositório.

---

## 1. Matriz de cobertura fiscal (resumo executivo)

Legenda: ✅ real · 🟡 parcial (fallback honesto / cálculo só) · ⛔ ausente · `—` não se aplica

| Documento | Emitir | Cancelar | CCe | Inutilizar | DANFE/PDF | Transmissão SEFAZ real |
|-----------|:------:|:--------:|:---:|:----------:|:---------:|:----------------------:|
| **NF-e (55)**  | ✅ | ✅ | ✅ | ✅ | 🟡 (QuestPDF simplificado) | ✅ (Hercules) |
| **NFC-e (65)** | ✅ | ✅ | `—`¹ | ✅ | 🟡 (cupom **sem QR Code**) | ✅ (Hercules) |
| **NFS-e**      | 🟡 | 🟡 | `—` | `—` | 🟡 (fallback) | ⛔ (só `NaoConfigurado`) |
| **CT-e (57)**  | 🟡 | 🟡 | `—` | `—` | ⛔ | ⛔ (só `NaoConfigurado`) |
| **MDF-e (58)** | 🟡² | `—` | `—` | `—` | ⛔ | ⛔ (só `NaoConfigurado`) |

¹ CCe não se aplica a NFC-e por regra de negócio. · ² MDF-e usa "encerrar" em vez de cancelar.

**Leitura rápida**: NF-e/NFC-e têm caminho de transmissão **REAL** (adapter `MotorLegadoFiscalService` → Hercules). NFS-e/CT-e/MDF-e têm **toda a plumbing pronta** (entidade, handler, persistência, controller) mas a transmissão está plugada em serviços `*NaoConfigurado` — trocar a implementação no DI liga o resto.

---

## 2. Gaps que são CÓDIGO (não ambiente) — o que priorizar

Ordenados por severidade. Estes NÃO se resolvem só com certificado/homologação.

| # | Gap de CÓDIGO | Severidade | Onde |
|---|---------------|:----------:|------|
| **G1** | **NFC-e DANFCE sem QR Code**. O cupom (`GerarCupomNfce`) não desenha o QR Code obrigatório por lei da NFC-e nem o hash CSC. Legado gerava via `NFe.Danfe.OpenFast.NFCe`. Falta lib de QR (ex.: QRCoder/ZXing) + montagem da URL de consulta. | **ALTA** | `Infrastructure/Services/DanfeQuestPdfService.cs` (linha ~180, `GerarCupomNfce`) — `Epros.Modules.Fiscal.csproj` só tem QuestPDF, sem lib de barcode |
| **G2** | **Adapter NFS-e real ausente**. Só existe `NfseFiscalServiceNaoConfigurado`. Legado usava `OpenAC.Net.NFSe` (GISS/Ginfes) — biblioteca é código, não ambiente (as *credenciais/município* é que são ambiente). Handlers/entidade/controller já existem. | **ALTA** | `Infrastructure/Services/NfseFiscalServiceNaoConfigurado.cs`; DI em `Program.cs:216` |
| **G3** | **IBPT sem tabela de alíquotas local**. Novo `CalcularIbptValorAproximadoQuery` recebe as alíquotas **como parâmetro de entrada** — não há entidade/tabela IBPT nem sync. Legado tinha tabela IBPT no banco + `POST /ibpts/atualizar` + `obter-aliquotas-por-ncm-uf`. O motor calcula, mas ninguém sabe a alíquota do NCM. | **ALTA** | `Application/Queries/IbptQueries.cs` (só o cálculo, sem lookup); não há entidade `Ibpt`; falta endpoint `obter-aliquotas-por-ncm-uf` e `atualizar` |
| **G4** | **Contingência NFC-e/NF-e não implementada**. Só existe a *config de impressão* de contingência (`ENfceDetalheVendaContingencia`, `SegundaViaContingencia`) e `emContingencia = false` **hardcoded** nas queries. Não há troca de `tpEmis`, assinatura offline, fila de reenvio nem SVC/EPEC. Legado tinha `VendaDocumentoContigencia`/`TipoEmissao`. | **MÉDIA** | `Application/Queries/DocumentoFiscalQueries.cs` (`emContingencia = false` fixo); nenhum uso de `tpEmis`/SVC no `MotorLegadoFiscalService` |
| **G5** | **DANFE NF-e sem código de barras da chave (Code128) e sem QR**. `GerarDanfeNfe` imprime a chave só como texto formatado. DANFE oficial exige código de barras Code128C da chave de 44 dígitos. | **MÉDIA** | `Infrastructure/Services/DanfeQuestPdfService.cs` (`GerarDanfeNfe`) |
| **G6** | **Adapters CT-e/MDF-e reais ausentes** — porém **isto era MOCK/TODO também no legado** (comentário `// TODO: Integrar com Zeus.Net.CTe/Hercules.Net.CTe`). É *paridade*, não regressão. Novo tem entidade+handler+persistência prontos; falta a lib de transporte. | **MÉDIA** (não-regressão) | `Infrastructure/Services/CteMdfeFiscalServiceNaoConfigurado.cs`; DI `Program.cs:217-218` |
| **G7** | **DANFE de pré-visualização (Vendas) é stub**. `DanfeVendaServiceIndisponivel` sempre retorna `null`. Falta o adaptador Vendas→Fiscal (montar `DocumentoFiscal` de rascunho + `IDanfeService`). | **MÉDIA** | `Modules/Vendas/Infrastructure/Services/DanfeVendaServiceIndisponivel.cs`; DI `Program.cs:209` |
| **G8** | **Eventos limitados a Cancelamento + CCe**. `EventoDocumentoFiscal` valida só `"Cancelamento"`/`"CartaCorrecao"`. Sem manifestação do destinatário, EPEC, ator interessado. (Legado **também** só tinha Cancel+CCe — paridade, mas registrado como limitação.) | **BAIXA** (paridade) | `Domain/Entities/EventoDocumentoFiscal.cs:36` |
| **G9** | **Persistência de PDF/XML em storage é local, não MinIO/S3**. `ArmazenamentoArquivoFiscalLocal` grava em disco; há `// TODO(MinIO)` no `MotorLegadoFiscalService`. Funciona, mas não é o storage-alvo. | **BAIXA** | `Infrastructure/Services/ArmazenamentoArquivoFiscalLocal.cs`; DI `Program.cs:205` |

**Gaps que são AMBIENTE (não conte como código faltando):** certificado A1 no cofre + `EmpresaCertificado`; homologação SEFAZ por UF; CSC/Id-token da NFC-e; webservice/credenciais do município (NFS-e); templates `.frx` FastReport (o novo optou por QuestPDF, então NÃO precisa dos `.frx` — decisão de design). NCM/CEST vindos da migração de dados (ver MEMORIA).

---

## 3. Tabela detalhada — Recurso × Legado × Novo

| Recurso fiscal | Legado tem? | Novo status | Código faltando ou Ambiente? | Sev. | Arquivo (novo) |
|----------------|:-----------:|-------------|------------------------------|:----:|----------------|
| **NF-e emitir** (autorização síncrona) | ✅ Hercules | **PRESENTE E REAL** — `NFeAutorizacao` síncrono, cStat 104+protNFe 100, gera `nfeProc` | Ambiente (certificado/homolog.) | Alta | `Infrastructure/Services/MotorLegadoFiscalService.cs` `EmitirAsync` |
| **NF-e cancelar** | ✅ | **PRESENTE E REAL** — `RecepcaoEventoCancelamento`, trata cStat 135/155 | Ambiente | Alta | `MotorLegadoFiscalService.cs` `CancelarAsync` |
| **NF-e carta de correção** | ✅ | **PRESENTE E REAL** — `RecepcaoEventoCartaCorrecao`, cStat 128/135, sequência de evento | Ambiente | Alta | `MotorLegadoFiscalService.cs` `CartaCorrecaoAsync` |
| **NF-e inutilização** | ✅ | **PRESENTE E REAL** — `NfeInutilizacao`, cStat 102 | Ambiente | Alta | `MotorLegadoFiscalService.cs` `InutilizarAsync` |
| **NF-e consulta status serviço** | ✅ | **PRESENTE E REAL** — `NfeStatusServico`, cStat 107 | Ambiente | Média | `MotorLegadoFiscalService.cs` `VerificarStatusServicoAsync` |
| **NF-e consulta protocolo/chave** | ✅ | **PRESENTE E REAL** — `NfeConsultaProtocolo`, cStat 100/101, deriva CNPJ da chave | Ambiente | Média | `MotorLegadoFiscalService.cs` `ConsultarProtocoloAsync` |
| **NF-e cálculo de impostos** (ICMS/IPI/PIS/COFINS/ST/FCP/IBS-CBS) | ✅ | **PRESENTE E REAL** — reusa motor `DfeCalculos` | Ambiente (dados NCM) | Alta | `Infrastructure/Services/MotorLegadoCalculoFiscalService.cs`, `CalculadoraImpostosDocumentoFiscal.cs`, `src/External/.../Impostos/*` |
| **NF-e DANFE PDF** | ✅ FastReport `.frx` | **PARCIAL** — QuestPDF A4 real e legível, marca d'água "SEM VALOR FISCAL"; **sem código de barras Code128 da chave** | **Código** (barcode) | Média | `DanfeQuestPdfService.cs` `GerarDanfeNfe` (**G5**) |
| **NF-e geração/validação XML + assinatura + schema** | ✅ | **PRESENTE E REAL** — `ObterNf` → `ObterXmlString` → `Valida()` (XSD) | Ambiente | Alta | `MotorLegadoFiscalService.cs` (bloco de montagem) |
| **NFC-e emitir** | ✅ | **PRESENTE E REAL** — mesmo adapter, modelo 65 | Ambiente (CSC) | Alta | `MotorLegadoFiscalService.cs` + `HerculesConfiguracaoFactory.cs` |
| **NFC-e cancelar/inutilizar** | ✅ | **PRESENTE E REAL** — mesmo caminho NF-e (modelo 65 em `NfeInutilizacao`) | Ambiente | Alta | `MotorLegadoFiscalService.cs` |
| **NFC-e QR Code no cupom** | ✅ (`NfceLayoutQrCode`/`VersaoQrCode`) | **AUSENTE no PDF** — config existe (entidade `ConfiguracaoImpressaoNfce`), mas o cupom não desenha QR | **Código** | **Alta** | `DanfeQuestPdfService.cs` `GerarCupomNfce` (**G1**) |
| **NFC-e DANFCE cupom 80mm** | ✅ | **PARCIAL** — QuestPDF bobina 80mm real, mas sem QR (ver acima) | Código (QR) | Alta | `DanfeQuestPdfService.cs` `GerarCupomNfce` |
| **Contingência (offline/SVC/EPEC, tpEmis)** | ✅ (`VendaDocumentoContigencia`) | **AUSENTE (emissão)** — só flags de *impressão*; `emContingencia=false` fixo | **Código** | Média | `DocumentoFiscalQueries.cs`, `ConfiguracaoImpressaoNfce.cs` (**G4**) |
| **NFS-e emitir lote** | ✅ OpenAC (GISS/Ginfes) | **PARCIAL** — entidade+handler+persistência prontos; transmissão = `NaoConfigurado` | **Código** (adapter OpenAC) + Ambiente (município) | **Alta** | `NfseHandlers.cs` (real) / `NfseFiscalServiceNaoConfigurado.cs` (stub) (**G2**) |
| **NFS-e consultar lote / por RPS** | ✅ | **PARCIAL** — handler chama serviço; serviço é stub | Código + Ambiente | Média | `NfseHandlers.cs` `ConsultarLote`/`ConsultarPorRps` |
| **NFS-e cancelar** | ✅ | **PARCIAL** — handler real, transmissão stub | Código + Ambiente | Média | `NfseHandlers.cs` `CancelarNfse` |
| **NFS-e DANFSe PDF** | ✅ OpenAC.DANFSe.FastReport | **PARCIAL/stub** — `GerarPdfDeXmlAsync` retorna indisponível | Código + Ambiente | Média | `NfseFiscalServiceNaoConfigurado.cs` |
| **CT-e emitir/cancelar** | ⛔ **MOCK/TODO no legado** | **PARCIAL** — entidade+handler+persistência prontos; transmissão `NaoConfigurado` | Código (lib CT-e) + Ambiente — **paridade, não regressão** | Média | `CteMdfeHandlers.cs` (real) / `CteMdfeFiscalServiceNaoConfigurado.cs` (stub) (**G6**) |
| **MDF-e emitir/encerrar** | ⛔ **MOCK/TODO no legado** | **PARCIAL** — idem CT-e | Código + Ambiente — **paridade** | Média | `CteMdfeHandlers.cs` / `CteMdfeFiscalServiceNaoConfigurado.cs` (**G6**) |
| **IBPT — cálculo valor aproximado** | ✅ | **PARCIAL** — motor calcula, mas **alíquotas vêm do request** (sem lookup) | **Código** | Alta | `IbptQueries.cs`; `src/External/.../Impostos/Ibpts/IbptCalculo.cs` (**G3**) |
| **IBPT — tabela local + sync (`atualizar`, `obter-por-ncm-uf`)** | ✅ (tabela DB + `POST /atualizar`) | **AUSENTE** — sem entidade/tabela IBPT, sem endpoints de lookup/atualização | **Código** (+ dados = Ambiente) | Alta | não existe no módulo (**G3**) |
| **Eventos além de Cancel+CCe** (manifestação, EPEC) | ⛔ (legado só Cancel+CCe) | **AUSENTE** — `EventoDocumentoFiscal` só valida Cancelamento/CCe | Código — **paridade** | Baixa | `EventoDocumentoFiscal.cs:36` (**G8**) |
| **Download XML/PDF por chave, cancelamento, CCe** | ✅ (`NfceNfeController`) | **PRESENTE E REAL** — controller portado com todas as rotas | — | Média | `Controllers/BaixaDocumentoDfeController.cs`, `Queries/BaixaDocumentoDfeQueries.cs` |
| **Download documentos p/ contador; obter por referência mês/ano** | ✅ | **PRESENTE** — rotas portadas | — | Baixa | `BaixaDocumentoDfeController.cs` |
| **DANFE pré-visualização em Vendas (sem autorização)** | ✅ (`gerar-danfe-sem-autorizacao`) | **AUSENTE/stub** — `DanfeVendaServiceIndisponivel` retorna null | **Código** | Média | `DanfeVendaServiceIndisponivel.cs` (**G7**) |
| **Persistência XML/PDF em storage** | ✅ (disco/config) | **PRESENTE (local)** — grava em disco; MinIO/S3 é TODO | Código (MinIO) — não-bloqueante | Baixa | `ArmazenamentoArquivoFiscalLocal.cs` (**G9**) |
| **Fallback honesto (nunca fabrica chave/protocolo)** | n/a | **PRESENTE E REAL** — todos os `NaoConfigurado` retornam `Sucesso=false, StatusSefaz=0` com motivo | — (qualidade) | — | `*NaoConfigurado.cs`, `EmpresaEmitenteFiscalProvider` retorna null controlado |

---

## 4. Wiring de DI (fonte da verdade sobre "real vs fallback")

`src/API/Epros.API/Program.cs` (linhas 186-218):

| Interface | Implementação registrada | Real? |
|-----------|--------------------------|:-----:|
| `ICalculoFiscalService` | `MotorLegadoCalculoFiscalService` | ✅ real (motor legado) |
| `IEmitenteFiscalProvider` | `EmpresaEmitenteFiscalProvider` | ✅ real (resolve Empresa+Certificado; null honesto se faltar) |
| `IHerculesFiscalService` | `MotorLegadoFiscalService` | ✅ real (transmissão NF-e/NFC-e) |
| `IDanfeService` | `DanfeQuestPdfService` | 🟡 real mas simplificado (sem QR/barcode) |
| `IArmazenamentoArquivoFiscal` | `ArmazenamentoArquivoFiscalLocal` | 🟡 real, storage local |
| `IDanfeVendaService` | `DanfeVendaServiceIndisponivel` | ⛔ stub (retorna null) |
| `INfseFiscalService` | `NfseFiscalServiceNaoConfigurado` | ⛔ fallback honesto |
| `ICteFiscalService` | `CteFiscalServiceNaoConfigurado` | ⛔ fallback honesto |
| `IMdfeFiscalService` | `MdfeFiscalServiceNaoConfigurado` | ⛔ fallback honesto |

> **Trocar o adapter no DI é o único passo de código para religar NFS-e/CT-e/MDF-e** (além de escrever o adapter em si). A camada de aplicação/persistência/API já está pronta e aguardando.

---

## 5. Conclusão

- **NF-e e NFC-e**: emissão, cancelamento, CCe, inutilização, consulta status/protocolo e cálculo de impostos estão **PRESENTES E REAIS** via `MotorLegadoFiscalService` + Hercules. O que falta para operar é **ambiente** (certificado A1, homologação SEFAZ, CSC). Não há gap de código nesse fluxo, **exceto DANFE/DANFCE** (QR Code da NFC-e = **G1**, código de barras da NF-e = **G5**).
- **NFS-e**: toda a orquestração existe; falta o **adapter real OpenAC** (**G2**) — código.
- **CT-e / MDF-e**: `NaoConfigurado`, mas isso é **paridade** com o legado (que era MOCK/TODO). Orquestração pronta; falta lib de transporte (**G6**).
- **IBPT**: motor calcula, mas **não há tabela de alíquotas nem lookup** (**G3**) — o legado tinha. Gap de código real.
- **Fallbacks honestos** em todo o módulo: nenhum caminho fabrica chave/protocolo fake — comportamento correto e auditável.

**Prioridade de código (sem depender de ambiente):** G1 (QR NFC-e) → G2 (adapter NFS-e) → G3 (tabela/lookup IBPT) → G4 (contingência) → G5 (barcode DANFE) → G7 (preview DANFE Vendas). G6/G8 são paridade; G9 é otimização de storage.
