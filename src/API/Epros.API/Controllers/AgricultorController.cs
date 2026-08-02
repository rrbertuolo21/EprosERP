using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.API.Security;
using Epros.Modules.Agricultor.Application.Commands;
using Epros.Modules.Agricultor.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Módulo AGRICULTOR (produtor rural PF): PAINEL DO PRODUTOR + LIVRO CAIXA DIGITAL (LCDPR).
    /// Controller fino: apenas MediatR. Protegido por ABAC (recurso "AgricultorGestao").
    /// Módulo novo: sobe desabilitado (ABAC nega por padrão até a permissão ser semeada — AGR-D06).
    /// </summary>
    [ApiController]
    [Route("api/v1/agricultor")]
    [Produces("application/json")]
    public class AgricultorController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AgricultorController(IMediator mediator) => _mediator = mediator;

        private async Task<IActionResult> Enviar(IRequest<CommandResult> req)
        {
            var result = await _mediator.Send(req);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        // ==================== Painel: propriedades ====================

        [HttpGet("propriedades")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarPropriedades() => Ok(await _mediator.Send(new ListarPropriedadesQuery()));

        [HttpGet("propriedades/{id:guid}")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ObterPropriedade(Guid id)
        {
            var result = await _mediator.Send(new ObterPropriedadeQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("propriedades")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarPropriedade([FromBody] CriarPropriedadeCommand cmd) => Enviar(cmd);

        [HttpPut("propriedades/{id:guid}")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> AlterarPropriedade(Guid id, [FromBody] AlterarPropriedadeCommand cmd)
            => Enviar(cmd with { PropriedadeId = id });

        [HttpDelete("propriedades/{id:guid}")]
        [AbacAuthorize("AgricultorGestao", "Excluir")]
        public Task<IActionResult> ExcluirPropriedade(Guid id) => Enviar(new ExcluirPropriedadeCommand(id));

        [HttpPost("propriedades/{id:guid}/talhoes")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> AdicionarTalhao(Guid id, [FromBody] AdicionarTalhaoCommand cmd)
            => Enviar(cmd with { PropriedadeId = id });

        // ==================== Painel: cadastros ====================

        [HttpGet("culturas")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarCulturas() => Ok(await _mediator.Send(new ListarCulturasQuery()));

        [HttpPost("culturas")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarCultura([FromBody] CriarCulturaCommand cmd) => Enviar(cmd);

        [HttpGet("categorias-despesa")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarCategorias() => Ok(await _mediator.Send(new ListarCategoriasDespesaQuery()));

        [HttpPost("categorias-despesa")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarCategoria([FromBody] CriarCategoriaDespesaCommand cmd) => Enviar(cmd);

        [HttpGet("fornecedores")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarFornecedores() => Ok(await _mediator.Send(new ListarFornecedoresQuery()));

        [HttpPost("fornecedores")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarFornecedor([FromBody] CriarFornecedorCommand cmd) => Enviar(cmd);

        [HttpPost("colaboradores")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarColaborador([FromBody] CriarColaboradorCommand cmd) => Enviar(cmd);

        [HttpPost("anotacoes")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarAnotacao([FromBody] CriarAnotacaoCommand cmd) => Enviar(cmd);

        // ==================== Painel: safras ====================

        [HttpGet("safras")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarSafras([FromQuery] Guid? talhaoId)
            => Ok(await _mediator.Send(new ListarSafrasQuery(talhaoId)));

        [HttpPost("safras")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> CriarSafra([FromBody] CriarSafraCommand cmd) => Enviar(cmd);

        [HttpPost("safras/{id:guid}/iniciar")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> IniciarSafra(Guid id) => Enviar(new IniciarSafraCommand(id));

        [HttpPost("safras/{id:guid}/colheita")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> RegistrarColheita(Guid id, [FromBody] RegistrarColheitaCommand cmd)
            => Enviar(cmd with { SafraId = id });

        [HttpPost("safras/{id:guid}/encerrar")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> EncerrarSafra(Guid id) => Enviar(new EncerrarSafraCommand(id));

        // ==================== Painel: despesas / receitas ====================

        [HttpGet("despesas")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarDespesas([FromQuery] Guid? propriedadeId, [FromQuery] Guid? safraId)
            => Ok(await _mediator.Send(new ListarDespesasQuery(propriedadeId, safraId)));

        [HttpPost("despesas")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> RegistrarDespesa([FromBody] RegistrarDespesaCommand cmd) => Enviar(cmd);

        [HttpPost("despesas/{id:guid}/classificar-lcdpr")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> ClassificarDespesa(Guid id, [FromBody] ClassificarDespesaLcdprCommand cmd)
            => Enviar(cmd with { DespesaId = id });

        [HttpGet("receitas")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarReceitas([FromQuery] Guid? propriedadeId, [FromQuery] Guid? safraId)
            => Ok(await _mediator.Send(new ListarReceitasQuery(propriedadeId, safraId)));

        [HttpPost("receitas")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> RegistrarReceita([FromBody] RegistrarReceitaCommand cmd) => Enviar(cmd);

        [HttpPost("receitas/{id:guid}/classificar-lcdpr")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> ClassificarReceita(Guid id, [FromBody] ClassificarReceitaLcdprCommand cmd)
            => Enviar(cmd with { ReceitaId = id });

        // ==================== Painel do agricultor (consolidado) ====================

        [HttpGet("painel")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> PainelConsolidado([FromQuery] Guid? propriedadeId, [FromQuery] int? ano)
            => Ok(await _mediator.Send(new PainelConsolidadoQuery(propriedadeId, ano)));

        // ==================== LCDPR: escrituração ====================

        [HttpGet("lcdpr/escrituracoes")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ListarEscrituracoes() => Ok(await _mediator.Send(new ListarEscrituracoesQuery()));

        [HttpGet("lcdpr/escrituracoes/{id:guid}")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> ObterEscrituracao(Guid id)
        {
            var result = await _mediator.Send(new ObterEscrituracaoQuery(id));
            return result.Sucesso ? Ok(result) : NotFound(result);
        }

        [HttpPost("lcdpr/escrituracoes")]
        [AbacAuthorize("AgricultorGestao", "Criar")]
        public Task<IActionResult> AbrirEscrituracao([FromBody] AbrirEscrituracaoCommand cmd) => Enviar(cmd);

        [HttpPut("lcdpr/escrituracoes/{id:guid}/dados-cadastrais")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> DefinirDadosCadastrais(Guid id, [FromBody] DefinirDadosCadastraisCommand cmd)
            => Enviar(cmd with { EscrituracaoId = id });

        [HttpPost("lcdpr/escrituracoes/{id:guid}/imoveis")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> AdicionarImovel(Guid id, [FromBody] AdicionarImovelLcdprCommand cmd)
            => Enviar(cmd with { EscrituracaoId = id });

        [HttpPost("lcdpr/escrituracoes/{id:guid}/contas")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> AdicionarConta(Guid id, [FromBody] AdicionarContaLcdprCommand cmd)
            => Enviar(cmd with { EscrituracaoId = id });

        [HttpPost("lcdpr/escrituracoes/{id:guid}/lancamentos")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> RegistrarLancamento(Guid id, [FromBody] RegistrarLancamentoCommand cmd)
            => Enviar(cmd with { EscrituracaoId = id });

        [HttpPost("lcdpr/escrituracoes/{id:guid}/fechar")]
        [AbacAuthorize("AgricultorGestao", "Formalizar")]
        public Task<IActionResult> FecharEscrituracao(Guid id, [FromBody] FecharEscrituracaoCommand cmd)
            => Enviar(cmd with { EscrituracaoId = id });

        [HttpPost("lcdpr/escrituracoes/{id:guid}/retificar")]
        [AbacAuthorize("AgricultorGestao", "Formalizar")]
        public Task<IActionResult> Retificar(Guid id) => Enviar(new ReabrirRetificadoraCommand(id));

        [HttpGet("lcdpr/escrituracoes/{id:guid}/validar")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> Validar(Guid id) => Ok(await _mediator.Send(new ValidarEscrituracaoQuery(id)));

        [HttpGet("lcdpr/escrituracoes/{id:guid}/preview")]
        [AbacAuthorize("AgricultorGestao", "Ler")]
        public async Task<IActionResult> Preview(Guid id)
        {
            var result = await _mediator.Send(new PreviewArquivoLcdprQuery(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("lcdpr/escrituracoes/{id:guid}/gerar-arquivo")]
        [AbacAuthorize("AgricultorGestao", "Exportar")]
        public Task<IActionResult> GerarArquivo(Guid id) => Enviar(new GerarArquivoLcdprCommand(id));

        [HttpPut("lcdpr/parametros-obrigatoriedade")]
        [AbacAuthorize("AgricultorGestao", "Alterar")]
        public Task<IActionResult> DefinirParamObrigatoriedade([FromBody] DefinirParamObrigatoriedadeCommand cmd) => Enviar(cmd);
    }
}
