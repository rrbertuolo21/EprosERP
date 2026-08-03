using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Imobiliaria.Application.Commands;
using Epros.Modules.Imobiliaria.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// IMO-001 (Gestao Imobiliaria): imoveis, contratos de servico e locacoes.
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "ImobiliariaGestao").
    /// Submodulo novo: sobe desabilitado (ABAC nega por padrao ate a permissao ser semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/imobiliaria")]
    [Produces("application/json")]
    public class ImobiliariaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ImobiliariaController(IMediator mediator) => _mediator = mediator;

        // ==================== Imovel ====================

        [HttpGet("imoveis")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarImoveis()
            => Ok(await _mediator.Send(new ListarImoveisQuery()));

        [HttpGet("imoveis/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterImovel(Guid id)
        {
            var result = await _mediator.Send(new ObterImovelQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("imoveis")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarImovel([FromBody] CriarImovelCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("imoveis/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirImovel(Guid id)
        {
            var result = await _mediator.Send(new ExcluirImovelCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("imoveis/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarImovel(Guid id, [FromBody] AlterarImovelCommand command)
        {
            var result = await _mediator.Send(command with { ImovelId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("imoveis/{id:guid}/disponibilizar")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DisponibilizarImovel(Guid id)
        {
            var result = await _mediator.Send(new DisponibilizarImovelCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("imoveis/{id:guid}/inativar")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> InativarImovel(Guid id)
        {
            var result = await _mediator.Send(new InativarImovelCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("imoveis/{id:guid}/reativar")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ReativarImovel(Guid id)
        {
            var result = await _mediator.Send(new ReativarImovelCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Contrato de servico ====================

        [HttpGet("contratos-servico")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarContratosServico()
            => Ok(await _mediator.Send(new ListarContratosServicoQuery()));

        [HttpPost("contratos-servico")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarContratoServico([FromBody] CriarContratoServicoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("contratos-servico/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirContratoServico(Guid id)
        {
            var result = await _mediator.Send(new ExcluirContratoServicoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("contratos-servico/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarContratoServico(Guid id, [FromBody] AlterarContratoServicoCommand command)
        {
            var result = await _mediator.Send(command with { ContratoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Locacao ====================

        [HttpGet("locacoes")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarLocacoes([FromQuery] DateTime? periodoDe, [FromQuery] DateTime? periodoAte)
            => Ok(await _mediator.Send(new ListarLocacoesQuery(periodoDe, periodoAte)));

        [HttpPost("locacoes")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarLocacao([FromBody] CriarLocacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("locacoes/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirLocacao(Guid id)
        {
            var result = await _mediator.Send(new ExcluirLocacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("locacoes/{id:guid}/resumo-aluguel")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterResumoAluguel(Guid id)
        {
            var result = await _mediator.Send(new ObterResumoAluguelQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPut("locacoes/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarLocacao(Guid id, [FromBody] AlterarLocacaoCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---- Ciclo de vida da locacao (ID1/PRD-01) ----

        [HttpPost("locacoes/{id:guid}/formalizar")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> FormalizarLocacao(Guid id)
        {
            var result = await _mediator.Send(new FormalizarLocacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("locacoes/{id:guid}/encerrar")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EncerrarLocacao(Guid id)
        {
            var result = await _mediator.Send(new EncerrarLocacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("locacoes/{id:guid}/cancelar")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CancelarLocacao(Guid id)
        {
            var result = await _mediator.Send(new CancelarLocacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("locacoes/{id:guid}/renovar")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RenovarLocacao(Guid id, [FromBody] RenovarLocacaoCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("locacoes/{id:guid}/reajustar")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ReajustarLocacao(Guid id, [FromBody] AplicarReajusteCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("locacoes/{id:guid}/rescindir")]
        [AbacAuthorize("ImobiliariaGestao", "Formalizar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RescindirLocacao(Guid id, [FromBody] RescindirLocacaoCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("locacoes/{id:guid}/reajustes")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarReajustes(Guid id)
            => Ok(await _mediator.Send(new ListarReajustesLocacaoQuery(id)));

        // ---- Ciclo financeiro do aluguel (ID8/NF-01) ----

        [HttpPost("locacoes/{id:guid}/cobrancas")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> GerarCobranca(Guid id, [FromBody] GerarCobrancaAluguelCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("locacoes/{id:guid}/cobrancas")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCobrancas(Guid id)
            => Ok(await _mediator.Send(new ListarCobrancasLocacaoQuery(id)));

        [HttpPost("cobrancas/{cobrancaId:guid}/baixa")]
        [AbacAuthorize("ImobiliariaGestao", "Baixar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RefletirBaixa(Guid cobrancaId, [FromBody] RefletirBaixaAluguelCommand command)
        {
            var result = await _mediator.Send(command with { CobrancaId = cobrancaId });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("cobrancas/{cobrancaId:guid}/estorno")]
        [AbacAuthorize("ImobiliariaGestao", "Baixar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EstornarBaixa(Guid cobrancaId)
        {
            var result = await _mediator.Send(new EstornarBaixaAluguelCommand(cobrancaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("cobrancas/{cobrancaId:guid}/recibo")]
        [AbacAuthorize("ImobiliariaGestao", "EmitirRecibo")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EmitirRecibo(Guid cobrancaId)
        {
            var result = await _mediator.Send(new EmitirReciboAluguelCommand(cobrancaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---- Garantias (ID6/NF-04) ----

        [HttpPost("locacoes/{id:guid}/garantias")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarGarantia(Guid id, [FromBody] AdicionarGarantiaCommand command)
        {
            var result = await _mediator.Send(command with { LocacaoId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpGet("locacoes/{id:guid}/garantias")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarGarantias(Guid id)
            => Ok(await _mediator.Send(new ListarGarantiasLocacaoQuery(id)));

        [HttpPost("garantias/{garantiaId:guid}/substituir")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SubstituirGarantia(Guid garantiaId, [FromBody] SubstituirGarantiaCommand command)
        {
            var result = await _mediator.Send(command with { GarantiaAnteriorId = garantiaId });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("garantias/{garantiaId:guid}/liberar")]
        [AbacAuthorize("ImobiliariaGestao", "Alterar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> LiberarGarantia(Guid garantiaId)
        {
            var result = await _mediator.Send(new LiberarGarantiaCommand(garantiaId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Propostas (ID2) ====================

        [HttpGet("propostas")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPropostas([FromQuery] Guid? imovelId)
            => Ok(await _mediator.Send(new ListarPropostasQuery(imovelId)));

        [HttpGet("propostas/{id:guid}")]
        [AbacAuthorize("ImobiliariaGestao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterProposta(Guid id)
        {
            var result = await _mediator.Send(new ObterPropostaQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("propostas")]
        [AbacAuthorize("ImobiliariaGestao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarProposta([FromBody] CriarPropostaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("propostas/{id:guid}/aprovar")]
        [AbacAuthorize("ImobiliariaGestao", "AprovarProposta")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AprovarProposta(Guid id)
        {
            var result = await _mediator.Send(new AprovarPropostaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("propostas/{id:guid}/rejeitar")]
        [AbacAuthorize("ImobiliariaGestao", "AprovarProposta")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RejeitarProposta(Guid id)
        {
            var result = await _mediator.Send(new RejeitarPropostaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("propostas/{id:guid}/converter")]
        [AbacAuthorize("ImobiliariaGestao", "AprovarProposta")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConverterProposta(Guid id, [FromBody] ConverterPropostaCommand command)
        {
            var result = await _mediator.Send(command with { PropostaId = id });
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
