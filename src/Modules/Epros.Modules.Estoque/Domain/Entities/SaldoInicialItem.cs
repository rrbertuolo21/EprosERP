using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Linha de saldo inicial importada ou digitada (EF Movimentação Manual e Ajustes §15.9).
    /// </summary>
    public class SaldoInicialItem : EntidadeSaaSBase
    {
        public Guid SaldoInicialImportacaoId { get; private set; }
        public string ProdutoCodigo { get; private set; } = string.Empty;
        public Guid? ProdutoId { get; private set; }
        public string? LocalNome { get; private set; }
        public Guid? LocalId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal CustoUnitario { get; private set; }
        public string? Lote { get; private set; }
        public DateTime? DataValidade { get; private set; }
        public string? MensagemErro { get; private set; }

        // Navegação intra-módulo
        public SaldoInicialImportacao? Importacao { get; private set; }

        protected SaldoInicialItem() { } // EF Core

        public SaldoInicialItem(Guid saldoInicialImportacaoId, string produtoCodigo, Guid? produtoId, string? localNome, Guid? localId, decimal quantidade, decimal custoUnitario, string? lote, DateTime? dataValidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SaldoInicialImportacaoId = saldoInicialImportacaoId;
            ProdutoCodigo = produtoCodigo ?? string.Empty;
            ProdutoId = produtoId;
            LocalNome = localNome;
            LocalId = localId;
            Quantidade = quantidade;
            CustoUnitario = custoUnitario;
            Lote = lote;
            DataValidade = dataValidade;
        }

        public void Validar() { }

        public void RegistrarErro(string mensagem)
        {
            MensagemErro = mensagem;
        }
    }
}
