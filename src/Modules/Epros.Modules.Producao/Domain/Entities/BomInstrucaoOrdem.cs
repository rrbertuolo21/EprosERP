using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-BOM — Vínculo instrução ↔ ordem de produção (prd_bom_instrucao_ordem).
    /// Complementa a execução sem substituir a estrutura de componentes (BOM-REG-022).
    /// </summary>
    public class BomInstrucaoOrdem : EntidadeSaaSBase
    {
        public Guid InstrucaoId { get; private set; }
        public Guid OrdemProducaoId { get; private set; }
        public EStatusAtivoInativo Status { get; private set; } = EStatusAtivoInativo.Ativo;

        protected BomInstrucaoOrdem() { } // EF Core

        public BomInstrucaoOrdem(Guid instrucaoId, Guid ordemProducaoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            InstrucaoId = instrucaoId;
            OrdemProducaoId = ordemProducaoId;
            Status = EStatusAtivoInativo.Ativo;

            AddNotifications(new Contract<BomInstrucaoOrdem>()
                .Requires()
                .AreNotEquals(instrucaoId, Guid.Empty, nameof(InstrucaoId), "A instrução é obrigatória [Origem: BomInstrucaoOrdem].")
                .AreNotEquals(ordemProducaoId, Guid.Empty, nameof(OrdemProducaoId), "A ordem de produção é obrigatória [Origem: BomInstrucaoOrdem].")
            );
        }

        public void Inativar(string alteradoPor)
        {
            Status = EStatusAtivoInativo.Inativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
