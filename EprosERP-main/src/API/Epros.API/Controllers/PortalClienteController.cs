using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Portal do Cliente (VEN-PCL): administração de usuários externos, permissões, formulários web e
    /// solicitações. Controller fino: apenas MediatR. AutZ ABAC nega por padrão (submódulo novo).
    /// Toda consulta é filtrada por cliente/tenant (§13/§18). Fonte: EF_7_VENDAS_PORTAL_DO_CLIENTE_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/portal")]
    public class PortalClienteController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PortalClienteController(IMediator mediator) => _mediator = mediator;

        [HttpGet("usuarios")]
        [AbacAuthorize("PortalUsuarios", "Ler")]
        public async Task<IActionResult> ListarUsuarios([FromQuery] Guid? clienteId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarPortalUsuariosClienteQuery(clienteId, pagina, tamanhoPagina)));

        [HttpPost("usuarios")]
        [AbacAuthorize("PortalUsuarios", "Criar")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarPortalUsuarioClienteCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("usuarios/permissoes")]
        [AbacAuthorize("PortalUsuarios", "Administrar")]
        public async Task<IActionResult> DefinirPermissao([FromBody] DefinirPortalPermissaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("formularios")]
        [AbacAuthorize("PortalFormularios", "Criar")]
        public async Task<IActionResult> CriarFormulario([FromBody] CriarPortalFormularioCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("formularios/{id:guid}/publicar")]
        [AbacAuthorize("PortalFormularios", "Editar")]
        public async Task<IActionResult> PublicarFormulario(Guid id)
        {
            var result = await _mediator.Send(new PublicarPortalFormularioCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("formularios/{id:guid}/responsaveis")]
        [AbacAuthorize("PortalFormularios", "Editar")]
        public async Task<IActionResult> AtribuirResponsavel(Guid id, [FromBody] AtribuirPortalFormularioResponsavelCommand command)
        {
            if (id != command.FormularioId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpGet("solicitacoes")]
        [AbacAuthorize("PortalSolicitacoes", "Ler")]
        public async Task<IActionResult> ListarSolicitacoes([FromQuery] Guid clienteId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarPortalSolicitacoesQuery(clienteId, pagina, tamanhoPagina)));

        [HttpPost("solicitacoes")]
        [AbacAuthorize("PortalSolicitacoes", "Criar")]
        public async Task<IActionResult> AbrirSolicitacao([FromBody] AbrirPortalSolicitacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("solicitacoes/{id:guid}/responder")]
        [AbacAuthorize("PortalSolicitacoes", "Editar")]
        public async Task<IActionResult> ResponderSolicitacao(Guid id)
        {
            var result = await _mediator.Send(new ResponderPortalSolicitacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
