# MAPA MESTRE — Status real dos 132 submódulos (reconciliação spec × código)

> Consolidado da Fase 0. Data 2026-07-22. Detalhe por grupo em `mapa_mestre/01..08_*.md`.
> **Status do `.xlsx` é otimista** — esta reconciliação mede contra a EF completa (mais rígida).

## Panorama agregado (aprox.)

| Status | Qtd aprox. | Leitura |
|---|---|---|
| **DONE** (fiel à EF) | ~10 | EST-PR, VEN-GP, COM-GC, CAD-GEO, CAD-PRM + núcleo fiscal NF-e/NFC-e (emissão/cancel/CCe/inut/motor/cadastros/IBPT) |
| **PARCIAL** (núcleo existe, falta EF) | ~22 | VEN-PDV, FIN-CP/CR(core), FIN-CGL, FIN-Tesouraria, CAD-PEM, EST-SC/MVM/CEX/TMS, APP-TEN-*, PLT-WF/UPL/Config/Offline/Compliance/API, vários MAN/PRJ/GRC/PRD/QLD/RH |
| **SCAFFOLD** (só esqueleto) | ~14 | PLT-Integrações/Colaboração, DMS-Garantias/Vendas/Manut, ESG-Carbono/Relatórios, QLD-ACR/INS, RH-WFM/PNT, EST-LDE, VEN-FCI/LDS, COM-FCI |
| **AUSENTE** (0 entidade) | ~86 | maioria de RH/PRD/QLD/MAN/PRJ/GRC/ESG/DMS + FIN(8 subs) + PLT(GED/IA/IoT/SDK/Wizards/Assinatura/Analytics/InMemory) + VEN(CRM/e-com/demanda/contratos/serviços/portais/garantias) + EST(WMS/INV/RLT/APE/Portal/GCC/Sub) + IMO + RPT(BI/Operacionais) + fiscal(SPED/Sintegra/Devolução/ManifestoDFe/CF-e-SAT/Contingência) |

**Conclusão:** o núcleo transacional (Nível 0-1: Produto, Venda, PDV, Compra, CP/CR, NF-e/NFC-e) está funcional e **já destrava** Vendas/Compras — o "gargalo fiscal" do `.xlsx` está, na prática, resolvido para emissão. O grande volume de trabalho é **construção nova** (86 ausentes) nos módulos de gestão/compliance/verticais, mais completar governança de fundação (CAD-PEM, RBAC, Workflow, Upload).

## Fundações prontas (reutilizáveis por todos)
Outbox por módulo · Vault/Cofre · multi-tenant/RLS + Security (ABAC/menu/token) · Quartz · Sync primitives (SyncId/SyncVersion) · Motor fiscal (DfeCalculos) · eventos wired: VendaFaturada, CompraLancada, FolhaProcessada, OrdemProducaoEncerrada, InspecaoReprovada, ProjetoFaturado.

---

## Backlog F1 — Fundação & Gargalos (Nível 0, alto leverage)

Organizado por **módulo-dono** (paralelismo seguro = agentes em módulos disjuntos; cada um edita só o `Context` do seu módulo; migrations congeladas → passe serial no fim).

### Frente A — GestaoClientes (dono de Pessoa + RBAC + Menu + catálogo)
- **CAD-PEM** governança: consentimento_titular, solicitacao_titular (DSAR), regra_deduplicacao, candidato_duplicata, pessoa_historico_estado, pessoa_log_auditoria, pessoa_importacao_lote/linha, identificador_fiscal, relacionamento_parceiro, empresa_grupo, extensões fornecedor/comprador/contador.
- **CAD-PEM eventos**: publicar `pessoa.criada/atualizada/inativada/mesclada/anonimizada` (REG-PEM-160/161) via Outbox.
- **APP-TEN-003 RBAC domínio**: papel, capacidade, usuario_papel, nivel_usuario, preco_nivel_usuario (estende PerfilAcesso).
- **APP-TEN-008 Menu**: endpoint "acessos do usuário" (AcessosResponse) + CRUD catálogo de menu.
- **APP-CAT**: enforcement de `IGlobalEntity` + Funcionalidade/add-on + resolução de módulos ativos.

### Frente B — Aplicativo (dono de identidade/tenant/super-admin/plataforma-core)
- **PLT-WF** motor genérico: wf_definicao, wf_instancia, wf_transicao, wf_tarefa + contrato de aprovação (substituir MakerChecker hardcoded). Publica `AprovacaoSolicitada/Concluida`.
- **PLT-UPL** upload/migração genérico: upload em partes, CSV/XLSX, storage dedup, job de importação (substituir só-XML-fiscal).
- **APP-TEN-002** onboarding → encadear criação de Cliente SaaS (RF-6.6).
- **APP-TEN-010** super-admin: fluxo upgrade/governança de versão.

### Frente C — Fiscal (pure-code, sem bloqueio externo)
- **DEVOLUCAO_FISCAL**: fluxo/estados/numeração de devolução.
- **Contingência NF-e/NFC-e**: lógica tpEmis/SVC-AN/SVC-RS/EPEC.
- *(SPED/Sintegra/Manifesto-DFe/CF-e-SAT → F2; NFS-e/CT-e/MDF-e transmissão real → dependem de homologação/credenciais = tarefa humana.)*

## Contratos cross-module a publicar ANTES dos consumidores
1. **Eventos `pessoa.*`** (GestaoClientes → futuros consumidores).
2. **RBAC** papel/capacidade (GestaoClientes) — Usuario(Aplicativo) referencia via UsuarioPapel (Guid).
3. **Workflow** `AprovacaoSolicitada/AprovacaoConcluida` (Aplicativo) — qualquer módulo pede aprovação por alçada.
4. **Import genérico** contrato de job (Aplicativo) — reusado por migração dos 20 clientes (PLT-UPL/Bloco 7).

## Regra de execução F1
Cada frente = 1 agente, módulo disjunto. Seguir `CONVENCAO_CODIGO.md` à risca (EntidadeSaaSBase, Flunt Validar, private set, enums `E*`, mapping inline no Context do módulo, CQRS records + handlers assembly-scan, controller fino, xUnit). **NÃO** rodar `dotnet ef migrations add` nem `dotnet build` (evita lock/race) — orquestrador faz build+migrations serial no fechamento da F1.
