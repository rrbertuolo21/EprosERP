---
name: fan-out-paralelo
categoria: processo-agile
tags: [fan-out, paralelismo, orquestracao, agentes, ondas, pastas-disjuntas, contrato, scaffolding, gate, re-execucao, ambiente-vivo, molde, cobertura, erd, produção-em-escala]
nivel: avancado
aplica-se-a: [orquestracao-de-agentes, produção-em-escala, times, processo]
fontes:
  - "RETROSPECTIVA-EPROSERP.md — primeira produção real de ponta a ponta da fábrica (15 módulos, ~870 tabelas, ~1059 endpoints, 328 telas, ondas de 5–6 agentes)"
  - "EprosERP/HISTORICO-DESENVOLVIMENTO-IA.md — diário técnico da produção"
status: v1-semente
revisao: semestral
---

# Fan-out Paralelo — orquestrar muitos agentes com qualidade

> **Conhecimento agnóstico** — vale para qualquer produção em escala dirigida por IA (telas, módulos,
> rotas, transcrição de legado). O multiplicador real da fábrica não é "1 agente rápido"; é **N agentes
> em paralelo sem colisão e com o mesmo molde**. Como *este* projeto nomeia suas pastas, rotas e o
> contrato entre back e front fica no overlay do projeto — ver "Como um projeto aterra isto".

## Quando usar

Ative quando a tarefa envolver: **produzir muitas fatias parecidas de uma vez (telas/CRUDs/módulos),
portar um legado inteiro, distribuir trabalho entre vários agentes, decidir a ordem entre scaffolding
compartilhado e trabalho paralelo, ou quando um orquestrador precisa consolidar o resultado de vários
agentes sem confiar cegamente no "verde" de cada um**.

## Princípio central

O "verde" de um agente é **entrada, não prova**. Entre o auto-relato do agente e o veredito humano
existe um **orquestrador** que (a) faz fan-out em pastas disjuntas, (b) **re-executa** a validação de
cada agente no ambiente vivo, e (c) consolida e reporta por bloco com números reverificados. São
**três gates, não um**: auto-validação do agente → **re-execução do orquestrador** → veredito humano.
É o gate do meio que faz o "verde" ser verdade.

---

## Receitas

### 1. Gargalo serial primeiro, fan-out depois

O que **todos consomem e ninguém edita** é feito **por um único agente/integrador antes** do paralelo:
scaffolding compartilhado (cliente de API, layouts, componentes base como DataTable, shell/sidebar,
config). Só quando o alicerce está de pé os agentes de fatia entram — e o **consomem**, não o editam.
Fan-out sobre um alicerce ainda em movimento gera retrabalho e colisão.

### 2. Provar o molde antes de multiplicar

Antes de N cópias, faça **1 fatia de referência** no padrão exato (um submódulo/tela perfeito, batendo
no padrão que já existe) e **valide-a no ambiente vivo**. Se o molde está certo, N cópias saem certas;
se está errado, você errou 1 vez, não N. Só depois do molde aprovado dispara o fan-out.

### 3. Pastas disjuntas = zero colisão

Cada agente é **dono de uma pasta exclusiva** (1 módulo / 1 rota — ex.: `pages/<mod>/`,
`src/Modules/<X>/`). **Regra de ouro: dois agentes nunca escrevem o mesmo arquivo.** Isso é o que
torna o paralelismo real e livre de conflito de merge. Os **arquivos compartilhados** que vários
tocariam (menu, config global, layout, roteador) **não** entram no fan-out: um único **integrador os
fia DEPOIS**, quando as fatias já existem.

### 4. Contrato fixo entre agentes paralelos

O que atravessa a fronteira entre agentes (rota, DTO, chave compartilhada) é **definido ANTES** do
fan-out e **congelado**. Ex.: fixar `POST /public/plataforma/login → {token}` e a chave do localStorage
antes de back e front começarem — assim os dois batem sem retrabalho. Contrato fixo é o que permite dois
agentes trabalharem "às cegas" um do outro e ainda encaixarem.

### 5. Gate de re-execução do orquestrador

O agente entrega **evidência reproduzível** (comando + saída), não a palavra "passou". O orquestrador
**re-executa build/test/checagem no ambiente vivo** (banco real, Docker, chamada externa real) e só
então consolida. Motivo provado: agentes reportaram build/teste passando que **não passava**; e "build
verde" ≠ "funciona" (faltavam migrations, factories com `localhost` hardcoded, adaptador outbound
ausente — só o ambiente vivo pegou). O **report é por bloco, com NÚMEROS REVERIFICADOS** do banco/git
vivos — não do relato do agente. É assim que o diretor confia sem reler tudo.

