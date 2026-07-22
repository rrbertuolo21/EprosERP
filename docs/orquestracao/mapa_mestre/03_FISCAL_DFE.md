# Mapa Mestre — FISCAL / Faturamento Fiscal Eletrônico (PLT-DFE)

> Reconciliação spec × código real. Fonte: agente de mapa 03. Data 2026-07-22.

## Constatação central
O EprosERP tem **um único motor de transmissão SEFAZ real** para NF-e/NFC-e (modelos 55/65) via Hercules/Zeus.Net em `src/Modules/Epros.Modules.Fiscal/Infrastructure/Services/MotorLegadoFiscalService.cs`. Demais documentos (NFS-e, CT-e, MDF-e) têm entidade+handler+persistência mas transmissão é **fallback honesto** (`*NaoConfigurado`, retorna Sucesso=false sem fabricar chave/protocolo). Vários itens do macro-escopo estão **ausentes**.

Distinção: `ManifestoEletronicoDocumentosFiscais` = **MDF-e (modelo 58)**, NÃO o "Manifesto DFe / manifestação do destinatário" (Distribuição DFe) — este último não existe.

## Tabela resumo

| Documento/Área | Status | Transmissão real? | Bloqueio externo | Tier |
|---|---|---|---|---|
| PARAMETROS_FISCAIS_EMPRESA | DONE (GestaoClientes/EmpresaParametrosDfe.cs) | N/A | — | P |
| NFE_SAIDA | DONE (MotorLegadoFiscalService.EmitirAsync) | **Sim** (Hercules) | Homolog SEFAZ + cert A1 | P |
| NFCE_PDV | DONE (modelo 65, CSC/QR) | **Sim** | Homolog + CSC + cert | M |
| CANCELAMENTO_DFE | DONE (cStat 135/155) | **Sim** | SEFAZ + cert | P |
| CARTA_CORRECAO | DONE (cStat 135) | **Sim** | SEFAZ + cert | P |
| INUTILIZACAO_NUMERACAO | DONE (cStat 102) | **Sim** | SEFAZ + cert | P |
| MOTOR_CALCULO_TRIBUTARIO | DONE (External/DfeCalculos: ICMS/PIS/COFINS/IPI/IBS/CBS/ISS) | N/A | — | M |
| CADASTROS_FISCAIS | DONE (CFOP/NCM/CEST/ANP/IPI/FCP...) | N/A | — | P |
| IBPT | DONE (DfeCalculos/Impostos/Ibpts) | N/A | Base IBPT atualizada | P |
| IMPORTACAO_XML | PARCIAL (em Modules.Estoque, NfeXmlParser) | N/A inbound | — | M |
| NFE_ENTRADA | PARCIAL (reusa import Estoque; sem emissão dedicada) | N/A inbound | — | M |
| XML_CONTADOR_DOWNLOADS | PARCIAL (BaixaDocumentoDfe; storage local, MinIO TODO) | N/A | MinIO futuro | M |
| NFSE | SCAFFOLD (NfseFiscalServiceNaoConfigurado) | **Não** | OpenAC.Net.NFSe + provedor municipal + cert | G |
| CTE | SCAFFOLD (CteFiscalServiceNaoConfigurado) | **Não** | Zeus.Net.CTe + cert + homolog | G |
| MDFE | SCAFFOLD (MdfeFiscalServiceNaoConfigurado) | **Não** | Zeus.Net.MDFe + cert + homolog | G |
| MANIFESTO_DFE (destinatário) | AUSENTE (sem DistribuiçãoDFe/NSU) | **Não** | Serviço nacional DistDFe + cert | G |
| DEVOLUCAO_FISCAL | AUSENTE (só CFOPs; sem fluxo) | **Não** | — | M |
| CFE_SAT | AUSENTE (só helper cálculo; sem emissão mod.59) | **Não** | Equip/driver SAT + homolog | G |
| SPED_EFD | AUSENTE (grep = 0) | N/A arquivo | — | G |
| SINTEGRA | AUSENTE (grep = 0) | N/A arquivo | — | G |
| Contingência NF-e/NFC-e | AUSENTE (tpEmis/SVC/EPEC só em config; sem lógica) | parcial | Regras SEFAZ SVC-AN/RS | M |

## Gaps concretos
- DANFE via QuestPDF (`DanfeQuestPdfService.cs`), não `.frx`/FastReport — funcional mas divergente do legado.
- Storage: `ArmazenamentoArquivoFiscalLocal`; TODO(MinIO) em MotorLegadoFiscalService:113-115.
- NFS-e/CT-e/MDF-e: arquitetura pronta (interface+entidade+handler+persistência+evento); falta plugar lib real + cert homolog → **bloqueio externo, código construível agora**.
- Manifesto DFe, CF-e/SAT, Sintegra, SPED/EFD, Devolução: **construir do zero**.

## Direcionamento para F1
- **Construível agora (sem bloqueio externo):** SPED_EFD, SINTEGRA, DEVOLUCAO_FISCAL, MANIFESTO_DFE (DistDFe), Contingência (lógica). Completar PARCIAIS (NFE_ENTRADA, XML_CONTADOR).
- **Código + integração de lib agora, homologação = tarefa humana:** NFS-e, CT-e, MDF-e, CF-e/SAT.
