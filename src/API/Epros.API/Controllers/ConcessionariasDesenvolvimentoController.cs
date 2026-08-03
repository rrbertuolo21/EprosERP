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
    /// CON-DEV — Desenvolvimento de Concessionárias (rede/contratos). Recursos ABAC novos → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/desenvolvimento")]
    [Produces("application/json")]
    public class ConcessionariasDesenvolvimentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasDesenvolvimentoController(IMediator mediator) => _mediator = mediator;

        [HttpPost("rede")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarRedeNo([FromBody] CriarRedeNoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("rede")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Consultar")]
        public async Task<IActionResult> ListarRede() => Ok(await _mediator.Send(new ObterRedeNosQuery()));

        [HttpPost("contratos")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarContrato([FromBody] CriarContratoRedeCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("contratos")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Consultar")]
        public async Task<IActionResult> ListarContratos() => Ok(await _mediator.Send(new ObterContratosRedeQuery()));

        // ----- Transições de contrato de rede (D-02) -----

        [HttpPost("contratos/{id}/encerrar")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarContrato(System.Guid id)
        {
            var result = await _mediator.Send(new EncerrarContratoRedeCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("contratos/{id}/cancelar")]
        [AbacAuthorize("ConcessionariasDesenvolvimento", "Editar")]
        public async Task<ActionResult<CommandResult>> CancelarContrato(System.Guid id)
        {
            var result = await _mediator.Send(new CancelarContratoRedeCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
