using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ZonaEntrega : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string CepInicio { get; private set; } = string.Empty;
        public string CepFim { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        protected ZonaEntrega() { } // EF Core

        public ZonaEntrega(
            string nome,
            string cepInicio,
            string cepFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ZonaEntrega>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome da zona de entrega é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "O Nome da zona de entrega deve ter no máximo 100 caracteres.")
                .IsNotNullOrEmpty(cepInicio, nameof(CepInicio), "O CEP de início é obrigatório.")
                .HasMaxLen(cepInicio, 8, nameof(CepInicio), "O CEP de início deve ter no máximo 8 caracteres.")
                .IsNotNullOrEmpty(cepFim, nameof(CepFim), "O CEP de fim é obrigatório.")
                .HasMaxLen(cepFim, 8, nameof(CepFim), "O CEP de fim deve ter no máximo 8 caracteres.")
            );

            Nome = nome;
            CepInicio = cepInicio;
            CepFim = cepFim;
            Ativo = true;
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
