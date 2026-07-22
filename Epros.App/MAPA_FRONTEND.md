# MAPA MESTRE — Reconstrução do Frontend EprosERP (Epros.App)

> Objetivo deste documento: permitir soltar ~20 agentes de tela em paralelo **sem colisão de arquivos**.
> Ele NÃO contém telas prontas — contém o mapa de rotas, o scaffolding compartilhado (gargalo serial) e a partição em fatias disjuntas.

## Contexto dos dois repositórios

| | Legado (fonte de UX) | Novo (destino) |
|---|---|---|
| Caminho | `Epros/epros_erp_front-main/app` | `EprosERP/Epros.App` |
| Stack | Nuxt 4 SPA · Vue 3 · **Vuetify 3** · Pinia · `useApiFetch` (OpenAPI types) · SignalR | Nuxt 3 SPA (Electron) · Vue 3 · **CSS custom (dark/glass)** · sem framework de UI · sem Pinia |
| API client | `useApiFetch` → `useRuntimeConfig().public.BASE_URL` | `plugins/api.ts` (override do `$fetch` com token+tenant) — **mas páginas hardcodam `http://localhost:5000`** |
| Design | Vuetify default | Tokens em `assets/css/main.css` (`--bg-color`, `--primary`, `.glass-panel`, `.btn`, `.badge`, tabela `.admin-table`) |

**Decisão de porte:** NÃO copiar Vuetify. Portar o COMPORTAMENTO do legado (fluxos, campos, validações, chamadas de API) para o design system existente do novo (glassmorphism / CSS vars). Componentes `<v-*>` viram os componentes compartilhados descritos na seção 2.

### Rotas de API disponíveis (base paths reais, extraídos dos Controllers `EprosERP/src/API/Epros.API/Controllers`)

Prefixo global: `api/v1`. Bases relevantes por módulo:

- **Cadastros:** `cadastros/pessoas`, `cadastros/pessoa-grupos`, `cadastros/empresas`, `cadastros/empresas/{empresaId}/parametros-dfe`, `cadastros/empresas/{empresaId}` (contatos), `cadastros/enderecos`, `cadastros/geografia`, `contadores`
- **Produtos/Estoque:** `estoque-produtos`, `produtos-especificos`, `produto-grupos`, `categorias-produtos`, `marcas-produtos`, `adicionais`, `balancas`, `unidades-de-medidas-comercial`, `unidades-de-medidas-tributaveis`, `estoque`, `estoque-movimentos-manuais`
- **Serviços:** `servicos`, `codigos-servicos-sefaz`
- **Fiscal:** `cfops`, `cfop-padrao`, `fiscal/cests`, `fiscal/ncms`, `fiscal/ncm-tributacoes`, `fiscal/codigos-anp`, `fiscal/csts-ibs-cbs`, `codigos-beneficios-fiscais`, `fiscal/icms-aliquotas-interestaduais`, `fiscal/fcp-aliquotas-uf`, `fiscal/enquadramentos-ipi`, `tipos-operacoes-fiscais`, `fiscal/tributario-grupos`, `fiscal/observacoes-nfe`, `fiscal/configuracoes-dfe`, `fiscal/configuracoes-impressao-nfce`, `fiscal/documentos`
- **Vendas / Emissão:** `vendas`, `vendas-fiscal` (`{id}/nfce`, `{id}/nfce/transmitir`, `{id}/nfce/cancelar`, `{id}/nfe`, `{id}/nfe/transmitir`, `{id}/nfe/cancelar`, `{id}/nfe/carta-correcao`, `{id}/nfe/referenciadas`)
- **Compras:** `compras` (`lancar`, `{id}/cancelar`, `{id}`)
- **Financeiro:** `financeiro/contas-pagar`, `financeiro/contas-receber`, `financeiro/fluxo-caixa`, `bancos`, `contas-bancarias`, `cartoes-credito`, `plano-de-contas-financeiro`, `plano-de-contas-financeiro-itens`, `configuracao-codigo-naturezas-financeiras`
- **Configurações:** `configuracoes` (parâmetros operacionais), `aplicativo/usuarios`
- **Plataforma / Área do cliente:** `plataforma/clientes`, `plataforma/contratos`, `plataforma/revendas`, `plataforma/vendedores`, `plataforma/perfil`, `plataforma/superadmin`, `plataforma/configuracoes`, `aplicativo/assinaturas`, `aplicativo/pedidos`, `aplicativo/cupons`, `public/areapublica`
- **Auth/Onboarding:** `api/v1` (`AuthController`, `AccountController`, `OnboardingController`, `InstallationController`)

> Módulos de API sem tela correspondente no legado (RH, GRC, ESG, DMS, Projetos, Producao, Inspecoes, NaoConformidades, Veiculos) **não entram** neste mapa — não inventar telas.

---

## 1. INVENTÁRIO DE ROTAS

Legenda de prioridade: **P0** = crítico primeiro (emissão/DANFE/PDV + cadastros core: produto, parceiro, empresa). **P1** = fiscal/financeiro/estoque de suporte à emissão. **P2** = config/relatórios/área do cliente/auxiliares.

