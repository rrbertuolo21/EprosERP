# 03 — Auditoria Cética de Paridade Visual (Frontend)

**Eixo mais crítico da migração:** o cliente NÃO PODE sentir a mudança visual. O sistema novo deve ficar visualmente igual ao legado.

- **Legado (verdade visual):** `Epros/epros_erp_front-main/app` — Nuxt 3 + **Vuetify 3**, tema **claro fixo**, fonte **Manrope**, primária **`#14325a` (azul-marinho)**.
- **Novo:** `EprosERP/Epros.App` — Nuxt 3, **CSS custom (SEM Vuetify)**, tema **ESCURO por padrão** (glassmorphism), fonte **Plus Jakarta Sans**, primária **`#6366f1` (indigo)**.

Auditoria READ-ONLY, cética. Método: mapeamento de todas as pages do legado, comparação campo-a-campo/coluna-a-coluna/botão-a-botão com a equivalente no novo, mais análise do "chrome" global (shell, sidebar, header, tabela, diálogos, campos) que aparece em TODAS as telas.

---

## RESUMO EXECUTIVO

### Veredito

**A paridade visual NÃO foi alcançada. O cliente vai perceber a mudança imediatamente, em TODAS as telas.**

A migração, na prática, foi um **rebranding completo** (Material/Vuetify claro → design system próprio, glassmorphism, escuro por padrão), não uma reimplementação visualmente fiel. Nenhuma tela é pixel-igual ao legado; a divergência começa no shell (fundo, sidebar, header, cores, fonte) e se propaga para cada componente (botões, tabelas, campos, modais, chips).

### Números

| Métrica | Valor |
|---|---|
| Telas/rotas do legado (excl. 14 páginas `dev/ui` de styleguide) | ~70 |
| "Telas" lógicas consolidadas (par lista+form conta como 1) | ~52 |
| Telas com equivalente no novo | ~48 (92%) |
| Telas AUSENTES no novo (sem equivalente) | 2 confirmadas + lacunas de conteúdo |
| **Paridade visual REAL (aparência ~igual ao legado)** | **≈ 0%** |
| Paridade **funcional/estrutural** (mesmos campos/fluxo, aparência diferente) | ≈ 70% |
| Áreas do chrome global divergentes (de 8 avaliadas) | 8 de 8 (0 IGUAL) |

> "Paridade visual real ≈ 0%" não significa que faltam funcionalidades — significa que **nenhuma tela se parece com o legado** por causa da troca de design system + tema escuro padrão + fonte + cor primária. Estruturalmente/funcionalmente o novo está ~70% equivalente.

### Top 15 divergências que o cliente notaria (ordenadas por impacto)

1. **Tema ESCURO por padrão.** Legado é claro fixo (fundo branco `#F8F7FA`). Novo abre em escuro `#09090b` (`useTheme.ts`: `DEFAULT_TEMA = 'dark'`). Só cai em claro se o SO estiver `prefers-color-scheme: light`. A maioria dos clientes verá um app quase preto onde antes era branco. **Impacto máximo — some sobre todas as telas.**
2. **Cor primária mudou:** `#14325a` (azul-marinho do legado) → `#6366f1` (indigo). Muda a identidade de marca em botões, links, foco, chips.
3. **Fonte mudou:** Manrope → Plus Jakarta Sans. Percebido em cada texto.
4. **Sidebar visualmente outra:** legado cinza-claro (`grey-50`) com ícones Tabler e menu vindo do backend (`acessos`); novo é painel glass escuro, ícones **emoji** (📇🧾🛒), logo "Epros ERP ▲" e **menu HARDCODED** em `menu.ts`.
5. **Header outro:** legado branco/flat com VChip de usuário + dropdown (Trocar Empresa, Minhas Faturas, Sair); novo é glass escuro com e-mail cru + botão logout.
6. **Estilo de tabela (todas as listagens):** Vuetify `VDataTableServer` (cabeçalho cinza, striped, densidade compacta, fixed-header/footer) → `DataTable` custom (visual escuro, sem densidade exposta). Aparece em ~40 telas.
7. **Botões:** VBtn Material → `.btn` custom com **gradiente** e `translateY` no hover. Forma, cor, hover e sombra diferentes em todo lugar.
8. **Modais/diálogos:** VDialog card branco → `AppDialog`/glass-panel escuro. Confirmações eram SweetAlert2; agora `ConfirmDialog`/`DeleteAlert` próprios.
9. **CFOP listagem:** reduzida de **19 colunas → 7**. Cliente perde visão de 12 indicadores fiscais na grade.
10. **Compras — NF-e de Entrada:** faltam cards de **Recebimentos/pagamentos**, **Transporte** e **cálculo manual de desconto/frete/seguro** presentes no legado. Perda funcional, não só visual.
11. **Compras — Devolução/Retorno de entrada:** página **inexistente** no novo (`compras/emissao/devolucao-retorno/nfe-entrada`). 404.
12. **Perfis de Acesso (detalhe):** no legado é a árvore de permissões editável; no novo a página **não renderiza o formulário** (bloqueio de backend). Página em branco.
13. **Certificado Digital:** upload de certificado A1/A3 **não existe** no novo (endpoint ausente); tela reestruturada em duas seções DF-e.
14. **PDV:** legado tem barra de menu com teclas F (F1/F6/F8...) e layout POS; novo é abas laterais sem a barra de F-keys visível — usuários de caixa perdem a descoberta por teclado.
15. **Login/Cadastro/Recuperar senha (primeiras telas vistas):** legado = card Material centralizado com imagem de fundo; novo = split-screen 55/45 com painel de marca, orbs/glow e glassmorphism. Cadastro virou wizard de 3 passos com seleção de plano.

