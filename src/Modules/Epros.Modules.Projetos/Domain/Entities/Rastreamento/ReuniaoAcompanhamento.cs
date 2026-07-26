using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Rastreamento
{
    /// <summary>
    /// Reuniao de acompanhamento do projeto. Origem: EF PRJ-RST 4.8 (prj_rst_reuniao).
    /// PRJ-RST-RN-026: data/hora final deve ser igual ou posterior a inicial.
    /// </summary>
    public class ReuniaoAcompanhamento : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Tipo { get; private set; }
        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }
        public string? Departamento { get; private set; }
        public string? Local { get; private set; }
        public Guid? OrganizadorId { get; private set; }
        public Guid? RelatorId { get; private set; }

        protected ReuniaoAcompanhamento() { } // EF Core

        public ReuniaoAcompanhamento(
            Guid projetoId,
            string nome,
            string? tipo,
            DateTime inicio,
            DateTime fim,
            string? departamento,
            string? local,
            Guid? organizadorId,
            Guid? relatorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ReuniaoAcompanhamento>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ReuniaoAcompanhamento]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da reuniao e obrigatorio. [Origem: ReuniaoAcompanhamento]"));

            if (fim < inicio)
                AddNotification(nameof(Fim), "A data/hora final deve ser igual ou posterior a inicial. [Origem: ReuniaoAcompanhamento]");

            ProjetoId = projetoId;
            Nome = nome;
            Tipo = tipo;
            Inicio = inicio;
            Fim = fim;
            Departamento = departamento;
            Local = local;
            OrganizadorId = organizadorId;
            RelatorId = relatorId;
        }
    }
}
