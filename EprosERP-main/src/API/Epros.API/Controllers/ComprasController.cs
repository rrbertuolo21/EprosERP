using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Recurso de Compras compatível com a rota do legado (api/v1/compras). O lançamento principal
    /// (POST api/v1/compras/lancar) persiste o agregado Compra fiscal completo (emitente/destinatário/
    /// itens/impostos/transporte/total/fatura/pagamentos/Outbox) via LancarCompraFiscalCommand. O fluxo
    /// simplificado (cabeçalho + itens/estoque) permanece disponível, deprecado, em api/v1/compras/lancar-simplificado.
    /// O EstoqueController mantém as rotas equivalentes sob api/v1/estoque/compras.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/compras")]
    public class ComprasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComprasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] string? localizar,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarComprasQuery(localizar, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCompraPorIdQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Lança uma compra a partir do payload fiscal completo (NF-e de entrada): cabeçalho, emitente,
        /// destinatário, itens (com entrada de estoque/movimentação), impostos, transporte, total e pagamentos.
        /// Persiste o agregado Compra completo em uma única transação (SaveChanges atômico) com evento no Outbox.
        /// </summary>
        /// <param name="command">Payload fiscal completo do lançamento da compra.</param>
        /// <returns>Identificador da compra criada.</returns>
        /// <response code="201">Compra lançada com sucesso.</response>
        /// <response code="422">Dados de domínio inválidos.</response>
        [HttpPost("lancar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Lancar([FromBody] LancarCompraFiscalCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// [DEPRECATED] Lançamento simplificado de compra (apenas cabeçalho + itens/estoque). Preferir
        /// POST api/v1/compras/lancar, que persiste o agregado fiscal completo. Mantido por compatibilidade.
        /// </summary>
        /// <param name="command">Payload simplificado do lançamento da compra.</param>
        /// <returns>Identificador da compra criada.</returns>
        /// <response code="201">Compra lançada com sucesso.</response>
        /// <response code="422">Dados de domínio inválidos.</response>
        [Obsolete("Use POST api/v1/compras/lancar (fluxo fiscal completo). Rota mantida por compatibilidade.")]
        [HttpPost("lancar-simplificado")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> LancarSimplificado([FromBody] LancarCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/cancelar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar(Guid id, [FromBody] CancelarCompraInput input, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelarCompraCommand(id, input.Motivo), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Importa uma NF-e de entrada a partir do upload do arquivo XML (multipart/form-data, campo "arquivo").
        /// O XML é parseado e persistido como agregado de compra completo.
        /// </summary>
        /// <param name="arquivo">Arquivo XML da NF-e de entrada.</param>
        /// <returns>Identificador da compra criada.</returns>
        [HttpPost("importar-xml")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ImportarXml(IFormFile? arquivo, CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { Sucesso = false, Mensagem = "Nenhum arquivo XML foi enviado." });

            string conteudoXml;
            using (var reader = new StreamReader(arquivo.OpenReadStream()))
            {
                conteudoXml = await reader.ReadToEndAsync(cancellationToken);
            }

            var result = await _mediator.Send(new ImportarCompraXmlCommand(conteudoXml), cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Lança uma NF-e de entrada própria (a empresa é a emitente, modelo 55). Persiste o agregado
        /// Compra completo e cria a CompraNfe própria marcada para emissão. Compatível com o legado
        /// POST api/v1/compras/entrada-propria.
        /// </summary>
        /// <response code="201">Entrada própria lançada.</response>
        /// <response code="422">Dados de domínio inválidos.</response>
        [HttpPost("entrada-propria")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EntradaPropria([FromBody] EntradaPropriaCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Lança uma compra a partir de uma NF-e já emitida pelo fornecedor (o fornecedor é o emitente).
        /// Persiste o agregado Compra completo e a CompraNfe com a chave/série/número do fornecedor.
        /// Compatível com o legado POST api/v1/compras/entrada-fornecedor.
        /// </summary>
        /// <response code="201">Entrada de fornecedor lançada.</response>
        /// <response code="422">Dados de domínio inválidos.</response>
        [HttpPost("entrada-fornecedor")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EntradaFornecedor([FromBody] EntradaFornecedorCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Registra uma Carta de Correção Eletrônica (CC-e) na NF-e da compra. A transmissão do evento
        /// à SEFAZ é feita pela camada de emissão fiscal. Compatível com o legado POST api/v1/compras/carta-correcao.
        /// </summary>
        /// <response code="200">Carta de correção registrada.</response>
        /// <response code="422">Compra/NF-e inválida para carta de correção.</response>
        [HttpPost("carta-correcao")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CartaCorrecao([FromBody] CartaCorrecaoCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Duplica uma compra existente (cabeçalho + itens) em uma nova compra, com novo número/chave.
        /// Compatível com o legado PUT api/v1/compras/duplicar-compras.
        /// </summary>
        /// <response code="200">Compra duplicada.</response>
        /// <response code="422">Dados de domínio inválidos.</response>
        [HttpPut("duplicar-compras")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DuplicarCompras([FromBody] DuplicarCompraCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>
        /// Gera uma pré-visualização (DANFE SEM VALOR FISCAL) da compra em PDF a partir dos dados
        /// persistidos. Compatível com o legado POST api/v1/compras/gerar-danfe-sem-autorizacao/{compraId}.
        /// </summary>
        /// <param name="compraId">Id da compra.</param>
        /// <response code="200">PDF de pré-visualização (application/pdf).</response>
        /// <response code="404">Compra não localizada.</response>
        [HttpPost("gerar-danfe-sem-autorizacao/{compraId:guid}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GerarDanfeSemAutorizacao(Guid compraId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GerarDanfeSemAutorizacaoCompraCommand(compraId), cancellationToken);
            if (!result.Sucesso || result.Dados is not DanfePreviewResult preview)
                return NotFound(result);

            return File(preview.Conteudo, "application/pdf", preview.NomeArquivo);
        }
    }
}
