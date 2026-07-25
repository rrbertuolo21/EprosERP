using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_acr_item — Item avaliado na analise (secao 12.2).</summary>
    public class AcrItem : EntidadeSaaSBase
    {
        public Guid AnaliseId { get; private set; }
        public int Sequencia { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public string? CodigoItem { get; private set; }
        public string? NomeItem { get; private set; }
        public string? Lote { get; private set; }
        public decimal? QuantidadeIntegral { get; private set; }
        public decimal QuantidadeAnalisada { get; private set; }
        public string UnidadeMedida { get; private set; } = string.Empty;
        public bool ItemParcial { get; private set; }
        public string? Ncm { get; private set; }
        public string? Cfop { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public string? Observacao { get; private set; }

        protected AcrItem() { }

        public AcrItem(Guid analiseId, int sequencia, decimal quantidadeAnalisada, string unidadeMedida, bool itemParcial,
            Guid? produtoId, string? codigoItem, string? nomeItem, string? lote, decimal? quantidadeIntegral,
            string? ncm, string? cfop, decimal? valorUnitario, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrItem>()
                .Requires()
                .AreNotEquals(analiseId, Guid.Empty, nameof(AnaliseId), "A analise e obrigatoria [Origem: AcrItem]")
                .IsGreaterThan(quantidadeAnalisada, 0, nameof(QuantidadeAnalisada), "A quantidade analisada deve ser maior que zero [Origem: AcrItem]")
                .IsNotNullOrEmpty(unidadeMedida, nameof(UnidadeMedida), "A unidade de medida e obrigatoria [Origem: AcrItem]"));
            AnaliseId = analiseId;
            Sequencia = sequencia;
            QuantidadeAnalisada = quantidadeAnalisada;
            UnidadeMedida = unidadeMedida;
            ItemParcial = itemParcial;
            ProdutoId = produtoId;
            CodigoItem = codigoItem;
            NomeItem = nomeItem;
            Lote = lote;
            QuantidadeIntegral = quantidadeIntegral;
            Ncm = ncm;
            Cfop = cfop;
            ValorUnitario = valorUnitario;
            Observacao = observacao;
        }
    }

    /// <summary>qld_acr_resultado — Decisao de qualidade da analise (secao 12.3).</summary>
    public class AcrResultado : EntidadeSaaSBase
    {
        public Guid AnaliseId { get; private set; }
        public EResultadoAcr ResultadoInspecao { get; private set; }
        public Guid? MotivoId { get; private set; }
        public string? Severidade { get; private set; }
        public decimal? QuantidadeAfetada { get; private set; }
        public string? Justificativa { get; private set; }
        public Guid? AprovadoPor { get; private set; }
        public DateTime DecididoEm { get; private set; }
        public bool StatusFiscalIgnorado { get; private set; }

        protected AcrResultado() { }

        public AcrResultado(Guid analiseId, EResultadoAcr resultadoInspecao, Guid? motivoId, string? severidade,
            decimal? quantidadeAfetada, string? justificativa, Guid? aprovadoPor, bool statusFiscalIgnorado,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrResultado>()
                .Requires()
                .AreNotEquals(analiseId, Guid.Empty, nameof(AnaliseId), "A analise e obrigatoria [Origem: AcrResultado]"));

            // RN: motivo obrigatorio para rejeicao/quarentena/desvio.
            if ((resultadoInspecao == EResultadoAcr.Rejeitado || resultadoInspecao == EResultadoAcr.Quarentena
                 || resultadoInspecao == EResultadoAcr.AceitoComDesvio) && motivoId == null)
                AddNotification(nameof(MotivoId), "O motivo e obrigatorio para rejeicao, quarentena ou aceite com desvio [Origem: AcrResultado]");

            AnaliseId = analiseId;
            ResultadoInspecao = resultadoInspecao;
            MotivoId = motivoId;
            Severidade = severidade;
            QuantidadeAfetada = quantidadeAfetada;
            Justificativa = justificativa;
            AprovadoPor = aprovadoPor;
            StatusFiscalIgnorado = statusFiscalIgnorado;
            DecididoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_acr_motivo — Motivo parametrizado de rejeicao/quarentena/desvio (secao 12.4).</summary>
    public class AcrMotivo : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public string? Categoria { get; private set; }
        public string Severidade { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }

        protected AcrMotivo() { }

        public AcrMotivo(string codigo, string descricao, string severidade, string? categoria,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrMotivo>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O codigo do motivo e obrigatorio [Origem: AcrMotivo]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do motivo e obrigatoria [Origem: AcrMotivo]")
                .IsNotNullOrEmpty(severidade, nameof(Severidade), "A severidade do motivo e obrigatoria [Origem: AcrMotivo]"));
            Codigo = codigo;
            Descricao = descricao;
            Severidade = severidade;
            Categoria = categoria;
            Ativo = true;
        }

        public void Inativar(string usuario)
        {
            Ativo = false;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_acr_documento_fiscal — Vinculo com documento fiscal (secao 12.5).</summary>
    public class AcrDocumentoFiscal : EntidadeSaaSBase
    {
        public Guid? LocalId { get; private set; }
        public string? ChaveFiscalReferencia { get; private set; }
        public string? ChaveFiscalGerada { get; private set; }
        public string? NumeroFiscalReferencia { get; private set; }
        public string? NumeroFiscalGerado { get; private set; }
        public string StatusFiscal { get; private set; } = string.Empty;
        public string? TipoDocumento { get; private set; }
        public decimal? ValorIntegral { get; private set; }
        public decimal? ValorDevolvido { get; private set; }
        public bool DevolucaoParcial { get; private set; }
        public string? MotivoFiscal { get; private set; }
        public string? ObservacaoFiscal { get; private set; }
        public int? SequenciaCce { get; private set; }
        public string? DadosTransporteJson { get; private set; }

        protected AcrDocumentoFiscal() { }

        public AcrDocumentoFiscal(string statusFiscal, bool devolucaoParcial, Guid? localId, string? chaveFiscalReferencia,
            string? chaveFiscalGerada, string? numeroFiscalReferencia, string? numeroFiscalGerado, string? tipoDocumento,
            decimal? valorIntegral, decimal? valorDevolvido, string? motivoFiscal, string? observacaoFiscal,
            int? sequenciaCce, string? dadosTransporteJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrDocumentoFiscal>()
                .Requires()
                .IsNotNullOrEmpty(statusFiscal, nameof(StatusFiscal), "O status fiscal e obrigatorio [Origem: AcrDocumentoFiscal]"));
            StatusFiscal = statusFiscal;
            DevolucaoParcial = devolucaoParcial;
            LocalId = localId;
            ChaveFiscalReferencia = chaveFiscalReferencia;
            ChaveFiscalGerada = chaveFiscalGerada;
            NumeroFiscalReferencia = numeroFiscalReferencia;
            NumeroFiscalGerado = numeroFiscalGerado;
            TipoDocumento = tipoDocumento;
            ValorIntegral = valorIntegral;
            ValorDevolvido = valorDevolvido;
            MotivoFiscal = motivoFiscal;
            ObservacaoFiscal = observacaoFiscal;
            SequenciaCce = sequenciaCce;
            DadosTransporteJson = dadosTransporteJson;
        }
    }

    /// <summary>qld_acr_evento_estoque — Evento operacional de estoque (secao 12.6).</summary>
    public class AcrEventoEstoque : EntidadeSaaSBase
    {
        public Guid ResultadoId { get; private set; }
        public ETipoEventoEstoqueAcr TipoEvento { get; private set; }
        public string? Lote { get; private set; }
        public Guid? LocalOrigemId { get; private set; }
        public Guid? LocalDestinoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public EStatusEventoAcr StatusEvento { get; private set; }
        public string? RetornoJson { get; private set; }

        protected AcrEventoEstoque() { }

        public AcrEventoEstoque(Guid resultadoId, ETipoEventoEstoqueAcr tipoEvento, decimal quantidade, string? lote,
            Guid? localOrigemId, Guid? localDestinoId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrEventoEstoque>()
                .Requires()
                .AreNotEquals(resultadoId, Guid.Empty, nameof(ResultadoId), "O resultado e obrigatorio [Origem: AcrEventoEstoque]"));
            ResultadoId = resultadoId;
            TipoEvento = tipoEvento;
            Quantidade = quantidade;
            Lote = lote;
            LocalOrigemId = localOrigemId;
            LocalDestinoId = localDestinoId;
            StatusEvento = EStatusEventoAcr.Pendente;
        }

        public void RegistrarRetorno(EStatusEventoAcr status, string? retornoJson, string usuario)
        {
            StatusEvento = status;
            RetornoJson = retornoJson;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_acr_evento_ncr — Disparo de NCR a partir do resultado (secao 12.7).</summary>
    public class AcrEventoNcr : EntidadeSaaSBase
    {
        public Guid ResultadoId { get; private set; }
        public Guid? NcrId { get; private set; }
        public EGatilhoNcrAcr Gatilho { get; private set; }
        public EStatusEventoNcrAcr StatusEvento { get; private set; }
        public string? RetornoJson { get; private set; }

        protected AcrEventoNcr() { }

        public AcrEventoNcr(Guid resultadoId, EGatilhoNcrAcr gatilho, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrEventoNcr>()
                .Requires()
                .AreNotEquals(resultadoId, Guid.Empty, nameof(ResultadoId), "O resultado e obrigatorio [Origem: AcrEventoNcr]"));
            ResultadoId = resultadoId;
            Gatilho = gatilho;
            StatusEvento = EStatusEventoNcrAcr.Pendente;
        }

        public void VincularNcr(Guid ncrId, string usuario)
        {
            NcrId = ncrId;
            StatusEvento = EStatusEventoNcrAcr.Criado;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_acr_historico — Trilha auditavel da analise (secao 12.8).</summary>
    public class AcrHistorico : EntidadeSaaSBase
    {
        public Guid AnaliseId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? Ip { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected AcrHistorico() { }

        public AcrHistorico(Guid analiseId, string entidade, Guid entidadeId, EAcaoHistoricoQualidade acao, Guid usuarioId,
            string payloadJson, string? ip, string? motivo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrHistorico>()
                .Requires()
                .AreNotEquals(analiseId, Guid.Empty, nameof(AnaliseId), "A analise e obrigatoria [Origem: AcrHistorico]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade e obrigatoria [Origem: AcrHistorico]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O usuario e obrigatorio [Origem: AcrHistorico]"));
            AnaliseId = analiseId;
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

    /// <summary>qld_acr_anexo — Evidencia documental (secao 12.9).</summary>
    public class AcrAnexo : EntidadeSaaSBase
    {
        public Guid AnaliseId { get; private set; }
        public string? Entidade { get; private set; }
        public Guid? EntidadeId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected AcrAnexo() { }

        public AcrAnexo(Guid analiseId, Guid arquivoId, string? entidade, Guid? entidadeId, string? tipoAnexo,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrAnexo>()
                .Requires()
                .AreNotEquals(analiseId, Guid.Empty, nameof(AnaliseId), "A analise e obrigatoria [Origem: AcrAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: AcrAnexo]"));
            AnaliseId = analiseId;
            ArquivoId = arquivoId;
            Entidade = entidade;
            EntidadeId = entidadeId;
            TipoAnexo = tipoAnexo;
        }
    }

    /// <summary>qld_acr_parametro — Parametros por tenant (secao 12.10).</summary>
    public class AcrParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected AcrParametro() { }

        public AcrParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AcrParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: AcrParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: AcrParametro]"));
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
