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
    /// CON-GAR — Garantias (planos, garantia do veículo, claims). Recursos ABAC novos → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/garantias")]
    [Produces("application/json")]
    public class ConcessionariasGarantiasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasGarantiasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("planos")]
        [AbacAuthorize("ConcessionariasGarantias", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarPlano([FromBody] CriarPlanoGarantiaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("planos")]
        [AbacAuthorize("ConcessionariasGarantias", "Consultar")]
        public async Task<IActionResult> ListarPlanos() => Ok(await _mediator.Send(new ObterPlanosGarantiaQuery()));

        [HttpPost("veiculos")]
        [AbacAuthorize("ConcessionariasGarantias", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarVeiculoGarantia([FromBody] CriarVeiculoGarantiaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("veiculos")]
        [AbacAuthorize("ConcessionariasGarantias", "Consultar")]
        public async Task<IActionResult> ListarVeiculosGarantia() => Ok(await _mediator.Send(new ObterVeiculosGarantiaQuery()));

        [HttpPost("solicitacoes")]
        [AbacAuthorize("ConcessionariasGarantias", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarSolicitacao([FromBody] CriarSolicitacaoGarantiaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("solicitacoes")]
        [AbacAuthorize("ConcessionariasGarantias", "Consultar")]
        public async Task<IActionResult> ListarSolicitacoes() => Ok(await _mediator.Send(new ObterSolicitacoesGarantiaQuery()));

        // ----- Julgamento de solicitação (D-02: expõe Aprovar/Rejeitar; segregação decisor = NF-10) -----

        [HttpPost("solicitacoes/{id}/julgar")]
        [AbacAuthorize("ConcessionariasGarantias", "Julgar")]
        public async Task<ActionResult<CommandResult>> JulgarSolicitacao(System.Guid id, [FromBody] JulgarSolicitacaoBody body)
        {
            var result = await _mediator.Send(new JulgarSolicitacaoGarantiaCommand(id, body.Aprovar));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("veiculos/{id}/encerrar")]
        [AbacAuthorize("ConcessionariasGarantias", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarVeiculoGarantia(System.Guid id)
        {
            var result = await _mediator.Send(new EncerrarVeiculoGarantiaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record JulgarSolicitacaoBody(bool Aprovar);
    }
}
