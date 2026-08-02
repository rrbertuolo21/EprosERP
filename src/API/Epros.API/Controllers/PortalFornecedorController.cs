using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Portal do Fornecedor (EST-PFO). Superfície B2B externa: convite/acesso do fornecedor, publicação e
    /// resposta de cotação, pré-aviso de embarque (ASN) e envio de documentos. Isolamento por fornecedor
    /// (PFO-002) é aplicado nos handlers/queries; a autenticação externa reutiliza o padrão do Portal do
    /// Cliente (D6). ABAC conforme EF §18 (estoque.portal_fornecedor.*).
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/estoque-portal-fornecedor")]
    public class PortalFornecedorController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PortalFornecedorController(IMediator mediator) => _mediator = mediator;

        // ---------- Convite / acesso (interno) ----------

        [HttpGet("convites")]
        [AbacAuthorize("EstoquePortalFornecedor", "GerenciarConvite")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarConvites([FromQuery] EStatusConviteFornecedor? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarConvitesFornecedorQuery(status, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("convites")]
        [AbacAuthorize("EstoquePortalFornecedor", "GerenciarConvite")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Convidar([FromBody] ConvidarFornecedorCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        [HttpPost("convites/{id:guid}/ativar")]
        [AbacAuthorize("EstoquePortalFornecedor", "GerenciarConvite")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtivarAcesso(Guid id, [FromBody] AtivarAcessoFornecedorCommand command, CancellationToken ct)
        {
            if (id != command.ConviteId) return BadRequest("O ID da rota não corresponde ao corpo.");
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ---------- Cotação ----------

        [HttpPost("cotacoes/publicar")]
        [AbacAuthorize("EstoquePortalFornecedor", "GerenciarConvite")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> PublicarCotacao([FromBody] PublicarCotacaoFornecedorCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        [HttpGet("cotacoes")]
        [AbacAuthorize("EstoquePortalFornecedor", "Acessar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarCotacoes([FromQuery] Guid fornecedorId, [FromQuery] EStatusCotacaoPublicada? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarCotacoesFornecedorQuery(fornecedorId, status, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("cotacoes/responder")]
        [AbacAuthorize("EstoquePortalFornecedor", "ResponderCotacao")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResponderCotacao([FromBody] ResponderCotacaoFornecedorCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        // ---------- Pré-aviso (ASN) ----------

        [HttpGet("pre-avisos")]
        [AbacAuthorize("EstoquePortalFornecedor", "Acessar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarPreAvisos([FromQuery] Guid fornecedorId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarPreAvisosFornecedorQuery(fornecedorId, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("pre-avisos")]
        [AbacAuthorize("EstoquePortalFornecedor", "CriarPreAviso")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> EnviarPreAviso([FromBody] EnviarPreAvisoEmbarqueCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }

        // ---------- Documentos ----------

        [HttpGet("documentos")]
        [AbacAuthorize("EstoquePortalFornecedor", "Acessar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarDocumentos([FromQuery] Guid fornecedorId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        {
            var r = await _mediator.Send(new ListarDocumentosFornecedorQuery(fornecedorId, pagina, tamanhoPagina), ct);
            return r.Sucesso ? Ok(r) : BadRequest(r);
        }

        [HttpPost("documentos")]
        [AbacAuthorize("EstoquePortalFornecedor", "EnviarDocumento")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> EnviarDocumento([FromBody] EnviarDocumentoFornecedorCommand command, CancellationToken ct)
        {
            var r = await _mediator.Send(command, ct);
            return r.Sucesso ? Created(string.Empty, r) : UnprocessableEntity(r);
        }
    }
}
