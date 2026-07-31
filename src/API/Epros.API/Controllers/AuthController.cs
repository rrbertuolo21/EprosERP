using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("public/auth/login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Login([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "unknown";

            var command = new AutenticarUsuarioCommand(request.Email, request.Senha, ipAddress, userAgent);
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("public/plataforma/login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> LoginPlataforma([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "unknown";

            var command = new AutenticarUsuarioInternoCommand(request.Email, request.Senha, ipAddress, userAgent);
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("auth/selecionar-empresa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SelecionarEmpresa([FromBody] SelecionarEmpresaCommand command)
        {
            // Segurança (anti-IDOR/escalonamento): o usuário é SEMPRE a identidade do token de sessão,
            // nunca o UsuarioId enviado no corpo — senão qualquer autenticado escolheria empresa por outro.
            if (!TryObterUsuarioAutenticado(out var usuarioId))
            {
                return Unauthorized();
            }

            command = command with { UsuarioId = usuarioId };
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("auth/selecionar-tenant")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SelecionarTenant([FromBody] SelecionarTenantCommand command)
        {
            // Segurança (anti-IDOR/escalonamento): o usuário é SEMPRE a identidade do token de sessão
            // pré-tenant, nunca o UsuarioId do corpo — senão um autenticado emitiria token escopado como
            // outro usuário (privilege escalation), pois o handler só valida a membership (usuário × tenant).
            if (!TryObterUsuarioAutenticado(out var usuarioId))
            {
                return Unauthorized();
            }

            command = command with { UsuarioId = usuarioId };
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("public/auth/recuperar-senha")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RecuperarSenha([FromBody] SolicitarRecuperacaoSenhaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("public/auth/resetar-senha")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ResetarSenha([FromBody] ResetarSenhaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("auth/alterar-senha")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarSenha([FromBody] AlterarSenhaUsuarioCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("public/auth/registrar-tenant")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarTenant([FromBody] RegistrarNovoTenantCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("auth/social/{provedor}/start")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> IniciarLoginSocial([FromRoute] string provedor)
        {
            // Login social (1.04 PASS 3): gera state/nonce e devolve a URL de autorização do provedor.
            // O front redireciona o navegador para result.Dados.UrlAutorizacao.
            var result = await _mediator.Send(new IniciarLoginSocialCommand(provedor));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("auth/social/{provedor}/callback")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CallbackLoginSocial(
            [FromRoute] string provedor,
            [FromQuery] string? code,
            [FromQuery] string? state,
            [FromQuery] string? error)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = Request.Headers["User-Agent"].ToString() ?? "unknown";

            var command = new CallbackLoginSocialCommand(provedor, code, state, error, ipAddress, userAgent);
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("auth/logout")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Logout()
        {
            // Logout real (REG-013): revoga as sessões do usuário autenticado (identidade do token).
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var usuarioId))
            {
                return UnprocessableEntity(CommandResult.Falha(new[] { "Usuário não identificado no token." }));
            }

            var result = await _mediator.Send(new EncerrarSessaoCommand(usuarioId));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("auth/empresas-disponiveis")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarEmpresasDisponiveis()
        {
            // Segurança (anti-IDOR): o usuário vem do token autenticado (claim), NUNCA de query param —
            // antes qualquer um enumerava as empresas de qualquer usuarioId. Cada um lista só as próprias.
            if (!TryObterUsuarioAutenticado(out var usuarioId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new ListarEmpresasDisponiveisQuery(usuarioId));
            return Ok(result);
        }

        [HttpGet("auth/tenants-disponiveis")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Epros.Modules.Aplicativo.Application.Dtos.TenantDisponivelDto>>> ListarTenantsDisponiveis()
        {
            // Segurança (anti-IDOR): idem — o token de sessão pré-tenant já autentica a identidade; a lista
            // de tenants sai do claim do principal, jamais de um usuarioId controlado pelo cliente na URL.
            if (!TryObterUsuarioAutenticado(out var usuarioId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new ListarTenantsDisponiveisQuery(usuarioId));
            return Ok(result);
        }

        /// <summary>
        /// Identidade autenticada (claim <see cref="ClaimTypes.NameIdentifier"/> do token de sessão).
        /// Fonte ÚNICA do usuarioId nos fluxos de seleção/listagem — nunca confiar em input do cliente.
        /// </summary>
        private bool TryObterUsuarioAutenticado(out Guid usuarioId)
        {
            usuarioId = Guid.Empty;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out usuarioId);
        }

        public record LoginRequest(string Email, string Senha);
    }
}
