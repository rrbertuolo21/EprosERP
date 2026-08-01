---
title: "Trilha QA — os 8 testes e o plano a partir dos ACs"
confluence_id: "192118787"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192118787/Trilha+QA+os+8+testes+e+o+plano+a+partir+dos+ACs"
last_updated: "2026-07-06"
---

> [!NOTE]
> **O que você vai aprender:** fundação de testes, Testcontainers, QA Agent e gates de CI — trilha para QA/SDET.

**Leitura prévia obrigatória:** [Multi-tenancy e os 8 testes](05-multi-tenancy-8-testes.md) e [16 agentes no Cursor](06-16-agentes-cursor.md).

> [!IMPORTANT]
> `TenantLeakTest` e `PCIDataMaskingTest` são **críticos** — reproduza em todo módulo novo antes do merge.

---

## Etapa 1 — Fundação de testes

### O que estudar

* Por que testes automatizados existem — custo do manual em escala (132 submódulos)
* xUnit — Arrange/Act/Assert, nomenclatura descritiva
* Tipos: unitário, integração, e2e — o que cada um protege

### Os 8 testes obrigatórios

| # | Teste | Prioridade para QA |
| --- | --- | --- |
| 1 | TenantLeakTest | **Crítica** — reproduzir em todo módulo |
| 2 | SoftDeleteFilterTest | Alta |
| 3 | LedgerAppendOnlyTest | Alta (financeiro) |
| 4 | AuditTrailTest | Média |
| 5 | OutboxDeliveryTest | Alta |
| 6 | PCIDataMaskingTest | Crítica (dados sensíveis) |
| 7 | EntitlementGateTest | Alta |
| 8 | PerformanceSLOTest | Média |

### Exercício prático

Ler `ContasAPagarSecurityTests.cs` linha a linha. Reproduzir estrutura em módulo novo.

---

## Etapa 2 — Testcontainers na prática

### O que estudar

* Testcontainers para .NET — PostgreSQL real sobe e desce por teste
* Fixtures de banco — seed, isolamento entre testes
* `IAsyncLifetime` — setup/teardown do container

### Exercício prático

**Escrever TenantLeakTest para módulo Vendas:**

1. Dois contextos com tenants diferentes
2. Seed no tenant B
3. Query no tenant A → deve retornar vazio
4. Mensagem de assertion clara (facilita debug no CI)

### Critério de conclusão

- [ ] Teste falha se remover QueryFilter (prova que protege)
- [ ] Container sobe em <30s no CI
- [ ] Documentado no PR o que o teste garante

---

## Etapa 3 — QA Agent e análise de ACs

### Fluxo com QA Agent

```
1. PO entrega US com ACs em Given/When/Then
2. Cole os ACs no QA Agent (Cursor)
3. Agente retorna plano de testes:
   - Casos manuais
   - Casos automatizáveis
   - Edge cases fiscais e multi-tenancy
4. QA implementa automatizáveis, dev apoia se necessário
```

### Edge cases fiscais a sempre considerar

> [!WARNING]
> Sempre cubra estes cenários no plano de testes: NF-e em contingência, precisão decimal (18,2), fuso UTC no backend e tenant sem módulo ativo (403).

### Exercício prático

Pegar ACs do Bloco 6 (Compras) e gerar plano completo com QA Agent. Implementar pelo menos 3 casos automatizados.

---

## Etapa 4 — Performance e regressão

### O que estudar

* PerformanceSLOTest — P95 leitura <200ms
* Suite de regressão — o que automatizar primeiro
* CI/CD — gates no GitHub Actions

### Meta de cobertura

| Escopo | Mínimo |
| --- | --- |
| Arquivos alterados no PR | 70% |
| Módulo novo completo | 80% |
| 8 testes de segurança | 100% (obrigatório) |

### Exercício prático

Configurar suite do Bloco 6 (Compras) para rodar no CI — build falha se qualquer teste de segurança quebrar.

---

## DoD do ponto de vista QA

Antes de aprovar uma US:

- [ ] 8 testes de segurança passando (se módulo novo ou alterado)
- [ ] Plano de testes do QA Agent executado
- [ ] Edge cases de multi-tenancy cobertos
- [ ] Fluxo manual validado (até automação completa)
- [ ] Sem P0/P1 abertos para a US

---

## Agentes IA para QA

| Momento | Agente |
| --- | --- |
| Após US pronta | QA Agent — plano de testes |
| Código suspeito | Security Agent |
| Documentar caso | Docs Agent |

---

**Trilha QA concluída.**

**Próximo passo →** [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md) — passo a passo hands-on de plano de testes, regressão e catálogo de edge cases no Cursor.

[Índice do Onboarding](README.md)
