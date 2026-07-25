using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class PacoteServico : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected PacoteServico() { } // EF Core

        public PacoteServico(
            string codigo,
            string nome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PacoteServico>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do pacote de serviço é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do pacote de serviço é obrigatório.")
            );

            Codigo = codigo;
            Nome = nome;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