Todas as rotas novas assumem raiz do ERP em `pages/erp/...` (ver seção 2, decisão de roteamento). Caminhos de página são relativos a `Epros.App/pages/`.

### Módulo: Autenticação / Onboarding

| Rota legada | Página nova (Epros.App/pages/...) | Endpoints api/v1 | Prio |
|---|---|---|---|
| `login.vue` | `index.vue` (já existe login) / `login.vue` | `login`, `me` (AuthController) | P0 |
| `register.vue` | `cadastro.vue` (já existe) | `AccountController`, `OnboardingController` | P2 |
| `recuperarSenha.vue` | `recuperar-senha.vue` | `AccountController` | P2 |
| `acesso-rapido.vue` | `erp/acesso-rapido.vue` | `me`, menus | P2 |
| `acesso-restrito.vue` | `erp/acesso-restrito.vue` | — | P2 |
| `planos.vue` | `planos.vue` | `aplicativo/assinaturas`, `plataforma/contratos` | P2 |

### Módulo: Cadastros

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `cadastros/parceiro/index.vue` | `erp/cadastros/parceiros/index.vue` | `cadastros/pessoas`, `cadastros/enderecos`, `cadastros/geografia` | **P0** |
| `cadastros/parceiro/[id].vue` | `erp/cadastros/parceiros/[id].vue` | `cadastros/pessoas`, `cadastros/enderecos`, `cadastros/pessoa-grupos` | **P0** |
| `cadastros/produto/item/index.vue` | `erp/cadastros/produtos/index.vue` | `estoque-produtos`, `produto-grupos`, `categorias-produtos`, `marcas-produtos`, `unidades-de-medidas-comercial` | **P0** |
| `cadastros/produto/item/[id].vue` | `erp/cadastros/produtos/[id].vue` | `estoque-produtos`, `produtos-especificos`, `adicionais`, `balancas`, `fiscal/ncms`, `fiscal/cests` | **P0** |
| `cadastros/produto/categoria.vue` | `erp/cadastros/produtos/categoria.vue` | `categorias-produtos` | P1 |
| `cadastros/produto/marca.vue` | `erp/cadastros/produtos/marca.vue` | `marcas-produtos` | P1 |
| `cadastros/produto/unidade.vue` | `erp/cadastros/produtos/unidade.vue` | `unidades-de-medidas-comercial`, `unidades-de-medidas-tributaveis` | P1 |
| `cadastros/produto/adicional.vue` | `erp/cadastros/produtos/adicional.vue` | `adicionais` | P1 |
| `cadastros/produto/balanca.vue` | `erp/cadastros/produtos/balanca.vue` | `balancas` | P1 |
| `cadastros/empresa/index.vue` | `erp/cadastros/empresas/index.vue` | `cadastros/empresas` | **P0** |
| `cadastros/empresa/[id].vue` | `erp/cadastros/empresas/[id].vue` | `cadastros/empresas`, `cadastros/empresas/{id}/parametros-dfe`, `cadastros/empresas/{id}` (contatos) | **P0** |
| `cadastros/contador/index.vue` | `erp/cadastros/contadores/index.vue` | `contadores` | P1 |
| `cadastros/contador/[id].vue` | `erp/cadastros/contadores/[id].vue` | `contadores` | P1 |
| `cadastros/grupo/pessoa.vue` | `erp/cadastros/grupos/pessoa.vue` | `cadastros/pessoa-grupos` | P1 |
| `cadastros/grupo/produto.vue` | `erp/cadastros/grupos/produto.vue` | `produto-grupos` | P1 |
| `cadastros/grupo/tributario.vue` | `erp/cadastros/grupos/tributario.vue` | `fiscal/tributario-grupos` | P1 |
| `cadastros/servicos/index.vue` | `erp/cadastros/servicos/index.vue` | `servicos` | P1 |
| `cadastros/servicos/[id].vue` | `erp/cadastros/servicos/[id].vue` | `servicos`, `codigos-servicos-sefaz` | P1 |
| `cadastros/servicos/codigo-servicos-sefaz.vue` | `erp/cadastros/servicos/codigo-servicos-sefaz.vue` | `codigos-servicos-sefaz` | P2 |

### Módulo: Vendas / Emissão (P0 — núcleo)

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `vendas/emissao/nfe/[[id]].vue` | `erp/vendas/emissao/nfe/[[id]].vue` | `vendas`, `vendas-fiscal/{id}/nfe`, `.../nfe/transmitir`, `.../nfe/cancelar`, `.../nfe/referenciadas`, `cfops`, `tipos-operacoes-fiscais` | **P0** |
| `vendas/emissao/nfe-simplificada/[[id]].vue` | `erp/vendas/emissao/nfe-simplificada/[[id]].vue` | `vendas`, `vendas-fiscal/{id}/nfe`, `.../nfe/transmitir` | **P0** |
| `vendas/emissao/nfce/[[id]].vue` | `erp/vendas/emissao/nfce/[[id]].vue` | `vendas`, `vendas-fiscal/{id}/nfce`, `.../nfce/transmitir`, `.../nfce/cancelar` | **P0** |
| `vendas/emissao/devolucao-retorno/nfe/[[id]].vue` | `erp/vendas/emissao/devolucao-retorno/nfe/[[id]].vue` | `vendas`, `vendas-fiscal/{id}/nfe`, `.../nfe/referenciadas` | P1 |
| `vendas/transmissoes.vue` | `erp/vendas/transmissoes.vue` | `fiscal/documentos`, `vendas-fiscal` | **P0** (lista/DANFE) |
| `vendas/inutilizacao-numeracao.vue` | `erp/vendas/inutilizacao-numeracao.vue` | `fiscal/documentos`, `vendas-fiscal` | P1 |

