using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>Landlord: gestão de Faturas (GAP-4). Geração continua em plataforma/clientes/faturas.</summary>
    [ApiController]
    [Route("api/v1/plataforma/faturas")]
    [Produces("application/json")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    // 1.11 decisão #5 — área comercial: faixa de Suporte Negócio (SuporteTecnico é negado; PrimaryAdmin passa).
    [AbacAuthorize(SuperAdminSeguranca.RecursoSuporteComercial, "Configurar")]
    public class FaturasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FaturasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            [FromQuery] Guid? clienteId = null,
            [FromQuery] string? status = null)
        {
            var result = await _mediator.Send(new ListarFaturasQuery(pagina, tamanhoPagina, clienteId, status));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new ObterFaturaPorIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlterarFaturaCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Delete(Guid id)
        {
            var result = await _mediator.Send(new ExcluirFaturaCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("{id}/baixar-manual")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> BaixarManual(Guid id, [FromBody] BaixarFaturaManualCommand command)
        {
            if (id != command.FaturaId)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>1.08A — Recibo de pagamento da fatura quitada (documento simples; NFS-e diferida).</summary>
        [HttpGet("{id}/recibo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterRecibo(Guid id)
        {
            var recibo = await _mediator.Send(new ObterReciboPorFaturaQuery(id));
            if (recibo == null) return NotFound();
            return Ok(recibo);
        }

        /// <summary>Gera a cobrança PIX da fatura no gateway ativo (config por tenant, senão global).</summary>
        [HttpPost("{id}/gerar-cobranca-pix")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GerarCobrancaPix(Guid id)
        {
            var result = await _mediator.Send(new GerarCobrancaPixCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>1.08B — Emite o BOLETO da fatura no gateway ativo (concilia pelo webhook unificado).</summary>
        [HttpPost("{id}/gerar-boleto")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GerarBoleto(Guid id)
        {
            var result = await _mediator.Send(new GerarBoletoCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }
    }
}
