using System;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>MAN-IND — Vinculo patrimonial do equipamento. EF 11.2.</summary>
    public class EquipamentoVinculoPatrimonial : EntidadeSaaSBase
    {
        public Guid EquipamentoId { get; private set; }
        public Guid AtivoPatrimonialId { get; private set; }
        public string? NumeroPatrimonial { get; private set; }
        public string? NomeAtivo { get; private set; }
        public string? DescricaoAtivo { get; private set; }
        public DateTime? DataAquisicao { get; private set; }
        public DateTime? DataAceite { get; private set; }
        public DateTime? DataCadastro { get; private set; }
        public DateTime? DataVistoria { get; private set; }
        public DateTime? DataBaixa { get; private set; }
        public DateTime? VencimentoGarantia { get; private set; }
        public string? NumeroNotaFiscal { get; private set; }
        public string? ChaveNfe { get; private set; }
        public decimal? ValorOriginal { get; private set; }
        public decimal? ValorCompra { get; private set; }
        public decimal? ValorAtualizado { get; private set; }
        public decimal? ValorBaixa { get; private set; }
        public bool? Deprecia { get; private set; }
        public string? MetodoDepreciacao { get; private set; }
        public DateTime? InicioDepreciacao { get; private set; }
        public DateTime? UltimaDepreciacao { get; private set; }
        public string? TipoDepreciacao { get; private set; }
        public decimal? TaxaAnual { get; private set; }
        public decimal? TaxaMensal { get; private set; }
        public decimal? TaxaAcelerada { get; private set; }
        public decimal? TaxaIncentivada { get; private set; }
        public string? Funcao { get; private set; }

        protected EquipamentoVinculoPatrimonial() { }

        public EquipamentoVinculoPatrimonial(Guid equipamentoId, Guid ativoPatrimonialId, string? numeroPatrimonial, string? nomeAtivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EquipamentoId = equipamentoId;
            AtivoPatrimonialId = ativoPatrimonialId;
            NumeroPatrimonial = numeroPatrimonial;
            NomeAtivo = nomeAtivo;

            AddNotifications(new Contract<EquipamentoVinculoPatrimonial>()
                .Requires()
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio.")
                .AreNotEquals(ativoPatrimonialId, Guid.Empty, nameof(AtivoPatrimonialId), "O ativo patrimonial e obrigatorio."));
        }
    }

    /// <summary>MAN-IND — Inducao (aceite e prontidao operacional). EF 11.3.</summary>
    public class EquipamentoInducao : EntidadeSaaSBase
    {
        public Guid EquipamentoId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataAceite { get; private set; }
        public EStatusInducao StatusInducao { get; private set; } = EStatusInducao.Rascunho;
        public Guid? ResponsavelId { get; private set; }
        public string? ChecklistJson { get; private set; }
        public string? Observacao { get; private set; }

        protected EquipamentoInducao() { }

        public EquipamentoInducao(Guid equipamentoId, DateTime? dataInicio, Guid? responsavelId, string? checklistJson, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EquipamentoId = equipamentoId;
            DataInicio = dataInicio ?? DateTime.UtcNow;
            ResponsavelId = responsavelId;
            ChecklistJson = checklistJson;
            Observacao = observacao;
            StatusInducao = EStatusInducao.Rascunho;

            AddNotifications(new Contract<EquipamentoInducao>()
                .Requires()
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio."));
        }

        public void Aprovar(string usuario)
        {
            if (StatusInducao != EStatusInducao.Rascunho && StatusInducao != EStatusInducao.EmAnalise)
            {
                AddNotification(nameof(StatusInducao), "Somente inducoes em rascunho ou analise podem ser aprovadas.");
                return;
            }
            StatusInducao = EStatusInducao.Aprovada;
            DataAceite = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Observacao), "Informe o motivo da rejeicao.");
                return;
            }
            StatusInducao = EStatusInducao.Rejeitada;
            Observacao = motivo;
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (StatusInducao != EStatusInducao.Aprovada)
            {
                AddNotification(nameof(StatusInducao), "Somente inducoes aprovadas podem ser ativadas.");
                return;
            }
            StatusInducao = EStatusInducao.Ativa;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-IND — Servico aplicavel por equipamento/ativo. EF 11.5.</summary>
    public class EquipamentoServico : EntidadeSaaSBase
    {
        public Guid ServicoId { get; private set; }
        public Guid EquipamentoId { get; private set; }
        public Guid? AtivoPatrimonialId { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected EquipamentoServico() { }

        public EquipamentoServico(Guid servicoId, Guid equipamentoId, Guid? ativoPatrimonialId, DateTime? vigenciaInicio, DateTime? vigenciaFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ServicoId = servicoId;
            EquipamentoId = equipamentoId;
            AtivoPatrimonialId = ativoPatrimonialId;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Ativo = true;

            AddNotifications(new Contract<EquipamentoServico>()
                .Requires()
                .AreNotEquals(servicoId, Guid.Empty, nameof(ServicoId), "O servico e obrigatorio.")
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio."));
        }
    }

    /// <summary>MAN-IND — Atributo de extensao (tecnicos adicionais). EF 11.6.</summary>
    public class EquipamentoAtributo : EntidadeSaaSBase
    {
        public Guid EquipamentoId { get; private set; }
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public string? OrigemFuncional { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected EquipamentoAtributo() { }

        public EquipamentoAtributo(Guid equipamentoId, string chave, string? valor, string? origemFuncional, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EquipamentoId = equipamentoId;
            Chave = chave;
            Valor = valor;
            OrigemFuncional = origemFuncional;
            Ativo = true;

            AddNotifications(new Contract<EquipamentoAtributo>()
                .Requires()
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio.")
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do atributo e obrigatoria."));
        }
    }
}