### Recomendação

Se o objetivo é "cliente não sente a mudança", isto **não está pronto**. Prioridade absoluta antes de qualquer cutover:
1. **Default de tema = claro** (ou baseado em preferência), e garantir que sidebar/header/modais/tabela fiquem claros como o legado.
2. Trocar primária para **`#14325a`** e fonte para **Manrope** (ou aprovar formalmente o rebranding com o cliente).
3. Restaurar ícones de menu (Tabler no lugar de emoji) e menu data-driven por `acessos`.
4. Fechar as lacunas funcionais (CFOP colunas, NF-e entrada pagamentos/transporte, devolução de entrada, perfis, certificado).

---

## CHROME GLOBAL (aparece em TODAS as telas) — o maior ofensor

| Elemento | Legado | Novo | Paridade | Severidade |
|---|---|---|---|---|
| Tema padrão | **Claro fixo** (`dark:false`) | **Escuro** (`DEFAULT_TEMA='dark'`) | AUSENTE | **ALTA** |
| Cor primária | `#14325a` navy | `#6366f1` indigo | AUSENTE | ALTA |
| Fonte | Manrope | Plus Jakarta Sans | AUSENTE | MÉDIA |
| Fundo do conteúdo | Gradiente claro `#f9fafb→#fff` | `bg-grid` + glow orbs sobre `#09090b` | AUSENTE | ALTA |
| Sidebar | `VNavigationDrawer` cinza-claro, Tabler, menu do backend (`acessos`), rail+hover+pin, banner PWA | `aside.glass-panel` escuro, **emoji**, **menu hardcoded** (`menu.ts`), collapse simples, sem PWA | AUSENTE | ALTA |
| Header | `VAppBar` branco/flat, VChip usuário + dropdown, notificações | glass escuro, e-mail + logout, sem dropdown rico | AUSENTE | ALTA |
| Footer | `AppFooter` no shell | Ausente no layout default | AUSENTE | BAIXA |
| Tabela | `VDataTableServer` (striped, compact, fixed-header) | `DataTable` custom escuro, sem densidade | AUSENTE | ALTA |
| Toolbar de página | `AppToolbar`/`VToolbar` branco | `PageToolbar` div custom | PARCIAL | MÉDIA |
| Diálogos | VDialog card branco + SweetAlert2 | `AppDialog`/glass escuro + Confirm/DeleteAlert | AUSENTE | ALTA |
| Toasts | vue-sonner | `ToastHost`/`useToast` custom, ícones unicode | PARCIAL | MÉDIA |
| Campos (money/qtd/%/data/texto) | Componentes Vuetify (outline, foco azul) | `shared/fields/*` custom (foco glow, superfícies escuras) | AUSENTE | MÉDIA |

Resumo do chrome: **0 IGUAL, 18 PARCIAL, 23 AUSENTE.** Como o chrome está em todas as telas, ele sozinho garante que o cliente perceba a diferença em qualquer página.

### Observações do menu (navegação)
- Legado: menu **data-driven** a partir da árvore de permissões (`acessos`) devolvida pelo backend — reflete o perfil do usuário.
- Novo: menu **fixo em código** (`components/menu.ts`, 9 grupos com emoji). Mudança de permissão do usuário **não** altera o menu sem redeploy. Divergência funcional além de visual.

---

