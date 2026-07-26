using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Cabecalho do inventario de GEE por periodo e fronteira (EF PEGADA_DE_CARBONO 11.1).
    /// Agregado raiz das fontes, dados de atividade, calculos e consolidacoes.
    /// </summary>
    public class InventarioGee : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public int Versao { get; private set; }
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public string CriterioFronteira { get; private set; } = string.Empty;
        public string Metodologia { get; private set; } = string.Empty;
        public EStatusWorkflowEsg Status { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public string? Observacao { get; private set; }

        protected InventarioGee() { } // EF Core

        public InventarioGee(
            string codigo,
            int versao,
            DateTime periodoInicio,
            DateTime periodoFim,
            string criterioFronteira,
            string metodologia,
            Guid responsavelId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Versao = versao <= 0 ? 1 : versao;
            PeriodoInicio = periodoInicio.Date;
            PeriodoFim = periodoFim.Date;
            CriterioFronteira = criterioFronteira;
            Metodologia = metodologia;
            ResponsavelId = responsavelId;
            Observacao = observacao;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<InventarioGee>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do inventario e obrigatorio. [Origem: InventarioGee]")
                .IsNotNullOrEmpty(CriterioFronteira, nameof(CriterioFronteira), "O criterio de fronteira e obrigatorio. [Origem: InventarioGee]")
                .IsNotNullOrEmpty(Metodologia, nameof(Metodologia), "A metodologia e obrigatoria. [Origem: InventarioGee]")
                .IsGreaterThan(Versao, 0, nameof(Versao), "A versao deve ser positiva. [Origem: InventarioGee]")
                .IsFalse(PeriodoFim < PeriodoInicio, nameof(PeriodoFim), "O fim do periodo deve ser igual ou posterior ao inicio. [Origem: InventarioGee]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: InventarioGee]"));
        }

        // RN-GHG: transicoes de estado (secao 12)
        public void Submeter(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas inventarios em rascunho podem ser submetidos.");
                return;
            }
            Status = EStatusWorkflowEsg.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas inventarios em analise podem ser aprovados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas inventarios em analise podem ser rejeitados.");
                return;
            }
            Status = EStatusWorkflowEsg.Rascunho;
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas inventarios ativos podem ser suspensos.");
                return;
            }
            Status = EStatusWorkflowEsg.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Reativar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Suspenso && Status != EStatusWorkflowEsg.Inativo)
            {
                AddNotification(nameof(Status), "Apenas inventarios suspensos ou inativos podem ser reativados.");
                return;
            }
            Status = EStatusWorkflowEsg.Ativo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusWorkflowEsg.Ativo)
            {
                AddNotification(nameof(Status), "Apenas inventarios ativos podem ser encerrados.");
                return;
            }
            Status = EStatusWorkflowEsg.Encerrado;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            if (Status == EStatusWorkflowEsg.Encerrado)
            {
                AddNotification(nameof(Status), "Inventarios encerrados nao podem ser inativados.");
                return;
            }
            Status = EStatusWorkflowEsg.Inativo;
            MarcarAlterado(usuario);
        }
    }
}
