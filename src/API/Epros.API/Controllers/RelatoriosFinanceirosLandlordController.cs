using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Queries;

namespace Epros.API.Controllers
{
    /// <summary>
    /// 1.08G — Relatórios financeiros AGREGADOS do Landlord (Siser cobrando os tenants).
    ///
    /// ⛔ Escopo: apenas AGREGADOS FACTUAIS (regime de CAIXA) sobre dados que a 1.08A já grava. NÃO há
    /// reconhecimento de receita por competência/diferimento, NEM apuração fiscal de comissão — isso é
    /// ONDA 2 (skill de negócio + validação de contador).
    ///
    /// Segurança: AbacAuthorize (operador interno) no ingresso + guard de operador interno no handler
    /// (fail-closed). Cada endpoint aceita um intervalo de datas [inicio, fim] inclusivo.
    /// </summary>
    [ApiController]
    [Route("api/v1/plataforma/relatorios-financeiros")]
    [Produces("application/json")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    // 1.11 decisão #5 — área comercial: faixa de Suporte Negócio (SuporteTecnico é negado; PrimaryAdmin passa).
    [AbacAuthorize(SuperAdminSeguranca.RecursoSuporteComercial, "Configurar")]
    public class RelatoriosFinanceirosLandlordController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RelatoriosFinanceirosLandlordController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Receita por período (CAIXA): faturas pagas por mês — bruto/tarifa/líquido.</summary>
        [HttpGet("receita")]
        [ProducesResponseType(typeof(RelatorioReceitaPeriodoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReceitaPorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var result = await _mediator.Send(new ObterRelatorioReceitaPorPeriodoQuery(inicio, fim));
            return Ok(result);
        }

        /// <summary>Inadimplência por período: faturas vencidas e não pagas, por mês/tenant.</summary>
        [HttpGet("inadimplencia")]
        [ProducesResponseType(typeof(RelatorioInadimplenciaPeriodoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> InadimplenciaPorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var result = await _mediator.Send(new ObterRelatorioInadimplenciaPorPeriodoQuery(inicio, fim));
            return Ok(result);
        }

        /// <summary>Comissão por período (CAIXA): soma factual de comissão revenda/vendedor, por mês/revenda.</summary>
        [HttpGet("comissao")]
        [ProducesResponseType(typeof(RelatorioComissaoPeriodoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ComissaoPorPeriodo([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            var result = await _mediator.Send(new ObterRelatorioComissaoPorPeriodoQuery(inicio, fim));
            return Ok(result);
        }
    }
}
