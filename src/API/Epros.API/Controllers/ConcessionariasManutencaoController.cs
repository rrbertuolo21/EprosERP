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
    /// CON-MNT — Manutenção (oficina). Recursos ABAC novos → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/manutencao")]
    [Produces("application/json")]
    public class ConcessionariasManutencaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasManutencaoController(IMediator mediator) => _mediator = mediator;

        [HttpPost("ordens-servico")]
        [AbacAuthorize("ConcessionariasManutencao", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarOrdemServico([FromBody] CriarOrdemServicoManutencaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("ordens-servico")]
        [AbacAuthorize("ConcessionariasManutencao", "Consultar")]
        public async Task<IActionResult> ListarOrdensServico() => Ok(await _mediator.Send(new ObterOrdensServicoManutencaoQuery()));

        [HttpPost("orcamentos")]
        [AbacAuthorize("ConcessionariasManutencao", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarOrcamento([FromBody] CriarOrcamentoManutencaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("orcamentos")]
        [AbacAuthorize("ConcessionariasManutencao", "Consultar")]
        public async Task<IActionResult> ListarOrcamentos() => Ok(await _mediator.Send(new ObterOrcamentosManutencaoQuery()));
    }
}
