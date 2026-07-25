using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Fluxo circular agregando origem, objetivo, responsavel e estado (EF ECONOMIA_CIRCULAR 11.3).</summary>
    public class FluxoCircular : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty;
        public Guid? DevolucaoId { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }
        public int Versao { get; private set; }

        protected FluxoCircular() { } // EF Core

        public FluxoCircular(
            string codigo,
            string descricao,
            string tipo,
            Guid? devolucaoId,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Tipo = tipo;
            DevolucaoId = devolucaoId;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowEsg.Rascunho;
            Versao = 1;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FluxoCircular>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do fluxo e obrigatorio. [Origem: FluxoCircular]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: FluxoCircular]")
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo do fluxo e obrigatorio. [Origem: FluxoCircular]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: FluxoCircular]"));
        }

        public void Submeter(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas fluxos em rascunho podem ser submetidos.");
                return;
            }
            Status = EStatusWorkflowEsg.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas fluxos em analise podem ser aprovados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            // RN-ECO: encerrar exige reconciliacao (validada no handler antes de chamar).
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas fluxos ativos podem ser encerrados.");
                return;
            }
            Status = EStatusWorkflowEsg.Encerrado;
            MarcarAlterado(usuario);
        }
    }
}
