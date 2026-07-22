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
    [Produces("application/json")]
    [Route("api/v1/fiscal/ncm-tributacoes")]
    public class NcmTributacoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NcmTributacoesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] Guid? tributarioGrupoId,
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarNcmTributacoesQuery(tributarioGrupoId, localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterNcmTributacaoPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarNcmTributacaoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarNcmTributacaoCommand command, CancellationToken cancellationToken)
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
            var result = await _mediator.Send(new DeletarNcmTributacaoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- IBS / CBS -----

        [HttpPut("{id:guid}/ibs-cbs")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirIbsCbs(Guid id, [FromBody] DefinirIbsCbsNcmTributacaoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Substituição Tributária (St) -----

        [HttpPost("{id:guid}/sts")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarSt(Guid id, [FromBody] AdicionarNcmTributacaoStCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/sts/{stId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AlterarSt(Guid id, Guid stId, [FromBody] AlterarNcmTributacaoStCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId || stId != command.NcmTributacaoStId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/sts/{stId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarSt(Guid id, Guid stId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarNcmTributacaoStCommand(id, stId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Fundo de Combate à Pobreza -----

        [HttpPost("{id:guid}/fundos-combate-pobreza")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarFundoCombatePobreza(Guid id, [FromBody] AdicionarNcmTributacaoFundoCombatePobrezaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/fundos-combate-pobreza/{fundoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AlterarFundoCombatePobreza(Guid id, Guid fundoId, [FromBody] AlterarNcmTributacaoFundoCombatePobrezaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId || fundoId != command.FundoCombatePobrezaId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/fundos-combate-pobreza/{fundoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarFundoCombatePobreza(Guid id, Guid fundoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarNcmTributacaoFundoCombatePobrezaCommand(id, fundoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Configurações (vínculo NCM) -----

        [HttpPost("{id:guid}/configuracoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarConfiguracao(Guid id, [FromBody] AdicionarNcmConfiguracaoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/configuracoes/{configuracaoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AlterarConfiguracao(Guid id, Guid configuracaoId, [FromBody] AlterarNcmConfiguracaoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.NcmTributacaoId || configuracaoId != command.NcmConfiguracaoId)
            {
                return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/configuracoes/{configuracaoId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarConfiguracao(Guid id, Guid configuracaoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarNcmConfiguracaoCommand(id, configuracaoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
