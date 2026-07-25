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
    /// CON-FIN — Finanças / F&I (jornada, simulação, contrato). Recursos ABAC novos → sobem desabilitados.
    /// Observação: dados de cartão/meio de pagamento NÃO são persistidos aqui (RN-CFIN-013).
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/financas")]
    [Produces("application/json")]
    public class ConcessionariasFinancasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasFinancasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("jornadas")]
        [AbacAuthorize("ConcessionariasFinancas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarJornada([FromBody] CriarJornadaFinCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("jornadas")]
        [AbacAuthorize("ConcessionariasFinancas", "Consultar")]
        public async Task<IActionResult> ListarJornadas() => Ok(await _mediator.Send(new ObterJornadasFinQuery()));

        [HttpPost("simulacoes")]
        [AbacAuthorize("ConcessionariasFinancas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarSimulacao([FromBody] CriarSimulacaoFinCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("simulacoes")]
        [AbacAuthorize("ConcessionariasFinancas", "Consultar")]
        public async Task<IActionResult> ListarSimulacoes() => Ok(await _mediator.Send(new ObterSimulacoesFinQuery()));

        [HttpPost("contratos")]
        [AbacAuthorize("ConcessionariasFinancas", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarContrato([FromBody] CriarContratoFinCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("contratos")]
        [AbacAuthorize("ConcessionariasFinancas", "Consultar")]
        public async Task<IActionResult> ListarContratos() => Ok(await _mediator.Send(new ObterContratosFinQuery()));
    }
}
