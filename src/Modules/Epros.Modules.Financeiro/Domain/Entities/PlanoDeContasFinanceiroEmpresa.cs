using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Entidade de junção que restaura a relação N:N legada entre PlanoDeContasFinanceiro e Empresa
    /// (legado: ICollection&lt;Empresa&gt; Empresas em PlanoDeContasFinanceiro).
    /// EmpresaId referencia o módulo Plataforma (GestaoClientes) por Guid FK — sem navegação cruzada.
    /// </summary>
    public class PlanoDeContasFinanceiroEmpresa : EntidadeSaaSBase
    {
        public Guid PlanoDeContasFinanceiroId { get; private set; }
        public Guid EmpresaId { get; private set; }

        // Navegação intra-módulo
        public PlanoDeContasFinanceiro PlanoDeContasFinanceiro { get; private set; } = null!;

        protected PlanoDeContasFinanceiroEmpresa() { } // EF Core

        public PlanoDeContasFinanceiroEmpresa(Guid planoDeContasFinanceiroId, Guid empresaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoDeContasFinanceiroId = planoDeContasFinanceiroId;
            EmpresaId = empresaId;
        }
    }
}
