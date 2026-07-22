using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/fiscal/ncms")]
    [Produces("application/json")]
    public class NcmsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NcmsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarNcmsQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterNcmPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarNcmCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarNcmCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarNcmCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Loader (carga em massa) da tabela de NCM a partir do arquivo oficial (JSON de NCMs vigentes).
        /// Insere apenas NCMs de 8 dígitos ainda não presentes (idempotente). Fiel ao legado
        /// <c>POST api/v1/ncm/atualizar</c>.
        /// </summary>
        /// <param name="arquivo">Arquivo enviado (JSON <c>{ "Nomenclaturas": [...] }</c>).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>A quantidade de registros importados.</returns>
        /// <response code="200">Tabela atualizada.</response>
        /// <response code="422">Arquivo ausente, vazio ou inválido.</response>
        [HttpPost("atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarTabela(IFormFile arquivo, CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
                return UnprocessableEntity(CommandResult.Falha("Nenhum arquivo foi enviado."));

            await using var stream = arquivo.OpenReadStream();
            var result = await _mediator.Send(new AtualizarTabelaNcmCommand(stream, arquivo.FileName), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
