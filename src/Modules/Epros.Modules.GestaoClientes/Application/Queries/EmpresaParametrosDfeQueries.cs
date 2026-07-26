using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterEmpresaParametrosDfeQuery(Guid EmpresaId) : IQuery<EmpresaParametrosDfeDto?>;

    public class EmpresaParametrosDfeDto
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public bool DestacarIcmsSt { get; set; }
        public ETipoAmbiente TipoAmbienteNfce { get; set; }
        public ETipoAmbiente TipoAmbienteNfe { get; set; }
        public int SerieNfce { get; set; }
        public long NumeroNfce { get; set; }
        public int SerieNfe { get; set; }
        public long NumeroNfe { get; set; }
    }
}
