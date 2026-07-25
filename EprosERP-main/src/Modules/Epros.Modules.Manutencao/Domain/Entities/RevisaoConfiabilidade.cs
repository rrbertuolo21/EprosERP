using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>
    /// MAN-CRV — Revisao de confiabilidade (agregado raiz). Fiel a EF secao 11.1.
    /// Ciclo: Rascunho -> EmAnalise -> Ativo -> (Suspenso|Encerrado|Inativo).
    /// </summary>
    public class RevisaoConfiabilidade : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroManutencao Status { get; private set; } = EStatusRegistroManutencao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? AtivoId { get; private set; }
        public string? FuncaoOperacional { get; private set; }
        public string? EstadoConservacao { get; private set; }
        public string? CriticidadeOperacional { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataSubmissao { get; private set; }
        public DateTime? DataAprovacao { get; private set; }
        public Guid? AprovadorId { get; private set; }
        public string? MotivoRejeicao { get; private set; }
        public string? MotivoSuspensao { get; private set; }
        public string? MotivoEncerramento { get; private set; }
        public int Versao { get; private set; } = 1;

        public List<ModoFalhaConfiabilidade> ModosFalha { get; private set; } = new();
        public List<IndicadorConfiabilidade> Indicadores { get; private set; } = new();
        public List<RecomendacaoEstrategia> Recomendacoes { get; private set; } = new();
        public List<HistoricoConfiabilidade> Historicos { get; private set; } = new();
        public List<AnexoConfiabilidade> Anexos { get; private set; } = new();

        protected RevisaoConfiabilidade() { } // EF Core

        public RevisaoConfiabilidade(
            string codigo,
            string descricao,
            Guid responsavelId,
            Guid? ativoId,
            string? funcaoOperacional,
            string? estadoConservacao,
            string? criticidadeOperacional,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            AtivoId = ativoId;
            FuncaoOperacional = funcaoOperacional;
            EstadoConservacao = estadoConservacao;
            CriticidadeOperacional = criticidadeOperacional;
            Status = EStatusRegistroManutencao.Rascunho;
            DataCriacao = DateTime.UtcNow;
            Versao = 1;
            Validar();
        }

        public void Alterar(string descricao, Guid responsavelId, Guid? ativoId, string? funcaoOperacional, string? estadoConservacao, string? criticidadeOperacional, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho && Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente revisoes em rascunho ou analise podem ser alteradas.");
                return;
            }
            Descricao = descricao;
            ResponsavelId = responsavelId;
            AtivoId = ativoId;
            FuncaoOperacional = funcaoOperacional;
            EstadoConservacao = estadoConservacao;
            CriticidadeOperacional = criticidadeOperacional;
            Versao++;
            MarcarAlterado(usuario);
            Validar();
        }

        public void AdicionarModoFalha(ModoFalhaConfiabilidade modo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho && Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Modos de falha so podem ser adicionados em revisoes editaveis.");
                return;
            }
            if (!modo.IsValid) { AddNotifications(modo.Notifications); return; }
            ModosFalha.Add(modo);
            MarcarAlterado(usuario);
        }

        public void AdicionarIndicador(IndicadorConfiabilidade indicador, string usuario)
        {
            if (!indicador.IsValid) { AddNotifications(indicador.Notifications); return; }
            Indicadores.Add(indicador);
            MarcarAlterado(usuario);
        }

        public void AdicionarRecomendacao(RecomendacaoEstrategia recomendacao, string usuario)
        {
            if (!recomendacao.IsValid) { AddNotifications(recomendacao.Notifications); return; }
            Recomendacoes.Add(recomendacao);
            MarcarAlterado(usuario);
        }

        public void RegistrarHistorico(HistoricoConfiabilidade historico)
        {
            if (historico.IsValid) Historicos.Add(historico);
        }

        public void AdicionarAnexo(AnexoConfiabilidade anexo, string usuario)
        {
            if (!anexo.IsValid) { AddNotifications(anexo.Notifications); return; }
            Anexos.Add(anexo);
            MarcarAlterado(usuario);
        }

        // RN-CRV-006: rascunho pode ser submetido quando validacoes minimas atendidas.
        public void Submeter(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho)
            {
                AddNotification(nameof(Status), "Somente revisoes em rascunho podem ser submetidas.");
                return;
            }
            Validar();
            if (!IsValid) return;
            Status = EStatusRegistroManutencao.EmAnalise;
            DataSubmissao = DateTime.UtcNow;
            Versao++;
            MarcarAlterado(usuario);
        }

        // RN-CRV-008: aprovacao move para Ativo.
        public void Aprovar(Guid aprovadorId, string usuario)
        {
            if (Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente revisoes em analise podem ser aprovadas.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            AprovadorId = aprovadorId;
            DataAprovacao = DateTime.UtcNow;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string motivo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.EmAnalise)
            {
                AddNotification(nameof(Status), "Somente revisoes em analise podem ser rejeitadas.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "Informe o motivo da rejeicao.");
                return;
            }
            Status = EStatusRegistroManutencao.Rascunho;
            MotivoRejeicao = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        // RN-CRV-009: revisao ativa pode ser suspensa.
        public void Suspender(string motivo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Ativo)
            {
                AddNotification(nameof(Status), "Somente revisoes ativas podem ser suspensas.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoSuspensao), "Informe o motivo da suspensao.");
                return;
            }
            Status = EStatusRegistroManutencao.Suspenso;
            MotivoSuspensao = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        // RN-CRV-010: revisao suspensa retorna a ativo.
        public void Retomar(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Suspenso)
            {
                AddNotification(nameof(Status), "Somente revisoes suspensas podem ser retomadas.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string motivo, string usuario)
        {
            if (Status != EStatusRegistroManutencao.Ativo && Status != EStatusRegistroManutencao.Suspenso)
            {
                AddNotification(nameof(Status), "Somente revisoes ativas ou suspensas podem ser encerradas.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoEncerramento), "Informe o motivo do encerramento.");
                return;
            }
            Status = EStatusRegistroManutencao.Encerrado;
            MotivoEncerramento = motivo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Inativar(string usuario)
        {
            Status = EStatusRegistroManutencao.Inativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RevisaoConfiabilidade>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo da revisao e obrigatorio [Origem: RevisaoConfiabilidade].")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da revisao e obrigatoria [Origem: RevisaoConfiabilidade].")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: RevisaoConfiabilidade]."));
        }
    }

    /// <summary>MAN-CRV — Modo de falha / FMEA. EF 11.2. RPN = S x O x D.</summary>
    public class ModoFalhaConfiabilidade : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public int Sequencia { get; private set; }
        public string? Componente { get; private set; }
        public string ModoFalha { get; private set; } = string.Empty;
        public string? EfeitoFalha { get; private set; }
        public string? CausaFalha { get; private set; }
        public string? ControleAtual { get; private set; }
        public int? Severidade { get; private set; }
        public int? Ocorrencia { get; private set; }
        public int? Deteccao { get; private set; }
        public int? Rpn { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected ModoFalhaConfiabilidade() { }

        public ModoFalhaConfiabilidade(
            Guid revisaoId,
            int sequencia,
            string? componente,
            string modoFalha,
            string? efeitoFalha,
            string? causaFalha,
            string? controleAtual,
            int? severidade,
            int? ocorrencia,
            int? deteccao,
            decimal? quantidade,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            Sequencia = sequencia;
            Componente = componente;
            ModoFalha = modoFalha;
            EfeitoFalha = efeitoFalha;
            CausaFalha = causaFalha;
            ControleAtual = controleAtual;
            Severidade = severidade;
            Ocorrencia = ocorrencia;
            Deteccao = deteccao;
            Quantidade = quantidade;
            Observacao = observacao;
            Ativo = true;
            CalcularRpn();

            AddNotifications(new Contract<ModoFalhaConfiabilidade>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria.")
                .IsNotNullOrEmpty(modoFalha, nameof(ModoFalha), "O modo de falha e obrigatorio.")
                .IsTrue(!severidade.HasValue || (severidade.Value >= 1 && severidade.Value <= 10), nameof(Severidade), "Severidade deve estar entre 1 e 10.")
                .IsTrue(!ocorrencia.HasValue || (ocorrencia.Value >= 1 && ocorrencia.Value <= 10), nameof(Ocorrencia), "Ocorrencia deve estar entre 1 e 10.")
                .IsTrue(!deteccao.HasValue || (deteccao.Value >= 1 && deteccao.Value <= 10), nameof(Deteccao), "Deteccao deve estar entre 1 e 10."));
        }

        // RN: RPN = S x O x D quando todos informados.
        private void CalcularRpn()
        {
            if (Severidade.HasValue && Ocorrencia.HasValue && Deteccao.HasValue)
                Rpn = Severidade.Value * Ocorrencia.Value * Deteccao.Value;
            else
                Rpn = null;
        }

        public void Desativar(string usuario)
        {
            Ativo = false;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-CRV — Indicador de confiabilidade (MTTR/MTBF/Disponibilidade/RPN). EF 11.3.</summary>
    public class IndicadorConfiabilidade : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public ETipoIndicadorConfiabilidade TipoIndicador { get; private set; }
        public DateTime? PeriodoInicio { get; private set; }
        public DateTime? PeriodoFim { get; private set; }
        public decimal Valor { get; private set; }
        public string? Unidade { get; private set; }
        public string FormulaAplicada { get; private set; } = string.Empty;
        public string? OrigemDados { get; private set; }
        public DateTime DataCalculo { get; private set; }
        public ECalculadoPorConfiabilidade CalculadoPor { get; private set; }

        protected IndicadorConfiabilidade() { }

        public IndicadorConfiabilidade(
            Guid revisaoId,
            ETipoIndicadorConfiabilidade tipoIndicador,
            DateTime? periodoInicio,
            DateTime? periodoFim,
            decimal valor,
            string? unidade,
            string formulaAplicada,
            string? origemDados,
            ECalculadoPorConfiabilidade calculadoPor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            TipoIndicador = tipoIndicador;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            Valor = valor;
            Unidade = unidade;
            FormulaAplicada = formulaAplicada;
            OrigemDados = origemDados;
            DataCalculo = DateTime.UtcNow;
            CalculadoPor = calculadoPor;

            AddNotifications(new Contract<IndicadorConfiabilidade>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria.")
                .IsNotNullOrEmpty(formulaAplicada, nameof(FormulaAplicada), "A formula aplicada e obrigatoria para reproduzir o calculo."));
        }
    }

    /// <summary>MAN-CRV — Recomendacao de estrategia de manutencao. EF 11.4.</summary>
    public class RecomendacaoEstrategia : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public EEstrategiaManutencao Estrategia { get; private set; }
        public string Justificativa { get; private set; } = string.Empty;
        public int? RpnReferencia { get; private set; }
        public decimal? MtbfReferencia { get; private set; }
        public decimal? MttrReferencia { get; private set; }
        public decimal? DisponibilidadeReferencia { get; private set; }
        public DateTime DataRecomendacao { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public EStatusRecomendacaoEstrategia Status { get; private set; } = EStatusRecomendacaoEstrategia.Proposta;

        protected RecomendacaoEstrategia() { }

        public RecomendacaoEstrategia(
            Guid revisaoId,
            EEstrategiaManutencao estrategia,
            string justificativa,
            int? rpnReferencia,
            decimal? mtbfReferencia,
            decimal? mttrReferencia,
            decimal? disponibilidadeReferencia,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            Estrategia = estrategia;
            Justificativa = justificativa;
            RpnReferencia = rpnReferencia;
            MtbfReferencia = mtbfReferencia;
            MttrReferencia = mttrReferencia;
            DisponibilidadeReferencia = disponibilidadeReferencia;
            ResponsavelId = responsavelId;
            DataRecomendacao = DateTime.UtcNow;
            Status = EStatusRecomendacaoEstrategia.Proposta;

            AddNotifications(new Contract<RecomendacaoEstrategia>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria.")
                .IsNotNullOrEmpty(justificativa, nameof(Justificativa), "A justificativa da recomendacao e obrigatoria.")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel pela recomendacao e obrigatorio."));
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusRecomendacaoEstrategia.Proposta)
            {
                AddNotification(nameof(Status), "Somente recomendacoes propostas podem ser aprovadas.");
                return;
            }
            Status = EStatusRecomendacaoEstrategia.Aprovada;
            MarcarAlterado(usuario);
        }

        public void Rejeitar(string usuario)
        {
            if (Status != EStatusRecomendacaoEstrategia.Proposta)
            {
                AddNotification(nameof(Status), "Somente recomendacoes propostas podem ser rejeitadas.");
                return;
            }
            Status = EStatusRecomendacaoEstrategia.Rejeitada;
            MarcarAlterado(usuario);
        }

        public void Substituir(string usuario)
        {
            Status = EStatusRecomendacaoEstrategia.Substituida;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-CRV — Historico/auditoria. EF 11.5.</summary>
    public class HistoricoConfiabilidade : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public EAcaoHistoricoConfiabilidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string? IpOrigem { get; private set; }
        public string? Justificativa { get; private set; }
        public string PayloadJson { get; private set; } = "{}";

        protected HistoricoConfiabilidade() { }

        public HistoricoConfiabilidade(
            Guid revisaoId,
            EAcaoHistoricoConfiabilidade acao,
            Guid usuarioId,
            string? ipOrigem,
            string? justificativa,
            string payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            Acao = acao;
            UsuarioId = usuarioId;
            DataHora = DateTime.UtcNow;
            IpOrigem = ipOrigem;
            Justificativa = justificativa;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;

            AddNotifications(new Contract<HistoricoConfiabilidade>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria."));
        }
    }

    /// <summary>MAN-CRV — Anexo/evidencia. EF 11.6.</summary>
    public class AnexoConfiabilidade : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoDocumento { get; private set; }
        public string? Descricao { get; private set; }
        public bool Obrigatorio { get; private set; }
        public DateTime DataInclusao { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected AnexoConfiabilidade() { }

        public AnexoConfiabilidade(
            Guid revisaoId,
            Guid arquivoId,
            string? tipoDocumento,
            string? descricao,
            bool obrigatorio,
            Guid usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            ArquivoId = arquivoId;
            TipoDocumento = tipoDocumento;
            Descricao = descricao;
            Obrigatorio = obrigatorio;
            DataInclusao = DateTime.UtcNow;
            UsuarioId = usuarioId;

            AddNotifications(new Contract<AnexoConfiabilidade>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria.")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio."));
        }
    }

    /// <summary>MAN-CRV — Parametro por tenant. EF 11.7.</summary>
    public class ParametroConfiabilidade : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public string? Descricao { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected ParametroConfiabilidade() { }

        public ParametroConfiabilidade(
            string chave,
            string valorJson,
            string? descricao,
            DateTime? vigenciaInicio,
            DateTime? vigenciaFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            ValorJson = string.IsNullOrWhiteSpace(valorJson) ? "{}" : valorJson;
            Descricao = descricao;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Ativo = true;

            AddNotifications(new Contract<ParametroConfiabilidade>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria."));
        }

        public void Inativar(string usuario)
        {
            Ativo = false;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-CRV — Evento de integracao (publicacao pos-confirmacao). EF 11.8.</summary>
    public class EventoIntegracaoConfiabilidade : EntidadeSaaSBase
    {
        public Guid RevisaoId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public string DestinoFuncional { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public EStatusEnvioEvento StatusEnvio { get; private set; } = EStatusEnvioEvento.Pendente;
        public int Tentativas { get; private set; }
        public string? UltimoErro { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public DateTime? DataEnvio { get; private set; }

        protected EventoIntegracaoConfiabilidade() { }

        public EventoIntegracaoConfiabilidade(
            Guid revisaoId,
            string tipoEvento,
            string destinoFuncional,
            string payloadJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RevisaoId = revisaoId;
            TipoEvento = tipoEvento;
            DestinoFuncional = destinoFuncional;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            StatusEnvio = EStatusEnvioEvento.Pendente;
            Tentativas = 0;
            DataCriacao = DateTime.UtcNow;

            AddNotifications(new Contract<EventoIntegracaoConfiabilidade>()
                .Requires()
                .AreNotEquals(revisaoId, Guid.Empty, nameof(RevisaoId), "A revisao e obrigatoria.")
                .IsNotNullOrEmpty(tipoEvento, nameof(TipoEvento), "O tipo do evento e obrigatorio.")
                .IsNotNullOrEmpty(destinoFuncional, nameof(DestinoFuncional), "O destino funcional e obrigatorio."));
        }

        public void MarcarEnviado(string usuario)
        {
            StatusEnvio = EStatusEnvioEvento.Enviado;
            DataEnvio = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void MarcarFalha(string erro, string usuario)
        {
            StatusEnvio = EStatusEnvioEvento.Falhou;
            Tentativas++;
            UltimoErro = erro;
            MarcarAlterado(usuario);
        }
    }
}
