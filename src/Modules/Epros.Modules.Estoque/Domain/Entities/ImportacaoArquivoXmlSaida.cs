using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Registro do processamento em lote de arquivos XML de saída (contagem de XMLs/produtos/clientes).
    /// Porte fiel do legado Epros.ERP.Domain.Entities.Importacoes.ImportacaoArquivoXmlSaida.
    /// </summary>
    public class ImportacaoArquivoXmlSaida : EntidadeSaaSBase
    {
        public string NomeArquivo { get; private set; } = string.Empty;
        public int QtdXmls { get; private set; }
        public int QtdXmlsInvalidos { get; private set; }
        public int QtdProdutosLocalizados { get; private set; }
        public int QtdClientesLocalizados { get; private set; }
        public int QtdProdutosImportados { get; private set; }
        public int QtdClientesImportados { get; private set; }
        public string MensagemErro { get; private set; } = string.Empty;
        public EImportacaoArquivoXmlSaidaStatus Status { get; private set; }

        protected ImportacaoArquivoXmlSaida() { } // EF Core

        public ImportacaoArquivoXmlSaida(string nomeArquivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeArquivo = nomeArquivo;
            Status = EImportacaoArquivoXmlSaidaStatus.Verificando;
        }

        public void Processar() => Status = EImportacaoArquivoXmlSaidaStatus.Processando;

        public void Finalizar(int qtdXmls, int qtdXmlsInvalidos, int qtdProdutosLocalizados, int qtdClientesLocalizados, int qtdProdutosImportados, int qtdClientesImportados)
        {
            QtdXmls = qtdXmls;
            QtdXmlsInvalidos = qtdXmlsInvalidos;
            QtdProdutosLocalizados = qtdProdutosLocalizados;
            QtdClientesLocalizados = qtdClientesLocalizados;
            QtdProdutosImportados = qtdProdutosImportados;
            QtdClientesImportados = qtdClientesImportados;
            Status = EImportacaoArquivoXmlSaidaStatus.Finalizado;
        }

        public void Errar(string mensagemErro)
        {
            MensagemErro = (string.IsNullOrEmpty(mensagemErro) ? "" : (mensagemErro.Length > 500 ? mensagemErro[0..500] : mensagemErro));
            Status = EImportacaoArquivoXmlSaidaStatus.Erro;
        }
    }
}
