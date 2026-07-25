using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Item de divulgacao de um framework (EF RELATORIOS_ESG 11.3). Entidade mestre.</summary>
    public class RequisitoRel : EntidadeSaaSBase
    {
        public Guid FrameworkId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string TipoResposta { get; private set; } = string.Empty; // Quantitativo, Narrativo, Misto
        public bool Obrigatorio { get; private set; }
        public int Ordem { get; private set; }

        protected RequisitoRel() { } // EF Core

        public RequisitoRel(
            Guid frameworkId,
            string codigo,
            string titulo,
            string tipoResposta,
            bool obrigatorio,
            int ordem,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FrameworkId = frameworkId;
            Codigo = codigo;
            Titulo = titulo;
            TipoResposta = tipoResposta;
            Obrigatorio = obrigatorio;
            Ordem = ordem;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RequisitoRel>()
                .Requires()
                .AreNotEquals(FrameworkId, Guid.Empty, nameof(FrameworkId), "O framework e obrigatorio. [Origem: RequisitoRel]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do requisito e obrigatorio. [Origem: RequisitoRel]")
                .IsNotNullOrEmpty(Titulo, nameof(Titulo), "O titulo e obrigatorio. [Origem: RequisitoRel]"));
        }
    }
}
