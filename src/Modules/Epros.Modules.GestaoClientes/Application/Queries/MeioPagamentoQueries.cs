using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    /// <summary>1.08B — Lista os meios de pagamento salvos (cartões-on-file) do cliente do tenant corrente.</summary>
    public record ListarMeiosPagamentoQuery() : IQuery<List<MeioPagamentoClienteDto>>;

    public class MeioPagamentoClienteDto
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; } = "Cartao";
        public string? Bandeira { get; set; }
        public string? UltimosQuatro { get; set; }
        public int? ValidadeMes { get; set; }
        public int? ValidadeAno { get; set; }
        public bool Padrao { get; set; }
    }
}
