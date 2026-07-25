using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.API.Security;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// FIN-SF — Serviços Financeiros (cobrança/boleto/remessa/portal do sacado/cobrança por e-mail).
    /// Controller fino: apenas MediatR. Submódulo novo — sobe desabilitado (ABAC nega por padrão;
    /// recurso "ServicosFinanceiros" não é semeado). Isolamento por tenant via ContextBase.
    /// </summary>
    [ApiController]
    [Route("api/v1/servicos-financeiros")]
    public class ServicosFinanceirosController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ServicosFinanceirosController(IMediator mediator) => _mediator = mediator;

        // ----- Configuração cedente -----
        [HttpPost("cedentes")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarCedente([FromBody] CriarConfiguracaoCedenteCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("cedentes/{id:guid}")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarCedente(Guid id, [FromBody] AtualizarConfiguracaoCedenteCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Contas emissoras -----
        [HttpGet("contas-emissoras")]
        [AbacAuthorize("ServicosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarContasEmissoras()
            => Ok(await _mediator.Send(new ListarContasEmissorasQuery()));

        [HttpPost("contas-emissoras")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarContaEmissora([FromBody] CriarContaEmissoraCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("contas-emissoras/{id:guid}")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarContaEmissora(Guid id, [FromBody] AtualizarContaEmissoraCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("contas-emissoras/{id:guid}/ativar")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtivarContaEmissora(Guid id)
        {
            var r = await _mediator.Send(new AtivarContaEmissoraCommand(id));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Grupos de recorrência -----
        [HttpGet("grupos-recorrencia")]
        [AbacAuthorize("ServicosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarGrupos()
            => Ok(await _mediator.Send(new ListarGruposRecorrenciaQuery()));

        [HttpPost("grupos-recorrencia")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarGrupo([FromBody] CriarGrupoRecorrenciaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("grupos-recorrencia/{id:guid}")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarGrupo(Guid id, [FromBody] AtualizarGrupoRecorrenciaCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Sacados -----
        [HttpGet("sacados")]
        [AbacAuthorize("ServicosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarSacados([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarSacadosQuery(pagina, tamanhoPagina)));

        [HttpPost("sacados")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarSacado([FromBody] CriarSacadoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPut("sacados/{id:guid}")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AtualizarSacado(Guid id, [FromBody] AtualizarSacadoCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("sacados/{id:guid}/bloqueio")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> BloquearSacado(Guid id, [FromQuery] bool bloquear = true)
        {
            var r = await _mediator.Send(new BloquearSacadoCommand(id, bloquear));
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Faturas de cobrança -----
        [HttpGet("faturas")]
        [AbacAuthorize("ServicosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarFaturas([FromQuery] Guid? sacadoId, [FromQuery] ESituacaoFaturaCobranca? situacao, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarFaturasCobrancaQuery(sacadoId, situacao, pagina, tamanhoPagina)));

        [HttpPost("faturas")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarFatura([FromBody] CriarFaturaCobrancaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("faturas/{id:guid}/baixar")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> BaixarFatura(Guid id, [FromBody] BaixarFaturaCobrancaCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Boletos -----
        [HttpPost("boletos")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> EmitirBoleto([FromBody] EmitirBoletoCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Remessa -----
        [HttpPost("remessas")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> GerarRemessa([FromBody] GerarRemessaCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("remessas/{id:guid}/boletos")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> AdicionarBoletoRemessa(Guid id, [FromBody] AdicionarBoletoRemessaCommand command)
        {
            var r = await _mediator.Send(command with { RemessaId = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        // ----- Cobrança por e-mail -----
        [HttpGet("cobrancas-email")]
        [AbacAuthorize("ServicosFinanceiros", "Ler")]
        public async Task<IActionResult> ListarCobrancasEmail([FromQuery] int ultimos = 10)
            => Ok(await _mediator.Send(new ListarCobrancasEmailQuery(ultimos)));

        [HttpPost("cobrancas-email")]
        [AbacAuthorize("ServicosFinanceiros", "Criar")]
        public async Task<ActionResult<CommandResult>> CriarCobrancaEmail([FromBody] CriarCobrancaEmailCommand command)
        {
            var r = await _mediator.Send(command);
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }

        [HttpPost("cobrancas-email/{id:guid}/transicionar")]
        [AbacAuthorize("ServicosFinanceiros", "Editar")]
        public async Task<ActionResult<CommandResult>> TransicionarCobrancaEmail(Guid id, [FromBody] TransicionarCobrancaEmailCommand command)
        {
            var r = await _mediator.Send(command with { Id = id });
            return r.Sucesso ? Ok(r) : UnprocessableEntity(r);
        }
    }
}