### Módulo: PDV (P0)

> No legado o PDV é o fluxo NFC-e sob layouts `pos`/`pos2` + componentes `components/pos/*`. Não há arquivo `pages/pdv/*`; a "tela PDV" é a emissão NFC-e com layout de caixa.

| Origem legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `layouts/pos.vue` + `components/pos/*` + `nfce/[[id]]` | `erp/pdv/index.vue` (+ layout `pos`) | `vendas`, `vendas-fiscal/{id}/nfce`, `.../nfce/transmitir`, `.../nfce/cancelar`, `estoque-produtos`, `balancas` | **P0** |

### Módulo: Compras

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `compras/lista-compras.vue` | `erp/compras/index.vue` | `compras` | P1 |
| `compras/entrada-mercadorias/[[id]].vue` | `erp/compras/entrada-mercadorias/[[id]].vue` | `compras` (`lancar`, `{id}`), `estoque-produtos`, `cadastros/pessoas` | P1 |
| `compras/emissao/nfe-entrada/[[id]].vue` | `erp/compras/emissao/nfe-entrada/[[id]].vue` | `compras`, `vendas-fiscal` | P1 |
| `compras/emissao/devolucao-retorno/nfe-entrada/[[id]].vue` | `erp/compras/emissao/devolucao-retorno/nfe-entrada/[[id]].vue` | `compras`, `vendas-fiscal/{id}/nfe/referenciadas` | P2 |
| `integracao/importar-xml.vue` | `erp/integracao/importar-xml.vue` | `compras`, `fiscal/documentos` | P2 |

### Módulo: Estoque

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `estoque/produtos/index.vue` | `erp/estoque/produtos/index.vue` | `estoque`, `estoque-produtos` | P1 |
| `estoque/produtos/[id].vue` | `erp/estoque/produtos/[id].vue` | `estoque`, `estoque-produtos` | P1 |
| `estoque/movimento-manual.vue` | `erp/estoque/movimento-manual.vue` | `estoque-movimentos-manuais` | P1 |

### Módulo: Financeiro

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `financeiro/contas-a-receber/index.vue` | `erp/financeiro/contas-a-receber/index.vue` | `financeiro/contas-receber` | P1 |
| `financeiro/contas-a-receber/[id].vue` | `erp/financeiro/contas-a-receber/[id].vue` | `financeiro/contas-receber`, `configuracao-codigo-naturezas-financeiras` | P1 |
| `financeiro/contas-a-pagar/index.vue` | `erp/financeiro/contas-a-pagar/index.vue` | `financeiro/contas-pagar` | P1 |
| `financeiro/contas-a-pagar/[id].vue` | `erp/financeiro/contas-a-pagar/[id].vue` | `financeiro/contas-pagar` | P1 |
| `financeiro/bancos.vue` | `erp/financeiro/bancos.vue` | `bancos` | P2 |
| `financeiro/conta-bancaria/index.vue` | `erp/financeiro/conta-bancaria/index.vue` | `contas-bancarias`, `cartoes-credito` | P2 |
| `financeiro/conta-bancaria/[id].vue` | `erp/financeiro/conta-bancaria/[id].vue` | `contas-bancarias` | P2 |
| `financeiro/natureza-financeira/index.vue` | `erp/financeiro/natureza-financeira/index.vue` | `configuracao-codigo-naturezas-financeiras` | P2 |
| `financeiro/natureza-financeira/[id].vue` | `erp/financeiro/natureza-financeira/[id].vue` | `configuracao-codigo-naturezas-financeiras` | P2 |
| `financeiro/plano-de-contas/index.vue` | `erp/financeiro/plano-de-contas/index.vue` | `plano-de-contas-financeiro`, `plano-de-contas-financeiro-itens` | P2 |
| `financeiro/plano-de-contas/[id].vue` | `erp/financeiro/plano-de-contas/[id].vue` | `plano-de-contas-financeiro` | P2 |

