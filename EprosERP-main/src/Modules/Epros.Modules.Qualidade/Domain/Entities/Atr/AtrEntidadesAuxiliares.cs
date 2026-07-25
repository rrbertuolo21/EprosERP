using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_atr_vinculo_contexto — Vinculo do atributo a um contexto (secao 12.2).</summary>
    public class AtrVinculoContexto : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public EContextoVinculoAtributo ContextoTipo { get; private set; }
        public Guid ContextoId { get; private set; }
        public bool Obrigatorio { get; private set; }
        public int? Posicao { get; private set; }
        public DateTime? VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public bool Ativo { get; private set; }

        protected AtrVinculoContexto() { }

        public AtrVinculoContexto(Guid atributoId, EContextoVinculoAtributo contextoTipo, Guid contextoId, bool obrigatorio,
            int? posicao, DateTime? vigenciaInicio, DateTime? vigenciaFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrVinculoContexto>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrVinculoContexto]")
                .AreNotEquals(contextoId, Guid.Empty, nameof(ContextoId), "O contexto e obrigatorio [Origem: AtrVinculoContexto]"));
            AtributoId = atributoId;
            ContextoTipo = contextoTipo;
            ContextoId = contextoId;
            Obrigatorio = obrigatorio;
            Posicao = posicao;
            VigenciaInicio = vigenciaInicio;
            VigenciaFim = vigenciaFim;
            Ativo = true;
        }
    }

    /// <summary>qld_atr_especificacao — Especificacao versionada de qualidade/regulatorio (secao 12.3).</summary>
    public class AtrEspecificacao : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public EContextoVinculoAtributo? ContextoTipo { get; private set; }
        public Guid? ContextoId { get; private set; }
        public string VersaoEspecificacao { get; private set; } = string.Empty;
        public string? ValorNominal { get; private set; }
        public decimal? LimiteInferior { get; private set; }
        public decimal? LimiteSuperior { get; private set; }
        public decimal? ToleranciaMenos { get; private set; }
        public decimal? ToleranciaMais { get; private set; }
        public string? Unidade { get; private set; }
        public string? MetodoMedicao { get; private set; }
        public string? Criticidade { get; private set; }
        public DateTime VigenciaInicio { get; private set; }
        public DateTime? VigenciaFim { get; private set; }
        public EStatusRegistroQualidade Status { get; private set; }

        protected AtrEspecificacao() { }

        public AtrEspecificacao(Guid atributoId, string versaoEspecificacao, DateTime vigenciaInicio,
            EContextoVinculoAtributo? contextoTipo, Guid? contextoId, string? valorNominal, decimal? limiteInferior,
            decimal? limiteSuperior, decimal? toleranciaMenos, decimal? toleranciaMais, string? unidade,
            string? metodoMedicao, string? criticidade, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrEspecificacao>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrEspecificacao]")
                .IsNotNullOrEmpty(versaoEspecificacao, nameof(VersaoEspecificacao), "A versao da especificacao e obrigatoria [Origem: AtrEspecificacao]"));

            // CA-ATR-010: limite inferior nao pode ser maior que o superior.
            if (limiteInferior.HasValue && limiteSuperior.HasValue && limiteInferior > limiteSuperior)
                AddNotification(nameof(LimiteInferior), "O limite inferior nao pode ser maior que o superior [Origem: AtrEspecificacao]");

            // QLD-ATR-RN-010: unidade obrigatoria quando houver nominal/limite.
            if ((valorNominal != null || limiteInferior.HasValue || limiteSuperior.HasValue) && string.IsNullOrWhiteSpace(unidade))
                AddNotification(nameof(Unidade), "A unidade e obrigatoria quando houver valor nominal, limite ou tolerancia [Origem: AtrEspecificacao]");

            AtributoId = atributoId;
            VersaoEspecificacao = versaoEspecificacao;
            VigenciaInicio = vigenciaInicio;
            ContextoTipo = contextoTipo;
            ContextoId = contextoId;
            ValorNominal = valorNominal;
            LimiteInferior = limiteInferior;
            LimiteSuperior = limiteSuperior;
            ToleranciaMenos = toleranciaMenos;
            ToleranciaMais = toleranciaMais;
            Unidade = unidade;
            MetodoMedicao = metodoMedicao;
            Criticidade = criticidade;
            Status = EStatusRegistroQualidade.Rascunho;
        }
    }

    /// <summary>qld_atr_valor — Valor preenchido do atributo em um contexto (secao 12.4).</summary>
    public class AtrValor : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public string ContextoTipo { get; private set; } = string.Empty;
        public Guid ContextoId { get; private set; }
        public string? ValorTexto { get; private set; }
        public decimal? ValorNumero { get; private set; }
        public DateTime? ValorData { get; private set; }
        public bool? ValorBooleano { get; private set; }
        public string? ValorJson { get; private set; }
        public Guid? PreenchidoPor { get; private set; }
        public DateTime? PreenchidoEm { get; private set; }

        protected AtrValor() { }

        public AtrValor(Guid atributoId, string contextoTipo, Guid contextoId, string? valorTexto, decimal? valorNumero,
            DateTime? valorData, bool? valorBooleano, string? valorJson, Guid? preenchidoPor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrValor>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrValor]")
                .IsNotNullOrEmpty(contextoTipo, nameof(ContextoTipo), "O tipo de contexto e obrigatorio [Origem: AtrValor]")
                .AreNotEquals(contextoId, Guid.Empty, nameof(ContextoId), "O contexto e obrigatorio [Origem: AtrValor]"));

            // CA-ATR-009: valor texto acima de 255 caracteres.
            if (valorTexto != null && valorTexto.Length > 255)
                AddNotification(nameof(ValorTexto), "O valor texto nao pode exceder 255 caracteres [Origem: AtrValor]");

            AtributoId = atributoId;
            ContextoTipo = contextoTipo;
            ContextoId = contextoId;
            ValorTexto = valorTexto;
            ValorNumero = valorNumero;
            ValorData = valorData;
            ValorBooleano = valorBooleano;
            ValorJson = valorJson;
            PreenchidoPor = preenchidoPor;
            PreenchidoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_atr_opcao — Opcao de lista de valores permitidos (secao 12.5).</summary>
    public class AtrOpcao : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Rotulo { get; private set; } = string.Empty;
        public int? Posicao { get; private set; }
        public bool Ativo { get; private set; }

        protected AtrOpcao() { }

        public AtrOpcao(Guid atributoId, string codigo, string rotulo, int? posicao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrOpcao>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrOpcao]")
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo da opcao e obrigatorio [Origem: AtrOpcao]")
                .IsNotNullOrEmpty(rotulo, nameof(Rotulo), "O rotulo da opcao e obrigatorio [Origem: AtrOpcao]"));
            AtributoId = atributoId;
            Codigo = codigo;
            Rotulo = rotulo;
            Posicao = posicao;
            Ativo = true;
        }
    }

    /// <summary>qld_atr_historico — Trilha auditavel (secao 12.6).</summary>
    public class AtrHistorico : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected AtrHistorico() { }

        public AtrHistorico(Guid atributoId, string entidade, Guid entidadeId, EAcaoHistoricoQualidade acao, Guid usuarioId,
            string payloadJson, string? motivo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrHistorico>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrHistorico]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade e obrigatoria [Origem: AtrHistorico]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio [Origem: AtrHistorico]"));
            AtributoId = atributoId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson ?? "{}";
            Motivo = motivo;
            OcorridoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_atr_anexo — Documento formal vinculado ao atributo (secao 12.7).</summary>
    public class AtrAnexo : EntidadeSaaSBase
    {
        public Guid AtributoId { get; private set; }
        public string? Entidade { get; private set; }
        public Guid? EntidadeId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected AtrAnexo() { }

        public AtrAnexo(Guid atributoId, Guid arquivoId, string? entidade, Guid? entidadeId, string? tipoAnexo,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrAnexo>()
                .Requires()
                .AreNotEquals(atributoId, Guid.Empty, nameof(AtributoId), "O atributo e obrigatorio [Origem: AtrAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: AtrAnexo]"));
            AtributoId = atributoId;
            ArquivoId = arquivoId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            TipoAnexo = tipoAnexo;
        }
    }

    /// <summary>qld_atr_parametro — Parametros por tenant (secao 12.8).</summary>
    public class AtrParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected AtrParametro() { }

        public AtrParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AtrParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: AtrParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: AtrParametro]"));
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
