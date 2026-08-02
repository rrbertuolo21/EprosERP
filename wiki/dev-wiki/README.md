# Epros — Wiki do Desenvolvedor

Documentação técnica versionada no repo. Cada módulo tem uma página que **linka** para a fonte canônica na fábrica
(`projetos/siser/iniciativas/plataforma/especificacoes/<MOD>/`) e para os arquivos reais do código — nunca copia
(ver [regra de não-duplicação](../README.md#regra-de-não-duplicação-inegociável)).

## Arquitetura geral

O Epros é um **monólito modular** em .NET 8 seguindo **Clean Architecture por módulo**, com front separado em Nuxt 3.

- **Monólito modular.** Um único deploy; cada área de negócio é um módulo isolado em
  `src/Modules/Epros.Modules.<X>/` (Domain / Application / Infrastructure), com fronteiras explícitas. Módulos sobem
  **desabilitados** (ABAC nega por padrão) e são liberados por plano/cliente.
- **Multi-tenant por `TenantId` + RLS Postgres.** Toda entidade herda `EntidadeSaaSBase`
  (`Id, SyncId, TenantId, SyncVersion, CriadoEm/Por, AlteradoEm/Por, DeletadoEm`) e é isolada por tenant.
  O isolamento é **fail-closed** via Row-Level Security no PostgreSQL — não depende só do filtro da aplicação.
- **CQRS / MediatR.** Comandos e queries passam por handlers (`Application/`), não por lógica no controller.
  O controller é fino: autoriza, despacha o command/query, devolve o resultado.
- **Outbox.** Efeitos colaterais e integrações saem por eventos gravados na mesma transação (padrão Outbox) e
  entregues por um processador — garante consistência entre a mudança de estado e a notificação/integração.
- **RBAC — Papel + Capacidade via `AbacFilter`.** Autorização é ABAC: cada endpoint declara `[AbacAuthorize("Recurso","Acao")]`;
  o `AbacFilter` cobra a **capacidade** (`recurso:acao`) que o papel do usuário concede. O menu é **projeção** dessas
  mesmas capacidades (fonte única) — não há tela sem gate correspondente.
- **EF Core + PostgreSQL.** Persistência via EF Core (provider Npgsql), schemas físicos por módulo, snake_case,
  soft-delete e concurrency token (`xmin` do Postgres) configurados no `ContextBase`.
- **Front Nuxt 3 + TypeScript** (`EprosApp/`). IO só via `useApi`/`useApiList`. Mobile em React Native (submódulo).

## Convenções

- **Nomes / código:** ver `CONVENCAO_CODIGO.md` na raiz do repo (Modo Porte vs. Consolidação) e as regras `.mdc` do
  Cursor em `docs/fabrica/cursor/cursor-install/rules/`. Controllers em `src/API/Epros.API/Controllers/<Area><Coisa>Controller.cs`,
  rota `api/v1/<area>/...`.
- **Migrations:** aditivas sempre que possível (ex.: `xmin` como concurrency token não exigiu migration). Nomeadas
  `Implanta_<versao>_<Descricao>`. Aplicadas no container de teste; apply em lote + smoke test em banco persistente é
  passo de fechamento.
- **Testes:** `dotnet test` com **Testcontainers** (Postgres real) — *"build verde ≠ testado"*. Regras críticas cobertas
  em banco real; suíte InMemory permanece para o caminho rápido. O "verde" é **entrada, não prova**: re-execute e cole a evidência.
- **O "verde" é entrada, não prova.** Nunca reporte build/testes sem re-executar e anexar comando + saída. Validar no
  ambiente vivo (banco real, chamada externa real). Ver `docs/fabrica/processo/RETROSPECTIVA-EPROSERP.md`.

## Módulos

| Módulo | Contexto(s) / schema(s) | Página | Fonte canônica (fábrica) |
|---|---|---|---|
| Aplicativo (control-plane SaaS) | `aplicativo.*`, `plataforma.*` | [dev-wiki/aplicativo](aplicativo/README.md) | `especificacoes/0_APLICATIVO/` |

> Cada módulo do ERP que fecha entra aqui com sua página técnica — entregável obrigatório do fechamento.
