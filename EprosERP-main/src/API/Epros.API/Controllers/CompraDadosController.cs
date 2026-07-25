using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Dados auxiliares para a tela de compra (emitente/fornecedor/produtos/transportadora/CFOPs).
    /// Controller fino: cada action delega a uma query MediatR no módulo dono (GestaoClientes/Estoque/Fiscal).
    /// Compatível com o legado <c>api/v1/compras-dados</c>.
    /// </summary>
    [ApiController]
    [Route("api/v1/compras-dados")]
    [Produces("application/json")]
    public class CompraDadosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompraDadosController(IMediator mediator) => _mediator = mediator;

        /// <summary>Dados do emitente (empresa) para a compra.</summary>
        [HttpGet("obter-emitente-por-id/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterEmitente(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDadosEmitentePorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Dados do fornecedor (pessoa) para a compra.</summary>
        [HttpGet("obter-fornecedor-por-id/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterFornecedor(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDadosPessoaPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Dados da transportadora para a compra.</summary>
        [HttpGet("obter-transportadora-por-id/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterTransportadora(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDadosPessoaPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Produtos por lista de Ids para a compra.</summary>
        [HttpGet("obter-produtos-por-ids")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ObterProdutos([FromQuery] Guid[] idsProdutos, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDadosProdutosPorIdsQuery(idsProdutos ?? Array.Empty<Guid>()), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>CFOPs de entrada (ou saída) para a compra.</summary>
        [HttpGet("obter-cfops")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObterCfops([FromQuery] int tipoOperacao = 0, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarCfopsPorTipoOperacaoQuery(tipoOperacao), cancellationToken);
            return Ok(result);
        }

        /// <summary>Serviços por lista de Ids para a compra (Servico é do módulo Fiscal; leitura via lookup).</summary>
        [HttpGet("obter-servicos-por-ids")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ObterServicos([FromQuery] Guid[] idsServicos, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterDadosServicosPorIdsQuery(idsServicos ?? Array.Empty<Guid>()), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
