using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Agregado raiz de Gestao Ambiental e SST (EF GESTAO_AMBIENTAL_EHS 11.4).
    /// Agrega atividades, licencas, residuos, incidentes e acoes corretivas.
    /// </summary>
    public class RegistroEhs : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty; // Licenca, Residuo, Incidente, Risco...
        public Guid ResponsavelId { get; private set; }
        public Guid? LocalId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }

        protected RegistroEhs() { } // EF Core

        public RegistroEhs(
            string codigo,
            string descricao,
            string tipo,
            Guid responsavelId,
            Guid? localId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Tipo = tipo;
            ResponsavelId = responsavelId;
            LocalId = localId;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RegistroEhs>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do registro EHS e obrigatorio. [Origem: RegistroEhs]")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria. [Origem: RegistroEhs]")
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo e obrigatorio. [Origem: RegistroEhs]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: RegistroEhs]"));
        }

        public void Submeter(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas registros em rascunho podem ser submetidos.");
                return;
            }
            Status = EStatusWorkflowEsg.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas registros em analise podem ser aprovados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas registros em analise podem ser rejeitados.");
                return;
            }
            Status = EStatusWorkflowEsg.Rascunho;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas registros ativos podem ser encerrados.");
                return;
            }
            Status = EStatusWorkflowEsg.Encerrado;
            MarcarAlterado(usuario);
        }
    }
}
