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
    /// MAN-IND — Inducao e Configuracao de Equipamentos. Controller fino. ABAC nega por padrao.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/manutencao/equipamentos-config")]
    public class ManutencaoEquipamentoConfigController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ManutencaoEquipamentoConfigController(IMediator mediator) => _mediator = mediator;

        [HttpGet("inducoes")]
        [AbacAuthorize("ManutencaoEquipamentoConfig", "Consultar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarInducoes([FromQuery] Guid? equipamentoId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListarInducoesQuery(equipamentoId, pagina, tamanhoPagina), ct);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPut("equipamentos/{id:guid}/configuracao")]
        [AbacAuthorize("ManutencaoEquipamentoConfig", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Configurar(Guid id, [FromBody] ConfigurarEquipamentoCommand command, CancellationToken ct)
        {
            if (id != command.EquipamentoId) return BadRequest("O ID da rota nao corresponde ao ID do corpo.");
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("inducoes")]
        [AbacAuthorize("ManutencaoEquipamentoConfig", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IniciarInducao([FromBody] IniciarInducaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("inducoes/{id:guid}/aprovar")]
        [AbacAuthorize("ManutencaoEquipamentoConfig", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AprovarInducao(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new AprovarInducaoCommand(id), ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
