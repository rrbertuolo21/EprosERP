using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Consulta HTTP do FatoGeradorFinanceiro (origem/rastreabilidade de títulos). Controller fino.
    /// No legado a entidade não possuía rota HTTP dedicada (só navegação a partir de ContasA*).
    /// </summary>
    [ApiController]
    [Route("api/v1/financeiro/fatos-geradores")]
    [Produces("application/json")]
    public class FatosGeradoresFinanceirosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FatosGeradoresFinanceirosController(IMediator mediator) => _mediator = mediator;

        /// <summary>Listagem paginada, filtrável por origem / vendaId / compraId.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] int? origem,
            [FromQuery] Guid? vendaId,
            [FromQuery] Guid? compraId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarFatosGeradoresFinanceirosQuery(origem, vendaId, compraId, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>Obtém um fato gerador por Id, com os títulos vinculados.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterFatoGeradorFinanceiroPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
