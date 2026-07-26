using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Shared.Application.Models;
using System.Threading;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/codigos-servicos-sefaz")]
    [Produces("application/json")]
    public class CodigosServicosSefazController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CodigosServicosSefazController(IMediator mediator)
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
            var query = new ListarCodigosServicosSefazQuery(localizar, pagina, tamanhoPagina);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var query = new ObterCodigoServicoSefazPorIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Loader (carga em massa) da tabela de Códigos de Serviço da SEFAZ a partir de arquivo CSV/TXT
        /// (<c>codigo;descricao</c>). Insere apenas códigos ainda não presentes (idempotente). Fiel ao
        /// legado <c>POST api/v1/codigos-servicos-sefaz/atualizar</c>.
        /// </summary>
        /// <param name="arquivo">Arquivo CSV/TXT com os códigos de serviço.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>A quantidade de registros importados.</returns>
        /// <response code="200">Tabela atualizada.</response>
        /// <response code="422">Arquivo ausente, vazio ou inválido.</response>
        [HttpPost("atualizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CommandResult>> AtualizarTabela(IFormFile arquivo, CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
                return UnprocessableEntity(CommandResult.Falha("Nenhum arquivo foi enviado."));

            await using var stream = arquivo.OpenReadStream();
            var result = await _mediator.Send(new AtualizarTabelaCodigoServicoSefazCommand(stream, arquivo.FileName), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
