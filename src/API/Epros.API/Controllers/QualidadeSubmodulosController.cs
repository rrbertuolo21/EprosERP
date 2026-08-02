using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Commands.Ins;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Modules.Qualidade.Application.Queries.Ins;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    // ============================================================
    // QLD-NCR — Nao Conformidades
    // Controller fino: apenas MediatR. ABAC nega por padrao (submodulo novo,
    // sem permissao semeada) — nunca aberto.
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/ncr")]
    public class QualidadeNcrController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeNcrController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeNaoConformidades", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarNcrsQuery(status, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadeNaoConformidades", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("tratar")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Tratar([FromBody] TratarNaoConformidadeCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ============================================================
    // QLD-INS — Planos de Inspecao e Amostragem
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/planos-inspecao")]
    public class QualidadePlanosInspecaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadePlanosInspecaoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadePlanosInspecao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarPlanosInspecaoQuery(status, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadePlanosInspecao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarPlanoInspecaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // Simulador do motor AQL (ISO 2859-1 / NBR 5426): N + nivel + AQL -> n, Ac/Re, 100%.
        [HttpGet("amostragem/calcular")]
        [AbacAuthorize("QualidadePlanosInspecao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> CalcularAmostragem(
            [FromQuery] long tamanhoLote, [FromQuery] ENivelInspecao nivel, [FromQuery] decimal aql,
            [FromQuery] ESeveridadeAql severidade = ESeveridadeAql.Normal)
            => Ok(await _mediator.Send(new CalcularPlanoAmostragemQuery(tamanhoLote, nivel, aql, severidade)));

        [HttpPost("caracteristicas")]
        [AbacAuthorize("QualidadePlanosInspecao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarCaracteristica([FromBody] AdicionarCaracteristicaPlanoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("regras-amostragem")]
        [AbacAuthorize("QualidadePlanosInspecao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarRegra([FromBody] AdicionarRegraAmostragemCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("ativar")]
        [AbacAuthorize("QualidadePlanosInspecao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Ativar([FromBody] AtivarPlanoInspecaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("status")]
        [AbacAuthorize("QualidadePlanosInspecao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarStatus([FromBody] AlterarStatusPlanoInspecaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("executar")]
        [AbacAuthorize("QualidadePlanosInspecao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Executar([FromBody] ExecutarInspecaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ============================================================
    // QLD-ACR — Analise de Aceitacao e Rejeicao
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/aceitacao-rejeicao")]
    public class QualidadeAceitacaoRejeicaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeAceitacaoRejeicaoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarAnalisesAcrQuery(status, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarAnaliseAcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ============================================================
    // QLD-ADM — Administracao da Qualidade
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/administracao")]
    public class QualidadeAdministracaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeAdministracaoController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeAdministracao", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarRegistrosAdmQuery(status, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadeAdministracao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarRegistroAdmCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ============================================================
    // QLD-ATR — Gestao de Atributos
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/atributos")]
    public class QualidadeAtributosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeAtributosController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeAtributos", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarAtributosQuery(status, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadeAtributos", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarAtributoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
