using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    public class CaixaMovimento : EntidadeSaaSBase
    {
        public Guid CaixaId { get; private set; }
        // 'Suprimento'/'Sangria' = ajustes manuais de gaveta (entram no cálculo físico do fechamento).
        // 'Venda' (T3) = recebimento de venda por forma de pagamento; é um LANÇAMENTO de detalhamento
        // por forma no caixa, NÃO somado ao total físico Suprimento/Sangria — o fechamento do caixa já
        // contabiliza a venda pelo Venda.Total (ver SincronizarCaixasCommandHandler), então somar aqui
        // seria dupla-contagem. Serve para a quebra por forma (Dinheiro/Cartão/Pix...) que faltava.
        public string Tipo { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string? Observacao { get; private set; }

        protected CaixaMovimento() { } // EF Core

        public CaixaMovimento(Guid id, Guid syncId, Guid caixaId, string tipo, decimal valor, string? observacao, string tenantId, string criadoPor, DateTime criadoEm)
            : base(id, syncId, tenantId, criadoPor, criadoEm)
        {
            AddNotifications(new Contract<CaixaMovimento>()
                .Requires()
                .AreNotEquals(caixaId, Guid.Empty, nameof(CaixaId), "O ID do caixa é obrigatório.")
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "O tipo de movimentação é obrigatório.")
                .IsGreaterThan(valor, 0m, nameof(Valor), "O valor da movimentação deve ser maior que zero.")
            );

            if (tipo != "Suprimento" && tipo != "Sangria" && tipo != "Venda")
            {
                AddNotification(nameof(Tipo), "O tipo de movimentação deve ser 'Suprimento', 'Sangria' ou 'Venda'.");
            }

            CaixaId = caixaId;
            Tipo = tipo;
            Valor = valor;
            Observacao = observacao;
        }
    }
}
