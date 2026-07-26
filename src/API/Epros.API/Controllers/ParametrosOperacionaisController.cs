using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    [ApiController]
    [Route("api/v1/configuracoes")]
    [Produces("application/json")]
    public class ParametrosOperacionaisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParametrosOperacionaisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Empresa / Perfil
        [HttpGet("empresa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommandResult>> ObterEmpresa()
        {
            var result = await _mediator.Send(new ObterEmpresaConfiguracoesQuery());
            if (!result.Sucesso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPost("empresa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> SalvarEmpresa([FromBody] CriarEmpresaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("empresa")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarEmpresa([FromBody] AtualizarEmpresaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 2. Preferências
        [HttpGet("preferencias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ObterPreferencias()
        {
            var result = await _mediator.Send(new ObterPreferenciasQuery());
            return Ok(result);
        }

        [HttpPut("preferencias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarPreferencias([FromBody] AtualizarPreferenciasCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 3. E-mail SMTP
        [HttpGet("email")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ObterEmail()
        {
            var result = await _mediator.Send(new ObterConfiguracaoEmailQuery());
            return Ok(result);
        }

        [HttpPut("email")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> AtualizarEmail([FromBody] AtualizarConfiguracaoEmailCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 4. Categorias
        [HttpGet("categorias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarCategorias([FromQuery] string? nome)
        {
            var result = await _mediator.Send(new ListarCategoriasQuery(nome));
            return Ok(result);
        }

        [HttpPost("categorias")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarCategoria([FromBody] CriarCategoriaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("categorias/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarCategoria(Guid id, [FromBody] AtualizarCategoriaCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID da rota diverge do ID do comando.");

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("categorias/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirCategoria(Guid id)
        {
            var result = await _mediator.Send(new ExcluirCategoriaCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 5. Unidades de Medida
        [HttpGet("unidades")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarUnidades()
        {
            var result = await _mediator.Send(new ListarUnidadesMedidaQuery());
            return Ok(result);
        }

        [HttpPost("unidades")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarUnidade([FromBody] CriarUnidadeMedidaCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("unidades/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarUnidade(Guid id, [FromBody] AtualizarUnidadeMedidaCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID da rota diverge do ID do comando.");

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("unidades/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirUnidade(Guid id)
        {
            var result = await _mediator.Send(new ExcluirUnidadeMedidaCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 6. Armazéns
        [HttpGet("armazens")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarArmazens()
        {
            var result = await _mediator.Send(new ListarArmazensQuery());
            return Ok(result);
        }

        [HttpPost("armazens")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarArmazem([FromBody] CriarArmazemCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("armazens/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarArmazem(Guid id, [FromBody] AtualizarArmazemCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID da rota diverge do ID do comando.");

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("armazens/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirArmazem(Guid id)
        {
            var result = await _mediator.Send(new ExcluirArmazemCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 7. Projetos
        [HttpGet("projetos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarProjetos()
        {
            var result = await _mediator.Send(new ListarProjetosQuery());
            return Ok(result);
        }

        [HttpPost("projetos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarProjeto([FromBody] CriarProjetoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("projetos/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarProjeto(Guid id, [FromBody] AtualizarProjetoCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID da rota diverge do ID do comando.");

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("projetos/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirProjeto(Guid id)
        {
            var result = await _mediator.Send(new ExcluirProjetoCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 8. Impostos
        [HttpGet("impostos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarImpostos()
        {
            var result = await _mediator.Send(new ListarImpostosQuery());
            return Ok(result);
        }

        [HttpPost("impostos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarImposto([FromBody] CriarImpostoCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpPut("impostos/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarImposto(Guid id, [FromBody] AtualizarImpostoCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID da rota diverge do ID do comando.");

            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("impostos/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirImposto(Guid id)
        {
            var result = await _mediator.Send(new ExcluirImpostoCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 9. Conversão de Unidades
        [HttpGet("conversoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarConversoes()
        {
            var result = await _mediator.Send(new ListarConversoesUnidadesQuery());
            return Ok(result);
        }

        [HttpPost("conversoes")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> CriarConversao([FromBody] AdicionarConversaoUnidadeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        [HttpDelete("conversoes/{id}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<CommandResult>> ExcluirConversao(Guid id)
        {
            var result = await _mediator.Send(new ExcluirConversaoUnidadeCommand(id));
            if (!result.Sucesso)
                return UnprocessableEntity(result);
            return Ok(result);
        }

        // 10. Logs de Auditoria
        [HttpGet("logs-auditoria")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarLogsAuditoria()
        {
            var result = await _mediator.Send(new ListarLogsAuditoriaQuery());
            return Ok(result);
        }

        // 11. Globais: Fusos Horários e Moedas
        [HttpGet("fusos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarFusos()
        {
            var result = await _mediator.Send(new ListarFusosHorariosQuery());
            return Ok(result);
        }

        [HttpGet("moedas")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommandResult>> ListarMoedas()
        {
            var result = await _mediator.Send(new ListarMoedasQuery());
            return Ok(result);
        }
    }
}
