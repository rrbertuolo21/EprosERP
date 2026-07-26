using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// MAN-PRV — Manutencao Preventiva. Controller fino (apenas MediatR).
    /// Protegido por ABAC (EF §18). Filtro de tenant e global (ContextBase).
    /// Submodulo novo sobe desabilitado: ABAC nega por padrao; nenhuma permissao e semeada.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/preventiva")]
    public class ManutencaoPreventivaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoPreventivaController(IMediator mediator) => _mediator = mediator;

        [HttpGet("planos")]
        [AbacAuthorize("ManutencaoPreventiva", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarPlanosPreventivosQuery(pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("planos/{id:guid}")]
        [AbacAuthorize("ManutencaoPreventiva", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ObterPlanoPreventivoPorIdQuery(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("planos")]
        [AbacAuthorize("ManutencaoPreventiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarPlanoPreventivoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("planos/{id:guid}/periodicidades")]
        [AbacAuthorize("ManutencaoPreventiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarPeriodicidade(Guid id, [FromBody] AdicionarPeriodicidadePlanoCommand command, CancellationToken ct)
        {
            if (id != command.PlanoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("planos/{id:guid}/kit-pecas")]
        [AbacAuthorize("ManutencaoPreventiva", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarKitPeca(Guid id, [FromBody] AdicionarKitPecaPlanoCommand command, CancellationToken ct)
        {
            if (id != command.PlanoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("planos/{id:guid}/ativar")]
        [AbacAuthorize("ManutencaoPreventiva", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Ativar(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AtivarPlanoPreventivoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
