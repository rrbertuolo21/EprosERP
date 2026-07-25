using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-POL — Publico-alvo da politica (grc_pol_publico_alvo). Define quem deve
    /// conhecer/aceitar a politica. Fiel a EF_13_GRC_GESTAO_DE_POLITICAS_V1 (secao 11.3).
    /// </summary>
    public class PoliticaPublicoAlvo : EntidadeSaaSBase
    {
        public Guid PoliticaId { get; private set; }
        // Usuario, Papel, Area, Unidade, Todos
        public string TipoAlvo { get; private set; } = "Todos";
        public Guid? AlvoId { get; private set; }
        public bool ExigeAceite { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected PoliticaPublicoAlvo() { } // EF Core

        public PoliticaPublicoAlvo(
            Guid politicaId,
            string tipoAlvo,
            Guid? alvoId,
            bool exigeAceite,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PoliticaPublicoAlvo>()
                .Requires()
                .IsTrue(politicaId != Guid.Empty, nameof(PoliticaId), "A politica do publico-alvo e obrigatoria.")
                .IsTrue(tipoAlvo == "Usuario" || tipoAlvo == "Papel" || tipoAlvo == "Area" || tipoAlvo == "Unidade" || tipoAlvo == "Todos",
                    nameof(TipoAlvo), "O tipo de alvo deve ser 'Usuario', 'Papel', 'Area', 'Unidade' ou 'Todos'.")
            );

            PoliticaId = politicaId;
            TipoAlvo = tipoAlvo;
            AlvoId = alvoId;
            ExigeAceite = exigeAceite;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
