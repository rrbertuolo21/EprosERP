using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    public class Equipamento : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Codigo { get; private set; } = string.Empty;
        public string Setor { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Ativo"; // Ativo, EmManutencao, Inativo
        public DateTime DataAquisicao { get; private set; }
        public string Criticidade { get; private set; } = "Media"; // Alta, Media, Baixa

        protected Equipamento() { } // EF Core

        public Equipamento(
            string nome,
            string codigo,
            string setor,
            DateTime dataAquisicao,
            string criticidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Equipamento>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do equipamento é obrigatório.")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O Código do equipamento é obrigatório.")
                .IsNotNullOrEmpty(setor, nameof(Setor), "O Setor é obrigatório.")
                .IsTrue(criticidade == "Alta" || criticidade == "Media" || criticidade == "Baixa", nameof(Criticidade), "A Criticidade deve ser 'Alta', 'Media' ou 'Baixa'.")
            );

            Nome = nome;
            Codigo = codigo;
            Setor = setor;
            DataAquisicao = dataAquisicao.Date;
            Criticidade = criticidade;
            Status = "Ativo";
        }

        public void AlterarStatus(string novoStatus, string usuario)
        {
            if (novoStatus != "Ativo" && novoStatus != "EmManutencao" && novoStatus != "Inativo")
            {
                AddNotification(nameof(Status), "Status inválido.");
                return;
            }
            Status = novoStatus;
            MarcarAlterado(usuario);
        }
    }
}
