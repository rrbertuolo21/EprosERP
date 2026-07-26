using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class TipoServicoConcessionaria : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected TipoServicoConcessionaria() { } // EF Core

        public TipoServicoConcessionaria(
            string codigo,
            string nome,
            string? descricao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TipoServicoConcessionaria>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do tipo de serviço é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do tipo de serviço é obrigatório.")
            );

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
