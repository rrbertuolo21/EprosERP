using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Alias de rota compatível com o legado <c>api/v1/vendas-nfe</c>.
    /// Controller fino: apenas MediatR, encaminhando para o CQRS de documentos fiscais do módulo Fiscal.
    /// Não duplica lógica de Vendas — apenas expõe emissão/consulta de NF-e sob a rota histórica.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/vendas-nfe")]
    public class VendaNfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendaNfeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Emite/transmite uma NF-e (modelo 55). Alias de <c>api/v1/fiscal/documentos/emitir</c>.</summary>
        [HttpPost("nfe")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EmitirNfe([FromBody] EmitirDocumentoFiscalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>Consulta uma NF-e pelo Id. Alias de <c>api/v1/fiscal/documentos/{id}</c>.</summary>
        [HttpGet("{id:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterDocumentoFiscalPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result.Mensagem);
            }
            return Ok(result.Dados);
        }
    }
}
