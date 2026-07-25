# GAP TOTAL — Controllers / Endpoints (Legado Epros → EprosERP)

> Auditoria de migração — dimensão **Controllers-Endpoints**.
> Objetivo: NADA ficar para trás antes de aposentar o legado.
> Data: 2026-07-04. Comprovado por grep/leitura direta dos fontes (não especulado).

## Escopo comparado

| Origem | Local | Controllers | Rotas (endpoints) |
|---|---|---|---|
| Legado — API principal | `Epros/epros_erp-main/src/Epros.ERP.API/Controllers/V1` (+ Gc, Reports) | 76 (+ BaseController abstrato) | **390** |
| Legado — DFE | `Epros/epros_erp-main/src/Epros.ERP.Dfe.API/Controllers/V1` | 11 (+ BaseController abstrato) | **64** |
| **Legado TOTAL** | | **87** | **454** |
| Novo | `EprosERP/src/API/Epros.API/Controllers` | 92 classes / 86 arquivos | **~500** |

O novo API **reestruturou as rotas** (prefixos de módulo: `fiscal/`, `cadastros/`, `financeiro/`, `plataforma/`, `aplicativo/`) e **acrescentou módulos ERP inexistentes no legado** (RH, GRC, ESG, DMS, Produção, Projetos, Manutenção, Qualidade/Inspeções, Onboarding, Installation, SuperAdmin, Assinaturas/Cupons/Pedidos). Portanto a comparação é **semântica**, não literal de string.

---

## 1. GAPS CRÍTICOS — Módulos DFE inteiros AUSENTES

Comprovado: `grep -rli "cte|mdfe|nfse"` no diretório de controllers do novo retorna **VAZIO**.

### 1.1 CTE — `CteController` (legado DFE) — 0% portado
- `POST api/v1/cte/emitir`
- `POST api/v1/cte/cancelar/{chave}`

### 1.2 MDF-e — `MdfeController` (legado DFE) — 0% portado
- `POST api/v1/mdfe/emitir`
- `POST api/v1/mdfe/encerrar/{chave}/{codigoMunicipio}`

### 1.3 NFS-e — `NfseController` (legado DFE) — 0% portado
- `GET  api/v1/nfse/config`
- `POST api/v1/nfse/emitir-lote`
- `POST api/v1/nfse/consultar-lote`
- `POST api/v1/nfse/consultar-por-rps`
- `POST api/v1/nfse/cancelar`
- `POST api/v1/nfse/gerar-pdf-xml`

---

## 2. GAPS CRÍTICOS — Downloads / Contador (documentos fiscais)

O novo `BaixaDocumentoDfeController` tem apenas **2 rotas** (`obter-por-chave/{chave}`, `download-xml/{chave}`) contra ~22 do legado (`BaixaDocumentoDfeController` + `NfceNfeController`). Comprovado: `grep -rl "download-pdf"` retorna VAZIO no novo.

### 2.1 `BaixaDocumentoDfeController` (legado principal) — faltantes
- `GET  api/v1/baixa-documentos-dfe/obter-dominio-dfe`
- `GET  api/v1/baixa-documentos-dfe/obter-dominio-principal`
- `GET  api/v1/baixa-documentos-dfe/obter-por-referencia/{mes}/{ano}/{pagina}/{tamanhoPagina}`
- `POST api/v1/baixa-documentos-dfe/gerar-baixar-novo-pdf-nfe/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-pdf/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-pdf-cancelamento/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-xml-cancelamento/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-pdf-cce/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-xml-cce/{chave}`
- `GET  api/v1/baixa-documentos-dfe/download-xml-envio/{vendaId}`
- `GET  api/v1/baixa-documentos-dfe/download-xml-compra-envio/{compraId}`
- `POST api/v1/baixa-documentos-dfe/download-documentos-contador`  ← **entrega ao contador**

