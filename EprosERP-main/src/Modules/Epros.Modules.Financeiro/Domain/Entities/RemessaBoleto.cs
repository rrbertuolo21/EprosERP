using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Relacionamento remessa-boleto (EF FIN-SF §10.2 sf_remessa_boleto). Usado para idempotência (RSF-036).
    /// </summary>
    public class RemessaBoleto : EntidadeSaaSBase
    {
        public Guid RemessaId { get; private set; }
        public Guid BoletoId { get; private set; }
        public Guid FaturaCobrancaId { get; private set; }

        public Remessa Remessa { get; private set; } = null!;

        protected RemessaBoleto() { } // EF Core

        public RemessaBoleto(Guid remessaId, Guid boletoId, Guid faturaCobrancaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RemessaId = remessaId;
            BoletoId = boletoId;
            FaturaCobrancaId = faturaCobrancaId;
        }
    }
}
