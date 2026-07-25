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
    /// CT-e (Conhecimento de Transporte Eletrônico). Emissão, cancelamento e listagem.
    /// Controller fino: apenas MediatR. Compatível com o legado <c>api/v1/cte</c>.
    /// </summary>
    [ApiController]
    [Route("api/v1/cte")]
    public class CteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CteController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lista os CT-e emitidos (histórico), paginado.</summary>
        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarCtesQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        /// <summary>Emite um CT-e de forma síncrona.</summary>
        [HttpPost("emitir")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Emitir([FromBody] EmitirCteCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Cancela um CT-e previamente autorizado.</summary>
        [HttpPost("cancelar/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar(string chave, [FromBody] string justificativa)
        {
            var result = await _mediator.Send(new CancelarCteCommand(chave, justificativa));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
