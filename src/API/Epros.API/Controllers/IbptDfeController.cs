using System.Threading;
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
    /// IBPT / Lei da Transparência: cálculo do valor aproximado de tributos.
    /// Controller fino: apenas MediatR (usa ICalculoFiscalService no handler).
    /// Compatível com o legado <c>api/v1/ibpt-dfe</c>.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/ibpt-dfe")]
    public class IbptDfeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IbptDfeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Calcula o valor aproximado dos tributos (IBPT) para um NCM/UF/valor/origem.</summary>
        [HttpGet("calcular-valor-aproximado/{ncm}/{uf}/{valorBase}/{origem}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CalcularValorAproximado(
            string ncm,
            string uf,
            decimal valorBase,
            string origem,
            [FromQuery] decimal aliquotaFederalNacional = 0,
            [FromQuery] decimal aliquotaFederalImportado = 0,
            [FromQuery] decimal aliquotaEstadual = 0,
            [FromQuery] decimal aliquotaMunicipal = 0,
            [FromQuery] string? versaoIbpt = null)
        {
            var result = await _mediator.Send(new CalcularIbptValorAproximadoQuery(
                ncm,
                uf,
                valorBase,
                origem,
                aliquotaFederalNacional,
                aliquotaFederalImportado,
                aliquotaEstadual,
                aliquotaMunicipal,
                versaoIbpt));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retorna as alíquotas IBPT (federal nacional/importado, estadual, municipal) cadastradas na
        /// tabela nacional para um NCM em uma UF. Fiel ao legado <c>obter-aliquotas-por-ncm-uf</c>.
        /// </summary>
        [HttpGet("obter-aliquotas-por-ncm-uf/{ncm}/{uf}")]
        [AbacAuthorize("DocumentosFiscais", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterAliquotasPorNcmUf(string ncm, string uf, [FromQuery] int ex = 0)
        {
            var result = await _mediator.Send(new ObterAliquotasIbptPorNcmUfQuery(ncm, uf, ex));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Loader (carga em massa) da tabela nacional IBPT a partir do arquivo oficial por UF
        /// (<c>TabelaIBPTax&lt;UF&gt;*.csv</c>). Recarrega os registros das UFs contidas no arquivo. Fiel ao
        /// legado <c>POST api/v1/ibpts/atualizar</c>.
        /// </summary>
        /// <param name="arquivo">Arquivo CSV/TXT no layout oficial do IBPT (a UF é inferida do nome).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>A quantidade de registros importados.</returns>
        /// <response code="200">Tabela atualizada.</response>
        /// <response code="422">Arquivo ausente, vazio ou inválido.</response>
        [HttpPost("atualizar")]
        [AbacAuthorize("DocumentosFiscais", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CommandResult>> AtualizarTabela(IFormFile arquivo, CancellationToken cancellationToken)
        {
            if (arquivo == null || arquivo.Length == 0)
                return UnprocessableEntity(CommandResult.Falha("Nenhum arquivo foi enviado."));

            await using var stream = arquivo.OpenReadStream();
            var result = await _mediator.Send(new AtualizarTabelaIbptCommand(stream, arquivo.FileName), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
