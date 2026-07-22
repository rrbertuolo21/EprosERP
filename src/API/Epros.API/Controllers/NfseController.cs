using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Modules.Fiscal.Application.Services;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// NFS-e (Nota Fiscal de Serviço Eletrônica). Emissão de lote de RPS, consultas (lote/RPS),
    /// cancelamento, configuração do provedor municipal e geração de PDF a partir de XML.
    /// Controller fino: apenas MediatR (exceto o transform de arquivo <c>gerar-pdf-xml</c>, que usa
    /// o serviço de NFS-e diretamente). Compatível com o legado <c>api/v1/nfse</c>.
    /// </summary>
    [ApiController]
    [Route("api/v1/nfse")]
    public class NfseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INfseFiscalService _nfseService;

        public NfseController(IMediator mediator, INfseFiscalService nfseService)
        {
            _mediator = mediator;
            _nfseService = nfseService;
        }

        /// <summary>Obtém a configuração do provedor NFS-e para o município informado.</summary>
        [HttpGet("config")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> ObterConfig([FromQuery] int codigoMunicipioIbge)
        {
            var result = await _mediator.Send(new ObterConfigNfseQuery(codigoMunicipioIbge));
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Emite um lote de RPS (NFS-e).</summary>
        [HttpPost("emitir-lote")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EmitirLote([FromBody] EmitirLoteNfseCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Consulta a situação de um lote de NFS-e.</summary>
        [HttpPost("consultar-lote")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> ConsultarLote([FromBody] ConsultarLoteNfseCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Consulta a NFS-e gerada a partir de um RPS.</summary>
        [HttpPost("consultar-por-rps")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> ConsultarPorRps([FromBody] ConsultarNfsePorRpsCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Cancela uma NFS-e autorizada.</summary>
        [HttpPost("cancelar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Cancelar([FromBody] CancelarNfseCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Lista as NFS-e emitidas (histórico), paginado.</summary>
        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarNotasServicoQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        /// <summary>Gera o PDF (DANFSe) a partir de um arquivo XML de NFS-e já emitido.</summary>
        [HttpPost("gerar-pdf-xml")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Consumes("multipart/form-data")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GerarPdfDeXml(
            IFormFile arquivo,
            [FromQuery] int? codigoMunicipioIbge,
            CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(CommandResult.Falha("Envie o arquivo XML no campo 'arquivo'."));

            var extensao = Path.GetExtension(arquivo.FileName);
            if (!string.Equals(extensao, ".xml", System.StringComparison.OrdinalIgnoreCase)
                && !arquivo.ContentType.Contains("xml", System.StringComparison.OrdinalIgnoreCase))
                return BadRequest(CommandResult.Falha("O arquivo deve ser XML (.xml)."));

            await using var stream = arquivo.OpenReadStream();
            var retorno = await _nfseService.GerarPdfDeXmlAsync(stream, codigoMunicipioIbge, cancellationToken);

            if (!retorno.Sucesso)
                return BadRequest(CommandResult.Falha(retorno.Motivo));

            return File(retorno.Pdf, "application/pdf", retorno.NomeArquivo);
        }
    }
}
