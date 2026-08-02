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

        // ----- Transições (D-02): orçamento aprovar/rejeitar; OS status/encerrar -----

        [HttpPost("orcamentos/{id}/aprovar")]
        [AbacAuthorize("ConcessionariasManutencao", "Aprovar")]
        public async Task<ActionResult<CommandResult>> AprovarOrcamento(System.Guid id)
        {
            var result = await _mediator.Send(new AprovarOrcamentoManutencaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("orcamentos/{id}/rejeitar")]
        [AbacAuthorize("ConcessionariasManutencao", "Aprovar")]
        public async Task<ActionResult<CommandResult>> RejeitarOrcamento(System.Guid id)
        {
            var result = await _mediator.Send(new RejeitarOrcamentoManutencaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("ordens-servico/{id}/status")]
        [AbacAuthorize("ConcessionariasManutencao", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarStatusOs(System.Guid id, [FromBody] AtualizarStatusOsBody body)
        {
            var result = await _mediator.Send(new AtualizarStatusOsManutencaoCommand(id, body.Status));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("ordens-servico/{id}/encerrar")]
        [AbacAuthorize("ConcessionariasManutencao", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarOs(System.Guid id)
        {
            var result = await _mediator.Send(new EncerrarOsManutencaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AtualizarStatusOsBody(string Status);
    }
}
