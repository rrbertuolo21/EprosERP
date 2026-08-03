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
    /// Faturamento Comercial Internacional (VEN-FCI): fatura comercial (9) e nota de crédito (10),
    /// impostos comerciais e preferências. Documento COMERCIAL internacional — NÃO é documento fiscal
    /// brasileiro (não é NF-e). Controller fino: apenas MediatR. AutZ ABAC nega por padrão.
    /// Fonte: EF_7_VENDAS_FATURAMENTO_COMERCIAL_INTERNACIONAL_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/faturamento-internacional")]
    public class FaturamentoComercialInternacionalController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FaturamentoComercialInternacionalController(IMediator mediator) => _mediator = mediator;

        // ----- Documentos comerciais (faturas / notas de crédito) -----

        [HttpGet("documentos")]
        [AbacAuthorize("FciDocumentos", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] EFciTipoDocumento tipoDocumento, [FromQuery] DateTime? dataInicial,
            [FromQuery] DateTime? dataFinal, [FromQuery] Guid? clienteId, [FromQuery] string? numero, [FromQuery] EFciStatus? status,
            [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarFciDocumentosQuery(tipoDocumento, dataInicial, dataFinal, clienteId, numero, status, pagina, tamanhoPagina)));

        [HttpGet("documentos/{id:guid}")]
        [AbacAuthorize("FciDocumentos", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var result = await _mediator.Send(new ObterFciDocumentoPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpGet("contas-em-aberto")]
        [AbacAuthorize("FciDocumentos", "Ler")]
        public async Task<IActionResult> ContasEmAberto([FromQuery] Guid? clienteId)
            => Ok(await _mediator.Send(new ListarFciContasEmAbertoQuery(clienteId)));

        [HttpPost("documentos")]
        [AbacAuthorize("FciDocumentos", "Criar")]
        public async Task<IActionResult> Criar([FromBody] CriarFciDocumentoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("documentos/{id:guid}/aprovar")]
        [AbacAuthorize("FciDocumentos", "Aprovar")]
        public async Task<IActionResult> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarFciDocumentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("documentos/{id:guid}")]
        [AbacAuthorize("FciDocumentos", "Editar")]
        public async Task<IActionResult> Editar(Guid id, [FromBody] EditarFciDocumentoCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("documentos/{id:guid}/pagamento")]
        [AbacAuthorize("FciDocumentos", "Editar")]
        public async Task<IActionResult> RegistrarPagamento(Guid id, [FromBody] RegistrarPagamentoFciDocumentoCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("documentos/{id:guid}")]
        [AbacAuthorize("FciDocumentos", "Excluir")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var result = await _mediator.Send(new ExcluirFciDocumentoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Impostos comerciais -----

        [HttpGet("impostos")]
        [AbacAuthorize("FciImpostos", "Ler")]
        public async Task<IActionResult> ListarImpostos([FromQuery] bool apenasAtivos = false)
            => Ok(await _mediator.Send(new ListarFciImpostosQuery(apenasAtivos)));

        [HttpPost("impostos")]
        [AbacAuthorize("FciImpostos", "Criar")]
        public async Task<IActionResult> CriarImposto([FromBody] CriarFciImpostoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("impostos/{id:guid}")]
        [AbacAuthorize("FciImpostos", "Editar")]
        public async Task<IActionResult> AtualizarImposto(Guid id, [FromBody] AtualizarFciImpostoCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("impostos/{id:guid}")]
        [AbacAuthorize("FciImpostos", "Excluir")]
        public async Task<IActionResult> ExcluirImposto(Guid id)
        {
            var result = await _mediator.Send(new ExcluirFciImpostoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ----- Preferências gerais -----

        [HttpPut("preferencias")]
        [AbacAuthorize("FciPreferencias", "Editar")]
        public async Task<IActionResult> SalvarPreferencia([FromBody] SalvarFciPreferenciaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
