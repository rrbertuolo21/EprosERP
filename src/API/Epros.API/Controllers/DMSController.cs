using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/dms")]
    [Produces("application/json")]
    public class DMSController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DMSController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("vendas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarVendaVeiculo([FromBody] RegistrarVendaVeiculoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("ordens-servico")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirOrdemServico([FromBody] AbrirOrdemServicoDmsCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("ordens-servico/{id}/fechar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FecharOrdemServico(Guid id)
        {
            var command = new FecharOrdemServicoDmsCommand(id);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("vendas/{id}/faturar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FaturarVenda(Guid id)
        {
            var result = await _mediator.Send(new FaturarVendaVeiculoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("vendas/{id}/entregar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EntregarVenda(Guid id)
        {
            var result = await _mediator.Send(new EntregarVendaVeiculoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("garantias/{id}/julgar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> JulgarGarantia(Guid id, [FromBody] JulgarGarantiaRequest request)
        {
            var command = new JulgarGarantiaMontadoraCommand(id, request.NovoStatus, request.Parecer);
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        public record JulgarGarantiaRequest(string NovoStatus, string Parecer);

        [HttpGet("vendas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarVendas()
        {
            var result = await _mediator.Send(new ObterVendasVeiculosQuery());
            return Ok(result);
        }

        [HttpGet("ordens-servico")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarOrdensServico()
        {
            var result = await _mediator.Send(new ObterOrdensServicoDmsQuery());
            return Ok(result);
        }

        [HttpGet("garantias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarGarantias()
        {
            var result = await _mediator.Send(new ObterGarantiasMontadoraQuery());
            return Ok(result);
        }
    }
}
