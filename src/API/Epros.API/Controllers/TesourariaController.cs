using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// FIN-TS — Tesouraria e Gestão de Liquidez (contas financeiras, transações, transferências, movimentos, cheques, caixa).
    /// Controller fino: apenas MediatR. Submódulo de evolução — sobe desabilitado (ABAC nega por padrão;
    /// recurso "Tesouraria" não é semeado em nenhum perfil). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/tesouraria")]
    public class TesourariaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TesourariaController(IMediator mediator) => _mediator = mediator;

        // ----- Contas financeiras -----
        [HttpGet("contas")]
        [AbacAuthorize("Tesouraria", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarContas([FromQuery] bool? apenasAbertas, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarContasFinanceirasQuery(apenasAbertas, pagina, tamanhoPagina)));

        [HttpPost("contas")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarConta([FromBody] CriarContaFinanceiraCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("contas/{id:guid}")]
        [AbacAuthorize("Tesouraria", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarConta(Guid id, [FromBody] AtualizarContaFinanceiraCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("contas/{id:guid}/fechar")]
        [AbacAuthorize("Tesouraria", "Editar")]
        public async Task<ActionResult<CommandResult>> FecharConta(Guid id)
        {
            var r = await _mediator.Send(new FecharContaFinanceiraCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpGet("contas/{id:guid}/saldo")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> Saldo(Guid id)
            => Ok(await _mediator.Send(new ObterSaldoContaFinanceiraQuery(id)));

        [HttpGet("contas/{id:guid}/transacoes")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> ListarTransacoes(Guid id, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarTransacoesContaQuery(id, pagina, tamanhoPagina)));

        [HttpPost("contas/{id:guid}/transacoes")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarTransacao(Guid id, [FromBody] RegistrarTransacaoContaCommand command)
        {
            var r = await _mediator.Send(command with { ContaFinanceiraId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("transferencias")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> Transferir([FromBody] RegistrarTransferenciaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Fluxo de caixa projetado -----
        [HttpGet("fluxo-caixa/projecao")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> ProjetarFluxoCaixa(
            [FromQuery] DateTime? dataBase, [FromQuery] int numeroPeriodos = 12,
            [FromQuery] EGranularidadeFluxo granularidade = EGranularidadeFluxo.Mensal)
            => Ok(await _mediator.Send(new ProjetarFluxoCaixaQuery(dataBase, numeroPeriodos, granularidade)));

        // ----- Movimentos financeiros -----
        [HttpGet("movimentos")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> ListarMovimentos([FromQuery] bool? apenasNaoConciliados, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarMovimentosFinanceirosQuery(apenasNaoConciliados, pagina, tamanhoPagina)));

        [HttpPost("movimentos")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarMovimento([FromBody] RegistrarMovimentoFinanceiroCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("movimentos/{id:guid}/conciliar")]
        [AbacAuthorize("Tesouraria", "Editar")]
        public async Task<ActionResult<CommandResult>> Conciliar(Guid id)
        {
            var r = await _mediator.Send(new ConciliarMovimentoCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Cheques -----
        [HttpGet("cheques")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> ListarCheques([FromQuery] ESituacaoCheque? situacao, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarChequesQuery(situacao, pagina, tamanhoPagina)));

        [HttpPost("cheques")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarCheque([FromBody] RegistrarChequeCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("cheques/{id:guid}/situacao")]
        [AbacAuthorize("Tesouraria", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarSituacaoCheque(Guid id, [FromBody] AtualizarSituacaoChequeCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Caixa operacional -----
        [HttpGet("caixas")]
        [AbacAuthorize("Tesouraria", "Ler")]
        public async Task<IActionResult> ListarCaixas([FromQuery] EStatusCaixaOperacional? status)
            => Ok(await _mediator.Send(new ListarCaixasOperacionaisQuery(status)));

        [HttpPost("caixas/abrir")]
        [AbacAuthorize("Tesouraria", "Criar")]
        public async Task<ActionResult<CommandResult>> AbrirCaixa([FromBody] AbrirCaixaOperacionalCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("caixas/{id:guid}/fechar")]
        [AbacAuthorize("Tesouraria", "Editar")]
        public async Task<ActionResult<CommandResult>> FecharCaixa(Guid id, [FromBody] FecharCaixaOperacionalCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
