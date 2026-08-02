using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Relatorios.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// RPT-ONM — BI (OneManager): KPIs gerenciais e series temporais multi-modulo.
    /// Somente leitura (query-side). Protegido por ABAC (recurso "RelatoriosBi").
    /// A capacidade "relatoriosbi:ler" e semeada no boot por CapacidadeCatalogoSeeder (auto-descoberta
    /// por reflexao deste [AbacAuthorize]) e ligada ao papel Administrador, que o admin do tenant recebe.
    /// Isolamento por tenant via global query filter (EF RN-BI-001).
    /// </summary>
    [ApiController]
    [Route("api/v1/relatorios/bi")]
    [Produces("application/json")]
    public class RelatoriosBiController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RelatoriosBiController(IMediator mediator) => _mediator = mediator;

        [HttpGet("kpi/faturamento")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Faturamento([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
            => Ok(await _mediator.Send(new KpiFaturamentoQuery(inicio, fim)));

        [HttpGet("serie/faturamento")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> SerieFaturamento(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] string granularidade = "Dia")
            => Ok(await _mediator.Send(new SerieFaturamentoQuery(inicio, fim, granularidade)));

        [HttpGet("kpi/margem")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Margem([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
            => Ok(await _mediator.Send(new KpiMargemQuery(inicio, fim)));

        [HttpGet("kpi/inadimplencia")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Inadimplencia([FromQuery] DateTime? dataReferencia)
            => Ok(await _mediator.Send(new KpiInadimplenciaQuery(dataReferencia)));

        [HttpGet("kpi/ruptura")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Ruptura()
            => Ok(await _mediator.Send(new KpiRupturaQuery()));

        [HttpGet("top-produtos")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> TopProdutos(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] int topN = 10, [FromQuery] string criterio = "Valor")
            => Ok(await _mediator.Send(new TopProdutosQuery(inicio, fim, topN, criterio)));

        [HttpGet("painel-gerencial")]
        [AbacAuthorize("RelatoriosBi", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> PainelGerencial([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
            => Ok(await _mediator.Send(new PainelGerencialQuery(inicio, fim)));
    }
}
