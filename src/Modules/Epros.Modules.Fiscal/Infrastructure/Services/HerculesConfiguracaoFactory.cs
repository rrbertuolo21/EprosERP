using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Epros.Modules.Fiscal.Application.Services;

// ISOLAMENTO DO NAMESPACE LEGADO: tipos do Hercules/DFe entram por alias, evitando
// colisão de enums entre Epros.ERP.Shared (legado) e Epros.Shared (novo).
using HerculesConfig = NFe.Utils.ConfiguracaoServico;
using HerculesTipoCertificado = DFe.Utils.TipoCertificado;
using HerculesVersaoServico = DFe.Classes.Flags.VersaoServico;
using HerculesModeloDocumento = DFe.Classes.Flags.ModeloDocumento;
using HerculesTipoEmissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao;
using HerculesTipoAmbiente = DFe.Classes.Flags.TipoAmbiente;
using HerculesEstado = DFe.Classes.Entidades.Estado;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Monta o <c>ConfiguracaoServico</c> do Hercules (usado por <c>ServicosNFe</c>) a partir de
    /// dados NEUTROS do emitente (<see cref="EmitenteFiscalDto"/> + <see cref="CertificadoDigitalDto"/>).
    /// Extrai SÓ a lógica pura de certificado->config do legado <c>ResolveCertificadoDigitaHelper</c>
    /// (validação X509 + atribuição de bytes/senha), SEM dependências de banco, download HTTP,
    /// ContextDfe ou storage de tenant.
    /// </summary>
    public class HerculesConfiguracaoFactory
    {
        public record ResultadoConfiguracao(HerculesConfig? Configuracao, bool Sucesso, string Mensagem);

        /// <summary>
        /// Constrói e configura o <c>ConfiguracaoServico</c> para transmissão.
        /// Valida o certificado (bytes/senha/validade) e aplica UF, ambiente, modelo e schemas.
        /// </summary>
        /// <param name="emitente">Dados do emitente (UF, ambiente é passado à parte).</param>
        /// <param name="modelo">"55" (NF-e) ou "65" (NFC-e).</param>
        /// <param name="ambiente">1=Produção, 2=Homologação.</param>
        /// <param name="tpEmis">
        /// Código tpEmis da SEFAZ (1=Normal; 6=SVC-AN; 7=SVC-RS; 4=EPEC; 9=Offline-NFCe). Default 1 (Normal).
        /// Quando != 1, roteia a transmissão para o webservice de contingência correspondente. NÃO altera
        /// o caminho normal: chamadas sem esse argumento continuam com tpEmis=teNormal.
        /// </param>
        public ResultadoConfiguracao Montar(EmitenteFiscalDto emitente, string modelo, int ambiente, int tpEmis = 1)
        {
            if (emitente is null)
                return new ResultadoConfiguracao(null, false, "Emitente fiscal não informado.");

            if (emitente.Certificado is null || emitente.Certificado.ArrayBytes is null || emitente.Certificado.ArrayBytes.Length == 0)
                return new ResultadoConfiguracao(null, false, "Certificado digital não configurado para o emitente.");

            if (!Enum.TryParse<HerculesEstado>(emitente.Uf, ignoreCase: true, out var estado))
                return new ResultadoConfiguracao(null, false, $"UF do emitente inválida: [{emitente.Uf}].");

            var validacaoCert = ValidarCertificado(emitente.Certificado.ArrayBytes, emitente.Certificado.Senha);
            if (!validacaoCert.Sucesso)
                return new ResultadoConfiguracao(null, false, validacaoCert.Mensagem);

            var modeloDoc = modelo == "65" ? HerculesModeloDocumento.NFCe : HerculesModeloDocumento.NFe;
            var tpAmb = ambiente == 1 ? HerculesTipoAmbiente.Producao : HerculesTipoAmbiente.Homologacao;

            // Equivale ao EstanciarConfig()/ConfigurarServicoAutomatico() do VendaNfeService legado.
            var cfg = HerculesConfig.Instancia;
            cfg.VersaoLayout = HerculesVersaoServico.Versao400;
            cfg.DefineVersaoServicosAutomaticamente = true;

            var diretorio = Directory.GetCurrentDirectory();
            cfg.DiretorioSchemas = Path.Combine(diretorio, "wwwroot", "Schemas", "400");
            cfg.ModeloDocumento = modeloDoc;
            cfg.cUF = estado;
            // tpEmis: os valores do enum Hercules seguem o código oficial da SEFAZ (1=Normal, 6=SVC-AN,
            // 7=SVC-RS, 4=EPEC, 9=Offline). Cast por inteiro; valor não definido cai em Normal (seguro).
            cfg.tpEmis = Enum.IsDefined(typeof(HerculesTipoEmissao), tpEmis)
                ? (HerculesTipoEmissao)tpEmis
                : HerculesTipoEmissao.teNormal;
            cfg.tpAmb = tpAmb;
            cfg.TimeOut = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
            cfg.ProtocoloDeSeguranca = ServicePointManager.SecurityProtocol;
            cfg.RemoverAcentos = true;

            cfg.Certificado.TipoCertificado = HerculesTipoCertificado.A1ByteArray;
            cfg.Certificado.ArrayBytesArquivo = emitente.Certificado.ArrayBytes;
            cfg.Certificado.Senha = emitente.Certificado.Senha;

            // Mensagem de aviso (perto de vencer) sobe junto, mas não impede a transmissão.
            return new ResultadoConfiguracao(cfg, true, validacaoCert.Mensagem);
        }

        /// <summary>
        /// Valida o certificado A1 (carrega X509, checa vencimento). Portado de
        /// <c>ResolveCertificadoDigitaHelper.CertificadoEstaOk</c> — versão pura, só bytes+senha.
        /// </summary>
        public ResultadoConfiguracao ValidarCertificado(byte[] certificadoBytes, string senha)
        {
            try
            {
                var x509 = new X509Certificate2(
                    certificadoBytes,
                    senha,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                DateTime.TryParse(x509.GetExpirationDateString(), out DateTime dataExpiracao);

                if (dataExpiracao > DateTime.MinValue && DateTime.Today > dataExpiracao)
                    return new ResultadoConfiguracao(null, false, $"Certificado digital vencido em: [{dataExpiracao:dd/MM/yyyy HH:mm}].");

                if (dataExpiracao > DateTime.MinValue && dataExpiracao.Date.Subtract(DateTime.Today.Date).TotalDays <= 30)
                {
                    var dias = Math.Round(dataExpiracao.Date.Subtract(DateTime.Today.Date).TotalDays);
                    return new ResultadoConfiguracao(null, true, $"Certificado digital perto de vencer: restam {dias} dia(s) - vencimento em [{dataExpiracao:dd/MM/yyyy HH:mm}].");
                }

                return new ResultadoConfiguracao(null, true, string.Empty);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("senha", StringComparison.OrdinalIgnoreCase))
                    return new ResultadoConfiguracao(null, false, "A senha do certificado digital informada é inválida.");

                return new ResultadoConfiguracao(null, false, $"Certificado Digital: erro inesperado - {ex.Message}");
            }
        }
    }
}
