using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class EnquadramentoIpi : EntidadeSaaSBase, IGlobalEntity
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public ETipoOperacaoEnquadramentoIpi TipoOperacao { get; private set; }

        protected EnquadramentoIpi() { } // EF Core

        public EnquadramentoIpi(
            string codigo,
            string descricao,
            ETipoOperacaoEnquadramentoIpi tipoOperacao,
            string criadoPor) : base("system", criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            TipoOperacao = tipoOperacao;
            Validar();
        }

        public void Alterar(
            string codigo,
            string descricao,
            ETipoOperacaoEnquadramentoIpi tipoOperacao,
            string alteradoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            TipoOperacao = tipoOperacao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EnquadramentoIpi>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório [Origem: EnquadramentoIpi]")
                .IsLowerOrEqualsThan((Codigo ?? "").Length, 3, nameof(Codigo), "O campo Codigo deve ter no máximo 3 caracteres [Origem: EnquadramentoIpi]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: EnquadramentoIpi]")
                .IsTrue(Enum.IsDefined(typeof(ETipoOperacaoEnquadramentoIpi), TipoOperacao), nameof(TipoOperacao), "TipoOperacao não consta na lista [Origem: EnquadramentoIpi]")
            );
        }
    }
}
