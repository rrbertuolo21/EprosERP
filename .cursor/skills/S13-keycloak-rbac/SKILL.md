---
name: S13-keycloak-rbac
description: >-
  Identidade e autorização do Epros com Keycloak 24: realms, claim tenantId no JWT, RBAC, Segregação de Funções (SoD) e revogação no desligamento. Use ao criar endpoints, definir permissões, revisar autenticação/autorização, ou debugar problemas de token, claim e acesso.
---

# keycloak-rbac

> **S13 · Camada 1 — Engenharia** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **Keycloak, autenticação, autorização, JWT, claim, permissão, perfil, RBAC, SoD, login, token, desligamento**.

## O que esta skill cobre

A camada de identidade do Epros com Keycloak 24: arquitetura de realms/clients, a claim tenantId no JWT (como é injetada e validada), criação de permissões RBAC, Segregação de Funções (SoD) e o fluxo de desligamento de colaborador com revogação de acessos.

Fazer com que autenticação e autorização sejam tratadas do jeito certo desde a spec: endpoint sem [Authorize] não passa, permissão nova segue o modelo, e eventos como ColaboradorDesligado disparam revogação automática.

## Instruções para o agente

1. Todo endpoint de negócio nasce com [Authorize] e verificação de permissão — consulte o checklist de endpoint novo.
2. A claim tenantId do JWT é a fonte do tenant no request — entenda o fluxo antes de mexer em auth.
3. Permissão nova: siga o modelo de nomenclatura e registre no catálogo de políticas.
4. JWT não carrega dado sensível no payload — valide isso em review.
5. Erros comuns (token expirado, claim ausente, CORS): consulte o troubleshooting antes de debugar do zero.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `checklists/endpoint-novo-auth.md` — checklist de segurança de endpoint
- ⬜ `exemplos/politicas-rbac.md` — catálogo de perfis e permissões existentes
- ⬜ `exemplos/troubleshooting-auth.md` — os 10 erros de auth mais comuns e correções

## Como completar esta skill (do v1-semente à versão completa)

1. Documente a configuração real do Keycloak (realms, clients, mappers da claim tenantId).
2. Exporte e catalogue os perfis/permissões existentes.
3. Trace o fluxo do evento ColaboradorDesligado → revogação e documente.
4. Compile o troubleshooting a partir dos tickets de auth já resolvidos.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
