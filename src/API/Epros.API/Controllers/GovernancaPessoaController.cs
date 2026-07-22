using System;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>CAD-PEM: governança de pessoas (privacidade/LGPD, deduplicação, merge, identificadores, relacionamentos).</summary>
    [ApiController]
    [Route("api/v1/cadastros")]
    [Produces("application/json")]
    public class GovernancaPessoaController : ControllerBase
    {
        private readonly IMediator _mediator;
        public GovernancaPessoaController(IMediator mediator) { _mediator = mediator; }

        private IActionResult Resolver(CommandResult result) =>
            result.Sucesso ? Ok(result) : (result.Mensagem == "Erro de validação" ? UnprocessableEntity(result) : BadRequest(result));

        // ===== Privacidade / LGPD =====
        [HttpPost("pessoas/{id:guid}/consentimentos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> RegistrarConsentimento(Guid id, [FromBody] RegistrarConsentimentoTitularBody body)
        {
            var result = await _mediator.Send(new RegistrarConsentimentoTitularCommand(id, body.Finalidade, body.BaseLegal, body.DataConsentimento, body.Canal));
            return Resolver(result);
        }

        [HttpDelete("consentimentos/{id:guid}")]
        public async Task<IActionResult> RevogarConsentimento(Guid id)
            => Resolver(await _mediator.Send(new RevogarConsentimentoTitularCommand(id)));

        [HttpGet("pessoas/{id:guid}/consentimentos")]
        public async Task<IActionResult> ListarConsentimentos(Guid id)
        {
            var result = await _mediator.Send(new ListarConsentimentosPorPessoaQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("pessoas/{id:guid}/solicitacoes-titular")]
        public async Task<IActionResult> AbrirSolicitacao(Guid id, [FromBody] AbrirSolicitacaoTitularBody body)
            => Resolver(await _mediator.Send(new AbrirSolicitacaoTitularCommand(id, body.Tipo, body.Prazo)));

        [HttpGet("pessoas/{id:guid}/dados-pessoais")]
        public async Task<IActionResult> ExportarDadosPessoais(Guid id)
        {
            var result = await _mediator.Send(new ExportarDadosPessoaisQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("pessoas/{id:guid}/anonimizar")]
        public async Task<IActionResult> Anonimizar(Guid id)
        {
            var result = await _mediator.Send(new AnonimizarPessoaCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("pessoas/{id:guid}/inativar")]
        public async Task<IActionResult> Inativar(Guid id, [FromBody] InativarPessoaBody? body)
        {
            var result = await _mediator.Send(new InativarPessoaCommand(id, body?.Motivo, body?.Ip));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        // ===== Deduplicação / Merge =====
        [HttpGet("pessoas/duplicatas")]
        public async Task<IActionResult> ListarDuplicatas([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCandidatosDuplicataQuery(pagina, tamanhoPagina)));

        [HttpPost("pessoas/duplicatas/{id:guid}/descartar")]
        public async Task<IActionResult> DescartarDuplicata(Guid id)
        {
            var result = await _mediator.Send(new DescartarCandidatoDuplicataCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("pessoas/{id:guid}/mesclar")]
        public async Task<IActionResult> Mesclar(Guid id, [FromBody] MesclarPessoaBody body)
            => Resolver(await _mediator.Send(new MesclarPessoaCommand(id, body.PessoaMescladaId, body.CandidatoDuplicataId)));

        [HttpPost("regras-deduplicacao")]
        public async Task<IActionResult> CriarRegraDeduplicacao([FromBody] CriarRegraDeduplicacaoCommand command)
            => Resolver(await _mediator.Send(command));

        // ===== Identificadores fiscais / Relacionamentos =====
        [HttpPost("pessoas/{id:guid}/identificadores-fiscais")]
        public async Task<IActionResult> CriarIdentificadorFiscal(Guid id, [FromBody] IdentificadorFiscalBody body)
            => Resolver(await _mediator.Send(new CriarIdentificadorFiscalCommand(id, body.PaisId, body.Tipo, body.Valor, body.Validado)));

        [HttpPost("relacionamentos-parceiro")]
        public async Task<IActionResult> CriarRelacionamento([FromBody] CriarRelacionamentoParceiroCommand command)
            => Resolver(await _mediator.Send(command));

        // ===== Grupos de empresa =====
        [HttpPost("empresa-grupos")]
        public async Task<IActionResult> CriarEmpresaGrupo([FromBody] CriarEmpresaGrupoCommand command)
            => Resolver(await _mediator.Send(command));

        [HttpPut("empresa-grupos/{id:guid}")]
        public async Task<IActionResult> AtualizarEmpresaGrupo(Guid id, [FromBody] EmpresaGrupoBody body)
            => Resolver(await _mediator.Send(new AtualizarEmpresaGrupoCommand(id, body.Nome)));

        [HttpDelete("empresa-grupos/{id:guid}")]
        public async Task<IActionResult> DeletarEmpresaGrupo(Guid id)
        {
            var result = await _mediator.Send(new DeletarEmpresaGrupoCommand(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }
    }

    public record RegistrarConsentimentoTitularBody(
        Epros.Modules.GestaoClientes.Domain.Entities.EFinalidadeDados Finalidade,
        Epros.Modules.GestaoClientes.Domain.Entities.EBaseLegalPrivacidade BaseLegal,
        DateTime? DataConsentimento,
        string? Canal);

    public record AbrirSolicitacaoTitularBody(
        Epros.Modules.GestaoClientes.Domain.Entities.ETipoSolicitacaoTitular Tipo,
        DateTime Prazo);

    public record InativarPessoaBody(string? Motivo, string? Ip);

    public record MesclarPessoaBody(Guid PessoaMescladaId, Guid? CandidatoDuplicataId);

    public record IdentificadorFiscalBody(
        Guid PaisId,
        Epros.Modules.GestaoClientes.Domain.Entities.ETipoIdFiscal Tipo,
        string Valor,
        bool Validado);
}
