using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaGrupo : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;

        protected PessoaGrupo() { } // EF Core

        public PessoaGrupo(string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaGrupo>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "O campo Descricao é obrigatório.")
                .HasMaxLen(descricao, 100, nameof(Descricao), "O campo Descricao deve ter no máximo 100 caracteres [Origem: PessoaGrupo]")
            );

            Descricao = descricao;
        }

        public void Atualizar(string descricao, string alteradoPor)
        {
            AddNotifications(new Contract<PessoaGrupo>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "O campo Descricao é obrigatório.")
                .HasMaxLen(descricao, 100, nameof(Descricao), "O campo Descricao deve ter no máximo 100 caracteres [Origem: PessoaGrupo]")
            );

            if (IsValid)
            {
                Descricao = descricao;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
