using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ObterProdutosSyncQuery(DateTime Since) : IQuery<IEnumerable<ProdutoSyncDto>>;

    public class ProdutoSyncDto
    {
        public Guid Id { get; set; }
        public Guid SyncId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public decimal PrecoVenda { get; set; }
        public decimal SaldoEstoque { get; set; }
        public int SyncVersion { get; set; }
        public bool Deletado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? AlteradoEm { get; set; }
        public DateTime? DeletadoEm { get; set; }
    }
}
