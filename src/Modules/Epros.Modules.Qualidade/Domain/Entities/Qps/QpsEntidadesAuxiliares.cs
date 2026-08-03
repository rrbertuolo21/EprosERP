using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_qps_documento — Documento de homologacao com validade (vencido -> re-homologacao).</summary>
    public class QpsDocumento : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public EQpsTipoDocumento TipoDocumento { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Numero { get; private set; }
        public DateTime? DataValidade { get; private set; }
        public Guid? ArquivoId { get; private set; }
        public bool Ativo { get; private set; }

        protected QpsDocumento() { }

        public QpsDocumento(Guid registroId, EQpsTipoDocumento tipoDocumento, string titulo, string? numero,
            DateTime? dataValidade, Guid? arquivoId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsDocumento>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsDocumento]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do documento e obrigatorio [Origem: QpsDocumento]"));
            RegistroId = registroId;
            TipoDocumento = tipoDocumento;
            Titulo = titulo;
            Numero = numero;
            DataValidade = dataValidade;
            ArquivoId = arquivoId;
            Ativo = true;
        }

        public bool EstaVencido(DateTime referencia) => DataValidade.HasValue && DataValidade.Value.Date < referencia.Date;

        public void Inativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }

    /// <summary>qld_qps_scorecard — Consolidacao do score do fornecedor por periodo.</summary>
    public class QpsScorecard : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public decimal Score { get; private set; }
        public bool AbaixoLimite { get; private set; }
        public string? Observacao { get; private set; }
        public DateTime CalculadoEm { get; private set; }

        protected QpsScorecard() { }

        public QpsScorecard(Guid registroId, string periodo, decimal score, bool abaixoLimite, string? observacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsScorecard>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsScorecard]")
                .IsNotNullOrEmpty(periodo, nameof(Periodo), "O periodo e obrigatorio [Origem: QpsScorecard]"));
            RegistroId = registroId;
            Periodo = periodo;
            Score = score;
            AbaixoLimite = abaixoLimite;
            Observacao = observacao;
            CalculadoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_qps_indicador — Indicador rastreavel (PPM, OTIF-qualidade, peso NCR...) do scorecard.</summary>
    public class QpsIndicador : EntidadeSaaSBase
    {
        public Guid ScorecardId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public decimal Peso { get; private set; }
        public string? Fonte { get; private set; }

        protected QpsIndicador() { }

        public QpsIndicador(Guid scorecardId, string codigo, decimal valor, decimal peso, string? fonte,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsIndicador>()
                .Requires()
                .AreNotEquals(scorecardId, Guid.Empty, nameof(ScorecardId), "O scorecard e obrigatorio [Origem: QpsIndicador]")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do indicador e obrigatorio [Origem: QpsIndicador]")
                .IsGreaterOrEqualsThan(peso, 0, nameof(Peso), "O peso nao pode ser negativo [Origem: QpsIndicador]"));
            ScorecardId = scorecardId;
            Codigo = codigo;
            Valor = valor;
            Peso = peso;
            Fonte = fonte;
        }
    }

    /// <summary>qld_qps_bloqueio — Bloqueio do fornecedor (manual/automatico) com alcada e motivo.</summary>
    public class QpsBloqueio : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public EQpsTipoBloqueio TipoBloqueio { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public Guid? AlcadaId { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }

        protected QpsBloqueio() { }

        public QpsBloqueio(Guid registroId, EQpsTipoBloqueio tipoBloqueio, string motivo, Guid? alcadaId,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsBloqueio>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsBloqueio]")
                .IsNotNullOrEmpty(motivo, nameof(Motivo), "O motivo do bloqueio e obrigatorio [Origem: QpsBloqueio]"));
            RegistroId = registroId;
            TipoBloqueio = tipoBloqueio;
            Motivo = motivo;
            AlcadaId = alcadaId;
            Ativo = true;
            DataInicio = DateTime.UtcNow;
        }

        public void Desbloquear(string usuario) { Ativo = false; DataFim = DateTime.UtcNow; MarcarAlterado(usuario); }
    }

    /// <summary>qld_qps_plano_8d — Plano 8D para tratativa de problema recorrente do fornecedor.</summary>
    public class QpsPlano8d : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public EQps8dDisciplina DisciplinaAtual { get; private set; }
        public EQps8dStatus Status { get; private set; }
        public string? Conclusao { get; private set; }

        protected QpsPlano8d() { }

        public QpsPlano8d(Guid registroId, string titulo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsPlano8d>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsPlano8d]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do plano 8D e obrigatorio [Origem: QpsPlano8d]"));
            RegistroId = registroId;
            Titulo = titulo;
            DisciplinaAtual = EQps8dDisciplina.D1_Equipe;
            Status = EQps8dStatus.Aberto;
        }

        public void AvancarDisciplina(EQps8dDisciplina disciplina, string usuario)
        {
            DisciplinaAtual = disciplina;
            if (Status == EQps8dStatus.Aberto) Status = EQps8dStatus.EmAndamento;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string conclusao, string usuario)
        {
            AddNotifications(new Contract<QpsPlano8d>()
                .Requires()
                .IsNotNullOrEmpty(conclusao, nameof(Conclusao), "A conclusao e obrigatoria no encerramento do 8D [Origem: QpsPlano8d]"));
            if (!IsValid) return;
            Conclusao = conclusao;
            DisciplinaAtual = EQps8dDisciplina.D8_Encerramento;
            Status = EQps8dStatus.Concluido;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_qps_acao_8d — Acao de uma disciplina do plano 8D.</summary>
    public class QpsAcao8d : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public EQps8dDisciplina Disciplina { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public DateTime Prazo { get; private set; }
        public EQps8dStatusAcao Status { get; private set; }

        protected QpsAcao8d() { }

        public QpsAcao8d(Guid planoId, EQps8dDisciplina disciplina, string descricao, Guid responsavelId,
            DateTime prazo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsAcao8d>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "O plano 8D e obrigatorio [Origem: QpsAcao8d]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao da acao e obrigatoria [Origem: QpsAcao8d]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: QpsAcao8d]"));
            PlanoId = planoId;
            Disciplina = disciplina;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Prazo = prazo;
            Status = EQps8dStatusAcao.Pendente;
        }

        public void Concluir(string usuario) { Status = EQps8dStatusAcao.Concluida; MarcarAlterado(usuario); }
    }

    /// <summary>qld_qps_anexo — Evidencia documental do QPS.</summary>
    public class QpsAnexo : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected QpsAnexo() { }

        public QpsAnexo(Guid registroId, Guid arquivoId, string? tipoAnexo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsAnexo>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: QpsAnexo]"));
            RegistroId = registroId;
            ArquivoId = arquivoId;
            TipoAnexo = tipoAnexo;
        }
    }

    /// <summary>qld_qps_historico — Trilha auditavel do QPS.</summary>
    public class QpsHistorico : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected QpsHistorico() { }

        public QpsHistorico(Guid registroId, string entidade, EAcaoHistoricoQualidade acao, Guid usuarioId,
            string payloadJson, string? motivo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsHistorico>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsHistorico]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade e obrigatoria [Origem: QpsHistorico]"));
            RegistroId = registroId;
            Entidade = entidade;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson ?? "{}";
            Motivo = motivo;
            OcorridoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_qps_parametro — Parametros por tenant (formula/pesos do score, limiares — GAP negocio D14).</summary>
    public class QpsParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected QpsParametro() { }

        public QpsParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: QpsParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: QpsParametro]"));
            Chave = chave;
            ValorJson = valorJson;
            Ativo = true;
        }

        public void Atualizar(string valorJson, bool ativo, string usuario)
        {
            ValorJson = valorJson;
            Ativo = ativo;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_qps_evento — Eventos funcionais emitidos/recebidos do QPS.</summary>
    public class QpsEvento : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public EDirecaoEvento Direcao { get; private set; }

        protected QpsEvento() { }

        public QpsEvento(Guid registroId, string tipoEvento, EDirecaoEvento direcao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<QpsEvento>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio [Origem: QpsEvento]")
                .IsNotNullOrEmpty(tipoEvento, nameof(TipoEvento), "O tipo de evento e obrigatorio [Origem: QpsEvento]"));
            RegistroId = registroId;
            TipoEvento = tipoEvento;
            Direcao = direcao;
        }
    }
}
