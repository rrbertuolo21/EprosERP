using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// APP-TEN-003 (Usuarios e Papeis): auditoria e segurança de usuário — grant/deny direto de
    /// capacidade. Controller fino: apenas MediatR.
    /// Histórico de login e impersonação controlada pertencem ao módulo Aplicativo (Identity) e são
    /// expostos por UsuariosController.
    /// Protegido por ABAC (nega por padrão; sobe efetivamente desabilitado até concessão explícita).
    /// Filtro de tenant é global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/plataforma/usuarios-seguranca")]
    public class UsuariosSegurancaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosSegurancaController(IMediator mediator) => _mediator = mediator;

        private IActionResult Resolver(CommandResult result) =>
            result.Sucesso ? Ok(result) : (result.Mensagem == "Erro de validação" ? UnprocessableEntity(result) : BadRequest(result));

        // ===== Grant/deny direto de capacidade =====
        [HttpPost("capacidades")]
        [AbacAuthorize("UsuarioCapacidade", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirCapacidade([FromBody] DefinirUsuarioCapacidadeCommand command, CancellationToken ct)
            => Resolver(await _mediator.Send(command, ct));

        [HttpDelete("capacidades/{id:guid}")]
        [AbacAuthorize("UsuarioCapacidade", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoverCapacidade(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RemoverUsuarioCapacidadeCommand(id), ct);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
