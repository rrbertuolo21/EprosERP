using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Fonte de emissao classificada por escopo e categoria (EF PEGADA_DE_CARBONO 11.2 Fonte).</summary>
    public class FonteEmissaoGee : EntidadeSaaSBase
    {
        public Guid InventarioId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public int Escopo { get; private set; } // 1, 2 ou 3
        public string Categoria { get; private set; } = string.Empty;
        public Guid UnidadeOrganizacionalId { get; private set; }

        protected FonteEmissaoGee() { } // EF Core

        public FonteEmissaoGee(
            Guid inventarioId,
            string codigo,
            string descricao,
            int escopo,
            string categoria,
            Guid unidadeOrganizacionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            InventarioId = inventarioId;
            Codigo = codigo;
            Descricao = descricao;
            Escopo = escopo;
            Categoria = categoria;
            UnidadeOrganizacionalId = unidadeOrganizacionalId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FonteEmissaoGee>()
                .Requires()
                .AreNotEquals(InventarioId, Guid.Empty, nameof(InventarioId), "O inventario e obrigatorio. [Origem: FonteEmissaoGee]")
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da fonte e obrigatorio. [Origem: FonteEmissaoGee]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da fonte e obrigatoria. [Origem: FonteEmissaoGee]")
                .IsTrue(Escopo == 1 || Escopo == 2 || Escopo == 3, nameof(Escopo), "O escopo deve ser 1, 2 ou 3. [Origem: FonteEmissaoGee]")
                .IsNotNullOrEmpty(Categoria, nameof(Categoria), "A categoria e obrigatoria. [Origem: FonteEmissaoGee]"));
        }
    }
}
