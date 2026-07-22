using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    // ----- BANCOS -----
    [ApiController]
    [Route("api/v1/bancos")]
    [Produces("application/json")]
    public class BancosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BancosController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarBancosQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterBancoPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarBancoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarBancoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("ID da rota não coincide com o do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarBancoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ----- CONTAS BANCARIAS -----
    [ApiController]
    [Route("api/v1/contas-bancarias")]
    [Produces("application/json")]
    public class ContasBancariasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContasBancariasController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarContasBancariasQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterContaBancariaPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarContaBancariaCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContaBancariaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("ID da rota não coincide com o do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarContaBancariaCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // OFX Statement Upload
        [HttpPost("{id:guid}/importar-ofx")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ImportarOfx(Guid id, [FromBody] ImportarOfxRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ProcessarExtratoOfxCommand(id, request.OfxConteudo), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    public record ImportarOfxRequest(string OfxConteudo);

    // ----- CARTÕES DE CRÉDITO -----
    [ApiController]
    [Route("api/v1/cartoes-credito")]
    [Produces("application/json")]
    public class CartoesDeCreditoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CartoesDeCreditoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarCartoesDeCreditoQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCartaoDeCreditoPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarCartaoDeCreditoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCartaoDeCreditoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("ID da rota não coincide com o do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarCartaoDeCreditoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // Faturas do Cartão
        [HttpGet("{id:guid}/faturas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarFaturas(Guid id, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarFaturasCartaoQuery(id, pagina, tamanhoPagina), cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/faturas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> LancarFatura(Guid id, [FromBody] LancarFaturaRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LancarFaturaCartaoCommand(id, request.DataLancamento, request.DataVencimento, request.Valor, request.Pago), cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("faturas/{faturaId:guid}/baixar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> BaixarFatura(Guid faturaId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BaixarFaturaCartaoCommand(faturaId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    public record LancarFaturaRequest(DateTime DataLancamento, DateTime DataVencimento, decimal Valor, bool Pago);
}
