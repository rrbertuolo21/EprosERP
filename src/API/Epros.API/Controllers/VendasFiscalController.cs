using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Models;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Expõe as operações do agregado fiscal Venda (cabeçalho, itens, emitente/destinatário,
    /// totais, pagamentos, fatura, transporte e emissão/cancelamento de NF-e/NFC-e).
    /// Controller fino: apenas encaminha ao MediatR. Porte do legado api/v1/vendas-nfe.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/vendas-fiscal")]
    public class VendasFiscalController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendasFiscalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ---------- Consultas ----------

        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> Listar(
            [FromQuery] EVendaStatus? status,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ListarVendasFiscaisQuery(status, dataInicio, dataFim, pagina, tamanhoPagina), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterVendaFiscalCompletaQuery(id), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Relatório de vendas "Simplificado01" — porte fiel de
        /// GET api/v1/relatorios/vendas/simplificado01 do legado. Filtra vendas do tenant no
        /// período informado cuja NFC-e/NF-e vinculada tenha o status interno indicado, e
        /// devolve os dados paginados (Data, Número, Destinatário, Valor, Modelo, Status, Chave).
        /// A exportação para Excel (NPOI, feita pelo legado) fica a cargo do consumidor/frontend.
        /// </summary>
        [HttpGet("relatorios/simplificado01")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommandResult>> ObterRelatorioSimplificado01(
            [FromQuery] DateTime dataVendaInicial,
            [FromQuery] DateTime dataVendaFinal,
            [FromQuery] EDocumentoFiscalStatus status,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new ObterRelatorioVendasSimplificadoQuery(dataVendaInicial, dataVendaFinal, status, pagina, tamanhoPagina),
                cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        // ---------- Venda (cabeçalho fiscal) ----------

        [HttpPost]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarVendaFiscalCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarVendaFiscalCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Deletar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarVendaFiscalCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Itens ----------

        [HttpPost("{id:guid}/itens")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarItem(Guid id, [FromBody] AdicionarItemFiscalCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/itens/{itemId:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarItem(Guid id, Guid itemId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarItemFiscalCommand(id, itemId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Emitente / Destinatário ----------

        [HttpPut("{id:guid}/emitente")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirEmitente(Guid id, [FromBody] DefinirEmitenteCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/destinatario")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirDestinatario(Guid id, [FromBody] DefinirDestinatarioCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Totais / Configuração / Imposto ----------

        [HttpPut("{id:guid}/totais")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirTotal(Guid id, [FromBody] DefinirTotalCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/totais-ibs-cbs")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirTotalIbsCbs(Guid id, [FromBody] DefinirTotalIbsCbsCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/configuracao")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirConfiguracao(Guid id, [FromBody] DefinirConfiguracaoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPut("{id:guid}/imposto")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirImposto(Guid id, [FromBody] DefinirVendaImpostoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Pagamentos ----------

        [HttpPost("{id:guid}/pagamentos")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarPagamento(Guid id, [FromBody] AdicionarPagamentoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/pagamentos/{pagamentoId:guid}")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarPagamento(Guid id, Guid pagamentoId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarPagamentoCommand(id, pagamentoId), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Fatura ----------

        [HttpPut("{id:guid}/fatura")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirFatura(Guid id, [FromBody] DefinirFaturaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpDelete("{id:guid}/fatura")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DeletarFatura(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarFaturaCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/fatura/duplicatas")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IncluirDuplicata(Guid id, [FromBody] IncluirDuplicataCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // ---------- Transporte ----------

        [HttpPut("{id:guid}/transporte")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DefinirTransporte(Guid id, [FromBody] DefinirTransporteCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- NFC-e ----------

        [HttpPost("{id:guid}/nfce")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IncluirNfce(Guid id, [FromBody] IncluirNfceCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfce/transmitir")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> TransmitirNfce(Guid id, [FromBody] TransmitirNfceCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfce/cancelar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CancelarNfce(Guid id, [FromBody] CancelarNfceCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- NF-e ----------

        [HttpPost("{id:guid}/nfe")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> IncluirNfe(Guid id, [FromBody] IncluirNfeCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfe/transmitir")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> TransmitirNfe(Guid id, [FromBody] TransmitirNfeCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfe/cancelar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CancelarNfe(Guid id, [FromBody] CancelarNfeCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfe/carta-correcao")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarCartaCorrecao(Guid id, [FromBody] AdicionarCartaCorrecaoCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/nfe/referenciadas")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AdicionarNfeReferenciada(Guid id, [FromBody] AdicionarNfeReferenciadaCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // ---------- Vínculo documento fiscal (módulo Fiscal) ----------

        [HttpPost("{id:guid}/documento-fiscal")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> VincularDocumentoFiscal(Guid id, [FromBody] VincularDocumentoFiscalCommand command, CancellationToken cancellationToken)
        {
            if (id != command.VendaId) return BadRequest("O id da rota difere do id do corpo.");
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Ações fiscais (Onda 7 — porte de api/v1/vendas-nfe do legado) ----------

        /// <summary>
        /// Duplica a venda informada como novo rascunho (sem NFC-e/NF-e).
        /// Porte de VendaNfeController.duplicar-venda.
        /// </summary>
        [HttpPut("{id:guid}/duplicar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Duplicar(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DuplicarVendaCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Gera o PDF de PRÉ-VISUALIZAÇÃO do DANFE (SEM VALOR FISCAL) do documento fiscal vinculado à venda.
        /// Porte de VendaNfeController.gerar-danfe-sem-autorizacao/{vendaId}.
        /// </summary>
        [HttpPost("{id:guid}/gerar-danfe-sem-autorizacao")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GerarDanfeSemAutorizacao(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GerarDanfeSemAutorizacaoCommand(id), cancellationToken);
            if (!result.Sucesso) return BadRequest(result);

            var arquivo = (ArquivoPdfResult)result.Dados!;
            return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeArquivo);
        }
    }
}
