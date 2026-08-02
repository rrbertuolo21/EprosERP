using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC — Aresta de rastreabilidade da taxonomia normativa (grc_taxonomia_vinculo). D-TEC-05:
    /// liga dois nós/agregados quaisquer (Origem ↔ Destino) com a Natureza do vínculo (ex.: controle
    /// 'mitiga' risco, política 'deriva_de' obrigação, controle 'atende' obrigação). É por aqui que se
    /// navega Política→Obrigação→Controle→Risco de ponta a ponta (skill compliance/frameworks).
    /// </summary>
    public class TaxonomiaVinculo : EntidadeSaaSBase
    {
        public string OrigemTipo { get; private set; } = string.Empty; // Politica, Obrigacao, Controle, Risco
        public Guid OrigemId { get; private set; }
        public string DestinoTipo { get; private set; } = string.Empty;
        public Guid DestinoId { get; private set; }
        // deriva_de, atende, mitiga, operacionaliza, cobre, origina
        public string Natureza { get; private set; } = string.Empty;

        protected TaxonomiaVinculo() { } // EF Core

        public TaxonomiaVinculo(
            string origemTipo,
            Guid origemId,
            string destinoTipo,
            Guid destinoId,
            string natureza,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TaxonomiaVinculo>()
                .Requires()
                .IsTrue(TaxonomiaNormativa.EhTipoValido(origemTipo), nameof(OrigemTipo),
                    "O tipo de origem deve ser 'Politica', 'Obrigacao', 'Controle' ou 'Risco'.")
                .IsTrue(TaxonomiaNormativa.EhTipoValido(destinoTipo), nameof(DestinoTipo),
                    "O tipo de destino deve ser 'Politica', 'Obrigacao', 'Controle' ou 'Risco'.")
                .IsTrue(origemId != Guid.Empty, nameof(OrigemId), "A origem do vinculo e obrigatoria.")
                .IsTrue(destinoId != Guid.Empty, nameof(DestinoId), "O destino do vinculo e obrigatorio.")
                .IsTrue(!(origemTipo == destinoTipo && origemId == destinoId), nameof(DestinoId),
                    "Um no nao pode se vincular a si mesmo.")
                .IsNotNullOrEmpty(natureza, nameof(Natureza), "A natureza do vinculo e obrigatoria.")
            );

            OrigemTipo = origemTipo;
            OrigemId = origemId;
            DestinoTipo = destinoTipo;
            DestinoId = destinoId;
            Natureza = natureza;
        }
    }
}
