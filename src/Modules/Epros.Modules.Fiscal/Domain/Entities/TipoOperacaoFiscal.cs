using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class TipoOperacaoFiscal : EntidadeSaaSBase
    {
        public Guid TributarioGrupoId { get; private set; }
        public Guid? CfopNfeId { get; private set; }
        public Guid? CfopNfceId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public bool SobescreveTributacaoNcm { get; private set; }
        public EFinalidadeEmissao Finalidade { get; private set; }
        public ETipoAtendimento Atendimento { get; private set; }
        public EModalidadeFrete TipoFrete { get; private set; }
        public ETipoMovimento TipoMovimento { get; private set; }

        // EF Navigation Properties
        public Cfop? CfopNfe { get; private set; }
        public Cfop? CfopNfce { get; private set; }

        protected TipoOperacaoFiscal() { } // EF Core

        public TipoOperacaoFiscal(
            Guid tributarioGrupoId,
            Guid? cfopNfeId,
            Guid? cfopNfceId,
            string descricao,
            bool sobescreveTributacaoNcm,
            EFinalidadeEmissao finalidade,
            ETipoAtendimento atendimento,
            EModalidadeFrete tipoFrete,
            ETipoMovimento tipoMovimento,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            TributarioGrupoId = tributarioGrupoId;
            CfopNfeId = cfopNfeId;
            CfopNfceId = cfopNfceId;
            Descricao = descricao;
            SobescreveTributacaoNcm = sobescreveTributacaoNcm;
            Finalidade = finalidade;
            Atendimento = atendimento;
            TipoFrete = tipoFrete;
            TipoMovimento = tipoMovimento;
            Validar();
        }

        public void Alterar(
            Guid tributarioGrupoId,
            Guid? cfopNfeId,
            Guid? cfopNfceId,
            string descricao,
            bool sobescreveTributacaoNcm,
            EFinalidadeEmissao finalidade,
            ETipoAtendimento atendimento,
            EModalidadeFrete tipoFrete,
            ETipoMovimento tipoMovimento,
            string alteradoPor)
        {
            TributarioGrupoId = tributarioGrupoId;
            CfopNfeId = cfopNfeId;
            CfopNfceId = cfopNfceId;
            Descricao = descricao;
            SobescreveTributacaoNcm = sobescreveTributacaoNcm;
            Finalidade = finalidade;
            Atendimento = atendimento;
            TipoFrete = tipoFrete;
            TipoMovimento = tipoMovimento;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();

            AddNotifications(new Contract<TipoOperacaoFiscal>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(Descricao ?? string.Empty, 150, nameof(Descricao), "A descrição deve ter no máximo 150 caracteres.")
                .IsTrue(Enum.IsDefined(typeof(EFinalidadeEmissao), Finalidade), nameof(Finalidade), "Finalidade de emissão inválida.")
                .IsTrue(Enum.IsDefined(typeof(ETipoAtendimento), Atendimento), nameof(Atendimento), "Tipo de atendimento inválido.")
                .IsTrue(Enum.IsDefined(typeof(EModalidadeFrete), TipoFrete), nameof(TipoFrete), "Modalidade de frete inválida.")
                .IsTrue(Enum.IsDefined(typeof(ETipoMovimento), TipoMovimento), nameof(TipoMovimento), "Tipo de movimento inválido.")
            );
        }
    }
}
