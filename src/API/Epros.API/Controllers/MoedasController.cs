using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;
using Epros.API.Security;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma/moedas")]
    [Produces("application/json")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    public class MoedasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MoedasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Listar() => Ok(await _mediator.Send(new ListarMoedasCatalogoQuery()));

        [HttpPost]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarMoedaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CommandResult>> Atualizar(Guid id, [FromBody] AtualizarMoedaCommand command)
        {
            var result = await _mediator.Send(command with { Id = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id)
        {
            var result = await _mediator.Send(new ExcluirMoedaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
