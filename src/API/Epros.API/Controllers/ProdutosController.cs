using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Shared.Application.Models;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Ações de negócio de Produtos compatíveis com a rota do legado (api/v1/produtos): localização,
    /// consulta GTIN, imagem, duplicação, importação CSV e reajuste de preços em massa.
    /// Controller fino: cada action delega a um command/query MediatR no módulo dono (Estoque).
    /// O CRUD básico de produtos permanece em ProdutosEspecificosController/EstoqueController.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/produtos")]
    public class ProdutosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProdutosController(IMediator mediator) => _mediator = mediator;

        /// <summary>Localiza produtos por código/descrição/EAN, com filtro de ativo, ordenação e paginação.</summary>
        /// <response code="200">Lista paginada de produtos.</response>
        [HttpGet("localizar-produto")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Localizar(
            [FromQuery] string? localizar,
            [FromQuery] DateTime? dataAlteracao,
            [FromQuery] bool ativo = true,
            [FromQuery] string ordenarPor = "codigo",
            [FromQuery] bool ordemAscendente = true,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 50,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new LocalizarProdutoQuery(localizar, dataAlteracao, ativo, ordenarPor, ordemAscendente, pagina, tamanhoPagina),
                cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Consulta um produto pelo GTIN/EAN. Retorna o produto do cadastro local quando existir.</summary>
        /// <param name="valorGtin">Código GTIN/EAN a consultar.</param>
        /// <response code="200">Produto localizado.</response>
        /// <response code="400">GTIN não localizado ou inválido.</response>
        [HttpGet("gtin/{valorGtin}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConsultarGtin(string valorGtin, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConsultarProdutoPorGtinQuery(valorGtin), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Altera a imagem de um produto (base64/data URI ou caminho).</summary>
        /// <response code="200">Imagem atualizada.</response>
        /// <response code="400">Produto não localizado.</response>
        [HttpPut("alterar-imagem")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AlterarImagem([FromBody] AlterarImagemProdutoInput input, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AlterarImagemProdutoCommand(input.ProdutoId, input.Imagem), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Remove a imagem de um produto pelo Id.</summary>
        /// <param name="id">Id do produto.</param>
        /// <response code="200">Imagem removida.</response>
        /// <response code="400">Produto não localizado.</response>
        [HttpPut("deletar-imagem-por-id-produto/{id:guid}")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletarImagem(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeletarImagemProdutoCommand(id), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Duplica um produto existente, gerando um novo com Código/Descrição informados.</summary>
        /// <response code="201">Produto duplicado.</response>
        /// <response code="422">Falha de validação/dados.</response>
        [HttpPost("duplicar-produto")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DuplicarProduto([FromBody] DuplicarProdutoCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        /// <summary>Importa produtos em massa a partir de um arquivo CSV (multipart/form-data, campo "file").</summary>
        /// <param name="file">Arquivo CSV com cabeçalho (obrigatórios: codigo, descricao).</param>
        /// <response code="200">Importação processada.</response>
        /// <response code="400">Arquivo ausente/ inválido ou já existem produtos.</response>
        [HttpPost("importar-csv")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportarCsv(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(CommandResult.Falha("Arquivo CSV não enviado."));

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(CommandResult.Falha("Arquivo inválido. Envie um arquivo .csv."));

            string conteudo;
            using (var reader = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                conteudo = await reader.ReadToEndAsync(cancellationToken);
            }

            var result = await _mediator.Send(new ImportarProdutosCsvCommand(conteudo), cancellationToken);
            return result.Sucesso ? Ok(result) : BadRequest(result);
        }

        /// <summary>Aplica reajuste de preço (venda/compra) em massa a uma lista de produtos (ou todos os ativos).</summary>
        /// <response code="200">Reajuste aplicado.</response>
        /// <response code="422">Falha de validação/dados.</response>
        [HttpPut("reajustar-precos")]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CommandResult), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ReajustarPrecos([FromBody] ReajustarPrecosCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }

    /// <summary>Payload da alteração de imagem de produto (rota compatível com o legado).</summary>
    public record AlterarImagemProdutoInput(Guid ProdutoId, string Imagem);
}
