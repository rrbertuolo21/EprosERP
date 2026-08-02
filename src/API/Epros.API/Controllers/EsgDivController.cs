using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// ESG-DIV (Diversidade e Responsabilidade Social) — construido no V1 (CD1).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "EsgDiversidade").
    /// Medicao e SEMPRE agregada; a supressao de grupos pequenos (NF-09/T1) e aplicada no dominio.
    /// </summary>
    [ApiController]
    [Route("api/v1/esg/div")]
    [Produces("application/json")]
    public class EsgDivController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EsgDivController(IMediator mediator) => _mediator = mediator;

        [HttpGet("programas")]
        [AbacAuthorize("EsgDiversidade", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarProgramas()
            => Ok(await _mediator.Send(new ListarProgramasDivQuery()));

        [HttpGet("indicadores")]
        [AbacAuthorize("EsgDiversidade", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarIndicadores()
            => Ok(await _mediator.Send(new ListarIndicadoresDivQuery()));

        [HttpGet("indicadores/{id:guid}/medicoes")]
        [AbacAuthorize("EsgDiversidade", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMedicoes(Guid id)
            => Ok(await _mediator.Send(new ListarMedicoesDivQuery(id)));

        [HttpPost("programas")]
        [AbacAuthorize("EsgDiversidade", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarPrograma([FromBody] CriarProgramaDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("programas/{id:guid}/submeter")]
        [AbacAuthorize("EsgDiversidade", "Editar")]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterProgramaDivCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("programas/{id:guid}/aprovar")]
        [AbacAuthorize("EsgDiversidade", "Editar")]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarProgramaDivCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("indicadores")]
        [AbacAuthorize("EsgDiversidade", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarIndicador([FromBody] CriarIndicadorDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("parametros")]
        [AbacAuthorize("EsgDiversidade", "Editar")]
        public async Task<ActionResult<CommandResult>> DefinirParametro([FromBody] DefinirParametroDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("medicoes")]
        [AbacAuthorize("EsgDiversidade", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarMedicao([FromBody] RegistrarMedicaoDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("metas")]
        [AbacAuthorize("EsgDiversidade", "Criar")]
        public async Task<ActionResult<CommandResult>> DefinirMeta([FromBody] DefinirMetaDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("acoes")]
        [AbacAuthorize("EsgDiversidade", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarAcao([FromBody] CriarAcaoDivCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
