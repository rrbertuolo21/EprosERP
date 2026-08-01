# Code Review — publicar comentário via CI (GitHub App)

Documentação canônica do fluxo `/code-review` → comentário no PR como **epros-code-review-agent[bot]**, sem PAT de bot nas máquinas dos devs.

Repositórios: `SISER-PROSIS/epros`, `SISER-PROSIS/epros_erp`, `SISER-PROSIS/epros_erp_front`.

---

## Problema

Publicar com `gh pr comment` usa a conta do desenvolvedor autenticado no `gh`.

## Solução

1. Cursor gera o relatório (chat + arquivo local).
2. O agent dispara o workflow `code-review-comment.yml` com `pr_number` + `body`.
3. O workflow usa secrets `CODE_REVIEW_APP_ID` e `CODE_REVIEW_APP_PRIVATE_KEY` e posta o comentário via GitHub App.

Script de disparo (JSON multilinha, Windows-friendly):

| Repo | Script |
|------|--------|
| Meta `epros` | `node scripts/dispatch-code-review-comment.mjs` |
| Frontend | `node scripts/dispatch-code-review-comment.mjs` |
| Backend | `pwsh -File .github/scripts/dispatch-code-review-comment.ps1` (PowerShell + `gh`; sem Node local) |

Uso comum: `--pr <N> --body-file .cursor/tmp/code-review-pr-<N>.md` e, se o cwd não for o repo do PR, `--repo SISER-PROSIS/<repo>`.

Limite conservador do body: ~55 KB (exit code 2 → fallback).

## Ops — criar GitHub App (uma vez)

Caminho: org **SISER-PROSIS** → Settings → Developer settings → GitHub Apps → **New GitHub App**.

### Formulário Create GitHub App

**Basic information**

- **GitHub App name:** `epros-code-review-agent`
- **Description:**

```text
Publica o relatório do /code-review (Epros Dev Framework / S23) como comentário em Pull Requests.
Usado apenas por GitHub Actions via installation token — sem OAuth de usuário e sem webhooks.
Repositórios: epros, epros_erp, epros_erp_front.
```

- **Homepage URL:** `https://github.com/SISER-PROSIS/epros`

**Identifying and authorizing users**

- **Callback URL:** vazio
- **Expire user authorization tokens:** marcado (default)
- **Request user authorization (OAuth) during installation:** desmarcado
- **Enable Device Flow:** desmarcado

**Post installation**

- **Setup URL:** vazio
- **Redirect on update:** desmarcado

**Webhook**

- **Active:** **desmarcado**
- **Webhook URL / Secret:** não preencher

**Repository permissions**

- **Metadata:** Read-only
- **Pull requests:** Read and write
- Demais: **No access**

**Organization / Account permissions:** todas **No access**

**Subscribe to events:** nenhuma

**Where can this GitHub App be installed?** **Only on this account**

### Após Create

1. **App ID** → secret org/repo `CODE_REVIEW_APP_ID`
2. **Generate a private key** → secret `CODE_REVIEW_APP_PRIVATE_KEY` (conteúdo completo do `.pem`)
3. **Install App** em `epros`, `epros_erp`, `epros_erp_front` (Only select repositories)
4. Smoke: Actions → workflow **Code Review — publicar comentário** → Run workflow

Nunca commitar o `.pem` nem secrets em docs.

## Fallback

Se dispatch falhar (secrets ausentes, body grande, Actions off):

1. Manter relatório no chat
2. `gh pr comment <url> --body-file <arquivo>` (conta do dev), ou
3. Markdown para colar manualmente no PR

## Segurança

- Workflow valida PR no mesmo repositório (`gh pr view`); inputs não aceitam owner/repo arbitrário.
- `permissions:` mínimas no workflow.
- Não logar o body completo no Actions.

## Referências

- Workflow: `.github/workflows/code-review-comment.yml` (em cada repo)
- Commands: `.cursor/commands/code-review.md` (seção **Publicar no PR**)
- Skill: `.cursor/skills/S23-code-review-epros/SKILL.md`