### Módulo: Fiscal

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `fiscal/cfop/index.vue` | `erp/fiscal/cfop/index.vue` | `cfops`, `cfop-padrao` | P1 |
| `fiscal/cfop/[id].vue` | `erp/fiscal/cfop/[id].vue` | `cfops` | P1 |
| `fiscal/ncm.vue` | `erp/fiscal/ncm.vue` | `fiscal/ncms` | P1 |
| `fiscal/ncm-tributacao/index.vue` | `erp/fiscal/ncm-tributacao/index.vue` | `fiscal/ncm-tributacoes` | P1 |
| `fiscal/ncm-tributacao/[id].vue` | `erp/fiscal/ncm-tributacao/[id].vue` | `fiscal/ncm-tributacoes`, `fiscal/cests`, `fiscal/csts-ibs-cbs` | P1 |
| `fiscal/tipo-operacao-fiscal/index.vue` | `erp/fiscal/tipo-operacao-fiscal/index.vue` | `tipos-operacoes-fiscais` | P1 |
| `fiscal/tipo-operacao-fiscal/[id].vue` | `erp/fiscal/tipo-operacao-fiscal/[id].vue` | `tipos-operacoes-fiscais`, `cfops` | P1 |
| `fiscal/icms-interestadual/index.vue` | `erp/fiscal/icms-interestadual/index.vue` | `fiscal/icms-aliquotas-interestaduais`, `fiscal/fcp-aliquotas-uf` | P2 |
| `fiscal/codigo-beneficio-fiscal.vue` | `erp/fiscal/codigo-beneficio-fiscal.vue` | `codigos-beneficios-fiscais` | P2 |
| `fiscal/observacoes-nfe/index.vue` | `erp/fiscal/observacoes-nfe/index.vue` | `fiscal/observacoes-nfe` | P2 |
| `fiscal/xml-contador/index.vue` | `erp/fiscal/xml-contador/index.vue` | `fiscal/documentos` | P2 |

### Módulo: Relatórios

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `relatorios/vendas/simplificado01.vue` | `erp/relatorios/vendas/simplificado01.vue` | `vendas`, `fiscal/documentos` | P2 |

### Módulo: Configurações

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `configuracoes/certificado.vue` | `erp/configuracoes/certificado.vue` | `fiscal/configuracoes-dfe`, `cadastros/empresas/{id}/parametros-dfe` | P1 |
| `configuracoes/permissoes/usuarios/index.vue` | `erp/configuracoes/permissoes/usuarios/index.vue` | `aplicativo/usuarios` | P1 |
| `configuracoes/permissoes/usuarios/[id].vue` | `erp/configuracoes/permissoes/usuarios/[id].vue` | `aplicativo/usuarios`, `plataforma/perfil` | P1 |
| `configuracoes/permissoes/perfil-usuarios/index.vue` | `erp/configuracoes/permissoes/perfis/index.vue` | `plataforma/perfil` | P1 |
| `configuracoes/permissoes/perfil-usuarios/[id].vue` | `erp/configuracoes/permissoes/perfis/[id].vue` | `plataforma/perfil` | P1 |

### Módulo: Área do cliente (portal SaaS)

| Rota legada | Página nova | Endpoints api/v1 | Prio |
|---|---|---|---|
| `area-cliente/minhas-faturas.vue` | `area-cliente/minhas-faturas.vue` (já existe) | `aplicativo/assinaturas`, `plataforma/contratos` | P2 |
| `area-cliente/faturas-vencidas.vue` | `area-cliente/faturas-vencidas.vue` | `aplicativo/assinaturas` | P2 |
| (planos) | `area-cliente/planos.vue` (já existe) | `aplicativo/assinaturas` | P2 |

> **`dev/ui/*` (12 telas de style-guide) NÃO são portadas** — o novo tem seu próprio design system em `assets/css/main.css`. Serão substituídas pela galeria de componentes compartilhados (opcional, tarefa do integrador).

**Total de telas mapeadas (portáveis): ~63** (48 páginas de ERP `.vue` reais no legado, excluindo `dev/ui` e wrapper-shells `*.vue` que só renderizam `<NuxtPage>`; contando aqui cada `index`/`[id]`/`[[id]]` como tela distinta).

---

## 2. SCAFFOLDING COMPARTILHADO (gargalo serial — construir ANTES do fan-out)

Estes são os **recursos exclusivos**: UM agente/integrador cria todos eles primeiro. Nenhum agente de tela pode editá-los depois. Ordem de dependência de cima para baixo.

### 2.1 Configuração de ambiente e roteamento
- **`nuxt.config.ts`** (editar existente): adicionar `runtimeConfig.public.apiBaseUrl` lendo `NUXT_PUBLIC_API_BASE_URL` (default configurável, ex. `http://localhost:5000`), `realtimeUrl`, `storageUri`. Manter `ssr:false` e `baseURL:'./'` (Electron). **NÃO hardcodar localhost nas telas.**
- **`.env.example`** (novo): `NUXT_PUBLIC_API_BASE_URL=`, `NUXT_PUBLIC_REALTIME_URL=`, `NUXT_PUBLIC_STORAGE_URI=`.
- **Convenção de rotas:** todo o ERP sob `pages/erp/...`; portal SaaS permanece em `pages/area-cliente` e `pages/plataforma`. Login em `pages/index.vue`.

