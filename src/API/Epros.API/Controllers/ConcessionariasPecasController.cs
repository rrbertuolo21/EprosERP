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
    /// CON-PES — Peças de Reposição. Recursos ABAC novos → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/pecas")]
    [Produces("application/json")]
    public class ConcessionariasPecasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasPecasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("pecas")]
        [AbacAuthorize("ConcessionariasPecas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarPeca([FromBody] CriarPecaReposicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("pecas")]
        [AbacAuthorize("ConcessionariasPecas", "Consultar")]
        public async Task<IActionResult> ListarPecas() => Ok(await _mediator.Send(new ObterPecasReposicaoQuery()));

        [HttpPost("demandas")]
        [AbacAuthorize("ConcessionariasPecas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarDemanda([FromBody] CriarDemandaPecaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("demandas")]
        [AbacAuthorize("ConcessionariasPecas", "Consultar")]
        public async Task<IActionResult> ListarDemandas() => Ok(await _mediator.Send(new ObterDemandasPecaQuery()));

        [HttpPost("reservas")]
        [AbacAuthorize("ConcessionariasPecas", "Reservar")]
        public async Task<ActionResult<CommandResult>> CriarReserva([FromBody] CriarReservaPecaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("reservas")]
        [AbacAuthorize("ConcessionariasPecas", "Consultar")]
        public async Task<IActionResult> ListarReservas() => Ok(await _mediator.Send(new ObterReservasPecaQuery()));

        // ----- Transições (D-02) -----

        [HttpPost("reservas/{id}/cancelar")]
        [AbacAuthorize("ConcessionariasPecas", "Reservar")]
        public async Task<ActionResult<CommandResult>> CancelarReserva(System.Guid id)
        {
            var result = await _mediator.Send(new CancelarReservaPecaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("demandas/{id}/atender")]
        [AbacAuthorize("ConcessionariasPecas", "Editar")]
        public async Task<ActionResult<CommandResult>> AtenderDemanda(System.Guid id)
        {
            var result = await _mediator.Send(new AtenderDemandaPecaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("demandas/{id}/cancelar")]
        [AbacAuthorize("ConcessionariasPecas", "Editar")]
        public async Task<ActionResult<CommandResult>> CancelarDemanda(System.Guid id)
        {
            var result = await _mediator.Send(new CancelarDemandaPecaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
