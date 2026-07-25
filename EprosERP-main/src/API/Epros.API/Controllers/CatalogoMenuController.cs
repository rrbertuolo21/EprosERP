using System;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;
using Epros.API.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>APP-TEN-008: CRUD do catálogo de menu (3 níveis) e endpoint de acessos do usuário.</summary>
    [ApiController]
    [Route("api/v1/plataforma/catalogo-menus")]
    [Produces("application/json")]
    public class CatalogoMenuController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CatalogoMenuController(IMediator mediator) { _mediator = mediator; }

        private IActionResult Resolver(CommandResult result) =>
            result.Sucesso ? Ok(result) : (result.Mensagem == "Erro de validação" ? UnprocessableEntity(result) : BadRequest(result));

        // ===== Endpoint de acessos (AcessosResponse) =====
        [HttpPost("acessos")]
        [AbacAuthorize("Acessos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterAcessos([FromBody] ObterAcessosUsuarioQuery query)
        {
            var result = await _mediator.Send(query);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        // ===== Menu principal =====
        [HttpPost]
        [AbacAuthorize("Menu", "Criar")]
        public async Task<IActionResult> CriarMenu([FromBody] CriarMenuCommand command) => Resolver(await _mediator.Send(command));

        [HttpPut("{id:guid}")]
        [AbacAuthorize("Menu", "Editar")]
        public async Task<IActionResult> AtualizarMenu(Guid id, [FromBody] AtualizarMenuCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("Menu", "Excluir")]
        public async Task<IActionResult> DeletarMenu(Guid id)
        {
            var result = await _mediator.Send(new DeletarMenuCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Nível 1 =====
        [HttpPost("itens-nivel1")]
        [AbacAuthorize("Menu", "Criar")]
        public async Task<IActionResult> CriarItemN1([FromBody] CriarMenuItemNivel1Command command) => Resolver(await _mediator.Send(command));

        [HttpPut("itens-nivel1/{id:guid}")]
        [AbacAuthorize("Menu", "Editar")]
        public async Task<IActionResult> AtualizarItemN1(Guid id, [FromBody] AtualizarMenuItemNivel1Command command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpDelete("itens-nivel1/{id:guid}")]
        [AbacAuthorize("Menu", "Excluir")]
        public async Task<IActionResult> DeletarItemN1(Guid id)
        {
            var result = await _mediator.Send(new DeletarMenuItemNivel1Command(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Nível 2 =====
        [HttpPost("itens-nivel2")]
        [AbacAuthorize("Menu", "Criar")]
        public async Task<IActionResult> CriarItemN2([FromBody] CriarMenuItemNivel2Command command) => Resolver(await _mediator.Send(command));

        [HttpPut("itens-nivel2/{id:guid}")]
        [AbacAuthorize("Menu", "Editar")]
        public async Task<IActionResult> AtualizarItemN2(Guid id, [FromBody] AtualizarMenuItemNivel2Command command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpDelete("itens-nivel2/{id:guid}")]
        [AbacAuthorize("Menu", "Excluir")]
        public async Task<IActionResult> DeletarItemN2(Guid id)
        {
            var result = await _mediator.Send(new DeletarMenuItemNivel2Command(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
