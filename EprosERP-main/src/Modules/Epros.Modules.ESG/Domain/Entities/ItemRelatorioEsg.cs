using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Conteudo ordenado do relatorio, respondendo a um requisito (EF RELATORIOS_ESG 11.2).</summary>
    public class ItemRelatorioEsg : EntidadeSaaSBase
    {
        public Guid RelatorioId { get; private set; } // FK ao agregado RelatorioESG
        public int Sequencia { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }
        public Guid? RequisitoId { get; private set; }
        public string TipoConteudo { get; private set; } = string.Empty; // Quantitativo, Narrativo, Misto
        public string StatusPreenchimento { get; private set; } = "NaoIniciado"; // NaoIniciado, Pendente, Completo, NaoAplicavel
        public string? Justificativa { get; private set; }

        protected ItemRelatorioEsg() { } // EF Core

        public ItemRelatorioEsg(
            Guid relatorioId,
            int sequencia,
            decimal? quantidade,
            string? observacao,
            Guid? requisitoId,
            string tipoConteudo,
            string? justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RelatorioId = relatorioId;
            Sequencia = sequencia;
            Quantidade = quantidade;
            Observacao = observacao;
            RequisitoId = requisitoId;
            TipoConteudo = tipoConteudo;
            Justificativa = justificativa;
            StatusPreenchimento = "NaoIniciado";
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ItemRelatorioEsg>()
                .Requires()
                .AreNotEquals(RelatorioId, Guid.Empty, nameof(RelatorioId), "O relatorio e obrigatorio. [Origem: ItemRelatorioEsg]")
                .IsGreaterThan(Sequencia, 0, nameof(Sequencia), "A sequencia deve ser positiva. [Origem: ItemRelatorioEsg]")
                .IsNotNullOrEmpty(TipoConteudo, nameof(TipoConteudo), "O tipo de conteudo e obrigatorio. [Origem: ItemRelatorioEsg]"));
        }
    }
}
