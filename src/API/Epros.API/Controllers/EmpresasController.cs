using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/cadastros/empresas")]
    public class EmpresasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmpresasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AbacAuthorize("Empresa", "Criar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Criar([FromBody] CriarEmpresaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Created(string.Empty, result);
        }

        [HttpPut]
        [AbacAuthorize("Empresa", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Atualizar([FromBody] AtualizarEmpresaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AbacAuthorize("Empresa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorId(Guid id)
        {
            var result = await _mediator.Send(new ObterEmpresaPorIdQuery(id));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [AbacAuthorize("Empresa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar()
        {
            var result = await _mediator.Send(new ListarEmpresasQuery());
            return Ok(result);
        }

        [HttpGet("obter-por-cnpj/{cnpj}")]
        [AbacAuthorize("Empresa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorCnpj(string cnpj)
        {
            var result = await _mediator.Send(new ObterEmpresaPorCnpjQuery(cnpj));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpGet("obter-por-cpf/{cpf}")]
        [AbacAuthorize("Empresa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterPorCpf(string cpf)
        {
            var result = await _mediator.Send(new ObterEmpresaPorCpfQuery(cpf));
            if (!result.Sucesso)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPut("alterar-logo")]
        [AbacAuthorize("Empresa", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AlterarLogo([FromBody] AlterarLogoDto dto)
        {
            var result = await _mediator.Send(new AlterarLogoEmpresaCommand(dto.EmpresaId, dto.Logo));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [AbacAuthorize("Empresa", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Excluir(Guid id)
        {
            var result = await _mediator.Send(new ExcluirEmpresaCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // ==========================================
        // CERTIFICADOS DIGITAIS & SMTP ENDPOINTS
        // ==========================================

        [HttpGet("{id}/certificados")]
        [AbacAuthorize("Empresa", "Ler")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarCertificados(Guid id)
        {
            var result = await _mediator.Send(new ListarCertificadosEmpresaQuery(id));
            return Ok(result);
        }

        public record UploadCertificadoDto(string ArquivoBase64, string Senha);

        [HttpPost("{id}/certificados")]
        [AbacAuthorize("Empresa", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> UploadCertificado(Guid id, [FromBody] UploadCertificadoDto dto)
        {
            var result = await _mediator.Send(new UploadCertificadoDigitalCommand(id, dto.ArquivoBase64, dto.Senha));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}/certificados/{certificadoId}")]
        [AbacAuthorize("Empresa", "Excluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirCertificado(Guid id, Guid certificadoId)
        {
            var result = await _mediator.Send(new ExcluirCertificadoDigitalCommand(id, certificadoId));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/testar-email")]
        [AbacAuthorize("Empresa", "Editar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> TestarEmail(Guid id)
        {
            var result = await _mediator.Send(new TestarEmailEmpresaCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }
    }

    public record AlterarLogoDto(Guid EmpresaId, string Logo);
}
