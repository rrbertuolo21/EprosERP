using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Gestão de Serviços comerciais (VEN-GSV): catálogo de serviços e faturas de serviço.
    /// Controller fino: apenas MediatR. AutZ ABAC nega por padrão (submódulo novo, sem permissão semeada).
    /// Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/servicos")]
    public class ServicosComerciaisController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ServicosComerciaisController(IMediator mediator) => _mediator = mediator;

        // ---------- Catálogo de serviços ----------

        [HttpGet("catalogo")]
        [AbacAuthorize("ServicosComerciais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCatalogo([FromQuery] string? localizar, [FromQuery] bool apenasAtivos = true, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarServicoCatalogoQuery(localizar, apenasAtivos, pagina, tamanhoPagina)));

        [HttpGet("catalogo/{id:guid}")]
        [AbacAuthorize("ServicosComerciais", "Ler")]
        public async Task<IActionResult> ObterServico(Guid id)
        {
            var result = await _mediator.Send(new ObterServicoCatalogoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("catalogo")]
        [AbacAuthorize("ServicosComerciais", "Criar")]
        public async Task<IActionResult> CriarServico([FromBody] CriarServicoCatalogoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("catalogo/{id:guid}")]
        [AbacAuthorize("ServicosComerciais", "Editar")]
        public async Task<IActionResult> AtualizarServico(Guid id, [FromBody] AtualizarServicoCatalogoCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("catalogo/{id:guid}")]
        [AbacAuthorize("ServicosComerciais", "Excluir")]
        public async Task<IActionResult> ExcluirServico(Guid id)
        {
            var result = await _mediator.Send(new ExcluirServicoCatalogoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("catalogo/{id:guid}/reativar")]
        [AbacAuthorize("ServicosComerciais", "Editar")]
        public async Task<IActionResult> ReativarServico(Guid id)
        {
            var result = await _mediator.Send(new ReativarServicoCatalogoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Faturas de serviço ----------

        [HttpGet("faturas")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Ler")]
        public async Task<IActionResult> ListarFaturas([FromQuery] Guid? clienteId, [FromQuery] Guid? funcionarioId, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarServicoFaturaQuery(clienteId, funcionarioId, dataInicio, dataFim, pagina, tamanhoPagina)));

        [HttpGet("faturas/{id:guid}")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Ler")]
        public async Task<IActionResult> ObterFatura(Guid id)
        {
            var result = await _mediator.Send(new ObterServicoFaturaPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("faturas")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Criar")]
        public async Task<IActionResult> CriarFatura([FromBody] CriarServicoFaturaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("faturas/{id:guid}")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Editar")]
        public async Task<IActionResult> AtualizarFatura(Guid id, [FromBody] AtualizarServicoFaturaCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("faturas/{id:guid}/confirmar")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Editar")]
        public async Task<IActionResult> ConfirmarFatura(Guid id)
        {
            var result = await _mediator.Send(new ConfirmarServicoFaturaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("faturas/{id:guid}/faturar")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Editar")]
        public async Task<IActionResult> Faturar(Guid id)
        {
            var result = await _mediator.Send(new FaturarServicoFaturaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("faturas/{id:guid}/cancelar")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Editar")]
        public async Task<IActionResult> CancelarFatura(Guid id)
        {
            var result = await _mediator.Send(new CancelarServicoFaturaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>EC-08: estorna fatura já faturada (transição distinta de cancelar), revertendo lançamentos.</summary>
        [HttpPost("faturas/{id:guid}/estornar")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Editar")]
        public async Task<IActionResult> EstornarFatura(Guid id)
        {
            var result = await _mediator.Send(new EstornarServicoFaturaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("faturas/{id:guid}")]
        [AbacAuthorize("ServicosComerciaisFaturas", "Excluir")]
        public async Task<IActionResult> ExcluirFatura(Guid id)
        {
            var result = await _mediator.Send(new ExcluirServicoFaturaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
