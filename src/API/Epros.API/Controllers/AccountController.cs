using System;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;
using Epros.Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEprosTokenService _tokenService;

        public AccountController(IMediator mediator, IEprosTokenService tokenService)
        {
            _mediator = mediator;
            _tokenService = tokenService;
        }

        [HttpPost("public/account/login")]
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

        // account/session e account/obter-acessos validam o próprio token estruturado (mecanismo
        // de estabelecimento de sessão), por isso são [AllowAnonymous] em relação à FallbackPolicy —
        // a autorização real é a validação do token feita aqui e nos handlers.

        [HttpGet("account/session")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Session()
        {
            if (!TentarLerToken(out var tenantId, out var usuarioId, out var token))
            {
                return Unauthorized(new { error = "Token inválido ou não fornecido." });
            }

            // Q5: toda a leitura de dados/regra de inadimplência foi movida para o handler MediatR.
            var result = await _mediator.Send(new RecuperarSessaoQuery(tenantId!, usuarioId!.Value, token!));

            if (!result.Sucesso)
            {
                return Unauthorized(new { error = result.Mensagem ?? "Sessão inválida." });
            }

            return Ok(result);
        }

        [HttpPost("account/obter-acessos")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status402PaymentRequired)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ObterAcessos([FromBody] ObterAcessosRequest request)
        {
            if (!TentarLerToken(out _, out var usuarioId, out _))
            {
                return Unauthorized(new { error = "Token inválido ou não fornecido." });
            }

            var query = new ObterAcessosSessaoQuery(usuarioId!.Value, request.EmpresaId);
            var result = await _mediator.Send(query);

            if (!result.Sucesso)
            {
                if (result.Block)
                {
                    return StatusCode(402, result); // 402 Payment Required para inadimplência SaaS
                }
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("public/account/gerar-nova-senha/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarNovaSenha(string email)
        {
            var command = new SolicitarRecuperacaoSenhaCommand(email);
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Extrai tenantId/usuarioId do token estruturado do header Authorization.
        /// Mantém o controller fino: apenas o parsing do token vive aqui; a lógica de negócio
        /// reside nos handlers MediatR.
        /// </summary>
        private bool TentarLerToken(out string? tenantId, out Guid? usuarioId, out string? token)
        {
            tenantId = null;
            usuarioId = null;
            token = null;

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return false;
            }

            token = authHeader.Replace("Bearer ", "").Trim();

            // Valida o JWT assinado (assinatura + expiração + emissor/audiência) e lê os claims.
            var principal = _tokenService.Validar(token);
            if (principal == null)
            {
                token = null;
                return false;
            }

            tenantId = principal.FindFirst("tenantId")?.Value;
            var uidClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(uidClaim, out var uid)) usuarioId = uid;

            return !string.IsNullOrEmpty(tenantId) && usuarioId != null;
        }

        public record LoginRequest(string Email, string Senha);
        public record ObterAcessosRequest(Guid EmpresaId);
    }
}
