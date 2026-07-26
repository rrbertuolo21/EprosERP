using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/esg")]
    [Produces("application/json")]
    public class ESGController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ESGController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("emissoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarEmissao([FromBody] RegistrarEmissaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("relatorios")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarRelatorio([FromBody] CriarRelatorioESGCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("relatorios/{ano}/consolidar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConsolidarRelatorio(int ano)
        {
            var command = new ConsolidarRelatorioESGCommand(ano);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpGet("emissoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarEmissoes()
        {
            var result = await _mediator.Send(new ObterEmissoesQuery());
            return Ok(result);
        }

        [HttpGet("relatorios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarRelatorios()
        {
            var result = await _mediator.Send(new ObterRelatoriosESGQuery());
            return Ok(result);
        }
    }
}
