using System;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>APP-CAT: catálogos globais SaaS (funcionalidades, add-ons) e resolução de módulos ativos.</summary>
    [ApiController]
    [Route("api/v1/plataforma")]
    [Produces("application/json")]
    public class CatalogoSaaSController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CatalogoSaaSController(IMediator mediator) { _mediator = mediator; }

        private IActionResult Resolver(CommandResult result) =>
            result.Sucesso ? Ok(result) : (result.Mensagem == "Erro de validação" ? UnprocessableEntity(result) : BadRequest(result));

        // ===== Funcionalidades =====
        [HttpGet("funcionalidades")]
        public async Task<IActionResult> ListarFuncionalidades() => Ok(await _mediator.Send(new ListarFuncionalidadesQuery()));

        [HttpPost("funcionalidades")]
        public async Task<IActionResult> CriarFuncionalidade([FromBody] CriarFuncionalidadeCommand command) => Resolver(await _mediator.Send(command));

        [HttpPut("funcionalidades/{id:guid}")]
        public async Task<IActionResult> AtualizarFuncionalidade(Guid id, [FromBody] AtualizarFuncionalidadeCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            return Resolver(await _mediator.Send(command));
        }

        [HttpDelete("funcionalidades/{id:guid}")]
        public async Task<IActionResult> DeletarFuncionalidade(Guid id)
        {
            var result = await _mediator.Send(new DeletarFuncionalidadeCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Add-ons =====
        [HttpGet("add-ons")]
        public async Task<IActionResult> ListarAddOns([FromQuery] bool apenasHabilitados = false, [FromQuery] bool apenasNaoAdmin = false)
            => Ok(await _mediator.Send(new ListarAddOnsQuery(apenasHabilitados, apenasNaoAdmin)));

        [HttpPost("add-ons")]
        public async Task<IActionResult> CriarAddOn([FromBody] CriarAddOnCommand command) => Resolver(await _mediator.Send(command));

        [HttpPut("add-ons/{id:guid}")]
        public async Task<IActionResult> AtualizarAddOn(Guid id, [FromBody] AtualizarAddOnCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("add-ons/{id:guid}/habilitar")]
        public async Task<IActionResult> HabilitarAddOn(Guid id)
        {
            var result = await _mediator.Send(new HabilitarAddOnCommand(id));
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpPost("add-ons/{id:guid}/desabilitar")]
        public async Task<IActionResult> DesabilitarAddOn(Guid id)
        {
            var result = await _mediator.Send(new DesabilitarAddOnCommand(id));
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        // ===== Módulos ativos =====
        [HttpPost("modulos-ativos")]
        public async Task<IActionResult> AtivarModuloUsuario([FromBody] AtivarModuloUsuarioCommand command) => Resolver(await _mediator.Send(command));

        [HttpGet("modulos-ativos/resolver/{usuarioId:guid}")]
        public async Task<IActionResult> ResolverModulosAtivos(Guid usuarioId)
            => Ok(await _mediator.Send(new ResolverModulosAtivosQuery(usuarioId)));
    }
}
