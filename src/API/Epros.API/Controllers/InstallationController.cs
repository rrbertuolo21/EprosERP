using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class InstallationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public InstallationController(IMediator mediator, IConfiguration configuration, IHostEnvironment environment)
        {
            _mediator = mediator;
            _configuration = configuration;
            _environment = environment;
        }

        // GET api/v1/installation/state
        [HttpGet("state")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetState()
        {
            var result = await _mediator.Send(new ObterInstalacaoStateQuery());
            return Ok(result);
        }

        // GET api/v1/installation/check-requirements
        [HttpGet("check-requirements")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRequirements()
        {
            var result = await _mediator.Send(new VerificarRequisitosQuery());
            return Ok(result);
        }

        // POST api/v1/installation/execute
        [HttpPost("execute")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Execute([FromBody] ExecutarInstalacaoCommand command)
        {
            // SEGURANÇA (fechamento do "gato"): este endpoint é [AllowAnonymous] (bootstrap, antes de
            // existir qualquer usuário). Duas barreiras impedem reinstalar / recriar admin em aberto:
            //
            // 1. Guard "já instalado": se a instalação já foi concluída, ninguém pode reexecutar.
            //    Retorna 410 Gone (recurso encerrado permanentemente), não 400.
            var state = await _mediator.Send(new ObterInstalacaoStateQuery());
            if (state.IsCompleted)
            {
                return StatusCode(StatusCodes.Status410Gone,
                    "A instalação já foi concluída. Reexecução bloqueada.");
            }

            // 2. Segredo de instalação: o passo inicial exige um segredo out-of-band (env/secret),
            //    provado pelo header X-Install-Secret. Fail-closed: fora de desenvolvimento local, se
            //    o segredo não estiver configurado ou não bater, a instalação é recusada (403).
            if (!SegredoInstalacaoValido())
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    "Segredo de instalação ausente ou inválido. Defina 'Instalacao:Segredo' (env/secret) e envie-o no header 'X-Install-Secret'.");
            }

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Valida o segredo de instalação (comparação em tempo constante). Se nenhum segredo estiver
        /// configurado, só libera em desenvolvimento local puro (fail-closed em ambiente deployado).
        /// </summary>
        private bool SegredoInstalacaoValido()
        {
            var esperado = _configuration["Instalacao:Segredo"]
                ?? Environment.GetEnvironmentVariable("EPROS_INSTALL_SECRET");

            if (string.IsNullOrWhiteSpace(esperado))
            {
                // Sem segredo configurado: permitido apenas em desenvolvimento local puro.
                return Epros.Shared.Security.AmbienteImplantacao
                    .EhDesenvolvimentoLocal(_environment.EnvironmentName);
            }

            var fornecido = Request.Headers["X-Install-Secret"].ToString();
            if (string.IsNullOrEmpty(fornecido))
            {
                return false;
            }

            var esperadoBytes = Encoding.UTF8.GetBytes(esperado);
            var fornecidoBytes = Encoding.UTF8.GetBytes(fornecido);
            return CryptographicOperations.FixedTimeEquals(esperadoBytes, fornecidoBytes);
        }

        // POST api/v1/installation/upgrade
        // 1.11 fix #3 — além de SuperAdmin, exige faixa técnica (SuporteInfra) e guard system no handler.
        [HttpPost("upgrade")]
        [AbacAuthorize("SuperAdmin", "Configurar")]
        [AbacAuthorize(SuperAdminSeguranca.RecursoSuporteInfra, "Executar")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> Upgrade([FromBody] ExecutarAtualizacaoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
            {
                return UnprocessableEntity(result);
            }

            return Ok(result);
        }

        // GET api/v1/installation/upgrades-log
        [HttpGet("upgrades-log")]
        [AbacAuthorize("SuperAdmin", "Configurar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpgradesLog()
        {
            var result = await _mediator.Send(new ListarUpdateLogsQuery());
            return Ok(result);
        }
    }
}
