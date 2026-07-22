using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Documentos fiscais (NF-e/NFC-e). Controller fino: apenas MediatR, sem DbContext.
    /// </summary>
    [ApiController]
    [Route("api/v1/fiscal/documentos")]
    public class DocumentosFiscaisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentosFiscaisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] string? status,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 20)
        {
            var result = await _mediator.Send(new ListarDocumentosFiscaisQuery(status, pagina, tamanhoPagina));
            return Ok(result.Dados);
        }

        [HttpGet("{id}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterDocumentoFiscalPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result.Mensagem);
            }
            return Ok(result.Dados);
        }

        [HttpGet("chave/{chave}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorChave(string chave)
        {
            var result = await _mediator.Send(new ObterDocumentoFiscalPorChaveQuery(chave));
            if (!result.Sucesso)
            {
                return NotFound(result.Mensagem);
            }
            return Ok(result.Dados);
        }

        [HttpGet("{id}/xml")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterXml(Guid id)
        {
            var result = await _mediator.Send(new ObterXmlDocumentoFiscalQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result.Mensagem);
            }

            var xml = result.Dados as string;
            return Content(xml ?? string.Empty, "application/xml");
        }

        [HttpPost("emitir")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Emitir([FromBody] EmitirDocumentoFiscalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Emite um documento fiscal em CONTINGÊNCIA (tpEmis SVC-AN/SVC-RS/EPEC/Offline-NFCe), quando a
        /// SEFAZ está indisponível. Offline-NFCe fica pendente para reenvio posterior.
        /// </summary>
        [HttpPost("emitir-contingencia")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EmitirContingencia([FromBody] EmitirDocumentoFiscalContingenciaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Reenvia à SEFAZ os documentos emitidos em contingência offline pendentes de transmissão,
        /// tipicamente após a SEFAZ voltar a operar.
        /// </summary>
        [HttpPost("reprocessar-contingencia")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ReprocessarContingencia([FromBody] ReprocessarContingenciaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("cancelar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar([FromBody] CancelarDocumentoFiscalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Gera/retorna o PDF do DANFE (NF-e) ou cupom (NFC-e) do documento. Se o documento não estiver
        /// autorizado, o PDF sai marcado como "SEM VALOR FISCAL".
        /// </summary>
        [HttpGet("{id}/danfe")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterDanfe(Guid id)
        {
            var result = await _mediator.Send(new ObterDanfePdfQuery(id));
            if (!result.Sucesso || result.Dados is not DanfePdfDto pdf)
            {
                return NotFound(result.Mensagem);
            }

            return File(pdf.Conteudo, "application/pdf", pdf.NomeArquivo);
        }

        /// <summary>
        /// Importa uma NF-e/NFC-e JÁ AUTORIZADA a partir do seu XML (nfeProc), sem transmitir à SEFAZ.
        /// Uso: contingência (nota autorizada por outro emissor/offline) e reconciliação/migração.
        /// </summary>
        [HttpPost("salvar-nf-com-xml")]
        [AbacAuthorize("DocumentosFiscais", "Criar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SalvarNfComXml([FromBody] SalvarDocumentoFiscalComXmlCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Registra o cancelamento de um documento a partir de um XML de evento externo
        /// (procEventoNFe/retEnvEvento) homologado fora deste sistema (contingência/reconciliação).
        /// </summary>
        [HttpPost("registrar-cancelamento-xml")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarCancelamentoXml([FromBody] RegistrarCancelamentoPorXmlCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        /// <summary>Envia uma Carta de Correção Eletrônica (CC-e) para o documento autorizado.</summary>
        [HttpPost("carta-correcao")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CartaCorrecao([FromBody] CartaCorrecaoDocumentoFiscalCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }
    }
}
