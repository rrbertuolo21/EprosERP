# LOG DE COMPLETUDE — Epros (legado) → EprosERP (novo)
> Fonte única da verdade do que **falta** para aposentar o legado. Consolida a auditoria cética read-only de 8 eixos (05/07/2026), cada um linha a linha. Detalhe por eixo em `docs/migracao/completude/0X_*.md`.
> **Regra de leitura:** severidade = impacto real, não esforço. "Legado tem / novo não tem (ou tem parcial/divergente)". Onde marcado *(confirmar handler)*, o controller é fino e a lógica pode estar no handler — verificar antes de tratar como perda.

---

## 0. Releitura honesta (por que "99,3%" enganava)

O mapa anterior media **"% de campo existente na entidade"** → 99,3%. Isso está **correto a nível de campo** (a auditoria confirmou 94,5% de entidades íntegras, espinha tributária 100%), mas media a coisa errada. O que faz o **cliente sentir a mudança** e o que **impede produção** vive em 5 eixos que aquele número não tocava:

| Eixo | Situação real | Nota |
|---|---|---|
| Domínio / campos | ✅ **Sólido** (94,5%, espinha fiscal 100%) | Não é o problema |
| **Visual / percepção do cliente** | 🔴 **~0% de paridade** (rebrand completo) | **Crux do item 4** |
| **Autorização / segurança** | 🔴 **API aberta** (sem authz/RBAC) | **P0 produção** |
| **Fiscal — pontas de código** | 🟡 NF-e/NFC-e reais; NFC-e **sem QR (ilegal)**; NFS-e/CT-e/MDF-e fallback | Gaps de código reais |
| **Escopo (8 módulos novos)** | 🟡 backend pronto, **0 tela, 0 teste** | Decisão de escopo |
| **Qualidade (testes/docs/warnings)** | 🟡 467 avisos, buracos de teste, doc fraca | Item 7 |

**Conclusão estratégica:** o código de domínio **não** é o gargalo. O 100% de verdade depende de **visual + segurança + pontas fiscais + decisão de escopo + qualidade**.

---

## 1. O que está SÓLIDO (não relitigar — economiza tempo)

- **Domínio:** 165 entidades legadas, **156 presentes (94,5%)**, só 2 realmente dissolvidas (`EmpresaEndereco`, `PessoaEndereco` → FK direta). Espinha tributária (NCM/CST/alíquotas/IBS-CBS, 70 campos de imposto da Venda, 21 IBS/CBS, 53 NcmTributacao) **100% íntegra**. `[01_ENTIDADES]`
- **Fiscal núcleo:** NF-e (55) e NFC-e (65) **emitem, cancelam, CCe, inutilizam e transmitem de verdade** via Hercules, com tratamento real de cStat. `[05_FISCAL]`
- **Arquitetura:** pilares respeitados — 100% herda `EntidadeSaaSBase`, PK/FK Guid, sem ownership duplicado, decisões canônicas cumpridas (financeiro único, FatoGerador só no Financeiro, catálogos `IGlobalEntity`, motor fiscal reusado). `[07_ARQUITETURA]`
- **8 módulos novos:** estruturalmente limpos (CQRS, controllers thin, Flunt no ctor) — 0 violação Crítica/Alta de forma. `[07_ARQUITETURA]`
- **Composables:** ~88% da lógica presente (distribuída em composables + components + páginas). `[04_COMPOSABLES]`

---

## 2. GAPS REAIS — por eixo, com severidade e evidência

### B1 — VISUAL (o crux do item 4) — `[03_FRONTEND_VISUAL]`
**Paridade visual real ≈ 0%. Paridade funcional/estrutural ≈ 70%.** Nenhuma tela se parece com o legado — foi um rebrand completo.

| # | Divergência (o cliente nota) | Sev |
|---|---|---|
| V1 | Tema **escuro por padrão** (`useTheme DEFAULT_TEMA='dark'`) vs claro fixo | Alta |
| V2 | Primária **navy `#14325a` → indigo `#6366f1`**; fonte **Manrope → Plus Jakarta Sans** | Alta |
| V3 | Sidebar/Header/Tabelas/Botões/Modais: Vuetify Material claro → glass escuro custom (8/8 áreas do chrome divergem) | Alta |
| V4 | Ícones **emoji** (variam por SO) vs Tabler | Média |
| **Gaps funcionais dentro do visual** | | |
| V5 | **CFOP: 19 → 7 colunas** (perde 12 indicadores fiscais) | Alta |
| V6 | **NF-e de Entrada**: sem pagamentos, transporte, cálculo manual de desconto/frete/seguro | Alta |
| V7 | **Devolução/Retorno de entrada = página 404** | Alta |
| V8 | **Perfis de Acesso (detalhe) = tela em branco** (não renderiza) | Alta |
| V9 | **Certificado**: sem upload A1/A3 | Alta |
| V10 | **PDV**: barra de teclas F removida; layout POS virou abas | Média |

