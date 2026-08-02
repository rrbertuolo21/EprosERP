using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC — Taxonomia normativa ÚNICA (grc_taxonomia_normativa). D-TEC-05 (Rafael): Política,
    /// Obrigação, Controle e Risco são catálogos COMPARTILHADOS ligados por referência, não cópias
    /// por submódulo. Cada nó tem um Tipo e pode formar árvore por CatalogoPaiId. Os agregados dos
    /// 4 submódulos (POL/REG/CIA/RIS) referenciam este catálogo por FK opcional — nunca se duplica
    /// o catálogo dentro do submódulo (skill compliance/frameworks).
    /// </summary>
    public class TaxonomiaNormativa : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        // Politica, Obrigacao, Controle, Risco
        public string Tipo { get; private set; } = "Obrigacao";
        public string Nome { get; private set; } = string.Empty;
        public Guid? CatalogoPaiId { get; private set; }
        public string Status { get; private set; } = "Ativo"; // Ativo, Inativo

        protected TaxonomiaNormativa() { } // EF Core

        public TaxonomiaNormativa(
            string codigo,
            string tipo,
            string nome,
            Guid? catalogoPaiId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TaxonomiaNormativa>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do no da taxonomia e obrigatorio.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do no da taxonomia e obrigatorio.")
                .IsTrue(EhTipoValido(tipo), nameof(Tipo),
                    "O tipo deve ser 'Politica', 'Obrigacao', 'Controle' ou 'Risco'.")
            );

            Codigo = codigo;
            Tipo = tipo;
            Nome = nome;
            CatalogoPaiId = catalogoPaiId;
            Status = "Ativo";
        }

        public static bool EhTipoValido(string tipo) =>
            tipo == "Politica" || tipo == "Obrigacao" || tipo == "Controle" || tipo == "Risco";

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
