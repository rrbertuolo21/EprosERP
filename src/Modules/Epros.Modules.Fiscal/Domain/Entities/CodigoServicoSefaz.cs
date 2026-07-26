using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CodigoServicoSefaz : EntidadeSaaSBase, IGlobalEntity
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;

        protected CodigoServicoSefaz() { } // EF Core

        public CodigoServicoSefaz(string codigo, string descricao, string criadoPor) : base("system", criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Validar();
        }

        public void Alterar(string codigo, string descricao, string alteradoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CodigoServicoSefaz>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório.")
                .IsLowerOrEqualsThan(Codigo, 5, nameof(Codigo), "O código deve ter no máximo 5 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao, 1000, nameof(Descricao), "A descrição deve ter no máximo 1000 caracteres.")
            );
        }
    }
}
