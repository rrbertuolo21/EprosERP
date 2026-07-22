using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.GestaoClientes.Application.Commands; // Comandos legados/antigos
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/plataforma/[controller]")]
    [AbacAuthorize("SuperAdmin", "Configurar")]
    [Produces("application/json")]
    public class SuperAdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuperAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Endpoints Legados / Gestão de Clientes

        [HttpPost("clientes/{id}/aprovar-assinatura-manual")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AprovarAssinaturaManual(Guid id, [FromBody] AprovarAssinaturaManualCommand command)
        {
            if (id != command.ClienteId)
            {
                return BadRequest("O ID do cliente na URL não corresponde ao ID no corpo da requisição.");
            }

            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("execucao-massa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarExecucaoMassa([FromBody] CriarExecucaoMassaCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        [HttpPost("execucao-massa/{id}/aprovar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AprovarExecucaoMassa(Guid id)
        {
            var result = await _mediator.Send(new AprovarExecucaoMassaCommand(id));

            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        #endregion

        #region Endpoints do Módulo Aplicativo (OPERACAO_SUPER_ADMIN)

        // 1. Dashboard Global
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ObterDashboardGlobal()
        {
            var result = await _mediator.Send(new ObterDashboardGlobalQuery());
            return Ok(result);
        }

        [HttpGet("clientes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarClientes()
        {
            var result = await _mediator.Send(new ListarClientesQuery());
            return Ok(result);
        }

        // 2. Equipe Siser / Usuários Internos
        [HttpGet("usuarios-internos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarUsuariosInternos()
        {
            var result = await _mediator.Send(new ListarUsuariosInternosQuery());
            return Ok(result);
        }

        [HttpPost("usuarios-internos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarUsuarioInterno([FromBody] CriarUsuarioInternoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPut("usuarios-internos/{id}/alterar-senha")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AlterarSenhaUsuarioInterno(Guid id, [FromBody] AlterarSenhaUsuarioInternoCommand command)
        {
            if (id != command.UsuarioInternoId)
            {
                return BadRequest("O ID na URL não corresponde ao ID no corpo do comando.");
            }
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("usuarios-internos/{id}/tornar-admin")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> TornarAdminPrincipal(Guid id)
        {
            var result = await _mediator.Send(new TornarAdminPrincipalCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // 3. Configurações Globais (Aplicativo)
        [HttpGet("configuracoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarSystemSettings()
        {
            var result = await _mediator.Send(new ListarSystemSettingsQuery());
            return Ok(result);
        }

        [HttpPost("configuracoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirSystemSetting([FromBody] DefinirSystemSettingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // 4. Execução em Massa Global
        [HttpGet("execucoes-massa-global")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarExecucoesMassaGlobal()
        {
            var result = await _mediator.Send(new ListarExecucoesMassaGlobalQuery());
            return Ok(result);
        }

        [HttpPost("execucoes-massa-global")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarExecucaoMassaGlobal([FromBody] CriarExecucaoMassaGlobalCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("execucoes-massa-global/{id}/simular")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SimularExecucaoMassaGlobal(Guid id)
        {
            var result = await _mediator.Send(new SimularExecucaoMassaGlobalCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("execucoes-massa-global/{id}/ativar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtivarExecucaoMassaGlobal(Guid id)
        {
            var result = await _mediator.Send(new AtivarExecucaoMassaGlobalCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("execucoes-massa-global/{id}/concluir")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ConcluirExecucaoMassaGlobal(Guid id)
        {
            var result = await _mediator.Send(new ConcluirExecucaoMassaGlobalCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // 5. CMS / CustomPages (Administração)
        [HttpGet("paginas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarCustomPages()
        {
            var result = await _mediator.Send(new ListarCustomPagesQuery());
            return Ok(result);
        }

        [HttpPost("paginas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarCustomPage([FromBody] CriarCustomPageCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("paginas/{id}/publicar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> PublicarCustomPage(Guid id)
        {
            var result = await _mediator.Send(new PublicarCustomPageCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("paginas/{id}/rascunho")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> DefinirRascunhoCustomPage(Guid id)
        {
            var result = await _mediator.Send(new DefinirRascunhoCustomPageCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // 6. Newsletter (Administração)
        [HttpGet("newsletter")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarNewsletterSubscribers()
        {
            var result = await _mediator.Send(new ListarNewsletterSubscribersQuery());
            return Ok(result);
        }

        [HttpPost("newsletter/{id}/cancelar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CancelarNewsletter(Guid id)
        {
            var result = await _mediator.Send(new CancelarNewsletterCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("newsletter/{id}/reativar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ReativarNewsletter(Guid id)
        {
            var result = await _mediator.Send(new ReativarNewsletterCommand(id));
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        // 7. Configurações CMS / Landing
        [HttpPut("settings/landing")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarLandingPageSettings([FromBody] AtualizarLandingPageSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPut("settings/marketplace")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarMarketplaceSettings([FromBody] AtualizarMarketplaceSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        [HttpPost("comunicacao")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> EnviarComunicacaoSuperAdmin([FromBody] EnviarComunicacaoSuperAdminCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }
            return Ok(result);
        }

        #endregion
    }
}
