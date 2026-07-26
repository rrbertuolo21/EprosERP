using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Fato gerador de um título financeiro (venda, compra, lançamento manual, etc.).
    /// Porte fiel do legado Financeiros/FatoGeradorFinanceiro.
    /// VendaId/CompraId referenciam agregados de OUTROS módulos por Guid FK (sem navegação cruzada).
    /// </summary>
    public class FatoGeradorFinanceiro : EntidadeSaaSBase
    {
        public EOrigem Origem { get; private set; }
        public Guid? VendaId { get; private set; }
        public Guid? CompraId { get; private set; }
        public string? Descricao { get; private set; }

        // EF (navegação intra-módulo)
        public ICollection<ContasAReceber> ContasARecebers { get; private set; } = new List<ContasAReceber>();
        public ICollection<ContasAPagar> ContasAPagars { get; private set; } = new List<ContasAPagar>();

        protected FatoGeradorFinanceiro() { } // EF Core

        public FatoGeradorFinanceiro(EOrigem origem, Guid? vendaId, Guid? compraId, string? descricao,
                                     string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Origem = origem;
            VendaId = vendaId;
            CompraId = compraId;
            Descricao = descricao;
        }

        public void Alterar(EOrigem origem, Guid? vendaId, Guid? compraId, string? descricao, string alteradoPor)
        {
            Origem = origem;
            VendaId = vendaId;
            CompraId = compraId;
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// Duplica o fato gerador para um novo título (porte fiel do legado FatoGeradorFinanceiro.Duplicar).
        /// No legado o clone raso zerava Id/VendaId e reapontava o agregado pai (Venda/Compra) por navegação;
        /// aqui geramos uma nova entidade (novo Guid Id via ctor base) copiando os campos e, opcionalmente,
        /// repontando VendaId/CompraId para o agregado pai por Guid FK (sem navegação cross-module).
        /// </summary>
        public FatoGeradorFinanceiro Duplicar(string criadoPor, Guid? vendaPaiId = null, Guid? compraPaiId = null)
        {
            var clone = new FatoGeradorFinanceiro(Origem, vendaPaiId ?? VendaId, compraPaiId ?? CompraId, Descricao, TenantId, criadoPor);
            return clone;
        }

        /// <summary>Reaponta o vínculo para o agregado de venda pai após duplicação (legado VincularVendaPaiParaClone).</summary>
        public void VincularVendaPaiParaClone(Guid vendaPaiId)
        {
            VendaId = vendaPaiId;
        }
    }
}
