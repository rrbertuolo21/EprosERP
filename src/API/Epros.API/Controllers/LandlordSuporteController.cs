using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Área Landlord (Siser) — acesso de suporte a clientes (1.04 Pass 4).
    /// Um usuário da Siser com perfil de suporte (Suporte Técnico / Suporte Negócio) abre uma sessão
    /// de suporte num tenant cliente, com auditoria obrigatória e salvaguarda de bloqueio ao ADMIN
    /// do cliente. É o acesso cross-tenant que formaliza a antiga impersonação.
    ///
    /// Fronteira Landlord × cliente: protegido por ABAC (SuperAdmin:Configurar) — o token da Siser
    /// carrega tenant "system", único que passa. Além disso, cada handler reexige tenant "system"
    /// como guarda real (defense-in-depth), pois o cargo "Administrador" de um tenant comum
    /// curto-circuita o ABAC.
    /// </summary>
    [ApiController]
    [Route("api/v1/landlord/suporte")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    [Produces("application/json")]
    public class LandlordSuporteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LandlordSuporteController(IMediator mediator) => _mediator = mediator;

        /// <summary>Abre uma sessão de suporte da Siser num tenant cliente (emite token escopado ao alvo).</summary>
        [HttpPost("acessar-cliente")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AcessarCliente([FromBody] IniciarAcessoSuporteCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>Encerra uma sessão de suporte/impersonação (mesma trilha de auditoria).</summary>
        [HttpPost("encerrar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar([FromBody] EncerrarImpersonacaoCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>Define o perfil de suporte (papel de sistema) de um usuário interno da Siser.</summary>
        [HttpPut("usuarios-internos/{id:guid}/perfil-suporte")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirPerfilSuporte(Guid id, [FromBody] DefinirPerfilSuporteInternoCommand command, CancellationToken ct)
        {
            if (id != command.UsuarioInternoId)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo da requisição.");
            }
            var result = await _mediator.Send(command, ct);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }
}
