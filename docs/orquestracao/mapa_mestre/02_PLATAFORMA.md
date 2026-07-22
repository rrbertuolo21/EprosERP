# Mapa Mestre — PLATAFORMA_COMPARTILHADA (20 submódulos não-fiscais)

> Reconciliação spec × código. Agente de mapa 02. Data 2026-07-22.
> (FATURAMENTO_FISCAL_ELETRONICO em doc 03. `Modules.DMS` = Dealer Mgmt, NÃO document mgmt → GED sem código.)

## Fundações transversais confirmadas (DONE)
- **Outbox** por módulo (`Shared/Domain/Events/OutboxMessage.cs` + `OutboxProcessorJob` em cada módulo, Quartz 10s em `Program.cs`).
- **Vault/Cofre** (`Infrastructure/Services/VaultEncryptionService.cs`, Transit + fallback AES-256-GCM) via `ISegredoCofreService`.
- **Multi-tenant/RLS + segurança** (`TenantRlsInterceptor`, middlewares Inquilino/Modulo/Inadimplencia, `Security/` AbacFilter/PermissaoMenuFilter/EprosToken).
- **Jobs/Scheduler** Quartz wired.
- **Offline/Sync primitives**: `EntidadeSaaSBase.SyncId/SyncVersion` + `ISyncable`.

## Tabela resumo

| Submódulo | Status | Já existe? | Tier | Gap #1 |
|---|---|---|---|---|
| ANALYTICS_E_MOBILIDADE | AUSENTE | só sync queries (Clientes/Produtos) | G | sem motor relatórios/dashboards/KPIs |
| API_GATEWAY_E_OPENAPI | PARCIAL | Program.cs Swagger/token, ApiKeyMiddleware, migration rate-limit | G | rate limit não aplicado no pipeline; sem versionamento OpenAPI |
| ASSINATURA_ELETRONICA | AUSENTE | "assinatura" = SaaS (AssinaturaCliente) | M | sem solicitação/evidência de e-assinatura documental |
| COMPLIANCE_LGPD_SOX_IFRS | PARCIAL | DataMaskingMiddleware, AuditMiddleware, MakerChecker | G | sem consentimento/DSAR, retenção/anonimização, IFRS |
| CONFIGURACAO | PARCIAL | SystemSetting, ConfiguracaoGlobal, ParametrosOperacionais | G | sem campos custom, templates e-mail, i18n |
| GESTAO_ELETRONICA_DE_DOCUMENTOS_GED | AUSENTE | nenhum (storage só fiscal) | G | GED inteiro (Documento/Arquivo/Pasta + storage genérico) |
| IA_ML | AUSENTE | nenhum | M | sem registry/inferência governada |
| IMPRESSAO_TERMICA | AUSENTE | só config NFC-e | M | sem serviço ESC/POS |
| INTEGRACAO_IOT | AUSENTE | nenhum | M | sem dispositivo/telemetria |
| INTEGRACOES_E_CONECTORES | SCAFFOLD | só webhook pagamento inbound | G | sem catálogo conectores / webhooks saída |
| INTERFACE_ASSISTIDA_WIZARDS | AUSENTE | só onboarding fixo | M | sem builder declarativo de forms/wizards |
| OFFLINE_SHELL | PARCIAL | SyncId/SyncVersion, SincronizarVendas/Caixas | M | sem fila offline/resolução conflito/idempotência |
| PLANEJAMENTO_IN_MEMORY | AUSENTE | nenhum | M | sem cenário/simulação |
| SDK_EXTENSOES | AUSENTE | nenhum | P | sem registry/manifesto extensões |
| SOA_COLABORACAO | SCAFFOLD | INotificacaoService mock | G | sem comentários/DM/timeline/templates |
| UPLOAD_E_MIGRACAO_DE_DADOS | PARCIAL | só XML fiscal (ImportacoesXml) | G | sem upload genérico/CSV/XLSX/dedup |
| WORKFLOW | PARCIAL | MakerCheckerPipelineBehavior, ExecucaoMassa | G | sem motor genérico wf_definicao/instancia/transicao |

## Direcionamento F1
- **WORKFLOW** e **UPLOAD/MIGRAÇÃO** são gargalos F1 → precisam de motor genérico (hoje hardcoded/só-fiscal).
- GED, Assinatura, Colaboração, Config genérica: F2+.
- IA/ML, IoT, In-Memory, SDK, Wizards, Analytics: F4/F5 (Onda 5-6).
