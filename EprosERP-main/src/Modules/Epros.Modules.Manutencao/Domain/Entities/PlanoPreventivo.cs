using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>
    /// MAN-PRV — Plano preventivo (agregado raiz). Fiel a EF secao 11.1.
    /// </summary>
    public class PlanoPreventivo : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroManutencao Status { get; private set; } = EStatusRegistroManutencao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public string? AlvoTipo { get; private set; } // Equipamento, Local, Conjunto
        public Guid? AlvoId { get; private set; }
        public string? Observacao { get; private set; }
        public int Versao { get; private set; } = 1;

        public List<PlanoPreventivoPeriodicidade> Periodicidades { get; private set; } = new();
        public List<PlanoPreventivoChecklistItem> ChecklistItens { get; private set; } = new();
        public List<PlanoPreventivoKitPeca> KitPecas { get; private set; } = new();
        public List<PlanoPreventivoExecucao> Execucoes { get; private set; } = new();

        protected PlanoPreventivo() { } // EF Core

        public PlanoPreventivo(
            string codigo,
            string descricao,
            Guid responsavelId,
            string? alvoTipo,
            Guid? alvoId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            AlvoTipo = alvoTipo;
            AlvoId = alvoId;
            Observacao = observacao;
            Status = EStatusRegistroManutencao.Rascunho;
            Versao = 1;
            Validar();
        }

        public void Alterar(string descricao, Guid responsavelId, string? alvoTipo, Guid? alvoId, string? observacao, string usuario)
        {
            Descricao = descricao;
            ResponsavelId = responsavelId;
            AlvoTipo = alvoTipo;
            AlvoId = alvoId;
            Observacao = observacao;
            Versao++;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarPeriodicidade(PlanoPreventivoPeriodicidade periodicidade, string usuario)
        {
            if (!periodicidade.IsValid) { AddNotifications(periodicidade.Notifications); return; }
            Periodicidades.Add(periodicidade);
            MarcarAlterado(usuario);
        }

        public void AdicionarChecklistItem(PlanoPreventivoChecklistItem item, string usuario)
        {
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            ChecklistItens.Add(item);
            MarcarAlterado(usuario);
        }

        public void AdicionarKitPeca(PlanoPreventivoKitPeca item, string usuario)
        {
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            KitPecas.Add(item);
            MarcarAlterado(usuario);
        }

        // RN-PRV-008/009: plano ativo exige periodicidade e alvo validos.
        public void Ativar(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho && Status != EStatusRegistroManutencao.EmAnalise && Status != EStatusRegistroManutencao.Suspenso)
            {
                AddNotification(nameof(Status), "Somente planos em rascunho, analise ou suspensos podem ser ativados.");
                return;
            }
            if (Periodicidades.Count == 0)
            {
                AddNotification(nameof(Periodicidades), "Informe uma periodicidade valida para ativar o plano.");
                return;
            }
            if (AlvoId == null || AlvoId == Guid.Empty)
            {
                AddNotification(nameof(AlvoId), "Informe um alvo operacional valido para ativar o plano.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Suspender(string motivo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Ativo)
            {
                AddNotification(nameof(Status), "Somente planos ativos podem ser suspensos.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(motivo), "Informe o motivo para suspender o plano.");
                return;
            }
            Status = EStatusRegistroManutencao.Suspenso;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(motivo), "Informe o motivo para encerrar o plano.");
                return;
            }
            Status = EStatusRegistroManutencao.Encerrado;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PlanoPreventivo>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do plano preventivo e obrigatorio [Origem: PlanoPreventivo].")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao do plano preventivo e obrigatoria [Origem: PlanoPreventivo].")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: PlanoPreventivo]."));
        }
    }

    /// <summary>MAN-PRV — Periodicidade (calendario/contador/combinado). EF 11.3.</summary>
    public class PlanoPreventivoPeriodicidade : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public ETipoPeriodicidade TipoPeriodicidade { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public int? Intervalo { get; private set; }
        public string? UnidadeIntervalo { get; private set; }
        public string? ContadorTipo { get; private set; }
        public decimal? ContadorBase { get; private set; }
        public decimal? ContadorProximo { get; private set; }
        public string? Tolerancia { get; private set; }
        public DateTime? ProximaExecucao { get; private set; }
        public ESituacaoPeriodicidade Situacao { get; private set; } = ESituacaoPeriodicidade.Ativo;

        protected PlanoPreventivoPeriodicidade() { }

        public PlanoPreventivoPeriodicidade(
            Guid planoId,
            ETipoPeriodicidade tipoPeriodicidade,
            DateTime? dataInicio,
            int? intervalo,
            string? unidadeIntervalo,
            string? contadorTipo,
            decimal? contadorBase,
            decimal? contadorProximo,
            string? tolerancia,
            DateTime? proximaExecucao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoId = planoId;
            TipoPeriodicidade = tipoPeriodicidade;
            DataInicio = dataInicio;
            Intervalo = intervalo;
            UnidadeIntervalo = unidadeIntervalo;
            ContadorTipo = contadorTipo;
            ContadorBase = contadorBase;
            ContadorProximo = contadorProximo;
            Tolerancia = tolerancia;
            ProximaExecucao = proximaExecucao;
            Situacao = ESituacaoPeriodicidade.Ativo;

            AddNotifications(new Contract<PlanoPreventivoPeriodicidade>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio.")
                .IsTrue(
                    tipoPeriodicidade != ETipoPeriodicidade.Calendario || (intervalo.HasValue && intervalo.Value > 0),
                    nameof(Intervalo),
                    "Periodicidade por calendario exige intervalo positivo.")
                .IsTrue(
                    tipoPeriodicidade != ETipoPeriodicidade.Contador || (contadorProximo.HasValue),
                    nameof(ContadorProximo),
                    "Periodicidade por contador exige o proximo marco."));
        }
    }

    /// <summary>MAN-PRV — Checklist item do plano. EF 11.4.</summary>
    public class PlanoPreventivoChecklistItem : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public int Sequencia { get; private set; }
        public string DescricaoTarefa { get; private set; } = string.Empty;
        public bool Obrigatorio { get; private set; }
        public string? TipoResposta { get; private set; }
        public bool ExigeEvidencia { get; private set; }
        public string? CriterioAceite { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected PlanoPreventivoChecklistItem() { }

        public PlanoPreventivoChecklistItem(
            Guid planoId,
            int sequencia,
            string descricaoTarefa,
            bool obrigatorio,
            string? tipoResposta,
            bool exigeEvidencia,
            string? criterioAceite,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoId = planoId;
            Sequencia = sequencia;
            DescricaoTarefa = descricaoTarefa;
            Obrigatorio = obrigatorio;
            TipoResposta = tipoResposta;
            ExigeEvidencia = exigeEvidencia;
            CriterioAceite = criterioAceite;
            Ativo = true;

            AddNotifications(new Contract<PlanoPreventivoChecklistItem>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio.")
                .IsNotNullOrEmpty(descricaoTarefa, nameof(DescricaoTarefa), "A descricao da tarefa e obrigatoria."));
        }
    }

    /// <summary>MAN-PRV — Kit de peca previsto. EF 11.5.</summary>
    public class PlanoPreventivoKitPeca : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public Guid PecaId { get; private set; }
        public decimal Quantidade { get; private set; }
        public string? Unidade { get; private set; }
        public bool Obrigatoria { get; private set; }
        public string? Observacao { get; private set; }

        protected PlanoPreventivoKitPeca() { }

        public PlanoPreventivoKitPeca(
            Guid planoId,
            Guid pecaId,
            decimal quantidade,
            string? unidade,
            bool obrigatoria,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoId = planoId;
            PecaId = pecaId;
            Quantidade = quantidade;
            Unidade = unidade;
            Obrigatoria = obrigatoria;
            Observacao = observacao;

            // RN-PRV-012: quantidade deve ser positiva.
            AddNotifications(new Contract<PlanoPreventivoKitPeca>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio.")
                .AreNotEquals(pecaId, Guid.Empty, nameof(PecaId), "A peca e obrigatoria.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade da peca deve ser maior que zero."));
        }
    }

    /// <summary>MAN-PRV — Execucao programada (ciclo/vencimento). EF 11.6.</summary>
    public class PlanoPreventivoExecucao : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public Guid PeriodicidadeId { get; private set; }
        public DateTime? DataPrevista { get; private set; }
        public decimal? ContadorPrevisto { get; private set; }
        public EStatusExecucaoPreventiva Status { get; private set; } = EStatusExecucaoPreventiva.Prevista;
        public string? Prioridade { get; private set; }
        public DateTime? DataGeracaoOrdem { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public Guid? OrdemTrabalhoId { get; private set; }
        public string? MotivoCancelamento { get; private set; }

        protected PlanoPreventivoExecucao() { }

        public PlanoPreventivoExecucao(
            Guid planoId,
            Guid periodicidadeId,
            DateTime? dataPrevista,
            decimal? contadorPrevisto,
            string? prioridade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoId = planoId;
            PeriodicidadeId = periodicidadeId;
            DataPrevista = dataPrevista;
            ContadorPrevisto = contadorPrevisto;
            Prioridade = prioridade;
            Status = EStatusExecucaoPreventiva.Prevista;

            AddNotifications(new Contract<PlanoPreventivoExecucao>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano e obrigatorio.")
                .AreNotEquals(periodicidadeId, Guid.Empty, nameof(PeriodicidadeId), "A periodicidade e obrigatoria."));
        }

        public void MarcarElegivel(string usuario)
        {
            if (Status != EStatusExecucaoPreventiva.Prevista && Status != EStatusExecucaoPreventiva.Atrasada)
            {
                AddNotification(nameof(Status), "Somente execucoes previstas ou atrasadas podem ficar elegiveis.");
                return;
            }
            Status = EStatusExecucaoPreventiva.Elegivel;
            MarcarAlterado(usuario);
        }

        public void RegistrarOrdemGerada(Guid ordemTrabalhoId, string usuario)
        {
            if (Status != EStatusExecucaoPreventiva.Elegivel)
            {
                AddNotification(nameof(Status), "A ordem so pode ser gerada a partir de execucao elegivel.");
                return;
            }
            OrdemTrabalhoId = ordemTrabalhoId;
            DataGeracaoOrdem = DateTime.UtcNow;
            Status = EStatusExecucaoPreventiva.OrdemGerada;
            MarcarAlterado(usuario);
        }

        public void Concluir(string usuario)
        {
            Status = EStatusExecucaoPreventiva.Concluida;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoCancelamento), "Informe o motivo do cancelamento.");
                return;
            }
            Status = EStatusExecucaoPreventiva.Cancelada;
            MotivoCancelamento = motivo;
            MarcarAlterado(usuario);
        }
    }
}