### 2.2 Cliente de API único
- **`plugins/api.ts`** (refatorar existente): usar `useRuntimeConfig().public.apiBaseUrl` como `baseURL` do `ofetch.create`. Manter injeção de `Authorization: Bearer` + `X-Tenant-Id`. Adicionar `onResponseError` para 401 → limpar sessão e redirecionar.
- **`composables/useApi.ts`** (novo): wrapper fino sobre o `$fetch` global (baseURL já embutida). Assinatura: `useApi<T>(path, { method, query, body, params })` com substituição de `{param}` na rota. **Este é o único ponto de IO que as telas usam.** Espelha o `useApiFetch` do legado, mas sem OpenAPI types (o novo não tem geração de tipos ainda).
- **`composables/useApiList.ts`** (novo): helper de listagem paginada server-side (page, perPage, total, sortBy, filtros → chama `useApi`), portando o padrão `usePessoa`/`references/listing-pages.md` do legado. Base para todas as telas `index.vue`.

### 2.3 Autenticação / sessão / tenant
- **`composables/useAuth.ts`** (novo): login, logout, `me`, leitura/escrita de `epros_token`/`epros_user` no localStorage, `decodeJWT`. Porta `useAuth`/`useDecodeJWT`/`useTokenValidation` do legado.
- **`middleware/auth.global.ts`** (já existe — integrador ajusta): manter guard SaaS existente; adicionar liberação das rotas `/erp/*` para tenant adimplente.
- **`composables/useTenant.ts`** (novo): empresa/tenant ativo, troca de empresa, expõe `empresaId` para telas que filtram por empresa.

### 2.4 Layout(s) padrão
- **`layouts/default.vue`** (novo): shell do ERP = `AppHeader` + `AppSidebar` + `<slot/>`, sobre o fundo glass já em `app.vue`. Porta `layouts/default.vue` do legado.
- **`layouts/pos.vue`** (novo): layout de caixa/PDV (sem sidebar, foco em produto+pagamento). Porta `layouts/pos.vue`/`pos2.vue`.
- **`layouts/guest.vue`** (novo): login/cadastro/recuperar-senha.
- **`components/AppHeader.vue`** (já existe — integrador estende com menu ERP).
- **`components/AppSidebar.vue`** + **`AppSidebarGroup.vue`** + **`AppSidebarItem.vue`** (novos): navegação por módulo. Porta `AppSiderBar`/`AppSideBarGroup`/`AppSideBarItem`.

### 2.5 Design tokens / tema
- **`assets/css/main.css`** (já existe — integrador estende): adicionar classes utilitárias que faltam para formulários densos e tabelas de ERP (inputs, labels, grid de form, chips fiscais). **Nenhum agente de tela edita este arquivo**; telas usam `<style scoped>` para o que for específico.

### 2.6 Componentes compartilhados reutilizáveis (substituem os `<v-*>` do legado)

Todos em `Epros.App/components/shared/`. São **recursos exclusivos** — agentes de tela consomem, nunca editam.

| Componente | Arquivo-alvo | Substitui no legado | Uso |
|---|---|---|---|
| DataTable (listagem paginada) | `components/shared/DataTable.vue` | `VDataTableServer` / `Table.vue` | toda tela `index.vue` |
| Toolbar de página | `components/shared/PageToolbar.vue` | `AppToolbar.vue`/`Toolbar.vue` | título + ações + botão criar |
| Barra de filtros | `components/shared/FilterBar.vue` | `FilterBar.vue` | filtros das listagens |
| Dialog/modal base | `components/shared/AppDialog.vue` | `v-dialog` | todos os modais |
| Confirmação | `components/shared/ConfirmDialog.vue` | `ConfirmDialog.vue`/`useSwal` | exclusões/ações destrutivas |
| Alerta de exclusão | `components/shared/DeleteAlert.vue` | `DeleteAlert.vue` | delete em listas |
| Toast/notificação | `composables/useToast.ts` + `components/shared/ToastHost.vue` | `vue-sonner`/`AppNotification` | feedback de ações |
| Campo texto | `components/shared/fields/TextField.vue` | `v-text-field` | forms |
| Campo select | `components/shared/fields/SelectField.vue` | `v-select`/`v-autocomplete` | forms |
| Campo dinheiro | `components/shared/fields/MoneyInput.vue` | `InputMoney.vue` | forms fiscais/financeiros |
| Campo quantidade | `components/shared/fields/QuantityInput.vue` | `InputQuantidade.vue` | produtos/itens |
| Campo porcentagem | `components/shared/fields/PercentInput.vue` | `InputPorcentagem.vue` | impostos |
| Campo data/hora | `components/shared/fields/DateTimeField.vue` | `DateTimeInput.vue` | forms |
| Máscara (CPF/CNPJ/CEP/tel) | `composables/useMask.ts` + `composables/useDocumento.ts` | `maska`/`validations-br`/`cep-promise`/`useDocumento` | forms de parceiro/empresa |
| Seletor de empresa/tenant | `components/shared/EmpresaSelector.vue` | (header do legado) | header/telas multiempresa |
| DANFE viewer | `components/shared/DanfeViewer.vue` | `plugins/pdf.js`/`nfe/Exportacao` | transmissões, emissão, PDV |
| Overlay de transmissão | `components/shared/TransmissionOverlay.vue` | `TransmissionProgressOverlay.vue` | emissão NF-e/NFC-e |
| Cliente SignalR | `composables/useRealtime.ts` | `useSignalr.ts` | emissão em tempo real |
| Enums | `composables/useEnum.ts` | `useEnum.ts`/`enumStore` | selects de domínio |
| Helpers de formatação | `composables/useHelper.ts` | `useHelper.ts` (datas via date-fns, moeda) | geral |

