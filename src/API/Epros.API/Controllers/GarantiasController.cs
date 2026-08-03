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
    /// Garantias (VEN-GAR): políticas de garantia, coberturas aplicadas e consulta de pós-venda.
    /// Controller fino: apenas MediatR. AutZ ABAC nega por padrão (submódulo novo, sem permissão semeada).
    /// Fonte: EF_7_VENDAS_GARANTIAS_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/garantias")]
    public class GarantiasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public GarantiasController(IMediator mediator) => _mediator = mediator;

        [HttpGet("politicas")]
        [AbacAuthorize("GarantiaPoliticas", "Ler")]
        public async Task<IActionResult> ListarPoliticas([FromQuery] bool apenasAtivas = true, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarGarantiaPoliticasQuery(apenasAtivas, pagina, tamanhoPagina)));

        [HttpGet("politicas/{id:guid}")]
        [AbacAuthorize("GarantiaPoliticas", "Ler")]
        public async Task<IActionResult> ObterPolitica(Guid id)
        {
            var result = await _mediator.Send(new ObterGarantiaPoliticaPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("politicas")]
        [AbacAuthorize("GarantiaPoliticas", "Criar")]
        public async Task<IActionResult> CriarPolitica([FromBody] CriarGarantiaPoliticaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("politicas/{id:guid}")]
        [AbacAuthorize("GarantiaPoliticas", "Editar")]
        public async Task<IActionResult> AtualizarPolitica(Guid id, [FromBody] AtualizarGarantiaPoliticaCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("politicas/{id:guid}/inativar")]
        [AbacAuthorize("GarantiaPoliticas", "Editar")]
        public async Task<IActionResult> InativarPolitica(Guid id)
        {
            var result = await _mediator.Send(new InativarGarantiaPoliticaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("coberturas")]
        [AbacAuthorize("GarantiaCoberturas", "Criar")]
        public async Task<IActionResult> AplicarCobertura([FromBody] AplicarGarantiaCoberturaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpGet("coberturas")]
        [AbacAuthorize("GarantiaCoberturas", "Ler")]
        public async Task<IActionResult> ConsultarCobertura([FromQuery] Guid? vendaId, [FromQuery] Guid? produtoId, [FromQuery] Guid? clienteId, [FromQuery] string? numeroSerieLote)
            => Ok(await _mediator.Send(new ConsultarGarantiaCoberturaQuery(vendaId, produtoId, clienteId, numeroSerieLote)));

        /// <summary>GAR-016: registra leitura de uso (km/horas) numa cobertura e reapura a situação (vencimento por uso).</summary>
        [HttpPost("coberturas/{id:guid}/registrar-uso")]
        [AbacAuthorize("GarantiaCoberturas", "Editar")]
        public async Task<IActionResult> RegistrarUso(Guid id, [FromBody] RegistrarUsoGarantiaCoberturaCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
