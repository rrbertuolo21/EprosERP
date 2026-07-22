using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;

// ISOLAMENTO DO NAMESPACE LEGADO por alias (evita colisão de enums Epros.ERP.Shared vs Epros.Shared).
using MotorGeraXml = Epros.ERP.DfeCalculos.Models.GeraXmlDfe;
using MotorVenda = Epros.ERP.DfeCalculos.Models.Vendas.Venda;
using HerculesServicos = NFe.Servicos.ServicosNFe;
using HerculesIndicadorSincronizacao = NFe.Classes.Servicos.Tipos.IndicadorSincronizacao;
using HerculesNfeClasse = NFe.Classes.NFe;
using HerculesNfeProc = NFe.Classes.nfeProc;
using HerculesModeloDocumento = DFe.Classes.Flags.ModeloDocumento;
using NFe.Utils.NFe;        // ObterXmlString / Valida / Assina (extension methods)

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Adapter de TRANSMISSÃO SEFAZ real. Substitui o antigo stub <c>HerculesFiscalService</c>.
    /// Reusa o motor legado (Epros.ERP.DfeCalculos): monta o modelo <c>Venda</c> via
    /// <see cref="DocumentoFiscalVendaMapper"/>, gera o objeto NFe com <c>GeraXmlDfe.ObterNf</c>
    /// e transmite com o <c>ServicosNFe</c> do Hercules (autorização, cancelamento, CC-e, inutilização).
    ///
    /// FALLBACK CONTROLADO: quando não há emitente/certificado configurado, NÃO simula autorização
    /// (nunca gera chave/protocolo fake). Retorna Sucesso=false com StatusSefaz=0 e motivo claro,
    /// OU, se <c>PermitirModoSimuladoDev</c> estiver ligado por config, propaga uma exceção explícita.
    /// </summary>
    public class MotorLegadoFiscalService : IHerculesFiscalService
    {
        private readonly IEmitenteFiscalProvider _emitenteProvider;
        private readonly HerculesConfiguracaoFactory _configFactory;

        public MotorLegadoFiscalService(
            IEmitenteFiscalProvider emitenteProvider,
            HerculesConfiguracaoFactory configFactory)
        {
            _emitenteProvider = emitenteProvider;
            _configFactory = configFactory;
        }

        public Task<RetornoEmissaoDto> EmitirAsync(DocumentoFiscal documento)
        {
            // 1. Resolver emitente + certificado. Sem isso, degradar de forma controlada.
            var contexto = _emitenteProvider.ObterContexto(documento);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(FalhaEmissao(
                    "Emitente/certificado fiscal não configurado. Configure a empresa emitente e o certificado A1 antes de transmitir. (Modo simulado desativado por segurança — nenhuma chave/protocolo é gerado.)"));
            }

            // tpEmis do documento (Normal por padrão): em contingência SVC-AN/SVC-RS/EPEC roteia a
            // transmissão para o webservice de contingência. O XML (ide.tpEmis/xJust/dhCont) já é montado
            // pelo mapper a partir da contingência do documento.
            var resultadoCfg = _configFactory.Montar(contexto.Emitente, documento.Modelo, documento.Ambiente, (int)documento.TipoEmissao);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
                return Task.FromResult(FalhaEmissao(resultadoCfg.Mensagem));

            // 2. Montar Venda (motor) -> objeto NFe.
            string xmlEnvio = string.Empty;
            HerculesNfeClasse nfe;
            try
            {
                var venda = DocumentoFiscalVendaMapper.Mapear(documento, contexto);
                if (venda.Notifications.Count > 0)
                {
                    var erros = string.Join(" | ", venda.Notifications.Select(n => n.Message));
                    return Task.FromResult(FalhaEmissao($"Dados inválidos para gerar o XML: {erros}"));
                }

                nfe = MotorGeraXml.ObterNf(venda, resultadoCfg.Configuracao.VersaoNfeStatusServico);
                xmlEnvio = nfe.ObterXmlString();
                nfe.Valida(); // valida contra o schema XSD
            }
            catch (Exception ex)
            {
                return Task.FromResult(FalhaEmissao($"Falha ao gerar/validar o XML da NF-e: {ex.Message}"));
            }

            // 3. Transmitir (autorização síncrona) e tratar retorno.
            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var retorno = servico.NFeAutorizacao(
                    1,
                    HerculesIndicadorSincronizacao.Sincrono,
                    new List<HerculesNfeClasse> { nfe },
                    false);

                var recibo = retorno.Retorno?.infRec?.nRec ?? string.Empty;
                var prot = retorno.Retorno?.protNFe?.infProt;

                // cStat 104 = "Lote processado"; a autorização real está no protNFe (100 = autorizado).
                if (retorno.Retorno?.cStat == 104 && prot?.cStat == 100)
                {
                    var nfeProc = new HerculesNfeProc
                    {
                        NFe = nfe,
                        protNFe = retorno.Retorno.protNFe,
                        versao = retorno.Retorno.versao
                    };

                    var xmlRetorno = nfeProc.ObterXmlString();

                    return Task.FromResult(new RetornoEmissaoDto
                    {
                        Sucesso = true,
                        ChaveAcesso = prot.chNFe,
                        Protocolo = prot.nProt,
                        Recibo = recibo,
                        StatusSefaz = 100,
                        Motivo = prot.xMotivo,
                        XmlEnvio = xmlEnvio,
                        XmlRetorno = xmlRetorno,
                        // TODO(DANFE): geração de PDF DANFE via FastReport fica para a próxima leva.
                        // TODO(MinIO): persistência dos XMLs/PDF em storage fica para a próxima leva
                        //              (por ora o XML vai como string em XmlEnvio/XmlRetorno).
                        PdfCaminho = string.Empty,
                        XmlCaminho = string.Empty
                    });
                }

                // Rejeição: reporta o motivo real da SEFAZ (protNFe se houver, senão o do lote).
                var cStat = prot?.cStat ?? retorno.Retorno?.cStat ?? 0;
                var motivo = prot?.xMotivo ?? retorno.Retorno?.xMotivo ?? "Rejeição não detalhada pela SEFAZ.";

                return Task.FromResult(new RetornoEmissaoDto
                {
                    Sucesso = false,
                    StatusSefaz = cStat,
                    Motivo = $"[{cStat}] {motivo}",
                    Recibo = recibo,
                    XmlEnvio = xmlEnvio,
                    XmlRetorno = retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoEmissaoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ: {ex.Message}",
                    XmlEnvio = xmlEnvio,
                    XmlRetorno = string.Empty
                });
            }
        }

        public Task<RetornoCancelamentoDto> CancelarAsync(DocumentoFiscal documento, string justificativa)
        {
            var contexto = _emitenteProvider.ObterContexto(documento);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Emitente/certificado fiscal não configurado para cancelamento."
                });
            }

            var resultadoCfg = _configFactory.Montar(contexto.Emitente, documento.Modelo, documento.Ambiente);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
            {
                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = resultadoCfg.Mensagem
                });
            }

            if (string.IsNullOrWhiteSpace(documento.ChaveAcesso) || string.IsNullOrWhiteSpace(documento.Protocolo))
            {
                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Documento sem chave de acesso/protocolo de autorização — não é possível cancelar."
                });
            }

            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var retorno = servico.RecepcaoEventoCancelamento(
                    1,
                    1,
                    documento.Protocolo!,
                    documento.ChaveAcesso,
                    justificativa,
                    contexto.Emitente.Documento);

                var evento = retorno.Retorno?.retEvento?.FirstOrDefault();
                var cStat = evento?.infEvento?.cStat ?? retorno.Retorno?.cStat ?? 0;
                var motivo = evento?.infEvento?.xMotivo ?? retorno.Retorno?.xMotivo ?? "Retorno de cancelamento não detalhado.";
                var protocolo = evento?.infEvento?.nProt ?? string.Empty;

                // 135 = homologado o cancelamento; 155 = cancelamento fora de prazo homologado.
                var sucesso = cStat == 135 || cStat == 155;

                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = sucesso,
                    StatusSefaz = cStat,
                    Motivo = motivo,
                    Protocolo = protocolo,
                    XmlRetorno = retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ no cancelamento: {ex.Message}"
                });
            }
        }

        public Task<RetornoEventoDto> CartaCorrecaoAsync(DocumentoFiscal documento, string textoCorrecao, int sequenciaEvento)
        {
            var contexto = _emitenteProvider.ObterContexto(documento);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Emitente/certificado fiscal não configurado para carta de correção."
                });
            }

            var resultadoCfg = _configFactory.Montar(contexto.Emitente, documento.Modelo, documento.Ambiente);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
            {
                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = resultadoCfg.Mensagem
                });
            }

            if (string.IsNullOrWhiteSpace(documento.ChaveAcesso))
            {
                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Documento sem chave de acesso — não é possível enviar carta de correção."
                });
            }

            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var retorno = servico.RecepcaoEventoCartaCorrecao(
                    1,
                    sequenciaEvento,
                    documento.ChaveAcesso,
                    textoCorrecao,
                    contexto.Emitente.Documento);

                var cStat = retorno.Retorno?.cStat ?? 0;
                var motivo = retorno.Retorno?.xMotivo ?? "Retorno de carta de correção não detalhado.";

                // 128 = lote de evento processado; 135 = evento registrado e vinculado à NF-e.
                var sucesso = cStat == 128 || cStat == 135;

                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = sucesso,
                    StatusSefaz = cStat,
                    Motivo = motivo,
                    Protocolo = string.Empty,
                    XmlRetorno = retorno.RetornoCompletoStr ?? retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ na carta de correção: {ex.Message}"
                });
            }
        }

        public Task<RetornoInutilizacaoDto> InutilizarAsync(InutilizacaoFiscalRequest request)
        {
            // Inutilização precisa de um emitente. Reusa o provider por meio de um DocumentoFiscal
            // "sonda" só com os dados fiscais mínimos (modelo/ambiente).
            var sonda = new DocumentoFiscal(
                request.Modelo,
                request.Ambiente,
                request.Serie <= 0 ? 1 : request.Serie,
                request.NumeroInicial <= 0 ? 1 : request.NumeroInicial,
                0m,
                "00000000000",
                "INUTILIZACAO",
                "system",
                "system");

            var contexto = _emitenteProvider.ObterContexto(sonda);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(new RetornoInutilizacaoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Emitente/certificado fiscal não configurado para inutilização."
                });
            }

            var resultadoCfg = _configFactory.Montar(contexto.Emitente, request.Modelo, request.Ambiente);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
            {
                return Task.FromResult(new RetornoInutilizacaoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = resultadoCfg.Mensagem
                });
            }

            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var modeloDoc = request.Modelo == "65" ? HerculesModeloDocumento.NFCe : HerculesModeloDocumento.NFe;

                var retorno = servico.NfeInutilizacao(
                    contexto.Emitente.Documento,
                    request.Ano,
                    modeloDoc,
                    request.Serie,
                    (int)request.NumeroInicial,
                    (int)request.NumeroFinal,
                    request.Justificativa);

                var inf = retorno.Retorno?.infInut;
                var cStat = inf?.cStat ?? 0;
                var motivo = inf?.xMotivo ?? "Retorno de inutilização não detalhado.";

                // 102 = inutilização de número homologada.
                var sucesso = cStat == 102;

                return Task.FromResult(new RetornoInutilizacaoDto
                {
                    Sucesso = sucesso,
                    StatusSefaz = cStat,
                    Motivo = motivo,
                    Protocolo = inf?.nProt ?? string.Empty,
                    XmlRetorno = retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoInutilizacaoDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ na inutilização: {ex.Message}"
                });
            }
        }

        public Task<RetornoConsultaSefazDto> VerificarStatusServicoAsync(ConsultaStatusServicoRequest request)
        {
            // Resolve emitente/certificado pelo documento (CNPJ/CPF) do emitente informado.
            var contexto = _emitenteProvider.ObterContextoPorDocumento(request.Documento, request.Modelo, request.Ambiente);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Emitente/certificado fiscal não configurado para consulta de status."
                });
            }

            var resultadoCfg = _configFactory.Montar(contexto.Emitente, request.Modelo, request.Ambiente);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
                return Task.FromResult(new RetornoConsultaSefazDto { Sucesso = false, StatusSefaz = 0, Motivo = resultadoCfg.Mensagem });

            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var retorno = servico.NfeStatusServico();

                var cStat = retorno.Retorno?.cStat ?? 0;
                var motivo = retorno.Retorno?.xMotivo ?? "Retorno de status não detalhado.";

                // 107 = Serviço em operação.
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = cStat == 107,
                    StatusSefaz = cStat,
                    Motivo = motivo,
                    XmlRetorno = retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ na verificação de status: {ex.Message}"
                });
            }
        }

        public Task<RetornoConsultaSefazDto> ConsultarProtocoloAsync(ConsultaProtocoloRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Chave) || request.Chave.Length != 44)
                return Task.FromResult(new RetornoConsultaSefazDto { Sucesso = false, StatusSefaz = 0, Motivo = "Chave de acesso inválida (deve ter 44 dígitos)." });

            // O documento do emitente sai da própria chave (posições 6..19 = CNPJ).
            var documentoEmitente = request.Chave.Substring(6, 14);

            var contexto = _emitenteProvider.ObterContextoPorDocumento(documentoEmitente, request.Modelo, request.Ambiente);
            if (contexto is null || contexto.Emitente is null)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = "Emitente/certificado fiscal não configurado para consulta de protocolo."
                });
            }

            var resultadoCfg = _configFactory.Montar(contexto.Emitente, request.Modelo, request.Ambiente);
            if (!resultadoCfg.Sucesso || resultadoCfg.Configuracao is null)
                return Task.FromResult(new RetornoConsultaSefazDto { Sucesso = false, StatusSefaz = 0, Motivo = resultadoCfg.Mensagem });

            try
            {
                var servico = new HerculesServicos(resultadoCfg.Configuracao);
                var retorno = servico.NfeConsultaProtocolo(request.Chave);

                var cStat = retorno.Retorno?.cStat ?? 0;
                var motivo = retorno.Retorno?.xMotivo ?? "Retorno de consulta não detalhado.";

                // 100 = Autorizado o uso; 101 = Cancelamento homologado; 132 etc.
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = cStat == 100 || cStat == 101,
                    StatusSefaz = cStat,
                    Motivo = motivo,
                    XmlRetorno = retorno.RetornoStr ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = false,
                    StatusSefaz = 0,
                    Motivo = $"Falha de comunicação com a SEFAZ na consulta de protocolo: {ex.Message}"
                });
            }
        }

        private static RetornoEmissaoDto FalhaEmissao(string motivo) => new()
        {
            Sucesso = false,
            StatusSefaz = 0,
            Motivo = motivo,
            XmlEnvio = string.Empty,
            XmlRetorno = string.Empty
        };
    }
}