## ÁREA: Autenticação / Público / Área do Cliente (primeiras telas vistas)

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `login.vue` | `index.vue` | AUSENTE | Legado: card Material centralizado + imagem de fundo. Novo: split-screen 55/45, painel de marca, glow orbs, glass, toggle de tema, inputs HTML+SVG | ALTA |
| `register.vue` | `cadastro.vue` | AUSENTE | VStepper → wizard 3 passos com tracker; **seleção de plano** vira passo 1; overlay de provisionamento animado | ALTA |
| `recuperarSenha.vue` | `recuperar-senha.vue` | AUSENTE | Card com imagem de fundo → glass-panel centralizado, sem imagem | MÉDIA |
| `planos.vue` | `area-cliente/planos.vue` | AUSENTE | Público (escolher plano) → autenticado (gerenciar assinatura, upgrade in-page, badge "Plano Ativo") | ALTA |
| `area-cliente/minhas-faturas.vue` | idem | PARCIAL | `VDataTable`→tabela HTML custom; layout `blank`→`guest` com AppHeader; banner de inadimplência; modal PIX glass | MÉDIA |
| `area-cliente/faturas-vencidas.vue` | idem | PARCIAL | `VDataTable`→`DataTable`; layout `guest` com header | MÉDIA |

**Novas telas sem equivalente no legado (adições da plataforma):** `dashboard.vue` (redirect), `plataforma/admin.vue`, `plataforma/admin/{clientes,revendas,vendedores}`, `plataforma/geografia.vue`. Não são divergências do legado, mas telas novas (glass escuro).

---

## ÁREA: Cadastros

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `cadastros/parceiro/{index,[id]}` + `ParceiroForm` | `cadastros/parceiros/*` | PARCIAL | Toolbar ganhou subtítulo; VBtnGroup (tipo pessoa)→`div.grupo-botoes`; VCombobox classificações→checkboxes/chips; VTabs→`.tabs` custom; VChip→`.chip`; IconBtn+tooltip→button simples | ALTA |
| `cadastros/produto/item/{index,[id]}` + `ProdutoItemForm` | `cadastros/produtos/*` | PARCIAL | Form único (VRow/VCol+VTabs)→3 componentes de aba (`ProdutoAba*`) com tabs custom; coluna "Código"→"SKU"; busca dividida (codigo/descricao)→campo único "localizar"; add coluna "Situação" | ALTA |
| `cadastros/produto/{adicional,balanca,categoria,marca,unidade}` | `cadastros/produtos/*` | PARCIAL | Tabela cliente-side→`DataTable` server-side; VDialog→AppDialog; VTextField→TextField | MÉDIA |
| `cadastros/empresa/{index,[id]}` | `cadastros/empresas/*` | PARCIAL | Add coluna "Nome Fantasia" e reordena colunas; abas VTabs→custom; painéis extraídos (`EmpresaContatosPanel`, `EmpresaDfePanel`); SweetAlert2→DeleteAlert | ALTA |
| `cadastros/contador/{index,[id]}` | `cadastros/contadores/*` | PARCIAL (lista quase IGUAL) | Colunas/fluxo iguais; só troca de componentes (VDataTableServer→DataTable) e estilo | BAIXA |
| `cadastros/servicos/{index,[id],codigo-servicos-sefaz}` | `cadastros/servicos/*` | PARCIAL (lista quase IGUAL) | Mesmas colunas/busca; troca de componentes/estilo | BAIXA |
| `cadastros/grupo/{pessoa,produto,tributario}` | `cadastros/grupos/*` | PARCIAL (quase IGUAL) | Tabela simples + modal CRUD; VDialog→AppDialog; estilo | BAIXA |

Nota: mesmo nas telas "quase iguais" estruturalmente (contador, serviços, grupos), a aparência muda por causa do chrome/tema/tabela/botões.

---

