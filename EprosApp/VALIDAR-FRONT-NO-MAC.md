# Validar o frontend no Mac (build real)

O ambiente da fábrica não tem `node` no PATH (o build do esbuild trava lá), então o
**typecheck/build real roda aqui no seu Mac**. A fábrica já deixou pronto:
- `openapi/swagger.json` **atualizado** a partir do backend novo (2.3 MB, 1.047 endpoints).
- Todo o **scaffolding compartilhado** e as **63 telas do núcleo**.
- As **telas dos módulos avançados** (RH, Produção, Manutenção, Qualidade, Projetos, GRC, ESG,
  Concessionárias, Imobiliária, Financeiro-avançado, Contabilidade) sob `pages/erp/<modulo>/`.

## Passo a passo

```bash
cd /Users/rafaelrbertuolo/Desktop/Projetos/Epros/EprosERP/EprosApp

# 1) dependências (se ainda não instalou nesta máquina)
npm install

# 2) regenerar os tipos a partir do swagger novo do backend
npm run api:generate        # openapi-typescript openapi/swagger.json -> types/api.d.ts

# 3) typecheck (Nuxt/Vue) — é aqui que erros de tipo aparecem
npx nuxi typecheck          # ou: npm run typecheck (se existir no package.json)

# 4) subir o app pra clicar nas telas
npm run dev                 # abre o Nuxt; navegue por /erp/<modulo>/...
```

## Se aparecerem erros de tipo

São esperados alguns ajustes finos (o build não pôde ser verificado na fábrica). O padrão de
todas as telas é o mesmo (clonado de `pages/erp/cadastros/contadores/{index,[id]}.vue`), então
um erro costuma se repetir igual em várias telas e a correção propaga. Me mande a saída do
`typecheck` que eu corrijo em lote.

## Backend (para o front ter com quem falar)

O backend sobe apontando pro Postgres. A `baseURL` do front vem de
`NUXT_PUBLIC_API_BASE_URL` (ver `nuxt.config.ts` / `.env.example`) — aponte para o host da API.
Os módulos avançados sobem **desabilitados** (ABAC nega por padrão); liberar por plano/cliente.
