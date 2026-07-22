using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Consulta pública de dados de CNPJ em serviço externo (ReceitaWS/BrasilAPI). Controller fino.
    /// Compatível com o legado <c>api/v1/cnpj-online/{cnpj}</c>. Se o serviço externo não estiver
    /// configurado/acessível, retorna falha honesta (não simula dados).
    /// </summary>
    [ApiController]
    [Route("api/v1/cnpj-online")]
    [Produces("application/json")]
    public class CnpjOnlineController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CnpjOnlineController(IMediator mediator) => _mediator = mediator;

        /// <summary>Consulta os dados cadastrais de um CNPJ.</summary>
        [HttpGet("{cnpj}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Consultar(string cnpj, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConsultarCnpjOnlineQuery(cnpj), cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
