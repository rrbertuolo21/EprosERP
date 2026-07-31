using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.08B — Meio de pagamento SALVO do cliente (cartão-on-file tokenizado no Mercado Pago).
    ///
    /// ⛔ PCI (obrigatório): o dado de cartão CRU (PAN/CVV) NUNCA é persistido nem trafega pelo backend.
    /// O front tokeniza o cartão com a lib do Mercado Pago e envia apenas o TOKEN; o backend cria o
    /// cartão-on-file no MP (Customers/Cards) e guarda somente os identificadores opacos do gateway
    /// (<see cref="CustomerIdGateway"/>, <see cref="CardIdGateway"/>) + metadados NÃO sensíveis para exibição
    /// (bandeira, últimos 4 dígitos, validade). Com esses identificadores a cobrança recorrente é feita
    /// pelo gateway sem o cartão cru voltar a tocar o backend.
    /// </summary>
    public class MeioPagamentoCliente : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }

        /// <summary>Tipo do meio salvo (hoje: "Cartao").</summary>
        public string Tipo { get; private set; } = "Cartao";

        /// <summary>Bandeira do cartão (ex.: visa, master) — metadado NÃO sensível.</summary>
        public string? Bandeira { get; private set; }

        /// <summary>Últimos 4 dígitos — metadado NÃO sensível (nunca o PAN completo).</summary>
        public string? UltimosQuatro { get; private set; }

        public int? ValidadeMes { get; private set; }
        public int? ValidadeAno { get; private set; }

        /// <summary>Id opaco do CLIENTE no gateway (MP customer_id). Não é dado de cartão.</summary>
        public string CustomerIdGateway { get; private set; } = string.Empty;

        /// <summary>Id opaco do CARTÃO salvo no gateway (MP card_id). Não é o PAN.</summary>
        public string CardIdGateway { get; private set; } = string.Empty;

        /// <summary>Meio padrão do cliente para débito automático (recorrência).</summary>
        public bool Padrao { get; private set; }

        public bool Ativo { get; private set; } = true;

        protected MeioPagamentoCliente() { } // EF Core

        public MeioPagamentoCliente(
            Guid clienteId,
            string customerIdGateway,
            string cardIdGateway,
            string? bandeira,
            string? ultimosQuatro,
            int? validadeMes,
            int? validadeAno,
            bool padrao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<MeioPagamentoCliente>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsNotNullOrEmpty(customerIdGateway, nameof(CustomerIdGateway), "customer_id do gateway é obrigatório")
                .IsNotNullOrEmpty(cardIdGateway, nameof(CardIdGateway), "card_id do gateway é obrigatório")
            );

            ClienteId = clienteId;
            CustomerIdGateway = customerIdGateway;
            CardIdGateway = cardIdGateway;
            Bandeira = bandeira;
            UltimosQuatro = ultimosQuatro;
            ValidadeMes = validadeMes;
            ValidadeAno = validadeAno;
            Padrao = padrao;
            Ativo = true;
        }

        public void DefinirPadrao(bool padrao, string alteradoPor)
        {
            Padrao = padrao;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            Ativo = false;
            Padrao = false;
            MarcarAlterado(alteradoPor);
        }
    }
}
