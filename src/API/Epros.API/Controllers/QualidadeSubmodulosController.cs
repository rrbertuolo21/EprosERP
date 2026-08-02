using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Commands.Acr;
using Epros.Modules.Qualidade.Application.Commands.Ins;
using Epros.Modules.Qualidade.Application.Commands.Ncr;
using Epros.Modules.Qualidade.Application.Commands.Qps;
using Epros.Modules.Qualidade.Application.Commands.Rst;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Modules.Qualidade.Application.Queries.Ins;
using Epros.Modules.Qualidade.Application.Queries.Qps;
using Epros.Modules.Qualidade.Application.Queries.Rst;
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

        [HttpPost("causa-raiz")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarCausaRaiz([FromBody] AdicionarCausaRaizNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("capa")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarAcaoCapa([FromBody] AdicionarAcaoCapaNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("capa/concluir")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConcluirAcaoCapa([FromBody] ConcluirAcaoCapaNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("verificacao-eficacia")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarVerificacao([FromBody] RegistrarVerificacaoEficaciaNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("avancar-etapa")]
        [AbacAuthorize("QualidadeNaoConformidades", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AvancarEtapa([FromBody] AvancarEtapaNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("encerrar")]
        [AbacAuthorize("QualidadeNaoConformidades", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar([FromBody] EncerrarNcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("cancelar")]
        [AbacAuthorize("QualidadeNaoConformidades", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar([FromBody] CancelarNcrCommand command)
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

        [HttpPost("itens")]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarItem([FromBody] AdicionarItemAcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("submeter")]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Submeter([FromBody] SubmeterAcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // Decisao de disposicao (aceite/rejeicao/quarentena) — emite intencao ao Estoque/NCR via Outbox.
        [HttpPost("decidir")]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Decidir([FromBody] DecidirAnaliseAcrCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // Decisao automatica por amostragem AQL (motor decide aceitar/rejeitar).
        [HttpPost("avaliar-amostragem")]
        [AbacAuthorize("QualidadeAceitacaoRejeicao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AvaliarAmostragem([FromBody] AvaliarAcrPorAmostragemCommand command)
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

    // ============================================================
    // QLD-QPS — Qualidade de Fornecedor (Parceiro de Suprimento)
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/fornecedores")]
    public class QualidadeFornecedorController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeFornecedorController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeFornecedor", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? statusHomologacao, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarQpsRegistrosQuery(statusHomologacao, pagina, tamanhoPagina)));

        [HttpPost]
        [AbacAuthorize("QualidadeFornecedor", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarQpsRegistroCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("documentos")]
        [AbacAuthorize("QualidadeFornecedor", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarDocumento([FromBody] AdicionarDocumentoQpsCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("homologar")]
        [AbacAuthorize("QualidadeFornecedor", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Homologar([FromBody] HomologarFornecedorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("bloquear")]
        [AbacAuthorize("QualidadeFornecedor", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Bloquear([FromBody] BloquearFornecedorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("desbloquear")]
        [AbacAuthorize("QualidadeFornecedor", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Desbloquear([FromBody] DesbloquearFornecedorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // Scorecard: motor parametrizavel (formula/pesos = politica Siser, D14).
        [HttpPost("score")]
        [AbacAuthorize("QualidadeFornecedor", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CalcularScore([FromBody] CalcularScoreFornecedorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    // ============================================================
    // QLD-RST — Rastreabilidade e Recall
    // ============================================================
    [ApiController]
    [Route("api/v1/qualidade/recall")]
    public class QualidadeRecallController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QualidadeRecallController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AbacAuthorize("QualidadeRecall", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] string? etapa, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCampanhasRecallQuery(etapa, pagina, tamanhoPagina)));

        [HttpGet("{campanhaId:guid}/genealogia")]
        [AbacAuthorize("QualidadeRecall", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Genealogia([FromRoute] System.Guid campanhaId)
            => Ok(await _mediator.Send(new ObterGenealogiaRecallQuery(campanhaId)));

        [HttpPost]
        [AbacAuthorize("QualidadeRecall", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Criar([FromBody] CriarCampanhaRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("itens-afetados")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AdicionarItemAfetado([FromBody] AdicionarItemAfetadoRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("genealogia")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarGenealogia([FromBody] RegistrarGenealogiaNoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("bloquear")]
        [AbacAuthorize("QualidadeRecall", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Bloquear([FromBody] SolicitarBloqueioRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("comunicacao")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Comunicacao([FromBody] RegistrarComunicacaoRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("recolhimento")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Recolhimento([FromBody] RegistrarRecolhimentoRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("disposicao")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Disposicao([FromBody] RegistrarDisposicaoRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("avancar-etapa")]
        [AbacAuthorize("QualidadeRecall", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AvancarEtapa([FromBody] AvancarEtapaRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("encerrar")]
        [AbacAuthorize("QualidadeRecall", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Encerrar([FromBody] EncerrarRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("cancelar")]
        [AbacAuthorize("QualidadeRecall", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Cancelar([FromBody] CancelarRecallCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
