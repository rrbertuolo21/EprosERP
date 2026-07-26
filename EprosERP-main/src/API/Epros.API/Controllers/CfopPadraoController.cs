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
    [Route("api/v1/cfop-padrao")]
    [Produces("application/json")]
    public class CfopPadraoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CfopPadraoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var query = new ListarCfopPadroesQuery(localizar, pagina, tamanhoPagina);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("obter-por-cfop/{cfop}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorCfop(int cfop)
        {
            var query = new ObterCfopPadraoPorCodigoQuery(cfop);
            var result = await _mediator.Send(query);
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Loader (carga em massa) da tabela de CFOP-padrão a partir de arquivo CSV/TXT.
        /// Insere apenas códigos ainda não presentes (idempotente). Fiel ao legado
        /// <c>POST api/v1/cfop-padrao/atualizar</c>.
        /// </summary>
        /// <param name="arquivo">Arquivo CSV/TXT com os CFOP-padrão.</param>
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
            var result = await _mediator.Send(new AtualizarTabelaCfopPadraoCommand(stream, arquivo.FileName), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
