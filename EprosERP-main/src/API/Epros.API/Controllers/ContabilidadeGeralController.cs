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
    /// FIN-CGL — Contabilidade Geral (contabilidade plena: contas contábeis, períodos, lançamentos, saldos).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "ContabilidadeGeral" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/contabilidade-geral")]
    public class ContabilidadeGeralController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContabilidadeGeralController(IMediator mediator) => _mediator = mediator;

        // ----- Contas contábeis -----
        [HttpGet("contas")]
        [AbacAuthorize("ContabilidadeGeral", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarContas([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarContasContabeisQuery(pagina, tamanhoPagina)));

        [HttpGet("contas/{id:guid}")]
        [AbacAuthorize("ContabilidadeGeral", "Ler")]
        public async Task<IActionResult> ObterConta(Guid id)
        {
            var r = await _mediator.Send(new ObterContaContabilPorIdQuery(id));
            return r.Sucesso ? Ok(r) : NotFound(r);
        }

        [HttpPost("contas")]
        [AbacAuthorize("ContabilidadeGeral", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarConta([FromBody] CriarContaContabilCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("contas/{id:guid}")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarConta(Guid id, [FromBody] AtualizarContaContabilCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpDelete("contas/{id:guid}")]
        [AbacAuthorize("ContabilidadeGeral", "Excluir")]
        public async Task<ActionResult<CommandResult>> DeletarConta(Guid id)
        {
            var r = await _mediator.Send(new DeletarContaContabilCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Períodos contábeis -----
        [HttpGet("periodos")]
        [AbacAuthorize("ContabilidadeGeral", "Ler")]
        public async Task<IActionResult> ListarPeriodos([FromQuery] int? anoFiscal)
            => Ok(await _mediator.Send(new ListarPeriodosContabeisQuery(anoFiscal)));

        [HttpPost("periodos")]
        [AbacAuthorize("ContabilidadeGeral", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarPeriodo([FromBody] CriarPeriodoContabilCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/iniciar-fechamento")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> IniciarFechamento(Guid id)
        {
            var r = await _mediator.Send(new IniciarFechamentoPeriodoCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/fechar")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> FecharPeriodo(Guid id, [FromBody] FecharPeriodoContabilCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("periodos/{id:guid}/reabrir")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> ReabrirPeriodo(Guid id, [FromBody] ReabrirPeriodoContabilCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Lançamentos contábeis -----
        [HttpGet("lancamentos")]
        [AbacAuthorize("ContabilidadeGeral", "Ler")]
        public async Task<IActionResult> ListarLancamentos([FromQuery] Guid? periodoContabilId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarLancamentosContabeisQuery(periodoContabilId, pagina, tamanhoPagina)));

        [HttpPost("lancamentos")]
        [AbacAuthorize("ContabilidadeGeral", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarLancamento([FromBody] CriarLancamentoContabilCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("lancamentos/{id:guid}/confirmar")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> ConfirmarLancamento(Guid id)
        {
            var r = await _mediator.Send(new ConfirmarLancamentoContabilCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("lancamentos/{id:guid}/estornar")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> EstornarLancamento(Guid id)
        {
            var r = await _mediator.Send(new EstornarLancamentoContabilCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("lancamentos/{id:guid}/cancelar")]
        [AbacAuthorize("ContabilidadeGeral", "Editar")]
        public async Task<ActionResult<CommandResult>> CancelarLancamento(Guid id)
        {
            var r = await _mediator.Send(new CancelarLancamentoContabilCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Saldo de abertura -----
        [HttpPost("saldos-abertura")]
        [AbacAuthorize("ContabilidadeGeral", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarSaldoAbertura([FromBody] RegistrarSaldoAberturaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
