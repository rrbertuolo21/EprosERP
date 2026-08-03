# Governança da taxonomia Jira — EP

## Princípios

1. **Domínio** (cascading) = taxonomia permanente de produto (17 módulos · 132 submódulos).
2. **Epic** = iniciativa temporária com encerramento — nunca bucket de módulo.
3. **Componente** = capacidade técnica transversal — nunca menu funcional.
4. **Team** = quem executa (Back-End / Frontend) — nunca domínio de negócio.
5. **Labels** = marcadores temporários (`reforma-tributaria`, `divida-tecnica`) — nunca substituem taxonomia.

## DoR — entrada no sprint

Toda issue deve ter antes de entrar na sprint:

- [ ] **Domínio** preenchido (pai + filho, ou TRV)
- [ ] **Team** definido
- [ ] Estimativa (Story Points)
- [ ] Épico vinculado **somente** se fizer parte de iniciativa finita

## Responsável pela taxonomia

| Papel | Responsabilidade |
|---|---|
| **Tech Lead** | Aprovar novos códigos; revisão trimestral |
| **PO/Facilitador** | Garantir DoR no refinamento |
| **Planning Agent (S20)** | Preencher Domínio ao publicar backlog |

## Fluxo de alteração

```
1. Alterar mapa de produto (Confluence / onboarding)
2. Regenerar taxonomia-modulos-submodulos.json
3. Atualizar opções cascading no Jira (campo Domínio)
4. Comunicar no #epros-produto
5. Revisar de/para se componentes legados forem afetados
```

**Proibido** inventar código no Jira sem atualizar a fonte canônica.

## Revisão trimestral (checklist)

- [ ] Valores de Domínio sem uso nos últimos 90 dias
- [ ] Épicos abertos > 90 dias sem progresso — renomear, fechar ou replanejar
- [ ] Épicos com nome de módulo (`Financeiro`, `Estoque`) — eliminar
- [ ] Issues abertas com `"Domínio" is EMPTY` — zerar
- [ ] Componentes funcionais ainda usados em issues novas — corrigir

## Anti-padrões

| Anti-padrão | Correção |
|---|---|
| Epic "Financeiro FRONT" | Fechar; usar Domínio `FIN — Financeiro > FIN-…` |
| Componente "Vendas - Emissão NFe" em issue nova | Domínio `VEN — Vendas > VEN-GPE-001 — …` + Componente Backend - DFe |
| Label `modulo-financeiro` | Campo Domínio |
| 132 Quick Filters | Filtros salvos + dashboard por módulo em foco |
| Epic sem critério de encerramento | Adicionar DoD do épico ou fechar |

## Referências

- [README.md](README.md) — catálogo
- [configuracao-campos-jira.md](configuracao-campos-jira.md) — setup
- [auditoria-epicos-ativos.md](auditoria-epicos-ativos.md) — saneamento de épicos
- Skill S20 — metadados obrigatórios **Domínio** (cascading)
