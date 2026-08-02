using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    /// <summary>qld_rst_origem — Origem da campanha (NCR, reclamacao, auditoria...).</summary>
    public class RstOrigem : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public string TipoOrigem { get; private set; } = string.Empty;
        public string? ReferenciaId { get; private set; }
        public string? Observacao { get; private set; }

        protected RstOrigem() { }

        public RstOrigem(Guid campanhaId, string tipoOrigem, string? referenciaId, string? observacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstOrigem>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstOrigem]")
                .IsNotNullOrEmpty(tipoOrigem, nameof(TipoOrigem), "O tipo de origem e obrigatorio [Origem: RstOrigem]"));
            CampanhaId = campanhaId;
            TipoOrigem = tipoOrigem;
            ReferenciaId = referenciaId;
            Observacao = observacao;
        }
    }

    /// <summary>qld_rst_item_afetado — Item (lote/serie) no escopo da campanha.</summary>
    public class RstItemAfetado : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public string? Lote { get; private set; }
        public string? Serial { get; private set; }
        public decimal Quantidade { get; private set; }
        public string? Localizacao { get; private set; }

        protected RstItemAfetado() { }

        public RstItemAfetado(Guid campanhaId, decimal quantidade, Guid? produtoId, string? lote, string? serial,
            string? localizacao, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstItemAfetado>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstItemAfetado]")
                .IsGreaterOrEqualsThan(quantidade, 0, nameof(Quantidade), "A quantidade nao pode ser negativa [Origem: RstItemAfetado]"));
            CampanhaId = campanhaId;
            Quantidade = quantidade;
            ProdutoId = produtoId;
            Lote = lote;
            Serial = serial;
            Localizacao = localizacao;
        }
    }

    /// <summary>
    /// qld_rst_genealogia_no — No da arvore de genealogia (MP->WIP->PA). Leitura por contrato com
    /// Estoque/Producao (D6/D24): guarda a referencia, nao duplica saldo. Lacuna = genealogia
    /// incompleta registrada (RN-RST-011); segue so com justificativa.
    /// </summary>
    public class RstGenealogiaNo : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public Guid? PaiId { get; private set; }
        public ERstTipoNoGenealogia TipoNo { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public string? Lote { get; private set; }
        public string? Serial { get; private set; }
        public int Nivel { get; private set; }
        public bool Lacuna { get; private set; }
        public string? Justificativa { get; private set; }

        protected RstGenealogiaNo() { }

        public RstGenealogiaNo(Guid campanhaId, ERstTipoNoGenealogia tipoNo, int nivel, Guid? paiId, Guid? produtoId,
            string? lote, string? serial, bool lacuna, string? justificativa, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstGenealogiaNo>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstGenealogiaNo]")
                .IsGreaterOrEqualsThan(nivel, 0, nameof(Nivel), "O nivel nao pode ser negativo [Origem: RstGenealogiaNo]"));

            // RN-RST-011: lacuna exige justificativa para seguir.
            if (lacuna && string.IsNullOrWhiteSpace(justificativa))
                AddNotification(nameof(Justificativa), "Lacuna de genealogia exige justificativa [Origem: RstGenealogiaNo]");

            CampanhaId = campanhaId;
            TipoNo = tipoNo;
            Nivel = nivel;
            PaiId = paiId;
            ProdutoId = produtoId;
            Lote = lote;
            Serial = serial;
            Lacuna = lacuna;
            Justificativa = justificativa;
        }
    }

    /// <summary>qld_rst_bloqueio — Contencao: solicita bloqueio de lote/serie ao Estoque (nao movimenta saldo).</summary>
    public class RstBloqueio : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public string? Lote { get; private set; }
        public string? Serial { get; private set; }
        public decimal Quantidade { get; private set; }
        public bool Ativo { get; private set; }
        public string? Motivo { get; private set; }

        protected RstBloqueio() { }

        public RstBloqueio(Guid campanhaId, decimal quantidade, string? lote, string? serial, string? motivo,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstBloqueio>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstBloqueio]"));
            CampanhaId = campanhaId;
            Quantidade = quantidade;
            Lote = lote;
            Serial = serial;
            Motivo = motivo;
            Ativo = true;
        }

        public void Liberar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }

    /// <summary>qld_rst_comunicacao — Comunicacao a cliente/autoridade (conteudo aprovado — RN-RST-009; ⚠️ valida D16).</summary>
    public class RstComunicacao : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public ERstCanalComunicacao Canal { get; private set; }
        public string Conteudo { get; private set; } = string.Empty;
        public ERstStatusComunicacao Status { get; private set; }
        public Guid? AprovadoPor { get; private set; }
        public DateTime? EnviadoEm { get; private set; }

        protected RstComunicacao() { }

        public RstComunicacao(Guid campanhaId, ERstCanalComunicacao canal, string conteudo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstComunicacao>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstComunicacao]")
                .IsNotNullOrEmpty(conteudo, nameof(Conteudo), "O conteudo da comunicacao e obrigatorio [Origem: RstComunicacao]"));
            CampanhaId = campanhaId;
            Canal = canal;
            Conteudo = conteudo;
            Status = ERstStatusComunicacao.Rascunho;
        }

        public void Aprovar(Guid aprovadoPor, string usuario)
        {
            AprovadoPor = aprovadoPor;
            Status = ERstStatusComunicacao.Aprovada;
            MarcarAlterado(usuario);
        }

        public void RegistrarEnvio(string usuario)
        {
            if (Status != ERstStatusComunicacao.Aprovada)
            {
                AddNotification(nameof(Status), "A comunicacao precisa ser aprovada antes do envio (RN-RST-009) [Origem: RstComunicacao]");
                return;
            }
            Status = ERstStatusComunicacao.Enviada;
            EnviadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_rst_recolhimento — Recolhimento fisico dos itens afetados.</summary>
    public class RstRecolhimento : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public decimal QuantidadePrevista { get; private set; }
        public decimal QuantidadeRecolhida { get; private set; }
        public ERstStatusRecolhimento Status { get; private set; }

        protected RstRecolhimento() { }

        public RstRecolhimento(Guid campanhaId, decimal quantidadePrevista, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstRecolhimento>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstRecolhimento]")
                .IsGreaterOrEqualsThan(quantidadePrevista, 0, nameof(QuantidadePrevista), "A quantidade prevista nao pode ser negativa [Origem: RstRecolhimento]"));
            CampanhaId = campanhaId;
            QuantidadePrevista = quantidadePrevista;
            QuantidadeRecolhida = 0m;
            Status = ERstStatusRecolhimento.Pendente;
        }

        public void RegistrarRecolhimento(decimal quantidade, string usuario)
        {
            if (quantidade < 0) { AddNotification(nameof(QuantidadeRecolhida), "Quantidade invalida [Origem: RstRecolhimento]"); return; }
            QuantidadeRecolhida += quantidade;
            Status = QuantidadeRecolhida >= QuantidadePrevista ? ERstStatusRecolhimento.Concluido : ERstStatusRecolhimento.EmAndamento;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>qld_rst_disposicao — Destino dos itens recolhidos (retrabalho/descarte/devolucao...).</summary>
    public class RstDisposicao : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public ERstTipoDisposicao TipoDisposicao { get; private set; }
        public decimal Quantidade { get; private set; }
        public string? Observacao { get; private set; }

        protected RstDisposicao() { }

        public RstDisposicao(Guid campanhaId, ERstTipoDisposicao tipoDisposicao, decimal quantidade, string? observacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstDisposicao>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstDisposicao]")
                .IsGreaterOrEqualsThan(quantidade, 0, nameof(Quantidade), "A quantidade nao pode ser negativa [Origem: RstDisposicao]"));
            CampanhaId = campanhaId;
            TipoDisposicao = tipoDisposicao;
            Quantidade = quantidade;
            Observacao = observacao;
        }
    }

    /// <summary>qld_rst_anexo — Evidencia/dossie da campanha.</summary>
    public class RstAnexo : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected RstAnexo() { }

        public RstAnexo(Guid campanhaId, Guid arquivoId, string? tipoAnexo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstAnexo>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstAnexo]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo e obrigatorio [Origem: RstAnexo]"));
            CampanhaId = campanhaId;
            ArquivoId = arquivoId;
            TipoAnexo = tipoAnexo;
        }
    }

    /// <summary>qld_rst_historico — Trilha auditavel da campanha.</summary>
    public class RstHistorico : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public EAcaoHistoricoQualidade Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string PayloadJson { get; private set; } = "{}";
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected RstHistorico() { }

        public RstHistorico(Guid campanhaId, string entidade, EAcaoHistoricoQualidade acao, Guid usuarioId,
            string payloadJson, string? motivo, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstHistorico>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstHistorico]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade e obrigatoria [Origem: RstHistorico]"));
            CampanhaId = campanhaId;
            Entidade = entidade;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson ?? "{}";
            Motivo = motivo;
            OcorridoEm = DateTime.UtcNow;
        }
    }

    /// <summary>qld_rst_parametro — Parametros por tenant (criterios de recall/comunicacao — ⚠️ valida D16).</summary>
    public class RstParametro : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string ValorJson { get; private set; } = "{}";
        public bool Ativo { get; private set; }

        protected RstParametro() { }

        public RstParametro(string chave, string valorJson, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstParametro>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parametro e obrigatoria [Origem: RstParametro]")
                .IsNotNullOrEmpty(valorJson, nameof(ValorJson), "O valor do parametro e obrigatorio [Origem: RstParametro]"));
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

    /// <summary>qld_rst_evento — Eventos funcionais emitidos/recebidos da campanha.</summary>
    public class RstEvento : EntidadeSaaSBase
    {
        public Guid CampanhaId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public EDirecaoEvento Direcao { get; private set; }

        protected RstEvento() { }

        public RstEvento(Guid campanhaId, string tipoEvento, EDirecaoEvento direcao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RstEvento>()
                .Requires()
                .AreNotEquals(campanhaId, Guid.Empty, nameof(CampanhaId), "A campanha e obrigatoria [Origem: RstEvento]")
                .IsNotNullOrEmpty(tipoEvento, nameof(TipoEvento), "O tipo de evento e obrigatorio [Origem: RstEvento]"));
            CampanhaId = campanhaId;
            TipoEvento = tipoEvento;
            Direcao = direcao;
        }
    }
}
