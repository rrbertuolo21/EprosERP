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
    /// CON-SRV — Gestão de Serviços (catálogo/precificação). Recursos ABAC novos → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/servicos")]
    [Produces("application/json")]
    public class ConcessionariasServicosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasServicosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("tipos")]
        [AbacAuthorize("ConcessionariasServicos", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarTipo([FromBody] CriarTipoServicoConcessionariaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("tipos")]
        [AbacAuthorize("ConcessionariasServicos", "Consultar")]
        public async Task<IActionResult> ListarTipos() => Ok(await _mediator.Send(new ObterTiposServicoConcessionariaQuery()));

        [HttpPost("operacoes")]
        [AbacAuthorize("ConcessionariasServicos", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarOperacao([FromBody] CriarOperacaoServicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("operacoes")]
        [AbacAuthorize("ConcessionariasServicos", "Consultar")]
        public async Task<IActionResult> ListarOperacoes() => Ok(await _mediator.Send(new ObterOperacoesServicoQuery()));

        [HttpPost("pacotes")]
        [AbacAuthorize("ConcessionariasServicos", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarPacote([FromBody] CriarPacoteServicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("pacotes")]
        [AbacAuthorize("ConcessionariasServicos", "Consultar")]
        public async Task<IActionResult> ListarPacotes() => Ok(await _mediator.Send(new ObterPacotesServicoQuery()));
    }
}
