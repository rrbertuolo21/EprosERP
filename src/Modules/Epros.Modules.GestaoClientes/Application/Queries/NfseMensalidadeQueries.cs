using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    /// <summary>
    /// 1.08J — Lista (LANDLORD) as necessidades de NFS-e da mensalidade SaaS por competência, para dar
    /// VISIBILIDADE ao operador interno (Siser). Filtro default = Pendente. NÃO emite nada — só consulta.
    /// </summary>
    public record ListarNfseMensalidadesQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        string? Status = "Pendente",
        Guid? ClienteId = null
    ) : IQuery<PagedQueryResult<NfseMensalidadeListaDto>>;

    public class NfseMensalidadeListaDto
    {
        public Guid Id { get; set; }
        public Guid FaturaId { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteRazaoSocial { get; set; } = string.Empty;
        /// <summary>Competência mensal (1º dia do mês, UTC).</summary>
        public DateTime Competencia { get; set; }
        public decimal ValorBase { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public string? NumeroNfse { get; set; }
        public DateTime? EmitidaEm { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
