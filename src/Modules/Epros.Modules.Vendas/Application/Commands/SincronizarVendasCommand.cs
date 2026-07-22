using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    public class VendaSyncDto
    {
        public Guid Id { get; set; }
        public Guid SyncId { get; set; }
        public string CaixaId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public List<VendaItemSyncDto> Itens { get; set; } = new();
    }

    public class VendaItemSyncDto
    {
        public Guid ProdutoId { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public record SincronizarVendasCommand(List<VendaSyncDto> Vendas) : ICommand;
}
