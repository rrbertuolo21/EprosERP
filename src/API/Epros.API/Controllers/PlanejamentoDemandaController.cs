using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Planejamento de Demanda (VEN-PDM): previsões por período/produto, cenários, aprovação e
    /// publicação para estoque/produção. Controller fino: apenas MediatR. AutZ ABAC nega por padrão.
    /// Fonte: EF_7_VENDAS_PLANEJAMENTO_DE_DEMANDA_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/demanda")]
    public class PlanejamentoDemandaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PlanejamentoDemandaController(IMediator mediator) => _mediator = mediator;

        [HttpGet("previsoes")]
        [AbacAuthorize("DemandaPrevisoes", "Ler")]
        public async Task<IActionResult> ListarPrevisoes([FromQuery] EDemandaStatus? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarDemandaPrevisoesQuery(status, pagina, tamanhoPagina)));

        [HttpGet("previsoes/{id:guid}")]
        [AbacAuthorize("DemandaPrevisoes", "Ler")]
        public async Task<IActionResult> ObterPrevisao(Guid id)
        {
            var result = await _mediator.Send(new ObterDemandaPrevisaoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("previsoes")]
        [AbacAuthorize("DemandaPrevisoes", "Criar")]
        public async Task<IActionResult> CriarPrevisao([FromBody] CriarDemandaPrevisaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("previsoes/itens")]
        [AbacAuthorize("DemandaPrevisoes", "Editar")]
        public async Task<IActionResult> AdicionarItem([FromBody] AdicionarDemandaItemCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("itens/{id:guid}/ajustar")]
        [AbacAuthorize("DemandaPrevisoes", "Editar")]
        public async Task<IActionResult> AjustarItem(Guid id, [FromBody] AjustarDemandaItemCommand command)
        {
            if (id != command.ItemId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("previsoes/cenarios")]
        [AbacAuthorize("DemandaPrevisoes", "Editar")]
        public async Task<IActionResult> CriarCenario([FromBody] CriarDemandaCenarioCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("previsoes/{id:guid}/aprovar")]
        [AbacAuthorize("DemandaPrevisoes", "Aprovar")]
        public async Task<IActionResult> Aprovar(Guid id, [FromBody] AprovarDemandaPrevisaoCommand command)
        {
            if (id != command.PrevisaoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("previsoes/{id:guid}/publicar")]
        [AbacAuthorize("DemandaPrevisoes", "Publicar")]
        public async Task<IActionResult> Publicar(Guid id)
        {
            var result = await _mediator.Send(new PublicarDemandaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
