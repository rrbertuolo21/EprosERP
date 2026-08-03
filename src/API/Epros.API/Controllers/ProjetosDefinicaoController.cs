using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// PRJ-DEF (Definicao de Projeto) — enriquecimento do cadastro mestre: clientes, membros,
    /// atividades, arquivos e duplicacao. Controller fino (apenas MediatR).
    /// ABAC nega por padrao (submodulo novo sobe desabilitado; permissao nao semeada).
    /// </summary>
    [ApiController]
    [Route("api/v1/projetos/definicao")]
    [Produces("application/json")]
    public class ProjetosDefinicaoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProjetosDefinicaoController(IMediator mediator) => _mediator = mediator;

        [HttpPost("{id:guid}/clientes")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> VincularCliente(Guid id, [FromBody] VincularClienteRequest request)
        {
            var result = await _mediator.Send(new VincularClienteProjetoCommand(id, request.ClienteId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record VincularClienteRequest(Guid ClienteId);

        [HttpPost("{id:guid}/membros")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> VincularMembro(Guid id, [FromBody] VincularMembroRequest request)
        {
            var result = await _mediator.Send(new VincularMembroProjetoCommand(id, request.UsuarioId, request.Papel));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record VincularMembroRequest(Guid UsuarioId, string Papel);

        [HttpPost("{id:guid}/atividades")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> RegistrarAtividade(Guid id, [FromBody] RegistrarAtividadeRequest request)
        {
            var result = await _mediator.Send(new RegistrarAtividadeProjetoCommand(id, request.UsuarioId, request.TipoUsuario, request.TipoAtividade, request.Observacao));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record RegistrarAtividadeRequest(Guid? UsuarioId, string? TipoUsuario, string TipoAtividade, string? Observacao);

        [HttpPost("{id:guid}/arquivos")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AnexarArquivo(Guid id, [FromBody] AnexarArquivoRequest request)
        {
            var result = await _mediator.Send(new AnexarArquivoProjetoCommand(id, request.NomeArquivo, request.CaminhoArquivo, request.ArquivoId));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record AnexarArquivoRequest(string NomeArquivo, string? CaminhoArquivo, Guid? ArquivoId);

        [HttpPost("{id:guid}/duplicar")]
        [AbacAuthorize("ProjetosDefinicao", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Duplicar(Guid id)
        {
            var result = await _mediator.Send(new DuplicarProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ===== MM-d: workflow do mestre (status canônico T3) =====

        [HttpPost("{id:guid}/submeter")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Submeter(Guid id)
        {
            var result = await _mediator.Send(new SubmeterProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/aprovar")]
        [AbacAuthorize("ProjetosDefinicao", "Aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Aprovar(Guid id)
        {
            var result = await _mediator.Send(new AprovarProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/suspender")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Suspender(Guid id)
        {
            var result = await _mediator.Send(new SuspenderProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/retomar")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Retomar(Guid id)
        {
            var result = await _mediator.Send(new RetomarProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("{id:guid}/encerrar")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Encerrar(Guid id, [FromBody] EncerrarProjetoRequest request)
        {
            var result = await _mediator.Send(new EncerrarProjetoCommand(id, request.Motivo));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record EncerrarProjetoRequest(string Motivo);

        [HttpPost("{id:guid}/inativar")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> Inativar(Guid id)
        {
            var result = await _mediator.Send(new InativarProjetoCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        /// <summary>RN-DEF-012/013: converte o projeto em template ou de volta em projeto normal.</summary>
        [HttpPost("{id:guid}/template")]
        [AbacAuthorize("ProjetosDefinicao", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ConverterTemplate(Guid id, [FromBody] ConverterTemplateRequest request)
        {
            var result = await _mediator.Send(new ConverterProjetoTemplateCommand(id, request.ParaTemplate));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        public record ConverterTemplateRequest(bool ParaTemplate);
    }
}
