using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Baixa/impressão de documentos DF-e (NF-e/NFC-e) autorizados: metadados, DANFE/cupom (PDF),
    /// XML autorizado, XML/PDF de cancelamento e de carta de correção, XML de envio, entrega ao
    /// contador (ZIP) e consultas online à SEFAZ (status/protocolo/inutilizações).
    ///
    /// Controller fino: apenas MediatR. Compatível com o legado <c>api/v1/baixa-documentos-dfe</c>
    /// e <c>api/v1/nfce-nfe</c> (família download/consulta).
    /// </summary>
    [ApiController]
    [Route("api/v1/baixa-documentos-dfe")]
    public class BaixaDocumentoDfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BaixaDocumentoDfeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ---------------------------------------------------------------------------------------
        // Metadados / listagens
        // ---------------------------------------------------------------------------------------

        /// <summary>Obtém os dados do documento fiscal (incluindo urlDanfe/urlXml) pela chave de acesso.</summary>
        [HttpGet("obter-por-chave/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorChave(string chave)
        {
            var result = await _mediator.Send(new ObterDocumentoFiscalPorChaveQuery(chave));
            if (!result.Sucesso)
                return NotFound(result.Mensagem);
            return Ok(result.Dados);
        }

        /// <summary>Lista DF-e por mês/ano de referência (e documento do destinatário, opcional), paginado.</summary>
        [HttpGet("obter-por-referencia/{mes:int}/{ano:int}/{pagina:int}/{tamanhoPagina:int}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterPorReferencia(
            int mes, int ano, int pagina, int tamanhoPagina,
            [FromQuery] string? documentoDestinatario,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ListarDocumentosPorReferenciaQuery(mes, ano, documentoDestinatario, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        // ---------------------------------------------------------------------------------------
        // Downloads do documento autorizado
        // ---------------------------------------------------------------------------------------

        /// <summary>Baixa o XML autorizado (retorno) do documento fiscal pela chave de acesso.</summary>
        [HttpGet("download-xml/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/xml")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadXml(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadXmlPorChaveQuery(chave), ct);

        /// <summary>Baixa/gera o PDF do DANFE (NF-e) / cupom (NFC-e) autorizado pela chave de acesso.</summary>
        [HttpGet("download-pdf/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadPdf(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadPdfPorChaveQuery(chave), ct);

        /// <summary>Regera e baixa o PDF do DANFE da NF-e pela chave (alias de <c>download-pdf</c>).</summary>
        [HttpPost("gerar-baixar-novo-pdf-nfe/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> GerarBaixarNovoPdfNfe(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadPdfPorChaveQuery(chave), ct);

        // ---------------------------------------------------------------------------------------
        // Downloads de cancelamento
        // ---------------------------------------------------------------------------------------

        /// <summary>Baixa o XML do cancelamento agrupado com o XML autorizado (ZIP).</summary>
        [HttpGet("download-xml-cancelamento/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/zip")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadXmlCancelamento(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadXmlCancelamentoPorChaveQuery(chave), ct);

        /// <summary>Baixa o comprovante (PDF/XML) do cancelamento pela chave.</summary>
        [HttpGet("download-pdf-cancelamento/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadPdfCancelamento(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadPdfCancelamentoPorChaveQuery(chave), ct);

        // ---------------------------------------------------------------------------------------
        // Downloads de carta de correção (CC-e)
        // ---------------------------------------------------------------------------------------

        /// <summary>Baixa o XML da última carta de correção (CC-e) pela chave.</summary>
        [HttpGet("download-xml-cce/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/xml")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadXmlCce(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadXmlCcePorChaveQuery(chave), ct);

        /// <summary>Baixa o comprovante (PDF/XML) da última carta de correção (CC-e) pela chave.</summary>
        [HttpGet("download-pdf-cce/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadPdfCce(string chave, CancellationToken ct)
            => BaixarArquivo(new DownloadPdfCcePorChaveQuery(chave), ct);

        // ---------------------------------------------------------------------------------------
        // XML de envio (pré-autorização) por venda / compra
        // ---------------------------------------------------------------------------------------

        /// <summary>Baixa o XML de envio (pré-autorização) do DF-e a partir da venda de origem.</summary>
        [HttpGet("download-xml-envio/{vendaId:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/xml")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadXmlEnvio(Guid vendaId, CancellationToken ct)
            => BaixarArquivo(new DownloadXmlEnvioPorVendaQuery(vendaId), ct);

        /// <summary>Baixa o XML de envio do DF-e a partir da compra de origem.</summary>
        [HttpGet("download-xml-compra-envio/{compraId:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/xml")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> DownloadXmlCompraEnvio(Guid compraId, CancellationToken ct)
            => BaixarArquivo(new DownloadXmlCompraEnvioQuery(compraId), ct);

        // ---------------------------------------------------------------------------------------
        // Entrega ao contador (ZIP de XMLs por período)
        // ---------------------------------------------------------------------------------------

        /// <summary>Gera um ZIP com todos os XMLs (autorizados + cancelamentos + CC-e + inutilizações) do período, para o contador.</summary>
        [HttpPost("download-documentos-contador")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/zip")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IActionResult> DownloadDocumentosContador([FromBody] DownloadDocumentosContadorQuery query, CancellationToken ct)
            => BaixarArquivo(query, ct);

        // ---------------------------------------------------------------------------------------
        // Consultas online à SEFAZ + inutilizações
        // ---------------------------------------------------------------------------------------

        /// <summary>Verifica o status do serviço da SEFAZ (online) para o emitente/UF/ambiente.</summary>
        [HttpPost("verificar-status")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> VerificarStatus([FromBody] VerificarStatusServicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Consulta o protocolo/situação de uma NF-e/NFC-e pela chave (online, SEFAZ).</summary>
        [HttpPost("consultar-protocolo")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> ConsultarProtocolo([FromBody] ConsultarProtocoloCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Inutiliza uma faixa de numeração pulada/não usada na SEFAZ.</summary>
        [HttpPost("inutilizar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Inutilizar([FromBody] InutilizarFaixaFiscalCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Lista as inutilizações de faixa por documento/ambiente (histórico). Fiel a <c>obter-inutilizacoes-por-documento</c>.</summary>
        [HttpGet("obter-inutilizacoes-por-documento/{documento}/{ambiente:int}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterInutilizacoesPorDocumento(
            string documento, int ambiente,
            [FromQuery] int? modeloDocumento,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 50,
            CancellationToken cancellationToken = default)
        {
            // As inutilizações são por tenant (emitente ativo); o filtro relevante persistido é ambiente/modelo.
            var result = await _mediator.Send(
                new ListarInutilizacoesFiscaisQuery(modeloDocumento, ambiente, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        // ---------------------------------------------------------------------------------------
        // Helper: converte um IQuery<CommandResult> que devolve ArquivoDownloadDto em FileResult.
        // ---------------------------------------------------------------------------------------
        private async Task<IActionResult> BaixarArquivo(IRequest<CommandResult> query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            if (!result.Sucesso || result.Dados is not ArquivoDownloadDto arq)
                return NotFound(result.Mensagem);

            return File(arq.Conteudo, arq.ContentType, arq.NomeArquivo);
        }
    }
}
