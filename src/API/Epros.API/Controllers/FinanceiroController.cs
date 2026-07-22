using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Módulo Financeiro — Contas a Pagar (modelo FIEL: ContasAPagar + itens/baixas + FatoGerador).
    /// Base: /api/v1/financeiro/contas-pagar
    ///
    /// Segue arquitetura CQRS: todos os dados passam por MediatR Queries/Commands.
    /// TenantId é injetado pelo InquilinoSaaSMiddleware antes de chegar ao handler.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/financeiro/contas-pagar")]
    public class ContasPagarController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContasPagarController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lista as contas a pagar com filtros opcionais (paginado).</summary>
        [HttpGet]
        [AbacAuthorize("ContasPagar", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? status,
            [FromQuery] DateTime? vencimentoDe,
            [FromQuery] DateTime? vencimentoAte,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarContasPagarQuery(status, vencimentoDe, vencimentoAte, pagina, tamanhoPagina),
                cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Cards de resumo (vencendo hoje / vencido / a vencer) no período opcional.</summary>
        [HttpGet("totais")]
        [AbacAuthorize("ContasPagar", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Totais(
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ObterTotaisContasPagarQuery(de, ate), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Retorna detalhes completos de um título a pagar, incluindo itens/baixas.</summary>
        [HttpGet("{id:guid}")]
        [AbacAuthorize("ContasPagar", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterContaPagarPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Cria um novo título a pagar (modelo fiel).</summary>
        [HttpPost]
        [AbacAuthorize("ContasPagar", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarContasAPagarCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Atualiza um título a pagar existente.</summary>
        [HttpPut("{id:guid}")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContasAPagarCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("O Id da rota difere do Id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Inclui um item (parcela/baixa parcial) no título a pagar.</summary>
        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> IncluirItem(Guid id, [FromBody] IncluirContasAPagarItemRequest request, CancellationToken cancellationToken)
        {
            var command = new IncluirContasAPagarItemCommand(id, request.PlanoDeContasFinanceiroItemId,
                request.ContaBancariaId, request.TipoPagamento, request.ValorParcela, request.ValorPago,
                request.ValorDesconto, request.ValorMulta, request.ValorJuros, request.ValorAcrescimo, request.DataPagamento);
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Baixa (concilia) integralmente o título a pagar informando o valor pago.</summary>
        [HttpPost("{id:guid}/baixar")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Baixar(Guid id, [FromBody] BaixarContaPagarRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BaixarContasAPagarConciliadoCommand(id, request.ValorPago), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Estorna a baixa (conciliação) de um título a pagar.</summary>
        [HttpPost("{id:guid}/estornar")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Estornar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new EstornarContasAPagarConciliadoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Cancela um título a pagar com justificativa obrigatória.</summary>
        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, [FromBody] CancelarContaPagarRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelarContasAPagarCommand(id, request.Motivo), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Remove (soft delete) um título a pagar e seus itens.</summary>
        [HttpDelete("{id:guid}")]
        [AbacAuthorize("ContasPagar", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarContasAPagarCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }

    // DTOs de request — mantidos fora dos Commands para não expor campos internos (TenantId, UserId)
    public record BaixarContaPagarRequest(decimal ValorPago);
    public record CancelarContaPagarRequest(string Motivo);
    public record IncluirContasAPagarItemRequest(
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataPagamento);

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Módulo Financeiro — Contas a Receber (modelo FIEL: ContasAReceber + itens/baixas + FatoGerador).
    /// Base: /api/v1/financeiro/contas-receber
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/financeiro/contas-receber")]
    public class ContasReceberController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContasReceberController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lista as contas a receber com filtros opcionais (paginado).</summary>
        [HttpGet]
        [AbacAuthorize("ContasReceber", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? status,
            [FromQuery] DateTime? vencimentoDe,
            [FromQuery] DateTime? vencimentoAte,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ListarContasReceberQuery(status, vencimentoDe, vencimentoAte, pagina, tamanhoPagina),
                cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Cards de resumo (vencendo hoje / vencido / a vencer) no período opcional.</summary>
        [HttpGet("totais")]
        [AbacAuthorize("ContasReceber", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Totais(
            [FromQuery] DateTime? de,
            [FromQuery] DateTime? ate,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ObterTotaisContasReceberQuery(de, ate), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Retorna detalhes completos de um título a receber, incluindo itens/baixas.</summary>
        [HttpGet("{id:guid}")]
        [AbacAuthorize("ContasReceber", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterContaReceberPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Cria um novo título a receber (modelo fiel).</summary>
        [HttpPost]
        [AbacAuthorize("ContasReceber", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarContasAReceberCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Atualiza um título a receber existente.</summary>
        [HttpPut("{id:guid}")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContasAReceberCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("O Id da rota difere do Id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Inclui um item (parcela/baixa parcial) no título a receber.</summary>
        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> IncluirItem(Guid id, [FromBody] IncluirContasAReceberItemRequest request, CancellationToken cancellationToken)
        {
            var command = new IncluirContasAReceberItemCommand(id, request.PlanoDeContasFinanceiroItemId,
                request.ContaBancariaId, request.TipoPagamento, request.ValorParcela, request.ValorPago,
                request.ValorDesconto, request.ValorMulta, request.ValorJuros, request.ValorAcrescimo, request.DataRecebimento);
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Baixa (concilia) integralmente o título a receber informando o valor recebido.</summary>
        [HttpPost("{id:guid}/baixar")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Baixar(Guid id, [FromBody] BaixarContaReceberRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new BaixarContasAReceberConciliadoCommand(id, request.ValorRecebido), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Estorna a baixa (conciliação) de um título a receber.</summary>
        [HttpPost("{id:guid}/estornar")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Estornar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new EstornarContasAReceberConciliadoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Cancela um título a receber com justificativa obrigatória.</summary>
        [HttpPost("{id:guid}/cancelar")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, [FromBody] CancelarContaReceberRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelarContasAReceberCommand(id, request.Motivo), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Remove (soft delete) um título a receber e seus itens.</summary>
        [HttpDelete("{id:guid}")]
        [AbacAuthorize("ContasReceber", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarContasAReceberCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }

    public record BaixarContaReceberRequest(decimal ValorRecebido);
    public record CancelarContaReceberRequest(string Motivo);
    public record IncluirContasAReceberItemRequest(
        Guid PlanoDeContasFinanceiroItemId,
        Guid? ContaBancariaId,
        ETipoPagamento TipoPagamento,
        decimal ValorParcela,
        decimal ValorPago,
        decimal ValorDesconto,
        decimal ValorMulta,
        decimal ValorJuros,
        decimal ValorAcrescimo,
        DateTime DataRecebimento);

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Módulo Financeiro — Fluxo de Caixa Projetado
    /// Base: /api/v1/financeiro/fluxo-caixa
    ///
    /// Retorna o saldo projetado (CR - CP) para um período definido, com base no
    /// saldo em aberto dos títulos do modelo fiel.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/financeiro/fluxo-caixa")]
    public class FluxoCaixaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public FluxoCaixaController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Retorna o fluxo de caixa projetado: total a pagar, a receber e saldo líquido
        /// para o período informado.
        /// </summary>
        [HttpGet]
        [AbacAuthorize("FluxoCaixa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Projetado(
            [FromQuery] DateTime de,
            [FromQuery] DateTime ate,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ObterFluxoCaixaQuery(de, ate), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Módulo Financeiro — Importação e Conciliação de extratos OFX.
    /// Base: /api/v1/financeiro/ofx
    ///
    /// Porte fiel do legado Importacoes/ImportacacaoArquivoOfx: importação do cabeçalho,
    /// adição de transações, listagem, conciliação/estorno contra Contas a Pagar/Receber.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/financeiro/ofx")]
    public class ImportacaoOfxController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ImportacaoOfxController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lista as importações OFX (cabeçalhos de extrato) paginadas.</summary>
        [HttpGet]
        [AbacAuthorize("Ofx", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarImportacoesOfxQuery(pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Retorna uma importação OFX com suas transações e status de conciliação.</summary>
        [HttpGet("{id:guid}")]
        [AbacAuthorize("Ofx", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterImportacaoOfxComTransacoesQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Cria o cabeçalho de uma importação OFX (extrato bancário).</summary>
        [HttpPost]
        [AbacAuthorize("Ofx", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Importar([FromBody] CriarImportacaoArquivoOfxCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Adiciona uma transação a uma importação OFX existente.</summary>
        [HttpPost("{id:guid}/transacoes")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarTransacao(Guid id, [FromBody] AdicionarTransacaoOfxRequest request, CancellationToken cancellationToken)
        {
            var command = new AdicionarTransacaoOfxCommand(id, request.IdentificadorTransacao, request.Data,
                request.Valor, request.Tipo, request.Descricao);
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Remove (soft delete) uma importação OFX e suas transações.</summary>
        [HttpDelete("{id:guid}")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarImportacaoArquivoOfxCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Concilia uma transação OFX com um título a receber.</summary>
        [HttpPost("{id:guid}/transacoes/{transacaoId:guid}/conciliar-receber")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConciliarReceber(Guid id, Guid transacaoId, [FromBody] ConciliarOfxReceberRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConciliarTransacaoOfxContasAReceberCommand(id, transacaoId, request.ContasAReceberId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Concilia uma transação OFX com um título a pagar.</summary>
        [HttpPost("{id:guid}/transacoes/{transacaoId:guid}/conciliar-pagar")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConciliarPagar(Guid id, Guid transacaoId, [FromBody] ConciliarOfxPagarRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ConciliarTransacaoOfxContasAPagarCommand(id, transacaoId, request.ContasAPagarId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Estorna a conciliação de uma transação OFX com um título a receber.</summary>
        [HttpPost("transacoes/{transacaoId:guid}/estornar-receber")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EstornarReceber(Guid transacaoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new EstornarConciliacaoTransacaoOfxCReceberCommand(transacaoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Estorna a conciliação de uma transação OFX com um título a pagar.</summary>
        [HttpPost("transacoes/{transacaoId:guid}/estornar-pagar")]
        [AbacAuthorize("Ofx", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EstornarPagar(Guid transacaoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new EstornarConciliacaoTransacaoOfxCPagarCommand(transacaoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // DTOs de request OFX — mantidos fora dos Commands para não expor campos internos.
    public record AdicionarTransacaoOfxRequest(
        string IdentificadorTransacao,
        DateTime Data,
        decimal Valor,
        string Tipo,
        string? Descricao);
    public record ConciliarOfxReceberRequest(Guid ContasAReceberId);
    public record ConciliarOfxPagarRequest(Guid ContasAPagarId);
}
