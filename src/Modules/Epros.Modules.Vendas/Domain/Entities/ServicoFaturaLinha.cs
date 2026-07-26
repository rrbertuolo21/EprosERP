using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Linha (item) de fatura de serviço (ven_servico_fatura_linha).
    /// Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1 §10 e §19.3.
    /// A linha referencia um serviço do catálogo, nunca produto físico (EF §10.15) e
    /// não movimenta estoque (EF §10.16).
    /// </summary>
    public class ServicoFaturaLinha : EntidadeSaaSBase
    {
        public Guid FaturaId { get; private set; }
        public Guid ServicoId { get; private set; }
        public string? NomeServico { get; private set; }
        public string? Descricao { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal DescontoPercentual { get; private set; }
        public decimal Total { get; private set; }
        public bool Ativo { get; private set; } = true;

        public ServicoFatura Fatura { get; private set; } = null!;

        protected ServicoFaturaLinha() { } // EF Core

        public ServicoFaturaLinha(
            Guid servicoId,
            string? nomeServico,
            string? descricao,
            decimal quantidade,
            decimal precoUnitario,
            decimal descontoPercentual,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ServicoId = servicoId;
            NomeServico = nomeServico;
            Descricao = descricao;
            // EF §10.4/§10.5: ao selecionar o serviço, quantidade inicia em 1 e desconto em 0.
            Quantidade = quantidade <= 0 ? 1m : quantidade;
            PrecoUnitario = precoUnitario < 0 ? 0 : precoUnitario;
            DescontoPercentual = descontoPercentual < 0 ? 0 : descontoPercentual;
            CalcularTotal();
            Validar();
        }

        internal void DefinirFaturaId(Guid faturaId) => FaturaId = faturaId;

        public void Alterar(decimal quantidade, decimal precoUnitario, decimal descontoPercentual, string? descricao, string alteradoPor)
        {
            Quantidade = quantidade <= 0 ? 1m : quantidade;
            PrecoUnitario = precoUnitario < 0 ? 0 : precoUnitario;
            DescontoPercentual = descontoPercentual < 0 ? 0 : descontoPercentual;
            Descricao = descricao;
            CalcularTotal();
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>
        /// EF §10.12: total = quantidade x preço unitário - quantidade x preço unitário x % desconto.
        /// </summary>
        public void CalcularTotal()
        {
            var bruto = Quantidade * PrecoUnitario;
            var descontoValor = bruto * (DescontoPercentual / 100m);
            Total = decimal.Round(bruto - descontoValor, 2);
        }

        /// <summary>Valor de desconto absoluto da linha (para compor o desconto total da fatura, EF §11.2).</summary>
        public decimal DescontoValor() => decimal.Round(Quantidade * PrecoUnitario * (DescontoPercentual / 100m), 2);

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ServicoFaturaLinha>()
                .Requires()
                .AreNotEquals(ServicoId, Guid.Empty, nameof(ServicoId), "A linha deve referenciar um serviço do catálogo. [Origem: ServicoFaturaLinha]")
                .IsGreaterThan(Quantidade, 0m, nameof(Quantidade), "A quantidade da linha deve ser maior que zero. [Origem: ServicoFaturaLinha]")
                .IsGreaterThan(PrecoUnitario, -0.01m, nameof(PrecoUnitario), "O preço unitário não pode ser negativo. [Origem: ServicoFaturaLinha]")
                .IsGreaterThan(DescontoPercentual, -0.01m, nameof(DescontoPercentual), "O desconto da linha não pode ser negativo. [Origem: ServicoFaturaLinha]"));

            if (!string.IsNullOrEmpty(Descricao) && Descricao.Length > 500)
                AddNotification(nameof(Descricao), "A descrição da linha deve ter no máximo 500 caracteres. [Origem: ServicoFaturaLinha]");
        }
    }
}
