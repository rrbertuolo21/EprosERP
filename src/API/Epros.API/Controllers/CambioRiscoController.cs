using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// FIN-CAM — Câmbio e Risco de Mercado (moedas, taxas, exposição, reavaliação cambial).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "CambioRisco" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/cambio-risco")]
    public class CambioRiscoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CambioRiscoController(IMediator mediator) => _mediator = mediator;

        // ----- Moedas -----
        [HttpGet("moedas")]
        [AbacAuthorize("CambioRisco", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarMoedas([FromQuery] bool? apenasAtivas, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarMoedasQuery(apenasAtivas, pagina, tamanhoPagina)));

        [HttpPost("moedas")]
        [AbacAuthorize("CambioRisco", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarMoeda([FromBody] CriarMoedaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("moedas/{id:guid}")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarMoeda(Guid id, [FromBody] AtualizarMoedaCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("moedas/{id:guid}")]
        [AbacAuthorize("CambioRisco", "Excluir")]
        public async Task<ActionResult<CommandResult>> DeletarMoeda(Guid id)
        {
            var r = await _mediator.Send(new DeletarMoedaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Taxas de câmbio -----
        [HttpGet("taxas")]
        [AbacAuthorize("CambioRisco", "Ler")]
        public async Task<IActionResult> ListarTaxas([FromQuery] Guid? moedaId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarTaxasCambioQuery(moedaId, pagina, tamanhoPagina)));

        [HttpPost("taxas")]
        [AbacAuthorize("CambioRisco", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarTaxa([FromBody] RegistrarTaxaCambioCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Exposição cambial -----
        [HttpGet("exposicoes")]
        [AbacAuthorize("CambioRisco", "Ler")]
        public async Task<IActionResult> ListarExposicoes([FromQuery] Guid? moedaId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarExposicoesCambiaisQuery(moedaId, pagina, tamanhoPagina)));

        [HttpPost("exposicoes")]
        [AbacAuthorize("CambioRisco", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarExposicao([FromBody] CriarExposicaoCambialCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("exposicoes/{id:guid}/hedgear")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> Hedgear(Guid id)
        {
            var r = await _mediator.Send(new MarcarExposicaoHedgeadaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("exposicoes/{id:guid}/encerrar")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarExposicao(Guid id)
        {
            var r = await _mediator.Send(new EncerrarExposicaoCambialCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Reavaliação de títulos -----
        [HttpGet("reavaliacoes")]
        [AbacAuthorize("CambioRisco", "Ler")]
        public async Task<IActionResult> ListarReavaliacoes([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarReavaliacoesTituloQuery(pagina, tamanhoPagina)));

        [HttpGet("reavaliacoes/{id:guid}")]
        [AbacAuthorize("CambioRisco", "Ler")]
        public async Task<IActionResult> ObterReavaliacao(Guid id)
        {
            var r = await _mediator.Send(new ObterReavaliacaoTituloPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("reavaliacoes")]
        [AbacAuthorize("CambioRisco", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarReavaliacao([FromBody] CriarReavaliacaoTituloCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("reavaliacoes/{id:guid}/aprovar")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> AprovarReavaliacao(Guid id)
        {
            var r = await _mediator.Send(new AprovarReavaliacaoTituloCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("reavaliacoes/{id:guid}/contabilizar")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> ContabilizarReavaliacao(Guid id)
        {
            var r = await _mediator.Send(new ContabilizarReavaliacaoTituloCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("reavaliacoes/{id:guid}/cancelar")]
        [AbacAuthorize("CambioRisco", "Editar")]
        public async Task<ActionResult<CommandResult>> CancelarReavaliacao(Guid id)
        {
            var r = await _mediator.Send(new CancelarReavaliacaoTituloCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
