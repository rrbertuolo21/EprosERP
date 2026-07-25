using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Contrato de servico: formaliza a administracao do imovel para o proprietario
    /// (EF GESTAO_IMOBILIARIA 11.6, tabela imo_contrato_servico, RN-008/RN-024).
    /// </summary>
    public class ContratoServico : EntidadeSaaSBase
    {
        public Guid ProprietarioId { get; private set; }
        public Guid? ImovelId { get; private set; }
        public string? Descricao { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public decimal? Remuneracao { get; private set; }

        protected ContratoServico() { } // EF Core

        public ContratoServico(
            Guid proprietarioId,
            Guid? imovelId,
            string? descricao,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            decimal? remuneracao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProprietarioId = proprietarioId;
            ImovelId = imovelId;
            Descricao = descricao;
            VigenciaInicio = vigenciaInicio?.Date;
            VigenciaFim = vigenciaFim?.Date;
            Remuneracao = remuneracao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContratoServico>()
                .Requires()
                .AreNotEquals(ProprietarioId, Guid.Empty, nameof(ProprietarioId),
                    "O contrato de servico exige proprietario. [Origem: ContratoServico] (RN-008)"));

            if (VigenciaInicio.HasValue && VigenciaFim.HasValue && VigenciaFim < VigenciaInicio)
            {
                AddNotification(nameof(VigenciaFim),
                    "O fim da vigencia deve ser igual ou posterior ao inicio. [Origem: ContratoServico]");
            }
        }
    }
}
