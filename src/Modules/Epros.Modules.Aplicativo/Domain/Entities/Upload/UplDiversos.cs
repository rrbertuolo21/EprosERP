using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_configuracao — parâmetros de upload/importação/exportação por tenant (extensões, limites,
    /// fila...). [Origem: EF UPLOAD 12.3]
    /// </summary>
    public class UplConfiguracao : EntidadeSaaSBase
    {
        public string Chave { get; private set; } = string.Empty;
        public string? Valor { get; private set; }
        public bool Ativo { get; private set; }

        protected UplConfiguracao() { }

        public UplConfiguracao(string chave, string? valor, bool ativo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Chave = chave;
            Valor = valor;
            Ativo = ativo;

            AddNotifications(new Contract<UplConfiguracao>()
                .Requires()
                .IsNotNullOrEmpty(chave, nameof(Chave), "A chave do parâmetro é obrigatória [Origem: UplConfiguracao]"));
        }

        public void Alterar(string? valor, bool ativo, string alteradoPor)
        {
            Valor = valor;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_fila_url_remota — fila de download remoto com progresso. Preserva status pending/processing/
    /// downloading/complete/failed/cancelled. [Origem: EF UPLOAD 12.7]
    /// </summary>
    public class UplFilaUrlRemota : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string? ServidorProcessamentoId { get; private set; }
        public EUplStatusUrlRemota StatusJob { get; private set; }
        public long? TamanhoTotal { get; private set; }
        public long? TamanhoBaixado { get; private set; }
        public decimal? PercentualDownload { get; private set; }
        public string? PastaDestinoId { get; private set; }
        public Guid? NovoArquivoId { get; private set; }
        public string? MensagemErro { get; private set; }

        protected UplFilaUrlRemota() { }

        public UplFilaUrlRemota(Guid usuarioId, string url, string? pastaDestinoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            Url = url;
            PastaDestinoId = pastaDestinoId;
            StatusJob = EUplStatusUrlRemota.Pending;

            AddNotifications(new Contract<UplFilaUrlRemota>()
                .Requires()
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O dono da fila é obrigatório [Origem: UplFilaUrlRemota]")
                .IsNotNullOrEmpty(url, nameof(Url), "A URL remota é obrigatória [Origem: UplFilaUrlRemota]"));
        }

        public void AtualizarProgresso(EUplStatusUrlRemota status, long? tamanhoTotal, long? tamanhoBaixado, decimal? percentual, string alteradoPor)
        {
            StatusJob = status;
            TamanhoTotal = tamanhoTotal;
            TamanhoBaixado = tamanhoBaixado;
            PercentualDownload = percentual;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(Guid novoArquivoId, string alteradoPor)
        {
            StatusJob = EUplStatusUrlRemota.Complete;
            NovoArquivoId = novoArquivoId;
            PercentualDownload = 100;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarFalha(string mensagemErro, string alteradoPor)
        {
            StatusJob = EUplStatusUrlRemota.Failed;
            MensagemErro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            StatusJob = EUplStatusUrlRemota.Cancelled;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_migracao_offline — carga pontual de arquivos a partir de pasta controlada, sem mover a origem
    /// quando em modo copiar. [Origem: EF UPLOAD 12.19]
    /// </summary>
    public class UplMigracaoOffline : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string ContaDestino { get; private set; } = string.Empty;
        public string CaminhoOrigem { get; private set; } = string.Empty;
        public string PastaInicialDestino { get; private set; } = string.Empty;
        public string Modo { get; private set; } = "copiar";
        public EUplStatusMigracaoOffline Status { get; private set; }
        public int? ArquivosProcessados { get; private set; }
        public string? MensagemErro { get; private set; }

        protected UplMigracaoOffline() { }

        public UplMigracaoOffline(Guid usuarioId, string contaDestino, string caminhoOrigem, string pastaInicialDestino, string modo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            ContaDestino = contaDestino;
            CaminhoOrigem = caminhoOrigem;
            PastaInicialDestino = pastaInicialDestino;
            Modo = string.IsNullOrWhiteSpace(modo) ? "copiar" : modo;
            Status = EUplStatusMigracaoOffline.Criada;

            AddNotifications(new Contract<UplMigracaoOffline>()
                .Requires()
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O responsável da migração é obrigatório [Origem: UplMigracaoOffline]")
                .IsNotNullOrEmpty(contaDestino, nameof(ContaDestino), "A conta de destino é obrigatória [Origem: UplMigracaoOffline]")
                .IsNotNullOrEmpty(caminhoOrigem, nameof(CaminhoOrigem), "A pasta de origem é obrigatória [Origem: UplMigracaoOffline]")
                .IsNotNullOrEmpty(pastaInicialDestino, nameof(PastaInicialDestino), "A pasta inicial de destino é obrigatória [Origem: UplMigracaoOffline]"));
        }

        public void Processar(string alteradoPor)
        {
            Status = EUplStatusMigracaoOffline.Processando;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(int arquivosProcessados, string alteradoPor)
        {
            Status = EUplStatusMigracaoOffline.Concluida;
            ArquivosProcessados = arquivosProcessados;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarErro(string mensagemErro, string alteradoPor)
        {
            Status = EUplStatusMigracaoOffline.Erro;
            MensagemErro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_historico — auditoria funcional das execuções de upload/importação/exportação. [Origem: EF UPLOAD 12.20]
    /// </summary>
    public class UplHistorico : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public string EntidadeIdReferencia { get; private set; } = string.Empty;
        public string Acao { get; private set; } = string.Empty;
        public Guid? UsuarioId { get; private set; }
        public string? IpOrigem { get; private set; }
        public string? PayloadJson { get; private set; }

        protected UplHistorico() { }

        public UplHistorico(string entidade, string entidadeIdReferencia, string acao, Guid? usuarioId, string? ipOrigem, string? payloadJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeIdReferencia = entidadeIdReferencia;
            Acao = acao;
            UsuarioId = usuarioId;
            IpOrigem = ipOrigem;
            PayloadJson = payloadJson;

            AddNotifications(new Contract<UplHistorico>()
                .Requires()
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade auditada é obrigatória [Origem: UplHistorico]")
                .IsNotNullOrEmpty(entidadeIdReferencia, nameof(EntidadeIdReferencia), "O registro auditado é obrigatório [Origem: UplHistorico]")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação executada é obrigatória [Origem: UplHistorico]"));
        }
    }
}
