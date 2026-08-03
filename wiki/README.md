# Wiki Viva do Epros

Documentação **versionada no próprio repositório de código** — vive e evolui junto com o produto.
Não é um manual solto num Google Docs que envelhece: mora aqui, entra por **Pull Request** e é
**entregável obrigatório do fechamento de cada módulo**.

## Duas audiências, dois idiomas

| | **user-wiki** (`wiki/user-wiki/`) | **dev-wiki** (`wiki/dev-wiki/`) |
|---|---|---|
| **Para quem** | Usuários e operadores (Siser e clientes do ERP) | Desenvolvedores da fábrica / mantenedores |
| **Linguagem** | Negócio, zero jargão. "Como cobrar a mensalidade", não "webhook do gateway" | Técnica: contextos, schemas, endpoints, eventos, decisões |
| **Responde** | *O que faço com isto? Como uso?* | *Como funciona? Onde está no código? Como estendo?* |
| **Fonte** | O comportamento do produto (MANUAL da fábrica, telas) | Código real + a tríade EF/MC/MANUAL canônica |

Cada módulo tem, no máximo, **duas páginas**: uma em `user-wiki/<modulo>/` e uma em `dev-wiki/<modulo>/`.

## Como a wiki se mantém viva

1. **Versionada no repo.** Muda junto com o código, no mesmo commit/branch. Diff de wiki é revisável como diff de código.
2. **Atualizada por PR.** Toda alteração de comportamento que o usuário percebe, ou de contrato técnico,
   atualiza a página correspondente **no mesmo PR**. Wiki desatualizada é bug — trate como tal.
3. **Entregável no fechamento de módulo.** Quando um módulo fecha (todos os submódulos liberados), gerar/atualizar
   `user-wiki/<modulo>` e `dev-wiki/<modulo>` é item obrigatório do gate de saída do Orquestrador de Entrega
   (`agentes/20-orquestrador-de-entrega.md`, seção "Fechamento de MÓDULO"). Sem wiki, o módulo não é "liberado".

## Regra de não-duplicação (inegociável)

**A wiki NÃO copia conhecimento — ela LINKA.** Fato duplicado é bug (princípio da fábrica).

- A **dev-wiki** aponta para a fonte da verdade canônica na fábrica:
  `projetos/siser/iniciativas/plataforma/especificacoes/<MOD>/` (MANUAL, MANUAL_CENTRAL, EF, MC, DECISOES, MODELO_DADOS).
- A **dev-wiki** também aponta para os **arquivos reais do código** (controllers, entidades, handlers, middlewares) — caminho `src/...`.
- Ela não reescreve a regra de negócio nem o modelo de dados: **referencia** onde ele vive e **resume a orientação**
  (o que existe, como se conecta, como estender). Se um fato precisa ser citado, vem da skill de negócio ou do MANUAL/EF canônico.
- A **user-wiki** descreve o produto em linguagem de usuário; quando precisa da regra exata, também remete ao MANUAL canônico.

> Se você se pegar copiando um parágrafo do MANUAL para dentro da wiki, pare: coloque o link.

## Estrutura

```
wiki/
├── README.md                     ← este arquivo
├── _TEMPLATE-USER-MODULO.md      ← template da página de usuário
├── _TEMPLATE-DEV-MODULO.md       ← template da página técnica
├── user-wiki/
│   ├── README.md                 ← índice + "o que é o Epros"
│   └── <modulo>/README.md        ← 1 página por módulo (usuário)
└── dev-wiki/
    ├── README.md                 ← índice + arquitetura geral + convenções
    └── <modulo>/README.md        ← 1 página por módulo (técnico)
```

## Como criar a wiki de um módulo novo

1. Copie `_TEMPLATE-USER-MODULO.md` para `user-wiki/<modulo>/README.md` e preencha em linguagem de usuário.
2. Copie `_TEMPLATE-DEV-MODULO.md` para `dev-wiki/<modulo>/README.md` e preencha, **linkando** para a fábrica e para os arquivos reais.
3. Adicione o módulo aos índices (`user-wiki/README.md` e `dev-wiki/README.md`).
4. Abra o PR junto com o fechamento do módulo.

Exemplar de referência já preenchido: módulo **APLICATIVO** (control-plane SaaS) —
`user-wiki/aplicativo/` e `dev-wiki/aplicativo/`.
