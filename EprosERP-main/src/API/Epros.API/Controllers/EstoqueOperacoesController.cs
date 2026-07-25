using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Operações de movimentação e ajustes de estoque (EST-MVM-001): ajuste, avaria, transferência,
    /// requisição interna e importação de saldo inicial. Controller fino: apenas MediatR.
    /// Protegido por ABAC conforme EF §18. Filtro de tenant é global (ContextBase).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque")]
    public class EstoqueOperacoesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EstoqueOperacoesController(IMediator mediator) => _mediator = mediator;

        /// <summary>Cria e aplica um ajuste de estoque (MVM-025). Exige motivo.</summary>
        [HttpPost("ajustes")]
        [AbacAuthorize("EstoqueAjuste", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarAjuste([FromBody] CriarAjusteEstoqueCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Registra e aplica avaria/perda como saída controlada (MVM-026).</summary>
        [HttpPost("avarias")]
        [AbacAuthorize("EstoqueAjuste", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarAvaria([FromBody] CriarAvariaEstoqueCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Cria transferência entre locais (MVM-020/021).</summary>
        [HttpPost("transferencias")]
        [AbacAuthorize("EstoqueTransferencia", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarTransferencia([FromBody] CriarTransferenciaEstoqueCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Cria requisição interna (MVM-027).</summary>
        [HttpPost("requisicoes-internas")]
        [AbacAuthorize("EstoqueMovimentacao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarRequisicao([FromBody] CriarRequisicaoInternaCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Importa/registra saldo inicial (MVM-023/024).</summary>
        [HttpPost("saldo-inicial/importar")]
        [AbacAuthorize("EstoqueImportacaoSaldo", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ImportarSaldoInicial([FromBody] ImportarSaldoInicialCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
