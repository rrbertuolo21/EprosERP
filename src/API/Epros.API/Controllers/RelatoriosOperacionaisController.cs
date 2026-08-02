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
    /// RPT-OPB — Relatorios Operacionais (Openbook). Somente leitura (query-side).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "RelatoriosOperacionais").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// Isolamento por empresa/tenant e aplicado pelo global query filter do ContextBase (EF RN-001).
    /// </summary>
    [ApiController]
    [Route("api/v1/relatorios/operacionais")]
    [Produces("application/json")]
    public class RelatoriosOperacionaisController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RelatoriosOperacionaisController(IMediator mediator) => _mediator = mediator;

        // ---- ESTOQUE ----------------------------------------------------
        [HttpGet("estoque/posicao")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> PosicaoEstoque(
            [FromQuery] Guid? categoriaId, [FromQuery] Guid? produtoId, [FromQuery] bool somenteAbaixoMinimo = false)
            => Ok(await _mediator.Send(new PosicaoEstoqueQuery(categoriaId, produtoId, somenteAbaixoMinimo)));

        [HttpGet("estoque/giro")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GiroEstoque(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] Guid? produtoId)
            => Ok(await _mediator.Send(new GiroEstoqueQuery(inicio, fim, produtoId)));

        // ---- VENDAS -----------------------------------------------------
        [HttpGet("vendas/por-periodo")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> VendasPorPeriodo(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] bool incluirCanceladas = false)
            => Ok(await _mediator.Send(new VendasPorPeriodoQuery(inicio, fim, incluirCanceladas)));

        [HttpGet("vendas/por-produto")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> VendasPorProduto(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] Guid? produtoId, [FromQuery] bool incluirCanceladas = false)
            => Ok(await _mediator.Send(new VendasPorProdutoQuery(inicio, fim, produtoId, incluirCanceladas)));

        [HttpGet("vendas/por-cliente")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> VendasPorCliente(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] Guid? clienteId, [FromQuery] bool incluirCanceladas = false)
            => Ok(await _mediator.Send(new VendasPorClienteQuery(inicio, fim, clienteId, incluirCanceladas)));

        // ---- COMPRAS ----------------------------------------------------
        [HttpGet("compras/por-item")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ComprasPorItem(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] Guid? produtoId, [FromQuery] bool incluirCanceladas = false)
            => Ok(await _mediator.Send(new ComprasPorItemQuery(inicio, fim, produtoId, incluirCanceladas)));

        [HttpGet("compras/por-parceiro")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ComprasPorParceiro(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] string? fornecedorCnpj, [FromQuery] bool incluirCanceladas = false)
            => Ok(await _mediator.Send(new ComprasPorParceiroQuery(inicio, fim, fornecedorCnpj, incluirCanceladas)));

        // ---- FINANCEIRO -------------------------------------------------
        [HttpGet("financeiro/aging-receber")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> AgingReceber([FromQuery] DateTime? dataReferencia)
            => Ok(await _mediator.Send(new AgingContasReceberQuery(dataReferencia)));

        [HttpGet("financeiro/aging-pagar")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> AgingPagar([FromQuery] DateTime? dataReferencia)
            => Ok(await _mediator.Send(new AgingContasPagarQuery(dataReferencia)));

        [HttpGet("financeiro/receber")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResumoReceber(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] int? situacao, [FromQuery] bool somenteEmAberto = false)
            => Ok(await _mediator.Send(new ResumoContasReceberQuery(inicio, fim, situacao, somenteEmAberto)));

        [HttpGet("financeiro/pagar")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResumoPagar(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] int? situacao, [FromQuery] bool somenteEmAberto = false)
            => Ok(await _mediator.Send(new ResumoContasPagarQuery(inicio, fim, situacao, somenteEmAberto)));

        [HttpGet("financeiro/pagamentos-recebidos")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> PagamentosRecebidos([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
            => Ok(await _mediator.Send(new PagamentosRecebidosQuery(inicio, fim)));

        [HttpGet("financeiro/pagamentos-efetuados")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> PagamentosEfetuados([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
            => Ok(await _mediator.Send(new PagamentosEfetuadosQuery(inicio, fim)));

        [HttpGet("financeiro/fluxo-caixa")]
        [AbacAuthorize("RelatoriosOperacionais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> FluxoCaixa(
            [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, [FromQuery] bool incluirPlanejamento = false)
            => Ok(await _mediator.Send(new FluxoCaixaQuery(inicio, fim, incluirPlanejamento)));
    }
}
