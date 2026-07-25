using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Imposto : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public decimal Rate { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }

        protected Imposto() { } // EF Core

        public Imposto(
            string nome,
            decimal rate,
            bool isActive,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Imposto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do imposto é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome do imposto deve ter no máximo 100 caracteres.")
                .IsGreaterThan(rate, -0.01m, nameof(Rate), "A alíquota deve ser maior ou igual a zero.")
            );

            if (vigenciaInicio != null && vigenciaFim != null)
            {
                AddNotifications(new Contract<Imposto>()
                    .Requires()
                    .IsTrue(vigenciaFim > vigenciaInicio, nameof(VigenciaFim), "A vigência final deve ser posterior à inicial.")
                );
            }

            Nome = nome;
            Rate = rate;
            IsActive = isActive;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
        }

        public void Atualizar(
            string nome,
            decimal rate,
            bool isActive,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string alteradoPor)
        {
            AddNotifications(new Contract<Imposto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do imposto é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome do imposto deve ter no máximo 100 caracteres.")
                .IsGreaterThan(rate, -0.01m, nameof(Rate), "A alíquota deve ser maior ou igual a zero.")
            );

            if (vigenciaInicio != null && vigenciaFim != null)
            {
                AddNotifications(new Contract<Imposto>()
                    .Requires()
                    .IsTrue(vigenciaFim > vigenciaInicio, nameof(VigenciaFim), "A vigência final deve ser posterior à inicial.")
                );
            }

            if (IsValid)
            {
                Nome = nome;
                Rate = rate;
                IsActive = isActive;
                VigenciaInicio = vigenciaInicio;
                VigenciaFim = vigenciaFim;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Inativar(string alteradoPor)
        {
            IsActive = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            IsActive = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
