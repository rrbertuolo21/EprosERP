# GAP TOTAL — Fiscal / DFe (Epros legado → EprosERP)

> Auditoria de migração — dimensão **Fiscal-DFe além de NF-e/NFC-e**.
> Objetivo: nada ficar para trás antes de aposentar o legado.
> Data: 2026-07-04. Evidências por grep/leitura (não especulação).

Legado: `Epros/epros_erp-main/src/Epros.ERP.Dfe.API`
Novo: `EprosERP/src/Modules/Epros.Modules.Fiscal` (+ `src/API/Epros.API/Controllers`)

---

## Resumo executivo

O que **já foi portado e é real** (motor Hercules.NET, models 55/65):
- **NF-e (55)** e **NFC-e (65)**: emissão, cancelamento, CC-e, inutilização — `MotorLegadoFiscalService.cs` (378 linhas) + handlers CQRS.
- **DANFE / DANFCE**: gerados via **QuestPDF** (`DanfeQuestPdfService.cs`), NÃO via FastReport.
- **IBPT**: cálculo portado em `External/Epros.ERP.DfeCalculos/Impostos/CalculoIbpt.cs` + `IbptDfeController`.

O que **falta portar** (existe/existia no legado, ausente no novo):
1. **NFS-e** — implementação legada COMPLETA (OpenNFSe, provedores Ginfes/GISS, DANFSe) → **ZERO no novo**.
2. **CT-e** — só existia STUB mock no legado; **não portado**; precisa implementação real.
3. **MDF-e** — idem CT-e (stub mock legado) + schemas XSD existem; **não portado**.
4. **Templates FastReport (.frx)** — existem no legado (NFe/NFCe/CC-e/DANFSe); **não migrados** (novo usa QuestPDF). Decisão de arquitetura a confirmar.
5. **Contingência** — app desktop legado `Epros.PDV.Contingencia.Desktop.Data2.0`; sem equivalente no novo.
6. **Distribuição DFe / Manifestação do destinatário** — **NÃO existe nem no legado**; ausente também no novo (gap de produto, não de migração).

**Cobertura estimada da dimensão Fiscal-DFe: ~40%.**
(NF-e/NFC-e completos = peso alto já feito; NFS-e/CT-e/MDF-e/contingência = a maior parte do trabalho restante.)

---

## 1. NFS-e (Nota Fiscal de Serviço eletrônica) — FALTA TUDO

### Legado (existe e é robusto)
| Arquivo | Linhas | Papel |
|---|---|---|
| `Epros.ERP.Dfe.API/NFSe/Services/NfseService.cs` | 643 | Orquestração emissão/consulta/cancelamento lote |
| `Epros.ERP.Dfe.API/NFSe/Services/NfseProviderFactory.cs` | — | Fábrica de provedor → `OpenNFSe` (lib OpenAC) |
| `Epros.ERP.Dfe.API/Services/VendaNfseService.cs` | 341 | Serviço de venda→NFS-e |
| `Epros.ERP.Dfe.API/Controllers/V1/NfseController.cs` | — | 6 endpoints: `config`, `emitir-lote`, `consultar-lote`, `consultar-por-rps`, `cancelar`, `gerar-pdf-xml` |
| `NFSe/Helpers/*` | 8 helpers | DANFSe PDF/assets, IBS/CBS reader, enriquecimento e validação de XML, encoding, mapper Ginfes |
| `NFSe/Settings/NfseSettings.cs` | — | Configuração por provedor/município |
| `Schemas/Ginfes`, `Schemas/GISS` | — | XSDs dos provedores |
| `Entities/Nfse/*` | — | Entidades de persistência NFS-e |
| `Interfaces/INfseService.cs`, `IVendaNfseService.cs` | — | Contratos |

