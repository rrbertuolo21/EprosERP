# DEPLOY-READINESS — subida para validação de negócio (1 semana)
> Estratégia do usuário (07-jul): **fechar o essencial → subir → negócio valida 1 semana → ajustes finos.** Este doc separa o que PRECISA estar pronto pra validar do que é **ajuste fino pós-validação**.

## ✅ Bloqueia a validação — TEM que estar pronto antes de subir
| Item | Frente | Estado |
|---|---|---|
| Segurança: API fechada, login funciona, RBAC, tenant | F1 | ✅ Feito (381 testes) |
| Fiscal núcleo: NF-e/NFC-e emitem/cancelam/CCe/inutilizam; NFC-e com QR | F3 | ✅ Feito |
| Certificado A1 (upload/validade) | F3/GestaoClientes | ✅ Existe |
| **Visual parecido com o legado** (tema claro, cores/fonte, componentes, telas core) | F2 | 🔧 Rodando |
| **Telas quebradas consertadas** (Devolução 404, Perfis-detalhe branco) | F2b | 🔧 Rodando |
| **Endpoints funcionais** (loaders de tabela fiscal, resolução CST por NCM) | F4 | ⏳ Fila |
| Build 0 erros, migrations aplicam em banco limpo, seeds (CFOP/CST) | F9-lite | ⏳ Gate |
| Smoke das jornadas críticas (login, venda NF-e/NFC-e, compra XML, financeiro) | F9-lite | ⏳ Gate |
| Dados NCM/CEST carregados (loader pronto na F4; dados = ambiente) | F4 + ambiente | ⏳ |

## 🔧 AJUSTE FINO — pós-validação (NÃO bloqueia a subida)
- Warnings → 0 (467, sendo 766 linhas no `External.DfeCalculos`).
- Testes de NFS-e/CT-e/MDF-e/venda; subir cobertura de smoke → regra.
- XML-doc em controllers/handlers + README por módulo.
- Refactor de god-files (LandingPageSettingsHandlers, ContextEstoque, VendaFiscalHandlers).
- **Contingência** fiscal (tpEmis/SVC/EPEC) — TODO honesto.
- **NFS-e/CT-e/MDF-e transmissão real** (OpenAC/Zeus) — homologação/ambiente.
- `string→enum` nos 8 módulos novos (estão em quarentena, fora da validação).
- Paridade visual das telas NÃO-core (moderno tolerado).
- Emissão single-shot p/ integração externa (só se algum cliente usar PDV/ByFood externo).

## 🚚 Depende de você / ambiente (não é código)
- Homologação SEFAZ por UF + certificado A1 de produção + CSC/Id-token NFC-e.
- Credenciais municipais NFS-e.
- Dados NCM/CEST (o loader entra na F4).
- **ETL do cutover** (long→Guid, SQL Server→PostgreSQL) — roda na virada real, depois da validação.

## Sequência até "pode subir"
1. Gate rodada-1 pousa (build+migrations+testes). 2. F2 (visual) pousa. 3. F4 (endpoints) + F8-lite (só estabilidade) pousam. 4. **Gate de prontidão** (build 0 erros, migrations limpas, smoke verde) → **libera a subida**. 5. Negócio valida 1 semana. 6. Ajustes finos da lista acima.
