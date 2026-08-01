---
title: "PostgreSQL 16 — banco de dados"
confluence_id: "192708624"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192708624/PostgreSQL+16+banco+de+dados"
last_updated: "2026-07-06"
---

**Versão fixada:** `16.x`

### Por que PostgreSQL vs SQL Server

| Critério | SQL Server (legado) | PostgreSQL 16 (novo) |
| --- | --- | --- |
| Licença | Comercial — cresce com usuários | MIT — zero custo |
| Open source | Não | Sim |
| RLS (Row Level Security) | Limitado | Nativo e robusto |
| Extensões úteis | Limitadas | pgvector, pgcrypto, pg_trgm |
| On-premise para Enterprise | Licença cara | Grátis |
| Cloud-agnostic | Parcialmente | Totalmente |

### Estrutura de schemas por macrodomínio

```sql
-- Cada macrodomínio tem schema próprio
-- Módulos não fazem JOIN entre schemas — só conversam por eventos

CREATE SCHEMA IF NOT EXISTS plataforma;  -- GestaoClientes, DFe, Auditoria
CREATE SCHEMA IF NOT EXISTS financas;    -- CP, CR, GL, Ativos
CREATE SCHEMA IF NOT EXISTS vendas;      -- Venda, PDV, CRM
CREATE SCHEMA IF NOT EXISTS estoque;     -- Produto, Compra, WMS
CREATE SCHEMA IF NOT EXISTS producao;    -- OP, BOM, MRP
CREATE SCHEMA IF NOT EXISTS rh;          -- Colaborador, Folha
CREATE SCHEMA IF NOT EXISTS qualidade;   -- Inspeção, NCR
CREATE SCHEMA IF NOT EXISTS manutencao;  -- OS, Equipamentos
CREATE SCHEMA IF NOT EXISTS projetos;    -- PPM, Gantt
CREATE SCHEMA IF NOT EXISTS grc;         -- Risco, SoD
CREATE SCHEMA IF NOT EXISTS esg;         -- Carbono, CSRD
CREATE SCHEMA IF NOT EXISTS concessionarias; -- DMS, F&I
```

### Ledger imutável por trigger

```sql
-- O ledger contábil NUNCA pode ser alterado ou deletado
-- Garantia no banco — não depende de disciplina da aplicação

CREATE OR REPLACE FUNCTION financas.proteger_lancamento_contabil()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' THEN
        RAISE EXCEPTION 'Lançamentos contábeis são imutáveis (LGPD/Auditoria)';
    END IF;
    IF TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Lançamentos contábeis não podem ser deletados (LGPD/Auditoria)';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_proteger_lancamento
    BEFORE UPDATE OR DELETE ON financas.lancamento_contabil
    FOR EACH ROW EXECUTE FUNCTION financas.proteger_lancamento_contabil();

-- O LedgerAppendOnlyTest prova que o trigger funciona:
-- [Fact] Tentar UPDATE em lancamento_contabil lança exception
-- Este teste BLOQUEIA o deploy se falhar
```
