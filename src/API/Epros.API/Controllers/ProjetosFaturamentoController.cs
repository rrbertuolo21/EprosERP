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
    /// PRJ-FAT (Faturamento de Projeto). Ciclo do faturamento e itens faturaveis.
    /// A aprovacao (Ativo) publica evento ProjetoFaturado; o titulo de Contas a Receber pertence ao Financeiro.
    /// ABAC nega por padrao (submodulo novo sobe desabilitado).
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/faturamento")]
    [Produces("application/json")]
    public class ProjetosFaturamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosFaturamentoController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [AbacAuthorize("ProjetosFaturamento", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarFaturamentoProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ProjetosFaturamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarItem(Guid id, [FromBody] AdicionarItemRequest request)
        {
            var result = await _mediator.Send(new AdicionarItemFaturamentoCommand(
                id, request.Sequencia, request.Quantidade, request.Observacao, request.TipoItem,
                request.ValorUnitario, request.ValorTotal, request.OrigemTipo, request.OrigemId, request.Reembolsavel));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AdicionarItemRequest(
            int Sequencia, decimal? Quantidade, string? Observacao, ETipoItemFaturamento? TipoItem,
            decimal? ValorUnitario, decimal? ValorTotal, string? OrigemTipo, Guid? OrigemId, bool Reembolsavel = false);

        /// <summary>
        /// DP-FAT-004/008 — aplica tributos/retenções fiscais (ISS/IRRF/INSS/PIS/COFINS/CSLL) ao faturamento.
        /// // valida-contador: valores/alíquotas e incidência por serviço de projeto vêm do contador.
        /// </summary>
        [HttpPost("{id:guid}/tributacao")]
        [AbacAuthorize("ProjetosFaturamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AplicarTributacao(Guid id, [FromBody] TributacaoRequest request)
        {
            var result = await _mediator.Send(new AplicarTributacaoFaturamentoCommand(
                id, request.ValorIss, request.ValorIrrf, request.ValorInss,
                request.ValorPis, request.ValorCofins, request.ValorCsll));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record TributacaoRequest(
            decimal? ValorIss, decimal? ValorIrrf, decimal? ValorInss,
            decimal? ValorPis, decimal? ValorCofins, decimal? ValorCsll);

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosFaturamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterFaturamentoProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosFaturamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarFaturamentoProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/rejeitar")]
        [AbacAuthorize("ProjetosFaturamento", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Rejeitar(Guid id, [FromBody] RejeitarRequest request)
        {
            var result = await _mediator.Send(new RejeitarFaturamentoProjetoCommand(id, request.Motivo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RejeitarRequest(string Motivo);

        [HttpGet]
        [AbacAuthorize("ProjetosFaturamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ObterFaturamentosQuery(status, pagina, tamanhoPagina));
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ProjetosFaturamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterFaturamentoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }
}
