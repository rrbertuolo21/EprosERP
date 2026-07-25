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
    /// Inutilização de faixa de numeração DF-e (NF-e/NFC-e) na SEFAZ.
    /// Controller fino: apenas MediatR. Compatível com o legado <c>api/v1/inutilizacao-dfe</c>.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/inutilizacao-dfe")]
    public class InutilizacaoDfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InutilizacaoDfeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Lista as inutilizações de faixa de numeração já realizadas (histórico).</summary>
        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] int? modeloDocumento,
            [FromQuery] int? ambiente,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarInutilizacoesFiscaisQuery(modeloDocumento, ambiente, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>Inutiliza uma faixa de numeração pulada/não usada na SEFAZ.</summary>
        [HttpPost("inutilizar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Inutilizar([FromBody] InutilizarFaixaFiscalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }
    }
}
