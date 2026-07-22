using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Histórico de reajuste de preço de produtos. Controller fino (só MediatR).
    /// Compatível com o legado <c>api/v1/produtos-historicos-reajustes</c>.
    /// </summary>
    [ApiController]
    [Route("api/v1/produtos-historicos-reajustes")]
    [Produces("application/json")]
    public class ProdutosHistoricosReajustesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProdutosHistoricosReajustesController(IMediator mediator) => _mediator = mediator;

        /// <summary>Listagem paginada do histórico de reajustes, com filtros localizar / dataAlteracao / ativo.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] DateTime? dataAlteracao,
            [FromQuery] bool ativo = true,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarProdutosHistoricosReajustesQuery(localizar, dataAlteracao, ativo, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>Histórico de reajustes de um produto específico.</summary>
        [HttpGet("localizar-por-id-produto/{idProduto:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorProduto(Guid idProduto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ListarProdutosHistoricosReajustesPorProdutoQuery(idProduto), cancellationToken);
            return Ok(result);
        }

        /// <summary>Obtém um registro de histórico de reajuste por Id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterProdutoHistoricoReajustePorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Registra um reajuste de valor (venda/compra) de um produto e grava o histórico.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Registrar([FromBody] RegistrarProdutoReajusteCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }
    }
}
