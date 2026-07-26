using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    public class ControleInterno : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Frequencia { get; private set; } = "Mensal"; // Diaria, Semanal, Mensal, Anual
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected ControleInterno() { } // EF Core

        public ControleInterno(
            string codigo,
            string nome,
            string descricao,
            string frequencia,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ControleInterno>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do controle é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do controle é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição do controle é obrigatória.")
                .IsTrue(frequencia == "Diaria" || frequencia == "Semanal" || frequencia == "Mensal" || frequencia == "Anual", nameof(Frequencia), "A frequência deve ser 'Diaria', 'Semanal', 'Mensal' ou 'Anual'.")
            );

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Frequencia = frequencia;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            Status = "Ativo";
            MarcarAlterado(usuario);
        }
    }
}
