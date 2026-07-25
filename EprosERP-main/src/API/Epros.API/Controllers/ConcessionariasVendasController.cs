using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// CON-VEN — Vendas F&I de veículos. Recursos ABAC novos (não semeados) → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/vendas")]
    [Produces("application/json")]
    public class ConcessionariasVendasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasVendasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("estoque")]
        [AbacAuthorize("ConcessionariasVendas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarEstoqueVeiculo([FromBody] CriarEstoqueVeiculoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("estoque")]
        [AbacAuthorize("ConcessionariasVendas", "Consultar")]
        public async Task<IActionResult> ListarEstoque() => Ok(await _mediator.Send(new ObterEstoqueVeiculosQuery()));

        [HttpPost("reservas")]
        [AbacAuthorize("ConcessionariasVendas", "Reservar")]
        public async Task<ActionResult<CommandResult>> CriarReserva([FromBody] CriarReservaVeiculoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("reservas")]
        [AbacAuthorize("ConcessionariasVendas", "Consultar")]
        public async Task<IActionResult> ListarReservas() => Ok(await _mediator.Send(new ObterReservasVeiculoQuery()));

        [HttpPost("propostas")]
        [AbacAuthorize("ConcessionariasVendas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarProposta([FromBody] CriarPropostaVendaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("propostas")]
        [AbacAuthorize("ConcessionariasVendas", "Consultar")]
        public async Task<IActionResult> ListarPropostas() => Ok(await _mediator.Send(new ObterPropostasVendaQuery()));
    }
}
