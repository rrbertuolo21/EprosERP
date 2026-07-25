using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class FusoHorario : EntidadeSaaSBase, IGlobalEntity
    {
        public string Nome { get; private set; } = string.Empty;
        public string Offset { get; private set; } = string.Empty;
        public string? CodigoIANA { get; private set; }
        public int FusoHorarioId { get; private set; }

        protected FusoHorario() { } // EF Core

        public FusoHorario(string nome, string offset, string? codigoIANA, int fusoHorarioId, string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<FusoHorario>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do fuso horário é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome deve ter no máximo 100 caracteres.")
                .IsNotNullOrEmpty(offset, nameof(Offset), "Offset é obrigatório.")
                .HasMaxLen(offset, 10, nameof(Offset), "Offset deve ter no máximo 10 caracteres.")
            );

            Nome = nome;
            Offset = offset;
            CodigoIANA = codigoIANA;
            FusoHorarioId = fusoHorarioId;
        }

        public void Atualizar(string nome, string offset, string? codigoIANA, int fusoHorarioId, string alteradoPor)
        {
            AddNotifications(new Contract<FusoHorario>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do fuso horário é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome deve ter no máximo 100 caracteres.")
                .IsNotNullOrEmpty(offset, nameof(Offset), "Offset é obrigatório.")
                .HasMaxLen(offset, 10, nameof(Offset), "Offset deve ter no máximo 10 caracteres.")
            );

            if (IsValid)
            {
                Nome = nome;
                Offset = offset;
                CodigoIANA = codigoIANA;
                FusoHorarioId = fusoHorarioId;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
