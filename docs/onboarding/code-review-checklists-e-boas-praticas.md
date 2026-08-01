---
title: "Code Review — checklist do autor e boas práticas"
confluence_id: "147128324"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/147128324/Code+Review+checklists+e+guia+de+boas+pr+ticas"
last_updated: "2026-07-20"
---

> [!NOTE]
> **Para quem é:** dev backend e frontend **antes de abrir o PR**. Checklist humano do autor + retrocompatibilidade PWA. Gate do merge → [Tutorial Tech Lead](tech-lead/tutorial-tech-lead-arquiteto.md).

Code review não é desconfiança — é garantir que ninguém envia código à produção sem segunda verificação. No Epros, a sequência é **duas etapas**, sem revisor humano intermediário:

1. **Code Review Agent** (`/code-review`, skill S23) — padrão, tenancy, testes, segurança óbvia
2. **Tech Lead** — negócio, arquitetura e decisão de merge na `develop`

O usuário valida que a funcionalidade funciona (homolog). O Agent valida padrão. O Tech Lead confirma que o código está correto para o negócio.

---

## Fluxo antes do merge

```
Dev implementa
  → /code-review (Agent publica relatório no PR)
  → abre PR para develop
  → Tech Lead revisa e decide merge
```

Detalhe Git/Jira: [Fluxo de desenvolvimento — artigo 10](10-fluxo-de-desenvolvimento.md) · Agentes: [16 agentes no Cursor](06-16-agentes-cursor.md)

Após o **merge** do PR, o GitHub **apaga a branch head no remoto** (auto-delete). Retrabalho, task **Rejeitado** ou nova entrega → **branch nova** a partir de `develop` atualizada — não tente reutilizar a branch antiga.

---

## Checklist do autor

Use **antes** de solicitar aprovação do PR, depois de rodar o Code Review Agent e corrigir bloqueantes.

Itens de **código, segurança, contratos, testes e checks locais** foram transferidos para o `/code-review` (commands em `backend/` e `frontend/`). O autor só marca o processo humano abaixo e confirma o relatório do Agent.

### Processo

- [ ] Branch atualizada com rebase a partir da `develop`
- [ ] Descrição no Jira de como testar
- [ ] `/code-review` executado — relatório no PR, zero bloqueante 🔴, checks locais ✅

> Branch/título com `EP-xxx`, segurança, Swagger/contratos, tenancy/Outbox, cenários de teste e suite local são validados pelo Agent — ver relatório no PR.
---

## Retrocompatibilidade e o PWA

O front é um PWA. O navegador do cliente armazena em cache os arquivos do front e os serve localmente mesmo após o deploy de uma nova versão. Um cliente pode estar rodando uma versão do front de dias ou semanas atrás enquanto a API já foi atualizada. Se a API quebrou retrocompatibilidade, esse cliente recebe erro sem entender o motivo.

### O que quebra retrocompatibilidade

| Mudança | Impacto |
| --- | --- |
| Remover campo da resposta | Front tenta acessar campo que não existe mais |
| Renomear campo da resposta | Front lê valor indefinido onde esperava um dado |
| Tornar obrigatório campo que era opcional no envio | Front antigo não envia o campo — requisição rejeitada |
| Alterar formato de data, moeda ou enumerado | Front processa valor com formato errado |
| Mudar código de status HTTP de endpoint existente | Front trata como erro o que deveria ser sucesso |
| Alterar estrutura de objeto aninhado | Front não encontra propriedade no caminho esperado |

### O que não quebra retrocompatibilidade

* Adicionar campo novo e opcional na resposta — o front ignora o que não conhece
* Criar endpoint novo — o front antigo simplesmente não o usa
* Alterar lógica interna sem mudar contrato

### Como proceder quando a mudança quebra retrocompatibilidade

1. Alinhar com o front antes de implementar — a mudança precisa ser coordenada nos dois repos
2. Se não for possível coordenar, versionar o endpoint — manter o antigo funcionando e criar um novo com o contrato atualizado
3. Nunca alterar silenciosamente — qualquer mudança de contrato deve ser explícita no comentário da task e no PR

---

## Etiqueta do autor

* Não leve o feedback como crítica pessoal — o review olha para o código, não para você
* Se discordar de um comentário, argumente no PR ou Jira — não resolva só na conversa informal
* Responda todos os comentários antes de pedir nova revisão ao Tech Lead

---

## Perguntas frequentes (autor)

**Posso fazer merge do meu próprio PR?**
Não. Após o Agent, só o **Tech Lead** autoriza merge na `develop`.

**Posso pular o Code Review Agent?**
Não. É gate obrigatório antes do PR — ver [DoD no artigo 07](07-squads-cerimonias.md).

**O que faço se o Agent apontar bloqueante?**
Corrija, rode `/code-review` de novo e só então abra ou atualize o PR.

**Hotfix — preciso do Agent?**
Sim. Hotfix fura o ciclo semanal, **não fura o review** — Agent + Tech Lead. Ver [artigo 10](10-fluxo-de-desenvolvimento.md).

**O que fazer se o PR ficou grande demais?**
Quebre em PRs menores antes de abrir — facilita o Agent e a revisão do Tech Lead.

**Checklist do Tech Lead (gate do merge)?**
Ver [Tutorial Tech Lead — gate do PR](tech-lead/tutorial-tech-lead-arquiteto.md).

---

[Índice do Onboarding](README.md)
