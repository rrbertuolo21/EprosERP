<!--
TEMPLATE — Página TÉCNICA de um módulo (dev-wiki).
Copie para wiki/dev-wiki/<modulo>/README.md e preencha.
Regra de não-duplicação: LINKE para a fábrica (projetos/siser/iniciativas/plataforma/especificacoes/<MOD>/)
e para os arquivos reais do código (src/...). NÃO copie a regra nem o modelo de dados — referencie.
-->

# <Módulo> — Dev

> Fonte canônica (fábrica): `projetos/siser/iniciativas/plataforma/especificacoes/<MOD>/`
> Código: `src/Modules/Epros.Modules.<X>/` · API: `src/API/Epros.API/Controllers/`

## Visão técnica & fronteiras
<O que o módulo é tecnicamente, e onde começa/termina. O que é dele e o que é do vizinho.>

## Arquitetura
- **Contextos / schemas:** <ex.: `plataforma.*` em `Epros.Modules.<X>`>.
- **Entidades principais:** <lista curta> → modelo completo em
  [`especificacoes/<MOD>/MODELO_DADOS.md`](../../.. caminho da fábrica ..).
- **Base:** herança/convenções relevantes (ex.: `EntidadeSaaSBase`, RLS, Outbox).

## Endpoints
| Rota | Método | Auth | O que faz |
|---|---|---|---|
| `/api/v1/...` | GET/POST/... | `[AbacAuthorize("Recurso","Acao")]` / público | <...> |

Controllers reais: `src/API/Epros.API/Controllers/<...>Controller.cs`.

## Eventos & integrações
- **Outbox (publica):** `<Evento>Event` — <quando/por quê> — consumidor: `<...>`.
- **Cross-módulo:** <chamadas cruzadas, contratos compartilhados, o que o vizinho consome>.

## Regras / decisões implementadas
Não repita a regra — linke para a fonte:
- [`especificacoes/<MOD>/MANUAL...`](..) — o que foi construído + lógica + campos.
- [`especificacoes/<MOD>/DECISOES...`](..) / `EF...` — decisões fechadas (contrato).

## Como estender / gotchas
- <Padrão a seguir para adicionar X. Armadilhas conhecidas.>

## Estado & pendências
- **Estado:** <feito / parcial / quebrado — com evidência>.
- **Valida contador/advogado:** <parâmetros de negócio que dependem de validação humana>.
- **Dependências de ambiente:** <node/npm, credencial de gateway, migrations em banco persistente...>.