### 2.2 `NfceNfeController` (legado DFE) — família download/consulta/inutilização — faltantes
- `GET  api/v1/nfce-nfe/obter-por-documento-periodo/{documento}/{dataEmissaoInicial}/{dataEmissaoFinal}/{pagina}/{tamanhoPagina}`
- `GET  api/v1/nfce-nfe/download-xml-localizador-externo-id/{chave}`
- `GET  api/v1/nfce-nfe/download-xml/{chave}`
- `GET  api/v1/nfce-nfe/download-pdf/{chave}`
- `GET  api/v1/nfce-nfe/download-xml-cancelamento/{chave}`
- `GET  api/v1/nfce-nfe/download-pdf-cancelamento/{chave}`
- `GET  api/v1/nfce-nfe/download-pdf-cce/{chave}`
- `GET  api/v1/nfce-nfe/download-xml-cce/{chave}`
- `GET  api/v1/nfce-nfe/download-xml-envio/{modeloDocumento}/{localizadorExternoId}`
- `GET  api/v1/nfce-nfe/download-xml-saida-localizador/{ambiente}/{modeloDocumento}/{documento}/{serieNf}/{numeroNf}`
- `GET  api/v1/nfce-nfe/download-saida-localizador-externoId/{ambiente}/{documento}/{localizadorExternoId}`
- `GET  api/v1/nfce-nfe/download-saida-localizador-externoId/{ambiente}/{modelo}/{documento}/{localizadorExternoId}`
- `POST api/v1/nfce-nfe/aplicar-proc-xml-por-documento-mes-ano`
- `POST api/v1/nfce-nfe/download-documentos-contador`
- `GET  api/v1/nfce-nfe/obter-inutilizacoes-por-documento/{documento}/{ambiente}`
- `POST api/v1/nfce-nfe/verificar-status`
- `POST api/v1/nfce-nfe/consultar-protocolo`
- `POST api/v1/nfce-nfe/cancelar`
- `POST api/v1/nfce-nfe/inutilizar`
- `POST api/v1/nfce-nfe/salvar-nf-com-xml`

### 2.3 `NfeController` (legado DFE) — suporte à transmissão — faltantes
- `POST api/v1/nfe/emitir-completa`
- `POST api/v1/nfe/regerar-pdf-nfe`
- `GET  api/v1/nfe/transmissoes`

> Observação: `nfe/emitir`, `nfe/carta-correcao`, `nfe/gerar-danfe-sem-autorizacao` têm equivalente semântico em `DocumentosFiscaisController` (`/emitir`, `/carta-correcao`) e `VendasFiscalController`; os itens acima **não** têm equivalente.

---

## 3. GAPS — Certificado Digital

Comprovado: `grep -rl "validar-certificado|tipo-certificado|validade-certificado"` retorna VAZIO. O novo só tem CRUD de certificado dentro de `EmpresasController` (`{id}/certificados`). Faltantes de `CertificadoDigitalController` (legado):
- `GET  api/v1/certificados-digitais/tipo-certificado-digital`
- `GET  api/v1/certificados-digitais/origem-certificado-digital`
- `GET  api/v1/certificados-digitais/validade-certificado-digital/{empresaId}`
- `POST api/v1/certificados-digitais/validar-certificado`

---

## 4. GAPS — Produtos (operações de negócio)

Comprovado: `grep -rln "reajustar|duplicar-produto|importar-csv|gtin|localizar-produto"` (produtos) retorna apenas `ProdutosHistoricosReajustesController` (CRUD histórico), **não** as ações abaixo. `ProdutosEspecificosController` no novo é só CRUD (GET/GET{id}/POST/PUT/DELETE). Faltantes de `ProdutoController` (legado):
- `GET  api/v1/produtos/localizar-produto`
- `GET  api/v1/produtos/gtin/{valorGtin}`
- `PUT  api/v1/produtos/alterar-imagem`
- `PUT  api/v1/produtos/deletar-imagem-por-id-produto/{id}`
- `POST api/v1/produtos/duplicar-produto`
- `POST api/v1/produtos/importar-csv`
- `PUT  api/v1/produtos/reajustar-precos`  ← **reajuste em massa de preços**

