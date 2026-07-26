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
    /// CON-CRM — CRM de Concessionária. Recursos ABAC novos (não semeados) → sobem desabilitados.
    /// </summary>
    [ApiController]
    [Route("api/v1/concessionarias/crm")]
    [Produces("application/json")]
    public class ConcessionariasCrmController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConcessionariasCrmController(IMediator mediator) => _mediator = mediator;

        [HttpPost("prospects")]
        [AbacAuthorize("ConcessionariasCrm", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarProspect([FromBody] CriarProspectShowroomCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("prospects")]
        [AbacAuthorize("ConcessionariasCrm", "Consultar")]
        public async Task<IActionResult> ListarProspects() => Ok(await _mediator.Send(new ObterProspectsShowroomQuery()));

        [HttpPost("oportunidades")]
        [AbacAuthorize("ConcessionariasCrm", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarOportunidade([FromBody] CriarOportunidadeConcessionariaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("oportunidades")]
        [AbacAuthorize("ConcessionariasCrm", "Consultar")]
        public async Task<IActionResult> ListarOportunidades() => Ok(await _mediator.Send(new ObterOportunidadesConcessionariaQuery()));

        [HttpPost("test-drives")]
        [AbacAuthorize("ConcessionariasCrm", "TestDrive")]
        public async Task<ActionResult<CommandResult>> CriarTestDrive([FromBody] CriarTestDriveCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("test-drives")]
        [AbacAuthorize("ConcessionariasCrm", "Consultar")]
        public async Task<IActionResult> ListarTestDrives() => Ok(await _mediator.Send(new ObterTestDrivesQuery()));
    }
}
