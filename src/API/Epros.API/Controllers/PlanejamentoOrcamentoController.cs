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
    /// FIN-PO — Planejamento e Orçamento (versões orçamentárias, previsto x realizado, períodos, budgets, metas).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "PlanejamentoOrcamento" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/planejamento-orcamento")]
    public class PlanejamentoOrcamentoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PlanejamentoOrcamentoController(IMediator mediator) => _mediator = mediator;

        // ----- Versões orçamentárias -----
        [HttpGet("versoes")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarVersoes([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarVersoesOrcamentariasQuery(pagina, tamanhoPagina)));

        [HttpGet("versoes/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ObterVersao(Guid id)
        {
            var r = await _mediator.Send(new ObterVersaoOrcamentariaPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("versoes")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarVersao([FromBody] CriarVersaoOrcamentariaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("versoes/{id:guid}/aprovar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AprovarVersao(Guid id)
        {
            var r = await _mediator.Send(new AprovarVersaoOrcamentariaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("versoes/{id:guid}/ativar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AtivarVersao(Guid id)
        {
            var r = await _mediator.Send(new AtivarVersaoOrcamentariaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("versoes/{id:guid}/encerrar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarVersao(Guid id)
        {
            var r = await _mediator.Send(new EncerrarVersaoOrcamentariaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("versoes/{id:guid}/previsto-realizado")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> PrevistoRealizado(Guid id)
            => Ok(await _mediator.Send(new PrevistoRealizadoQuery(id)));

        [HttpPost("linhas/{linhaId:guid}/realizado")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarRealizado(Guid linhaId, [FromBody] RegistrarRealizadoLinhaCommand command)
        {
            var r = await _mediator.Send(command with { LinhaId = linhaId });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Períodos e budgets -----
        [HttpGet("periodos")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ListarPeriodos()
            => Ok(await _mediator.Send(new ListarPeriodosOrcamentariosQuery()));

        [HttpPost("periodos")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarPeriodo([FromBody] CriarPeriodoOrcamentarioCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/aprovar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AprovarPeriodo(Guid id)
        {
            var r = await _mediator.Send(new AprovarPeriodoOrcamentarioCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/ativar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AtivarPeriodo(Guid id)
        {
            var r = await _mediator.Send(new AtivarPeriodoOrcamentarioCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/encerrar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarPeriodo(Guid id)
        {
            var r = await _mediator.Send(new EncerrarPeriodoOrcamentarioCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("periodos/{id:guid}/budgets")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ListarBudgets(Guid id)
            => Ok(await _mediator.Send(new ListarBudgetsPeriodoQuery(id)));

        [HttpPost("periodos/{id:guid}/budgets")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarBudget(Guid id, [FromBody] CriarBudgetCommand command)
        {
            var r = await _mediator.Send(command with { PeriodoId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("budgets/{id:guid}/aprovar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AprovarBudget(Guid id)
        {
            var r = await _mediator.Send(new AprovarBudgetCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("budgets/{id:guid}/ativar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AtivarBudget(Guid id)
        {
            var r = await _mediator.Send(new AtivarBudgetCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("budgets/{id:guid}/encerrar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> EncerrarBudget(Guid id)
        {
            var r = await _mediator.Send(new EncerrarBudgetCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("budgets/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Excluir")]
        public async Task<ActionResult<CommandResult>> ExcluirBudget(Guid id)
        {
            var r = await _mediator.Send(new ExcluirBudgetCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("budgets/{id:guid}/alocacoes")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AlocarBudget(Guid id, [FromBody] AlocarBudgetCommand command)
        {
            var r = await _mediator.Send(command with { BudgetId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Orçamento comercial -----
        [HttpGet("orcamentos-comerciais")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ListarOrcamentosComerciais([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarOrcamentosComerciaisQuery(pagina, tamanhoPagina)));

        [HttpGet("orcamentos-comerciais/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ObterOrcamentoComercial(Guid id)
        {
            var r = await _mediator.Send(new ObterOrcamentoComercialPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("orcamentos-comerciais")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarOrcamentoComercial([FromBody] CriarOrcamentoComercialCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("orcamentos-comerciais/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AlterarOrcamentoComercial(Guid id, [FromBody] AlterarOrcamentoComercialCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("orcamentos-comerciais/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Excluir")]
        public async Task<ActionResult<CommandResult>> ExcluirOrcamentoComercial(Guid id)
        {
            var r = await _mediator.Send(new ExcluirOrcamentoComercialCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Metas -----
        [HttpPost("metas/categorias")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarCategoriaMeta([FromBody] CriarMetaCategoriaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("metas")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ListarMetas([FromQuery] Guid? categoriaId)
            => Ok(await _mediator.Send(new ListarMetasQuery(categoriaId)));

        [HttpGet("metas/{id:guid}")]
        [AbacAuthorize("PlanejamentoOrcamento", "Ler")]
        public async Task<IActionResult> ObterMeta(Guid id)
        {
            var r = await _mediator.Send(new ObterMetaPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("metas")]
        [AbacAuthorize("PlanejamentoOrcamento", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarMeta([FromBody] CriarMetaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("metas/{id:guid}/ativar")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> AtivarMeta(Guid id)
        {
            var r = await _mediator.Send(new AtivarMetaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("metas/{id:guid}/contribuicoes")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarContribuicao(Guid id, [FromBody] RegistrarContribuicaoMetaCommand command)
        {
            var r = await _mediator.Send(command with { MetaId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("metas/{id:guid}/tracking")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarTracking(Guid id, [FromBody] RegistrarTrackingMetaCommand command)
        {
            var r = await _mediator.Send(command with { MetaId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("metas/{id:guid}/milestones")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarMilestone(Guid id, [FromBody] CriarMilestoneMetaCommand command)
        {
            var r = await _mediator.Send(command with { MetaId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("milestones/{milestoneId:guid}/concluir")]
        [AbacAuthorize("PlanejamentoOrcamento", "Editar")]
        public async Task<ActionResult<CommandResult>> ConcluirMilestone(Guid milestoneId)
        {
            var r = await _mediator.Send(new ConcluirMilestoneMetaCommand(milestoneId));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
