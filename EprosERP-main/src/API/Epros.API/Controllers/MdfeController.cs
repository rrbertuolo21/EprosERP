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
    /// MDF-e (Manifesto Eletrônico de Documentos Fiscais). Emissão, encerramento e listagem.
    /// Controller fino: apenas MediatR. Compatível com o legado <c>api/v1/mdfe</c>.
    /// </summary>
    [ApiController]
    [Route("api/v1/mdfe")]
    public class MdfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MdfeController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lista os MDF-e emitidos (histórico), paginado.</summary>
        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarMdfesQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        /// <summary>Emite um MDF-e de forma síncrona.</summary>
        [HttpPost("emitir")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Emitir([FromBody] EmitirMdfeCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Encerra um MDF-e previamente autorizado.</summary>
        [HttpPost("encerrar/{chave}/{codigoMunicipio}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar(string chave, string codigoMunicipio)
        {
            var result = await _mediator.Send(new EncerrarMdfeCommand(chave, codigoMunicipio));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
