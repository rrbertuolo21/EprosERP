using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Configuração da loja e-commerce (eco_configuracao_loja). Fonte: EF §8.1.
    /// ECO-042/ECO-043: credenciais de pagamento são dados protegidos e o token da loja é
    /// obrigatório para as APIs públicas. As credenciais NÃO devem ser expostas na vitrine.
    /// </summary>
    public class EcoConfiguracaoLoja : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Rua { get; private set; }
        public string? Numero { get; private set; }
        public string? Bairro { get; private set; }
        public string? Cidade { get; private set; }
        public string? Uf { get; private set; }
        public string? Cep { get; private set; }
        public string? Telefone { get; private set; }
        public string? Email { get; private set; }
        public string? LinkFacebook { get; private set; }
        public string? LinkTwitter { get; private set; }
        public string? LinkInstagram { get; private set; }
        public decimal FreteGratisValor { get; private set; }
        public string? PagamentoPublicKey { get; private set; }
        public string? PagamentoAccessToken { get; private set; }
        public string? Funcionamento { get; private set; }
        public string? PoliticaPrivacidade { get; private set; }
        public string? MensagemAgradecimento { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }
        public string? MapaUrl { get; private set; }
        public string? MapaApiKey { get; private set; }
        public string TokenLoja { get; private set; } = string.Empty;
        public string? CorFundo { get; private set; }
        public string? CorBotao { get; private set; }
        public string? Logo { get; private set; }
        public string? ImagemContato { get; private set; }
        public string? Icone { get; private set; }
        public int? TimerCarrossel { get; private set; }

        protected EcoConfiguracaoLoja() { }

        public EcoConfiguracaoLoja(
            string? nome, string? rua, string? numero, string? bairro, string? cidade, string? uf, string? cep,
            string? telefone, string? email, string? linkFacebook, string? linkTwitter, string? linkInstagram,
            decimal freteGratisValor, string? pagamentoPublicKey, string? pagamentoAccessToken, string? funcionamento,
            string? politicaPrivacidade, string? mensagemAgradecimento, decimal? latitude, decimal? longitude,
            string? mapaUrl, string? mapaApiKey, string tokenLoja, string? corFundo, string? corBotao,
            string? logo, string? imagemContato, string? icone, int? timerCarrossel,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome; Rua = rua; Numero = numero; Bairro = bairro; Cidade = cidade; Uf = uf; Cep = cep;
            Telefone = telefone; Email = email; LinkFacebook = linkFacebook; LinkTwitter = linkTwitter; LinkInstagram = linkInstagram;
            FreteGratisValor = freteGratisValor; PagamentoPublicKey = pagamentoPublicKey; PagamentoAccessToken = pagamentoAccessToken;
            Funcionamento = funcionamento; PoliticaPrivacidade = politicaPrivacidade; MensagemAgradecimento = mensagemAgradecimento;
            Latitude = latitude; Longitude = longitude; MapaUrl = mapaUrl; MapaApiKey = mapaApiKey;
            TokenLoja = string.IsNullOrWhiteSpace(tokenLoja) ? Guid.NewGuid().ToString("N") : tokenLoja;
            CorFundo = corFundo; CorBotao = corBotao; Logo = logo; ImagemContato = imagemContato; Icone = icone; TimerCarrossel = timerCarrossel;
            AddNotifications(new Contract<EcoConfiguracaoLoja>()
                .Requires()
                .IsNotNullOrEmpty(TokenLoja, nameof(TokenLoja), "O token da loja é obrigatório. [Origem: EcoConfiguracaoLoja]"));
        }
    }

    /// <summary>Cliente da loja e-commerce (eco_cliente). Fonte: EF §8.2. ECO-014/ECO-039.</summary>
    public class EcoCliente : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Sobrenome { get; private set; }
        public string? Cpf { get; private set; }
        public string? InscricaoEstadual { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public string SenhaHash { get; private set; } = string.Empty;
        public string TokenCliente { get; private set; } = string.Empty;
        public bool Status { get; private set; } = true;

        protected EcoCliente() { }

        public EcoCliente(string nome, string? sobrenome, string? cpf, string? inscricaoEstadual, string email, string? telefone, string senhaHash, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Sobrenome = sobrenome;
            Cpf = cpf;
            InscricaoEstadual = inscricaoEstadual;
            Email = email;
            Telefone = telefone;
            SenhaHash = senhaHash;
            TokenCliente = Guid.NewGuid().ToString("N").Substring(0, 20); // token de sessão (20 caracteres, EF §8.2)
            Status = true;
            AddNotifications(new Contract<EcoCliente>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do cliente é obrigatório. [Origem: EcoCliente]")
                .IsEmail(email ?? string.Empty, nameof(Email), "E-mail inválido. [Origem: EcoCliente]")
                .IsNotNullOrEmpty(senhaHash, nameof(SenhaHash), "A senha do cliente é obrigatória. [Origem: EcoCliente]"));
        }

        public void Alterar(string nome, string? sobrenome, string? cpf, string? inscricaoEstadual, string? telefone, string alteradoPor)
        {
            Nome = nome; Sobrenome = sobrenome; Cpf = cpf; InscricaoEstadual = inscricaoEstadual; Telefone = telefone;
            MarcarAlterado(alteradoPor);
        }

        public void AlterarSenha(string senhaHash, string alteradoPor)
        {
            SenhaHash = senhaHash;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = false;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Endereço do cliente e-commerce (eco_endereco_cliente). Fonte: EF §8.3.</summary>
    public class EcoEnderecoCliente : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public string Rua { get; private set; } = string.Empty;
        public string Numero { get; private set; } = string.Empty;
        public string Bairro { get; private set; } = string.Empty;
        public string Cep { get; private set; } = string.Empty;
        public string Cidade { get; private set; } = string.Empty;
        public string Uf { get; private set; } = string.Empty;
        public string? Complemento { get; private set; }

        protected EcoEnderecoCliente() { }

        public EcoEnderecoCliente(Guid clienteId, string rua, string numero, string bairro, string cep, string cidade, string uf, string? complemento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            Rua = rua; Numero = numero; Bairro = bairro; Cep = cep; Cidade = cidade; Uf = uf; Complemento = complemento;
            AddNotifications(new Contract<EcoEnderecoCliente>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório. [Origem: EcoEnderecoCliente]")
                .IsNotNullOrEmpty(rua, nameof(Rua), "A rua é obrigatória. [Origem: EcoEnderecoCliente]")
                .IsNotNullOrEmpty(cep, nameof(Cep), "O CEP é obrigatório. [Origem: EcoEnderecoCliente]")
                .IsNotNullOrEmpty(uf, nameof(Uf), "A UF é obrigatória. [Origem: EcoEnderecoCliente]"));
        }
    }

    /// <summary>Cupom de desconto (eco_cupom). Fonte: EF §8.7. ECO-027..ECO-031.</summary>
    public class EcoCupom : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public EEcoTipoCupom Tipo { get; private set; }
        public bool Status { get; private set; } = true;

        protected EcoCupom() { }

        public EcoCupom(string codigo, decimal valor, EEcoTipoCupom tipo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Valor = valor;
            Tipo = tipo;
            Status = true;
            AddNotifications(new Contract<EcoCupom>()
                .Requires()
                // ECO-031: código com 6 caracteres quando gravado no pedido.
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do cupom é obrigatório. [Origem: EcoCupom]")
                .IsGreaterThan(valor, 0, nameof(Valor), "O valor do cupom deve ser maior que zero. [Origem: EcoCupom]"));
        }

        public void AtivarInativar(bool status, string alteradoPor)
        {
            Status = status;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-029/ECO-030: calcula o desconto do cupom limitado ao total do pedido.</summary>
        public decimal CalcularDesconto(decimal totalPedido)
        {
            var desconto = Tipo == EEcoTipoCupom.Percentual ? totalPedido * (Valor / 100m) : Valor;
            return desconto > totalPedido ? totalPedido : desconto;
        }
    }

    /// <summary>Cidade com frete grátis (eco_frete_gratis_cidade). Fonte: EF §8.8. ECO-033.</summary>
    public class EcoFreteGratisCidade : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Uf { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected EcoFreteGratisCidade() { }

        public EcoFreteGratisCidade(string nome, string uf, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Uf = uf;
            Ativo = true;
            AddNotifications(new Contract<EcoFreteGratisCidade>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da cidade é obrigatório. [Origem: EcoFreteGratisCidade]")
                .IsNotNullOrEmpty(uf, nameof(Uf), "A UF é obrigatória. [Origem: EcoFreteGratisCidade]"));
        }

        public void AtivarInativar(bool ativo, string alteradoPor)
        {
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Banner/carrossel da vitrine (eco_carrossel). Fonte: EF §8.6.</summary>
    public class EcoCarrossel : EntidadeSaaSBase
    {
        public string? Titulo { get; private set; }
        public string? Descricao { get; private set; }
        public string? LinkAcao { get; private set; }
        public string? NomeBotao { get; private set; }
        public string? Imagem { get; private set; }
        public string? CorFundo { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected EcoCarrossel() { }

        public EcoCarrossel(string? titulo, string? descricao, string? linkAcao, string? nomeBotao, string? imagem, string? corFundo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Titulo = titulo; Descricao = descricao; LinkAcao = linkAcao; NomeBotao = nomeBotao; Imagem = imagem; CorFundo = corFundo;
            Ativo = true;
        }

        public void Alterar(string? titulo, string? descricao, string? linkAcao, string? nomeBotao, string? imagem, string? corFundo, bool ativo, string alteradoPor)
        {
            Titulo = titulo; Descricao = descricao; LinkAcao = linkAcao; NomeBotao = nomeBotao; Imagem = imagem; CorFundo = corFundo; Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Mensagem de contato recebida (eco_contato). Fonte: EF §8.9. ECO-040.</summary>
    public class EcoContato : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Texto { get; private set; } = string.Empty;

        protected EcoContato() { }

        public EcoContato(string nome, string email, string texto, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome; Email = email; Texto = texto;
            AddNotifications(new Contract<EcoContato>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do contato é obrigatório. [Origem: EcoContato]")
                .IsEmail(email ?? string.Empty, nameof(Email), "E-mail inválido. [Origem: EcoContato]")
                .IsNotNullOrEmpty(texto, nameof(Texto), "A mensagem é obrigatória. [Origem: EcoContato]"));
        }
    }

    /// <summary>Inscrição em newsletter (eco_newsletter). Fonte: EF §8.10.</summary>
    public class EcoNewsletter : EntidadeSaaSBase
    {
        public string Email { get; private set; } = string.Empty;

        protected EcoNewsletter() { }

        public EcoNewsletter(string email, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Email = email;
            AddNotifications(new Contract<EcoNewsletter>()
                .Requires()
                .IsEmail(email ?? string.Empty, nameof(Email), "E-mail inválido. [Origem: EcoNewsletter]"));
        }
    }

    /// <summary>Produto favorito do cliente (eco_favorito_produto). Fonte: EF §8.11.</summary>
    public class EcoFavoritoProduto : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid ProdutoId { get; private set; }

        protected EcoFavoritoProduto() { }

        public EcoFavoritoProduto(Guid clienteId, Guid produtoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            ProdutoId = produtoId;
            AddNotifications(new Contract<EcoFavoritoProduto>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório. [Origem: EcoFavoritoProduto]")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto é obrigatório. [Origem: EcoFavoritoProduto]"));
        }
    }

    /// <summary>
    /// Pedido e-commerce (eco_pedido). Fonte: EF §8.4. Regras ECO-015..ECO-041.
    /// ECO-016: inicia em CriadoSemPagamento. ECO-017: total = itens + frete - desconto.
    /// FISCAL: ao converter em Venda (ECO-035/ECO-036), a emissão de NF-e usa o pedido como origem
    /// e permanece no módulo Fiscal (IHerculesFiscalService) — aqui só o vínculo venda_id/numero_nfe.
    /// [FATO fiscal: indPres=2 (internet) para NF-e/NFC-e de e-commerce — MOC 7.0 Anexo I campo B25b.
    ///  Regra da fábrica: fiscal nunca de memória.]
    /// </summary>
    public class EcoPedido : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid EnderecoId { get; private set; }
        public EEcoStatusPagamento StatusPagamentoCodigo { get; private set; } = EEcoStatusPagamento.CriadoSemPagamento;
        public EEcoStatusPreparacao StatusPreparacaoCodigo { get; private set; } = EEcoStatusPreparacao.Novo;
        public decimal ValorTotal { get; private set; }
        public decimal ValorFrete { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public string? TipoFrete { get; private set; }
        public Guid? VendaId { get; private set; }
        public string? NumeroNfe { get; private set; }
        public string? Observacao { get; private set; }
        public string? RandPedido { get; private set; }
        public string? Hash { get; private set; }
        public string TokenPedido { get; private set; } = string.Empty;
        public string? CupomDesconto { get; private set; }
        public string? TransacaoId { get; private set; }
        public EEcoFormaPagamento? FormaPagamento { get; private set; }
        public string? StatusPagamento { get; private set; }
        public string? StatusDetalhe { get; private set; }
        public string? LinkBoleto { get; private set; }
        public string? QrCode { get; private set; }
        public string? QrCodeBase64 { get; private set; }
        public string? CodigoRastreio { get; private set; }
        public long? SequenciaExibicao { get; private set; }

        protected EcoPedido() { }

        public EcoPedido(
            Guid clienteId,
            Guid enderecoId,
            decimal valorItens,
            decimal valorFrete,
            decimal valorDesconto,
            string? tipoFrete,
            string? cupomDesconto,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ClienteId = clienteId;
            EnderecoId = enderecoId;
            ValorFrete = valorFrete;
            ValorDesconto = valorDesconto;
            TipoFrete = tipoFrete;
            CupomDesconto = cupomDesconto;
            Observacao = observacao;
            // ECO-017: total = itens + frete - desconto.
            ValorTotal = valorItens + valorFrete - valorDesconto;
            // ECO-016: pedido criado pelo checkout inicia sem pagamento.
            StatusPagamentoCodigo = EEcoStatusPagamento.CriadoSemPagamento;
            StatusPreparacaoCodigo = EEcoStatusPreparacao.Novo;
            TokenPedido = Guid.NewGuid().ToString("N").Substring(0, 20); // ECO: token do checkout (20 caracteres)
            RandPedido = Guid.NewGuid().ToString("N").Substring(0, 12);
            AddNotifications(new Contract<EcoPedido>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório. [Origem: EcoPedido]")
                .AreNotEquals(enderecoId, Guid.Empty, nameof(EnderecoId), "O endereço de entrega é obrigatório. [Origem: EcoPedido]")
                .IsGreaterOrEqualsThan(ValorTotal, 0m, nameof(ValorTotal), "O total do pedido não pode ser negativo. [Origem: EcoPedido]"));
        }

        /// <summary>ECO-021: pagamento por cartão grava transação e passa a pagamento iniciado.</summary>
        public void IniciarPagamentoCartao(string? transacaoId, string alteradoPor)
        {
            FormaPagamento = EEcoFormaPagamento.Cartao;
            TransacaoId = transacaoId;
            StatusPagamentoCodigo = EEcoStatusPagamento.PagamentoIniciado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-022: pagamento Pix grava transação, QR Code e base64.</summary>
        public void IniciarPagamentoPix(string? transacaoId, string? qrCode, string? qrCodeBase64, string alteradoPor)
        {
            FormaPagamento = EEcoFormaPagamento.Pix;
            TransacaoId = transacaoId;
            QrCode = qrCode;
            QrCodeBase64 = qrCodeBase64;
            StatusPagamentoCodigo = EEcoStatusPagamento.PagamentoIniciado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-023: pagamento boleto grava link.</summary>
        public void IniciarPagamentoBoleto(string? transacaoId, string? linkBoleto, string alteradoPor)
        {
            FormaPagamento = EEcoFormaPagamento.Boleto;
            TransacaoId = transacaoId;
            LinkBoleto = linkBoleto;
            StatusPagamentoCodigo = EEcoStatusPagamento.PagamentoIniciado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-024: pedido só passa a pagamento confirmado quando o provedor confirmar.</summary>
        public void ConfirmarPagamento(string? statusPagamento, string? statusDetalhe, string alteradoPor)
        {
            StatusPagamentoCodigo = EEcoStatusPagamento.PagamentoConfirmado;
            StatusPagamento = statusPagamento;
            StatusDetalhe = statusDetalhe;
            StatusPreparacaoCodigo = EEcoStatusPreparacao.Aprovado;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-035: pedido convertido em venda grava o vínculo da venda gerada.</summary>
        public void VincularVenda(Guid vendaId, string? numeroNfe, string alteradoPor)
        {
            VendaId = vendaId;
            NumeroNfe = numeroNfe;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-041: alteração de rastreio atualiza o código do pedido.</summary>
        public void AtualizarRastreio(string codigoRastreio, string alteradoPor)
        {
            CodigoRastreio = codigoRastreio;
            StatusPreparacaoCodigo = EEcoStatusPreparacao.Enviado;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarPreparacao(EEcoStatusPreparacao status, string alteradoPor)
        {
            StatusPreparacaoCodigo = status;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ECO-031: grava o cupom (6 caracteres) e recalcula o total com o desconto aplicado.</summary>
        public void AplicarCupom(string codigoCupom, decimal descontoCupom, string alteradoPor)
        {
            CupomDesconto = codigoCupom;
            ValorTotal = ValorTotal + ValorDesconto - descontoCupom; // remove desconto anterior e aplica o novo
            ValorDesconto = descontoCupom;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>Item do pedido e-commerce (eco_pedido_item). Fonte: EF §8.5. ECO-018..ECO-020.</summary>
    public class EcoPedidoItem : EntidadeSaaSBase
    {
        public Guid PedidoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public Guid? VariacaoId { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }

        protected EcoPedidoItem() { }

        public EcoPedidoItem(Guid pedidoId, Guid produtoId, decimal quantidade, Guid? variacaoId, decimal valorUnitario, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PedidoId = pedidoId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            VariacaoId = variacaoId; // ECO-019: zero/ausente representa produto simples
            ValorUnitario = valorUnitario; // ECO-020: preço de e-commerce do produto quando informado
            ValorTotal = quantidade * valorUnitario;
            AddNotifications(new Contract<EcoPedidoItem>()
                .Requires()
                .AreNotEquals(pedidoId, Guid.Empty, nameof(PedidoId), "O pedido é obrigatório. [Origem: EcoPedidoItem]")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto é obrigatório. [Origem: EcoPedidoItem]")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero. [Origem: EcoPedidoItem]"));
        }
    }

    /// <summary>Histórico/auditoria do e-commerce (eco_historico). Fonte: EF §8.12. ECO-044.</summary>
    public class EcoHistorico : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? DadosAnteriores { get; private set; }
        public string? DadosNovos { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public Guid? ClienteId { get; private set; }
        public DateTime RegistradoEm { get; private set; }

        protected EcoHistorico() { }

        public EcoHistorico(string entidade, Guid entidadeId, string evento, string? dadosAnteriores, string? dadosNovos, Guid? usuarioId, Guid? clienteId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeId = entidadeId;
            Evento = evento;
            DadosAnteriores = dadosAnteriores;
            DadosNovos = dadosNovos;
            UsuarioId = usuarioId;
            ClienteId = clienteId;
            RegistradoEm = DateTime.UtcNow;
            AddNotifications(new Contract<EcoHistorico>()
                .Requires()
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade auditada é obrigatória. [Origem: EcoHistorico]")
                .IsNotNullOrEmpty(evento, nameof(Evento), "O evento é obrigatório. [Origem: EcoHistorico]"));
        }
    }
}
