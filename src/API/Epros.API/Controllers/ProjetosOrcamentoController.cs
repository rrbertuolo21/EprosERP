using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRJ-ORC (Planejamento e Orcamento). Orcamento/baseline, marcos e workflow de aprovacao.
    /// ABAC nega por padrao (submodulo novo sobe desabilitado).
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/orcamento")]
    [Produces("application/json")]
    public class ProjetosOrcamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosOrcamentoController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [AbacAuthorize("ProjetosOrcamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarOrcamentoProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/marcos")]
        [AbacAuthorize("ProjetosOrcamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarMarco(Guid id, [FromBody] AdicionarMarcoRequest request)
        {
            var result = await _mediator.Send(new AdicionarMarcoOrcamentarioCommand(id, request.Titulo, request.Custo, request.DataInicio, request.DataFim, request.Resumo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AdicionarMarcoRequest(string Titulo, decimal Custo, DateTime DataInicio, DateTime DataFim, string? Resumo);

        [HttpPost("{id:guid}/marcos/{marcoId:guid}/progresso")]
        [AbacAuthorize("ProjetosOrcamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarProgressoMarco(Guid id, Guid marcoId, [FromBody] AtualizarProgressoMarcoRequest request)
        {
            var result = await _mediator.Send(new AtualizarProgressoMarcoCommand(id, marcoId, request.Progresso, request.Status));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AtualizarProgressoMarcoRequest(int Progresso, EMarcoStatus Status);

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosOrcamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterOrcamentoProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosOrcamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarOrcamentoProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>DP-ORC-002: congela uma baseline imutável (budget + marcos) do orçamento aprovado.</summary>
        [HttpPost("{id:guid}/baseline")]
        [AbacAuthorize("ProjetosOrcamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CongelarBaseline(Guid id, [FromBody] CongelarBaselineRequest? request)
        {
            var result = await _mediator.Send(new CongelarBaselineOrcamentoCommand(id, request?.Motivo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record CongelarBaselineRequest(string? Motivo);

        [HttpGet("{id:guid}/baselines")]
        [AbacAuthorize("ProjetosOrcamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarBaselines(Guid id)
        {
            var result = await _mediator.Send(new ObterBaselinesOrcamentoQuery(id));
            return Ok(result);
        }

        /// <summary>DP-ORC-004/005: EVM do orçamento (PV/EV/AC → CPI/SPI/EAC...). // valida-contador (método de EV).</summary>
        [HttpGet("{id:guid}/evm")]
        [AbacAuthorize("ProjetosOrcamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterEvm(Guid id, [FromQuery] decimal actualCost, [FromQuery] decimal? percentualPlanejado, [FromQuery] decimal? percentualConcluido)
        {
            var result = await _mediator.Send(new ObterEvmOrcamentoQuery(id, actualCost, percentualPlanejado, percentualConcluido));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpGet("projeto/{projetoId:guid}")]
        [AbacAuthorize("ProjetosOrcamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPorProjeto(Guid projetoId)
        {
            var result = await _mediator.Send(new ObterOrcamentosPorProjetoQuery(projetoId));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProjetosOrcamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterOrcamentoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
