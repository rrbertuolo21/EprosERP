---
name: armadilhas-de-porte
categoria: engenharia-reversa
tags: [armadilhas-de-porte, reengenharia, porte-de-legado, migracao-de-codigo, crud-incompleto, estrutura-sem-funcao, migration-faltando, relation-does-not-exist, design-time-factory, connection-string, url-hardcoded, credencial-hardcoded, integracao-outbound, webhook, gateway-de-pagamento, adaptador, recon-exaustiva, build-verde-nao-funciona, ambiente-vivo, "a-tela-nao-fecha", "500-em-runtime"]
nivel: avancado
aplica-se-a: [qualquer-linguagem, qualquer-projeto, porte-de-legado]
fontes:
  - "Produção real do EprosERP (Rafael) — lições de campo do porte/reengenharia de um ERP legado: CRUD incompleto, migrations faltando (26 tabelas GestaoClientes), design-time factories inconsistentes, URLs/credenciais hardcoded no front, integração de cobrança sem adaptador outbound, recon amostral vs. exaustiva, e o gap entre build verde e sistema vivo"
  - "Trabalho Eficaz com Código Legado — Michael C. Feathers — seams, testes de caracterização (a rede que 'build verde' não substitui)"
  - "Building Microservices — Sam Newman (Strangler Fig, integração como cidadão de 1ª classe) — corte incremental e fronteiras"
status: v1-semente
revisao: semestral
---

# Armadilhas de Porte / Reengenharia de Legado

> **Conhecimento agnóstico** — o catálogo das **falhas recorrentes ao portar/reescrever um sistema
> existente** e as receitas para detectá-las *antes* do ambiente vivo. Destilado da produção real do
> EprosERP, mas generalizado: vale para qualquer stack de origem e destino. Como *um projeto* aterra
> isto (qual é o comando de migration, qual o cliente de IO único, quais integrações externas existem)
> vive no overlay do projeto — ver "Como um projeto aterra isto". É a **sub-skill de controle de
> qualidade do porte**: pressupõe que a extração AS-IS e o plano de migração (ver
> [[portabilidade]]) já existem, e responde "**o que costuma faltar quando eu acho que terminei?**".

## Quando usar

Ative quando a tarefa envolver: **portar/reescrever um sistema legado para outra stack, revisar um
porte "pronto" antes de subir, uma tela nova que não fecha o fluxo, um `500` em runtime logo após o
deploy, `relation does not exist` / tabela ausente, `migrate` que quebra fora do localhost, front
batendo em host morto ou tomando `401`, uma integração externa que "existe no legado mas não funciona
no novo", decidir a profundidade da recon (amostral × exaustiva), ou desconfiar de um "build verde"
que ninguém validou no ambiente vivo.** Se a pergunta ainda é *como planejar* a migração, a skill é
[[portabilidade]]; volte a esta quando o alvo for **caçar o que o porte deixou pela metade**.

## Princípios

**Estrutura não é função — porte "pronto" é o que fecha o fluxo do usuário, não o que compila.** A
falha mais comum do porte é parar na casca: cria-se a entidade, o `GET /lista` e o `POST`, e declara-se
feito — mas sem `GET /{id}`, `PUT` e `DELETE` a tela **não fecha**: não abre o detalhe, não edita, não
apaga. O critério de pronto é o *fluxo de ponta a ponta*, não a existência de arquivos. Toda raiz de
agregado portada precisa do CRUD completo mais o endpoint de detalhe — senão você entregou um esqueleto.

**O modelo mapear a entidade não significa que a tabela existe.** A entidade compila, o ORM a mapeia,
o código referencia — e a tabela nunca foi migrada. O resultado é um `500` de `relation/table does not
exist` no primeiro acesso real. O modelo de classes e o schema do banco são **duas verdades separadas**;
o porte precisa reconciliá-las explicitamente. Toda entidade nova exige uma migration correspondente,
e o `Up` da migration tem que ser **só aditivo** (cria tabela/coluna/índice; não dropa nem renomeia
sem plano de dados).

