using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Documentos;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>Landlord: gestão de Faturas (GAP-4). Geração continua em plataforma/clientes/faturas.</summary>
    [ApiController]
    [Route("api/v1/plataforma/faturas")]
    [Produces("application/json")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    // 1.11 decisão #5 — área comercial: faixa de Suporte Negócio (SuporteTecnico é negado; PrimaryAdmin passa).
    [AbacAuthorize(SuperAdminSeguranca.RecursoSuporteComercial, "Configurar")]
    public class FaturasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FaturasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            [FromQuery] Guid? clienteId = null,
            [FromQuery] string? status = null)
        {
            var result = await _mediator.Send(new ListarFaturasQuery(pagina, tamanhoPagina, clienteId, status));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new ObterFaturaPorIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlterarFaturaCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Delete(Guid id)
        {
            var result = await _mediator.Send(new ExcluirFaturaCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPost("{id}/baixar-manual")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> BaixarManual(Guid id, [FromBody] BaixarFaturaManualCommand command)
        {
            if (id != command.FaturaId)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>1.08A — Recibo de pagamento da fatura quitada (documento simples; NFS-e diferida).</summary>
        [HttpGet("{id}/recibo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterRecibo(Guid id)
        {
            var recibo = await _mediator.Send(new ObterReciboPorFaturaQuery(id));
            if (recibo == null) return NotFound();
            return Ok(recibo);
        }

        /// <summary>1.08F — PDF da fatura (nº, tenant, itens, valores, vencimento, status). application/pdf.</summary>
        [HttpGet("{id}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterFaturaPdf(Guid id)
        {
            var doc = await _mediator.Send(new ObterFaturaPdfQuery(id));
            if (doc == null) return NotFound();
            return File(doc.Conteudo, doc.ContentType, doc.NomeArquivo);
        }

        /// <summary>1.08F — PDF do recibo de pagamento da fatura (nº, pagador, valor, data, meio). application/pdf.</summary>
        [HttpGet("{id}/recibo/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterReciboPdf(Guid id)
        {
            var doc = await _mediator.Send(new ObterReciboPdfQuery(id));
            if (doc == null) return NotFound();
            return File(doc.Conteudo, doc.ContentType, doc.NomeArquivo);
        }

        /// <summary>
        /// 1.08F — Boleto: o gateway (Mercado Pago) já hospeda o PDF do boleto; aqui apenas EXPOMOS/redirecionamos
        /// para essa URL (não geramos boleto do zero). 302 para a URL do gateway; 404 se não houver boleto emitido.
        /// </summary>
        [HttpGet("{id}/boleto/pdf")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterBoletoPdf(Guid id)
        {
            var link = await _mediator.Send(new ObterBoletoLinkQuery(id));
            if (link?.UrlBoleto == null) return NotFound();
            return Redirect(link.UrlBoleto);
        }

        /// <summary>Gera a cobrança PIX da fatura no gateway ativo (config por tenant, senão global).</summary>
        [HttpPost("{id}/gerar-cobranca-pix")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GerarCobrancaPix(Guid id)
        {
            var result = await _mediator.Send(new GerarCobrancaPixCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>1.08B — Emite o BOLETO da fatura no gateway ativo (concilia pelo webhook unificado).</summary>
        [HttpPost("{id}/gerar-boleto")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GerarBoleto(Guid id)
        {
            var result = await _mediator.Send(new GerarBoletoCommand(id));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>
        /// 1.08E — Estorna (refund) um pagamento liquidado da fatura no gateway. Operação landlord: além
        /// do AbacAuthorize desta controller, o handler exige operador interno (defesa em profundidade 1.11).
        /// </summary>
        [HttpPost("pagamentos/{pagamentoFaturaId}/estornar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Estornar(Guid pagamentoFaturaId, [FromBody] EstornarPagamentoCorpo? corpo)
        {
            var result = await _mediator.Send(new EstornarPagamentoFaturaCommand(pagamentoFaturaId, corpo?.Valor, corpo?.Motivo));
            if (!result.Sucesso) return UnprocessableEntity(result);
            return Ok(result);
        }

        /// <summary>Corpo opcional do estorno (valor parcial e motivo).</summary>
        public sealed class EstornarPagamentoCorpo
        {
            public decimal? Valor { get; set; }
            public string? Motivo { get; set; }
        }

        /// <summary>
        /// 1.08J — VISIBILIDADE (landlord/operador interno): lista as NFS-e da mensalidade SaaS por competência
        /// (default = Pendente). Apenas CONSULTA — NÃO emite NFS-e. A emissão REAL depende do overlay
        /// `negocio-siser` (alíquota/subitem/município — contador) + provedor municipal + certificado; enquanto
        /// isso os registros ficam Pendente. [LC 116/2003 item 1.05/1.03; STF ADI 1.945/5.659]
        /// </summary>
        [HttpGet("nfse-mensalidade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarNfseMensalidade(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 25,
            [FromQuery] string? status = "Pendente",
            [FromQuery] Guid? clienteId = null)
        {
            var result = await _mediator.Send(new ListarNfseMensalidadesQuery(pagina, tamanhoPagina, status, clienteId));
            return Ok(result);
        }
    }
}
