---
title: "Tutorial — Guardião de Domínio (Fiscal)"
confluence_id: "200802305"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200802305/Tutorial+Guardi+o+de+Dom+nio+Fiscal"
last_updated: "2026-07-13"
---

**O que você entrega:** o dono do conhecimento tributário — nenhuma feature fiscal passa por spec/código/teste sem o seu ok.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

| Gatilho | O que fazer | Obrigatório em |
| --- | --- | --- |
| Dúvida de CFOP, NCM, CST, ST, regime | Fiscal Agent — bloco [Dúvida] | qualquer fase |
| Validação de spec com impacto fiscal | Fiscal Agent — bloco [Dúvida] | fase 03 Requirements |
| Validação de código que toca emissão/DFe | Fiscal Agent — bloco [Dúvida] | fase 07 Dev |
| Validação de cenários de teste fiscal | Fiscal Agent — bloco [Dúvida] | fase 08 QA |
| Rejeição da SEFAZ | Fiscal Agent — bloco [Rejeição SEFAZ] | transversal |
| SPED ou obrigação acessória | Fiscal Agent — bloco [SPED] | transversal |

---

## Pré-requisitos

* **Repositório:** abra o **epros-back** no Cursor.
* **Context Agent:** ativo automaticamente.
* **Contexto do tenant:** UF, regime (Simples/Presumido/Real), CNPJ quando relevante.

---

## Passo a passo

### Dúvida tributária

1. Abra um **chat novo**.
2. Execute `/fiscal`.
3. Cole o prompt abaixo.
4. **Anexe** spec, trecho de código ou cenário de teste, se houver.
5. **Saída esperada:** resposta com referência, impacto no Epros (campos, validações, eventos).
6. Se exigir interpretação de legislação, o agente sinaliza que precisa de validação do contador.

```
[Dúvida] {pergunta — ex: qual CFOP para devolução de compra interestadual?}
Contexto do tenant: UF {XX}, regime {Simples/Presumido/Real}.
Responda com base nos fundamentos fiscais e emissão NF-e, cite a referência,
e diga o impacto no Epros (campos, validações, eventos).
Se exigir interpretação de legislação, sinalize que precisa de validação do contador.
```

---

### Rejeição SEFAZ

1. Chat novo → `/fiscal`.
2. Cole o prompt abaixo.
3. **Anexe** XML, log ou payload da emissão, se disponível.
4. **Saída esperada:** diagnóstico — causa provável → correção → classificação (config do tenant / dado do documento / SEFAZ).

```
[Rejeição SEFAZ] Código {XXX}: "{mensagem}". Emissão de {NF-e/NFC-e}, UF {XX},
tenant {id}. Diagnostique pelo catálogo de rejeições: causa provável → correção →
é problema de configuração do tenant, de dado do documento ou da SEFAZ?
```

---

### SPED / obrigação acessória

1. Chat novo → `/fiscal`.
2. Cole o prompt abaixo.
3. **Saída esperada:** layout dos registros, prazos, mapeamento dado-do-Epros → registro, campos faltantes.

```
[SPED / obrigação acessória] Gerar/validar {SPED Fiscal / Contribuições / bloco XX}
para o tenant {id}, competência {MM/AAAA}. Use o layout dos registros, prazos,
e o mapeamento dado-do-Epros → registro. Aponte campos faltantes ou divergências.
```

---

### Validação nas fases da esteira (checklist)

| Fase | Quem chama | O que validar |
| --- | --- | --- |
| 03 Requirements | PO / Dev | impacto fiscal na US respondido |
| 07 Dev | Dev Backend | fluxo de emissão, campos, eventos |
| 08 QA | QA | cenários fiscais e edge cases |

**→ Handoff:** ok fiscal documentado → fase pode avançar.

---

## Seu gate (pronto quando…)

| Situação | Gate |
| --- | --- |
| Feature fiscal (spec/código/teste) | ok fiscal explícito antes de avançar |
| Rejeição SEFAZ | causa identificada + ação de correção definida |
| SPED | registros mapeados, sem divergência bloqueante |

**Dono do gate:** Guardião de Domínio (Fiscal).

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Dev implementar fiscal sem sua validação | Sempre acionar na fase 07 |
| QA marcar fiscal como ok sem cenários | Validar catálogo de edge cases |
| Ignorar contexto do tenant (UF/regime) | Sempre informar no prompt |
| Resumir rejeição SEFAZ sem código/mensagem | Colar código e mensagem exatos |
| Decisão de legislação sem contador | Sinalizar quando precisa validação externa |