### 6. Ondas

Não dispare 50 agentes de uma vez. Divida em **ondas de 5–6 agentes**; ao fechar cada onda, rode o
**gate agregado** (build/test do conjunto no ambiente vivo, números reverificados) **antes** de liberar
a próxima onda. Erro sistemático aparece na onda 1 e é corrigido no molde antes de contaminar a 5.

### 7. Gate de cobertura para porte

Para trazer 100% de um legado, **antes** do fan-out de construção: **recon exaustivo por artefato**
(tela-por-tela, campo-por-campo — recon amostral subporta e deixa a área "fina") → produzir **ERD +
matriz de cobertura** → **validação humana**. Barato, e evita construir o errado em escala. Mapeie
**integrações externas como cidadãos de primeira classe** (adaptador + config + credencial), não só
entidades — o que não está no modelo de dados (ex.: adaptador outbound de um gateway) some no porte.

---

## Checklist de fan-out (acionável)

**Antes do fan-out**
- [ ] Scaffolding compartilhado pronto por 1 integrador; agentes vão **consumir**, não editar.
- [ ] Molde provado: 1 fatia de referência no padrão exato, **validada no ambiente vivo**.
- [ ] Contrato de fronteira (rota/DTO/chave) **definido e congelado** por escrito.
- [ ] Fronteiras de posse desenhadas: 1 agente = 1 pasta exclusiva; **nenhum arquivo escrito por dois**.
- [ ] Arquivos compartilhados (menu/config/layout) reservados para o integrador **do depois**.
- [ ] (Porte) Recon exaustivo → ERD + matriz de cobertura → **validação humana** concluída.
- [ ] (Porte) Integrações externas mapeadas como cidadãos de 1ª classe (adaptador + config + credencial).

**Durante (por onda de 5–6)**
- [ ] Cada agente recebe: sua pasta, o contrato congelado, o padrão do molde.
- [ ] Cada agente entrega **evidência reproduzível** (comando + saída), não "passou".

**Gate de fechamento (por bloco/onda — o orquestrador)**
- [ ] Orquestrador **re-executou** build/test/checagem no **ambiente vivo** (não confiou no relato).
- [ ] (Porte/CRUD) Migration aplicada + CRUD completo por raiz de agregado + endpoint de detalhe.
- [ ] Números do report **reverificados** do banco/git vivos.
- [ ] Report por bloco emitido; onda seguinte só libera com o gate agregado verde.
- [ ] Veredito humano no fim — os três gates fechados (agente → orquestrador → humano).

---

## Armadilhas comuns

- **Confiar no "verde" do agente.** É a falha nº 1. Sem re-execução independente, o falso-verde passa.
- **"Build verde" tratado como "funciona".** Compila e testa, mas quebra no ambiente vivo (migrations
  faltando, config hardcoded, adaptador ausente). Só banco/Docker/chamada real reais provam.
- **Fan-out antes do molde.** Multiplica o erro N vezes em vez de 1.
- **Fan-out antes do scaffolding.** Agentes editam o alicerce em movimento → colisão e retrabalho.
- **Dois agentes no mesmo arquivo.** Conflito garantido; a posse por pasta disjunta é inegociável.
- **Recon amostral no porte.** Subporta a área; a recon precisa ser exaustiva por artefato.
- **Report do relato, não do vivo.** Números não reverificados corroem a confiança do diretor.

## Como um projeto aterra isto

O ponto de encaixe (*seam*): cada projeto define **como recorta as pastas de posse** (a granularidade
de "1 agente = 1 pasta": por módulo? por rota? por raiz de agregado?), **quais são os arquivos
compartilhados** que o integrador fia depois, o **formato do contrato de fronteira** (convenção de
rota/DTO/chave) e **quais comandos** compõem o gate de re-execução no seu ambiente vivo (build/test/
Docker/migração). Os overlays de projeto (ex.: `projetos/<projeto>/skills/…`) preenchem isto com as
pastas, rotas e comandos reais do produto — **referenciando** esta skill, sem copiá-la.

## Fontes

- `RETROSPECTIVA-EPROSERP.md` — seções 1 (o que virou padrão), 2 (o que quebrou → calibração) e 4 (a tese recalibrada).
- `EprosERP/HISTORICO-DESENVOLVIMENTO-IA.md` — diário técnico da primeira produção real.

> Rascunhos de extração de novas produções acumulam em `EXTRACOES.md` nesta mesma pasta até amadurecerem para cá.
