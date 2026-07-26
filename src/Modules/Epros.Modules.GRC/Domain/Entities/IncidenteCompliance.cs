using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    public class IncidenteCompliance : EntidadeSaaSBase
    {
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Origem { get; private set; } = "Auditoria"; // SoD, Denuncia, Auditoria, Seguranca
        public string Status { get; private set; } = "Aberto"; // Aberto, EmInvestigacao, Resolvido, Cancelado
        public string Gravidade { get; private set; } = "Media"; // Baixa, Media, Alta, Critica
        public string? Resolucao { get; private set; }
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataFechamento { get; private set; }

        protected IncidenteCompliance() { } // EF Core

        public IncidenteCompliance(
            string titulo,
            string descricao,
            string origem,
            string gravidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<IncidenteCompliance>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título do incidente é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição do incidente é obrigatória.")
                .IsTrue(origem == "SoD" || origem == "Denuncia" || origem == "Auditoria" || origem == "Seguranca", nameof(Origem), "A origem deve ser 'SoD', 'Denuncia', 'Auditoria' ou 'Seguranca'.")
                .IsTrue(gravidade == "Baixa" || gravidade == "Media" || gravidade == "Alta" || gravidade == "Critica", nameof(Gravidade), "A gravidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.")
            );

            Titulo = titulo;
            Descricao = descricao;
            Origem = origem;
            Gravidade = gravidade;
            Status = "Aberto";
            DataAbertura = DateTime.UtcNow;
        }

        public void Investigar(string usuario)
        {
            if (Status != "Aberto")
            {
                AddNotification(nameof(Status), "O incidente só pode ser colocado em investigação se estiver Aberto.");
                return;
            }

            Status = "EmInvestigacao";
            MarcarAlterado(usuario);
        }

        public void Resolver(string resolucao, string usuario)
        {
            if (Status != "Aberto" && Status != "EmInvestigacao")
            {
                AddNotification(nameof(Status), "Apenas incidentes abertos ou em investigação podem ser resolvidos.");
                return;
            }

            if (string.IsNullOrWhiteSpace(resolucao))
            {
                AddNotification(nameof(Resolucao), "A descrição da resolução é obrigatória para encerramento.");
                return;
            }

            Resolucao = resolucao;
            Status = "Resolvido";
            DataFechamento = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == "Resolvido")
            {
                AddNotification(nameof(Status), "Não é possível cancelar um incidente já resolvido.");
                return;
            }

            Status = "Cancelado";
            DataFechamento = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
