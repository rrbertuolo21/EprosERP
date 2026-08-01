using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Shared.Application.Models;
using Epros.API.Security;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/aplicativo/cupons")]
    [Produces("application/json")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    // 1.11 decisão #5 — área comercial: faixa de Suporte Negócio (SuporteTecnico é negado; PrimaryAdmin passa).
    [AbacAuthorize(SuperAdminSeguranca.RecursoSuporteComercial, "Configurar")]
    public class CuponsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CuponsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarCupomCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("validar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Validar([FromQuery] string codigo, [FromQuery] Guid planoId)
        {
            var command = new ValidarCupomCommand(codigo, planoId);
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50)
            => Ok(await _mediator.Send(new ListarCuponsQuery(pagina, tamanhoPagina)));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var dto = await _mediator.Send(new ObterCupomQuery(id));
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CommandResult>> Atualizar(Guid id, [FromBody] AtualizarCupomCommand command)
        {
            var result = await _mediator.Send(command with { Id = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id)
        {
            var result = await _mediator.Send(new ExcluirCupomCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