**Configuração hardcoded é bomba-relógio de ambiente.** Design-time factories que fixam
`Host=localhost;Database=..._design` e ignoram a connection string do ambiente; telas de front que
apontam para `http://localhost:5000` com credencial embutida. Tudo funciona na máquina de quem portou
e quebra em qualquer outro ambiente — `migrate` estoura fora do localhost, o front toma `401` contra um
host morto. **Toda origem de conexão e todo endpoint têm que vir do ambiente**, nunca do código: uma
única fonte de connection string, um único cliente de IO com base de runtime e token automático.

**Integração externa é cidadã de 1ª classe do modelo — não some entre as tabelas.** O legado tinha o
webhook de *entrada* (recebe o retorno do gateway), mas faltou o adaptador de *saída* que efetivamente
gera a cobrança no gateway. Isso **não aparece olhando as tabelas** — só olhando o *fluxo*: o dado
entra, mas nunca saiu. Mapeie cada integração como um trio explícito — **adaptador (outbound)
+ configuração + credencial** — nas duas direções, senão metade da integração fica invisível até o
usuário reclamar que "a cobrança não é gerada".

**Recon rasa subporta; a varredura que traz 100% é exaustiva.** Varredura amostral ("olhei umas
telas") sistematicamente perde as telas ricas — as que têm mais campos, mais regras, mais valor. A recon
confiável é **tela por tela, campo por campo**: cansativa por natureza. Amostra serve para estimar
esforço, nunca para definir escopo de porte. Se a recon foi rápida, o porte vai ter buracos do tamanho
do que a recon não viu.

**"Build verde" não é "funciona" — só o ambiente vivo pega o que falta.** Compilar e passar nos testes
prova coerência interna; não prova que a migration rodou, que a URL é a certa, que o adaptador existe.
Esses defeitos moram *entre* o código e o mundo (banco, rede, serviço externo) — território que o
compilador e o teste unitário não visitam. O gate de porte só fecha depois de **exercitar o fluxo no
ambiente vivo** (request real → banco real → integração real), não no CI.

## Na prática

Antipadrão → padrão nas decisões mais comuns do porte:

```txt
❌ Entidade + GET lista + POST e declarar a tela "portada".
✅ CRUD completo por raiz de agregado: GET lista, POST, GET /{id}, PUT, DELETE + endpoint de detalhe.

❌ Entidade mapeia no contexto → assumir que a tabela existe.
✅ Gerar migration do diff do contexto; conferir que o Up é só aditivo; aplicar; só então usar.

❌ IDesignTimeDbContextFactory com Host=localhost;Database=..._design hardcoded.
✅ Toda factory lê ConnectionStrings__DefaultConnection do ambiente — uma fonte única.

❌ Tela batendo em http://localhost:5000 sem token (API morta → 401).
✅ Cliente de IO único: base de runtime do ambiente + token injetado automaticamente.

❌ Ver só o webhook (entrada) e achar a integração completa.
✅ Mapear os dois lados: adaptador OUTBOUND que gera a cobrança + config + credencial.

❌ Recon amostral ("olhei umas telas") definindo escopo.
✅ Recon exaustiva, tela-por-tela / campo-por-campo, para escopo; amostra só estima esforço.

❌ "Compilou e os testes passam, então está pronto."
✅ Fluxo exercitado no ambiente VIVO: request real → banco real → integração real, antes de fechar.
```

## Receitas (tarefas canônicas — o agente adapta, não reinventa)

**1. Fechar o CRUD de uma raiz de agregado portada (matar "estrutura sem função").**
Para cada raiz de agregado do módulo portado, o mapa mínimo que fecha a tela é:
```txt
GET    /recurso           → lista (paginada/filtrável)
POST   /recurso           → cria
GET    /recurso/{id}      → DETALHE  ← o mais esquecido; sem ele a tela não abre o item
PUT    /recurso/{id}      → atualiza (ou PATCH parcial)
DELETE /recurso/{id}      → remove
```
Passos: (1) liste as raízes de agregado do módulo; (2) para cada uma, marque presença/ausência dos 5
verbos + o endpoint de detalhe; (3) qualquer célula faltando é um buraco de porte, não "escopo futuro";
(4) valide fechando o fluxo na tela (abrir → detalhar → editar → apagar), não só olhando a lista.
Regra de teste: se a lista carrega mas clicar num item não abre nada, falta o `GET /{id}`.

**2. Detectar entidade sem migration (caçar o `relation does not exist` antes do runtime).**
```txt
# a) todas as entidades/tabelas que o modelo declara:
#    (varra o contexto/mapeamento do ORM — DbSet/@Entity/models registrados)
# b) todas as tabelas que as migrations criam:
#    (varra o histórico de migrations aplicadas)
# c) o diff (a) − (b) = entidades mapeadas SEM tabela → estouram em runtime
```
Passos: (1) gere a migration a partir do **diff do contexto** (o ORM sabe dizer o que falta); (2)
**leia o `Up` gerado** e confirme que é só aditivo (`CREATE TABLE/COLUMN/INDEX`) — nenhum `DROP`/rename
sem plano de dados; (3) aplique em ambiente limpo e rode o fluxo; (4) confira o count de tabelas
esperado vs. criado. Caso real EprosERP: o módulo GestaoClientes tinha **26 tabelas** (add_ons etc.)
mapeadas e nunca migradas — todas viravam `500` no primeiro acesso.

**3. Padronizar as design-time factories (migrate que roda em qualquer ambiente).**
```txt
❌ new DbContextOptionsBuilder().UseNpgsql("Host=localhost;Database=app_design;...")
✅ var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
   new DbContextOptionsBuilder().Use<Provider>(cs);
```
Passos: (1) localize **todas** as `IDesignTimeDbContextFactory` (ou equivalente da stack); (2)
substitua qualquer host/database hardcoded pela leitura de `ConnectionStrings__DefaultConnection` do
ambiente; (3) garanta uma fonte única de connection string (nenhuma factory inventa a sua); (4) teste
o `migrate` fora do localhost (container/CI) — é lá que o hardcode aparece.

**4. Migrar o front legado para o cliente de IO único (matar URL/credencial hardcoded).**
```txt
❌ fetch("http://localhost:5000/api/...")            // host morto, sem token → 401
✅ apiClient.get("/api/...")                          // base de runtime + token automático
```
Passos: (1) faça grep por `http://`, `localhost:`, portas fixas e URLs absolutas no front; (2)
troque cada chamada crua pelo **cliente de IO único** do projeto (base vinda do runtime + token
injetado); (3) remova qualquer credencial/segredo embutido na tela; (4) valide que uma rota protegida
sem token devolve `401` de forma controlada — e que com o cliente certo passa.

**5. Mapear uma integração externa como cidadã de 1ª classe (não só o webhook).**
```txt
Integração <gateway>:
  ├─ OUTBOUND  adaptador que CHAMA o serviço (ex.: gerar cobrança)   ← costuma faltar
  ├─ INBOUND   webhook/handler que RECEBE o retorno                   ← costuma existir sozinho
  ├─ CONFIG    endpoint/base/opções por ambiente
  └─ CREDENCIAL  chave/token do serviço (do ambiente, nunca no código)
```
Passos: (1) para cada integração, verifique os **quatro** itens — a falta do outbound é invisível nas
tabelas, só aparece no fluxo ("o dado entra mas nunca saiu"); (2) siga o dado ponta a ponta
(gatilho → adaptador de saída → serviço externo → webhook de retorno → efeito no banco); (3) qualquer
elo ausente é gap de porte. Caso real EprosERP: existia o webhook de retorno da cobrança, mas faltava o
adaptador que **gera** a cobrança no gateway — a integração parecia pronta e nunca cobrava.

**6. Recon exaustiva de um módulo (100%, não amostra).**
Passos: (1) enumere **todas** as telas do módulo (não uma amostra); (2) por tela, catalogue **cada
campo**, ação, validação e regra visível; (3) marque as telas ricas (muitos campos/regras) como alto
risco de subporte; (4) só então feche o escopo do porte. Amostra é legítima **apenas** para estimar
esforço — nunca para definir o que será portado. Sinal de alerta: recon que terminou rápido = escopo
com buracos do tamanho do que não foi visto.

## Checklist de pronto (o agente roda antes de declarar o porte concluído)

- [ ] **CRUD completo** por raiz de agregado: `GET lista`, `POST`, `GET /{id}`, `PUT`, `DELETE` + endpoint de detalhe — a tela fecha o fluxo (abrir → detalhar → editar → apagar).
- [ ] **Toda entidade mapeada tem migration**; o diff (modelo − migrations) é vazio; nenhum `relation/table does not exist` no fluxo.
- [ ] O `Up` de cada migration é **só aditivo** (sem `DROP`/rename sem plano de dados); aplicado em ambiente limpo.
- [ ] **Todas** as design-time factories leem `ConnectionStrings__DefaultConnection` do ambiente; nenhum `Host=localhost`/`_design` hardcoded; `migrate` roda fora do localhost.
- [ ] Front sem **URL/porta/credencial hardcoded**; toda chamada passa pelo cliente de IO único (base de runtime + token automático); rota protegida sem token → `401` controlado.
- [ ] Cada integração externa tem os **quatro** cidadãos: adaptador OUTBOUND + INBOUND/webhook + config + credencial (do ambiente); o dado foi seguido de ponta a ponta.
- [ ] Recon do módulo foi **exaustiva** (todas as telas, todos os campos), não amostral; telas ricas conferidas.
- [ ] **Fluxo exercitado no ambiente VIVO** (request real → banco real → integração real), não só "build verde" no CI.

## Armadilhas comuns

- **Confundir esqueleto com pronto.** Entidade + lista + criar existe, então "portado" — mas sem
  detalhe/editar/apagar a tela não fecha. Estrutura sem função.
- **Confiar no modelo como se fosse o banco.** A classe mapeia, logo a tabela existe — falso; a
  migration pode nunca ter sido gerada/aplicada. `500` no primeiro acesso real.
- **Hardcode que "funciona na minha máquina".** Connection string e URL fixas passam local e quebram
  em todo o resto — o defeito viaja escondido até o outro ambiente.
- **Ver só metade da integração.** O webhook de entrada dá a sensação de integração completa; o
  adaptador de saída que falta só aparece quando alguém pergunta por que a ação externa não acontece.
- **Recon por amostragem virando escopo.** "Olhei umas telas" perde justamente as telas ricas — as de
  maior valor e mais regras. Amostra estima; não define escopo.
- **Parar no build verde.** Compilar e testar não visita banco, rede nem serviço externo — os defeitos
  de porte moram exatamente aí. Sem ambiente vivo, "pronto" é palpite.

## Como um projeto aterra isto

O ponto de encaixe (*seam*) que cada projeto preenche no seu overlay, **referenciando** esta skill:
- **Comando/ferramenta de migration** e como gerar o diff do contexto (ex.: `dotnet ef migrations add`
  / `add-migration`), e onde ficam as migrations aplicadas.
- **A fonte única de connection string** (nome exato da variável — ex.: `ConnectionStrings__DefaultConnection`)
  e o padrão canônico da design-time factory do projeto.
- **O cliente de IO único** do front (base de runtime, injeção de token) que substitui chamadas cruas.
- **O inventário de integrações externas** (quais gateways/serviços, credenciais por ambiente) e onde
  vivem os adaptadores outbound/inbound.
- **A lista de raízes de agregado por módulo** e o padrão de endpoint de detalhe.
O overlay do EprosERP (`projetos/epros/`) aterra isto com a stack e os módulos reais (ex.: GestaoClientes).

## Fontes

- Produção real do EprosERP (Rafael) — lições de campo do porte de um ERP legado.
- Trabalho Eficaz com Código Legado — Michael C. Feathers (seams, testes de caracterização).
- Building Microservices — Sam Newman (Strangler Fig, integração como cidadão de 1ª classe).

> Rascunhos de extração acumulam em `EXTRACOES.md` nesta mesma pasta até amadurecerem para cá.
