using System;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Epros.API.Controllers
{
    /// <summary>
    /// Comércio Eletrônico (VEN-ECM) — administração da loja: configuração, clientes, cupons, pedidos,
    /// pagamento, conversão em venda e rastreio. Controller fino: apenas MediatR. AutZ ABAC nega por padrão
    /// (submódulo novo, sem permissão semeada). As APIs públicas da vitrine (token da loja) ficam fora deste
    /// controller administrativo. FISCAL: emissão de NF-e do pedido convertido é do módulo Fiscal.
    /// Fonte: EF_7_VENDAS_COMERCIO_ELETRONICO_V1.
    /// </summary>
    [ApiController]
    [Route("api/v1/vendas/ecommerce")]
    public class ComercioEletronicoController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ComercioEletronicoController(IMediator mediator) => _mediator = mediator;

        [HttpPost("configuracao")]
        [AbacAuthorize("EcommerceConfiguracao", "Editar")]
        public async Task<IActionResult> SalvarConfiguracao([FromBody] SalvarEcoConfiguracaoLojaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("clientes")]
        [AbacAuthorize("EcommerceClientes", "Criar")]
        public async Task<IActionResult> CriarCliente([FromBody] CriarEcoClienteCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("cupons")]
        [AbacAuthorize("EcommerceCupons", "Criar")]
        public async Task<IActionResult> CriarCupom([FromBody] CriarEcoCupomCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("pedidos")]
        [AbacAuthorize("EcommercePedidos", "Criar")]
        public async Task<IActionResult> CriarPedido([FromBody] CriarEcoPedidoCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Sucesso ? Created(string.Empty, result) : UnprocessableEntity(result);
        }

        [HttpPost("pedidos/{id:guid}/pagamento")]
        [AbacAuthorize("EcommercePedidos", "Editar")]
        public async Task<IActionResult> IniciarPagamento(Guid id, [FromBody] IniciarPagamentoPedidoCommand command)
        {
            if (id != command.PedidoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pedidos/{id:guid}/pagamento/confirmar")]
        [AbacAuthorize("EcommercePedidos", "Editar")]
        public async Task<IActionResult> ConfirmarPagamento(Guid id, [FromBody] ConfirmarPagamentoPedidoCommand command)
        {
            if (id != command.PedidoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pedidos/{id:guid}/converter-venda")]
        [AbacAuthorize("EcommercePedidos", "Converter")]
        public async Task<IActionResult> ConverterEmVenda(Guid id)
        {
            var result = await _mediator.Send(new ConverterEcoPedidoEmVendaCommand(id));
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }

        [HttpPost("pedidos/{id:guid}/rastreio")]
        [AbacAuthorize("EcommercePedidos", "Editar")]
        public async Task<IActionResult> AtualizarRastreio(Guid id, [FromBody] AtualizarRastreioPedidoCommand command)
        {
            if (id != command.PedidoId) return BadRequest(CommandResult.Falha("Id da rota diferente do corpo."));
            var result = await _mediator.Send(command);
            return result.Sucesso ? Ok(result) : UnprocessableEntity(result);
        }
    }
}
