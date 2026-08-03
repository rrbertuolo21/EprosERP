using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.Aplicativo.Application.Queries;
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

        // ===== Endpoint de acessos (menu do usuário) =====
        // 1.10: FURO FECHADO. Antes recebia IsAdmin/PerfilAcessoId do CORPO (caller-supplied → escalonamento).
        // Agora IGNORA o corpo: identidade e empresa vêm SEMPRE do token, e o menu é a MESMA projeção por
        // capacidade do GET /api/v1/menu (fonte única). "Admin" deixa de ser afirmável pelo chamador — é
        // derivado do RBAC. Consolida as duas árvores server-side numa só (LC-1/LC-2; dúvida #4 da verificação).
        [HttpPost("acessos")]
        [AbacAuthorize("Acessos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterAcessos()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("nameid")?.Value
                      ?? User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(uid, out var usuarioId))
                return Unauthorized(new { error = "Token sem identidade de usuário válida." });

            if (!Guid.TryParse(User.FindFirst("empresaId")?.Value, out var empresaId))
                return Unauthorized(new { error = "Selecione uma empresa (token sem empresa corrente)." });

            var result = await _mediator.Send(new ObterMenuDoUsuarioQuery(usuarioId, empresaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
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
