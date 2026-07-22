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
    /// <summary>APP-TEN-003: RBAC (papel, capacidade, usuario_papel, nivel_usuario).</summary>
    [ApiController]
    [Route("api/v1/plataforma")]
    [Produces("application/json")]
    public class RbacController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RbacController(IMediator mediator) { _mediator = mediator; }

        private IActionResult Resolver(CommandResult result) =>
            result.Sucesso ? Ok(result) : (result.Mensagem == "Erro de validação" ? UnprocessableEntity(result) : BadRequest(result));

        // ===== Papéis =====
        [HttpGet("papeis")]
        public async Task<IActionResult> ListarPapeis([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarPapeisQuery(pagina, tamanhoPagina)));

        [HttpGet("papeis/{id:guid}")]
        public async Task<IActionResult> ObterPapel(Guid id)
        {
            var result = await _mediator.Send(new ObterPapelPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("papeis")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> CriarPapel([FromBody] CriarPapelCommand command)
            => Resolver(await _mediator.Send(command));

        [HttpPut("papeis/{id:guid}")]
        public async Task<IActionResult> AtualizarPapel(Guid id, [FromBody] AtualizarPapelCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            return Resolver(await _mediator.Send(command));
        }

        [HttpDelete("papeis/{id:guid}")]
        public async Task<IActionResult> DeletarPapel(Guid id)
        {
            var result = await _mediator.Send(new DeletarPapelCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Capacidades =====
        [HttpGet("capacidades")]
        public async Task<IActionResult> ListarCapacidades([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCapacidadesQuery(pagina, tamanhoPagina)));

        [HttpPost("capacidades")]
        public async Task<IActionResult> CriarCapacidade([FromBody] CriarCapacidadeCommand command)
            => Resolver(await _mediator.Send(command));

        [HttpPut("capacidades/{id:guid}")]
        public async Task<IActionResult> AtualizarCapacidade(Guid id, [FromBody] AtualizarCapacidadeCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha(new[] { "O ID da rota difere do corpo." }));
            return Resolver(await _mediator.Send(command));
        }

        [HttpDelete("capacidades/{id:guid}")]
        public async Task<IActionResult> DeletarCapacidade(Guid id)
        {
            var result = await _mediator.Send(new DeletarCapacidadeCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Atribuição de papel a usuário =====
        [HttpPost("usuarios-papeis")]
        public async Task<IActionResult> AtribuirPapel([FromBody] AtribuirPapelUsuarioCommand command)
            => Resolver(await _mediator.Send(command));

        [HttpDelete("usuarios-papeis/{id:guid}")]
        public async Task<IActionResult> RemoverPapel(Guid id)
        {
            var result = await _mediator.Send(new RemoverPapelUsuarioCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Níveis de usuário =====
        [HttpGet("niveis-usuario")]
        public async Task<IActionResult> ListarNiveis([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarNiveisUsuarioQuery(pagina, tamanhoPagina)));

        [HttpPost("niveis-usuario")]
        public async Task<IActionResult> CriarNivel([FromBody] CriarNivelUsuarioCommand command)
            => Resolver(await _mediator.Send(command));

        [HttpPost("niveis-usuario/{id:guid}/precos")]
        public async Task<IActionResult> AdicionarPreco(Guid id, [FromBody] PrecoNivelBody body)
            => Resolver(await _mediator.Send(new AdicionarPrecoNivelUsuarioCommand(id, body.PricingLabel, body.PackagePricingType, body.Period, body.DownloadAllowance, body.Price)));
    }

    public record PrecoNivelBody(string PricingLabel, string PackagePricingType, string? Period, long? DownloadAllowance, decimal Price);
}
