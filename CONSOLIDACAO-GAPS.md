# Consolidação EprosERP — gaps dos originais ainda a portar

> Recon comparativo (3 codebases): `epros_erp-main` (ERP antigo), `epros_gestao_clientes-new-version`
> (admin SaaS) e o **novo** `EprosERP`. Feito em 2026-07-25. Fonte: `scratchpad/recon-*.md`.

## ✅ Já consolidado (Bloco 10)
- **Login de operador interno / SuperAdmin** (não existia no novo): `POST /api/v1/public/plataforma/login`
  contra `UsuariosInternos` (PBKDF2), token `tenantId="system"`, curto-circuito no `AbacFilter`.
  Admin: `admin@epros.local` / `Admin@12345` → acessa `/plataforma/superadmin/*`.
- **Front legado religado:** 9 arquivos (`plataforma/admin*`, `geografia`, `area-cliente/*`,
  `cadastro`, `PessoasTab`, `PerfisAcessoTab`) migrados de `$fetch http://localhost:5000` (sem token)
  para `useApi` (rota relativa + Bearer automático). `index.vue` liga o login admin ao endpoint novo.
  Keycloak hardcoded e `dashboard.vue.bak` removidos.

## ⏳ Gaps de backend a portar (próxima onda — "validar o back")
Priorizados pelo recon do ERP antigo. Nenhum bloqueia o uso atual; são features que o antigo tinha.

| Prio | Gap | Origem (antigo) | Estado no novo |
|---|---|---|---|
| ALTO | **DANFE / DANFCE** (impressão do documento fiscal) | FastReport `.frx` no `epros_erp-main` | só QuestPDF + PDF de NFS-e — falta o DANFE/DANFCE |
| ALTO | **Importação OFX + conciliação bancária** | `ImportacaoArquivoOfxController` + `OFXParser` | 0 no novo |
| MÉDIO | **Real-time do PDV (SignalR)** | microserviço `Epros.Erp.RealTime.API` | sem equivalente (só Outbox genérico) |
| MÉDIO | **Provider de transmissão NFS-e** | `OpenAC.Net.NFSe` | novo referencia `Hercules` — confirmar cobertura |
| MÉDIO | **`VendaReportsController`** (relatórios de venda) | antigo | sem equivalente dedicado |
| BAIXO | Paridade fina AP/AR, cartão/fatura, Município/País | antigo (controllers específicos) | consolidados em controllers guarda-chuva |

### Notas
- **Migração de senha:** usuários do antigo eram SHA-256 sem salt; novo é PBKDF2 → exigir troca no 1º login.
- **Auditoria do login SuperAdmin:** o handler novo não grava `HistoricoLogin`/lockout para operador
  interno (a entidade não tem os campos). Adicionar se quiser trilha de auditoria do admin.
- **Front admin em JS:** as telas `plataforma/admin*` seguem `<script setup>` JS (molde `clientes`).
  Converter para `lang="ts"` com build habilitado é um passo separado.