Provedores suportados no legado: **OpenNFSe (OpenAC)** com esquemas **Ginfes** e **GISS**; helper `NfseOpenAcMunicipioHelper.cs` resolve município. Suporte a **IBS/CBS** já presente (`NfseIbsCbsXmlReader.cs`) — reforma tributária.

### Novo (EprosERP)
- **Nenhum** controller, serviço, entidade ou handler de NFS-e.
- Única referência: `External/Epros.ERP.DfeCalculos/Impostos/NFses/` — apenas **cálculo de ISS** (CalculaIss*), sem emissão/transmissão.
- `MotorLegadoFiscalService.cs` só instancia `HerculesNfeClasse` (NF-e/NFC-e) — não há caminho NFS-e.

### O que falta portar
- [ ] Entidades NFS-e no `Domain` (ou reuso de `DocumentoFiscal` com modelo dedicado).
- [ ] Serviço de emissão/transmissão NFS-e (OpenNFSe ou equivalente).
- [ ] Factory de provedor + configuração por município (Ginfes/GISS + genéricos ABRASF).
- [ ] Helpers de XML (enricher, schema validator, encoding, IBS/CBS reader).
- [ ] Geração DANFSe (PDF) — ver seção FastReport.
- [ ] Controller com os 6 endpoints (emitir-lote, consultar-lote, consultar-por-rps, cancelar, config, gerar-pdf-xml).
- [ ] Persistência + estados da NFS-e (RPS → lote → protocolo).

---

## 2. CT-e (Conhecimento de Transporte) — STUB no legado, FALTA implementação real

### Legado
- `Controllers/V1/CteController.cs` (91 linhas) é **MOCK**:
  - Linha 41: `// TODO: Integrar com a biblioteca Zeus.Net.CTe / Hercules.Net.CTe`
  - Retorna chave/protocolo/status **falsos** (`mockRetorno`).
  - Endpoints: `POST emitir`, `POST cancelar/{chave}`.
- **Não há** `CteService`, entidade, schemas ou mapeamento real.

### Novo
- Ausente por completo (nenhum controller/serviço/entidade CT-e).

### O que falta portar/implementar (não é "port", é "construir")
- [ ] Integração real Hercules.NET/Zeus para CT-e (o legado nunca teve).
- [ ] Modelagem CT-e (modais rodoviário etc.), assinatura, transmissão SEFAZ.
- [ ] Eventos CT-e: cancelamento, CC-e, EPEC/contingência.
- [ ] DACTE (PDF).
> Observação: como o legado é só mock, **não há débito de paridade** — decidir se CT-e entra no escopo de aposentadoria (se nenhum cliente emite CT-e hoje, pode virar backlog e não bloqueador de cutover).

---

## 3. MDF-e (Manifesto de Documentos Fiscais) — STUB no legado, FALTA implementação real

### Legado
- `Controllers/V1/MdfeController.cs` (91 linhas) é **MOCK**:
  - Linha 41: `// TODO: Integrar com a biblioteca Zeus.Net.MDFe / Hercules.Net.MDFe`
  - `mockRetorno` falso; endpoints `emitir` e `encerrar`.
- **Schemas XSD MDF-e existem** em `Epros.ERP.API/wwwroot/SchemasMDFe/` (v1.00 e v3.00): envio, consulta recibo/situação/status, eventos (cancelamento, encerramento, inclusão de condutor), modais aéreo/aquaviário, distribuição. Mas **sem serviço que os use**.

### Novo
- Ausente por completo.

### O que falta portar/implementar
- [ ] Integração real Hercules.NET/Zeus MDF-e (legado nunca teve).
- [ ] Eventos: cancelamento, encerramento, inclusão de condutor.
- [ ] DAMDFE (PDF).
- [ ] Reaproveitar os XSDs de `wwwroot/SchemasMDFe` se seguir com biblioteca que valide por schema.
> Mesmo raciocínio do CT-e: sem paridade real a preservar; decisão de escopo.

---

## 4. DANFE / DANFCE / DANFSe e FastReport (.frx / DLLs)

