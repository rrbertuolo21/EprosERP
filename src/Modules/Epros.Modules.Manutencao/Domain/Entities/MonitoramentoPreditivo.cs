using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>
    /// MAN-PDT — Monitoramento preditivo (agregado raiz). Fiel a EF secao 11.1.
    /// Ciclo: Rascunho -> EmAnalise -> Ativo -> (Suspenso|Encerrado|Inativo).
    /// </summary>
    public class MonitoramentoPreditivo : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroManutencao Status { get; private set; } = EStatusRegistroManutencao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? EquipamentoId { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public int Versao { get; private set; } = 1;
        public string? Observacao { get; private set; }

        public List<ItemMonitoramentoPreditivo> Itens { get; private set; } = new();
        public List<PontoMedicao> PontosMedicao { get; private set; } = new();
        public List<AlarmePreditivo> Alarmes { get; private set; } = new();
        public List<HistoricoPreditivo> Historicos { get; private set; } = new();
        public List<AnexoPreditivo> Anexos { get; private set; } = new();

        protected MonitoramentoPreditivo() { } // EF Core

        public MonitoramentoPreditivo(
            string codigo,
            string descricao,
            Guid responsavelId,
            Guid? equipamentoId,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            EquipamentoId = equipamentoId;
            Observacao = observacao;
            Status = EStatusRegistroManutencao.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Alterar(string descricao, Guid responsavelId, Guid? equipamentoId, string? observacao, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho && Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente monitoramentos em rascunho ou analise podem ser alterados.");
                return;
            }
            Descricao = descricao;
            ResponsavelId = responsavelId;
            EquipamentoId = equipamentoId;
            Observacao = observacao;
            Versao++;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarItem(ItemMonitoramentoPreditivo item, string usuario)
        {
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            Itens.Add(item);
            MarcarAlterado(usuario);
        }

        // RN: ponto exige equipamento no monitoramento.
        public void AdicionarPontoMedicao(PontoMedicao ponto, string usuario)
        {
            if (!ponto.IsValid) { AddNotifications(ponto.Notifications); return; }
            PontosMedicao.Add(ponto);
            MarcarAlterado(usuario);
        }

        public void RegistrarAlarme(AlarmePreditivo alarme, string usuario)
        {
            if (!alarme.IsValid) { AddNotifications(alarme.Notifications); return; }
            Alarmes.Add(alarme);
            MarcarAlterado(usuario);
        }

        public void RegistrarHistorico(HistoricoPreditivo historico)
        {
            if (historico.IsValid) Historicos.Add(historico);
        }

        public void AdicionarAnexo(AnexoPreditivo anexo, string usuario)
        {
            if (!anexo.IsValid) { AddNotifications(anexo.Notifications); return; }
            Anexos.Add(anexo);
            MarcarAlterado(usuario);
        }

        public void Submeter(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho)
            {
                AddNotification(nameof(Status), "Somente monitoramentos em rascunho podem ser submetidos.");
                return;
            }
            Validar();
            if (!IsValid) return;
            Status = EStatusRegistroManutencao.EmAnalise;
            Versao++;
            MarcarAlterado(usuario);
        }

        // RN: apenas monitoramento ativo recebe leitura e gera alarme.
        public void Aprovar(string usuario)
        {
            if (Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente monitoramentos em analise podem ser aprovados.");
                return;
            }
            if (EquipamentoId == null || EquipamentoId == Guid.Empty)
            {
                AddNotification(nameof(EquipamentoId), "Equipamento e obrigatorio para ativar o monitoramento.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string motivo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente monitoramentos em analise podem ser rejeitados.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "Informe o motivo da rejeicao.");
                return;
            }
            Status = EStatusRegistroManutencao.Rascunho;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Ativo)
            {
                AddNotification(nameof(Status), "Somente monitoramentos ativos podem ser suspensos.");
                return;
            }
            Status = EStatusRegistroManutencao.Suspenso;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Retomar(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Suspenso)
            {
                AddNotification(nameof(Status), "Somente monitoramentos suspensos podem ser retomados.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "Informe o motivo do encerramento.");
                return;
            }
            Status = EStatusRegistroManutencao.Encerrado;
            Versao++;
            MarcarAlterado(usuario);
        }

        public bool PermiteLeituraOuAlarme() => Status == EStatusRegistroManutencao.Ativo;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MonitoramentoPreditivo>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do monitoramento e obrigatorio [Origem: MonitoramentoPreditivo].")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao do monitoramento e obrigatoria [Origem: MonitoramentoPreditivo].")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: MonitoramentoPreditivo]."));
        }
    }

    /// <summary>MAN-PDT — Item de monitoramento. EF 11.2.</summary>
    public class ItemMonitoramentoPreditivo : EntidadeSaaSBase
    {
        public Guid MonitoramentoId { get; private set; }
        public int Sequencia { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }

        protected ItemMonitoramentoPreditivo() { }

        public ItemMonitoramentoPreditivo(
            Guid monitoramentoId,
            int sequencia,
            decimal? quantidade,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            MonitoramentoId = monitoramentoId;
            Sequencia = sequencia;
            Quantidade = quantidade;
            Observacao = observacao;

            AddNotifications(new Contract<ItemMonitoramentoPreditivo>()
                .Requires()
                .AreNotEquals(monitoramentoId, Guid.Empty, nameof(MonitoramentoId), "O monitoramento e obrigatorio."));
        }
    }

    /// <summary>MAN-PDT — Ponto de medicao. EF 11.3.</summary>
    public class PontoMedicao : EntidadeSaaSBase
    {
        public Guid MonitoramentoId { get; private set; }
        public Guid EquipamentoId { get; private set; }
        public string CodigoPonto { get; private set; } = string.Empty;
        public string Variavel { get; private set; } = string.Empty;
        public string Unidade { get; private set; } = string.Empty;
        public string? LocalTecnico { get; private set; }
        public string? Periodicidade { get; private set; }
        public ESituacaoPontoMedicao Situacao { get; private set; } = ESituacaoPontoMedicao.Ativo;

        public List<LeituraCondicao> Leituras { get; private set; } = new();
        public List<RegraMonitoramento> Regras { get; private set; } = new();

        protected PontoMedicao() { }

        public PontoMedicao(
            Guid monitoramentoId,
            Guid equipamentoId,
            string codigoPonto,
            string variavel,
            string unidade,
            string? localTecnico,
            string? periodicidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            MonitoramentoId = monitoramentoId;
            EquipamentoId = equipamentoId;
            CodigoPonto = codigoPonto;
            Variavel = variavel;
            Unidade = unidade;
            LocalTecnico = localTecnico;
            Periodicidade = periodicidade;
            Situacao = ESituacaoPontoMedicao.Ativo;

            AddNotifications(new Contract<PontoMedicao>()
                .Requires()
                .AreNotEquals(monitoramentoId, Guid.Empty, nameof(MonitoramentoId), "O monitoramento e obrigatorio.")
                .AreNotEquals(equipamentoId, Guid.Empty, nameof(EquipamentoId), "O equipamento e obrigatorio.")
                .IsNotNullOrEmpty(codigoPonto, nameof(CodigoPonto), "O codigo do ponto e obrigatorio.")
                .IsNotNullOrEmpty(variavel, nameof(Variavel), "A variavel monitorada e obrigatoria.")
                .IsNotNullOrEmpty(unidade, nameof(Unidade), "A unidade e obrigatoria."));
        }

        // RN: apenas ponto ativo recebe leitura.
        public void RegistrarLeitura(LeituraCondicao leitura, string usuario)
        {
            if (Situacao != ESituacaoPontoMedicao.Ativo)
            {
                AddNotification(nameof(Situacao), "Somente pontos ativos podem receber leitura.");
                return;
            }
            if (!leitura.IsValid) { AddNotifications(leitura.Notifications); return; }
            Leituras.Add(leitura);
            MarcarAlterado(usuario);
        }

        public void AdicionarRegra(RegraMonitoramento regra, string usuario)
        {
            if (!regra.IsValid) { AddNotifications(regra.Notifications); return; }
            Regras.Add(regra);
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            Situacao = ESituacaoPontoMedicao.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Situacao = ESituacaoPontoMedicao.Inativo;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-PDT — Leitura de condicao. EF 11.4.</summary>
    public class LeituraCondicao : EntidadeSaaSBase
    {
        public Guid PontoMedicaoId { get; private set; }
        public DateTime DataHoraMedicao { get; private set; }
        public decimal Valor { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public decimal? QualidadeDado { get; private set; }
        public string Origem { get; private set; } = string.Empty;
        public string? PayloadJson { get; private set; }

        protected LeituraCondicao() { }

        public LeituraCondicao(
            Guid pontoMedicaoId,
            DateTime dataHoraMedicao,
            decimal valor,
            string unidade,
            decimal? qualidadeDado,
            string origem,
            string? payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PontoMedicaoId = pontoMedicaoId;
            DataHoraMedicao = dataHoraMedicao;
            Valor = valor;
            Unidade = unidade;
            QualidadeDado = qualidadeDado;
            Origem = origem;
            PayloadJson = payloadJson;

            AddNotifications(new Contract<LeituraCondicao>()
                .Requires()
                .AreNotEquals(pontoMedicaoId, Guid.Empty, nameof(PontoMedicaoId), "O ponto de medicao e obrigatorio.")
                .IsNotNullOrEmpty(unidade, nameof(Unidade), "A unidade da leitura e obrigatoria.")
                .IsNotNullOrEmpty(origem, nameof(Origem), "A origem da leitura e obrigatoria."));
        }
    }

    /// <summary>MAN-PDT — Regra de monitoramento. EF 11.5.</summary>
    public class RegraMonitoramento : EntidadeSaaSBase
    {
        public Guid PontoMedicaoId { get; private set; }
        public ETipoRegraMonitoramento TipoRegra { get; private set; }
        public string? Operador { get; private set; }
        public decimal? LimiteMinimo { get; private set; }
        public decimal? LimiteMaximo { get; private set; }
        public string? JanelaAvaliacao { get; private set; }
        public string Severidade { get; private set; } = string.Empty;
        public string AcaoEsperada { get; private set; } = string.Empty;
        public ESituacaoRegraMonitoramento Situacao { get; private set; } = ESituacaoRegraMonitoramento.Rascunho;
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }

        protected RegraMonitoramento() { }

        public RegraMonitoramento(
            Guid pontoMedicaoId,
            ETipoRegraMonitoramento tipoRegra,
            string? operador,
            decimal? limiteMinimo,
            decimal? limiteMaximo,
            string? janelaAvaliacao,
            string severidade,
            string acaoEsperada,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PontoMedicaoId = pontoMedicaoId;
            TipoRegra = tipoRegra;
            Operador = operador;
            LimiteMinimo = limiteMinimo;
            LimiteMaximo = limiteMaximo;
            JanelaAvaliacao = janelaAvaliacao;
            Severidade = severidade;
            AcaoEsperada = acaoEsperada;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Situacao = ESituacaoRegraMonitoramento.Rascunho;

            AddNotifications(new Contract<RegraMonitoramento>()
                .Requires()
                .AreNotEquals(pontoMedicaoId, Guid.Empty, nameof(PontoMedicaoId), "O ponto de medicao e obrigatorio.")
                .IsNotNullOrEmpty(severidade, nameof(Severidade), "A severidade da regra e obrigatoria.")
                .IsNotNullOrEmpty(acaoEsperada, nameof(AcaoEsperada), "A acao esperada e obrigatoria."));
        }

        // RN: apenas regra ativa gera alarme.
        public void Ativar(string usuario)
        {
            Situacao = ESituacaoRegraMonitoramento.Ativo;
            MarcarAlterado(usuario);
        }

        public void Suspender(string usuario)
        {
            Situacao = ESituacaoRegraMonitoramento.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Situacao = ESituacaoRegraMonitoramento.Inativo;
            MarcarAlterado(usuario);
        }

        public bool GeraAlarme() => Situacao == ESituacaoRegraMonitoramento.Ativo;
    }

    /// <summary>MAN-PDT — Alarme preditivo. EF 11.6.</summary>
    public class AlarmePreditivo : EntidadeSaaSBase
    {
        public Guid MonitoramentoId { get; private set; }
        public Guid PontoMedicaoId { get; private set; }
        public Guid RegraId { get; private set; }
        public Guid? LeituraId { get; private set; }
        public DateTime DataHoraDisparo { get; private set; }
        public string Severidade { get; private set; } = string.Empty;
        public EStatusAlarmePreditivo Status { get; private set; } = EStatusAlarmePreditivo.Aberto;
        public string Descricao { get; private set; } = string.Empty;
        public string? MotivoEncerramento { get; private set; }

        public List<VinculoOrdemTrabalhoPreditivo> Vinculos { get; private set; } = new();

        protected AlarmePreditivo() { }

        public AlarmePreditivo(
            Guid monitoramentoId,
            Guid pontoMedicaoId,
            Guid regraId,
            Guid? leituraId,
            string severidade,
            string descricao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            MonitoramentoId = monitoramentoId;
            PontoMedicaoId = pontoMedicaoId;
            RegraId = regraId;
            LeituraId = leituraId;
            DataHoraDisparo = DateTime.UtcNow;
            Severidade = severidade;
            Descricao = descricao;
            Status = EStatusAlarmePreditivo.Aberto;

            AddNotifications(new Contract<AlarmePreditivo>()
                .Requires()
                .AreNotEquals(monitoramentoId, Guid.Empty, nameof(MonitoramentoId), "O monitoramento e obrigatorio.")
                .AreNotEquals(pontoMedicaoId, Guid.Empty, nameof(PontoMedicaoId), "O ponto de medicao e obrigatorio.")
                .AreNotEquals(regraId, Guid.Empty, nameof(RegraId), "A regra e obrigatoria.")
                .IsNotNullOrEmpty(severidade, nameof(Severidade), "A severidade e obrigatoria.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do alarme e obrigatoria."));
        }

        public void ColocarEmAnalise(string usuario)
        {
            if (Status != EStatusAlarmePreditivo.Aberto)
            {
                AddNotification(nameof(Status), "Somente alarmes abertos podem entrar em analise.");
                return;
            }
            Status = EStatusAlarmePreditivo.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void ConverterEmOrdem(VinculoOrdemTrabalhoPreditivo vinculo, string usuario)
        {
            if (Status != EStatusAlarmePreditivo.Aberto && Status != EStatusAlarmePreditivo.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente alarmes abertos ou em analise podem virar ordem.");
                return;
            }
            if (!vinculo.IsValid) { AddNotifications(vinculo.Notifications); return; }
            Vinculos.Add(vinculo);
            Status = EStatusAlarmePreditivo.ConvertidoEmOrdem;
            MarcarAlterado(usuario);
        }

        // RN: descarte exige motivo.
        public void Descartar(string motivo, string usuario)
        {
            if (Status == EStatusAlarmePreditivo.Encerrado)
            {
                AddNotification(nameof(Status), "Alarme encerrado nao pode ser descartado.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoEncerramento), "Informe o motivo do descarte.");
                return;
            }
            Status = EStatusAlarmePreditivo.Descartado;
            MotivoEncerramento = motivo;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoEncerramento), "Informe o motivo do encerramento.");
                return;
            }
            Status = EStatusAlarmePreditivo.Encerrado;
            MotivoEncerramento = motivo;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-PDT — Vinculo de ordem de trabalho. EF 11.7.</summary>
    public class VinculoOrdemTrabalhoPreditivo : EntidadeSaaSBase
    {
        public Guid AlarmeId { get; private set; }
        public Guid OrdemTrabalhoId { get; private set; }
        public DateTime DataHoraSolicitacao { get; private set; }
        public string? StatusRetorno { get; private set; }
        public string? PayloadRetorno { get; private set; }

        protected VinculoOrdemTrabalhoPreditivo() { }

        public VinculoOrdemTrabalhoPreditivo(
            Guid alarmeId,
            Guid ordemTrabalhoId,
            string? statusRetorno,
            string? payloadRetorno,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AlarmeId = alarmeId;
            OrdemTrabalhoId = ordemTrabalhoId;
            DataHoraSolicitacao = DateTime.UtcNow;
            StatusRetorno = statusRetorno;
            PayloadRetorno = payloadRetorno;

            AddNotifications(new Contract<VinculoOrdemTrabalhoPreditivo>()
                .Requires()
                .AreNotEquals(alarmeId, Guid.Empty, nameof(AlarmeId), "O alarme e obrigatorio.")
                .AreNotEquals(ordemTrabalhoId, Guid.Empty, nameof(OrdemTrabalhoId), "A ordem de trabalho e obrigatoria."));
        }
    }

    /// <summary>MAN-PDT — Historico/auditoria. EF 11.8.</summary>
    public class HistoricoPreditivo : EntidadeSaaSBase
    {
        public Guid? MonitoramentoId { get; private set; }
        public Guid? AlarmeId { get; private set; }
        public EAcaoHistoricoPreditivo Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string? Ip { get; private set; }
        public string PayloadJson { get; private set; } = "{}";

        protected HistoricoPreditivo() { }

        public HistoricoPreditivo(
            Guid? monitoramentoId,
            Guid? alarmeId,
            EAcaoHistoricoPreditivo acao,
            Guid usuarioId,
            string? ip,
            string payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            MonitoramentoId = monitoramentoId;
            AlarmeId = alarmeId;
            Acao = acao;
            UsuarioId = usuarioId;
            DataHora = DateTime.UtcNow;
            Ip = ip;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;

            AddNotifications(new Contract<HistoricoPreditivo>()
                .Requires()
                .IsTrue((monitoramentoId.HasValue && monitoramentoId.Value != Guid.Empty) || (alarmeId.HasValue && alarmeId.Value != Guid.Empty),
                    nameof(MonitoramentoId), "Historico deve referenciar monitoramento ou alarme."));
        }
    }

    /// <summary>MAN-PDT — Anexo. EF 11.9.</summary>
    public class AnexoPreditivo : EntidadeSaaSBase
    {
        public Guid MonitoramentoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoDocumento { get; private set; }
        public DateTime DataHoraVinculo { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected AnexoPreditivo() { }

        public AnexoPreditivo(
            Guid monitoramentoId,
            Guid arquivoId,
            string? tipoDocumento,
            Guid usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            MonitoramentoId = monitoramentoId;
            ArquivoId = arquivoId;
            TipoDocumento = tipoDocumento;
            DataHoraVinculo = DateTime.UtcNow;
            UsuarioId = usuarioId;

            AddNotifications(new Contract<AnexoPreditivo>()
                .Requires()
                .AreNotEquals(monitoramentoId, Guid.Empty, nameof(MonitoramentoId), "O monitoramento e obrigatorio.")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio."));
        }
    }

    /// <summary>MAN-PDT — Parametro por tenant. EF 11.10.</summary>
    public class ParametroPreditivo : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public ESituacaoParametroManutencao Situacao { get; private set; } = ESituacaoParametroManutencao.Ativo;
        public DateTime DataHoraAlteracao { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected ParametroPreditivo() { }

        public ParametroPreditivo(
            string chave,
            string valorJson,
            Guid usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            ValorJson = string.IsNullOrWhiteSpace(valorJson) ? "{}" : valorJson;
            Situacao = ESituacaoParametroManutencao.Ativo;
            DataHoraAlteracao = DateTime.UtcNow;
            UsuarioId = usuarioId;

            AddNotifications(new Contract<ParametroPreditivo>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria."));
        }

        public void Inativar(string usuario)
        {
            Situacao = ESituacaoParametroManutencao.Inativo;
            DataHoraAlteracao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-PDT — Evento de integracao (publicado/recebido). EF 11.11.</summary>
    public class EventoIntegracaoPreditivo : EntidadeSaaSBase
    {
        public string EntidadeOrigem { get; private set; } = string.Empty;
        public Guid EntidadeOrigemId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public EDirecaoEventoPreditivo Direcao { get; private set; }
        public EStatusEventoPreditivo Status { get; private set; } = EStatusEventoPreditivo.Pendente;
        public string PayloadJson { get; private set; } = "{}";
        public DateTime DataHora { get; private set; }
        public string? MensagemErro { get; private set; }

        protected EventoIntegracaoPreditivo() { }

        public EventoIntegracaoPreditivo(
            string entidadeOrigem,
            Guid entidadeOrigemId,
            string tipoEvento,
            EDirecaoEventoPreditivo direcao,
            string payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntidadeOrigem = entidadeOrigem;
            EntidadeOrigemId = entidadeOrigemId;
            TipoEvento = tipoEvento;
            Direcao = direcao;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            Status = EStatusEventoPreditivo.Pendente;
            DataHora = DateTime.UtcNow;

            AddNotifications(new Contract<EventoIntegracaoPreditivo>()
                .Requires()
                .IsNotNullOrEmpty(entidadeOrigem, nameof(EntidadeOrigem), "A entidade de origem e obrigatoria.")
                .IsNotNullOrEmpty(tipoEvento, nameof(TipoEvento), "O tipo do evento e obrigatorio."));
        }

        public void MarcarProcessado(string usuario)
        {
            Status = EStatusEventoPreditivo.Processado;
            MarcarAlterado(usuario);
        }

        public void MarcarErro(string erro, string usuario)
        {
            Status = EStatusEventoPreditivo.Erro;
            MensagemErro = erro;
            MarcarAlterado(usuario);
        }
    }
}
