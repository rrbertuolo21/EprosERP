using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Upload
{
    /// <summary>
    /// upl_importacao_xml — staging e controle de XML fiscal com status separados de importação, cadastro
    /// e PDF. [Origem: EF UPLOAD 12.12]
    /// </summary>
    public class UplImportacaoXml : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public string Xml { get; private set; } = string.Empty;
        public EUplTipoXml TipoDeXml { get; private set; }
        public string NfeId { get; private set; } = string.Empty;
        public EUplStatusProcessamentoXml StatusImportacaoXml { get; private set; }
        public string? MensagemErroImportacaoXml { get; private set; }
        public EUplStatusProcessamentoXml StatusCadastro { get; private set; }
        public string? MensagemErroCadastro { get; private set; }
        public EUplStatusProcessamentoXml StatusSalvarPdf { get; private set; }
        public string? MensagemErroSalvarPdf { get; private set; }
        public int CodigoSefaz { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;

        protected UplImportacaoXml() { }

        public UplImportacaoXml(
            Guid? empresaId,
            string xml,
            EUplTipoXml tipoDeXml,
            string nfeId,
            int codigoSefaz,
            string tipoEvento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Xml = xml;
            TipoDeXml = tipoDeXml;
            NfeId = nfeId;
            CodigoSefaz = codigoSefaz;
            TipoEvento = tipoEvento;
            StatusImportacaoXml = EUplStatusProcessamentoXml.NaoProcessado;
            StatusCadastro = EUplStatusProcessamentoXml.NaoProcessado;
            StatusSalvarPdf = EUplStatusProcessamentoXml.NaoProcessado;

            AddNotifications(new Contract<UplImportacaoXml>()
                .Requires()
                .IsNotNullOrEmpty(xml, nameof(Xml), "O conteúdo do XML é obrigatório [Origem: UplImportacaoXml]")
                .IsNotNullOrEmpty(nfeId, nameof(NfeId), "O identificador fiscal (nfe_id) é obrigatório [Origem: UplImportacaoXml]"));
        }

        public void AtualizarStatusImportacao(EUplStatusProcessamentoXml status, string? mensagemErro, string alteradoPor)
        {
            StatusImportacaoXml = status;
            MensagemErroImportacaoXml = mensagemErro;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarStatusCadastro(EUplStatusProcessamentoXml status, string? mensagemErro, string alteradoPor)
        {
            StatusCadastro = status;
            MensagemErroCadastro = mensagemErro;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarStatusPdf(EUplStatusProcessamentoXml status, string? mensagemErro, string alteradoPor)
        {
            StatusSalvarPdf = status;
            MensagemErroSalvarPdf = mensagemErro;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// upl_arquivo_xml_saida — resumo de um lote de XML de saída. Todos os contadores são obrigatórios
    /// no material. [Origem: EF UPLOAD 12.13]
    /// </summary>
    public class UplArquivoXmlSaida : EntidadeSaaSBase
    {
        public string NomeArquivo { get; private set; } = string.Empty;
        public int QtdXmls { get; private set; }
        public int QtdXmlsInvalidos { get; private set; }
        public int QtdProdutosLocalizados { get; private set; }
        public int QtdClientesLocalizados { get; private set; }
        public int QtdProdutosImportados { get; private set; }
        public int QtdClientesImportados { get; private set; }
        public string MensagemErro { get; private set; } = string.Empty;
        public EUplStatusArquivoXmlSaida Status { get; private set; }

        protected UplArquivoXmlSaida() { }

        public UplArquivoXmlSaida(string nomeArquivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeArquivo = nomeArquivo;
            Status = EUplStatusArquivoXmlSaida.Verificando;
            MensagemErro = string.Empty;

            AddNotifications(new Contract<UplArquivoXmlSaida>()
                .Requires()
                .IsNotNullOrEmpty(nomeArquivo, nameof(NomeArquivo), "O nome do arquivo de lote é obrigatório [Origem: UplArquivoXmlSaida]"));
        }

        public void AtualizarContadores(int qtdXmls, int qtdInvalidos, int prodLocalizados, int cliLocalizados, int prodImportados, int cliImportados, string alteradoPor)
        {
            QtdXmls = qtdXmls;
            QtdXmlsInvalidos = qtdInvalidos;
            QtdProdutosLocalizados = prodLocalizados;
            QtdClientesLocalizados = cliLocalizados;
            QtdProdutosImportados = prodImportados;
            QtdClientesImportados = cliImportados;
            MarcarAlterado(alteradoPor);
        }

        public void Finalizar(EUplStatusArquivoXmlSaida status, string mensagemErro, string alteradoPor)
        {
            Status = status;
            MensagemErro = mensagemErro ?? string.Empty;
            MarcarAlterado(alteradoPor);
        }
    }
}
