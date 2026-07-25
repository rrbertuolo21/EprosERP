using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-BOM — Catálogo de instruções técnicas (prd_bom_instrucao). Código único por tenant (BOM-REG-020).</summary>
    public class BomInstrucao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusAtivoInativo Status { get; private set; } = EStatusAtivoInativo.Ativo;

        protected BomInstrucao() { } // EF Core

        public BomInstrucao(string codigo, string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Status = EStatusAtivoInativo.Ativo;

            AddNotifications(new Contract<BomInstrucao>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código da instrução é obrigatório [Origem: BomInstrucao]. (BOM-REG-021)")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da instrução é obrigatória [Origem: BomInstrucao]. (BOM-REG-021)")
            );
        }

        public void Alterar(string descricao, string alteradoPor)
        {
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EStatusAtivoInativo.Inativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
