using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class Cest : EntidadeSaaSBase, IGlobalEntity
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;

        protected Cest() { } // EF Core

        public Cest(string codigo, string descricao, string criadoPor) : base("system", criadoPor)
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
            AddNotifications(new Contract<Cest>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório [Origem: Cest]")
                .IsLowerOrEqualsThan((Codigo ?? "").Length, 7, nameof(Codigo), "O campo Codigo deve ter no máximo 7 caracteres [Origem: Cest]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 1000, nameof(Descricao), "O campo Descricao deve ter no máximo 1000 caracteres [Origem: Cest]")
            );
        }
    }
}
