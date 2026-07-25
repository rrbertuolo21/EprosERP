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
    /// FIN-CON — Consolidação e Relatórios (grupos, demonstrativos, balancetes, eliminação intercompany).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "Consolidacao" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/consolidacao")]
    public class ConsolidacaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ConsolidacaoController(IMediator mediator) => _mediator = mediator;

        // ----- Grupos -----
        [HttpGet("grupos")]
        [AbacAuthorize("Consolidacao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarGrupos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarGruposConsolidacaoQuery(pagina, tamanhoPagina)));

        [HttpGet("grupos/{id:guid}")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ObterGrupo(Guid id)
        {
            var r = await _mediator.Send(new ObterGrupoConsolidacaoPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("grupos")]
        [AbacAuthorize("Consolidacao", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarGrupo([FromBody] CriarGrupoConsolidacaoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("grupos/{id:guid}")]
        [AbacAuthorize("Consolidacao", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarGrupo(Guid id, [FromBody] AtualizarGrupoConsolidacaoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("grupos/{id:guid}/empresas")]
        [AbacAuthorize("Consolidacao", "Editar")]
        public async Task<ActionResult<CommandResult>> AdicionarEmpresa(Guid id, [FromBody] AdicionarEmpresaGrupoCommand command)
        {
            var r = await _mediator.Send(command with { GrupoConsolidacaoId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("empresas/{vinculoId:guid}")]
        [AbacAuthorize("Consolidacao", "Editar")]
        public async Task<ActionResult<CommandResult>> RemoverEmpresa(Guid vinculoId)
        {
            var r = await _mediator.Send(new RemoverEmpresaGrupoCommand(vinculoId));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Demonstrativos -----
        [HttpGet("grupos/{id:guid}/demonstrativos")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ListarDemonstrativos(Guid id)
            => Ok(await _mediator.Send(new ListarDemonstrativosQuery(id)));

        [HttpGet("demonstrativos/{id:guid}")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ObterDemonstrativo(Guid id)
        {
            var r = await _mediator.Send(new ObterDemonstrativoPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("demonstrativos")]
        [AbacAuthorize("Consolidacao", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarDemonstrativo([FromBody] CriarDemonstrativoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("demonstrativos/{id:guid}/publicar")]
        [AbacAuthorize("Consolidacao", "Editar")]
        public async Task<ActionResult<CommandResult>> PublicarDemonstrativo(Guid id, [FromBody] PublicarDemonstrativoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Balancetes -----
        [HttpGet("grupos/{id:guid}/balancetes")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ListarBalancetes(Guid id)
            => Ok(await _mediator.Send(new ListarBalancetesQuery(id)));

        [HttpGet("balancetes/{id:guid}")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ObterBalancete(Guid id)
        {
            var r = await _mediator.Send(new ObterBalancetePorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("balancetes")]
        [AbacAuthorize("Consolidacao", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarBalancete([FromBody] CriarBalanceteConsolidadoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("balancetes/{id:guid}/publicar")]
        [AbacAuthorize("Consolidacao", "Editar")]
        public async Task<ActionResult<CommandResult>> PublicarBalancete(Guid id)
        {
            var r = await _mediator.Send(new PublicarBalanceteConsolidadoCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Eliminação intercompany -----
        [HttpGet("grupos/{id:guid}/eliminacoes")]
        [AbacAuthorize("Consolidacao", "Ler")]
        public async Task<IActionResult> ListarEliminacoes(Guid id, [FromQuery] string? periodo)
            => Ok(await _mediator.Send(new ListarEliminacoesQuery(id, periodo)));

        [HttpPost("eliminacoes")]
        [AbacAuthorize("Consolidacao", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarEliminacao([FromBody] RegistrarEliminacaoIntercompanyCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
