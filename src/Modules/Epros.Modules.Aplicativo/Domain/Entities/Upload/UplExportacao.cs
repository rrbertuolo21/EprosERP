using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_execucao_exportacao — geração de arquivo de exportação (XLSX) com filtros atuais da listagem.
    /// Não grava dados de negócio. [Origem: EF UPLOAD 12.14]
    /// </summary>
    public class UplExecucaoExportacao : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string Entidade { get; private set; } = string.Empty;
        public string? FiltrosJson { get; private set; }
        public EUplStatusExportacao Status { get; private set; }
        public Guid? ArquivoId { get; private set; }
        public string? UrlDownload { get; private set; }
        public string? MensagemErro { get; private set; }

        protected UplExecucaoExportacao() { }

        public UplExecucaoExportacao(Guid usuarioId, string entidade, string? filtrosJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            Entidade = entidade;
            FiltrosJson = filtrosJson;
            Status = EUplStatusExportacao.Solicitada;

            AddNotifications(new Contract<UplExecucaoExportacao>()
                .Requires()
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O solicitante da exportação é obrigatório [Origem: UplExecucaoExportacao]")
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "A entidade exportada é obrigatória [Origem: UplExecucaoExportacao]"));
        }

        public void Disponibilizar(Guid arquivoId, string urlDownload, string alteradoPor)
        {
            ArquivoId = arquivoId;
            UrlDownload = urlDownload;
            Status = EUplStatusExportacao.Disponivel;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarErro(string mensagemErro, string alteradoPor)
        {
            Status = EUplStatusExportacao.Erro;
            MensagemErro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_exportacao_campo — campos padrão/customizados escolhidos para a exportação. [Origem: EF UPLOAD 12.15]
    /// </summary>
    public class UplExportacaoCampo : EntidadeSaaSBase
    {
        public Guid ExecucaoExportacaoId { get; private set; }
        public EUplOrigemCampo OrigemCampo { get; private set; }
        public string ChaveCampo { get; private set; } = string.Empty;
        public string? Rotulo { get; private set; }
        public int? Ordem { get; private set; }

        protected UplExportacaoCampo() { }

        public UplExportacaoCampo(Guid execucaoExportacaoId, EUplOrigemCampo origemCampo, string chaveCampo, string? rotulo, int? ordem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ExecucaoExportacaoId = execucaoExportacaoId;
            OrigemCampo = origemCampo;
            ChaveCampo = chaveCampo;
            Rotulo = rotulo;
            Ordem = ordem;

            AddNotifications(new Contract<UplExportacaoCampo>()
                .Requires()
                .AreNotEquals(execucaoExportacaoId, Guid.Empty, nameof(ExecucaoExportacaoId), "A exportação do campo é obrigatória [Origem: UplExportacaoCampo]")
                .IsNotNullOrEmpty(chaveCampo, nameof(ChaveCampo), "O campo escolhido é obrigatório [Origem: UplExportacaoCampo]"));
        }
    }
}
