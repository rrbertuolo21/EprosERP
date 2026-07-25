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
    /// FIN-AFX — Ativos Fixos (bens patrimoniais, depreciação, baixa, movimentação).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "AtivosFixos" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/ativos-fixos")]
    public class AtivosFixosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AtivosFixosController(IMediator mediator) => _mediator = mediator;

        // ----- Ativos -----
        [HttpGet]
        [AbacAuthorize("AtivosFixos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] EStatusAtivoFixo? status, [FromQuery] Guid? grupoBemId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarAtivosFixosQuery(status, grupoBemId, pagina, tamanhoPagina)));

        [HttpGet("{id:guid}")]
        [AbacAuthorize("AtivosFixos", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var r = await _mediator.Send(new ObterAtivoFixoPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost]
        [AbacAuthorize("AtivosFixos", "Criar")]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarAtivoFixoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("AtivosFixos", "Editar")]
        public async Task<ActionResult<CommandResult>> Atualizar(Guid id, [FromBody] AtualizarAtivoFixoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/baixar")]
        [AbacAuthorize("AtivosFixos", "Editar")]
        public async Task<ActionResult<CommandResult>> Baixar(Guid id, [FromBody] BaixarAtivoFixoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("{id:guid}/depreciacoes")]
        [AbacAuthorize("AtivosFixos", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarDepreciacao(Guid id, [FromBody] RegistrarDepreciacaoMensalCommand command)
        {
            var r = await _mediator.Send(command with { AtivoId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("{id:guid}/depreciacoes")]
        [AbacAuthorize("AtivosFixos", "Ler")]
        public async Task<IActionResult> ListarDepreciacoes(Guid id)
            => Ok(await _mediator.Send(new ListarDepreciacoesAtivoQuery(id)));

        [HttpPost("{id:guid}/movimentacoes")]
        [AbacAuthorize("AtivosFixos", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarMovimentacao(Guid id, [FromBody] RegistrarMovimentacaoAtivoCommand command)
        {
            var r = await _mediator.Send(command with { AtivoId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("{id:guid}/movimentacoes")]
        [AbacAuthorize("AtivosFixos", "Ler")]
        public async Task<IActionResult> ListarMovimentacoes(Guid id)
            => Ok(await _mediator.Send(new ListarMovimentacoesAtivoQuery(id)));

        // ----- Grupos de bem -----
        [HttpGet("grupos")]
        [AbacAuthorize("AtivosFixos", "Ler")]
        public async Task<IActionResult> ListarGrupos([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarGruposBemQuery(pagina, tamanhoPagina)));

        [HttpPost("grupos")]
        [AbacAuthorize("AtivosFixos", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarGrupo([FromBody] CriarGrupoBemCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("grupos/{id:guid}")]
        [AbacAuthorize("AtivosFixos", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarGrupo(Guid id, [FromBody] AtualizarGrupoBemCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
