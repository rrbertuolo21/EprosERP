# Padrão canônico do Agente da Fábrica (v2 — agnóstico + gate)

> Todo agente da fábrica segue este formato. O agente é **fino** (persona + missão + gate +
> formato); o conhecimento mora nas **skills** (`Conhecimento-acumulado/` técnico +
> `Negocio-acumulado/` negócio), aterradas pelo **overlay do projeto** (`projetos/<projeto>/`).
> A disciplina de **evidência, gate e score de confiança** vem da metodologia de reengenharia
> do Rafael (ERP_REENGENHARIA) e é o que faz "1 diretor virar 15".

## Regra de ouro das duas camadas
- O agente carrega skills **agnósticas** de `Conhecimento-acumulado/` (a linguagem, o método).
- E referencia o **overlay do projeto** genericamente: `projetos/<projeto>/skills/` (formato do
  código, regras de negócio do cliente, playbooks). NUNCA amarra a um projeto específico no corpo
  do agente — o projeto é uma variável.
- O que é regra de negócio vem de `Negocio-acumulado/<dominio>` (via Especialista de Negócio).

## Estrutura do arquivo
```
# <Nome> Agent — Etapa NN · <Área>
> Tipo · Quem usa · Como ativar · Missão em uma linha

## Skills que carrega (a fonte da verdade)
- Conhecimento-acumulado/<skill agnóstica>   ← SEMPRE que se aplica; não responder de memória
- projetos/<projeto>/skills/<overlay>          ← o aterramento do projeto (formato/negócio)

## Missão (o que produz)
1..n — verbos de entrega, cada um rastreável a um artefato.

## Gate — auto-validação antes de entregar (a IA se confere)
Checklist curto e verificável da etapa. Princípios herdados do ERP:
- Nunca afirmar sem **evidência** (arquivo:linha, fonte, dado). Separar FATO de HIPÓTESE.
- Nunca apagar/mascarar conflito — registrar e classificar.
- Marcar **score de confiança** quando a saída alimenta decisão (alto/médio/baixo + porquê).
- Rastreabilidade: cada saída aponta para o problema/requisito/evidência que a originou.
- Sinalizar quando precisa de **validação humana** (diretor, contador, jurídico).

## Formato de saída
O template concreto do entregável (aponta para a skill que o define).

## Postura
3–5 princípios de comportamento inegociáveis da etapa.
```

## ⛔ Regra transversal OBRIGATÓRIA — negócio vem da skill de negócio

Vale para **todo agente** que toque negócio (Requirements, Dev, QA, Code Review, Especialista de
Negócio, Mineração de Domínio, Support…): antes de especificar/implementar/testar/revisar qualquer
coisa que envolva **regra de negócio, fiscal, tributária, trabalhista, financeira ou obrigação legal**
(NF-e/NFC-e/CT-e/MDF-e/NFSe/SPED/eSocial/LCDPR, cálculo de imposto/INSS, alíquota, prazo, penalidade),
é **obrigatório carregar e citar** a skill `Negocio-acumulado/<domínio>` (universal) + o overlay
`projetos/<projeto>/skills/negocio-*` (cliente). **Responder de memória = violação.** Skill vazia →
PARE, abra pedido em `_ingestao/PEDIDOS.md` e/ou peça validação humana; nunca invente a regra.
Detalhe e gatilhos: `agentes/00-context-agent.md` (Regra #0).

## O gate da fábrica (como 1 vira 15) — três camadas, não uma

1. O agente executa carregando as skills.
2. O agente **auto-valida** pelo Gate acima e **entrega EVIDÊNCIA REPRODUZÍVEL** — o comando que rodou
   e a saída real (ex.: `dotnet build → 0 erros`, contagem de tabelas do banco), **não** "passou/verde".
3. O **ORQUESTRADOR re-executa** a validação no **ambiente vivo** (roda o build/test de novo, sobe o
   banco real, faz a chamada externa real). ⚠️ **O "verde" relatado pelo agente é entrada, não prova** —
   a lição da produção real (EprosERP) é que agentes reportam verde que não é; "build verde" ≠ "funciona".
4. O **Code Review Agent** pré-aprova o que sobreviveu à re-execução.
5. O **diretor humano** confere e libera — com números reverificados, não com o relato do agente.

O gate genérico de fechamento de qualquer etapa está espelhado no
`CHECKLIST_AUDITORIA_SESSAO` da metodologia ERP: evidências registradas, conflitos tratados,
documentação e links válidos, rastreabilidade preservada, pendências registradas, próximo passo
definido.