### Templates .frx no legado (existem)
| Template | Local |
|---|---|
| `NFCe.frx` | `Dfe.API/wwwroot/NFCe/` e `API/wwwroot/NFCe/` |
| `NFeRetrato.frx`, `NFeRetratoSemAut.frx`, `NFeSimplificado.frx`, `NFeEvento.frx`, `NFeCartaCorrecao.frx` | `Dfe.API/wwwroot/NFe/` |
| `CupomNaoFiscal.frx` | `API/wwwroot/Reports/` |
| `DANFSeProvedorGinfes.frx` | `tools/Fr3ToFrxConverter/.../wwwroot/NFSe/Report/` |

### DLLs FastReport
- **NÃO encontradas** DLLs `FastReport*.dll` em nenhum lugar do legado (provavelmente via NuGet/GAC, não versionadas no repo). **Risco:** a licença/DLL FastReport pode não estar disponível para o novo projeto.

### Novo
- **Nenhum .frx** no repositório novo.
- DANFE/DANFCE reimplementados em **QuestPDF** (`DanfeQuestPdfService.cs`, 247 linhas): NF-e A4 (55) e cupom 80mm NFC-e (65). Comentário no arquivo: "Não depende das DLLs FastReport/NFe.Danfe do legado."

### Gap / decisão
- [ ] **DANFSe** não tem equivalente QuestPDF (só existia `.frx` Ginfes no legado) — **falta** ao portar NFS-e.
- [ ] **DANFE de eventos** (CC-e / cancelamento) — legado tinha `NFeEvento.frx` / `NFeCartaCorrecao.frx`; confirmar se QuestPDF cobre o PDF do evento (hoje `DanfeQuestPdfService` só trata 55/65 de emissão).
- [ ] **DACTE / DAMDFE** — inexistentes (dependem de CT-e/MDF-e).
- Decisão arquitetural: **manter QuestPDF** (recomendado — remove dependência de licença/DLL FastReport) e **replicar layouts .frx restantes** em QuestPDF, OU reintroduzir FastReport (exige DLL + licença). Os `.frx` legados servem de **referência de layout**.

---

## 5. Eventos (cancelamento / CC-e / inutilização / manifestação)

### Legado (real, via Hercules)
- Cancelamento NF-e/NFC-e: `NfceNfeController.cs` linha 1257 `POST cancelar`.
- Inutilização: `NfceNfeController.cs` linha 1524 `POST inutilizar` + `obter-inutilizacoes-por-documento`.
- CC-e: `NfeController.cs` linha 226 `POST carta-correcao` → `VendaNfeService.CartaCorrecao` (linha 513), gera PDF via `NFeCartaCorrecao.frx`.
- Cancelamento NFS-e: `NfseController.cs` linha 76.
- **Manifestação do destinatário / Distribuição DFe:** grep não encontrou (`DistribuicaoDFe`, `distDFeInt`, `Manifesta`) em nenhum `.cs` do legado → **não existe no legado**.

### Novo (portado)
- Cancelamento NF-e/NFC-e: `VendasFiscalController.cs` linhas 272 (`nfce/cancelar`) e 307 (`nfe/cancelar`) + `CancelarDocumentoFiscalCommandHandler`.
- CC-e: `VendasFiscalController.cs` linha 318 (`nfe/carta-correcao`) + `CartaCorrecaoDocumentoFiscalCommandHandler`.
- Inutilização: `InutilizarFaixaFiscalCommandHandler` + `InutilizacaoFiscal` entidade + `MotorLegadoFiscalService.InutilizarAsync`.
- Entidade genérica `EventoDocumentoFiscal` no domínio.

### Gap
- [x] Eventos NF-e/NFC-e: **portados**.
- [ ] Cancelamento **NFS-e**: falta (depende de portar NFS-e).
- [ ] Manifestação / Distribuição DFe: **não é migração** (legado não tem). Registrar como **feature nova** se o negócio precisar (compras via XML de fornecedor).