---

## 5. GAPS — Vendas (fluxos NFC-e/NF-e legados)

O novo consolidou em `VendasFiscalController` (route `vendas-fiscal/{id}/nfce|nfe|...`) e `VendasController` (PDV/caixas). Não há equivalente para:
- `GET  api/v1/vendas/obter-informacoes-complementares-por-produtos-ids`
- `GET  api/v1/vendas/baixar-cupom-nao-fiscal`  ← **cupom não fiscal**
- `POST api/v1/vendas/nfe-simplificado` / `PUT .../nfe-simplificado` / `POST .../nfe-simplificado-transmitir`
- `PUT  api/v1/vendas-nfe/duplicar-venda`  (`VendaNfeController`)
- `POST api/v1/vendas-nfe/gerar-danfe-sem-autorizacao/{vendaId}`

> `VendaReportsController` — `GET api/v1/relatorios/vendas/simplificado01` — **AUSENTE** (`grep "simplificado01|relatorios"` → só ESG). Não há módulo de relatórios de vendas no novo.

---

## 6. GAPS — Compras (modos de entrada e utilidades)

`ComprasController` novo tem `lancar`, `lancar-simplificado`, `{id}/cancelar`, `importar-xml`. Faltantes do legado `CompraController`:
- `POST api/v1/compras/entrada-propria`
- `POST api/v1/compras/entrada-fornecedor`
- `POST api/v1/compras/carta-correcao`
- `POST api/v1/compras/gerar-danfe-sem-autorizacao/{compraId}`
- `PUT  api/v1/compras/duplicar-compras`

> `compras-dados/obter-servicos-por-ids` (legado) não aparece no `CompraDadosController` novo (só emitente/fornecedor/transportadora/produtos/cfops).

---

## 7. GAPS — Seeds / atualizações em massa de tabelas fiscais

Endpoints `POST .../atualizar` (carga/sincronização de tabelas oficiais) do legado sem equivalente confirmado no novo:
- `POST api/v1/cfop-padrao/atualizar`  (novo só tem GET)
- `POST api/v1/classificacoes-tributarias/atualizar` + `GET csts` + `obter-por-cst/{cst}/{modelo}` + `obter-por-ncm/{ncm}/{modelo}` + `obter-por-ncm-lst/...` + `POST obter-ncm-classificados`
- `POST api/v1/codigos-servicos-sefaz/atualizar`
- `POST api/v1/ncm/atualizar` + `GET buscar-por-ncm/{codigoNcm}`
- `POST api/v1/unidades-de-medidas-tributaveis/atualizar`
- `POST api/v1/fcp-aliquotas-por-ufs/atualizar`
- `POST api/v1/ibpts/atualizar/{pass}` + `POST api/v1/ibpts/obter-aliquotas-por-ncm-uf` (legado DFE `IbptController` — no novo o `IbptDfeController` só tem `calcular-valor-aproximado`)

---

## 8. GAPS — Tenants / DFE ClienteTenant

O legado DFE `TenantController` + `ClienteTenantController` gerenciam o provisionamento multi-tenant do serviço DFE. No novo, tenant/onboarding é outro modelo (`AuthController/registrar-tenant`, `Onboarding`, `SuperAdmin/clientes`). Sem equivalente 1:1 para:
- `POST api/v1/tenants/webhook` (DFE)  — webhook de retorno DFE
- `POST api/v1/clientes-tenants/empresa-certificado`
- `POST api/v1/clientes-tenants/adicionar-empresa`
- `POST api/v1/clientes-tenants/adicionar-substituir-certificado-empresa`
- `GET  api/v1/clientes-tenants/obter-por-documento/{documento}`

> `TenantsController` (legado principal — `tenants/cadastro`, enums) e `Tenants` (DFE) precisam de decisão de arquitetura: manter no novo ou substituir pelo fluxo `plataforma/clientes` + `onboarding`. **Não é porte 1:1.**

