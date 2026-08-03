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

        // ---- GRC-SOD avançado: bloqueio preventivo (D-SOD-03), exceção (D-SOD-02), bypass (D-SOD-04) ----
        // Fiação da camada avançada antes inalcançável (T1/T2). O bloqueio SoD também roda automaticamente
        // no caminho de concessão RBAC (ISoDAvaliadorConcessao); este endpoint permite avaliação sob demanda.

        [HttpPost("sod/avaliar-concessao")]
        [AbacAuthorize("SegregacaoFuncoes", "Avaliar")]
        public async Task<ActionResult<CommandResult>> AvaliarConcessao([FromBody] AvaliarConcessaoSoDCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("sod/excecoes")]
        [AbacAuthorize("SegregacaoFuncoes", "SolicitarExcecao")]
        public async Task<ActionResult<CommandResult>> SolicitarExcecao([FromBody] SolicitarExcecaoSoDCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("sod/excecoes/{id:guid}/aprovar")]
        [AbacAuthorize("SegregacaoFuncoes", "AprovarExcecao")]
        public async Task<ActionResult<CommandResult>> AprovarExcecao(Guid id, [FromBody] AprovarExcecaoSoDCommand command)
        {
            if (id != command.ExcecaoId) return BadRequest(CommandResult.Falha("O ID da rota não corresponde ao corpo."));
            return Resultado(await _mediator.Send(command));
        }

        [HttpPost("sod/excecoes/{id:guid}/renovar")]
        [AbacAuthorize("SegregacaoFuncoes", "AprovarExcecao")]
        public async Task<ActionResult<CommandResult>> RenovarExcecao(Guid id, [FromBody] RenovarExcecaoSoDCommand command)
        {
            if (id != command.ExcecaoId) return BadRequest(CommandResult.Falha("O ID da rota não corresponde ao corpo."));
            return Resultado(await _mediator.Send(command));
        }

        [HttpPost("sod/excecoes/expirar-vencidas")]
        [AbacAuthorize("SegregacaoFuncoes", "AprovarExcecao")]
        public async Task<ActionResult<CommandResult>> ExpirarExcecoesVencidas()
            => Resultado(await _mediator.Send(new ExpirarExcecoesVencidasSoDCommand()));

        [HttpPost("sod/bypass")]
        [AbacAuthorize("SegregacaoFuncoes", "RegistrarBypass")]
        public async Task<ActionResult<CommandResult>> RegistrarBypass([FromBody] RegistrarBypassAdminSoDCommand command)
            => Resultado(await _mediator.Send(command));

        // ===================== GRC-DEN (Investigacoes e Denuncias) =====================
        // Recurso ABAC "InvestigacoesDenuncias" nao e semeado: sobe DESABILITADO (nega por padrao).

        [HttpGet("denuncias/categorias")]
        [AbacAuthorize("InvestigacoesDenuncias", "Ler")]
        public async Task<IActionResult> ListarCategoriasDenuncia()
            => Ok(await _mediator.Send(new ObterCategoriasDenunciaQuery()));

        [HttpPost("denuncias/categorias")]
        [AbacAuthorize("InvestigacoesDenuncias", "Configurar")]
        public async Task<ActionResult<CommandResult>> CriarCategoriaDenuncia([FromBody] CriarCategoriaDenunciaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpPost("denuncias/categorias/{categoriaId}/inativar")]
        [AbacAuthorize("InvestigacoesDenuncias", "Configurar")]
        public async Task<ActionResult<CommandResult>> InativarCategoriaDenuncia(Guid categoriaId)
            => Resultado(await _mediator.Send(new InativarCategoriaDenunciaCommand(categoriaId)));

        [HttpPost("denuncias/detalhada")]
        [AbacAuthorize("InvestigacoesDenuncias", "Criar")]
        public async Task<ActionResult<CommandResult>> RegistrarDenunciaDetalhada([FromBody] RegistrarDenunciaDetalhadaCommand command)
            => Resultado(await _mediator.Send(command));

        [HttpGet("denuncias/{denunciaId}/detalhe")]
        [AbacAuthorize("InvestigacoesDenuncias", "Ler")]
        public async Task<IActionResult> ObterDenunciaDetalhe(Guid denunciaId)
            => Ok(await _mediator.Send(new ObterDenunciaDetalheQuery(denunciaId)));

        [HttpPost("denuncias/{denunciaId}/triar")]
        [AbacAuthorize("InvestigacoesDenuncias", "Triar")]
        public async Task<ActionResult<CommandResult>> TriarDenuncia(Guid denunciaId, [FromBody] TriarDenunciaRequest request)
            => Resultado(await _mediator.Send(new TriarDenunciaCommand(denunciaId, request.CategoriaId, request.Prioridade)));

        public record TriarDenunciaRequest(Guid? CategoriaId, string? Prioridade);

        [HttpPost("denuncias/{denunciaId}/participantes")]
        [AbacAuthorize("InvestigacoesDenuncias", "Triar")]
        public async Task<ActionResult<CommandResult>> AdicionarParticipante(Guid denunciaId, [FromBody] AdicionarParticipanteRequest request)
            => Resultado(await _mediator.Send(new AdicionarParticipanteDenunciaCommand(denunciaId, request.PessoaId, request.Papel, request.NomeDeclarado, request.Sigiloso)));

        public record AdicionarParticipanteRequest(Guid? PessoaId, string Papel, string? NomeDeclarado, bool Sigiloso);

        [HttpGet("denuncias/{denunciaId}/participantes")]
        [AbacAuthorize("InvestigacoesDenuncias", "Ler")]
        public async Task<IActionResult> ListarParticipantes(Guid denunciaId)
            => Ok(await _mediator.Send(new ObterParticipantesDenunciaQuery(denunciaId)));

        [HttpPost("denuncias/{denunciaId}/investigacoes")]
        [AbacAuthorize("InvestigacoesDenuncias", "Atribuir")]
        public async Task<ActionResult<CommandResult>> AtribuirInvestigacao(Guid denunciaId, [FromBody] AtribuirInvestigacaoRequest request)
            => Resultado(await _mediator.Send(new AtribuirInvestigacaoCommand(denunciaId, request.InvestigadorId, request.PrazoSla)));

        public record AtribuirInvestigacaoRequest(Guid InvestigadorId, DateTime? PrazoSla);

        [HttpGet("denuncias/{denunciaId}/investigacoes")]
        [AbacAuthorize("InvestigacoesDenuncias", "Ler")]
        public async Task<IActionResult> ListarInvestigacoes(Guid denunciaId)
            => Ok(await _mediator.Send(new ObterInvestigacoesDenunciaQuery(denunciaId)));

        [HttpPost("denuncias/investigacoes/{investigacaoId}/concluir")]
        [AbacAuthorize("InvestigacoesDenuncias", "Concluir")]
        public async Task<ActionResult<CommandResult>> ConcluirInvestigacao(Guid investigacaoId, [FromBody] ConcluirInvestigacaoRequest request)
            => Resultado(await _mediator.Send(new ConcluirInvestigacaoCommand(investigacaoId, request.ConclusaoProposta, request.DataConclusao)));

        public record ConcluirInvestigacaoRequest(string ConclusaoProposta, DateTime DataConclusao);

        [HttpPost("denuncias/{denunciaId}/respostas")]
        [AbacAuthorize("InvestigacoesDenuncias", "Responder")]
        public async Task<ActionResult<CommandResult>> ResponderDenuncia(Guid denunciaId, [FromBody] ResponderDenunciaRequest request)
            => Resultado(await _mediator.Send(new ResponderDenunciaCommand(denunciaId, request.Mensagem, request.Interna)));

        public record ResponderDenunciaRequest(string Mensagem, bool Interna);

        [HttpGet("denuncias/{denunciaId}/respostas")]
        [AbacAuthorize("InvestigacoesDenuncias", "Ler")]
        public async Task<IActionResult> ListarRespostas(Guid denunciaId, [FromQuery] bool incluirInternas = false)
            => Ok(await _mediator.Send(new ObterRespostasDenunciaQuery(denunciaId, incluirInternas)));

        [HttpPost("denuncias/{denunciaId}/anexos")]
        [AbacAuthorize("InvestigacoesDenuncias", "Responder")]
        public async Task<ActionResult<CommandResult>> AnexarEvidencia(Guid denunciaId, [FromBody] AnexarEvidenciaRequest request)
            => Resultado(await _mediator.Send(new AnexarEvidenciaDenunciaCommand(denunciaId, request.RespostaId, request.ArquivoId, request.Sigiloso)));

        public record AnexarEvidenciaRequest(Guid? RespostaId, Guid ArquivoId, bool Sigiloso);

        [HttpPost("denuncias/{denunciaId}/concluir")]
        [AbacAuthorize("InvestigacoesDenuncias", "Concluir")]
        public async Task<ActionResult<CommandResult>> ConcluirDenuncia(Guid denunciaId, [FromBody] ConcluirDenunciaRequest request)
            => Resultado(await _mediator.Send(new ConcluirDenunciaCommand(denunciaId, request.ResolvedAt, request.ParecerFinal)));

        public record ConcluirDenunciaRequest(DateTime ResolvedAt, string? ParecerFinal);

        [HttpGet("denuncias/parametros")]
        [AbacAuthorize("InvestigacoesDenuncias", "Configurar")]
        public async Task<IActionResult> ListarParametrosDenuncia()
            => Ok(await _mediator.Send(new ObterParametrosDenunciaQuery()));

        [HttpPost("denuncias/parametros")]
        [AbacAuthorize("InvestigacoesDenuncias", "Configurar")]
        public async Task<ActionResult<CommandResult>> DefinirParametroDenuncia([FromBody] DefinirParametroDenunciaCommand command)
            => Resultado(await _mediator.Send(command));
    }
}
