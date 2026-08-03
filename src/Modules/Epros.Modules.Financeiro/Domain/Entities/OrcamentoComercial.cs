using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de orçamento comercial (EF FIN-PO §6.2/§8.2/§12.3 po_orcamento_comercial).
    /// Agregado raiz: cliente, vendedor, transportadora, condição de pagamento e produtos são FKs
    /// cross-module por Guid (mesmo padrão de LinhaOrcamento.ContaId) — sem navegação entre módulos.
    /// Totais (subtotal, desconto, comissão, frete, total) são calculados quando os valores estão
    /// informados (§8.2 passo 4). Submódulo de evolução — sobe desabilitado (ABAC nega por padrão).
    /// </summary>
    public class OrcamentoComercial : EntidadeSaaSBase
    {
        public Guid VendedorId { get; private set; }
        public Guid TransportadoraId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid CondicaoPagamentoId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public DateTime? DataCadastro { get; private set; }
        public DateTime? DataEntrega { get; private set; }
        public DateTime? Validade { get; private set; }
        public string TipoFrete { get; private set; } = string.Empty;
        public decimal? ValorSubtotal { get; private set; }
        public decimal? ValorFrete { get; private set; }
        public decimal? TaxaComissao { get; private set; }
        public decimal? ValorComissao { get; private set; }
        public decimal? TaxaDesconto { get; private set; }
        public decimal? ValorDesconto { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public string Observacao { get; private set; } = string.Empty;
        public string StatusPedido { get; private set; } = string.Empty;

        private readonly List<OrcamentoComercialItem> _itens = new();
        public IReadOnlyCollection<OrcamentoComercialItem> Itens => _itens.AsReadOnly();

        protected OrcamentoComercial() { } // EF Core

        public OrcamentoComercial(
            Guid vendedorId, Guid transportadoraId, Guid clienteId, Guid condicaoPagamentoId,
            string tipo, string codigo, string tipoFrete, string observacao, string statusPedido,
            DateTime? dataCadastro, DateTime? dataEntrega, DateTime? validade,
            decimal? valorFrete, decimal? taxaComissao, decimal? taxaDesconto,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendedorId = vendedorId;
            TransportadoraId = transportadoraId;
            ClienteId = clienteId;
            CondicaoPagamentoId = condicaoPagamentoId;
            Tipo = tipo;
            Codigo = codigo;
            TipoFrete = tipoFrete;
            Observacao = observacao;
            StatusPedido = statusPedido;
            DataCadastro = dataCadastro ?? DateTime.UtcNow;
            DataEntrega = dataEntrega;
            Validade = validade;
            ValorFrete = valorFrete;
            TaxaComissao = taxaComissao;
            TaxaDesconto = taxaDesconto;
            Validar();
        }

        public OrcamentoComercialItem AdicionarItem(Guid produtoId, decimal? quantidade, decimal? valorUnitario, decimal? taxaDesconto, string tenantId, string criadoPor)
        {
            var item = new OrcamentoComercialItem(Id, produtoId, quantidade, valorUnitario, taxaDesconto, tenantId, criadoPor);
            _itens.Add(item);
            return item;
        }

        /// <summary>Substitui os itens do orçamento (usado em edição). Só permitido enquanto ativo (não excluído).</summary>
        public void LimparItens() => _itens.Clear();

        public void AlterarCabecalho(
            Guid vendedorId, Guid transportadoraId, Guid clienteId, Guid condicaoPagamentoId,
            string tipo, string codigo, string tipoFrete, string observacao, string statusPedido,
            DateTime? dataEntrega, DateTime? validade,
            decimal? valorFrete, decimal? taxaComissao, decimal? taxaDesconto, string usuario)
        {
            VendedorId = vendedorId;
            TransportadoraId = transportadoraId;
            ClienteId = clienteId;
            CondicaoPagamentoId = condicaoPagamentoId;
            Tipo = tipo;
            Codigo = codigo;
            TipoFrete = tipoFrete;
            Observacao = observacao;
            StatusPedido = statusPedido;
            DataEntrega = dataEntrega;
            Validade = validade;
            ValorFrete = valorFrete;
            TaxaComissao = taxaComissao;
            TaxaDesconto = taxaDesconto;
            MarcarAlterado(usuario);
            Validar();
        }

        /// <summary>
        /// Calcula totais quando os valores estão informados (EF FIN-PO §8.2). Cada item calcula seu
        /// subtotal (qtd × unitário), desconto (taxa) e total; o cabeçalho agrega subtotal dos itens e
        /// aplica desconto/comissão por taxa e o frete informado.
        /// </summary>
        public void CalcularTotais()
        {
            decimal subtotal = 0m;
            foreach (var item in _itens)
            {
                item.CalcularTotais();
                subtotal += item.ValorTotal ?? 0m;
            }

            ValorSubtotal = subtotal;
            ValorDesconto = TaxaDesconto.HasValue ? Arredondar(subtotal * TaxaDesconto.Value / 100m) : ValorDesconto;
            ValorComissao = TaxaComissao.HasValue ? Arredondar(subtotal * TaxaComissao.Value / 100m) : ValorComissao;
            ValorTotal = subtotal - (ValorDesconto ?? 0m) + (ValorFrete ?? 0m);
        }

        internal static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        public void Validar()
        {
            Clear();
            var contract = new Contract<OrcamentoComercial>()
                .Requires()
                .IsNotEmpty(VendedorId, nameof(VendedorId), "O vendedor é obrigatório [Origem: OrcamentoComercial]")
                .IsNotEmpty(TransportadoraId, nameof(TransportadoraId), "A transportadora é obrigatória [Origem: OrcamentoComercial]")
                .IsNotEmpty(ClienteId, nameof(ClienteId), "O cliente é obrigatório [Origem: OrcamentoComercial]")
                .IsNotEmpty(CondicaoPagamentoId, nameof(CondicaoPagamentoId), "A condição de pagamento é obrigatória [Origem: OrcamentoComercial]")
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo é obrigatório [Origem: OrcamentoComercial]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório [Origem: OrcamentoComercial]")
                .IsNotNullOrEmpty(TipoFrete, nameof(TipoFrete), "O tipo de frete é obrigatório [Origem: OrcamentoComercial]")
                .IsNotNullOrEmpty(Observacao, nameof(Observacao), "A observação é obrigatória [Origem: OrcamentoComercial]")
                .IsNotNullOrEmpty(StatusPedido, nameof(StatusPedido), "O status do pedido é obrigatório [Origem: OrcamentoComercial]");
            AddNotifications(contract);

            // §6.2: orçamento comercial deve possuir itens.
            if (!_itens.Any())
                AddNotification(nameof(Itens), "O orçamento comercial deve possuir ao menos um item [Origem: OrcamentoComercial]");

            foreach (var item in _itens)
                AddNotifications(item);
        }
    }

    /// <summary>Item de orçamento comercial (EF FIN-PO §12.4 po_orcamento_comercial_item). Produto por Guid FK cross-module.</summary>
    public class OrcamentoComercialItem : EntidadeSaaSBase
    {
        public Guid OrcamentoComercialId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal? Quantidade { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public decimal? ValorSubtotal { get; private set; }
        public decimal? TaxaDesconto { get; private set; }
        public decimal? ValorDesconto { get; private set; }
        public decimal? ValorTotal { get; private set; }

        public OrcamentoComercial OrcamentoComercial { get; private set; } = null!;

        protected OrcamentoComercialItem() { } // EF Core

        public OrcamentoComercialItem(Guid orcamentoComercialId, Guid produtoId, decimal? quantidade, decimal? valorUnitario, decimal? taxaDesconto, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrcamentoComercialId = orcamentoComercialId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            TaxaDesconto = taxaDesconto;
            Validar();
        }

        public void CalcularTotais()
        {
            if (Quantidade.HasValue && ValorUnitario.HasValue)
            {
                var subtotal = OrcamentoComercial.Arredondar(Quantidade.Value * ValorUnitario.Value);
                ValorSubtotal = subtotal;
                ValorDesconto = TaxaDesconto.HasValue ? OrcamentoComercial.Arredondar(subtotal * TaxaDesconto.Value / 100m) : ValorDesconto;
                ValorTotal = subtotal - (ValorDesconto ?? 0m);
            }
            else
            {
                ValorTotal ??= ValorSubtotal;
            }
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<OrcamentoComercialItem>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto é obrigatório [Origem: OrcamentoComercialItem]"));
        }
    }
}