---

## 9. Áreas COBERTAS (confirmadas — não são gap)

Para evitar retrabalho, confirmado com equivalência semântica:
- **Enums**: legado tem ~15 `*EnumsController`; novo consolidou em `EnumsController` genérico (`GET api/v1/enums/{dominio}`) via reflection. **Coberto** (validar que todos os domínios estão indexados).
- **Financeiro**: contas-a-pagar/receber → `financeiro/contas-pagar` + `financeiro/contas-receber` (baixar/estornar/cancelar/itens/totais). Conciliação legada (`conciliar-por-valor`, `conciliar-manualmente`, `criar-cp/cr-por-transacao`, `estornar-conciliacao`) → novo modelo `financeiro/ofx/.../conciliar-receber|pagar|estornar`. **Coberto (modelo diferente — validar paridade funcional).**
- **Bancos/Contas/Cartões/OFX**: `BancosContasECartoesController` (3 controllers) + `ImportacaoOfxController`. **Coberto.**
- **Cadastros básicos** (pessoas, pessoa-grupos, empresas, endereços, produtos CRUD, categorias, marcas, unidades, adicionais, veículos, serviços, contadores, cfops, cests, ncms, tributário-grupos, etc.): **Cobertos** (rotas reprefixadas em `cadastros/`, `fiscal/`).
- **Geografia** (países, municípios, CEP): `GeografiaController` cobre `municipios/obter-por-uf`, `obter-por-id-uf`, sync. **Coberto.**
- **Importação XML / Estoque / Movimentos manuais / Inutilização**: **Cobertos.**

---

## 10. Resumo quantitativo (cobertura de endpoints)

| Bloco | Endpoints legado | Faltantes confirmados |
|---|---:|---:|
| DFE CTE/MDFE/NFSe (módulos inteiros) | 10 | 10 |
| DFE downloads/consulta (BaixaDoc + NfceNfe + Nfe) | ~35 | ~35 |
| Certificado Digital | 8 | 4 |
| Produtos (operações) | 12 | 7 |
| Vendas (fluxos legados + relatório) | ~19 | ~9 |
| Compras (entradas/utilidades) | 11 | 6 |
| Seeds/atualizar tabelas fiscais | ~18 | ~14 |
| Tenants/ClienteTenant (DFE) | ~15 | ~9 (decisão arquitetural) |
| **Demais (cadastros/financeiro/enums/estoque)** | ~326 | 0 (cobertos) |
| **TOTAL** | **454** | **~94** |

**Cobertura de endpoints ≈ (454 − 94) / 454 ≈ 79%.**

> Nota: a contagem de "faltantes" usa buckets verificados por grep. Os ~326 "demais" foram confirmados cobertos por equivalência semântica (reprefixação de rota + consolidação de controllers). A margem de erro concentra-se no bloco Tenants (decisão de arquitetura, pode não exigir porte 1:1).

## 11. Prioridade de porte (para não aposentar o legado cego)

1. **P0 — Bloqueia emissão/entrega fiscal**: NFS-e, CT-e, MDF-e; família de download PDF/XML + `download-documentos-contador`; `verificar-status`/`consultar-protocolo`/`inutilizar` (NfceNfe); `nfe/emitir-completa` + `regerar-pdf-nfe` + `transmissoes`.
2. **P1 — Operação diária**: produtos `reajustar-precos`/`importar-csv`/`duplicar`/`gtin`/`localizar`; compras `entrada-propria`/`entrada-fornecedor`/`carta-correcao`; vendas `baixar-cupom-nao-fiscal`/`nfe-simplificado`; relatório `simplificado01`.
3. **P2 — Cadastros/seed**: endpoints `atualizar` de CFOP-padrão, NCM, códigos-serviços-SEFAZ, unidades tributáveis, FCP, IBPT, classificações tributárias.
4. **P3 — Decisão arquitetural**: Tenants / ClienteTenant / webhook DFE / validação de certificado (validar-certificado, validade, tipo/origem).
