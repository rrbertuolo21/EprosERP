using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Plataforma.Analytics;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers.Plataforma
{
    /// <summary>
    /// PLT · ANALYTICS (PD-06) — catálogo de KPIs versionado, snapshots (read-side), dashboards
    /// configuráveis e exportação auditada. Controller fino. ABAC desabilitado por padrão.
    /// </summary>
    [ApiController]
    [Route("api/v1/plt/analytics")]
    [Produces("application/json")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AnalyticsController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult r) => r.Sucesso ? Ok(r) : UnprocessableEntity(r);

        [HttpGet("kpis")]
        [AbacAuthorize("AnalyticsKpis", "Ler")]
        public async Task<IActionResult> ListarKpis([FromQuery] string? categoria, [FromQuery] bool apenasAtivos = true)
            => Ok(await _mediator.Send(new ObterKpisQuery(categoria, apenasAtivos)));

        [HttpPost("kpis")]
        [AbacAuthorize("AnalyticsKpis", "Definir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirKpi([FromBody] DefinirKpiCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("snapshots")]
        [AbacAuthorize("AnalyticsSnapshots", "Registrar")]
        public async Task<ActionResult<CommandResult>> RegistrarSnapshot([FromBody] RegistrarSnapshotMetricaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("kpis/{kpiCodigo}/serie")]
        [AbacAuthorize("AnalyticsSnapshots", "Ler")]
        public async Task<IActionResult> Serie(string kpiCodigo, [FromQuery] string? dimensao)
            => Ok(await _mediator.Send(new ObterSerieMetricaQuery(kpiCodigo, dimensao)));

        [HttpGet("dashboards")]
        [AbacAuthorize("AnalyticsDashboards", "Ler")]
        public async Task<IActionResult> ListarDashboards()
            => Ok(await _mediator.Send(new ObterDashboardsQuery()));

        [HttpGet("dashboards/{id}")]
        [AbacAuthorize("AnalyticsDashboards", "Ler")]
        public async Task<IActionResult> ObterDashboard(Guid id)
            => Ok(await _mediator.Send(new ObterDashboardPorIdQuery(id)));

        [HttpPost("dashboards")]
        [AbacAuthorize("AnalyticsDashboards", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarDashboard([FromBody] CriarDashboardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("dashboards/widgets")]
        [AbacAuthorize("AnalyticsDashboards", "Editar")]
        public async Task<ActionResult<CommandResult>> AdicionarWidget([FromBody] AdicionarWidgetDashboardCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("exportacoes")]
        [AbacAuthorize("AnalyticsExportacao", "Ler")]
        public async Task<IActionResult> ListarExportacoes()
            => Ok(await _mediator.Send(new ObterExportacoesAnalyticsQuery()));

        [HttpPost("exportacoes")]
        [AbacAuthorize("AnalyticsExportacao", "Solicitar")]
        public async Task<ActionResult<CommandResult>> SolicitarExportacao([FromBody] SolicitarExportacaoAnalyticsCommand command)
            => Resultado(await _mediator.Send(command));
    }
}
