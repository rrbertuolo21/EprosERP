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
    /// Logística de Saída (VEN-LDS): expedições, local de entrega, transporte e baixa de entrega.
    /// Controller fino: apenas MediatR. AutZ ABAC nega por padrão (submódulo novo, sem permissão semeada).
    /// FISCAL: local de entrega e transporte alimentam o documento fiscal via módulo Fiscal.
    /// Fonte: EF_7_VENDAS_LOGISTICA_DE_SAIDA_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/expedicoes")]
    public class LogisticaSaidaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public LogisticaSaidaController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("Expedicoes", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] Guid? pedidoId, [FromQuery] EExpedicaoStatus? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarExpedicoesQuery(pedidoId, status, pagina, tamanhoPagina)));

        [HttpGet("{id:guid}")]
        [AbacAuthorize("Expedicoes", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterExpedicaoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("Expedicoes", "Criar")]
        public async Task<IActionResult> Criar([FromBody] CriarExpedicaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/local-entrega")]
        [AbacAuthorize("Expedicoes", "Editar")]
        public async Task<IActionResult> DefinirLocalEntrega(Guid id, [FromBody] DefinirLocalEntregaCommand command)
        {
            if (id != command.ExpedicaoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/entregas")]
        [AbacAuthorize("Expedicoes", "Editar")]
        public async Task<IActionResult> RegistrarEntrega(Guid id, [FromBody] RegistrarEntregaItemCommand command)
        {
            if (id != command.ExpedicaoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/confirmar")]
        [AbacAuthorize("Expedicoes", "Editar")]
        public async Task<IActionResult> Confirmar(Guid id)
        {
            var result = await _mediator.Send(new ConfirmarExpedicaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/faturar")]
        [AbacAuthorize("Expedicoes", "Faturar")]
        public async Task<IActionResult> Faturar(Guid id, [FromBody] FaturarExpedicaoCommand command)
        {
            if (id != command.ExpedicaoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("Expedicoes", "Cancelar")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var result = await _mediator.Send(new CancelarExpedicaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
