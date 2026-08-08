using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/cadastros/pessoas")]
    public class PessoasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PessoasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarPessoaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        /// <summary>
        /// T-02/P1-3 — Importação em lote de pessoas (mapeamento + validação + processamento). Recebe as
        /// linhas já parseadas; o parsing do layout oficial (colunas/versão) fica no adaptador de upload.
        /// </summary>
        [HttpPost("importar-lote")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ImportarLote([FromBody] ImportarPessoasLoteCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPessoaCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID da rota não coincide com o ID do corpo da requisição.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterPessoaPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id)
        {
            var result = await _mediator.Send(new ExcluirPessoaCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? localizar,
            [FromQuery] string? inscricaoEstadual,
            [FromQuery] string? uf,
            [FromQuery] bool? ehCliente,
            [FromQuery] bool? ehFornecedor,
            [FromQuery] bool? ehTransportadora,
            [FromQuery] bool? ehMotorista,
            [FromQuery] bool? ehPrestadorServico,
            [FromQuery] bool? ehFuncionario,
            [FromQuery] bool? ehProdutorRural,
            [FromQuery] EStatusPessoa? status,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 10)
        {
            var result = await _mediator.Send(new ListarPessoasQuery(
                localizar, inscricaoEstadual, uf,
                ehCliente, ehFornecedor, ehTransportadora, ehMotorista,
                ehPrestadorServico, ehFuncionario, ehProdutorRural,
                status, pagina, tamanhoPagina));
            return Ok(result);
        }

        [HttpGet("localizar-cliente")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> LocalizarCliente([FromQuery] string query)
        {
            var result = await _mediator.Send(new LocalizarClienteQuery(query));
            return Ok(result);
        }
    }
}
