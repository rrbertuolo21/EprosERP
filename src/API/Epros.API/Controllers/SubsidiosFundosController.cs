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
    /// FIN-SBF — Subsídios e Fundos (programas/fundos, utilização por despesa elegível, saldo, prestação de contas).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "SubsidiosFundos" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/subsidios-fundos")]
    public class SubsidiosFundosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SubsidiosFundosController(IMediator mediator) => _mediator = mediator;

        // ----- Programas -----
        [HttpGet("programas")]
        [AbacAuthorize("SubsidiosFundos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] EEstadoProgramaSubsidio? estado, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarProgramasSubsidioQuery(estado, pagina, tamanhoPagina)));

        [HttpGet("programas/{id:guid}")]
        [AbacAuthorize("SubsidiosFundos", "Ler")]
        public async Task<IActionResult> Obter(Guid id)
        {
            var r = await _mediator.Send(new ObterProgramaSubsidioPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("programas")]
        [AbacAuthorize("SubsidiosFundos", "Criar")]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarProgramaSubsidioCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("programas/{id:guid}")]
        [AbacAuthorize("SubsidiosFundos", "Editar")]
        public async Task<ActionResult<CommandResult>> Atualizar(Guid id, [FromBody] AtualizarProgramaSubsidioCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("programas/{id:guid}/prestacao-contas")]
        [AbacAuthorize("SubsidiosFundos", "Editar")]
        public async Task<ActionResult<CommandResult>> IniciarPrestacaoContas(Guid id)
        {
            var r = await _mediator.Send(new IniciarPrestacaoContasProgramaCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("programas/{id:guid}/encerrar")]
        [AbacAuthorize("SubsidiosFundos", "Editar")]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id)
        {
            var r = await _mediator.Send(new EncerrarProgramaSubsidioCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("programas/{id:guid}/saldo")]
        [AbacAuthorize("SubsidiosFundos", "Ler")]
        public async Task<IActionResult> Saldo(Guid id)
            => Ok(await _mediator.Send(new ConsultarSaldoProgramaQuery(id)));

        // ----- Utilizações -----
        [HttpPost("programas/{id:guid}/utilizacoes")]
        [AbacAuthorize("SubsidiosFundos", "Criar")]
        public async Task<ActionResult<CommandResult>> Vincular(Guid id, [FromBody] VincularDespesaElegivelCommand command)
        {
            var r = await _mediator.Send(command with { ProgramaSubsidioId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("programas/{id:guid}/utilizacoes")]
        [AbacAuthorize("SubsidiosFundos", "Ler")]
        public async Task<IActionResult> ListarUtilizacoes(Guid id)
            => Ok(await _mediator.Send(new ListarUtilizacoesProgramaQuery(id)));

        [HttpDelete("utilizacoes/{utilizacaoId:guid}")]
        [AbacAuthorize("SubsidiosFundos", "Excluir")]
        public async Task<ActionResult<CommandResult>> RemoverUtilizacao(Guid utilizacaoId)
        {
            var r = await _mediator.Send(new RemoverUtilizacaoSubsidioCommand(utilizacaoId));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