### B2 — AUTORIZAÇÃO / SEGURANÇA (P0) — *verificado em código pelo lead*
| # | Gap | Evidência | Sev |
|---|---|---|---|
| S1 | **API sem imposição de autorização** — `AddAuthorization()` sem FallbackPolicy, **zero `[Authorize]`**, JWT aponta p/ Keycloak que não roda | `Program.cs:367-379` | **Crítica** |
| S2 | **Fallback anônimo** — sem token, `InquilinoSaaSMiddleware` faz `tenantId="tenant-padrao"` e segue (não rejeita) | `InquilinoSaaSMiddleware.cs:44-47` | **Crítica** |
| S3 | **RBAC por-menu do legado ausente** — `PodeLer/PodeIncluirAlterar/PodeDeletar` não portados p/ os controllers novos | sistêmico | Alta |
| S4 | Token não-JWT concatenado (risco) | `[07]` | Alta |

### B3 — FISCAL (pontas de código, não ambiente) — `[05_FISCAL]`
| # | Gap | Sev |
|---|---|---|
| F1 | **NFC-e sem QR Code no cupom** — exigência legal; `.csproj` sem lib de barcode | **Alta** |
| F2 | **Emissão mudou de single-shot → agregado multi-passo** — sem endpoint que aceite o DTO fiscal completo num tiro; **quebra integração externa stateless (PDV/ByFood)** *(confirmar necessidade p/ clientes atuais)* | Alta |
| F3 | **`salvar-nf-com-xml` ausente** — importar NF-e autorizada + registrar cancelamento por XML externo (contingência/reconciliação) | Alta |
| F4 | **Adapter NFS-e real ausente** (só `NaoConfigurado`) — integração OpenAC.Net.NFSe é código | Alta |
| F5 | **IBPT sem tabela de alíquotas** + endpoints `obter-aliquotas-por-ncm-uf`/`atualizar` | Alta |
| F6 | **CertificadoDigital ausente** — sem upload/validade/gestão de A1 (só série/CSC) | Alta |
| F7 | **Contingência não implementada** — `emContingencia=false` hardcoded; sem troca de `tpEmis`/SVC/EPEC | Média |
| F8 | **DANFE sem Code128** da chave; DANFE de pré-visualização em Vendas = stub | Média |
| F9 | Cancelamento com **cStat 573 (duplicidade)** — re-consulta de protocolo *(confirmar handler)* | Alta |
| F10 | Download por `localizadorExternoId` (PDV) e por `ambiente/documento/série/número` (ByFood) ausentes — novo só acha por `vendaId` Guid | Alta |
| — | *Não-regressão:* CT-e/MDF-e eram **mock no legado** — o `NaoConfigurado` é paridade, o novo até avançou | — |

### B4 — ENDPOINTS (fora do núcleo emissão) — `[02 substância]`
| # | Gap | Sev |
|---|---|---|
| E1 | **Loaders de tabela fiscal sumiram** — `POST .../atualizar` (IFormFile) de **NCM, CFOP-padrão, CódServiçoSefaz, FCP-alíquota, IBPT**. É o mecanismo de carga NCM/CEST | Alta |
| E2 | **Resolução CST/cClassTrib por NCM+modelo** (`obter-por-cst/{cst}/{modelo}`, `obter-por-ncm-lst`, `obter-ncm-classificados` batch) — crítico p/ emissão IBS/CBS | Alta |
| E3 | **Regras de negócio em GET perdidas** — CFOP por regime MEI + Entrada/Saída; `csosn-nfce` (escolha por regime); filtros IPI/PIS-COFINS Entrada×Saída no `EnumsController` genérico | Alta |
| E4 | **DELETE ausente** em CFOP, TipoOperacaoFiscal, CodigoBeneficioFiscal | Média |
| E5 | Envelope `ResultHttp → CommandResult`: confirmar `totalRegistros/totalPaginas` (contrato do front) | Média |
| E6 | Listas "todos" (FCP, ICMS interestadual) viraram paginadas (50) — risco de truncar cache | Média |
| E7 | Busca por código textual (NCM/CEST/ANP/IPI) trocada por Guid | Média |

