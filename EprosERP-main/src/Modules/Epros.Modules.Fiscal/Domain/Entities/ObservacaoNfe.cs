using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class ObservacaoNfe : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;

        protected ObservacaoNfe() { } // EF Core

        public ObservacaoNfe(string descricao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            Validar();
        }

        public void Alterar(string descricao, string alteradoPor)
        {
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ObservacaoNfe>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "O campo Descrição é obrigatório [Origem: ObservacaoNfe]")
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 5000, nameof(Descricao), "O campo Descrição deve ter no máximo 5000 caracteres [Origem: ObservacaoNfe]")
            );
        }
    }
}
