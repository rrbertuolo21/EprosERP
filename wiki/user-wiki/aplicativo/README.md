# Aplicativo — Gestão de Clientes da Siser

## Pra que serve
O Aplicativo é o **painel com que a Siser toca o próprio negócio de vender o Epros como serviço**. Não é o ERP que o
cliente usa no dia a dia — é o control-plane que fica por trás: é aqui que a Siser cadastra cada empresa cliente, define
qual plano ela contratou, emite e cobra as mensalidades, controla quem pode entrar e o que cada pessoa vê, e mantém tudo
funcionando com suporte. Para o cliente que se cadastra, é também a porta de entrada: criar a conta, começar o período de
teste e escolher como pagar. Em uma frase: **é onde nascem, são cobradas e são atendidas as empresas que usam o Epros.**

## O que você consegue fazer
- **Cadastrar uma nova empresa cliente** (você mesmo ou pelo autoatendimento do cliente) e começar um **período de teste** automático.
- **Vender e trocar planos** — escolher o que cada cliente contrata, subir ou descer de plano.
- **Cobrar a assinatura** por **PIX, cartão ou boleto**, de forma recorrente (mensal, anual ou vitalícia).
- **Acompanhar quem está pagando e quem está devendo** — avisos de vencimento, faturas em aberto, recibos.
- **Aplicar cupons de desconto** na contratação e nas renovações.
- **Controlar o acesso** — quem entra, com qual papel, e o que cada papel enxerga no menu.
- **Operar e dar suporte a qualquer cliente** pela área da operadora (Landlord), com registro de tudo o que foi feito.
- **Ver como o negócio vai** — receita recorrente, clientes ativos, inadimplência, conversão de testes em pagantes.

## Como usar

### Um cliente novo entra (autoatendimento)
1. O visitante acessa a página de cadastro e informa se é pessoa física ou jurídica.
2. O sistema cria a conta, a empresa e o primeiro usuário administrador, e **inicia o período de teste automaticamente**.
3. Um e-mail de boas-vindas é enviado. O cliente já pode operar o ERP dentro do teste.

### A mensalidade é cobrada
1. Ao fim do teste, o sistema **gera a primeira fatura** e cobra pelo meio escolhido (PIX, cartão ou boleto).
2. A cada ciclo (mês/ano), uma nova fatura é emitida automaticamente.
3. Quando o pagamento é confirmado, o cliente segue ativo e recebe o **recibo**.

### Um cliente atrasou o pagamento
1. O sistema **avisa antes do vencimento** e nos dias seguintes.
2. Passada a tolerância do plano, o acesso fica **somente-leitura** (o cliente ainda consegue consultar e exportar).
3. Persistindo o atraso, o acesso é **bloqueado**. **Assim que o cliente paga, o acesso volta na hora.**

### A Siser dá suporte a um cliente (área Landlord)
1. Um operador da Siser entra na área Landlord e escolhe o cliente que precisa atender.
2. Ele pode **operar como** aquele cliente para investigar um problema — com tudo registrado (quem, quando, por quê).
3. Também gerencia planos, faturas e cupons, e acompanha os números do negócio.

## Conceitos
- **Tenant (cliente)** — cada empresa que contrata o Epros. Seus dados ficam totalmente separados dos demais.
- **Plano** — o pacote contratado: define quais módulos e quais limites (quantos usuários, quantas empresas) estão liberados.
- **Assinatura** — o vínculo vigente entre o cliente e o plano, com o ciclo de cobrança (quando vence a próxima fatura).
- **Trial (período de teste)** — janela inicial gratuita antes de começar a cobrar.
- **Fatura** — a conta de cada ciclo. Quando paga, gera um **recibo**.
- **Status do cliente** — em que fase ele está: Em teste → Ativo → Aguardando pagamento → Cancelado.
- **Papel** — o "cargo" da pessoa no sistema, que decide o que ela pode fazer e ver.
- **Cupom** — desconto aplicado na contratação ou na renovação.
- **Landlord** — a área interna da Siser para operar e dar suporte a todos os clientes.
- **Inadimplência** — situação de quem não pagou; leva a somente-leitura e depois a bloqueio, com reativação ao pagar.

## Perguntas frequentes / limites
- **O cliente perde os dados se atrasar?** Não imediatamente. Primeiro fica somente-leitura (dá para consultar e exportar
  por uma janela), e o acesso volta assim que ele paga.
- **Dá para cancelar a assinatura?** Sim, com registro de motivo. O cliente entra numa janela de somente-leitura/exportação
  antes do bloqueio total, e pode ser reativado dentro dela.
- **Quais formas de pagamento existem?** PIX, cartão e boleto, de forma recorrente.
- **Isto é o ERP?** Não. Este módulo **gerencia os clientes** do ERP (o negócio da Siser). O ERP em si são os demais módulos.
- **Alguns avisos ainda não chegam por e-mail.** A régua de cobrança monta os alertas, mas a entrega por e-mail (dunning)
  e alguns comprovantes por e-mail ainda dependem de configuração de ambiente — ver o [estado técnico](../../dev-wiki/aplicativo/README.md#estado--pendências).

## Telas
Em construção — o front (Nuxt) foi entregue, mas sua validação campo-a-campo depende do ambiente de build. As telas serão
documentadas aqui quando o front for publicado e validado. Referência de comportamento: MANUAL_CENTRAL na fábrica
(`especificacoes/0_APLICATIVO/MANUAL_CENTRAL.md`).
