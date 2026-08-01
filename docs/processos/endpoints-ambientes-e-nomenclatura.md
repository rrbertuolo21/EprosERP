---
title: "Endpoints — ambientes e nomenclatura"
confluence_id: "146735106"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/146735106/Endpoints+ambientes+e+nomenclatura"
last_updated: "2026-04-28"
---

Este documento define os endpoints oficiais de cada serviço nos três ambientes: produção, homologação e desenvolvimento.

Em caso de dúvida sobre qual URL usar em cada ambiente, consulte aqui primeiro.

---

## Padrão de nomenclatura

O padrão adotado é `serviço.ambiente.siser.com.br`. Em produção o ambiente é omitido, ficando apenas `serviço.siser.com.br`.

| Ambiente | Padrão | Acesso |
| --- | --- | --- |
| Produção | `servico.siser.com.br` · `servico.epros.prosis.com.br` | Usuários finais |
| Homologação | `servico.homolog.siser.com.br` | Validação de negócio |
| Desenvolvimento | `servico.dev.siser.com.br` | Time de desenvolvimento |

---

## Endpoints por serviço

### Front (PWA)

| Ambiente | Domínios |
| --- | --- |
| Produção | `app.siser.com.br` · `app.epros.prosis.com.br` |
| Homologação | `app.homolog.siser.com.br` |
| Desenvolvimento | `app.dev.siser.com.br` |

### API principal

| Ambiente | Domínios |
| --- | --- |
| Produção | `api.siser.com.br` · `api.epros.prosis.com.br` |
| Homologação | `api.homolog.siser.com.br` |
| Desenvolvimento | `api.dev.siser.com.br` |

### API WebSocket (RealTime)

| Ambiente | Domínios |
| --- | --- |
| Produção | `ws.siser.com.br` · `ws.epros.prosis.com.br` |
| Homologação | `ws.homolog.siser.com.br` |
| Desenvolvimento | `ws.dev.siser.com.br` |

### API Dfe

| Ambiente | Domínios |
| --- | --- |
| Produção | `dfe.siser.com.br` · `dfe.epros.prosis.com.br` |
| Produção Versão Estável | `dfe-stable.siser.com.br` · `dfe-stable.epros.prosis.com.br` |
| Homologação | `dfe.homolog.siser.com.br` |
| Desenvolvimento | `dfe.dev.siser.com.br` |

---

## Regras de uso

* Código em `develop` aponta exclusivamente para endpoints `*.dev.siser.com.br`
* Código em `homolog` aponta exclusivamente para endpoints `*.homolog.siser.com.br`
* Código em `main` aponta exclusivamente para endpoints de produção
* Variáveis de ambiente controlam qual endpoint é usado em cada build — nunca hardcoded no código

---

## Adicionando um novo serviço

Se surgir uma nova API, o padrão já define o nome:

```
servico.siser.com.br          (produção)
servico.homolog.siser.com.br  (homologação)
servico.dev.siser.com.br      (desenvolvimento)
```

O nome do serviço deve ser curto, em minúsculas, sem referência ao ambiente ou ao produto interno.