**Total de itens de scaffolding: ~30 arquivos** (config + api client + auth/tenant + 3 layouts + 3 componentes de sidebar + ~19 componentes/composables compartilhados). Este é o gargalo serial.

---

## 3. PARTIÇÃO EM ~20 FATIAS DISJUNTAS

Regra de ouro: **dois grupos nunca escrevem o mesmo arquivo.** Cada fatia possui uma pasta exclusiva sob `pages/erp/<modulo>/<área>/` e, se precisar de componentes específicos, uma pasta exclusiva `components/<modulo>-<área>/`. Componentes compartilhados (seção 2.6) são somente-leitura para todos.

| # | Fatia | Arquivos-alvo (exclusivos) | Endpoints | Depende de scaffolding |
|---|---|---|---|---|
| 1 | **Cadastro Parceiros** | `pages/erp/cadastros/parceiros/{index,[id]}.vue` + `components/cadastros-parceiro/*` | `cadastros/pessoas`, `enderecos`, `geografia`, `pessoa-grupos` | 2.2 useApi/useApiList, 2.6 fields+DataTable+useMask+useDocumento |
| 2 | **Cadastro Produtos** | `pages/erp/cadastros/produtos/{index,[id]}.vue` + `components/cadastros-produto/*` | `estoque-produtos`, `produtos-especificos`, `fiscal/ncms`, `cests`, `adicionais`, `balancas` | 2.2, 2.6 fields+DataTable+MoneyInput+QuantityInput |
| 3 | **Cadastro Produtos-aux** | `pages/erp/cadastros/produtos/{categoria,marca,unidade,adicional,balanca}.vue` | `categorias-produtos`, `marcas-produtos`, `unidades-de-medidas-*`, `adicionais`, `balancas` | 2.2, 2.6 DataTable+fields |
| 4 | **Cadastro Empresas** | `pages/erp/cadastros/empresas/{index,[id]}.vue` + `components/cadastros-empresa/*` | `cadastros/empresas`, `empresas/{id}/parametros-dfe`, `empresas/{id}` (contatos), `geografia` | 2.2, 2.3 useTenant, 2.6 fields+useDocumento |
| 5 | **Cadastro Contadores + Grupos** | `pages/erp/cadastros/contadores/{index,[id]}.vue`, `pages/erp/cadastros/grupos/{pessoa,produto,tributario}.vue` | `contadores`, `pessoa-grupos`, `produto-grupos`, `fiscal/tributario-grupos` | 2.2, 2.6 DataTable+fields |
| 6 | **Cadastro Serviços** | `pages/erp/cadastros/servicos/{index,[id],codigo-servicos-sefaz}.vue` | `servicos`, `codigos-servicos-sefaz` | 2.2, 2.6 DataTable+fields |
| 7 | **Emissão NF-e** | `pages/erp/vendas/emissao/nfe/[[id]].vue` + `components/vendas-nfe/*` | `vendas`, `vendas-fiscal/{id}/nfe(+transmitir/cancelar/carta-correcao/referenciadas)`, `cfops`, `tipos-operacoes-fiscais` | 2.2, 2.6 DanfeViewer+TransmissionOverlay+useRealtime+fields |
| 8 | **Emissão NF-e Simplificada** | `pages/erp/vendas/emissao/nfe-simplificada/[[id]].vue` + `components/vendas-nfe-simplificada/*` | `vendas`, `vendas-fiscal/{id}/nfe(+transmitir)` | 2.2, 2.6 DanfeViewer+TransmissionOverlay+useRealtime |
| 9 | **Emissão NFC-e** | `pages/erp/vendas/emissao/nfce/[[id]].vue` + `components/vendas-nfce/*` | `vendas`, `vendas-fiscal/{id}/nfce(+transmitir/cancelar)` | 2.2, 2.6 DanfeViewer+TransmissionOverlay+useRealtime |
| 10 | **PDV (caixa)** | `pages/erp/pdv/index.vue` + `components/pdv/*` + `layouts/pos.vue` (consumo) | `vendas`, `vendas-fiscal/{id}/nfce(+transmitir/cancelar)`, `estoque-produtos`, `balancas` | 2.4 layout pos, 2.2, 2.6 DanfeViewer+TransmissionOverlay+MoneyInput |
| 11 | **Devolução/Retorno + Transmissões + Inutilização** | `pages/erp/vendas/emissao/devolucao-retorno/nfe/[[id]].vue`, `pages/erp/vendas/{transmissoes,inutilizacao-numeracao}.vue` + `components/vendas-transmissoes/*` | `vendas-fiscal`, `fiscal/documentos`, `.../nfe/referenciadas` | 2.2, 2.6 DataTable+DanfeViewer |
| 12 | **Compras — lista + entrada mercadorias** | `pages/erp/compras/index.vue`, `pages/erp/compras/entrada-mercadorias/[[id]].vue` + `components/compras-entrada/*` | `compras` (`lancar`,`{id}`,`{id}/cancelar`), `estoque-produtos`, `cadastros/pessoas` | 2.2, 2.6 DataTable+fields+MoneyInput |
| 13 | **Compras — NF-e entrada + Importar XML** | `pages/erp/compras/emissao/nfe-entrada/[[id]].vue`, `pages/erp/compras/emissao/devolucao-retorno/nfe-entrada/[[id]].vue`, `pages/erp/integracao/importar-xml.vue` + `components/compras-nfe-entrada/*` | `compras`, `vendas-fiscal`, `fiscal/documentos` | 2.2, 2.6 fields+DanfeViewer |
| 14 | **Estoque** | `pages/erp/estoque/produtos/{index,[id]}.vue`, `pages/erp/estoque/movimento-manual.vue` + `components/estoque/*` | `estoque`, `estoque-produtos`, `estoque-movimentos-manuais` | 2.2, 2.6 DataTable+QuantityInput+fields |
| 15 | **Financeiro — Contas a Receber** | `pages/erp/financeiro/contas-a-receber/{index,[id]}.vue` + `components/financeiro-receber/*` | `financeiro/contas-receber`, `configuracao-codigo-naturezas-financeiras` | 2.2, 2.6 DataTable+MoneyInput+DateTimeField |
| 16 | **Financeiro — Contas a Pagar** | `pages/erp/financeiro/contas-a-pagar/{index,[id]}.vue` + `components/financeiro-pagar/*` | `financeiro/contas-pagar` | 2.2, 2.6 DataTable+MoneyInput+DateTimeField |
| 17 | **Financeiro — Cadastros aux** | `pages/erp/financeiro/bancos.vue`, `conta-bancaria/{index,[id]}.vue`, `natureza-financeira/{index,[id]}.vue`, `plano-de-contas/{index,[id]}.vue` | `bancos`, `contas-bancarias`, `cartoes-credito`, `configuracao-codigo-naturezas-financeiras`, `plano-de-contas-financeiro(-itens)` | 2.2, 2.6 DataTable+fields |
| 18 | **Fiscal — CFOP + Tipo Operação + NCM** | `pages/erp/fiscal/cfop/{index,[id]}.vue`, `pages/erp/fiscal/tipo-operacao-fiscal/{index,[id]}.vue`, `pages/erp/fiscal/{ncm,ncm-tributacao/index,ncm-tributacao/[id]}.vue` | `cfops`, `cfop-padrao`, `tipos-operacoes-fiscais`, `fiscal/ncms`, `fiscal/ncm-tributacoes`, `cests`, `csts-ibs-cbs` | 2.2, 2.6 DataTable+fields+PercentInput |
| 19 | **Fiscal — ICMS/Benefícios/Observações/XML** | `pages/erp/fiscal/icms-interestadual/index.vue`, `codigo-beneficio-fiscal.vue`, `observacoes-nfe/index.vue`, `xml-contador/index.vue` | `fiscal/icms-aliquotas-interestaduais`, `fcp-aliquotas-uf`, `codigos-beneficios-fiscais`, `fiscal/observacoes-nfe`, `fiscal/documentos` | 2.2, 2.6 DataTable+fields+PercentInput |
| 20 | **Configurações + Certificado** | `pages/erp/configuracoes/certificado.vue`, `permissoes/usuarios/{index,[id]}.vue`, `permissoes/perfis/{index,[id]}.vue` + `components/config/*` | `fiscal/configuracoes-dfe`, `empresas/{id}/parametros-dfe`, `aplicativo/usuarios`, `plataforma/perfil` | 2.2, 2.6 DataTable+fields |
| 21 | **Relatórios + Área do cliente + Auth aux** | `pages/erp/relatorios/vendas/simplificado01.vue`, `pages/area-cliente/faturas-vencidas.vue`, `pages/recuperar-senha.vue`, `pages/erp/acesso-rapido.vue`, `pages/erp/acesso-restrito.vue` | `vendas`, `fiscal/documentos`, `aplicativo/assinaturas`, `AccountController` | 2.2, 2.6 DataTable; 2.4 layout guest |

