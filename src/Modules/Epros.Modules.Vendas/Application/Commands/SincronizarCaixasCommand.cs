using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    public class CaixaSyncDto
    {
        public Guid Id { get; set; }
        public Guid SyncId { get; set; }
        public string OperadorId { get; set; } = string.Empty;
        public decimal SaldoAbertura { get; set; }
        public decimal? SaldoFechamento { get; set; }
        public decimal? DiferencaFechamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime? FechadoEm { get; set; }
        public List<CaixaMovimentoSyncDto> Movimentos { get; set; } = new();
    }

    public class CaixaMovimentoSyncDto
    {
        public Guid Id { get; set; }
        public Guid SyncId { get; set; }
        public string Tipo { get; set; } = string.Empty; // 'Suprimento', 'Sangria'
        public decimal Valor { get; set; }
        public string? Observacao { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public record SincronizarCaixasCommand(List<CaixaSyncDto> Caixas) : ICommand;
}