### B5 — ENTIDADES (semântica/tipagem) — `[01_ENTIDADES]`
| # | Gap | Sev |
|---|---|---|
| D1 | **Venda**: `ModeloFiscal/ModalidadeFrete/VendaOrigem` de enum → `int?` (perda de tipagem) | Alta |
| D2 | **Compra**: agregados obrigatórios (`Configuracao`, `Entrega`) viraram nullable | Média |
| D3 | `EmpresaEndereco`/`PessoaEndereco` dissolvidas (N:N → 1:N) — confirmar que não perde múltiplos endereços | Média |
| D4 | `PessoaVeiculo.PaisId` ficou **`long`** (único FK não-Guid) | Alta |
| D5 | `Status` **string** em 7 entidades GestaoClientes (Fatura/PagamentoFatura/PedidoSaaS…) | Alta |
| D6 | Renames a mapear no cutover: `Usuario.Senha→PasswordHash`, `PerfilUsuario→PerfilAcesso` | Média |

### B6 — LÓGICA / COMPOSABLES (validações) — `[04_COMPOSABLES]`
| # | Gap | Sev |
|---|---|---|
| L1 | **Telefone** — novo valida só comprimento; legado validava operador/9º dígito | Alta |
| L2 | **EAN "SEM GTIN"** não aplicado (só hint) | Alta |
| L3 | **Endereço "Principal" obrigatório** sem validação | Alta |
| L4 | **CNPJ alfanumérico** quebra em `validarCpfCnpj` (só dígitos) | Média |
| L5 | **Sem módulo central de `rules`** — risco de divergência entre telas | Média |
| L6 | Estoque min≤max / saldo≥reservado / fator>0 não validados no client | Média |
| L7 | Arredondamento de item 2 casas (novo) vs 4 casas (legado NF-e) | Média |

### B7 — ESCOPO: 8 MÓDULOS NOVOS — `[06_NOVOS_RECURSOS]` `[07_ARQUITETURA]`
Backend pronto e limpo, **0 tela + 0 teste + dívida `string→enum`**: **Produção, Qualidade, RH** (alta), **Manutenção, Projetos** (média), **DMS, GRC, ESG** (baixa). → **Decisão de escopo** (§4).

### B8 — QUALIDADE (item 7) — `[08_QUALIDADE]`
| # | Gap | Sev |
|---|---|---|
| Q1 | **467 warnings** — ~90% nullable; **766 linhas no `External.DfeCalculos`** (corrigir 1 projeto mata a maioria) | Média |
| Q2 | **Testes 0** em NFS-e, CT-e, MDF-e, ImportarCompraXml; Venda só smoke; `VendaFiscalHandlers` (852 linhas) sem teste | Alta |
| Q3 | **Doc:** 85/90 controllers e 168/192 handlers sem `///`; **0 README de módulo** | Média |
| Q4 | God-files: `LandingPageSettingsHandlers` 1759, `ContextEstoque` 1048/56 DbSets, `VendaFiscalHandlers` 852 | Baixa |
| Q5 | `AccountController` injeta DbContext + regra de inadimplência inline (única violação Crítica de arquitetura) | Alta |
| Q6 | Outbox do Financeiro mapeado p/ schema `estoque` sem `ExcludeFromMigrations` | Alta |

---

## 3. NOVOS RECURSOS a alocar na tela (item 5) — `[06_NOVOS_RECURSOS]`
- **Módulos de negócio** (grupo novo no menu lateral): Produção, Qualidade, RH, Manutenção, Projetos, DMS, GRC, ESG.
- **Documentos fiscais novos** (composable pronto, falta página): CT-e, MDF-e, NFS-e, CST IBS/CBS, Pedidos de Venda, Cupons → Vendas/Emissão e Fiscal.
- **Backoffice SaaS** (parcial em `/plataforma/*`): faltam onboarding, contratos, API tokens, aprovações maker-checker, trilha de auditoria.
- **Ganhos rápidos de UI:** toggle tema, banner Demo, banner inadimplência (backend já responde 402/403).

---

## 4. DUAS DECISÕES QUE DEFINEM O PLANO (do usuário)
1. **Rumo visual:** (A) **clone fiel do legado** (claro, navy, Manrope, Material) → cliente não nota, custo alto; ou (B) **rebrand assumido** (moderno + tema claro) → custo baixo, cliente percebe.
2. **8 módulos novos:** (A) **finalizar** (enum + testes + tela) ; ou (B) **quarentenar** (ocultar do menu / feature-flag) até pós-cutover.

> O `PLANO_FECHAMENTO_FINAL.md` detalha as atividades e ramifica nessas duas escolhas.
