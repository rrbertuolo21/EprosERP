using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Resolução de CST e cClassTrib (classificação tributária IBS/CBS) por CST ou por NCM, usada na
    /// emissão de NF-e (55) e NFC-e (65). Fiel ao legado <c>api/v1/classificacoes-tributarias</c>.
    /// Controller fino: apenas delega ao MediatR.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/classificacoes-tributarias")]
    public class ClassificacoesTributariasController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>Cria o controller com o mediador de comandos/consultas.</summary>
        /// <param name="mediator">Mediador (MediatR) para despacho das consultas.</param>
        public ClassificacoesTributariasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Resolve o CST IBS/CBS pelo código, filtrando classes tributárias pelo modelo.</summary>
        /// <param name="cst">Código do CST (ex.: "000").</param>
        /// <param name="modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>O CST com suas classes tributárias aplicáveis ao modelo.</returns>
        /// <response code="200">CST encontrado.</response>
        /// <response code="404">CST não encontrado ou modelo inválido.</response>
        [HttpGet("obter-por-cst/{cst}/{modelo:int}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorCst(string cst, int modelo, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterCstIbsCbsPorCstQuery(cst, modelo), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Resolve a lista de classificações tributárias (cClassTrib) aplicáveis a um NCM.</summary>
        /// <param name="ncm">Código NCM (8 dígitos).</param>
        /// <param name="modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>As classificações do NCM; na ausência, o CST genérico "000".</returns>
        /// <response code="200">Consulta realizada.</response>
        /// <response code="404">Modelo inválido.</response>
        [HttpGet("obter-por-ncm-lst/{ncm}/{modelo:int}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorNcmLst(string ncm, int modelo, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterClassificacoesPorNcmQuery(ncm, modelo), cancellationToken);
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        /// <summary>Classifica em lote uma lista de NCMs (CST + cClassTrib), para a emissão IBS/CBS.</summary>
        /// <param name="dto">Lista de NCMs e o modelo do documento (55/65).</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Uma entrada por NCM com o CST e o cClassTrib resolvidos.</returns>
        /// <response code="200">Classificação realizada.</response>
        /// <response code="422">Requisição inválida (lista vazia ou modelo inválido).</response>
        [HttpPost("obter-ncm-classificados")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ObterNcmsClassificados([FromBody] ObterNcmsClassificadosInput dto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ObterNcmsClassificadosQuery(dto.Ncms, dto.Modelo), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    /// <summary>Corpo da requisição de classificação em lote de NCMs.</summary>
    /// <param name="Ncms">Lista de códigos NCM a classificar.</param>
    /// <param name="Modelo">Modelo do documento: 55 (NF-e) ou 65 (NFC-e).</param>
    public record ObterNcmsClassificadosInput(string[] Ncms, int Modelo);
}