---

## 6. Contingência

### Legado
- App desktop dedicado: `src/Epros.PDV.Contingencia.Desktop.Data2.0` (PDV offline).
- Campos de contingência no fluxo NF-e/NFC-e (`ChaveContingencia` em `VendaNfeService.cs`, linhas 769/810/866).

### Novo
- Sem projeto/serviço de contingência. `MotorLegadoFiscalService` não trata tpEmis de contingência (SVC-AN/SVC-RS/offline NFC-e).

### O que falta
- [ ] Modo contingência NFC-e (offline) e NF-e (SVC) no motor fiscal do novo.
- [ ] Decidir destino do app desktop de contingência (portar, substituir ou aposentar com o legado).

---

## 7. IBPT

- Legado: `Services/IbptService.cs` (176 linhas) + `IbptController` + entidade `Ibpt`.
- Novo: **portado** — `External/Epros.ERP.DfeCalculos/Impostos/CalculoIbpt.cs`, `Ibpts/IbptCalculo.cs`, `IbptDfeController`, `Application/Queries/IbptQueries.cs`.
- [x] Cobertura OK (validar apenas seed/importação da tabela IBPT).

---

## Matriz de cobertura

| Feature | Legado | Novo | Status |
|---|---|---|---|
| NF-e (55) emissão | ✅ real | ✅ real (Hercules) | **PORTADO** |
| NFC-e (65) emissão | ✅ real | ✅ real (Hercules) | **PORTADO** |
| Cancelamento NF-e/NFC-e | ✅ | ✅ | **PORTADO** |
| CC-e | ✅ | ✅ | **PORTADO** |
| Inutilização | ✅ | ✅ | **PORTADO** |
| DANFE/DANFCE PDF | ✅ FastReport | ✅ QuestPDF | **PORTADO (nova stack)** |
| PDF de evento (CC-e/canc) | ✅ .frx | ⚠️ parcial | **VERIFICAR** |
| IBPT | ✅ | ✅ | **PORTADO** |
| **NFS-e (emissão/lote/cancel/consulta)** | ✅ robusto (OpenNFSe, Ginfes/GISS) | ❌ | **FALTA (crítico)** |
| **DANFSe PDF** | ✅ .frx Ginfes | ❌ | **FALTA** |
| **CT-e** | 🟡 mock stub | ❌ | **FALTA (construir)** |
| **MDF-e** | 🟡 mock stub (+ XSDs) | ❌ | **FALTA (construir)** |
| **Contingência (offline/SVC + desktop)** | ✅ desktop + campos | ❌ | **FALTA** |
| Distribuição DFe / Manifestação | ❌ inexistente | ❌ | fora de escopo de migração |

---

## Prioridade para o cutover

1. **NFS-e** — único item com paridade real perdida e provável uso por clientes de serviço. **Bloqueador** se algum dos 20 clientes emite NFS-e.
2. **PDF de evento (CC-e/cancelamento)** — verificar cobertura QuestPDF.
3. **Contingência NFC-e offline** — bloqueador para PDV em queda de conexão.
4. **CT-e / MDF-e** — só bloqueadores se houver cliente transportador; legado era mock, então provavelmente **ninguém emite hoje** → backlog pós-cutover.
5. **DLL/licença FastReport** — decisão: consolidar em QuestPDF (recomendado) e usar `.frx` só como referência de layout.

---

## Ações de verificação antes do cutover
- [ ] Confirmar com o usuário **quais dos 20 clientes emitem NFS-e, CT-e, MDF-e** (define bloqueadores reais).
- [ ] Confirmar se o `DanfeQuestPdfService` gera PDF de **evento** (CC-e/cancelamento) ou só emissão.
- [ ] Confirmar disponibilidade da **tabela IBPT** (seed) no novo.
