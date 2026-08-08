using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Application.Models;
using Epros.API.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>Upload e Migração de Dados genéricos (PLT-UPL). Upload direto, em partes e importação CSV/XLSX.</summary>
    [ApiController]
    [Route("api/v1/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UploadController(IMediator mediator) => _mediator = mediator;

        private static async Task<byte[]> LerBytesAsync(IFormFile arquivo, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await arquivo.CopyToAsync(ms, ct);
            return ms.ToArray();
        }

        /// <summary>Upload direto de arquivo completo (multipart).</summary>
        [HttpPost("arquivos")]
        [RequestSizeLimit(200_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CommandResult>> RegistrarArquivo([FromForm] RegistrarArquivoForm form, CancellationToken ct)
        {
            var arquivo = form.Arquivo;
            if (arquivo == null || arquivo.Length == 0) return BadRequest(CommandResult.Falha("Arquivo vazio ou não informado."));
            var bytes = await LerBytesAsync(arquivo, ct);
            var ext = Path.GetExtension(arquivo.FileName);
            var command = new RegistrarArquivoUploadCommand(form.UsuarioId, null, EUplOrigemUpload.Direct, arquivo.FileName, ext, bytes, form.PastaDestino);
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Upload em partes. Informe o cabeçalho Content-Range: bytes {inicio}-{fim}/{total}.</summary>
        [HttpPost("partes")]
        [RequestSizeLimit(200_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CommandResult>> ReceberParte([FromForm] ReceberParteForm form, CancellationToken ct)
        {
            var arquivo = form.Arquivo;
            if (arquivo == null) return BadRequest(CommandResult.Falha("Parte não informada."));

            long byteInicio = 0, byteFim = arquivo.Length - 1, totalBytes = arquivo.Length;
            var contentRange = Request.Headers["Content-Range"].ToString();
            if (!string.IsNullOrWhiteSpace(contentRange) && contentRange.StartsWith("bytes "))
            {
                // Formato: bytes 0-999/5000
                var partes = contentRange.Substring(6).Split('/');
                if (partes.Length == 2 && long.TryParse(partes[1], out var t)) totalBytes = t;
                var faixa = partes[0].Split('-');
                if (faixa.Length == 2 && long.TryParse(faixa[0], out var ini) && long.TryParse(faixa[1], out var fim))
                {
                    byteInicio = ini; byteFim = fim;
                }
            }

            var bytes = await LerBytesAsync(arquivo, ct);
            var ext = Path.GetExtension(arquivo.FileName);
            var command = new ReceberParteUploadCommand(form.ExecucaoUploadId, form.UsuarioId, arquivo.FileName, ext, bytes, byteInicio, byteFim, totalBytes, form.PastaDestino);
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>Importação tabular CSV/XLSX (multipart).</summary>
        [HttpPost("importacoes")]
        [RequestSizeLimit(200_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CommandResult>> ImportarTabular([FromForm] ImportarTabularForm form, CancellationToken ct)
        {
            var arquivo = form.Arquivo;
            if (arquivo == null || arquivo.Length == 0) return BadRequest(CommandResult.Falha("Arquivo vazio ou não informado."));
            var bytes = await LerBytesAsync(arquivo, ct);
            var ext = Path.GetExtension(arquivo.FileName);
            var command = new ImportarArquivoTabularCommand(form.UsuarioId, form.TipoImportacao, form.ArquivoId, arquivo.FileName, ext, bytes, form.IgnorarLinhasInvalidas);
            var result = await _mediator.Send(command, ct);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [AbacAuthorize("Importacao", "Consultar")]
        [HttpGet("importacoes")]
        public async Task<IActionResult> ListarImportacoes([FromQuery] string? tipoImportacao, [FromQuery] EUplStatusImportacao? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarExecucoesImportacaoQuery(tipoImportacao, status, pagina, tamanhoPagina)));

        [AbacAuthorize("Importacao", "Consultar")]
        [HttpGet("importacoes/{id:guid}")]
        public async Task<IActionResult> ObterImportacao([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new ObterExecucaoImportacaoQuery(id));
            return result == null ? NotFound() : Ok(result);
        }

        [AbacAuthorize("Importacao", "Consultar")]
        [HttpGet("importacoes/erros/{referenciaErro}")]
        public async Task<IActionResult> ListarErros([FromRoute] string referenciaErro)
            => Ok(await _mediator.Send(new ListarErrosImportacaoQuery(referenciaErro)));

        [HttpPost("importacoes/{id:guid}/desfazer")]
        public async Task<ActionResult<CommandResult>> DesfazerImportacao([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new DesfazerImportacaoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("mapeamentos")]
        public async Task<ActionResult<CommandResult>> SalvarMapeamento([FromBody] SalvarMapeamentoImportacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpGet("arquivos/{id:guid}")]
        public async Task<IActionResult> ObterArquivo([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new ObterArquivoPorIdQuery(id));
            return result == null ? NotFound() : Ok(result);
        }
    }

    /// <summary>Formulário multipart do upload direto (PLT-UPL). DTO explícito para o binder e o OpenAPI.</summary>
    public class RegistrarArquivoForm
    {
        public IFormFile Arquivo { get; set; } = default!;
        public Guid UsuarioId { get; set; }
        public string? PastaDestino { get; set; }
    }

    /// <summary>Formulário multipart do upload em partes (PLT-UPL).</summary>
    public class ReceberParteForm
    {
        public IFormFile Arquivo { get; set; } = default!;
        public Guid UsuarioId { get; set; }
        public Guid? ExecucaoUploadId { get; set; }
        public string? PastaDestino { get; set; }
    }

    /// <summary>Formulário multipart da importação tabular CSV/XLSX (PLT-UPL).</summary>
    public class ImportarTabularForm
    {
        public IFormFile Arquivo { get; set; } = default!;
        public Guid UsuarioId { get; set; }
        public string TipoImportacao { get; set; } = default!;
        public Guid? ArquivoId { get; set; }
        public bool IgnorarLinhasInvalidas { get; set; }
    }
}
