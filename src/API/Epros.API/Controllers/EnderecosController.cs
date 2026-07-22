using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Consulta de endereços de pessoas. A criação/edição de endereço ocorre pelo
    /// agregado Pessoa (CriarPessoaCommand/AtualizarPessoaCommand); aqui expõe-se
    /// somente leitura, no mesmo estilo dos GET de PessoasController/EmpresasController.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/cadastros/enderecos")]
    public class EnderecosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EnderecosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterEnderecoPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] Guid? pessoaId,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarEnderecosQuery(pessoaId, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }
    }
}
