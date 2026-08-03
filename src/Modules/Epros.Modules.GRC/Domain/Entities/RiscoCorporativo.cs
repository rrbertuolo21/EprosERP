using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    public class RiscoCorporativo : EntidadeSaaSBase
    {
        public string Titulo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Categoria { get; private set; } = "Operacional"; // Operacional, Financeiro, Legal, Seguranca
        public int Probabilidade { get; private set; } // 1 a 5
        public int Impacto { get; private set; } // 1 a 5
        public int NivelRisco { get; private set; } // Probabilidade * Impacto
        public string Status { get; private set; } = "Identificado"; // Identificado, Mitigado, Inaceitavel
        public string AcoesMitigadoras { get; private set; } = string.Empty;
        // D-TEC-05 — FK opcional para o catálogo normativo único (nunca duplicar catálogo).
        public Guid? TaxonomiaNormativaId { get; private set; }

        protected RiscoCorporativo() { } // EF Core

        /// <summary>D-TEC-05 — classifica o risco num nó da taxonomia normativa única.</summary>
        public void Classificar(Guid taxonomiaNormativaId, string usuario)
        {
            TaxonomiaNormativaId = taxonomiaNormativaId;
            MarcarAlterado(usuario);
        }

        public RiscoCorporativo(
            string titulo,
            string descricao,
            string categoria,
            int probabilidade,
            int impacto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RiscoCorporativo>()
                .Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título do risco é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição do risco é obrigatória.")
                .IsTrue(probabilidade >= 1 && probabilidade <= 5, nameof(Probabilidade), "A probabilidade deve estar entre 1 e 5.")
                .IsTrue(impacto >= 1 && impacto <= 5, nameof(Impacto), "O impacto deve estar entre 1 e 5.")
                .IsTrue(categoria == "Operacional" || categoria == "Financeiro" || categoria == "Legal" || categoria == "Seguranca", nameof(Categoria), "A categoria deve ser 'Operacional', 'Financeiro', 'Legal' ou 'Seguranca'.")
            );

            Titulo = titulo;
            Descricao = descricao;
            Categoria = categoria;
            Probabilidade = probabilidade;
            Impacto = impacto;
            NivelRisco = probabilidade * impacto;
            Status = NivelRisco >= 20 ? "Inaceitavel" : "Identificado";
        }

        public void MitigarRisco(string acoesMitigadoras, string usuario)
        {
            if (string.IsNullOrWhiteSpace(acoesMitigadoras))
            {
                AddNotification(nameof(AcoesMitigadoras), "As ações mitigadoras devem ser informadas.");
                return;
            }

            AcoesMitigadoras = acoesMitigadoras;
            Status = "Mitigado";
            MarcarAlterado(usuario);
        }
    }
}
