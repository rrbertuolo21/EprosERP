using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/cadastros/[controller]")]
    [Produces("application/json")]
    public class GeografiaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GeografiaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("paises")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPaises()
        {
            var result = await _mediator.Send(new ListarPaisesQuery());
            return Ok(result);
        }

        [HttpGet("paises/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPaisPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterPaisPorIdQuery(id));
            if (result == null)
            {
                return NotFound(new { message = "País não encontrado." });
            }
            return Ok(result);
        }

        [HttpPost("paises")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarPais([FromBody] CriarPaisCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("municipios/{ibgeCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterMunicipioPorIbge(long ibgeCode)
        {
            var result = await _mediator.Send(new ObterMunicipioPorIbgeQuery(ibgeCode));
            if (result == null)
            {
                return NotFound(new { message = "Município não encontrado." });
            }
            return Ok(result);
        }

        [HttpPost("municipios")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarMunicipio([FromBody] CriarMunicipioCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("municipios/obter-por-uf/{uf}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMunicipiosPorUf(string uf)
        {
            var result = await _mediator.Send(new ListarMunicipiosPorUfQuery(uf));
            return Ok(result);
        }

        [HttpGet("municipios/obter-por-id-uf/{ufId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMunicipiosPorIdUf(Guid ufId)
        {
            var result = await _mediator.Send(new ListarMunicipiosPorIdUfQuery(ufId));
            return Ok(result);
        }

        [HttpGet("cep/{cep}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConsultarCep(string cep)
        {
            var result = await _mediator.Send(new ConsultarCepQuery(cep));
            if (result == null)
            {
                return NotFound(new { message = "CEP não pôde ser resolvido." });
            }
            return Ok(result);
        }

        [HttpPost("municipios/sincronizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Sincronizar()
        {
            var result = await _mediator.Send(new SincronizarGeografiaCommand());
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("zonas-entrega")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarZonasEntrega()
        {
            var result = await _mediator.Send(new ListarZonasEntregaQuery());
            return Ok(result);
        }

        [HttpPost("zonas-entrega")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarZonaEntrega([FromBody] CriarZonaEntregaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("cep/cache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCepCache()
        {
            var result = await _mediator.Send(new ListarCepCacheQuery());
            return Ok(result);
        }

        [HttpPost("cep/{cep}/reprocessar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ReprocessarCep(string cep)
        {
            var result = await _mediator.Send(new ReprocessarCepCommand(cep));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPut("cep/manual")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarCepManual([FromBody] RegistrarCepManualCommand command)
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
