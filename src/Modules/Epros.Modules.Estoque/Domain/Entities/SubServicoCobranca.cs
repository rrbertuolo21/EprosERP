using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Vínculo com a compra do serviço subcontratado (EF Subcontratação §6 `sub_servico_cobranca`).
    /// SUB-009: compra do serviço integra com compras e contas a pagar (integração externa via evento/Outbox;
    /// ver pendências). CompraId é referência externa por FK Guid. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubServicoCobranca : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid? CompraId { get; private set; }
        public decimal? ValorServico { get; private set; }

        protected SubServicoCobranca() { } // EF Core

        public SubServicoCobranca(Guid ordemId, Guid? compraId, decimal? valorServico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            CompraId = compraId;
            ValorServico = valorServico;
        }
    }
}
