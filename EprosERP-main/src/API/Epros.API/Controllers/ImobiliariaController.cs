using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// IMO-001 (Gestao Imobiliaria): imoveis, contratos de servico e locacoes.
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "ImobiliariaGestao").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/imobiliaria")]
    [Produces("application/json")]
    public class ImobiliariaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ImobiliariaController(IMediator mediator) => _mediator = mediator;

        // ==================== Imovel ====================

        [HttpGet("imoveis")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarImoveis()
            => Ok(await _mediator.Send(new ListarImoveisQuery()));

        [HttpGet("imoveis/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterImovel(Guid id)
        {
            var result = await _mediator.Send(new ObterImovelQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("imoveis")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarImovel([FromBody] CriarImovelCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("imoveis/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirImovel(Guid id)
        {
            var result = await _mediator.Send(new ExcluirImovelCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Contrato de servico ====================

        [HttpGet("contratos-servico")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarContratosServico()
            => Ok(await _mediator.Send(new ListarContratosServicoQuery()));

        [HttpPost("contratos-servico")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarContratoServico([FromBody] CriarContratoServicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("contratos-servico/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirContratoServico(Guid id)
        {
            var result = await _mediator.Send(new ExcluirContratoServicoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Locacao ====================

        [HttpGet("locacoes")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarLocacoes([FromQuery] DateTime? periodoDe, [FromQuery] DateTime? periodoAte)
            => Ok(await _mediator.Send(new ListarLocacoesQuery(periodoDe, periodoAte)));

        [HttpPost("locacoes")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarLocacao([FromBody] CriarLocacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("locacoes/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirLocacao(Guid id)
        {
            var result = await _mediator.Send(new ExcluirLocacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("locacoes/{id:guid}/resumo-aluguel")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterResumoAluguel(Guid id)
        {
            var result = await _mediator.Send(new ObterResumoAluguelQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
