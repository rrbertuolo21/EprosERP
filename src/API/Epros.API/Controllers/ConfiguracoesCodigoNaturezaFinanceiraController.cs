using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Módulo Financeiro — Configuração de Código de Natureza Financeira.
    /// Base: /api/v1/configuracao-codigo-naturezas-financeiras (compatível com o legado).
    /// </summary>
    [ApiController]
    [Route("api/v1/configuracao-codigo-naturezas-financeiras")]
    [Produces("application/json")]
    public class ConfiguracoesCodigoNaturezaFinanceiraController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConfiguracoesCodigoNaturezaFinanceiraController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarConfiguracoesCodigoNaturezaFinanceiraQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterConfiguracaoCodigoNaturezaFinanceiraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarConfiguracaoCodigoNaturezaFinanceiraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarConfiguracaoCodigoNaturezaFinanceiraCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("ID da rota não coincide com o do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarConfiguracaoCodigoNaturezaFinanceiraCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
