---
name: S27-migracao-legado
description: >-
  Migração dos 20 clientes legados do Epros: de-para long→Guid, ETL por módulo, conciliação de saldos financeiro/estoque que bloqueia o corte, migração de XMLs históricos e estratégia de convivência legado/novo. Use ao planejar, executar ou validar qualquer migração de cliente (Bloco 7).
---

# migracao-legado

> **S27 · Camada 3 — Especialização** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **migração, legado, de-para, long para Guid, importar dados, conciliação, corte, convivência, Bloco 7**.

## O que esta skill cobre

A metodologia de migração dos 20 clientes do Epros.ERP legado: tabela de-para long→Guid preservando relacionamentos, ETL por módulo, validação de integridade (contagens e somas de controle), conciliação de saldos financeiro/estoque, migração dos XMLs históricos para o MinIO e a estratégia de convivência (novo ativo, legado read-only até validar).

Executar o Bloco 7 — a parte mais delicada do projeto — sem perder dado e sem downtime perceptível: cada cliente é um tenant isolado, migra em janela própria e só corta quando os saldos batem.

## Instruções para o agente

1. Migração é por cliente, nunca big-bang: siga o plano-template (ensaio em staging → janela → convivência → validação → corte).
2. A tabela de-para (id_legado long → Guid) é o coração — toda entidade migrada registra o mapeamento para preservar FKs.
3. Conciliação bloqueia o corte: saldos de CP, CR e estoque têm que bater entre legado e novo. Diferença = investigar, nunca ajustar na mão.
4. XMLs fiscais históricos migram para o MinIO mantendo a trilha de guarda legal (5 anos).
5. Primeiro cliente: o menor e mais simples — é o ensaio geral que calibra tempo e riscos.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `templates/plano-migracao-cliente.md` — plano por cliente
- ⬜ `checklists/conciliacao-pos-migracao.md` — o que conferir antes do corte
- ⬜ `exemplos/depara-financeiro.md` — de-para real do módulo financeiro

## Como completar esta skill (do v1-semente à versão completa)

1. Escreva a metodologia geral a partir da estratégia já definida (B3.4).
2. Construa o de-para do financeiro (CP/CR) como piloto — módulos mais maduros no novo sistema.
3. Defina as queries de conciliação (somas de controle) por módulo.
4. Ensaie com o menor cliente em staging e incorpore os aprendizados à skill.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
