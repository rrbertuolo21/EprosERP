using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Models;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/vendas")]
    [Produces("application/json")]
    public class VendasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sync")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Sincronizar([FromBody] SincronizarVendasCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("caixas/sync")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SincronizarCaixas([FromBody] SincronizarCaixasCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("caixas/abrir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AbrirCaixa([FromBody] AbrirCaixaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("caixas/fechar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FecharCaixa([FromBody] FecharCaixaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("caixas/movimentar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> MovimentarCaixa([FromBody] RegistrarCaixaMovimentoCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpGet("caixas/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterCaixaStatus([FromQuery] string operadorId, System.Threading.CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCaixaStatusQuery(operadorId), cancellationToken);
            return Ok(result);
        }

        [HttpGet("caixas/{id:guid}/detalhado")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterCaixaDetalhado(System.Guid id, System.Threading.CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCaixaDetalhadoQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? status,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            System.Threading.CancellationToken cancellationToken = default)
        {
            var query = new ListarVendasQuery(status, pagina, tamanhoPagina);
            var result = await _mediator.Send(query, cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(System.Guid id, System.Threading.CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterVendaPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegistrarVenda([FromBody] RegistrarVendaCommand command, System.Threading.CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id}/cancelar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cancelar(System.Guid id, [FromBody] CancelarVendaRequest request)
        {
            var command = new CancelarVendaCommand(id, request?.Motivo ?? "Cancelamento solicitado via API");
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        // ---------- Ações de venda (Onda 7 — porte de api/v1/vendas do legado) ----------

        /// <summary>
        /// Baixa o PDF do cupom não fiscal (MEI) previamente gerado para a venda.
        /// Porte de VendaController.baixar-cupom-nao-fiscal.
        /// </summary>
        [HttpGet("baixar-cupom-nao-fiscal")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BaixarCupomNaoFiscal([FromQuery] Guid vendaId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BaixarCupomNaoFiscalCommand(vendaId), cancellationToken);
            if (!result.Sucesso) return BadRequest(result);

            var arquivo = (ArquivoPdfResult)result.Dados!;
            return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeArquivo);
        }

        /// <summary>
        /// Obtém as informações complementares consolidadas dos produtos informados (por NCM/empresa).
        /// Porte de VendaController.obter-informacoes-complementares-por-produtos-ids.
        /// </summary>
        /// <param name="empresaId">Empresa ativa (o legado usava a empresa logada).</param>
        /// <param name="produtosIds">Ids dos produtos.</param>
        [HttpGet("obter-informacoes-complementares-por-produtos-ids")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObterInformacoesComplementaresPorProdutos(
            [FromQuery] Guid empresaId,
            [FromQuery] List<Guid> produtosIds,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterInformacoesComplementaresPorProdutosQuery(empresaId, produtosIds), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Cria uma venda de NF-e simplificada (cabeçalho). Porte de VendaController.nfe-simplificado (POST).</summary>
        [HttpPost("nfe-simplificado")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CriarNfeSimplificado([FromBody] CriarVendaSimplificadaNfeCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Atualiza uma venda de NF-e simplificada. Porte de VendaController.nfe-simplificado (PUT).</summary>
        [HttpPut("nfe-simplificado/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarNfeSimplificado(Guid id, [FromBody] AtualizarVendaSimplificadaNfeCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Marca a NF-e simplificada para transmissão (inclui série/número e status Transmitido).
        /// Porte de VendaController.nfe-simplificado-transmitir.
        /// </summary>
        [HttpPost("nfe-simplificado-transmitir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> TransmitirNfeSimplificado([FromBody] TransmitirVendaSimplificadaNfeCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    public record CancelarVendaRequest(string Motivo);
}
