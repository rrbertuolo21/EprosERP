using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — endpoint CANÔNICO do menu do usuário logado. Devolve APENAS os itens
    /// VISÍVEIS, projetados das CAPACIDADES efetivas do RBAC (mesma fonte do AbacFilter), já respeitando a
    /// empresa corrente (1.09) e o entitlement de plano (1.06). Substitui o consumo de árvore estática do
    /// front (LC-3). Autenticação obrigatória; usuário e empresa vêm SEMPRE do token, nunca do corpo.
    /// </summary>
    [ApiController]
    [Route("api/v1/menu")]
    [Produces("application/json")]
    public class MenuUsuarioController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MenuUsuarioController(IMediator mediator) { _mediator = mediator; }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ObterMenu()
        {
            // Identidade e empresa SEMPRE derivadas do principal autenticado (token completo) — nunca do corpo.
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
    }
}
