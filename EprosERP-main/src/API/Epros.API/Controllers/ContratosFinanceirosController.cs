using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// FIN-GCF — Gestão de Contratos Financeiros (planos, contratos recorrentes, faturas, reajustes).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "ContratosFinanceiros" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/contratos-financeiros")]
    public class ContratosFinanceirosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContratosFinanceirosController(IMediator mediator) => _mediator = mediator;

        // ----- Planos -----
        [HttpGet("planos")]
        [AbacAuthorize("ContratosFinanceiros", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPlanos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarPlanosContratoQuery(pagina, tamanhoPagina)));

        [HttpPost("planos")]
        [AbacAuthorize("ContratosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarPlano([FromBody] CriarPlanoContratoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("planos/{id:guid}")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarPlano(Guid id, [FromBody] AtualizarPlanoContratoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Contratos -----
        [HttpGet]
        [AbacAuthorize("ContratosFinanceiros", "Ler")]
        public async Task<IActionResult> Listar([FromQuery] EStatusContratoFinanceiro? status, [FromQuery] Guid? pessoaId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarContratosFinanceirosQuery(status, pessoaId, pagina, tamanhoPagina)));

        [HttpGet("{id:guid}")]
        [AbacAuthorize("ContratosFinanceiros", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var r = await _mediator.Send(new ObterContratoFinanceiroPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost]
        [AbacAuthorize("ContratosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarContratoFinanceiroCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/suspender")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> Suspender(Guid id)
        {
            var r = await _mediator.Send(new SuspenderContratoFinanceiroCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/reativar")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> Reativar(Guid id)
        {
            var r = await _mediator.Send(new ReativarContratoFinanceiroCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, [FromBody] CancelarContratoFinanceiroCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/reajustar")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> Reajustar(Guid id, [FromBody] ReajustarContratoFinanceiroCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("{id:guid}/reajustes")]
        [AbacAuthorize("ContratosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarReajustes(Guid id)
            => Ok(await _mediator.Send(new ListarReajustesContratoQuery(id)));

        // ----- Faturas recorrentes -----
        [HttpPost("{id:guid}/faturas")]
        [AbacAuthorize("ContratosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> GerarFatura(Guid id, [FromBody] GerarFaturaRecorrenteCommand command)
        {
            var r = await _mediator.Send(command with { ContratoId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("{id:guid}/faturas")]
        [AbacAuthorize("ContratosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarFaturas(Guid id)
            => Ok(await _mediator.Send(new ListarFaturasRecorrentesQuery(id)));

        [HttpPost("faturas/{faturaId:guid}/cancelar")]
        [AbacAuthorize("ContratosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> CancelarFatura(Guid faturaId)
        {
            var r = await _mediator.Send(new CancelarFaturaRecorrenteCommand(faturaId));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
