---
name: S14-seguranca-lgpd
description: >-
  Segurança e LGPD aplicadas ao Epros: OWASP Top 10 com exemplos C#/EF Core, catálogo de dados pessoais e mascaramento, secrets via Vault, e o conflito direito de exclusão LGPD × guarda fiscal de 5 anos. Use em todo review de segurança, tratamento de dado pessoal, gestão de secrets ou análise de vulnerabilidade.
---

# seguranca-lgpd

> **S14 · Camada 1 — Engenharia** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **segurança, vulnerabilidade, OWASP, LGPD, dado pessoal, CPF, mascaramento, secret, Vault, injeção, XSS, exposição de dados**.

## O que esta skill cobre

Segurança aplicada ao stack Epros: OWASP Top 10 com exemplos em C#/EF Core (não genéricos), o catálogo de dados pessoais do sistema com regras de mascaramento (DataMaskingMiddleware), LGPD operacional (bases legais, retenção, o conflito direito de exclusão × guarda fiscal de 5 anos) e gestão de secrets com Vault.

Transformar o checklist de segurança do Security Agent em conhecimento profundo e específico do Epros, tratando dados fiscais e financeiros com sensibilidade máxima e garantindo que secret nunca chegue ao código.

## Instruções para o agente

1. Review de segurança: siga o checklist completo — autenticação, dados/LGPD, código — e reporte no formato vulnerabilidade/severidade/localização/correção.
2. Dados pessoais (CPF, CNPJ PF, telefone, endereço) nunca em log ou mensagem de erro — consulte o catálogo de campos e o mascaramento.
3. Secret: só Vault ou variável de ambiente. Secret em código/appsettings de produção é bloqueante.
4. Lembre o conflito LGPD × fiscal: exclusão de dados convive com guarda obrigatória de XML por 5 anos — a resposta é anonimização parcial, não deleção.
5. Vulnerabilidade crítica/alta: nunca aceite 'corrigimos depois'.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `checklists/security-review.md` — checklist expandido do Security Agent
- ⬜ `exemplos/mascaramento-campos.md` — campo a campo: o que mascarar e como
- ⬜ `exemplos/vulnerabilidades-corrigidas.md` — casos reais do projeto, anonimizados

## Como completar esta skill (do v1-semente à versão completa)

1. Reescreva o OWASP Top 10 com exemplos do stack (EF Core parametrizado, [Authorize], erros sem stacktrace).
2. Mapeie onde vive cada dado pessoal no modelo (por módulo) e as regras do DataMaskingMiddleware.
3. Documente o uso do Vault (dynamic secrets, rotação) com exemplos de configuração.
4. Valide a seção LGPD com o responsável legal/DPO.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
