using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_ncr_origem_ref — Referencia de origem da NCR (secao 11.2).</summary>
    public class NcrOrigemRef : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public ENcrOrigem TipoOrigem { get; private set; }
        public string? ReferenciaId { get; private set; }
        public string? ReferenciaCodigo { get; private set; }
        public bool OrigemPrincipal { get; private set; }
        public string? Observacao { get; private set; }

        protected NcrOrigemRef() { }

        public NcrOrigemRef(Guid ncrId, ENcrOrigem tipoOrigem, bool origemPrincipal, string? referenciaId,
            string? referenciaCodigo, string? observacao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrOrigemRef>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrOrigemRef]"));
            NcrId = ncrId;
            TipoOrigem = tipoOrigem;
            OrigemPrincipal = origemPrincipal;
            ReferenciaId = referenciaId;
            ReferenciaCodigo = referenciaCodigo;
            Observacao = observacao;
        }
    }

    /// <summary>qld_ncr_causa_raiz — Investigacao e conclusao de causa (secao 11.3).</summary>
    public class NcrCausaRaiz : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public ENcrMetodoCausa Metodo { get; private set; }
        public string DescricaoAnalise { get; private set; } = string.Empty;
        public string CausaIdentificada { get; private set; } = string.Empty;
        public string Conclusao { get; private set; } = string.Empty;
        public Guid? AprovadoPor { get; private set; }
        public DateTime? AprovadoEm { get; private set; }

        protected NcrCausaRaiz() { }

        public NcrCausaRaiz(Guid ncrId, ENcrMetodoCausa metodo, string descricaoAnalise, string causaIdentificada,
            string conclusao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrCausaRaiz>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrCausaRaiz]")
                .IsNotNullOrEmpty(descricaoAnalise, nameof(DescricaoAnalise), "A descricao da analise e obrigatoria [Origem: NcrCausaRaiz]")
                .IsNotNullOrEmpty(causaIdentificada, nameof(CausaIdentificada), "A causa identificada e obrigatoria [Origem: NcrCausaRaiz]")
                .IsNotNullOrEmpty(conclusao, nameof(Conclusao), "A conclusao e obrigatoria [Origem: NcrCausaRaiz]"));
            NcrId = ncrId;
            Metodo = metodo;
            DescricaoAnalise = descricaoAnalise;
            CausaIdentificada = causaIdentificada;
            Conclusao = conclusao;
        }

        public void Aprovar(Guid aprovadoPor, string usuario)
        {
            AprovadoPor = aprovadoPor;
            AprovadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_ncr_acao_capa — Acao corretiva/preventiva/contencao (secao 11.4).</summary>
    public class NcrAcaoCapa : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public ENcrTipoAcao TipoAcao { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public DateTime Prazo { get; private set; }
        public ENcrStatusAcao Status { get; private set; }
        public bool EvidenciaObrigatoria { get; private set; }
        public DateTime? DataConclusao { get; private set; }
        public string? Resultado { get; private set; }
        public string? MotivoCancelamento { get; private set; }

        protected NcrAcaoCapa() { }

        public NcrAcaoCapa(Guid ncrId, ENcrTipoAcao tipoAcao, string descricao, Guid responsavelId, DateTime prazo,
            bool evidenciaObrigatoria, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrAcaoCapa>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrAcaoCapa]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao da acao e obrigatoria [Origem: NcrAcaoCapa]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel pela acao e obrigatorio [Origem: NcrAcaoCapa]"));
            NcrId = ncrId;
            TipoAcao = tipoAcao;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Prazo = prazo;
            EvidenciaObrigatoria = evidenciaObrigatoria;
            Status = ENcrStatusAcao.Pendente;
        }

        public void Concluir(string resultado, string usuario)
        {
            if (Status == ENcrStatusAcao.Cancelada || Status == ENcrStatusAcao.Concluida)
            {
                AddNotification(nameof(Status), "Acao ja finalizada nao pode ser concluida [Origem: NcrAcaoCapa]");
                return;
            }
            if (EvidenciaObrigatoria)
            {
                AddNotifications(new Contract<NcrAcaoCapa>()
                    .Requires()
                    .IsNotNullOrEmpty(resultado, nameof(Resultado), "Evidencia/resultado e obrigatorio para concluir a acao [Origem: NcrAcaoCapa]"));
                if (!IsValid) return;
            }
            Resultado = resultado;
            Status = ENcrStatusAcao.Concluida;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            AddNotifications(new Contract<NcrAcaoCapa>()
                .Requires()
                .IsNotNullOrEmpty(motivo, nameof(MotivoCancelamento), "O motivo do cancelamento e obrigatorio [Origem: NcrAcaoCapa]"));
            if (!IsValid) return;
            MotivoCancelamento = motivo;
            Status = ENcrStatusAcao.Cancelada;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_ncr_verificacao_eficacia — Verificacao de eficacia (secao 11.5).</summary>
    public class NcrVerificacaoEficacia : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public Guid? AcaoCapaId { get; private set; }
        public string Criterio { get; private set; } = string.Empty;
        public ENcrResultadoVerificacao Resultado { get; private set; }
        public string DescricaoResultado { get; private set; } = string.Empty;
        public Guid VerificadoPor { get; private set; }
        public DateTime VerificadoEm { get; private set; }
        public string? ProximaAcao { get; private set; }

        protected NcrVerificacaoEficacia() { }

        public NcrVerificacaoEficacia(Guid ncrId, Guid? acaoCapaId, string criterio, ENcrResultadoVerificacao resultado,
            string descricaoResultado, Guid verificadoPor, string? proximaAcao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrVerificacaoEficacia>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrVerificacaoEficacia]")
                .IsNotNullOrEmpty(criterio, nameof(Criterio), "O criterio de verificacao e obrigatorio [Origem: NcrVerificacaoEficacia]")
                .IsNotNullOrEmpty(descricaoResultado, nameof(DescricaoResultado), "A descricao do resultado e obrigatoria [Origem: NcrVerificacaoEficacia]")
                .AreNotEquals(verificadoPor, Guid.Empty, nameof(VerificadoPor), "O responsavel pela verificacao e obrigatorio [Origem: NcrVerificacaoEficacia]"));

            if (resultado == ENcrResultadoVerificacao.Reprovada && string.IsNullOrWhiteSpace(proximaAcao))
                AddNotification(nameof(ProximaAcao), "A proxima acao e obrigatoria quando a verificacao e reprovada [Origem: NcrVerificacaoEficacia]");

            NcrId = ncrId;
            AcaoCapaId = acaoCapaId;
            Criterio = criterio;
            Resultado = resultado;
            DescricaoResultado = descricaoResultado;
            VerificadoPor = verificadoPor;
            ProximaAcao = proximaAcao;
            VerificadoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_ncr_anexo — Evidencias documentais da NCR (secao 11.6).</summary>
    public class NcrAnexo : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public ENcrEntidadeAlvo EntidadeAlvo { get; private set; }
        public Guid? EntidadeAlvoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoEvidencia { get; private set; }
        public string? Observacao { get; private set; }

        protected NcrAnexo() { }

        public NcrAnexo(Guid ncrId, ENcrEntidadeAlvo entidadeAlvo, Guid arquivoId, Guid? entidadeAlvoId,
            string? tipoEvidencia, string? observacao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrAnexo>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: NcrAnexo]"));
            NcrId = ncrId;
            EntidadeAlvo = entidadeAlvo;
            ArquivoId = arquivoId;
            EntidadeAlvoId = entidadeAlvoId;
            TipoEvidencia = tipoEvidencia;
            Observacao = observacao;
        }
    }

    /// <summary>qld_ncr_historico — Trilha auditavel imutavel (secao 11.7).</summary>
    public class NcrHistorico : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public string? EstadoAnterior { get; private set; }
        public string? EstadoNovo { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? IpOrigem { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Justificativa { get; private set; }

        protected NcrHistorico() { }

        public NcrHistorico(Guid ncrId, EAcaoHistoricoQualidade acao, Guid usuarioId, string payloadJson,
            string? estadoAnterior, string? estadoNovo, string? ipOrigem, string? justificativa,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrHistorico>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrHistorico]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio [Origem: NcrHistorico]"));
            NcrId = ncrId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson ?? "{}";
            EstadoAnterior = estadoAnterior;
            EstadoNovo = estadoNovo;
            IpOrigem = ipOrigem;
            Justificativa = justificativa;
        }
    }

    /// <summary>qld_ncr_parametro — Parametros por tenant (secao 11.8).</summary>
    public class NcrParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected NcrParametro() { }

        public NcrParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: NcrParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: NcrParametro]"));
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

    /// <summary>qld_ncr_evento_integracao — Eventos funcionais emitidos/recebidos (secao 11.9).</summary>
    public class NcrEventoIntegracao : EntidadeSaaSBase
    {
        public Guid NcrId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public EDirecaoEvento Direcao { get; private set; }

        protected NcrEventoIntegracao() { }

        public NcrEventoIntegracao(Guid ncrId, string tipoEvento, EDirecaoEvento direcao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<NcrEventoIntegracao>()
                .Requires()
                .AreNotEquals(ncrId, Guid.Empty, nameof(NcrId), "A NCR e obrigatoria [Origem: NcrEventoIntegracao]")
                .IsNotNullOrEmpty(tipoEvento, nameof(TipoEvento), "O tipo de evento e obrigatorio [Origem: NcrEventoIntegracao]"));
            NcrId = ncrId;
            TipoEvento = tipoEvento;
            Direcao = direcao;
        }
    }
}