> São **21 fatias** (≈20). As fatias 7–10 (emissão + PDV) são P0 e devem ser as primeiras liberadas no fan-out, junto com 1, 2 e 4 (parceiro, produto, empresa). As demais são P1/P2.

Cada fatia = pasta de páginas exclusiva + (quando houver componentes locais) pasta `components/<slug>/` exclusiva daquela fatia. Como nenhuma pasta de páginas nem pasta de componentes se sobrepõe entre fatias, **não há colisão de escrita**.

---

## 4. REGRAS PARA OS AGENTES DE TELA (colar no prompt)

1. **IO só pelo cliente compartilhado.** Use exclusivamente `useApi`/`useApiList` (`composables/useApi.ts`, `composables/useApiList.ts`). **Nunca** chame `$fetch`/`useFetch`/`ofetch` direto, **nunca** hardcode URL (nada de `http://localhost:...`). A baseURL vem de `useRuntimeConfig().public.apiBaseUrl`.
2. **UI só com componentes compartilhados.** Use `components/shared/*` (DataTable, PageToolbar, FilterBar, AppDialog, ConfirmDialog, fields/*, DanfeViewer, TransmissionOverlay) e os composables compartilhados (`useAuth`, `useTenant`, `useToast`, `useEnum`, `useMask`, `useDocumento`, `useHelper`, `useRealtime`). Se faltar um componente compartilhado, **pare e reporte** ao integrador — não crie um concorrente.
3. **Não editar recursos exclusivos.** Proibido tocar em `nuxt.config.ts`, `middleware/*`, `layouts/*`, `plugins/api.ts`, `assets/css/main.css`, `components/AppHeader.vue`, `components/AppSidebar*`, `components/shared/*`, `composables/useApi*`, `composables/useAuth`/`useTenant`/`useEnum`/`useHelper`/`useMask`/`useRealtime`. Só leia.
4. **Escreva só nos arquivos da sua fatia.** Suas páginas ficam sob sua pasta `pages/erp/<...>/` e seus componentes locais sob `components/<slug-da-fatia>/`. Não crie arquivos fora dela.
5. **TypeScript + `<script setup lang="ts">`.** Props/emits tipados (`defineProps<{}>()`, `defineEmits<{}>()`). Sem `any`. `loading`/`error` em toda operação de IO; `try/catch` explícito.
6. **Textos em PT-BR.** Labels, títulos, mensagens e comentários em português. Termos técnicos e identificadores de API mantêm o nome original.
7. **Porte o comportamento, não o Vuetify.** Leia a tela legada equivalente em `Epros/epros_erp_front-main/app/pages/...` (e seus composables em `composables/<modulo>/`) para reproduzir campos, validações, fluxo de emissão/transmissão e regras. **Não copie markup Vuetify (`<v-*>`) nem SCSS Vuetify** — reconstrua com o design system do novo (`.glass-panel`, `.btn`, `.badge`, tokens CSS de `main.css`) e os componentes compartilhados.
8. **Listagens** seguem o padrão `useApiList` + `DataTable` + `FilterBar` (server-side, paginação, filtros na URL) — espelhando `references/listing-pages.md` do legado, adaptado ao design novo.
9. **Datas** com `date-fns` (via `useHelper`), nunca `moment`.
10. **Feedback** de ação via `useToast`; confirmações destrutivas via `ConfirmDialog`.

---

## RESUMO EXECUTIVO

- **Telas mapeadas (portáveis):** ~63 (48 páginas ERP reais + auth/portal; excluídas as 12 telas `dev/ui` de style-guide e módulos de API sem tela — RH/GRC/ESG/DMS/Projetos/Producao/Qualidade/Veiculos).
- **Fatias disjuntas:** 21 (≈20), cada uma com pasta de páginas + pasta de componentes exclusivas, garantindo zero colisão de escrita. Liberar primeiro as P0: fatias 1, 2, 4 (cadastros core), 7, 8, 9, 10 (emissão NF-e/NFC-e/simplificada + PDV) e 11 (transmissões/DANFE).
- **Gargalo serial (construir ANTES do fan-out) — ~30 arquivos de scaffolding compartilhado:**
  1. Config/ambiente: `nuxt.config.ts` (runtimeConfig `apiBaseUrl`), `.env.example`, convenção de rotas `pages/erp/*`.
  2. Cliente de API único: `plugins/api.ts` (baseURL configurável, sem localhost), `composables/useApi.ts`, `composables/useApiList.ts`.
  3. Auth/tenant: `composables/useAuth.ts`, `composables/useTenant.ts`, ajuste em `middleware/auth.global.ts`.
  4. Layouts: `layouts/default.vue`, `layouts/pos.vue`, `layouts/guest.vue` + `components/AppHeader.vue` (estender) + `AppSidebar(.Group/.Item).vue`.
  5. Tema: extensão de `assets/css/main.css`.
  6. Componentes/composables compartilhados (`components/shared/*` + composables): DataTable, PageToolbar, FilterBar, AppDialog, ConfirmDialog, DeleteAlert, fields (TextField, SelectField, MoneyInput, QuantityInput, PercentInput, DateTimeField), EmpresaSelector, DanfeViewer, TransmissionOverlay, ToastHost, e composables useToast, useMask, useDocumento, useEnum, useHelper, useRealtime.

  **Bloqueio crítico atual:** as páginas existentes (`plataforma/admin/clientes/*`) hardcodam `http://localhost:5000`. O item 2 (baseURL configurável no cliente único) precisa existir e ser adotado antes de qualquer tela nova.
