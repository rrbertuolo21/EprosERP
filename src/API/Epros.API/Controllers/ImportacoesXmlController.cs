using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Histórico dos XMLs importados (NF-e de entrada/saída). Controller fino (só MediatR).
    /// Compatível com o legado <c>api/v1/importacao-xmls</c> (aqui sob compras).
    /// </summary>
    [ApiController]
    [Route("api/v1/compras/importacoes-xml")]
    [Produces("application/json")]
    public class ImportacoesXmlController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImportacoesXmlController(IMediator mediator) => _mediator = mediator;

        /// <summary>Listagem/histórico paginado das importações de XML, com ordenação.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string ordenarPor = "data",
            [FromQuery] bool ordemAscendente = false,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarImportacoesXmlQuery(ordenarPor, ordemAscendente, pagina, tamanhoPagina),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>Detalhe de uma importação XML por Id (inclui o conteúdo do XML).</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterImportacaoXmlPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