## ÁREA: Vendas / Emissão (telas mais complexas e visíveis)

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `vendas/emissao/nfe/[[id]]` (7 cards `nfe/*`) | idem (`vendas-nfe/*`) | PARCIAL | 7 cards preservados; diálogo de **Observações NF-e** ausente na UI nova; add campo "Tipo de Operação Fiscal"; botão Cancelar condicional; nova status bar. Dialog de Impostos (9 abas) preservado | ALTA |
| `vendas/emissao/nfce/[[id]]` (layout POS `pos2` + `pos/*`) | idem (`vendas-nfce/*`, layout ERP) | PARCIAL | Redesenho total POS→ERP em cards; **Transporte ausente**; indicador de ambiente (HOMOLOGAÇÃO) menos visível | MÉDIA |
| `vendas/emissao/nfe-simplificada/[[id]]` | idem (`vendas-nfe-simplificada/*`) | PARCIAL | POS→ERP; "Tipo de Atendimento" some; Transporte ausente | MÉDIA |
| `vendas/emissao/devolucao-retorno/nfe/[[id]]` | idem | PARCIAL | Reusa cards NF-e; dialog de devolução→`DevolucaoRetornoCard`; finalidade radio→select | MÉDIA |
| `vendas/inutilizacao-numeracao.vue` | idem | PARCIAL (quase IGUAL) | Mesmos campos; VBtn→button; add colunas Protocolo/Data; ambiente menos destacado | BAIXA |
| `vendas/transmissoes.vue` | idem | PARCIAL | Colunas reordenadas/renomeadas; filtros clienteId/produtoId removidos; ações inline expandidas; badges→chips | MÉDIA |

---

## ÁREA: Compras / Estoque / Integração

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `compras/lista-compras.vue` | `compras/index.vue` | PARCIAL | Filtros reduzidos (8+ campos→1 busca), sem faixa de datas; ações por linha de **10+ → 2**; status vira badge custom | MÉDIA-ALTA |
| `compras/entrada-mercadorias/[[id]]` | idem | PARCIAL | VCard→glass sections; tabela de produtos sai do dialog p/ página; **totais sem desconto/frete/seguro manual** | MÉDIA |
| `compras/emissao/nfe-entrada/[[id]]` | idem (`compras-nfe-entrada/*`) | **AUSENTE** (conteúdo) | Faltam **Recebimentos/pagamentos**, **Transporte**, **cálculo imposto manual**, dialogs Exportação/Lacre/Reboque, ImpostosTabEntrada | ALTA |
| `compras/emissao/devolucao-retorno/nfe-entrada/[[id]]` | — | **AUSENTE** (página) | Página não existe no novo → 404 | ALTA |
| `estoque/produtos/{index,[id]}` | idem | PARCIAL | Menos colunas (some Mín/Máx/Reservado/Custeio); VDataTableServer→DataTable; SweetAlert2→DeleteAlert | MÉDIA |
| `estoque/movimento-manual.vue` | idem | PARCIAL (quase IGUAL) | Mesmas colunas; VDialog→AppDialog; estilo | BAIXA |
| `integracao/importar-xml.vue` | idem | PARCIAL | VFileInput→`<input file>` cru; **colunas da tabela diferentes** (metadados de import vs resumo do doc); chips diferentes | MÉDIA |

---

## ÁREA: Financeiro

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `financeiro/contas-a-receber/{index,[id]}` + dialogs | idem (`financeiro-receber/*`) | PARCIAL | **Filtro modal → FilterBar inline**; **lista mobile (VList) removida**; coluna "Ações" ausente no header novo; grid do form muda; dialogs de baixa com larguras/campos diferentes | ALTA |
| `financeiro/contas-a-pagar/{index,[id]}` | idem (`financeiro-pagar/*`) | N/A (legado era stub) | Legado incompleto; novo é 1ª implementação real | — |
| `financeiro/bancos.vue` | idem | PARCIAL | Legado com CRUD comentado; novo completo; troca de componentes | MÉDIA |
| `financeiro/conta-bancaria/{index,[id]}` | idem | PARCIAL (campos IGUAIS) | Mesmas colunas/labels; VDataTable→DataTable; add FilterBar | MÉDIA |
| `financeiro/natureza-financeira/{index,[id]}` | idem | PARCIAL | **Importação (upload) ausente** no novo; filtro muda; colunas ajustadas | ALTA |
| `financeiro/plano-de-contas/{index,[id]}` | idem | PARCIAL | **Importação (upload) ausente** no novo; colunas reduzidas; filtro muda | ALTA |

---

