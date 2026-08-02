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

        // ----- Transições de Oportunidade (D-02: expõe métodos de domínio antes órfãos) -----

        [HttpPost("oportunidades/{id}/avancar-etapa")]
        [AbacAuthorize("ConcessionariasCrm", "Editar")]
        public async Task<ActionResult<CommandResult>> AvancarEtapa(System.Guid id, [FromBody] AvancarEtapaOportunidadeCommandBody body)
        {
            var result = await _mediator.Send(new AvancarEtapaOportunidadeCommand(id, body.NovaEtapa));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("oportunidades/{id}/converter")]
        [AbacAuthorize("ConcessionariasCrm", "Editar")]
        public async Task<ActionResult<CommandResult>> Converter(System.Guid id, [FromBody] ConverterOportunidadeCommandBody body)
        {
            var result = await _mediator.Send(new ConverterOportunidadeCommand(id, body.VendaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("oportunidades/{id}/registrar-perda")]
        [AbacAuthorize("ConcessionariasCrm", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarPerda(System.Guid id, [FromBody] RegistrarPerdaOportunidadeCommandBody body)
        {
            var result = await _mediator.Send(new RegistrarPerdaOportunidadeCommand(id, body.MotivoPerdaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("test-drives/{id}/realizar")]
        [AbacAuthorize("ConcessionariasCrm", "TestDrive")]
        public async Task<ActionResult<CommandResult>> RealizarTestDrive(System.Guid id, [FromBody] RealizarTestDriveBody body)
        {
            var result = await _mediator.Send(new RealizarTestDriveCommand(id, body.Resultado));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("test-drives/{id}/cancelar")]
        [AbacAuthorize("ConcessionariasCrm", "TestDrive")]
        public async Task<ActionResult<CommandResult>> CancelarTestDrive(System.Guid id)
        {
            var result = await _mediator.Send(new CancelarTestDriveCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RealizarTestDriveBody(string Resultado);
        public record AvancarEtapaOportunidadeCommandBody(string NovaEtapa);
        public record ConverterOportunidadeCommandBody(System.Guid VendaId);
        public record RegistrarPerdaOportunidadeCommandBody(System.Guid MotivoPerdaId);
    }
}
