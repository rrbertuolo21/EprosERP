using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// CRM comercial (VEN-CRM): setup (pipelines/etapas/origens/etiquetas), leads, oportunidades,
    /// campanhas e atendimento por tickets. Controller fino: apenas MediatR. AutZ ABAC nega por padrão
    /// (submódulo novo, sem permissão semeada). Fonte: EF_7_VENDAS_CRM_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/crm")]
    public class CrmController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CrmController(IMediator mediator) => _mediator = mediator;

        // ---------- Setup ----------

        [HttpPost("pipelines")]
        [AbacAuthorize("CrmSetup", "Criar")]
        public async Task<IActionResult> CriarPipeline([FromBody] CriarCrmPipelineCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("etapas")]
        [AbacAuthorize("CrmSetup", "Criar")]
        public async Task<IActionResult> CriarEtapa([FromBody] CriarCrmEtapaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // Lista pipelines+etapas para alimentar o seletor no cadastro de lead. Autorização por CrmLeads
        // (quem cadastra lead precisa ver os pipelines disponíveis), não por CrmSetup.
        [HttpGet("pipelines")]
        [AbacAuthorize("CrmLeads", "Ler")]
        public async Task<IActionResult> ListarPipelines()
            => Ok(await _mediator.Send(new ListarCrmPipelinesQuery()));

        [HttpPost("etiquetas")]
        [AbacAuthorize("CrmSetup", "Criar")]
        public async Task<IActionResult> CriarEtiqueta([FromBody] CriarCrmEtiquetaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("origens")]
        [AbacAuthorize("CrmSetup", "Criar")]
        public async Task<IActionResult> CriarOrigem([FromBody] CriarCrmOrigemCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // ---------- Leads ----------

        [HttpGet("leads")]
        [AbacAuthorize("CrmLeads", "Ler")]
        public async Task<IActionResult> ListarLeads([FromQuery] Guid? etapaId, [FromQuery] ECrmLeadStatus? status, [FromQuery] Guid? responsavelUsuarioId, [FromQuery] bool incluirArquivados = false, [FromQuery] string? localizar = null, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCrmLeadsQuery(etapaId, status, responsavelUsuarioId, incluirArquivados, localizar, pagina, tamanhoPagina)));

        [HttpGet("leads/{id:guid}")]
        [AbacAuthorize("CrmLeads", "Ler")]
        public async Task<IActionResult> ObterLead(Guid id)
        {
            var result = await _mediator.Send(new ObterCrmLeadPorIdQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("leads")]
        [AbacAuthorize("CrmLeads", "Criar")]
        public async Task<IActionResult> CriarLead([FromBody] CriarCrmLeadCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("leads/{id:guid}/mover-etapa")]
        [AbacAuthorize("CrmLeads", "Editar")]
        public async Task<IActionResult> MoverLead(Guid id, [FromBody] MoverCrmLeadEtapaCommand command)
        {
            if (id != command.LeadId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("leads/{id:guid}/converter")]
        [AbacAuthorize("CrmLeads", "Converter")]
        public async Task<IActionResult> ConverterLead(Guid id, [FromBody] ConverterCrmLeadCommand command)
        {
            if (id != command.LeadId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("leads/{id:guid}/arquivar")]
        [AbacAuthorize("CrmLeads", "Editar")]
        public async Task<IActionResult> ArquivarLead(Guid id)
        {
            var result = await _mediator.Send(new ArquivarCrmLeadCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Oportunidades ----------

        [HttpGet("oportunidades")]
        [AbacAuthorize("CrmOportunidades", "Ler")]
        public async Task<IActionResult> ListarOportunidades([FromQuery] Guid? etapaId, [FromQuery] ECrmOportunidadeStatus? status, [FromQuery] Guid? clientePrincipalId, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCrmOportunidadesQuery(etapaId, status, clientePrincipalId, pagina, tamanhoPagina)));

        [HttpPost("oportunidades")]
        [AbacAuthorize("CrmOportunidades", "Criar")]
        public async Task<IActionResult> CriarOportunidade([FromBody] CriarCrmOportunidadeCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("oportunidades/{id:guid}/mover-etapa")]
        [AbacAuthorize("CrmOportunidades", "Editar")]
        public async Task<IActionResult> MoverOportunidade(Guid id, [FromBody] MoverCrmOportunidadeEtapaCommand command)
        {
            if (id != command.OportunidadeId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("oportunidades/{id:guid}/ganhar")]
        [AbacAuthorize("CrmOportunidades", "Editar")]
        public async Task<IActionResult> GanharOportunidade(Guid id)
        {
            var result = await _mediator.Send(new GanharCrmOportunidadeCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("oportunidades/{id:guid}/perder")]
        [AbacAuthorize("CrmOportunidades", "Editar")]
        public async Task<IActionResult> PerderOportunidade(Guid id, [FromBody] PerderCrmOportunidadeCommand command)
        {
            if (id != command.OportunidadeId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Campanhas ----------

        [HttpGet("campanhas")]
        [AbacAuthorize("CrmCampanhas", "Ler")]
        public async Task<IActionResult> ListarCampanhas([FromQuery] bool apenasAtivas = true, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCrmCampanhasQuery(apenasAtivas, pagina, tamanhoPagina)));

        [HttpPost("campanhas")]
        [AbacAuthorize("CrmCampanhas", "Criar")]
        public async Task<IActionResult> CriarCampanha([FromBody] CriarCrmCampanhaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // ---------- Atendimento (tickets) ----------

        [HttpGet("tickets")]
        [AbacAuthorize("CrmTickets", "Ler")]
        public async Task<IActionResult> ListarTickets([FromQuery] Guid? statusId, [FromQuery] Guid? clienteId, [FromQuery] bool incluirArquivados = false, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20)
            => Ok(await _mediator.Send(new ListarCrmTicketsQuery(statusId, clienteId, incluirArquivados, pagina, tamanhoPagina)));

        [HttpPost("tickets/status")]
        [AbacAuthorize("CrmTickets", "Criar")]
        public async Task<IActionResult> CriarTicketStatus([FromBody] CriarCrmTicketStatusCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tickets")]
        [AbacAuthorize("CrmTickets", "Criar")]
        public async Task<IActionResult> CriarTicket([FromBody] CriarCrmTicketCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("tickets/{id:guid}/responder")]
        [AbacAuthorize("CrmTickets", "Editar")]
        public async Task<IActionResult> ResponderTicket(Guid id, [FromBody] ResponderCrmTicketCommand command)
        {
            if (id != command.TicketId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Atividades / Agenda (CRM-029..037) ----------

        [HttpPost("atividades")]
        [AbacAuthorize("CrmAtividades", "Criar")]
        public async Task<IActionResult> CriarAtividade([FromBody] CriarCrmAtividadeCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("atividades/{id:guid}/concluir")]
        [AbacAuthorize("CrmAtividades", "Editar")]
        public async Task<IActionResult> ConcluirAtividade(Guid id, [FromBody] ConcluirCrmAtividadeCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Webforms (CRM-018..024) ----------

        [HttpPost("webforms")]
        [AbacAuthorize("CrmWebforms", "Criar")]
        public async Task<IActionResult> CriarWebform([FromBody] CriarCrmWebformCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("webforms/{identificador}/submissoes")]
        [AbacAuthorize("CrmWebforms", "Editar")]
        public async Task<IActionResult> SubmeterWebform(string identificador)
        {
            var result = await _mediator.Send(new RegistrarSubmissaoCrmWebformCommand(identificador));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ---------- Fidelização (CRM-066/067) ----------

        [HttpPost("fidelizacao/clientes")]
        [AbacAuthorize("CrmFidelizacao", "Criar")]
        public async Task<IActionResult> CriarClienteFidelizado([FromBody] CriarCrmClienteFidelizadoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("fidelizacao/pontuacoes")]
        [AbacAuthorize("CrmFidelizacao", "Criar")]
        public async Task<IActionResult> RegistrarPontuacao([FromBody] RegistrarPontuacaoFidelizacaoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        // ---------- Pix relacional (CRM-069..071) ----------

        [HttpPut("pix/configuracao")]
        [AbacAuthorize("CrmPix", "Editar")]
        public async Task<IActionResult> SalvarConfiguracaoPix([FromBody] SalvarCrmConfiguracaoPixRelacionalCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pix/pagamentos")]
        [AbacAuthorize("CrmPix", "Criar")]
        public async Task<IActionResult> CriarPagamentoPix([FromBody] CriarCrmPagamentoPixRelacionalCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("pix/pagamentos/{id:guid}/status")]
        [AbacAuthorize("CrmPix", "Editar")]
        public async Task<IActionResult> AtualizarStatusPagamentoPix(Guid id, [FromBody] AtualizarStatusCrmPagamentoPixCommand command)
        {
            if (id != command.Id) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