## ÁREA: Fiscal

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `fiscal/cfop/{index,[id]}` | idem | PARCIAL | **19 → 7 colunas** (perde 12 indicadores fiscais); modal de importação split-panel → lista simples; FAB removido | ALTA |
| `fiscal/tipo-operacao-fiscal/{index,[id]}` | idem | PARCIAL | Add FilterBar; enums via helper; booleanos vira badge | MÉDIA |
| `fiscal/ncm.vue` | idem | PARCIAL | Novo add CRUD+colunas de data (era read-only); filtro unificado | MÉDIA |
| `fiscal/ncm-tributacao/{index,[id]}` | idem | PARCIAL | 3 campos de filtro → 1 "localizar" | MÉDIA |
| `fiscal/icms-interestadual/index` | idem | IGUAL (estrutural) | Add coluna % FCP + hint; cores de destaque diferentes | BAIXA |
| `fiscal/codigo-beneficio-fiscal.vue` | idem | PARCIAL | CST/CSOSN: VAutocomplete multi-select → **texto separado por vírgula**; add coluna UF | MÉDIA |
| `fiscal/observacoes-nfe/index` | idem | PARCIAL | Form externo → modal inline com TextField | MÉDIA |
| `fiscal/xml-contador/index` | idem | PARCIAL | Radio→select; checkbox restyled; add colunas de metadados; formatter de data trocado | MÉDIA |

Wrappers de layout removidos no novo: `cfop.vue`, `ncm-tributacao.vue`, `xml-contador.vue`, `parceiro.vue`, etc. (roteamento por filesystem) — impacto baixo.

---

## ÁREA: Configurações / Permissões / Relatórios / PDV

| Tela legada | Tela nova | Paridade | Divergências concretas | Severidade |
|---|---|---|---|---|
| `configuracoes/certificado.vue` | idem | PARCIAL | **Upload de certificado A1/A3 ausente** (endpoint não existe); tela reestruturada em 2 seções DF-e | MÉDIA-ALTA |
| `configuracoes/permissoes/usuarios/{index,[id]}` | idem | PARCIAL | Ações viram **emoji** (✏️/🗑️); status vira badge; VCheckbox→toggle CSS; VDialog senha→AppDialog | MÉDIA |
| `configuracoes/permissoes/perfil-usuarios/{index,[id]}` | `.../perfis/{index,[id]}` | **AUSENTE** (detalhe) | Detalhe: árvore de permissões editável no legado; no novo o **form não renderiza** (bloqueio backend) → página em branco. Lista mostra aviso de erro | ALTA |
| `relatorios/vendas/simplificado01.vue` | idem | IGUAL (estrutural) | Mesmo fluxo (datas+status+export Excel); Vuetify card → glass-panel; moment→date-fns | BAIXA |
| PDV: `layouts/pos.vue`+`pos2` + `components/pos/*` (20+) | `pages/erp/pdv/index.vue` + `components/pdv/*` (6) | PARCIAL | Barra de menu com **teclas F** removida; layout vira abas laterais; overlay de transmissão unificado | MÉDIA |
| `acesso-rapido.vue` | `erp/acesso-rapido.vue` | PARCIAL | Cards coloridos+imagens → ícones AppIcon monocromáticos + banner de saudação | BAIXA |
| `acesso-restrito.vue` | `erp/acesso-restrito.vue` | PARCIAL (quase IGUAL) | VIcon lock → emoji 🔒; estilo | BAIXA |

---

## MÉTODO E LIMITAÇÕES

- Comparação por leitura de código-fonte (templates, componentes, tokens CSS/tema). **Não** houve render/execução dos apps nem screenshots lado-a-lado.
- Onde o agente não abriu o form `[id]` de detalhe explicitamente, a paridade do detalhe foi inferida pelo padrão da área (marcado "provável" nas notas dos sub-relatórios).
- Correção relevante: a primária do legado é **`#14325a`** (navy, `theme.ts`), não `#6366f1`. Portanto a cor de marca **diverge** (um sub-relatório havia sugerido igualdade — incorreto).
- Denominadores ("~52 telas") consolidam pares lista+form como 1 tela. Exclui as 14 páginas `dev/ui` (styleguide, não vistas pelo cliente).

## PRIORIZAÇÃO PARA "CLIENTE NÃO SENTIR"

1. **P0 — Tema claro por padrão** + garantir sidebar/header/tabela/modais claros. Sozinho, resolve a maior parte da percepção.
2. **P0 — Primária `#14325a` + fonte Manrope** (ou aprovar rebranding com o cliente por escrito).
3. **P1 — Sidebar:** ícones Tabler no lugar de emoji; menu data-driven por `acessos`; incluir footer.
4. **P1 — Lacunas funcionais visíveis:** CFOP (19 colunas), NF-e entrada (pagamentos/transporte/impostos manuais), devolução de entrada (página), perfis (form), certificado (upload), importações de plano-de-contas/natureza.
5. **P2 — Ajuste fino de tabela/botões/campos/modais** para densidade e aparência próximas do Vuetify claro.
6. **P2 — Regressão visual lado-a-lado** (desktop 1280px + tablet 768px) tela a tela antes do cutover.
