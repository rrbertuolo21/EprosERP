using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/aplicativo/assinaturas")]
    [Produces("application/json")]
    public class AssinaturasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssinaturasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("vigente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterVigente()
        {
            var query = new ObterAssinaturaVigenteQuery();
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { Mensagem = "Nenhuma assinatura ativa ou trial configurada para este inquilino." });
            }
            return Ok(result);
        }

        [HttpPost("contratar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Contratar([FromBody] ContratarPlanoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("faturas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFaturas([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
        {
            var query = new ListarFaturasTenantQuery(pageIndex, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("faturas/{faturaId:guid}/pix")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarPix(Guid faturaId)
        {
            var command = new GerarPixFaturaCommand(faturaId);
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>1.08B — Emite o BOLETO real da fatura (concilia pelo webhook unificado do MP).</summary>
        [HttpPost("faturas/{faturaId:guid}/boleto")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarBoleto(Guid faturaId)
        {
            var result = await _mediator.Send(new GerarBoletoCommand(faturaId));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        // ===== 1.08B — Meios de pagamento salvos (cartão-on-file) do cliente =====

        /// <summary>Lista os cartões salvos do cliente do tenant corrente.</summary>
        [HttpGet("meios-pagamento")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMeiosPagamento()
        {
            var result = await _mediator.Send(new ListarMeiosPagamentoQuery());
            return Ok(result);
        }

        /// <summary>
        /// Adiciona um cartão salvo. ⛔ PCI: o corpo carrega apenas o TOKEN do cartão gerado no FRONT pela
        /// lib do Mercado Pago — PAN/CVV nunca chegam ao backend.
        /// </summary>
        [HttpPost("meios-pagamento")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarCartao([FromBody] AdicionarCartaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>Remove (desativa) um cartão salvo.</summary>
        [HttpDelete("meios-pagamento/{meioId:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RemoverMeioPagamento(Guid meioId)
        {
            var result = await _mediator.Send(new RemoverMeioPagamentoCommand(meioId));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>Define um cartão salvo como padrão (débito automático).</summary>
        [HttpPost("meios-pagamento/{meioId:guid}/padrao")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirPadrao(Guid meioId)
        {
            var result = await _mediator.Send(new DefinirMeioPagamentoPadraoCommand(meioId));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
