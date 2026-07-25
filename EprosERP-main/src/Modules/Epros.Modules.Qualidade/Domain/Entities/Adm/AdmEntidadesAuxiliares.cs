using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_adm_item — Item do registro raiz (secao 12.2).</summary>
    public class AdmItem : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public int Sequencia { get; private set; }
        public string? TipoItem { get; private set; }
        public decimal? Quantidade { get; private set; }
        public string? Observacao { get; private set; }

        protected AdmItem() { }

        public AdmItem(Guid qualidadeId, int sequencia, string? tipoItem, decimal? quantidade, string? observacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmItem>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmItem]"));
            QualidadeId = qualidadeId;
            Sequencia = sequencia;
            TipoItem = tipoItem;
            Quantidade = quantidade;
            Observacao = observacao;
        }
    }

    /// <summary>qld_adm_documento_qms — Documento QMS versionado (secao 12.3).</summary>
    public class AdmDocumentoQms : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public string TipoDocumento { get; private set; } = string.Empty;
        public string VersaoDocumento { get; private set; } = string.Empty;
        public EStatusRegistroQualidade Status { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public Guid? AprovadorId { get; private set; }
        public Guid? ArquivoId { get; private set; }
        public string? MotivoRevisao { get; private set; }

        protected AdmDocumentoQms() { }

        public AdmDocumentoQms(Guid qualidadeId, string codigo, string titulo, string tipoDocumento, string versaoDocumento,
            Guid responsavelId, DateTime? vigenciaInicio, DateTime? vigenciaFim, Guid? aprovadorId, Guid? arquivoId,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmDocumentoQms>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmDocumentoQms]")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do documento e obrigatorio [Origem: AdmDocumentoQms]")
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O titulo do documento e obrigatorio [Origem: AdmDocumentoQms]")
                .IsNotNullOrEmpty(tipoDocumento, nameof(TipoDocumento), "O tipo do documento e obrigatorio [Origem: AdmDocumentoQms]")
                .IsNotNullOrEmpty(versaoDocumento, nameof(VersaoDocumento), "A versao do documento e obrigatoria [Origem: AdmDocumentoQms]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: AdmDocumentoQms]"));
            QualidadeId = qualidadeId;
            Codigo = codigo;
            Titulo = titulo;
            TipoDocumento = tipoDocumento;
            VersaoDocumento = versaoDocumento;
            ResponsavelId = responsavelId;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            AprovadorId = aprovadorId;
            ArquivoId = arquivoId;
            Status = EStatusRegistroQualidade.Rascunho;
        }

        public void Revisar(string versaoDocumento, string motivoRevisao, string usuario)
        {
            AddNotifications(new Contract<AdmDocumentoQms>()
                .Requires()
                .IsNotNullOrEmpty(motivoRevisao, nameof(MotivoRevisao), "O motivo da revisao e obrigatorio [Origem: AdmDocumentoQms]"));
            if (!IsValid) return;
            VersaoDocumento = versaoDocumento;
            MotivoRevisao = motivoRevisao;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_adm_objetivo — Objetivo da qualidade (secao 12.4).</summary>
    public class AdmObjetivo : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public string Objetivo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public Guid? AreaId { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public string? Meta { get; private set; }
        public string? Unidade { get; private set; }
        public DateTime? Prazo { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }

        protected AdmObjetivo() { }

        public AdmObjetivo(Guid qualidadeId, string objetivo, Guid responsavelId, string? descricao, Guid? areaId,
            string? meta, string? unidade, DateTime? prazo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmObjetivo>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmObjetivo]")
                .IsNotNullOrEmpty(objetivo, nameof(Objetivo), "O objetivo e obrigatorio [Origem: AdmObjetivo]")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio [Origem: AdmObjetivo]"));
            QualidadeId = qualidadeId;
            Objetivo = objetivo;
            ResponsavelId = responsavelId;
            Descricao = descricao;
            AreaId = areaId;
            Meta = meta;
            Unidade = unidade;
            Prazo = prazo;
            Status = EStatusRegistroQualidade.Rascunho;
        }
    }

    /// <summary>qld_adm_kpi — KPI da qualidade (secao 12.5).</summary>
    public class AdmKpi : EntidadeSaaSBase
    {
        public Guid? ObjetivoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Formula { get; private set; }
        public string? FonteDados { get; private set; }
        public string Periodicidade { get; private set; } = string.Empty;
        public decimal Meta { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public string Periodo { get; private set; } = string.Empty;
        public decimal? Resultado { get; private set; }
        public string? StatusResultado { get; private set; }

        protected AdmKpi() { }

        public AdmKpi(string codigo, string nome, string periodicidade, decimal meta, string unidade, string periodo,
            Guid? objetivoId, string? formula, string? fonteDados, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmKpi>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do KPI e obrigatorio [Origem: AdmKpi]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do KPI e obrigatorio [Origem: AdmKpi]")
                .IsNotNullOrEmpty(periodicidade, nameof(Periodicidade), "A periodicidade e obrigatoria [Origem: AdmKpi]")
                .IsNotNullOrEmpty(unidade, nameof(Unidade), "A unidade e obrigatoria [Origem: AdmKpi]")
                .IsNotNullOrEmpty(periodo, nameof(Periodo), "O periodo e obrigatorio [Origem: AdmKpi]"));
            Codigo = codigo;
            Nome = nome;
            Periodicidade = periodicidade;
            Meta = meta;
            Unidade = unidade;
            Periodo = periodo;
            ObjetivoId = objetivoId;
            Formula = formula;
            FonteDados = fonteDados;
        }

        public void ApurarResultado(decimal resultado, string? statusResultado, string usuario)
        {
            Resultado = resultado;
            StatusResultado = statusResultado;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_adm_programa_auditoria — Programa de auditorias (secao 12.6).</summary>
    public class AdmProgramaAuditoria : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public ETipoAuditoria TipoAuditoria { get; private set; }
        public string Escopo { get; private set; } = string.Empty;
        public DateTime DataPrevista { get; private set; }
        public DateTime? DataRealizada { get; private set; }
        public Guid? AuditorId { get; private set; }
        public Guid? AreaAuditadaId { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }
        public string? Observacao { get; private set; }

        protected AdmProgramaAuditoria() { }

        public AdmProgramaAuditoria(Guid qualidadeId, string nome, ETipoAuditoria tipoAuditoria, string escopo,
            DateTime dataPrevista, Guid? auditorId, Guid? areaAuditadaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmProgramaAuditoria>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmProgramaAuditoria]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do programa e obrigatorio [Origem: AdmProgramaAuditoria]")
                .IsNotNullOrEmpty(escopo, nameof(Escopo), "O escopo e obrigatorio [Origem: AdmProgramaAuditoria]"));
            QualidadeId = qualidadeId;
            Nome = nome;
            TipoAuditoria = tipoAuditoria;
            Escopo = escopo;
            DataPrevista = dataPrevista;
            AuditorId = auditorId;
            AreaAuditadaId = areaAuditadaId;
            Status = EStatusRegistroQualidade.Rascunho;
        }

        public void Realizar(DateTime dataRealizada, string usuario)
        {
            DataRealizada = dataRealizada;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_adm_historico — Trilha auditavel (secao 12.7).</summary>
    public class AdmHistorico : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? Ip { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected AdmHistorico() { }

        public AdmHistorico(Guid qualidadeId, string entidade, Guid entidadeId, EAcaoHistoricoQualidade acao, Guid usuarioId,
            string payloadJson, string? ip, string? motivo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmHistorico>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmHistorico]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade e obrigatoria [Origem: AdmHistorico]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio [Origem: AdmHistorico]"));
            QualidadeId = qualidadeId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson ?? "{}";
            Ip = ip;
            Motivo = motivo;
            OcorridoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_adm_anexo — Evidencia documental (secao 12.8).</summary>
    public class AdmAnexo : EntidadeSaaSBase
    {
        public Guid QualidadeId { get; private set; }
        public string? Entidade { get; private set; }
        public Guid? EntidadeId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected AdmAnexo() { }

        public AdmAnexo(Guid qualidadeId, Guid arquivoId, string? entidade, Guid? entidadeId, string? tipoAnexo,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmAnexo>()
                .Requires()
                .AreNotEquals(qualidadeId, Guid.Empty, nameof(QualidadeId), "O registro raiz e obrigatorio [Origem: AdmAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: AdmAnexo]"));
            QualidadeId = qualidadeId;
            ArquivoId = arquivoId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            TipoAnexo = tipoAnexo;
        }
    }

    /// <summary>qld_adm_parametro — Parametros por tenant (secao 12.9).</summary>
    public class AdmParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected AdmParametro() { }

        public AdmParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AdmParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: AdmParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: AdmParametro]"));
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
}
