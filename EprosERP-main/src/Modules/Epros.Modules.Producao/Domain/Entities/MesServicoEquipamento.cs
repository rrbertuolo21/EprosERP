using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Equipamento usado no serviço (prd_mes_servico_equipamento). Fiel ao EF §17.
    /// MES-REG-007/008: exige serviço e equipamento.
    /// </summary>
    public class MesServicoEquipamento : EntidadeSaaSBase
    {
        public Guid ServicoId { get; private set; }
        public Guid EquipamentoId { get; private set; }

        protected MesServicoEquipamento() { } // EF Core

        public MesServicoEquipamento(Guid servicoId, Guid equipamentoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ServicoId = servicoId;
            EquipamentoId = equipamentoId;

            AddNotifications(new Contract<MesServicoEquipamento>()
                .Requires()
                .AreNotEquals(servicoId, Guid.Empty, nameof(ServicoId), "O vínculo deve referenciar o serviço [Origem: MesServicoEquipamento]. (MES-REG-007)")
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O vínculo deve referenciar o equipamento [Origem: MesServicoEquipamento]. (MES-REG-008)")
            );
        }
    }
}
