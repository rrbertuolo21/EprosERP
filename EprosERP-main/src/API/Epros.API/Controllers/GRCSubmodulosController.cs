using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Queries;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// GRC — submodulos Frente 1-2 (Politicas, Compliance Regulatorio, Riscos avancado,
    /// Controles/Auditoria, Segregacao de Funcoes). Controller fino: apenas MediatR.
    /// Toda rota exige ABAC (recurso, acao). Submodulos novos sobem DESABILITADOS:
    /// nenhuma permissao e semeada, portanto o ABAC nega por padrao (feature flag por permissao).
    /// </summary>
    [ApiController]
    [Route("api/v1/grc")]
    [Produces("application/json")]
    public class GRCSubmodulosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GRCSubmodulosController(IMediator mediator) => _mediator = mediator;

        private ActionResult<CommandResult> Resultado(CommandResult result)
            => result.Sucesso ? Ok(result) : UnprocessableEntity(result);

        // ===================== GRC-POL (Gestao de Politicas) =====================

        [HttpGet("politicas")]
        [AbacAuthorize("PoliticasGRC", "Ler")]
        public async Task<IActionResult> ListarPoliticas()
            => Ok(await _mediator.Send(new ObterPoliticasQuery()));

        [HttpPost("politicas")]
        [AbacAuthorize("PoliticasGRC", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarPolitica([FromBody] CriarPoliticaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("politicas/versoes")]
        [AbacAuthorize("PoliticasGRC", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarVersaoPolitica([FromBody] CriarVersaoPoliticaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("politicas/publicar")]
        [AbacAuthorize("PoliticasGRC", "Editar")]
        public async Task<ActionResult<CommandResult>> PublicarPolitica([FromBody] PublicarPoliticaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("politicas/aceites")]
        [AbacAuthorize("PoliticasGRC", "Aceitar")]
        public async Task<ActionResult<CommandResult>> RegistrarAceite([FromBody] RegistrarAceitePoliticaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("politicas/{politicaId}/aceites")]
        [AbacAuthorize("PoliticasGRC", "Ler")]
        public async Task<IActionResult> ListarAceites(Guid politicaId)
            => Ok(await _mediator.Send(new ObterAceitesPoliticaQuery(politicaId)));

        // ===================== GRC-REG (Compliance Regulatorio) =====================

        [HttpGet("compliance/registros")]
        [AbacAuthorize("ComplianceRegulatorio", "Ler")]
        public async Task<IActionResult> ListarRegistros()
            => Ok(await _mediator.Send(new ObterRegistrosRegulatoriosQuery()));

        [HttpPost("compliance/registros")]
        [AbacAuthorize("ComplianceRegulatorio", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarRegistro([FromBody] RegistrarRegistroRegulatorioCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("compliance/certificados")]
        [AbacAuthorize("ComplianceRegulatorio", "Ler")]
        public async Task<IActionResult> ListarCertificados()
            => Ok(await _mediator.Send(new ObterCertificadosDigitaisQuery()));

        [HttpPost("compliance/certificados")]
        [AbacAuthorize("ComplianceRegulatorio", "Criar")]
        public async Task<ActionResult<CommandResult>> CadastrarCertificado([FromBody] CadastrarCertificadoDigitalCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("compliance/certificados/validacoes")]
        [AbacAuthorize("ComplianceRegulatorio", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarValidacao([FromBody] RegistrarValidacaoCertificadoCommand command)
            => Resultado(await _mediator.Send(command));

        // ===================== GRC-RIS (Riscos Corporativos — avancado) =====================

        [HttpPost("riscos/avaliacoes")]
        [AbacAuthorize("RiscosCorporativos", "Editar")]
        public async Task<ActionResult<CommandResult>> AvaliarRisco([FromBody] AvaliarRiscoCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("riscos/{riscoId}/avaliacoes")]
        [AbacAuthorize("RiscosCorporativos", "Ler")]
        public async Task<IActionResult> ListarAvaliacoes(Guid riscoId)
            => Ok(await _mediator.Send(new ObterAvaliacoesRiscoQuery(riscoId)));

        [HttpPost("riscos/planos-acao")]
        [AbacAuthorize("RiscosCorporativos", "Editar")]
        public async Task<ActionResult<CommandResult>> CriarPlanoAcaoRisco([FromBody] CriarPlanoAcaoRiscoCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("riscos/controles-mitigadores")]
        [AbacAuthorize("RiscosCorporativos", "Editar")]
        public async Task<ActionResult<CommandResult>> VincularControleMitigador([FromBody] VincularControleMitigadorCommand command)
            => Resultado(await _mediator.Send(command));

        // ===================== GRC-CIA (Controles Internos / Auditoria) =====================

        [HttpGet("auditoria/planos")]
        [AbacAuthorize("ControlesAuditoria", "Ler")]
        public async Task<IActionResult> ListarPlanosAuditoria()
            => Ok(await _mediator.Send(new ObterPlanosAuditoriaQuery()));

        [HttpPost("auditoria/planos")]
        [AbacAuthorize("ControlesAuditoria", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarPlanoAuditoria([FromBody] CriarPlanoAuditoriaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("auditoria/testes")]
        [AbacAuthorize("ControlesAuditoria", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarTeste([FromBody] RegistrarTesteControleCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("auditoria/achados")]
        [AbacAuthorize("ControlesAuditoria", "Ler")]
        public async Task<IActionResult> ListarAchados()
            => Ok(await _mediator.Send(new ObterAchadosQuery()));

        [HttpPost("auditoria/achados")]
        [AbacAuthorize("ControlesAuditoria", "Editar")]
        public async Task<ActionResult<CommandResult>> RegistrarAchado([FromBody] RegistrarAchadoCommand command)
            => Resultado(await _mediator.Send(command));

        // ===================== GRC-SOD (Segregacao de Funcoes) =====================

        [HttpPost("sod/funcoes")]
        [AbacAuthorize("SegregacaoFuncoes", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarFuncao([FromBody] CriarFuncaoSoDCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("sod/regras")]
        [AbacAuthorize("SegregacaoFuncoes", "Ler")]
        public async Task<IActionResult> ListarRegras()
            => Ok(await _mediator.Send(new ObterRegrasSoDQuery()));

        [HttpPost("sod/regras")]
        [AbacAuthorize("SegregacaoFuncoes", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarRegra([FromBody] CriarRegraSoDCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("sod/simulacoes")]
        [AbacAuthorize("SegregacaoFuncoes", "Editar")]
        public async Task<ActionResult<CommandResult>> Simular([FromBody] SimularSoDCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("sod/violacoes")]
        [AbacAuthorize("SegregacaoFuncoes", "Ler")]
        public async Task<IActionResult> ListarViolacoes()
            => Ok(await _mediator.Send(new ObterViolacoesSoDQuery()));
    }
}
