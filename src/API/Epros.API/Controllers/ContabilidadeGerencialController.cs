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
    /// FIN-CMG — Contabilidade Gerencial (centros de custo, rateio de títulos, dimensões analíticas).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "ContabilidadeGerencial" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/contabilidade-gerencial")]
    public class ContabilidadeGerencialController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContabilidadeGerencialController(IMediator mediator) => _mediator = mediator;

        // ----- Centros de custo -----
        [HttpGet("centros-custo")]
        [AbacAuthorize("ContabilidadeGerencial", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCentros([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCentrosCustoQuery(pagina, tamanhoPagina)));

        [HttpGet("centros-custo/{id:guid}")]
        [AbacAuthorize("ContabilidadeGerencial", "Ler")]
        public async Task<IActionResult> ObterCentro(Guid id)
        {
            var r = await _mediator.Send(new ObterCentroCustoPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("centros-custo")]
        [AbacAuthorize("ContabilidadeGerencial", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarCentro([FromBody] CriarCentroCustoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("centros-custo/{id:guid}")]
        [AbacAuthorize("ContabilidadeGerencial", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarCentro(Guid id, [FromBody] AtualizarCentroCustoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("centros-custo/{id:guid}/estado")]
        [AbacAuthorize("ContabilidadeGerencial", "Editar")]
        public async Task<ActionResult<CommandResult>> DefinirEstado(Guid id, [FromBody] DefinirEstadoCentroCustoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("centros-custo/{id:guid}")]
        [AbacAuthorize("ContabilidadeGerencial", "Excluir")]
        public async Task<ActionResult<CommandResult>> DeletarCentro(Guid id)
        {
            var r = await _mediator.Send(new DeletarCentroCustoCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Alocações a centro de custo -----
        [HttpPost("alocacoes")]
        [AbacAuthorize("ContabilidadeGerencial", "Criar")]
        public async Task<ActionResult<CommandResult>> Alocar([FromBody] AlocarTituloCentroCustoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("alocacoes/{id:guid}")]
        [AbacAuthorize("ContabilidadeGerencial", "Excluir")]
        public async Task<ActionResult<CommandResult>> RemoverAlocacao(Guid id)
        {
            var r = await _mediator.Send(new RemoverAlocacaoCentroCustoCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("titulos/{tituloId:guid}/alocacoes")]
        [AbacAuthorize("ContabilidadeGerencial", "Ler")]
        public async Task<IActionResult> ListarAlocacoesTitulo(Guid tituloId)
            => Ok(await _mediator.Send(new ListarAlocacoesTituloQuery(tituloId)));

        [HttpGet("centros-custo/{id:guid}/consulta")]
        [AbacAuthorize("ContabilidadeGerencial", "Ler")]
        public async Task<IActionResult> Consulta(Guid id)
            => Ok(await _mediator.Send(new ConsultaGerencialPorCentroCustoQuery(id)));

        // ----- Dimensões analíticas -----
        [HttpGet("dimensoes")]
        [AbacAuthorize("ContabilidadeGerencial", "Ler")]
        public async Task<IActionResult> ListarDimensoes([FromQuery] string? tipo)
            => Ok(await _mediator.Send(new ListarDimensoesAnaliticasQuery(tipo)));

        [HttpPost("dimensoes")]
        [AbacAuthorize("ContabilidadeGerencial", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarDimensao([FromBody] CriarDimensaoAnaliticaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("alocacoes/{alocacaoId:guid}/dimensoes")]
        [AbacAuthorize("ContabilidadeGerencial", "Criar")]
        public async Task<ActionResult<CommandResult>> VincularDimensao(Guid alocacaoId, [FromBody] VincularAlocacaoDimensaoCommand command)
        {
            var r = await _mediator.Send(command with { AlocacaoCentroCustoId = alocacaoId });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
