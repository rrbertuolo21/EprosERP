using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Grupo de empresas (matriz/filiais) - CAD-PEM 12.13.</summary>
    public class EmpresaGrupo : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        protected EmpresaGrupo() { } // EF Core

        public EmpresaGrupo(string nome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Ativo = true;
            Validar();
        }

        public void Alterar(string nome, string alteradoPor)
        {
            Nome = nome;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EmpresaGrupo>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "Nome é obrigatório [Origem: EmpresaGrupo]")
                .HasMaxLen(Nome ?? string.Empty, 250, nameof(Nome), "Nome deve ter no máximo 250 caracteres [Origem: EmpresaGrupo]")
            );
        }
    }
}
