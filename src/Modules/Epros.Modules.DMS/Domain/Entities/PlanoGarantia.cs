using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class PlanoGarantia : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public int Duracao { get; private set; }
        public string DuracaoTipo { get; private set; } = string.Empty; // Dias, Meses, Anos
        public string Status { get; private set; } = "Ativo";

        protected PlanoGarantia() { } // EF Core

        public PlanoGarantia(
            string codigo,
            string? nome,
            string? descricao,
            int duracao,
            string duracaoTipo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PlanoGarantia>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do plano é obrigatório.")
                .IsGreaterThan(duracao, 0, nameof(Duracao), "A duração do plano deve ser maior que zero.")
                .IsNotNullOrEmpty(duracaoTipo, nameof(DuracaoTipo), "O tipo de duração é obrigatório.")
            );

            if (duracaoTipo != "Dias" && duracaoTipo != "Meses" && duracaoTipo != "Anos")
            {
                AddNotification(nameof(DuracaoTipo), "O tipo de duração deve ser 'Dias', 'Meses' ou 'Anos'.");
            }

            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            Duracao = duracao;
            DuracaoTipo = duracaoTipo;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
