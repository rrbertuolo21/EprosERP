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
    /// Gestão de Contratos de Venda (VEN-GCV): contratos, tipos, modelos, assinaturas, renovações e
    /// comentários. Controller fino: apenas MediatR. AutZ ABAC nega por padrão (permissões separadas por
    /// família funcional — GCV-004). Fonte: EF_7_VENDAS_GESTAO_DE_CONTRATOS_DE_VENDA_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/contratos")]
    public class ContratosVendaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContratosVendaController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("Contratos", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] EContratoStatus? status, [FromQuery] Guid? tipoId, [FromQuery] Guid? usuarioResponsavelId, [FromQuery] string? localizar, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 10)
            => Ok(await _mediator.Send(new ListarContratosQuery(status, tipoId, usuarioResponsavelId, localizar, pagina, tamanhoPagina)));

        [HttpGet("{id:guid}")]
        [AbacAuthorize("Contratos", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterContratoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AbacAuthorize("Contratos", "Criar")]
        public async Task<IActionResult> Criar([FromBody] CriarContratoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/publicar")]
        [AbacAuthorize("Contratos", "Publicar")]
        public async Task<IActionResult> Publicar(Guid id, [FromBody] PublicarContratoCommand command)
        {
            if (id != command.ContratoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/assinar")]
        [AbacAuthorize("ContratosAssinatura", "Assinar")]
        public async Task<IActionResult> Assinar(Guid id, [FromBody] AssinarContratoCommand command)
        {
            if (id != command.ContratoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/renovacoes")]
        [AbacAuthorize("ContratosRenovacao", "Criar")]
        public async Task<IActionResult> CriarRenovacao(Guid id, [FromBody] CriarContratoRenovacaoCommand command)
        {
            if (id != command.ContratoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/comentarios")]
        [AbacAuthorize("ContratosComentario", "Criar")]
        public async Task<IActionResult> AdicionarComentario(Guid id, [FromBody] AdicionarContratoComentarioCommand command)
        {
            if (id != command.ContratoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tipos")]
        [AbacAuthorize("ContratosTipos", "Criar")]
        public async Task<IActionResult> CriarTipo([FromBody] CriarContratoTipoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("modelos")]
        [AbacAuthorize("ContratosModelos", "Criar")]
        public async Task<IActionResult> CriarModelo([FromBody] CriarContratoModeloCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }
    }
}
